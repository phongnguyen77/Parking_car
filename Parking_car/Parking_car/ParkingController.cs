using Parking_car;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using OpenCvSharp;
using Tesseract;

namespace Parking_car
{
    public class ParkingController
    {
        private static readonly string[] _samplePlates = new[] { "59H1568", "51H13903", "51H12902", "59H27723" };
        private static readonly Random _rand = new Random();
        private readonly SerialManager _serial;
        private readonly CameraManager _cams;
        private readonly DatabaseService _db;
        private readonly Action<string> _log;
        private readonly Action<int> _updateTotal;
        private readonly Action<int, int> _updateTotalsEntryExit;
        private readonly Action<string> _updateEntryPlate;
        private readonly Action<string> _updateExitPlate;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private bool _entrySessionActive = false;
        private bool _exitSessionActive = false;
        private DateTime _entryStart;
        private DateTime _exitStart;

        private string _lastEntryRfid = null;
        private string _lastExitRfid = null;

        private const int SESSION_TIMEOUT_SEC = 20;

        public ParkingController(
            SerialManager serial,
            CameraManager cams,
            DatabaseService db,
            Action<string> logger,
            Action<int> updateTotalCars,
            Action<int, int> updateTotalsEntryExit = null,
            Action<string> updateEntryPlate = null,
            Action<string> updateExitPlate = null
        )
        {
            _serial = serial;
            _cams = cams;
            _db = db;
            _log = logger;
            _updateTotal = updateTotalCars;
            _updateTotalsEntryExit = updateTotalsEntryExit;
            _updateEntryPlate = updateEntryPlate;
            _updateExitPlate = updateExitPlate;
        }

        public void Stop()
        {
            try { _cts.Cancel(); } catch { }
            try { _cams?.StopAll(); } catch { }
            try { _serial?.Close(); } catch { }
        }

        public void HandleEspLine(string line)
        {
            _log($"ESP32> {line}");

            if (_cts.IsCancellationRequested) return;

            if (line == "ENTRY_DETECTED")
            {
                _entrySessionActive = true;
                _entryStart = DateTime.Now;
                _lastEntryRfid = null;
                _ = HandleEntryDetectedAsync();
                return;
            }

            if (line == "EXIT_DETECTED")
            {
                _exitSessionActive = true;
                _exitStart = DateTime.Now;
                _lastExitRfid = null;
                _ = HandleExitDetectedAsync();
                return;
            }

            if (line.StartsWith("RFID_ENTRY:UID="))
            {
                _lastEntryRfid = line.Replace("RFID_ENTRY:UID=", "").Trim();
                _log($"RFID ENTRY UID = {_lastEntryRfid}");
                TryAuthorizeEntry();
                return;
            }

            if (line.StartsWith("RFID_EXIT:UID="))
            {
                _lastExitRfid = line.Replace("RFID_EXIT:UID=", "").Trim();
                _log($"RFID EXIT UID = {_lastExitRfid}");
                TryAuthorizeExit();
                return;
            }

            if (line == "ENTRY_PASSED" || line == "EXIT_PASSED")
            {
                UpdateTotalCarsFromDb();
                return;
            }

            if (line == "ENTRY_TIMEOUT")
            {
                _entrySessionActive = false;
                _log("ENTRY session timeout!");
                return;
            }

            if (line == "EXIT_TIMEOUT")
            {
                _exitSessionActive = false;
                _log("EXIT session timeout!");
                return;
            }
        }

