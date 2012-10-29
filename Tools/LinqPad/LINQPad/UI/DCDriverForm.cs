namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    internal class DCDriverForm : BaseForm
    {
        private DCDriverList autoDrivers;
        private Button btnCancel;
        private Button btnInstall;
        private Button btnNext;
        private IContainer components = null;
        private DCDriverList manualDrivers;
        private TableLayoutPanel panInstall;
        private TableLayoutPanel panOKCancel;
        private RadioButton rbAuto;
        private RadioButton rbManual;

        public DCDriverForm()
        {
            this.InitializeComponent();
            this.PopulateDrivers();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            DCDriverLoader selectedDriverLoader = this.SelectedDriverLoader;
            using (BrowseDriversForm form = new BrowseDriversForm(false))
            {
                form.ShowDialog();
            }
            this.PopulateDrivers();
            this.SelectedDriverLoader = selectedDriverLoader;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DriverClicked(object sender, EventArgs e)
        {
            if (this.SelectedDriverLoader != null)
            {
                base.DialogResult = DialogResult.OK;
            }
        }

        private void DriverDeleted(object sender, EventArgs e)
        {
            DCDriverLoader.UnloadDomains(true);
            DCDriverLoader selectedDriverLoader = this.SelectedDriverLoader;
            if (selectedDriverLoader != null)
            {
                string directoryName = Path.GetDirectoryName(selectedDriverLoader.GetAssemblyPath());
                try
                {
                    Directory.Delete(directoryName, true);
                }
                catch (Exception exception)
                {
                    Log.Write(exception, "Delete driver");
                    MessageBox.Show("Driver is in use - cannot delete.", "LINQPad");
                    return;
                }
                this.PopulateDrivers();
            }
        }

        private void EnableControls()
        {
            this.autoDrivers.Enabled = this.rbAuto.Checked;
            this.manualDrivers.Enabled = this.rbManual.Checked;
        }

        private void InitializeComponent()
        {
            this.panOKCancel = new TableLayoutPanel();
            this.btnNext = new Button();
            this.btnCancel = new Button();
            this.rbAuto = new RadioButton();
            this.rbManual = new RadioButton();
            this.autoDrivers = new DCDriverList();
            this.manualDrivers = new DCDriverList();
            this.panInstall = new TableLayoutPanel();
            this.btnInstall = new Button();
            this.panOKCancel.SuspendLayout();
            this.panInstall.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 4;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnNext, 2, 0);
            this.panOKCancel.Controls.Add(this.btnCancel, 3, 0);
            this.panOKCancel.Dock = DockStyle.Top;
            this.panOKCancel.Location = new Point(8, 0x1c1);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 10, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(660, 0x2a);
            this.panOKCancel.TabIndex = 4;
            this.btnNext.DialogResult = DialogResult.OK;
            this.btnNext.Location = new Point(490, 13);
            this.btnNext.Margin = new Padding(3, 3, 3, 0);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new Size(0x52, 0x1d);
            this.btnNext.TabIndex = 1;
            this.btnNext.Text = "&Next >";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x242, 13);
            this.btnCancel.Margin = new Padding(3, 3, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x52, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.rbAuto.AutoSize = true;
            this.rbAuto.Checked = true;
            this.rbAuto.Dock = DockStyle.Top;
            this.rbAuto.Location = new Point(8, 8);
            this.rbAuto.Name = "rbAuto";
            this.rbAuto.Padding = new Padding(0, 0, 0, 2);
            this.rbAuto.Size = new Size(660, 0x19);
            this.rbAuto.TabIndex = 5;
            this.rbAuto.TabStop = true;
            this.rbAuto.Text = "&Build data context automatically";
            this.rbAuto.UseVisualStyleBackColor = true;
            this.rbAuto.Click += new EventHandler(this.rbAuto_Click);
            this.rbManual.AutoSize = true;
            this.rbManual.Dock = DockStyle.Top;
            this.rbManual.Location = new Point(8, 0xd3);
            this.rbManual.Name = "rbManual";
            this.rbManual.Padding = new Padding(0, 0, 0, 2);
            this.rbManual.Size = new Size(660, 0x19);
            this.rbManual.TabIndex = 1;
            this.rbManual.TabStop = true;
            this.rbManual.Text = "&Use a typed data context from your own assembly";
            this.rbManual.UseVisualStyleBackColor = true;
            this.rbManual.Click += new EventHandler(this.rbManual_Click);
            this.autoDrivers.Dock = DockStyle.Top;
            this.autoDrivers.Drivers = null;
            this.autoDrivers.Location = new Point(8, 0x21);
            this.autoDrivers.Name = "autoDrivers";
            this.autoDrivers.SelectedDriver = null;
            this.autoDrivers.Size = new Size(660, 0xb2);
            this.autoDrivers.TabIndex = 0;
            this.autoDrivers.DriverDeleted += new EventHandler(this.DriverDeleted);
            this.autoDrivers.ItemActivated += new EventHandler(this.DriverClicked);
            this.manualDrivers.Dock = DockStyle.Top;
            this.manualDrivers.Drivers = null;
            this.manualDrivers.Location = new Point(8, 0xec);
            this.manualDrivers.Name = "manualDrivers";
            this.manualDrivers.SelectedDriver = null;
            this.manualDrivers.Size = new Size(660, 0xb2);
            this.manualDrivers.TabIndex = 2;
            this.manualDrivers.DriverDeleted += new EventHandler(this.DriverDeleted);
            this.manualDrivers.ItemActivated += new EventHandler(this.DriverClicked);
            this.panInstall.AutoSize = true;
            this.panInstall.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panInstall.ColumnCount = 4;
            this.panInstall.ColumnStyles.Add(new ColumnStyle());
            this.panInstall.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panInstall.ColumnStyles.Add(new ColumnStyle());
            this.panInstall.ColumnStyles.Add(new ColumnStyle());
            this.panInstall.Controls.Add(this.btnInstall, 0, 0);
            this.panInstall.Dock = DockStyle.Top;
            this.panInstall.Location = new Point(8, 0x19e);
            this.panInstall.Name = "panInstall";
            this.panInstall.Padding = new Padding(0, 5, 0, 0);
            this.panInstall.RowCount = 1;
            this.panInstall.RowStyles.Add(new RowStyle());
            this.panInstall.Size = new Size(660, 0x23);
            this.panInstall.TabIndex = 3;
            this.btnInstall.Location = new Point(0, 5);
            this.btnInstall.Margin = new Padding(0);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new Size(170, 30);
            this.btnInstall.TabIndex = 0;
            this.btnInstall.Text = "View more drivers...";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new EventHandler(this.btnInstall_Click);
            base.AcceptButton = this.btnNext;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x2a4, 0x1ec);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.panInstall);
            base.Controls.Add(this.manualDrivers);
            base.Controls.Add(this.rbManual);
            base.Controls.Add(this.autoDrivers);
            base.Controls.Add(this.rbAuto);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Margin = new Padding(3, 4, 3, 4);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "DCDriverForm";
            base.Padding = new Padding(8);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Choose Data Context";
            this.panOKCancel.ResumeLayout(false);
            this.panInstall.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            base.ClientSize = new Size(base.ClientSize.Width, this.panOKCancel.Bottom + base.Padding.Bottom);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Point pt = base.PointToScreen(e.Location);
            if (!(this.manualDrivers.Enabled || !this.manualDrivers.RectangleToScreen(this.manualDrivers.ClientRectangle).Contains(pt)))
            {
                this.rbManual.Checked = true;
                this.autoDrivers.SelectedDriver = null;
                this.EnableControls();
                this.manualDrivers.SetListCapture();
            }
            else if (!(this.autoDrivers.Enabled || !this.autoDrivers.RectangleToScreen(this.autoDrivers.ClientRectangle).Contains(pt)))
            {
                this.rbAuto.Checked = true;
                this.manualDrivers.SelectedDriver = null;
                this.EnableControls();
                this.autoDrivers.SetListCapture();
            }
            base.OnMouseDown(e);
        }

        private void PopulateDrivers()
        {
            DCDriverInfo[] first = new DCDriverInfo[] { new DCDriverInfo(new LinqToSqlDynamicDriver()), new DCDriverInfo(new AstoriaDynamicDriver()), new DCDriverInfo(new DallasDynamicDriver()), new DCDriverInfo(new LinqToSqlDriver()), new DCDriverInfo(new EntityFrameworkDriver()), new DCDriverInfo(new EntityFrameworkDbContextDriver()) };
            first[0].Name = "Default (LINQ to SQL)";
            DCDriverInfo[] thirdPartyDrivers = ThirdPartyDriverProber.GetThirdPartyDrivers();
            IEnumerable<DCDriverInfo> enumerable = first.Concat<DCDriverInfo>(thirdPartyDrivers);
            this.autoDrivers.Drivers = (from d in enumerable
                where d.IsAuto
                select d).ToArray<DCDriverInfo>();
            this.manualDrivers.Drivers = (from d in enumerable
                where !d.IsAuto
                select d).ToArray<DCDriverInfo>();
            this.EnableControls();
        }

        private void rbAuto_Click(object sender, EventArgs e)
        {
            this.manualDrivers.SelectedDriver = null;
            this.EnableControls();
            this.autoDrivers.FocusList();
        }

        private void rbManual_Click(object sender, EventArgs e)
        {
            this.autoDrivers.SelectedDriver = null;
            this.EnableControls();
            this.manualDrivers.FocusList();
        }

        public DCDriverLoader SelectedDriverLoader
        {
            get
            {
                DCDriverInfo info = this.rbAuto.Checked ? this.autoDrivers.SelectedDriver : this.manualDrivers.SelectedDriver;
                return ((info == null) ? null : info.Loader);
            }
            set
            {
                DCDriverInfo info = this.autoDrivers.Drivers.FirstOrDefault<DCDriverInfo>(d => d.Loader.Equals(value));
                if (info != null)
                {
                    this.rbAuto.Checked = true;
                    this.EnableControls();
                    this.autoDrivers.SelectedDriver = info;
                }
                else
                {
                    DCDriverInfo info2 = this.manualDrivers.Drivers.FirstOrDefault<DCDriverInfo>(d => d.Loader.Equals(value));
                    if (info2 != null)
                    {
                        this.autoDrivers.SelectedDriver = null;
                        this.rbManual.Checked = true;
                        this.EnableControls();
                        this.manualDrivers.SelectedDriver = info2;
                    }
                    else
                    {
                        this.manualDrivers.SelectedDriver = null;
                    }
                }
            }
        }
    }
}

