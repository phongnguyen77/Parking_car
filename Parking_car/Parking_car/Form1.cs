using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
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
        private int _entryZoomPercent = 100;
        private int _exitZoomPercent = 100;

        // Overlay thông báo mở barie
        private Label _lblNotifyEntry;
        private Label _lblNotifyExit;
        private Timer _timerEntry;
        private Timer _timerExit;

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

            InitNotifyOverlays();

            _controller = new ParkingController(
                _serial, _cams, _db,
                Log,
                UpdateTotalCars,
                UpdateTotalsEntryExit,
                UpdatePlateEntry,
                UpdatePlateExit,
                plateRecognizerToken: "a539ee56e86aa996a4300dc14b9f421b8fc8277d",
                notifyEntryOpen: ShowEntryNotify,
                notifyExitOpen:  ShowExitNotify,
                warnEntry:       ShowEntryWarning,
                warnExit:        ShowExitWarning
            );

            _serial.LineReceived += line =>
            {
                BeginInvoke(new Action(() => _controller.HandleEspLine(line)));
            };

            InitOcrButtons();

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
            MessageBox.Show("Chức năng gán thẻ đã được tắt.\nHệ thống hiện dùng camera đọc biển số tự động.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            try
            {
                txtLog.AppendText(line + Environment.NewLine);
                txtLog.SelectionStart = txtLog.TextLength;
                txtLog.ScrollToCaret();
            } catch { }
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
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdatePlateEntry(plate))); return; }
            try { txtPlateEntry.Text = string.IsNullOrWhiteSpace(plate) ? "(Chờ xe vào)" : plate; } catch { }
        }

        private void UpdatePlateExit(string plate)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdatePlateExit(plate))); return; }
            try { txtPlateExit.Text = string.IsNullOrWhiteSpace(plate) ? "(Chờ xe ra)" : plate; } catch { }
        }

        private void InitOcrButtons()
        {
            AddLaneButtons(pnlEntryCard,
                onOcr:   async () => await _controller.RetriggerEntryOcrAsync(),
                onOpen:  ()      => _controller.ManualOpenEntry(),
                onClose: ()      => _controller.ManualCloseEntry(),
                onIssue: async () => await _controller.IssueCardEntryAsync(
                    txtPlateEntry.Text.Trim().Replace("(Chờ xe vào)", "").Trim()));

            AddLaneButtons(pnlExitCard,
                onOcr:   async () => await _controller.RetriggerExitOcrAsync(),
                onOpen:  ()      => _controller.ManualOpenExit(),
                onClose: ()      => _controller.ManualCloseExit(),
                onIssue: async () => await _controller.IssueCardExitAsync(
                    txtPlateExit.Text.Trim().Replace("(Chờ xe ra)", "").Trim()));
        }

        private void AddLaneButtons(Panel panel, Func<Task> onOcr, Action onOpen, Action onClose, Func<Task> onIssue)
        {
            // 4 button chia đều trong 430px (x=10..440), gap 3px
            // Mỗi button rộng 104px, button cuối 109px
            // B1 x=10  B2 x=117  B3 x=224  B4 x=331
            var specs = new[]
            {
                (x:10,  w:104, text:"ĐỌC LẠI BIỂN", back:Color.FromArgb(0,80,130),   border:Color.FromArgb(0,180,220)),
                (x:117, w:104, text:"MỞ BARIE",      back:Color.FromArgb(0,130,55),   border:Color.FromArgb(0,220,100)),
                (x:224, w:104, text:"ĐÓNG BARIE",    back:Color.FromArgb(110,25,25),  border:Color.FromArgb(210,55,55)),
                (x:331, w:109, text:"CẤP THẺ",       back:Color.FromArgb(160,90,0),   border:Color.FromArgb(255,160,0)),
            };

            Action<object,EventArgs>[] handlers =
            {
                async (s,e) => await onOcr(),
                (s,e)       => onOpen(),
                (s,e)       => onClose(),
                async (s,e) => await onIssue(),
            };

            for (int i = 0; i < specs.Length; i++)
            {
                var (x, w, text, back, border) = specs[i];
                var hdl = handlers[i];
                var btn = new Button
                {
                    Location  = new Point(x, 532),
                    Size      = new Size(w, 46),
                    Text      = text,
                    Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = back,
                    FlatStyle = FlatStyle.Flat,
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = border;
                btn.Click += new EventHandler(hdl);
                panel.Controls.Add(btn);
                btn.BringToFront();
            }
        }

        private void InitNotifyOverlays()
        {
            // Overlay "MỜI XE VÀO" — đè lên picEntry (Location=10,44 Size=430,340)
            _lblNotifyEntry = new Label
            {
                Location  = new Point(10, 44),
                Size      = new Size(430, 340),
                Text      = "MỜI XE VÀO",
                Font      = new Font("Segoe UI", 36f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 150, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible   = false
            };
            pnlEntryCard.Controls.Add(_lblNotifyEntry);
            _lblNotifyEntry.BringToFront();

            // Overlay "MỜI XE RA" — đè lên picExit
            _lblNotifyExit = new Label
            {
                Location  = new Point(10, 44),
                Size      = new Size(430, 340),
                Text      = "MỜI XE RA",
                Font      = new Font("Segoe UI", 36f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 110, 190),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible   = false
            };
            pnlExitCard.Controls.Add(_lblNotifyExit);
            _lblNotifyExit.BringToFront();

            // Timer tự ẩn sau 4 giây
            _timerEntry = new Timer { Interval = 4000 };
            _timerEntry.Tick += (s, e) => { _lblNotifyEntry.Visible = false; _timerEntry.Stop(); };

            _timerExit = new Timer { Interval = 4000 };
            _timerExit.Tick += (s, e) => { _lblNotifyExit.Visible = false; _timerExit.Stop(); };
        }

        private void ShowEntryNotify()
        {
            if (InvokeRequired) { BeginInvoke(new Action(ShowEntryNotify)); return; }
            _lblNotifyEntry.Text      = "MỜI XE VÀO";
            _lblNotifyEntry.BackColor = Color.FromArgb(0, 150, 60);
            _lblNotifyEntry.Font      = new Font("Segoe UI", 36f, FontStyle.Bold);
            _lblNotifyEntry.Visible   = true;
            _timerEntry.Stop();
            _timerEntry.Start();
        }

        private void ShowExitNotify()
        {
            if (InvokeRequired) { BeginInvoke(new Action(ShowExitNotify)); return; }
            _lblNotifyExit.Text      = "MỜI XE RA";
            _lblNotifyExit.BackColor = Color.FromArgb(0, 110, 190);
            _lblNotifyExit.Font      = new Font("Segoe UI", 36f, FontStyle.Bold);
            _lblNotifyExit.Visible   = true;
            _timerExit.Stop();
            _timerExit.Start();
        }

        private void ShowEntryWarning(string msg)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => ShowEntryWarning(msg))); return; }
            _lblNotifyEntry.Text      = msg;
            _lblNotifyEntry.BackColor = Color.FromArgb(180, 30, 30);
            _lblNotifyEntry.Font      = new Font("Segoe UI", 22f, FontStyle.Bold);
            _lblNotifyEntry.Visible   = true;
            _timerEntry.Stop();
            _timerEntry.Start();
        }

        private void ShowExitWarning(string msg)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => ShowExitWarning(msg))); return; }
            _lblNotifyExit.Text      = msg;
            _lblNotifyExit.BackColor = Color.FromArgb(180, 30, 30);
            _lblNotifyExit.Font      = new Font("Segoe UI", 22f, FontStyle.Bold);
            _lblNotifyExit.Visible   = true;
            _timerExit.Stop();
            _timerExit.Start();
        }

        // small empty handlers to satisfy designer events
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox5_TextChanged(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void labelPlateExit_Click(object sender, EventArgs e) { }

        // Embedded HistoryForm
        private class HistoryForm : Form
        {
            private readonly DatabaseService _db;
            private DataGridView _dgv;
            private Button _btnRefresh;
            private Label _lblCount;

            public HistoryForm(DatabaseService db)
            {
                _db = db ?? throw new ArgumentNullException(nameof(db));
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                this.Text = "Lịch sử ra/vào";
                this.ClientSize = new Size(1000, 620);
                this.StartPosition = FormStartPosition.CenterParent;

                // Toolbar panel
                var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6, 4, 6, 0) };

                _btnRefresh = new Button
                {
                    Text = "Làm mới",
                    Width = 90,
                    Height = 30,
                    Location = new Point(6, 5)
                };
                _btnRefresh.Click += (s, e) => LoadHistory();

                _lblCount = new Label
                {
                    AutoSize = true,
                    Location = new Point(110, 10),
                    Font = new Font("Segoe UI", 9f)
                };

                toolbar.Controls.Add(_btnRefresh);
                toolbar.Controls.Add(_lblCount);

                _dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    RowHeadersVisible = false,
                    Font = new Font("Segoe UI", 9.5f),
                    GridColor = Color.LightGray,
                    BorderStyle = BorderStyle.None,
                    AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 248, 248) }
                };
                _dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "No",      HeaderText = "STT",           Width = 50,  AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Plate",   HeaderText = "Biển số" });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rfid",    HeaderText = "Mã thẻ (UID)" });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeIn",  HeaderText = "Thời gian vào" });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeOut", HeaderText = "Thời gian ra" });
                _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status",  HeaderText = "Trạng thái",    Width = 110, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });

                // Tô màu theo trạng thái
                _dgv.CellFormatting += (s, e) =>
                {
                    if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                    var row = _dgv.Rows[e.RowIndex];
                    string timeOut = row.Cells["TimeOut"].Value?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(timeOut))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(198, 239, 206); // xanh lá nhạt
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 97, 0);
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                };

                this.Controls.Add(_dgv);
                this.Controls.Add(toolbar);
                this.Load += (s, e) => LoadHistory();
            }

            private void LoadHistory()
            {
                try
                {
                    var logs = _db.GetAllLogs();
                    _dgv.Rows.Clear();
                    int idx = 1;
                    int inside = 0;
                    foreach (var r in logs)
                    {
                        bool parked = string.IsNullOrWhiteSpace(r.TimeOut);
                        string status = parked ? "Đang đậu" : "Đã ra";
                        _dgv.Rows.Add(idx++, r.Plate, r.Rfid, r.TimeIn, r.TimeOut, status);
                        if (parked) inside++;
                    }
                    _lblCount.Text = $"Tổng: {logs.Count} lượt  |  Đang đậu: {inside}";
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
