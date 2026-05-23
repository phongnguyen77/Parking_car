namespace Parking_car
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label1;
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlHeaderLine = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnConnect = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnResetDb = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.pnlEntryCard = new System.Windows.Forms.Panel();
            this.txtPlateEntry = new System.Windows.Forms.TextBox();
            this.labelPlateEntry = new System.Windows.Forms.Label();
            this.pnlSep1 = new System.Windows.Forms.Panel();
            this.lblEntryZoom = new System.Windows.Forms.Label();
            this.trackBarEntryZoom = new System.Windows.Forms.TrackBar();
            this.picEntry = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlStatsCard = new System.Windows.Forms.Panel();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.pnlSep3 = new System.Windows.Forms.Panel();
            this.txtTotalExits = new System.Windows.Forms.TextBox();
            this.labelExits = new System.Windows.Forms.Label();
            this.txtTotalEntries = new System.Windows.Forms.TextBox();
            this.labelEntries = new System.Windows.Forms.Label();
            this.pnlSep2 = new System.Windows.Forms.Panel();
            this.txtTotalCars = new System.Windows.Forms.TextBox();
            this.pnlExitCard = new System.Windows.Forms.Panel();
            this.txtPlateExit = new System.Windows.Forms.TextBox();
            this.labelPlateExit = new System.Windows.Forms.Label();
            this.pnlSep4 = new System.Windows.Forms.Panel();
            this.lblExitZoom = new System.Windows.Forms.Label();
            this.trackBarExitZoom = new System.Windows.Forms.TrackBar();
            this.picExit = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlEntryCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarEntryZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picEntry)).BeginInit();
            this.pnlStatsCard.SuspendLayout();
            this.pnlExitCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarExitZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picExit)).BeginInit();
            this.SuspendLayout();
            // 
            // label4
            // 
            label4.BackColor = System.Drawing.Color.Transparent;
            label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(155)))), ((int)(((byte)(185)))));
            label4.Location = new System.Drawing.Point(0, 336);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(420, 22);
            label4.TabIndex = 12;
            label4.Text = "STATUS LOG";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label1
            // 
            label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(34)))), ((int)(((byte)(52)))));
            label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(212)))), ((int)(((byte)(255)))));
            label1.Location = new System.Drawing.Point(0, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(420, 36);
            label1.TabIndex = 203;
            label1.Text = "TỔNG XE TRONG BÃI";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(33)))), ((int)(((byte)(62)))));
            this.pnlHeader.Controls.Add(this.pnlHeaderLine);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1366, 62);
            this.pnlHeader.TabIndex = 100;
            // 
            // pnlHeaderLine
            // 
            this.pnlHeaderLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(212)))), ((int)(((byte)(255)))));
            this.pnlHeaderLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlHeaderLine.Location = new System.Drawing.Point(0, 59);
            this.pnlHeaderLine.Name = "pnlHeaderLine";
            this.pnlHeaderLine.Size = new System.Drawing.Size(1366, 3);
            this.pnlHeaderLine.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(212)))), ((int)(((byte)(255)))));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1366, 62);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "HỆ THỐNG QUẢN LÝ BÃI ĐỖ XE";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(52)))), ((int)(((byte)(96)))));
            this.pnlToolbar.Controls.Add(this.btnConnect);
            this.pnlToolbar.Controls.Add(this.button1);
            this.pnlToolbar.Controls.Add(this.button2);
            this.pnlToolbar.Controls.Add(this.btnResetDb);
            this.pnlToolbar.Controls.Add(this.btnRestart);
            this.pnlToolbar.Controls.Add(this.btnExit);
            this.pnlToolbar.Location = new System.Drawing.Point(0, 62);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1366, 50);
            this.pnlToolbar.TabIndex = 101;
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(130)))));
            this.btnConnect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(180)))), ((int)(((byte)(220)))));
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.Location = new System.Drawing.Point(10, 10);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(148, 30);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "KẾT NỐI ESP32";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(130)))));
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(180)))), ((int)(((byte)(220)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(166, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 30);
            this.button1.TabIndex = 1;
            this.button1.Text = "LỊCH SỬ";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(130)))));
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(180)))), ((int)(((byte)(220)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(284, 10);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(130, 30);
            this.button2.TabIndex = 2;
            this.button2.Text = "QUẢN LÝ RFID";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // btnResetDb
            // 
            this.btnResetDb.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.btnResetDb.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnResetDb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetDb.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnResetDb.ForeColor = System.Drawing.Color.White;
            this.btnResetDb.Location = new System.Drawing.Point(422, 10);
            this.btnResetDb.Name = "btnResetDb";
            this.btnResetDb.Size = new System.Drawing.Size(148, 30);
            this.btnResetDb.TabIndex = 3;
            this.btnResetDb.Text = "RESET DATABASE";
            this.btnResetDb.UseVisualStyleBackColor = false;
            this.btnResetDb.Click += new System.EventHandler(this.btnResetDb_Click);
            // 
            // btnRestart
            // 
            this.btnRestart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(0)))));
            this.btnRestart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(135)))), ((int)(((byte)(0)))));
            this.btnRestart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRestart.ForeColor = System.Drawing.Color.White;
            this.btnRestart.Location = new System.Drawing.Point(578, 10);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(140, 30);
            this.btnRestart.TabIndex = 4;
            this.btnRestart.Text = "KHỞI ĐỘNG LẠI";
            this.btnRestart.UseVisualStyleBackColor = false;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(0)))), ((int)(((byte)(20)))));
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(20)))), ((int)(((byte)(60)))));
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Location = new System.Drawing.Point(1200, 10);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(155, 30);
            this.btnExit.TabIndex = 5;
            this.btnExit.Text = "THOÁT HỆ THỐNG";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // pnlEntryCard
            // 
            this.pnlEntryCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(42)))), ((int)(((byte)(59)))));
            this.pnlEntryCard.Controls.Add(this.txtPlateEntry);
            this.pnlEntryCard.Controls.Add(this.labelPlateEntry);
            this.pnlEntryCard.Controls.Add(this.pnlSep1);
            this.pnlEntryCard.Controls.Add(this.lblEntryZoom);
            this.pnlEntryCard.Controls.Add(this.trackBarEntryZoom);
            this.pnlEntryCard.Controls.Add(this.picEntry);
            this.pnlEntryCard.Controls.Add(this.label2);
            this.pnlEntryCard.Location = new System.Drawing.Point(10, 122);
            this.pnlEntryCard.Name = "pnlEntryCard";
            this.pnlEntryCard.Size = new System.Drawing.Size(450, 590);
            this.pnlEntryCard.TabIndex = 102;
            // 
            // txtPlateEntry
            // 
            this.txtPlateEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(14)))));
            this.txtPlateEntry.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPlateEntry.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.txtPlateEntry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.txtPlateEntry.Location = new System.Drawing.Point(10, 478);
            this.txtPlateEntry.Multiline = true;
            this.txtPlateEntry.Name = "txtPlateEntry";
            this.txtPlateEntry.Size = new System.Drawing.Size(430, 50);
            this.txtPlateEntry.TabIndex = 19;
            this.txtPlateEntry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelPlateEntry
            // 
            this.labelPlateEntry.BackColor = System.Drawing.Color.Transparent;
            this.labelPlateEntry.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelPlateEntry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(212)))), ((int)(((byte)(255)))));
            this.labelPlateEntry.Location = new System.Drawing.Point(0, 452);
            this.labelPlateEntry.Name = "labelPlateEntry";
            this.labelPlateEntry.Size = new System.Drawing.Size(450, 22);
            this.labelPlateEntry.TabIndex = 20;
            this.labelPlateEntry.Text = "BIỂN SỐ XE VÀO";
            this.labelPlateEntry.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlSep1
            // 
            this.pnlSep1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(200)))));
            this.pnlSep1.Location = new System.Drawing.Point(10, 442);
            this.pnlSep1.Name = "pnlSep1";
            this.pnlSep1.Size = new System.Drawing.Size(430, 2);
            this.pnlSep1.TabIndex = 200;
            // 
            // lblEntryZoom
            // 
            this.lblEntryZoom.BackColor = System.Drawing.Color.Transparent;
            this.lblEntryZoom.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblEntryZoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(155)))), ((int)(((byte)(185)))));
            this.lblEntryZoom.Location = new System.Drawing.Point(338, 398);
            this.lblEntryZoom.Name = "lblEntryZoom";
            this.lblEntryZoom.Size = new System.Drawing.Size(100, 25);
            this.lblEntryZoom.TabIndex = 201;
            this.lblEntryZoom.Text = "Zoom: 100%";
            this.lblEntryZoom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // trackBarEntryZoom
            // 
            this.trackBarEntryZoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(42)))), ((int)(((byte)(59)))));
            this.trackBarEntryZoom.Location = new System.Drawing.Point(10, 392);
            this.trackBarEntryZoom.Maximum = 300;
            this.trackBarEntryZoom.Minimum = 100;
            this.trackBarEntryZoom.Name = "trackBarEntryZoom";
            this.trackBarEntryZoom.Size = new System.Drawing.Size(320, 45);
            this.trackBarEntryZoom.TabIndex = 23;
            this.trackBarEntryZoom.TickFrequency = 25;
            this.trackBarEntryZoom.Value = 100;
            this.trackBarEntryZoom.Scroll += new System.EventHandler(this.trackBarEntryZoom_Scroll);
            // 
            // picEntry
            // 
            this.picEntry.BackColor = System.Drawing.Color.Black;
            this.picEntry.Location = new System.Drawing.Point(10, 44);
            this.picEntry.Name = "picEntry";
            this.picEntry.Size = new System.Drawing.Size(430, 340);
            this.picEntry.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picEntry.TabIndex = 0;
            this.picEntry.TabStop = false;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(34)))), ((int)(((byte)(52)))));
            this.label2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(212)))), ((int)(((byte)(255)))));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(450, 36);
            this.label2.TabIndex = 202;
            this.label2.Text = "   CAMERA XE VÀO";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // pnlStatsCard
            // 
            this.pnlStatsCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(42)))), ((int)(((byte)(59)))));
            this.pnlStatsCard.Controls.Add(this.txtLog);
            this.pnlStatsCard.Controls.Add(label4);
            this.pnlStatsCard.Controls.Add(this.pnlSep3);
            this.pnlStatsCard.Controls.Add(this.txtTotalExits);
            this.pnlStatsCard.Controls.Add(this.labelExits);
            this.pnlStatsCard.Controls.Add(this.txtTotalEntries);
            this.pnlStatsCard.Controls.Add(this.labelEntries);
            this.pnlStatsCard.Controls.Add(this.pnlSep2);
            this.pnlStatsCard.Controls.Add(this.txtTotalCars);
            this.pnlStatsCard.Controls.Add(label1);
            this.pnlStatsCard.Location = new System.Drawing.Point(470, 122);
            this.pnlStatsCard.Name = "pnlStatsCard";
            this.pnlStatsCard.Size = new System.Drawing.Size(420, 590);
            this.pnlStatsCard.TabIndex = 103;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(16)))), ((int)(((byte)(12)))));
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(220)))), ((int)(((byte)(120)))));
            this.txtLog.Location = new System.Drawing.Point(10, 362);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(400, 214);
            this.txtLog.TabIndex = 11;
            this.txtLog.Text = "";
            // 
            // pnlSep3
            // 
            this.pnlSep3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(200)))));
            this.pnlSep3.Location = new System.Drawing.Point(10, 328);
            this.pnlSep3.Name = "pnlSep3";
            this.pnlSep3.Size = new System.Drawing.Size(400, 2);
            this.pnlSep3.TabIndex = 202;
            // 
            // txtTotalExits
            // 
            this.txtTotalExits.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(30)))), ((int)(((byte)(44)))));
            this.txtTotalExits.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTotalExits.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.txtTotalExits.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(136)))));
            this.txtTotalExits.Location = new System.Drawing.Point(15, 280);
            this.txtTotalExits.Name = "txtTotalExits";
            this.txtTotalExits.ReadOnly = true;
            this.txtTotalExits.Size = new System.Drawing.Size(390, 34);
            this.txtTotalExits.TabIndex = 8;
            this.txtTotalExits.Text = "0";
            this.txtTotalExits.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelExits
            // 
            this.labelExits.BackColor = System.Drawing.Color.Transparent;
            this.labelExits.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelExits.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(136)))));
            this.labelExits.Location = new System.Drawing.Point(15, 256);
            this.labelExits.Name = "labelExits";
            this.labelExits.Size = new System.Drawing.Size(390, 22);
            this.labelExits.TabIndex = 16;
            this.labelExits.Text = "TỔNG LƯỢT XE RA";
            // 
            // txtTotalEntries
            // 
            this.txtTotalEntries.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(30)))), ((int)(((byte)(44)))));
            this.txtTotalEntries.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTotalEntries.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.txtTotalEntries.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.txtTotalEntries.Location = new System.Drawing.Point(15, 208);
            this.txtTotalEntries.Name = "txtTotalEntries";
            this.txtTotalEntries.ReadOnly = true;
            this.txtTotalEntries.Size = new System.Drawing.Size(390, 34);
            this.txtTotalEntries.TabIndex = 7;
            this.txtTotalEntries.Text = "0";
            this.txtTotalEntries.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelEntries
            // 
            this.labelEntries.BackColor = System.Drawing.Color.Transparent;
            this.labelEntries.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelEntries.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(212)))), ((int)(((byte)(255)))));
            this.labelEntries.Location = new System.Drawing.Point(15, 184);
            this.labelEntries.Name = "labelEntries";
            this.labelEntries.Size = new System.Drawing.Size(390, 22);
            this.labelEntries.TabIndex = 15;
            this.labelEntries.Text = "TỔNG LƯỢT XE VÀO";
            // 
            // pnlSep2
            // 
            this.pnlSep2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(200)))));
            this.pnlSep2.Location = new System.Drawing.Point(10, 174);
            this.pnlSep2.Name = "pnlSep2";
            this.pnlSep2.Size = new System.Drawing.Size(400, 2);
            this.pnlSep2.TabIndex = 201;
            // 
            // txtTotalCars
            // 
            this.txtTotalCars.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(30)))), ((int)(((byte)(44)))));
            this.txtTotalCars.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTotalCars.Font = new System.Drawing.Font("Segoe UI", 56F, System.Drawing.FontStyle.Bold);
            this.txtTotalCars.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(80)))), ((int)(((byte)(30)))));
            this.txtTotalCars.Location = new System.Drawing.Point(55, 44);
            this.txtTotalCars.Multiline = true;
            this.txtTotalCars.Name = "txtTotalCars";
            this.txtTotalCars.ReadOnly = true;
            this.txtTotalCars.Size = new System.Drawing.Size(310, 118);
            this.txtTotalCars.TabIndex = 6;
            this.txtTotalCars.Text = "0";
            this.txtTotalCars.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pnlExitCard
            // 
            this.pnlExitCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(42)))), ((int)(((byte)(59)))));
            this.pnlExitCard.Controls.Add(this.txtPlateExit);
            this.pnlExitCard.Controls.Add(this.labelPlateExit);
            this.pnlExitCard.Controls.Add(this.pnlSep4);
            this.pnlExitCard.Controls.Add(this.lblExitZoom);
            this.pnlExitCard.Controls.Add(this.trackBarExitZoom);
            this.pnlExitCard.Controls.Add(this.picExit);
            this.pnlExitCard.Controls.Add(this.label3);
            this.pnlExitCard.Location = new System.Drawing.Point(900, 122);
            this.pnlExitCard.Name = "pnlExitCard";
            this.pnlExitCard.Size = new System.Drawing.Size(450, 590);
            this.pnlExitCard.TabIndex = 104;
            // 
            // txtPlateExit
            // 
            this.txtPlateExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(14)))));
            this.txtPlateExit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPlateExit.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.txtPlateExit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.txtPlateExit.Location = new System.Drawing.Point(10, 478);
            this.txtPlateExit.Multiline = true;
            this.txtPlateExit.Name = "txtPlateExit";
            this.txtPlateExit.Size = new System.Drawing.Size(430, 50);
            this.txtPlateExit.TabIndex = 20;
            this.txtPlateExit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelPlateExit
            // 
            this.labelPlateExit.BackColor = System.Drawing.Color.Transparent;
            this.labelPlateExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelPlateExit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(136)))));
            this.labelPlateExit.Location = new System.Drawing.Point(0, 452);
            this.labelPlateExit.Name = "labelPlateExit";
            this.labelPlateExit.Size = new System.Drawing.Size(450, 22);
            this.labelPlateExit.TabIndex = 21;
            this.labelPlateExit.Text = "BIỂN SỐ XE RA";
            this.labelPlateExit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelPlateExit.Click += new System.EventHandler(this.labelPlateExit_Click);
            // 
            // pnlSep4
            // 
            this.pnlSep4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(200)))), ((int)(((byte)(100)))));
            this.pnlSep4.Location = new System.Drawing.Point(10, 442);
            this.pnlSep4.Name = "pnlSep4";
            this.pnlSep4.Size = new System.Drawing.Size(430, 2);
            this.pnlSep4.TabIndex = 203;
            // 
            // lblExitZoom
            // 
            this.lblExitZoom.BackColor = System.Drawing.Color.Transparent;
            this.lblExitZoom.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblExitZoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(155)))), ((int)(((byte)(185)))));
            this.lblExitZoom.Location = new System.Drawing.Point(338, 398);
            this.lblExitZoom.Name = "lblExitZoom";
            this.lblExitZoom.Size = new System.Drawing.Size(100, 25);
            this.lblExitZoom.TabIndex = 204;
            this.lblExitZoom.Text = "Zoom: 100%";
            this.lblExitZoom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // trackBarExitZoom
            // 
            this.trackBarExitZoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(42)))), ((int)(((byte)(59)))));
            this.trackBarExitZoom.Location = new System.Drawing.Point(10, 392);
            this.trackBarExitZoom.Maximum = 300;
            this.trackBarExitZoom.Minimum = 100;
            this.trackBarExitZoom.Name = "trackBarExitZoom";
            this.trackBarExitZoom.Size = new System.Drawing.Size(320, 45);
            this.trackBarExitZoom.TabIndex = 25;
            this.trackBarExitZoom.TickFrequency = 25;
            this.trackBarExitZoom.Value = 100;
            this.trackBarExitZoom.Scroll += new System.EventHandler(this.trackBarExitZoom_Scroll);
            // 
            // picExit
            // 
            this.picExit.BackColor = System.Drawing.Color.Black;
            this.picExit.Location = new System.Drawing.Point(10, 44);
            this.picExit.Name = "picExit";
            this.picExit.Size = new System.Drawing.Size(430, 340);
            this.picExit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picExit.TabIndex = 1;
            this.picExit.TabStop = false;
            this.picExit.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(44)))), ((int)(((byte)(34)))));
            this.label3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(136)))));
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(450, 36);
            this.label3.TabIndex = 205;
            this.label3.Text = "   CAMERA XE RA";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(27)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1391, 726);
            this.Controls.Add(this.pnlExitCard);
            this.Controls.Add(this.pnlStatsCard);
            this.Controls.Add(this.pnlEntryCard);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1370, 760);
            this.Name = "Form1";
            this.Text = "Hệ Thống Quản Lý Bãi Đỗ Xe";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlEntryCard.ResumeLayout(false);
            this.pnlEntryCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarEntryZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picEntry)).EndInit();
            this.pnlStatsCard.ResumeLayout(false);
            this.pnlStatsCard.PerformLayout();
            this.pnlExitCard.ResumeLayout(false);
            this.pnlExitCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarExitZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picExit)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel    pnlHeader;
        private System.Windows.Forms.Label    lblTitle;
        private System.Windows.Forms.Panel    pnlHeaderLine;
        private System.Windows.Forms.Panel    pnlToolbar;
        private System.Windows.Forms.Panel    pnlEntryCard;
        private System.Windows.Forms.Panel    pnlStatsCard;
        private System.Windows.Forms.Panel    pnlExitCard;
        private System.Windows.Forms.Panel    pnlSep1;
        private System.Windows.Forms.Panel    pnlSep2;
        private System.Windows.Forms.Panel    pnlSep3;
        private System.Windows.Forms.Panel    pnlSep4;
        private System.Windows.Forms.PictureBox picEntry;
        private System.Windows.Forms.PictureBox picExit;
        private System.Windows.Forms.TrackBar trackBarEntryZoom;
        private System.Windows.Forms.Label    lblEntryZoom;
        private System.Windows.Forms.TrackBar trackBarExitZoom;
        private System.Windows.Forms.Label    lblExitZoom;
        private System.Windows.Forms.Button   btnConnect;
        private System.Windows.Forms.Button   btnExit;
        private System.Windows.Forms.Button   btnResetDb;
        private System.Windows.Forms.Button   btnRestart;
        private System.Windows.Forms.Button   button1;
        private System.Windows.Forms.Button   button2;
        private System.Windows.Forms.TextBox  txtTotalCars;
        private System.Windows.Forms.TextBox  txtTotalEntries;
        private System.Windows.Forms.TextBox  txtTotalExits;
        private System.Windows.Forms.TextBox  txtPlateEntry;
        private System.Windows.Forms.TextBox  txtPlateExit;
        private System.Windows.Forms.Label    label2;
        private System.Windows.Forms.Label    label3;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Label    labelEntries;
        private System.Windows.Forms.Label    labelExits;
        private System.Windows.Forms.Label    labelPlateEntry;
        private System.Windows.Forms.Label    labelPlateExit;
    }
}