        private async Task HandleEntryDetectedAsync()
        {
            if (_cts.IsCancellationRequested) return;

            Bitmap snap = null;
            try { snap = _cams?.SnapshotEntry(); } catch { }

            Bitmap snapForSave = null;
            if (snap != null)
                try { snapForSave = (Bitmap)snap.Clone(); } catch { snapForSave = null; }

            string imgPath = SaveSnapshot(snapForSave, "entry");
            _log($"Saved ENTRY snapshot: {imgPath}");

            string plate = "";
            try
            {
                if (snap != null)
                {
                    plate = await OcrPlateAsync(snap);
                    _log($"OCR ENTRY -> {plate}");
                    try { _updateEntryPlate?.Invoke(plate); } catch { }
                }
            }
            catch (Exception ex) { _log("OCR entry failed: " + ex.Message); }
            finally { try { snap?.Dispose(); } catch { } }

            await WaitForConditionAsync(() => _lastEntryRfid != null || !IsEntrySessionValid(), 50);

            if (_cts.IsCancellationRequested) { _entrySessionActive = false; return; }
            if (!IsEntrySessionValid()) { _entrySessionActive = false; _log("ENTRY session expired."); return; }

            string timeIn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try { _db.InsertEntry(_lastEntryRfid, plate, "ENTRY", timeIn, imgPath); } catch { }
            try { _serial.SendLine("OPEN_ENTRY"); _log("PC> OPEN_ENTRY"); } catch { }

            _entrySessionActive = false;
            UpdateTotalCarsFromDb();
        }

        private async Task HandleExitDetectedAsync()
        {
            if (_cts.IsCancellationRequested) return;

            Bitmap snap = null;
            try { snap = _cams?.SnapshotExit(); } catch { }

            Bitmap snapForSave = null;
            if (snap != null)
                try { snapForSave = (Bitmap)snap.Clone(); } catch { snapForSave = null; }

            string imgPath = SaveSnapshot(snapForSave, "exit");
            _log($"Saved EXIT snapshot: {imgPath}");

            string plate = "";
            try
            {
                if (snap != null)
                {
                    plate = await OcrPlateAsync(snap);
                    _log($"OCR EXIT -> {plate}");
                    try { _updateExitPlate?.Invoke(plate); } catch { }
                }
            }
            catch (Exception ex) { _log("OCR exit failed: " + ex.Message); }
            finally { try { snap?.Dispose(); } catch { } }

            await WaitForConditionAsync(() => _lastExitRfid != null || !IsExitSessionValid(), 50);

            if (_cts.IsCancellationRequested) { _exitSessionActive = false; return; }
            if (!IsExitSessionValid()) { _exitSessionActive = false; _log("EXIT session expired."); return; }

            string timeOut = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool ok = false;
            try { ok = _db.UpdateExitByRfid(_lastExitRfid, timeOut, imgPath); } catch { }

            if (!ok)
            {
                _log("Không tìm thấy xe trong bãi với RFID này. Không mở barie!");
                _exitSessionActive = false;
                return;
            }

            try { _serial.SendLine("OPEN_EXIT"); _log("PC> OPEN_EXIT"); } catch { }

            _exitSessionActive = false;
            UpdateTotalCarsFromDb();
        }

        private void TryAuthorizeEntry()
        {
            try
            {
                string plate = _samplePlates[_rand.Next(_samplePlates.Length)];
                _log($"Simulated plate for ENTRY on RFID {_lastEntryRfid}: {plate}");
                try { _updateEntryPlate?.Invoke(plate); } catch { }
            }
            catch (Exception ex) { try { _log("TryAuthorizeEntry error: " + ex.Message); } catch { } }
        }

        private void TryAuthorizeExit()
        {
            try
            {
                string plate = _samplePlates[_rand.Next(_samplePlates.Length)];
                _log($"Simulated plate for EXIT on RFID {_lastExitRfid}: {plate}");
                try { _updateExitPlate?.Invoke(plate); } catch { }
            }
            catch (Exception ex) { try { _log("TryAuthorizeExit error: " + ex.Message); } catch { } }
        }

        private bool IsEntrySessionValid() =>
            _entrySessionActive && (DateTime.Now - _entryStart).TotalSeconds <= SESSION_TIMEOUT_SEC;

        private bool IsExitSessionValid() =>
            _exitSessionActive && (DateTime.Now - _exitStart).TotalSeconds <= SESSION_TIMEOUT_SEC;

        private async Task WaitForConditionAsync(Func<bool> condition, int delayMs)
        {
            while (!condition())
            {
                if (_cts.IsCancellationRequested) break;
                await Task.Delay(delayMs);
            }
        }

        private string SaveSnapshot(Bitmap bmp, string prefix)
        {
            try
            {
                if (bmp == null) return "";
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "snapshots");
                Directory.CreateDirectory(dir);
                string file = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.jpg";
                string path = Path.Combine(dir, file);
                bmp.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                bmp.Dispose();
                return path;
            }
            catch { return ""; }
        }

