using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Parking_car
{
    public class UidForm : Form
    {
        private readonly DatabaseService _db;
        private DataGridView _dgv;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private TextBox _txtRfid;
        private TextBox _txtPlate;

        public UidForm(DatabaseService db) //
        {
            _db = db ?? throw new ArgumentNullException(nameof(db)); 
            InitializeComponent();
            LoadMappings();
        }

        private void InitializeComponent()
        {
            this.Text = "RFID Registry";
            this.ClientSize = new Size(600, 400);

            _dgv = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(400, 300),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rfid", HeaderText = "RFID" });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Plate", HeaderText = "Plate" });

            _txtRfid = new TextBox { Location = new Point(420, 30), Width = 150 };
            _txtPlate = new TextBox { Location = new Point(420, 70), Width = 150 };

            _btnAdd = new Button { Text = "Add/Save", Location = new Point(420, 110), Width = 150 };
            _btnEdit = new Button { Text = "Load Selected", Location = new Point(420, 150), Width = 150 };
            _btnDelete = new Button { Text = "Delete", Location = new Point(420, 190), Width = 150 };

            _btnAdd.Click += BtnAdd_Click;
            _btnEdit.Click += BtnEdit_Click;
            _btnDelete.Click += BtnDelete_Click;

            this.Controls.Add(_dgv);
            this.Controls.Add(_txtRfid);
            this.Controls.Add(_txtPlate);
            this.Controls.Add(_btnAdd);
            this.Controls.Add(_btnEdit);
            this.Controls.Add(_btnDelete);
        }

        private void LoadMappings()
        {
            try
            {
                var list = _db.GetAllRfidMappings();
                _dgv.Rows.Clear();
                foreach (var kv in list)
                {
                    _dgv.Rows.Add(kv.Key, kv.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load mappings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string rfid = _txtRfid.Text.Trim();
            string plate = _txtPlate.Text.Trim();
            if (string.IsNullOrWhiteSpace(rfid))
            {
                MessageBox.Show("RFID cannot be empty", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _db.RegisterRfidMapping(rfid, plate);
                LoadMappings();
                MessageBox.Show("Saved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_dgv.SelectedRows.Count == 0) return;
            var row = _dgv.SelectedRows[0];
            _txtRfid.Text = Convert.ToString(row.Cells[0].Value);
            _txtPlate.Text = Convert.ToString(row.Cells[1].Value);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_dgv.SelectedRows.Count == 0) return;
            var row = _dgv.SelectedRows[0];
            string rfid = Convert.ToString(row.Cells[0].Value);

            var ok = MessageBox.Show($"Delete {rfid}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) return;

            try
            {
                _db.DeleteRfidMapping(rfid);
                LoadMappings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
