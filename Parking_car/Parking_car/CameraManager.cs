using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;

namespace Parking_car
{
    public class CameraManager
    {
        public FilterInfoCollection Devices { get; private set; }
        public VideoCaptureDevice CamEntry { get; private set; }
        public VideoCaptureDevice CamExit { get; private set; }

        private Bitmap _lastEntryFrame;
        private Bitmap _lastExitFrame;

        public event Action<Bitmap> EntryFrameArrived;
        public event Action<Bitmap> ExitFrameArrived;

        public void LoadDevices()
        {
            Devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }

        public void StartEntry(int deviceIndex)
        {
            StopEntry();
            CamEntry = new VideoCaptureDevice(Devices[deviceIndex].MonikerString);
            CamEntry.NewFrame += (s, e) =>
            {
                try
                {
                    _lastEntryFrame?.Dispose();
                    _lastEntryFrame = (Bitmap)e.Frame.Clone();
                    EntryFrameArrived?.Invoke((Bitmap)_lastEntryFrame.Clone());
                }
                catch { }
            };
            CamEntry.Start();
        }

        public void StartExit(int deviceIndex)
        {
            StopExit();
            CamExit = new VideoCaptureDevice(Devices[deviceIndex].MonikerString);
            CamExit.NewFrame += (s, e) =>
            {
                try
                {
                    _lastExitFrame?.Dispose();
                    _lastExitFrame = (Bitmap)e.Frame.Clone();
                    ExitFrameArrived?.Invoke((Bitmap)_lastExitFrame.Clone());
                }
                catch { }
            };
            CamExit.Start();
        }

        public Bitmap SnapshotEntry()
        {
            if (_lastEntryFrame == null) return null;
            return (Bitmap)_lastEntryFrame.Clone();
        }

        public Bitmap SnapshotExit()
        {
            if (_lastExitFrame == null) return null;
            return (Bitmap)_lastExitFrame.Clone();
        }

        public void StopEntry()
        {
            try
            {
                if (CamEntry != null)
                {
                    if (CamEntry.IsRunning)
                    {
                        CamEntry.SignalToStop();
                        CamEntry.WaitForStop();
                    }
                    CamEntry = null;
                }
            }
            catch { }
        }

        public void StopExit()
        {
            try
            {
                if (CamExit != null)
                {
                    if (CamExit.IsRunning)
                    {
                        CamExit.SignalToStop();
                        CamExit.WaitForStop();
                    }
                    CamExit = null;
                }
            }
            catch { }
        }

        public void StopAll()
        {
            StopEntry();
            StopExit();
            _lastEntryFrame?.Dispose(); _lastEntryFrame = null;
            _lastExitFrame?.Dispose(); _lastExitFrame = null;
        }
    }
}
