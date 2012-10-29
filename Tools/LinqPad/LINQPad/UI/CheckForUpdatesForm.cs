namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class CheckForUpdatesForm : BaseForm
    {
        private Version _latestBeta;
        private int? _latestReleaseMajorMinor;
        private Button btnClose;
        private Button btnProxy;
        private IContainer components = null;
        private Label lblIntro;
        private Label lblMessage;
        private LinkLabel llBeta;
        private TableLayoutPanel panOKCancel;
        private Panel panSpacer;
        private BackgroundWorker updateWorker;

        public CheckForUpdatesForm()
        {
            this.InitializeComponent();
            this.lblIntro.Height = this.lblIntro.Font.Height * 6;
            this.updateWorker.RunWorkerAsync();
        }

        private void btnProxy_Click(object sender, EventArgs e)
        {
            using (ProxyForm form = new ProxyForm())
            {
                if (!((form.ShowDialog(this) != DialogResult.OK) || this.updateWorker.IsBusy))
                {
                    this.updateWorker.RunWorkerAsync();
                }
            }
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
            this.panOKCancel = new TableLayoutPanel();
            this.btnClose = new Button();
            this.btnProxy = new Button();
            this.lblIntro = new Label();
            this.updateWorker = new BackgroundWorker();
            this.lblMessage = new Label();
            this.panSpacer = new Panel();
            this.llBeta = new LinkLabel();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnClose, 2, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(7, 0x13b);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 3, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x158, 0x26);
            this.panOKCancel.TabIndex = 7;
            this.btnClose.DialogResult = DialogResult.Cancel;
            this.btnClose.Location = new Point(0x108, 6);
            this.btnClose.Margin = new Padding(3, 3, 1, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(0x4f, 0x1d);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnProxy.Dock = DockStyle.Top;
            this.btnProxy.Location = new Point(7, 0x7d);
            this.btnProxy.Name = "btnProxy";
            this.btnProxy.Size = new Size(0x158, 0x1d);
            this.btnProxy.TabIndex = 6;
            this.btnProxy.Text = "Specify Web Proxy Server...";
            this.btnProxy.UseVisualStyleBackColor = true;
            this.btnProxy.Click += new EventHandler(this.btnProxy_Click);
            this.lblIntro.Dock = DockStyle.Top;
            this.lblIntro.Location = new Point(7, 8);
            this.lblIntro.Name = "lblIntro";
            this.lblIntro.Size = new Size(0x158, 0x75);
            this.lblIntro.TabIndex = 8;
            this.lblIntro.Text = "LINQPad automatically downloads most release-version updates on startup. If you are behind an HTTP proxy that requires special configuration, click the button below.";
            this.updateWorker.DoWork += new DoWorkEventHandler(this.updateWorker_DoWork);
            this.updateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.updateWorker_RunWorkerCompleted);
            this.lblMessage.Dock = DockStyle.Top;
            this.lblMessage.ForeColor = Color.FromArgb(0, 0, 0xc0);
            this.lblMessage.Location = new Point(7, 0xb0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new Size(0x158, 0x4c);
            this.lblMessage.TabIndex = 8;
            this.lblMessage.Text = "Checking...";
            this.panSpacer.Dock = DockStyle.Top;
            this.panSpacer.Location = new Point(7, 0x9a);
            this.panSpacer.Name = "panSpacer";
            this.panSpacer.Size = new Size(0x158, 0x16);
            this.panSpacer.TabIndex = 9;
            this.llBeta.AutoSize = true;
            this.llBeta.Dock = DockStyle.Top;
            this.llBeta.Location = new Point(7, 0xfc);
            this.llBeta.Name = "llBeta";
            this.llBeta.Size = new Size(0xac, 0x13);
            this.llBeta.TabIndex = 10;
            this.llBeta.TabStop = true;
            this.llBeta.Text = "www.linqpad.net/beta.aspx";
            this.llBeta.Visible = false;
            this.llBeta.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llBeta_LinkClicked);
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x166, 0x169);
            base.ControlBox = false;
            base.Controls.Add(this.llBeta);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.lblMessage);
            base.Controls.Add(this.panSpacer);
            base.Controls.Add(this.btnProxy);
            base.Controls.Add(this.lblIntro);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "CheckForUpdatesForm";
            base.Padding = new Padding(7, 8, 7, 8);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Check for Updates";
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void llBeta_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebHelper.LaunchBrowser("http://www.linqpad.net/beta.aspx");
        }

        private void updateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this._latestReleaseMajorMinor = UpdateAgent.GetLatestVersion(false, null);
            if (this._latestReleaseMajorMinor.HasValue)
            {
                this._latestBeta = UpdateAgent.GetLatestBetaVersion();
            }
        }

        private void updateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!base.IsDisposed)
            {
                Version version = Program.Version;
                if (!this._latestReleaseMajorMinor.HasValue)
                {
                    this.lblMessage.Text = "Unable to communicate with the update server. Check your Internet connection and proxy settings.";
                    this.lblMessage.ForeColor = Color.Crimson;
                }
                else
                {
                    int? nullable = this._latestReleaseMajorMinor;
                    int majorMinorVersion = Program.MajorMinorVersion;
                    if ((nullable.GetValueOrDefault() > majorMinorVersion) && nullable.HasValue)
                    {
                        this.lblMessage.Text = "You have an old version. Restart LINQPad to download the latest update.";
                        this.lblMessage.ForeColor = Color.Crimson;
                    }
                    else if ((Program.MajorMinorVersion < 400) && (this._latestBeta == new Version(3, 0x63, 0, 0)))
                    {
                        this.lblMessage.Text = "A newer version of LINQPad targeting a later .NET Framework is available at www.linqpad.net.";
                        this.lblMessage.ForeColor = Color.Crimson;
                    }
                    else if ((this._latestBeta > version) && (((majorMinorVersion = Program.MajorMinorVersion) > (nullable = this._latestReleaseMajorMinor).GetValueOrDefault()) && nullable.HasValue))
                    {
                        this.lblMessage.Text = "A more recent beta is available for download:";
                        this.lblMessage.ForeColor = Color.Crimson;
                        this.llBeta.Visible = true;
                    }
                    else if (this._latestBeta > version)
                    {
                        this.lblMessage.Text = "You have the release latest version. A beta of the next version is available here:";
                        this.lblMessage.ForeColor = Color.Green;
                        this.llBeta.Visible = true;
                    }
                    else
                    {
                        this.lblMessage.Text = "You have the latest version, and your Internet connection is fine.";
                        this.lblMessage.ForeColor = Color.Green;
                    }
                }
            }
        }
    }
}

