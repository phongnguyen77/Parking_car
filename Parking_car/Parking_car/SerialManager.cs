using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace Parking_car
{
    public class SerialManager
    {
        public SerialPort Port { get; private set; }
        public bool IsOpen => Port != null && Port.IsOpen;

        public event Action<string> LineReceived;

        public void Open(string portName, int baud = 115200)
        {
            Close();

            Port = new SerialPort(portName, baud);
            Port.NewLine = "\n";
            Port.DataReceived += (s, e) =>
            {
                try
                {
                    string line = Port.ReadLine();
                    line = line.Replace("\r", "");
                    if (!string.IsNullOrWhiteSpace(line))
                        LineReceived?.Invoke(line.Trim());
                }
                catch { /* ignore */ }
            };

            Port.Open();
        }

        public void SendLine(string text)
        {
            if (!IsOpen) return;
            try { Port.WriteLine(text); } catch { }
        }

        public void Close()
        {
            try
            {
                if (Port != null)
                {
                    if (Port.IsOpen) Port.Close();
                    Port.Dispose();
                    Port = null;
                }
            }
            catch { }
        }
    }
}

