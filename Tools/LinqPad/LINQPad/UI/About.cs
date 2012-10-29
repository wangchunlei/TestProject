namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;

    internal class About : BaseForm
    {
        private WebBrowser browser;
        private Button btnClose;
        private IContainer components = null;
        private Label label1;
        private Label lblLicensee;
        private Label lblTitle;
        private Label lblVersion;
        public static string LicenseeMsg;
        private LinkLabel llCheckForUpdates;
        private LinkLabel llWebSite;
        private Panel panBrowser;
        private Panel panLeft;
        private TableLayoutPanel panMain;
        private PictureBox pictureBox;

        public About()
        {
            this.InitializeComponent();
            try
            {
                this.lblTitle.Font = new Font("Tahoma", 14f, FontStyle.Bold);
                try
                {
                    this.llWebSite.Font = new Font(this.llCheckForUpdates.Font.FontFamily, 13f, FontStyle.Bold);
                }
                catch
                {
                }
                try
                {
                    this.llCheckForUpdates.Font = new Font(this.llCheckForUpdates.Font.FontFamily, 10f, FontStyle.Bold);
                }
                catch
                {
                }
                this.llWebSite.UseCompatibleTextRendering = true;
                this.llCheckForUpdates.UseCompatibleTextRendering = true;
            }
            catch
            {
            }
            this.pictureBox.Cursor = Cursors.Hand;
            this.pictureBox.Click += (sender, e) => WebHelper.LaunchBrowser("http://www.linqpad.net");
            if (string.IsNullOrEmpty(LicenseeMsg))
            {
                this.lblLicensee.Dispose();
            }
            else
            {
                this.lblLicensee.Text = LicenseeMsg;
            }
            this.lblVersion.Text = "v" + Program.VersionString;
            base.Icon = Resources.LINQPad;
            this.lblTitle.Text = this.lblTitle.Text + (!MainForm.Instance.ShowLicensee ? " (Free Edition)" : (MainForm.Instance.IsPremium ? " (Premium Edition)" : " (Pro Edition)"));
            this.browser.DocumentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LINQPad.UI.AboutPage.html");
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
            this.btnClose = new Button();
            this.panMain = new TableLayoutPanel();
            this.lblTitle = new Label();
            this.lblVersion = new Label();
            this.label1 = new Label();
            this.panBrowser = new Panel();
            this.browser = new WebBrowser();
            this.lblLicensee = new Label();
            this.llWebSite = new LinkLabel();
            this.pictureBox = new PictureBox();
            this.panLeft = new Panel();
            this.llCheckForUpdates = new LinkLabel();
            this.panMain.SuspendLayout();
            this.panBrowser.SuspendLayout();
            ((ISupportInitialize) this.pictureBox).BeginInit();
            this.panLeft.SuspendLayout();
            base.SuspendLayout();
            this.btnClose.Anchor = AnchorStyles.Top;
            this.btnClose.DialogResult = DialogResult.OK;
            this.btnClose.Location = new Point(230, 0x1f8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(0x4e, 0x1d);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.panMain.AutoSize = true;
            this.panMain.ColumnCount = 1;
            this.panMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panMain.Controls.Add(this.btnClose, 0, 10);
            this.panMain.Controls.Add(this.lblTitle, 0, 0);
            this.panMain.Controls.Add(this.lblVersion, 0, 1);
            this.panMain.Controls.Add(this.label1, 0, 2);
            this.panMain.Controls.Add(this.panBrowser, 0, 4);
            this.panMain.Controls.Add(this.lblLicensee, 0, 3);
            this.panMain.Dock = DockStyle.Top;
            this.panMain.Location = new Point(0x115, 0x15);
            this.panMain.Name = "panMain";
            this.panMain.Padding = new Padding(0x12, 0, 0, 0);
            this.panMain.RowCount = 12;
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.RowStyles.Add(new RowStyle());
            this.panMain.Size = new Size(0x209, 0x218);
            this.panMain.TabIndex = 2;
            this.lblTitle.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new Point(0x15, 0);
            this.lblTitle.Margin = new Padding(3, 0, 3, 5);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(0x1f1, 0x13);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "LINQPad";
            this.lblTitle.TextAlign = ContentAlignment.TopCenter;
            this.lblVersion.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new Point(0x15, 0x18);
            this.lblVersion.Margin = new Padding(3, 0, 3, 5);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new Size(0x1f1, 0x13);
            this.lblVersion.TabIndex = 5;
            this.lblVersion.Text = "v";
            this.lblVersion.TextAlign = ContentAlignment.TopCenter;
            this.lblVersion.DoubleClick += new EventHandler(this.lblVersion_DoubleClick);
            this.label1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0x15, 0x30);
            this.label1.Margin = new Padding(3, 0, 3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x1f1, 0x13);
            this.label1.TabIndex = 6;
            this.label1.Text = "\x00a9 2007-2012 Joseph Albahari";
            this.label1.TextAlign = ContentAlignment.MiddleCenter;
            this.panBrowser.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panBrowser.BorderStyle = BorderStyle.FixedSingle;
            this.panBrowser.Controls.Add(this.browser);
            this.panBrowser.Location = new Point(0x15, 0x67);
            this.panBrowser.Name = "panBrowser";
            this.panBrowser.Size = new Size(0x1f1, 0x18b);
            this.panBrowser.TabIndex = 7;
            this.browser.AllowNavigation = false;
            this.browser.Dock = DockStyle.Fill;
            this.browser.Location = new Point(0, 0);
            this.browser.MinimumSize = new Size(0x12, 0x15);
            this.browser.Name = "browser";
            this.browser.Size = new Size(0x1ef, 0x189);
            this.browser.TabIndex = 4;
            this.browser.WebBrowserShortcutsEnabled = false;
            this.lblLicensee.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.lblLicensee.AutoSize = true;
            this.lblLicensee.ForeColor = Color.Blue;
            this.lblLicensee.Location = new Point(0x15, 0x4c);
            this.lblLicensee.Margin = new Padding(3, 4, 3, 5);
            this.lblLicensee.Name = "lblLicensee";
            this.lblLicensee.Size = new Size(0x1f1, 0x13);
            this.lblLicensee.TabIndex = 8;
            this.lblLicensee.TextAlign = ContentAlignment.TopCenter;
            this.llWebSite.AutoSize = true;
            this.llWebSite.Dock = DockStyle.Bottom;
            this.llWebSite.Location = new Point(0, 0x1f6);
            this.llWebSite.Margin = new Padding(3, 0x10, 3, 0x10);
            this.llWebSite.Name = "llWebSite";
            this.llWebSite.Padding = new Padding(0, 0, 0, 15);
            this.llWebSite.Size = new Size(110, 0x22);
            this.llWebSite.TabIndex = 3;
            this.llWebSite.TabStop = true;
            this.llWebSite.Text = "www.linqpad.net";
            this.llWebSite.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llWebSite_LinkClicked);
            this.pictureBox.Image = Resources.LINQPadLarge;
            this.pictureBox.Location = new Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new Size(0x100, 0x100);
            this.pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            this.pictureBox.TabIndex = 3;
            this.pictureBox.TabStop = false;
            this.panLeft.AutoSize = true;
            this.panLeft.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panLeft.Controls.Add(this.pictureBox);
            this.panLeft.Controls.Add(this.llWebSite);
            this.panLeft.Controls.Add(this.llCheckForUpdates);
            this.panLeft.Dock = DockStyle.Left;
            this.panLeft.Location = new Point(0x12, 0x15);
            this.panLeft.Name = "panLeft";
            this.panLeft.Size = new Size(0x103, 0x22b);
            this.panLeft.TabIndex = 4;
            this.llCheckForUpdates.AutoSize = true;
            this.llCheckForUpdates.Dock = DockStyle.Bottom;
            this.llCheckForUpdates.Location = new Point(0, 0x218);
            this.llCheckForUpdates.Name = "llCheckForUpdates";
            this.llCheckForUpdates.Size = new Size(120, 0x13);
            this.llCheckForUpdates.TabIndex = 4;
            this.llCheckForUpdates.TabStop = true;
            this.llCheckForUpdates.Text = "Check for updates";
            this.llCheckForUpdates.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llCheckForUpdates_LinkClicked);
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = SystemColors.ControlLight;
            base.ClientSize = new Size(0x330, 0x255);
            base.Controls.Add(this.panMain);
            base.Controls.Add(this.panLeft);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "About";
            base.Padding = new Padding(0x12, 0x15, 0x12, 0x15);
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "About";
            this.panMain.ResumeLayout(false);
            this.panMain.PerformLayout();
            this.panBrowser.ResumeLayout(false);
            ((ISupportInitialize) this.pictureBox).EndInit();
            this.panLeft.ResumeLayout(false);
            this.panLeft.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void lblVersion_DoubleClick(object sender, EventArgs e)
        {
            Log.OpenLogFile();
        }

        private void llCheckForUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (CheckForUpdatesForm form = new CheckForUpdatesForm())
            {
                form.ShowDialog(this);
            }
        }

        private void llWebSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebHelper.LaunchBrowser("http://www.linqpad.net");
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if ((this.btnClose != null) && (this.panMain != null))
            {
                int height = (this.btnClose.Bottom + this.panMain.Top) + 10;
                if (base.ClientSize.Height != height)
                {
                    base.ClientSize = new Size(base.ClientSize.Width, height);
                }
                base.OnLayout(e);
            }
        }
    }
}

