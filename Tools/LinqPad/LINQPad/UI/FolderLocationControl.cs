namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class FolderLocationControl : UserControl
    {
        private string _defaultLocationText = "";
        private LINQPad.MRU _mru;
        private ComboBox cboCustomLocation;
        private IContainer components = null;
        private LinkLabel llBrowse;
        private TableLayoutPanel panMain;
        private RadioButton rbCustomLocation;
        private RadioButton rbDefaultLocation;

        public FolderLocationControl()
        {
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panMain = new TableLayoutPanel();
            this.cboCustomLocation = new ComboBox();
            this.rbDefaultLocation = new RadioButton();
            this.rbCustomLocation = new RadioButton();
            this.llBrowse = new LinkLabel();
            this.panMain.SuspendLayout();
            base.SuspendLayout();
            this.panMain.AutoSize = true;
            this.panMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panMain.ColumnCount = 2;
            this.panMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panMain.ColumnStyles.Add(new ColumnStyle());
            this.panMain.Controls.Add(this.cboCustomLocation, 0, 2);
            this.panMain.Controls.Add(this.rbDefaultLocation, 0, 0);
            this.panMain.Controls.Add(this.rbCustomLocation, 0, 1);
            this.panMain.Controls.Add(this.llBrowse, 1, 1);
            this.panMain.Dock = DockStyle.Top;
            this.panMain.Location = new Point(0, 0);
            this.panMain.Name = "panMain";
            this.panMain.RowCount = 3;
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.Size = new Size(0x1d8, 70);
            this.panMain.TabIndex = 0;
            this.cboCustomLocation.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this.panMain.SetColumnSpan(this.cboCustomLocation, 2);
            this.cboCustomLocation.Enabled = false;
            this.cboCustomLocation.Location = new Point(1, 0x2e);
            this.cboCustomLocation.Margin = new Padding(1, 3, 1, 0);
            this.cboCustomLocation.Name = "cboCustomLocation";
            this.cboCustomLocation.Size = new Size(470, 0x18);
            this.cboCustomLocation.TabIndex = 3;
            this.rbDefaultLocation.AutoSize = true;
            this.rbDefaultLocation.Checked = true;
            this.panMain.SetColumnSpan(this.rbDefaultLocation, 2);
            this.rbDefaultLocation.Location = new Point(0, 0);
            this.rbDefaultLocation.Margin = new Padding(0, 0, 3, 0);
            this.rbDefaultLocation.Name = "rbDefaultLocation";
            this.rbDefaultLocation.Size = new Size(0x84, 0x15);
            this.rbDefaultLocation.TabIndex = 0;
            this.rbDefaultLocation.TabStop = true;
            this.rbDefaultLocation.Text = "Default Location";
            this.rbDefaultLocation.UseVisualStyleBackColor = true;
            this.rbCustomLocation.AutoSize = true;
            this.rbCustomLocation.Location = new Point(0, 0x16);
            this.rbCustomLocation.Margin = new Padding(0, 1, 3, 0);
            this.rbCustomLocation.Name = "rbCustomLocation";
            this.rbCustomLocation.Size = new Size(0x86, 0x15);
            this.rbCustomLocation.TabIndex = 1;
            this.rbCustomLocation.Text = "Custom Location";
            this.rbCustomLocation.UseVisualStyleBackColor = true;
            this.rbCustomLocation.CheckedChanged += new EventHandler(this.rbCustomLocation_CheckedChanged);
            this.llBrowse.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.llBrowse.AutoSize = true;
            this.llBrowse.Enabled = false;
            this.llBrowse.Location = new Point(0x196, 0x18);
            this.llBrowse.Margin = new Padding(3, 0, 0, 2);
            this.llBrowse.Name = "llBrowse";
            this.llBrowse.Size = new Size(0x42, 0x11);
            this.llBrowse.TabIndex = 2;
            this.llBrowse.TabStop = true;
            this.llBrowse.Text = "Browse...";
            this.llBrowse.TextAlign = ContentAlignment.BottomRight;
            this.llBrowse.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llBrowse_LinkClicked);
            base.AutoScaleDimensions = new SizeF(8f, 16f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.Controls.Add(this.panMain);
            base.Name = "FolderLocationControl";
            base.Size = new Size(0x1d8, 0x53);
            this.panMain.ResumeLayout(false);
            this.panMain.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void llBrowse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (this.cboCustomLocation.Text.Trim().Length > 0)
                {
                    dialog.SelectedPath = this.cboCustomLocation.Text.Trim();
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.cboCustomLocation.Text = dialog.SelectedPath;
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (base.Height != this.panMain.Height)
            {
                base.Height = this.panMain.Height;
            }
            base.OnLayout(e);
        }

        private void rbCustomLocation_CheckedChanged(object sender, EventArgs e)
        {
            this.cboCustomLocation.Enabled = this.llBrowse.Enabled = this.rbCustomLocation.Checked;
        }

        public void SaveMRU()
        {
            string name = this.cboCustomLocation.Text.Trim();
            if ((this._mru != null) && (name.Length > 0))
            {
                this._mru.RegisterUse(name);
            }
        }

        public string DefaultLocationText
        {
            get
            {
                return this._defaultLocationText;
            }
            set
            {
                this.rbDefaultLocation.Text = "Default Location (" + (this._defaultLocationText = value) + ")";
            }
        }

        public string FolderText
        {
            get
            {
                return (this.rbDefaultLocation.Checked ? null : this.cboCustomLocation.Text.Trim());
            }
            set
            {
                this.cboCustomLocation.Text = value ?? "";
                (string.IsNullOrEmpty(value) ? this.rbDefaultLocation : this.rbCustomLocation).Checked = true;
            }
        }

        public bool IsDefaultLocation
        {
            get
            {
                return this.rbDefaultLocation.Checked;
            }
        }

        public LINQPad.MRU MRU
        {
            get
            {
                return this._mru;
            }
            set
            {
                if (this._mru != value)
                {
                    this._mru = value;
                    this.cboCustomLocation.Items.Clear();
                    this.cboCustomLocation.Items.AddRange(this._mru.ReadNames().ToArray());
                }
            }
        }
    }
}