        private void UpdateTotalCarsFromDb()
        {
            int total = 0, totalEntries = 0, totalExits = 0;
            try { total = _db.CountCarsInside(); } catch { }
            try { totalEntries = _db.CountTotalEntries(); } catch { }
            try { totalExits = _db.CountTotalExits(); } catch { }
            try { _updateTotal(total); } catch { }
            try { _updateTotalsEntryExit?.Invoke(totalEntries, totalExits); } catch { }
            _log($"Tổng xe trong bãi: {total}");
        }

        // ══════════════════════════════════════════════════════════════════════
        //  OCR PIPELINE
        // ══════════════════════════════════════════════════════════════════════

        private Task<string> OcrPlateAsync(Bitmap bmp)
        {
            return Task.Run(() =>
            {
                try
                {
                    string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
                    Directory.CreateDirectory(tempDir);
                    string ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    string srcFile = Path.Combine(tempDir, $"plate_{ts}.png");
                    bmp.Save(srcFile, System.Drawing.Imaging.ImageFormat.Png);

                    using (var src = Cv2.ImRead(srcFile, ImreadModes.Color))
                    {
                        if (src.Empty()) return "";

                        // 1. Try to detect plate region
                        Mat roi = null;
                        try { roi = FindPlateRegion(src); } catch { }

                        // 2. Build preprocessing variants from roi (or full image)
                        var variants = BuildOcrVariants(roi ?? src, tempDir, ts);
                        roi?.Dispose();

                        // 3. Setup Tesseract native path
                        SetupTesseractNativePath();

                        string tessData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                        if (!Directory.Exists(tessData))
                        {
                            _log("tessdata folder not found: " + tessData);
                            return "";
                        }

                        var availLangs = Directory.GetFiles(tessData, "*.traineddata")
                            .Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
                        if (availLangs.Count == 0) { _log("No .traineddata files found."); return ""; }

                        // Prefer eng for plate OCR (alphanumeric only), fall back to vie
                        string lang = availLangs.Contains("eng") ? "eng"
                                    : availLangs.Contains("vie") ? "vie"
                                    : availLangs[0];
                        _log("OCR language: " + lang);

                        // 4. Run OCR on all variants
                        string rawBest = RunTesseractBest(variants, tessData, lang);
                        _log($"OCR raw best: '{rawBest}'");

                        // 5. Post-process: extract Vietnamese plate pattern
                        string plate = ExtractVietnamesePlate(rawBest);
                        _log($"OCR plate result: '{plate}'");
                        return plate;
                    }
                }
                catch (Exception ex)
                {
                    _log("OCR pipeline error: " + ex.Message);
                    return "";
                }
            });
        }

        // ── Plate region detection ─────────────────────────────────────────────

        private Mat FindPlateRegion(Mat src)
        {
            using (var gray = new Mat())
            using (var blurred = new Mat())
            using (var grad = new Mat())
            using (var thresh = new Mat())
            using (var closed = new Mat())
            {
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, blurred, new OpenCvSharp.Size(5, 5), 0);

                // Morphological gradient highlights text edges better than Canny for plates
                var kSmall = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
                Cv2.MorphologyEx(blurred, grad, MorphTypes.Gradient, kSmall);
                Cv2.Threshold(grad, thresh, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

                // Close horizontally to connect individual characters into a text block
                var kClose = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(17, 3));
                Cv2.MorphologyEx(thresh, closed, MorphTypes.Close, kClose);

                OpenCvSharp.Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(closed, out contours, out hierarchy,
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                OpenCvSharp.Rect bestRect = default;
                double bestScore = 0;

                foreach (var c in contours)
                {
                    var r = Cv2.BoundingRect(c);
                    double area = r.Width * (double)r.Height;
                    double ar = r.Width / (double)Math.Max(1, r.Height);

                    // Vietnamese plates: aspect ratio ~2:1 to ~4.5:1
                    if (area < 1500 || area > src.Width * src.Height * 0.5) continue;
                    if (ar < 1.5 || ar > 6.5) continue;
                    if (r.Width < 70) continue;

                    // Prefer larger regions with ideal plate aspect ratio
                    double arScore = (ar >= 2.0 && ar <= 5.0) ? 1.5 : 1.0;
                    double score = area * arScore;
                    if (score > bestScore) { bestScore = score; bestRect = r; }
                }

                if (bestScore > 0)
                {
                    int px = (int)(bestRect.Width * 0.08);
                    int py = (int)(bestRect.Height * 0.30);
                    int x = Math.Max(0, bestRect.X - px);
                    int y = Math.Max(0, bestRect.Y - py);
                    int w = Math.Min(src.Width - x, bestRect.Width + px * 2);
                    int h = Math.Min(src.Height - y, bestRect.Height + py * 2);
                    if (w > 10 && h > 5)
                        return new Mat(src, new OpenCvSharp.Rect(x, y, w, h)).Clone();
                }
            }
            return null;
        }

