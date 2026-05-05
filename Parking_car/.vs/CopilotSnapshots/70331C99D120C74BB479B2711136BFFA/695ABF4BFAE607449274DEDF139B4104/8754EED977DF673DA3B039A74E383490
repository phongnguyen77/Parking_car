using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parking_car
{
    public partial class HistoryForm : Form
    {
        private readonly DatabaseService _db;
        private DataGridView _dgv;

        // Parameterless constructor kept for designer support
        public HistoryForm()
        {
            InitializeComponent();
            InitializeRuntimeComponents();
        }

        // New ctor used by Form1
        public HistoryForm(DatabaseService db) : this()
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        private void InitializeRuntimeComponents()
        {
            // create DataGridView at runtime
            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _dgv.Columns.Clear();
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
            // if db is not provided, try to create one (fallback)
            if (_db == null)
            {
                try
                {
                    // attempt to use default DatabaseService
                    var tmp = new DatabaseService();
                    LoadHistory(tmp);
                }
                catch
                {
                    // nothing
                }
            }
            else
            {
                LoadHistory(_db);
            }
        }

        private void LoadHistory(DatabaseService db)
        {
            try
            {
                var logs = db.GetAllLogs();
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
}
