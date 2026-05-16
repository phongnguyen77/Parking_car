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

            // Chờ quẹt thẻ RFID
            await WaitForConditionAsync(() => _lastEntryRfid != null || !IsEntrySessionValid(), 50);

            if (_cts.IsCancellationRequested) { _entrySessionActive = false; return; }
            if (!IsEntrySessionValid()) { _entrySessionActive = false; _log("ENTRY session expired."); return; }

            string rfid = _lastEntryRfid;
            string plate = _db.GetMappedPlateForRfid(rfid) ?? "";

            if (string.IsNullOrEmpty(plate))
            {
                _log($"RFID {rfid} chưa được gán biển số. Không mở barie!");
                _entrySessionActive = false;
                return;
            }

            _log($"RFID {rfid} → Biển số: {plate}");
            try { _updateEntryPlate?.Invoke(plate); } catch { }

            // Chụp ảnh lưu lại
            Bitmap snap = null;
            try { snap = _cams?.SnapshotEntry(); } catch { }
            string imgPath = SaveSnapshot(snap, "entry");

            string timeIn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try { _db.InsertEntry(rfid, plate, "ENTRY", timeIn, imgPath); } catch { }
            try { _serial.SendLine("OPEN_ENTRY"); _log("PC> OPEN_ENTRY"); } catch { }

            _entrySessionActive = false;
            UpdateTotalCarsFromDb();
        }

        private async Task HandleExitDetectedAsync()
        {
            if (_cts.IsCancellationRequested) return;

            // Chờ quẹt thẻ RFID
            await WaitForConditionAsync(() => _lastExitRfid != null || !IsExitSessionValid(), 50);

            if (_cts.IsCancellationRequested) { _exitSessionActive = false; return; }
            if (!IsExitSessionValid()) { _exitSessionActive = false; _log("EXIT session expired."); return; }

            string rfid = _lastExitRfid;
            string plate = _db.GetMappedPlateForRfid(rfid) ?? "";

            _log($"RFID {rfid} → Biển số: {plate}");
            try { _updateExitPlate?.Invoke(plate); } catch { }

            // Chụp ảnh lưu lại
            Bitmap snap = null;
            try { snap = _cams?.SnapshotExit(); } catch { }
            string imgPath = SaveSnapshot(snap, "exit");

            string timeOut = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool ok = false;
            try { ok = _db.UpdateExitByRfid(rfid, timeOut, imgPath); } catch { }

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