        // ── Preprocessing variants ─────────────────────────────────────────────

        private List<string> BuildOcrVariants(Mat src, string tempDir, string ts)
        {
            var files = new List<string>();

            // Scale up to 1200px wide for better OCR accuracy
            const int TARGET_W = 1200;
            using (var gray = new Mat())
            using (var resized = new Mat())
            {
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                int newH = Math.Max(1, (int)(gray.Height * (TARGET_W / (double)Math.Max(1, gray.Width))));
                Cv2.Resize(gray, resized, new OpenCvSharp.Size(TARGET_W, newH), 0, 0, InterpolationFlags.Cubic);

                // Variant 1: Bilateral filter + adaptive threshold (dark text on light plate)
                TrySave(files, Path.Combine(tempDir, $"v1_{ts}.png"), () =>
                {
                    using (var bil = new Mat()) using (var thr = new Mat())
                    {
                        Cv2.BilateralFilter(resized, bil, 9, 75, 75);
                        Cv2.AdaptiveThreshold(bil, thr, 255, AdaptiveThresholdTypes.GaussianC,
                            ThresholdTypes.BinaryInv, 13, 2);
                        return thr.Clone();
                    }
                });

                // Variant 2: Bilateral filter + adaptive threshold inverted (light text on dark)
                TrySave(files, Path.Combine(tempDir, $"v2_{ts}.png"), () =>
                {
                    using (var bil = new Mat()) using (var thr = new Mat())
                    {
                        Cv2.BilateralFilter(resized, bil, 9, 75, 75);
                        Cv2.AdaptiveThreshold(bil, thr, 255, AdaptiveThresholdTypes.GaussianC,
                            ThresholdTypes.Binary, 13, 2);
                        return thr.Clone();
                    }
                });

                // Variant 3: CLAHE + Otsu binary
                TrySave(files, Path.Combine(tempDir, $"v3_{ts}.png"), () =>
                {
                    using (var cl = new Mat()) using (var thr = new Mat())
                    {
                        var clahe = Cv2.CreateCLAHE(3.0, new OpenCvSharp.Size(8, 8));
                        clahe.Apply(resized, cl);
                        Cv2.Threshold(cl, thr, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.BinaryInv);
                        return thr.Clone();
                    }
                });

                // Variant 4: Unsharp mask (sharpened) + Otsu — helps with blurry camera frames
                TrySave(files, Path.Combine(tempDir, $"v4_{ts}.png"), () =>
                {
                    using (var blur = new Mat()) using (var sharp = new Mat()) using (var thr = new Mat())
                    {
                        Cv2.GaussianBlur(resized, blur, new OpenCvSharp.Size(0, 0), 3);
                        Cv2.AddWeighted(resized, 1.5, blur, -0.5, 0, sharp);
                        Cv2.Threshold(sharp, thr, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.BinaryInv);
                        return thr.Clone();
                    }
                });

                // Variant 5: Otsu directly on resized (fast baseline)
                TrySave(files, Path.Combine(tempDir, $"v5_{ts}.png"), () =>
                {
                    using (var thr = new Mat())
                    {
                        Cv2.Threshold(resized, thr, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.BinaryInv);
                        return thr.Clone();
                    }
                });
            }

            return files;
        }

        private void TrySave(List<string> files, string path, Func<Mat> build)
        {
            try
            {
                using (var m = build())
                {
                    if (!m.Empty()) { Cv2.ImWrite(path, m); files.Add(path); }
                }
            }
            catch { }
        }

