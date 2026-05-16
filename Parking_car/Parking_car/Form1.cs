using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Parking_car
{
    public partial class Form1 : Form
    {
        private SerialManager _serial;
        private CameraManager _cams;
        private DatabaseService _db;
        private ParkingController _controller;
        // trackbar zoom percents (default 100)
        private int _entryZoomPercent = 100;
        private int _exitZoomPercent = 100;

        public Form1()
        {
            InitializeComponent();
            InitSystem();

            try { button1.Click += button1_Click; } catch { }
            try { button2.Click += button2_Click; } catch { }
        }

        private void InitSystem()
        {
            _serial = new SerialManager();
            _cams = new CameraManager();
            _db = new DatabaseService();
            _db.Initialize();
            _db.SeedRfidMappings();

            _controller = new ParkingController(
                _serial, _cams, _db,
                Log,
                UpdateTotalCars,
                UpdateTotalsEntryExit,
                UpdatePlateEntry,
                UpdatePlateExit
            );

            _serial.LineReceived += line =>
            {
                BeginInvoke(new Action(() => _controller.HandleEspLine(line)));
            };

            _cams.LoadDevices();

            if (_cams.Devices != null && _cams.Devices.Count > 0)
            {
                int entryIndex = 0;
                int exitIndex = Math.Min(1, _cams.Devices.Count - 1);

                _cams.EntryFrameArrived += bmp =>
                {
                    BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            Image toShow = null;
                            try { toShow = ApplyZoomToImage(bmp, _entryZoomPercent, picEntry); }
                            catch { toShow = (Image)bmp.Clone(); }

                            picEntry.Image?.Dispose();
                            picEntry.Image = toShow;
                        }
                        catch { }
                        finally { try { bmp.Dispose(); } catch { } }
                    }));
                };

                _cams.ExitFrameArrived += bmp =>
                {
                    BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            Image toShow = null;
                            try { toShow = ApplyZoomToImage(bmp, _exitZoomPercent, picExit); }
                            catch { toShow = (Image)bmp.Clone(); }

                            picExit.Image?.Dispose();
                            picExit.Image = toShow;
                        }
                        catch { }
                        finally { try { bmp.Dispose(); } catch { } }
                    }));
                };

                _cams.StartEntry(entryIndex);
                _cams.StartExit(exitIndex);

                Log($"Camera ENTRY: {_cams.Devices[entryIndex].Name}");
                Log($"Camera EXIT : {_cams.Devices[exitIndex].Name}");
            }

            UpdateTotalCars(_db.CountCarsInside());
            UpdateTotalsEntryExit(_db.CountTotalEntries(), _db.CountTotalExits());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var hist = new HistoryForm(_db);
                hist.StartPosition = FormStartPosition.CenterParent;
                hist.ShowDialog(this);
            }
            catch (Exception ex)
            {
                Log("Lỗi mở lịch sử: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // attempt to find UidForm type dynamically
                var asm = Assembly.GetExecutingAssembly();
                var t = asm.GetTypes().FirstOrDefault(x => x.Name.Equals("UidForm", StringComparison.OrdinalIgnoreCase));
                if (t == null)
                {
                    Log("UID form type not found.");
                    return;
                }

                var ctor = t.GetConstructor(new[] { typeof(DatabaseService) });
                if (ctor == null)
                {
                    Log("UidForm constructor with DatabaseService not found.");
                    return;
                }

                using (var f = (Form)ctor.Invoke(new object[] { _db }))
                {
                    f.StartPosition = FormStartPosition.CenterParent;
                    f.ShowDialog(this);
                }

            }
            catch (Exception ex)
            {
                Log("Lỗi mở UID form: " + ex.Message);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                string targetPort = "COM6";

                var ports = SerialPort.GetPortNames().OrderBy(p => p).ToArray();
                Log("COM ports: " + (ports.Length == 0 ? "(none)" : string.Join(", ", ports)));

                if (!ports.Contains(targetPort))
                {
                    Log($"❌ Không thấy {targetPort}. Hãy kiểm tra Device Manager hoặc cắm lại USB.");
                    return;
                }

                if (_serial != null && _serial.IsOpen)
                {
                    _serial.Close();
                    Log("Đã đóng kết nối cũ.");
                }

                _serial.Open(targetPort, 115200);
                Log($"✅ Đã kết nối ESP32 qua {targetPort}");

                _serial.SendLine("PING");
                Log("PC> PING");
            }
            catch (Exception ex)
            {
                Log("Lỗi connect: " + ex.Message);
            }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            try { _serial.SendLine("RESET"); Log("PC> RESET"); } catch { }
        }

        private void btnResetDb_Click(object sender, EventArgs e)
        {
            try
            {
                var dr = MessageBox.Show("Are you sure you want to reset the entire database? This will delete all logs and RFID mappings.", "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr != DialogResult.Yes) return;

                // call ResetDatabase if available
                var mi = _db.GetType().GetMethod("ResetDatabase");
                if (mi != null) mi.Invoke(_db, null);

                UpdateTotalCars(0);
                UpdateTotalsEntryExit(0, 0);
                Log("Database reset performed.");
                MessageBox.Show("Database has been reset.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("Reset DB failed: " + ex.Message);
                MessageBox.Show("Reset failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { _controller?.Stop(); } catch { }
            try { _cams.StopAll(); _serial.Close(); } catch { }
        }

        private void Log(string msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Log(msg)));
                return;
            }

            string line = $"[{DateTime.Now:HH:mm:ss}] {msg}";
            try { txtLog.AppendText(line + Environment.NewLine); } catch { }
        }

        private void UpdateTotalCars(int total)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateTotalCars(total)));
                return;
            }

            try { txtTotalCars.Text = total.ToString(); } catch { }
        }

        private void UpdateTotalsEntryExit(int totalEntries, int totalExits)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateTotalsEntryExit(totalEntries, totalExits)));
                return;
            }

            try { txtTotalEntries.Text = totalEntries.ToString(); } catch { }
            try { txtTotalExits.Text = totalExits.ToString(); } catch { }
        }

        private void UpdatePlateEntry(string plate)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdatePlateEntry(plate)));
                return;
            }

            try { txtPlateEntry.Text = string.IsNullOrWhiteSpace(plate) ? "(Chưa quẹt thẻ)" : plate; } catch { }
            try { Log($"UpdatePlateEntry called -> '{plate}'"); } catch { }
        }

        private void UpdatePlateExit(string plate)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdatePlateExit(plate)));
                return;
            }

            try { txtPlateExit.Text = string.IsNullOrWhiteSpace(plate) ? "(Chưa quẹt thẻ)" : plate; } catch { }
            try { Log($"UpdatePlateExit called -> '{plate}'"); } catch { }
        }

        // small empty handlers to satisfy designer events
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox5_TextChanged(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void labelPlateExit_Click(object sender, EventArgs e) { }

        // Embedded HistoryForm kept for compatibility
        private class HistoryForm : Form
        {
            private readonly DatabaseService _db;
            private DataGridView _dgv;

            public HistoryForm(DatabaseService db)
            {
                _db = db ?? throw new ArgumentNullException(nameof(db));
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                this.Text = "Lịch sử ra/vào";
                this.ClientSize = new Size(900, 600);

                _dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "No", HeaderText = "STT", Width = 60 });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Plate", HeaderText = "Biển số" });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rfid", HeaderText = "Mã thẻ (UID)" });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeIn", HeaderText = "Thời gian vào" });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeOut", HeaderText = "Thời gian ra" });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Lane", HeaderText = "Lane" });

                this.Controls.Add(_dgv);
                this.Load += HistoryForm_Load;
            }

            private void HistoryForm_Load(object sender, EventArgs e)
            {
                LoadHistory();
            }

            private void LoadHistory()
            {
                try
                {
                    var logs = _db.GetAllLogs();
                    _dgv.Rows.Clear();
                    int idx = 1;
                    foreach (var r in logs)
                    {
                        _dgv.Rows.Add(idx++, r.Plate, r.Rfid, r.TimeIn, r.TimeOut, r.Lane);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể tải lịch sử: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Image ApplyZoomToImage(Image srcImg, int zoomPercent, PictureBox targetPic)
        {
            if (srcImg == null) return null;
            if (zoomPercent <= 100) return (Image)srcImg.Clone();

            try
            {
                double scale = zoomPercent / 100.0;
                int newW = (int)(srcImg.Width / scale);
                int newH = (int)(srcImg.Height / scale);

                newW = Math.Max(1, newW);
                newH = Math.Max(1, newH);

                int x = (srcImg.Width - newW) / 2;
                int y = (srcImg.Height - newH) / 2;

                var bmp = new Bitmap(newW, newH);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(srcImg, new Rectangle(0, 0, newW, newH), new Rectangle(x, y, newW, newH), GraphicsUnit.Pixel);
                }

                int w = targetPic?.Width ?? bmp.Width;
                int h = targetPic?.Height ?? bmp.Height;
                var display = new Bitmap(w, h);
                using (var g = Graphics.FromImage(display))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, 0, 0, display.Width, display.Height);
                }

                bmp.Dispose();
                return display;
            }
            catch
            {
                return (Image)srcImg.Clone();
            }
        }

        private void trackBarEntryZoom_Scroll(object sender, EventArgs e)
        {
            try
            {
                _entryZoomPercent = trackBarEntryZoom.Value;
                try { lblEntryZoom.Text = "Zoom: " + _entryZoomPercent + "%"; } catch { }
            }
            catch { }
        }

        private void trackBarExitZoom_Scroll(object sender, EventArgs e)
        {
            try
            {
                _exitZoomPercent = trackBarExitZoom.Value;
                try { lblExitZoom.Text = "Zoom: " + _exitZoomPercent + "%"; } catch { }
            }
            catch { }
        }
    }
}
