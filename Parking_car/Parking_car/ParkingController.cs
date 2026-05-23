using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Parking_car
{
    public class ParkingController
    {
        private readonly SerialManager _serial;
        private readonly CameraManager _cams;
        private readonly DatabaseService _db;
        private readonly ApiService _api;
        private readonly PlateRecognizerService _plateOcr;
        private readonly Action<string> _log;
        private readonly Action _notifyEntry;
        private readonly Action _notifyExit;
        private readonly Action<string> _warnEntry;
        private readonly Action<string> _warnExit;
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
            Action<string> updateExitPlate = null,
            string plateRecognizerToken = "",
            Action notifyEntryOpen = null,
            Action notifyExitOpen = null,
            Action<string> warnEntry = null,
            Action<string> warnExit = null
        )
        {
            _serial = serial;
            _cams = cams;
            _db = db;
            _api = new ApiService(logger, ApiService.CloudUrl);
            _plateOcr = new PlateRecognizerService(plateRecognizerToken, logger);
            _log = logger;
            _notifyEntry = notifyEntryOpen;
            _notifyExit = notifyExitOpen;
            _warnEntry = warnEntry;
            _warnExit = warnExit;
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

        public void ManualOpenEntry()
        {
            try { _serial.SendLine("OPEN_ENTRY"); _log("PC> OPEN_ENTRY (bảo vệ mở thủ công)"); } catch { }
            try { _notifyEntry?.Invoke(); } catch { }
        }

        public void ManualCloseEntry()
        {
            try { _serial.SendLine("CLOSE_ENTRY"); _log("PC> CLOSE_ENTRY (bảo vệ đóng thủ công)"); } catch { }
            _entrySessionActive = false;
        }

        public void ManualOpenExit()
        {
            try { _serial.SendLine("OPEN_EXIT"); _log("PC> OPEN_EXIT (bảo vệ mở thủ công)"); } catch { }
            try { _notifyExit?.Invoke(); } catch { }
        }

        public void ManualCloseExit()
        {
            try { _serial.SendLine("CLOSE_EXIT"); _log("PC> CLOSE_EXIT (bảo vệ đóng thủ công)"); } catch { }
            _exitSessionActive = false;
        }

        // Bảo vệ nhấn "Cấp thẻ" → chờ quẹt thẻ → mở barie vào
        public async Task IssueCardEntryAsync(string plate)
        {
            if (_entrySessionActive) { _log("[CẤP THẺ] Đang có session vào, bỏ qua."); return; }
            _entrySessionActive = true;
            _entryStart = DateTime.Now;
            _lastEntryRfid = null;

            string display = string.IsNullOrWhiteSpace(plate) ? "Quét thẻ!" : $"{plate}  —  Quét thẻ!";
            try { _updateEntryPlate?.Invoke(display); } catch { }
            _log($"[CẤP THẺ] Chờ quét thẻ xe vào: {(string.IsNullOrWhiteSpace(plate) ? "(chưa có biển)" : plate)}");

            await WaitForConditionAsync(() => _lastEntryRfid != null || !IsEntrySessionValid(), 50);

            if (_cts.IsCancellationRequested || !IsEntrySessionValid())
            {
                _entrySessionActive = false;
                _log("[CẤP THẺ] Hết giờ chờ thẻ.");
                try { _updateEntryPlate?.Invoke("(Hết giờ)"); } catch { }
                return;
            }

            string rfid = _lastEntryRfid;
            _log($"[CẤP THẺ] RFID: {rfid} | Biển: {plate}");
            try { _updateEntryPlate?.Invoke(plate); } catch { }

            Bitmap snap = null;
            try { snap = _cams?.SnapshotEntry(); } catch { }
            string imgPath = SaveSnapshot(snap, "entry");
            string timeIn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try { _db.InsertEntry(rfid, plate, "ENTRY", timeIn, imgPath); } catch { }
            _ = _api.PostEntryAsync(rfid, plate, "ENTRY", timeIn, imgPath);
            try { _serial.SendLine("OPEN_ENTRY"); _log("PC> OPEN_ENTRY"); } catch { }
            try { _notifyEntry?.Invoke(); } catch { }

            _entrySessionActive = false;
            UpdateTotalCarsFromDb();
        }

        // Bảo vệ nhấn "Cấp thẻ" → chờ quẹt thẻ → mở barie ra
        public async Task IssueCardExitAsync(string plate)
        {
            if (_exitSessionActive) { _log("[CẤP THẺ] Đang có session ra, bỏ qua."); return; }
            _exitSessionActive = true;
            _exitStart = DateTime.Now;
            _lastExitRfid = null;

            string display = string.IsNullOrWhiteSpace(plate) ? "Quét thẻ!" : $"{plate}  —  Quét thẻ!";
            try { _updateExitPlate?.Invoke(display); } catch { }
            _log($"[CẤP THẺ] Chờ quét thẻ xe ra: {(string.IsNullOrWhiteSpace(plate) ? "(chưa có biển)" : plate)}");

            await WaitForConditionAsync(() => _lastExitRfid != null || !IsExitSessionValid(), 50);

            if (_cts.IsCancellationRequested || !IsExitSessionValid())
            {
                _exitSessionActive = false;
                _log("[CẤP THẺ] Hết giờ chờ thẻ.");
                try { _updateExitPlate?.Invoke("(Hết giờ)"); } catch { }
                return;
            }

            string rfid = _lastExitRfid;
            _log($"[CẤP THẺ] RFID: {rfid} | Biển: {plate}");
            try { _updateExitPlate?.Invoke(plate); } catch { }

            Bitmap snap = null;
            try { snap = _cams?.SnapshotExit(); } catch { }
            string imgPath = SaveSnapshot(snap, "exit");
            string timeOut = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool ok = false;
            try { ok = _db.UpdateExitByRfid(rfid, timeOut, imgPath); } catch { }

            if (!ok)
            {
                _log("[CẤP THẺ] Không tìm thấy xe với thẻ này.");
                _exitSessionActive = false;
                return;
            }

            _ = _api.PostExitAsync(rfid, plate, "EXIT", timeOut, imgPath);
            try { _serial.SendLine("OPEN_EXIT"); _log("PC> OPEN_EXIT"); } catch { }
            try { _notifyExit?.Invoke(); } catch { }

            _exitSessionActive = false;
            UpdateTotalCarsFromDb();
        }

        public async Task RetriggerEntryOcrAsync()
        {
            Bitmap snap = null;
            try { snap = _cams?.SnapshotEntry(); } catch { }
            _log("[OCR] Đọc lại biển vào...");
            string plate = await _plateOcr.RecognizeAsync(snap);
            try { snap?.Dispose(); } catch { }
            if (!string.IsNullOrEmpty(plate))
            {
                _log($"[OCR] Biển vào: {plate}");
                try { _updateEntryPlate?.Invoke(plate); } catch { }
            }
            else
            {
                _log("[OCR] Đọc lại: không nhận diện được biển.");
                try { _updateEntryPlate?.Invoke("(Không đọc được)"); } catch { }
            }
        }

        public async Task RetriggerExitOcrAsync()
        {
            Bitmap snap = null;
            try { snap = _cams?.SnapshotExit(); } catch { }
            _log("[OCR] Đọc lại biển ra...");
            string plate = await _plateOcr.RecognizeAsync(snap);
            try { snap?.Dispose(); } catch { }
            if (!string.IsNullOrEmpty(plate))
            {
                _log($"[OCR] Biển ra: {plate}");
                try { _updateExitPlate?.Invoke(plate); } catch { }
            }
            else
            {
                _log("[OCR] Đọc lại: không nhận diện được biển.");
                try { _updateExitPlate?.Invoke("(Không đọc được)"); } catch { }
            }
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
                return;
            }

            if (line.StartsWith("RFID_EXIT:UID="))
            {
                _lastExitRfid = line.Replace("RFID_EXIT:UID=", "").Trim();
                _log($"RFID EXIT UID = {_lastExitRfid}");
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

            // 1. Chụp ảnh + OCR — thử lại đến khi đọc được biển hoặc hết session
            try { _updateEntryPlate?.Invoke("Đang đọc biển số..."); } catch { }

            Bitmap snap = null;
            string ocrPlate = "";
            int attempt = 0;

            while (string.IsNullOrEmpty(ocrPlate))
            {
                if (_cts.IsCancellationRequested || !IsEntrySessionValid())
                {
                    try { snap?.Dispose(); } catch { }
                    _entrySessionActive = false;
                    _log("ENTRY timeout khi đọc biển số.");
                    try { _updateEntryPlate?.Invoke("(Hết giờ)"); } catch { }
                    return;
                }

                try { snap?.Dispose(); } catch { }
                snap = null;
                try { snap = _cams?.SnapshotEntry(); } catch { }

                attempt++;
                _log($"[OCR] Lần {attempt}: đang đọc biển...");
                ocrPlate = await _plateOcr.RecognizeAsync(snap);

                if (string.IsNullOrEmpty(ocrPlate))
                {
                    _log("[OCR] Không đọc được — thử lại sau 1.5s");
                    try { _updateEntryPlate?.Invoke($"Đang đọc biển số... (lần {attempt})"); } catch { }
                    await Task.Delay(1500);
                }
            }

            _log($"[OCR] Biển vào: {ocrPlate}");

            // Kiểm tra xe đã trong bãi chưa
            if (_db.IsPlateInside(ocrPlate))
            {
                string warn = $"Xe {ocrPlate} đã có trong bãi!";
                _log($"[CẢNH BÁO] {warn}");
                try { snap?.Dispose(); } catch { }
                try { _updateEntryPlate?.Invoke($"ĐÃ TRONG BÃI: {ocrPlate}"); } catch { }
                try { _warnEntry?.Invoke(warn); } catch { }
                _entrySessionActive = false;
                return;
            }

            // 2. Hiển thị biển + nhắc quét thẻ
            try { _updateEntryPlate?.Invoke($"{ocrPlate}  —  Quét thẻ!"); } catch { }

            // 3. Chờ RFID
            await WaitForConditionAsync(() => _lastEntryRfid != null || !IsEntrySessionValid(), 50);

            if (_cts.IsCancellationRequested) { _entrySessionActive = false; return; }
            if (!IsEntrySessionValid())
            {
                _entrySessionActive = false;
                _log("ENTRY session timeout.");
                try { _updateEntryPlate?.Invoke("(Hết giờ)"); } catch { }
                return;
            }

            string rfid = _lastEntryRfid;
            _log($"RFID: {rfid}  |  Biển OCR: {(ocrPlate.Length > 0 ? ocrPlate : "(trống)")}");
            try { _updateEntryPlate?.Invoke(!string.IsNullOrEmpty(ocrPlate) ? ocrPlate : "(không đọc được)"); } catch { }

            // 4. Lưu và mở barie
            string imgPath = SaveSnapshot(snap, "entry");
            string timeIn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try { _db.InsertEntry(rfid, ocrPlate, "ENTRY", timeIn, imgPath); } catch { }
            _ = _api.PostEntryAsync(rfid, ocrPlate, "ENTRY", timeIn, imgPath);
            try { _serial.SendLine("OPEN_ENTRY"); _log("PC> OPEN_ENTRY"); } catch { }
            try { _notifyEntry?.Invoke(); } catch { }

            _entrySessionActive = false;
            UpdateTotalCarsFromDb();
        }

        private async Task HandleExitDetectedAsync()
        {
            if (_cts.IsCancellationRequested) return;

            // 1. Chụp ảnh + OCR — thử lại đến khi đọc được biển hoặc hết session
            try { _updateExitPlate?.Invoke("Đang đọc biển số..."); } catch { }

            Bitmap snap = null;
            string ocrPlate = "";
            int attempt = 0;

            while (string.IsNullOrEmpty(ocrPlate))
            {
                if (_cts.IsCancellationRequested || !IsExitSessionValid())
                {
                    try { snap?.Dispose(); } catch { }
                    _exitSessionActive = false;
                    _log("EXIT timeout khi đọc biển số.");
                    try { _updateExitPlate?.Invoke("(Hết giờ)"); } catch { }
                    return;
                }

                try { snap?.Dispose(); } catch { }
                snap = null;
                try { snap = _cams?.SnapshotExit(); } catch { }

                attempt++;
                _log($"[OCR] Lần {attempt}: đang đọc biển...");
                ocrPlate = await _plateOcr.RecognizeAsync(snap);

                if (string.IsNullOrEmpty(ocrPlate))
                {
                    _log("[OCR] Không đọc được — thử lại sau 1.5s");
                    try { _updateExitPlate?.Invoke($"Đang đọc biển số... (lần {attempt})"); } catch { }
                    await Task.Delay(1500);
                }
            }

            _log($"[OCR] Biển ra: {ocrPlate}");

            // Kiểm tra xe có trong bãi không
            if (!_db.IsPlateInside(ocrPlate))
            {
                string warn = $"Xe {ocrPlate} không có trong bãi!";
                _log($"[CẢNH BÁO] {warn}");
                try { snap?.Dispose(); } catch { }
                try { _updateExitPlate?.Invoke($"KHÔNG TRONG BÃI: {ocrPlate}"); } catch { }
                try { _warnExit?.Invoke(warn); } catch { }
                _exitSessionActive = false;
                return;
            }

            // 2. Hiển thị biển + nhắc quét thẻ
            try { _updateExitPlate?.Invoke($"{ocrPlate}  —  Quét thẻ!"); } catch { }

            // 3. Chờ RFID
            await WaitForConditionAsync(() => _lastExitRfid != null || !IsExitSessionValid(), 50);

            if (_cts.IsCancellationRequested) { _exitSessionActive = false; return; }
            if (!IsExitSessionValid())
            {
                _exitSessionActive = false;
                _log("EXIT session timeout.");
                try { _updateExitPlate?.Invoke("(Hết giờ)"); } catch { }
                return;
            }

            string rfid = _lastExitRfid;
            _log($"RFID: {rfid}  |  Biển OCR: {(ocrPlate.Length > 0 ? ocrPlate : "(trống)")}");
            try { _updateExitPlate?.Invoke(!string.IsNullOrEmpty(ocrPlate) ? ocrPlate : "(không đọc được)"); } catch { }

            // 4. Tìm xe trong bãi theo RFID
            string imgPath = SaveSnapshot(snap, "exit");
            string timeOut = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool ok = false;
            try { ok = _db.UpdateExitByRfid(rfid, timeOut, imgPath); } catch { }

            if (!ok)
            {
                _log("Không tìm thấy xe trong bãi với thẻ này. Không mở barie!");
                _exitSessionActive = false;
                return;
            }

            _ = _api.PostExitAsync(rfid, ocrPlate, "EXIT", timeOut, imgPath);
            try { _serial.SendLine("OPEN_EXIT"); _log("PC> OPEN_EXIT"); } catch { }
            try { _notifyExit?.Invoke(); } catch { }

            _exitSessionActive = false;
            UpdateTotalCarsFromDb();
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
    }
}