        // ── Tesseract runner ───────────────────────────────────────────────────

        private void SetupTesseractNativePath()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string arch = IntPtr.Size == 8 ? "x64" : "x86";
                string nativePath = Path.Combine(baseDir, arch);
                string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
                if (!pathEnv.Split(';').Any(p =>
                    string.Equals(p?.Trim(), nativePath, StringComparison.OrdinalIgnoreCase)))
                {
                    Environment.SetEnvironmentVariable("PATH", nativePath + ";" + pathEnv);
                    _log("Added Tesseract native path: " + nativePath);
                }
            }
            catch (Exception ex) { _log("SetupTesseractNativePath error: " + ex.Message); }
        }

        private string RunTesseractBest(List<string> variants, string tessData, string lang)
        {
            string best = "";
            try
            {
                using (var engine = new TesseractEngine(tessData, lang, EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");

                    foreach (var f in variants.Distinct())
                    {
                        if (!File.Exists(f)) continue;
                        try
                        {
                            using (var img = Pix.LoadFromFile(f))
                            {
                                // Try both PSM modes: SingleLine (1-line car plate) and
                                // SingleBlock (2-line motorcycle plate: "51H1" / "2902")
                                foreach (var psm in new[] { PageSegMode.SingleLine, PageSegMode.SingleBlock })
                                {
                                    using (var page = engine.Process(img, psm))
                                    {
                                        string res = page.GetText() ?? "";

                                        // Join ALL lines → converts 2-line plate to single string
                                        // e.g. ["51H1", "2902"] → "51H12902"
                                        string joined = string.Concat(
                                            res.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(l => new string(l.Where(c => char.IsLetterOrDigit(c)).ToArray()))
                                        ).ToUpper().Trim();

                                        _log($"  [{System.IO.Path.GetFileName(f)} PSM{(int)psm}] raw='{joined}'");

                                        if (joined.Length > best.Length) best = joined;
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { _log($"  OCR variant error: {ex.Message}"); }
                    }
                }
            }
            catch (Exception ex) { _log("Tesseract engine error: " + ex.Message); }

            return best;
        }

        // ── Post-processing: extract Vietnamese plate ─────────────────────────

        private static string ExtractVietnamesePlate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw ?? "";

            // Clean to alphanumeric uppercase
            string s = new string(raw.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpper();
            if (s.Length == 0) return "";

            // Try direct regex match: 2 digits + 1 letter + 4-7 digits
            var m = Regex.Match(s, @"\d{2}[A-Z]\d{4,7}");
            if (m.Success) return m.Value;

            // Apply positional character corrections and retry
            string corrected = ApplyPlateCorrections(s);
            m = Regex.Match(corrected, @"\d{2}[A-Z]\d{4,7}");
            if (m.Success) return m.Value;

            // Return corrected if long enough to be a plate, else original cleaned
            return corrected.Length >= 7 ? corrected : s;
        }

        // Correct OCR confusion based on expected position in Vietnamese plate:
        //   pos 0,1  → digits   (province code)
        //   pos 2    → letter   (series)
        //   pos 3+   → digits   (registration number)
        private static string ApplyPlateCorrections(string s)
        {
            if (s.Length < 3) return s;
            var sb = new StringBuilder(s);

            for (int i = 0; i < sb.Length; i++)
            {
                if (i < 2)       sb[i] = ToDigitChar(sb[i]);
                else if (i == 2) sb[i] = ToLetterChar(sb[i]);
                else             sb[i] = ToDigitChar(sb[i]);
            }
            return sb.ToString();
        }

        private static char ToDigitChar(char c)
        {
            switch (c)
            {
                case 'O': case 'Q': return '0';
                case 'I': case 'L': return '1';
                case 'Z':           return '2';
                case 'S':           return '5';
                case 'G':           return '6';
                case 'B':           return '8';
                default:            return c;
            }
        }

        private static char ToLetterChar(char c)
        {
            switch (c)
            {
                case '0': return 'O';
                case '1': return 'I';
                case '5': return 'S';
                case '6': return 'G';
                case '8': return 'B';
                default:  return c;
            }
        }
    }
}
