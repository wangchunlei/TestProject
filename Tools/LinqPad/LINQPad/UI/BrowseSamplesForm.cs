namespace LINQPad.UI
{
    using LINQPad.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    internal class BrowseSamplesForm : BaseForm
    {
        private string _libraryName;
        private Button btnCancel;
        private Button btnOK;
        private IContainer components = null;
        private GroupBox groupBox1;
        private Label label1;
        private Label lblSpacer;
        private LinkLabel llBrowse;
        private Panel panBrowser;
        private TableLayoutPanel panOKCancel;
        private TextBox txtSampleURI;
        private WebBrowser webBrowser;

        public BrowseSamplesForm()
        {
            this.InitializeComponent();
            this.webBrowser.DocumentStream = new MemoryStream(Encoding.UTF8.GetBytes("Connecting..."));
            this.webBrowser.Navigating += new WebBrowserNavigatingEventHandler(this.webBrowser_Navigating);
            this.EnableControls();
            base.Icon = Resources.LINQPad;
            Timer tmr = new Timer();
            tmr.Tick += delegate (object sender, EventArgs e) {
                if (!this.IsDisposed)
                {
                    this.webBrowser.Navigate("http://www.linqpad.net/RichClient/SampleLibraries.aspx");
                }
                tmr.Dispose();
            };
            tmr.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void EnableControls()
        {
            this.btnOK.Enabled = this.txtSampleURI.Text.Trim().Length > 0;
        }

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.groupBox1 = new GroupBox();
            this.txtSampleURI = new TextBox();
            this.panOKCancel = new TableLayoutPanel();
            this.btnCancel = new Button();
            this.btnOK = new Button();
            this.lblSpacer = new Label();
            this.panBrowser = new Panel();
            this.webBrowser = new WebBrowser();
            this.llBrowse = new LinkLabel();
            this.groupBox1.SuspendLayout();
            this.panOKCancel.SuspendLayout();
            this.panBrowser.SuspendLayout();
            base.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(8, 6);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 2);
            this.label1.Size = new Size(0xdd, 0x15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose from the featured libraries:";
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.llBrowse);
            this.groupBox1.Controls.Add(this.txtSampleURI);
            this.groupBox1.Dock = DockStyle.Bottom;
            this.groupBox1.Location = new Point(8, 0x24b);
            this.groupBox1.Margin = new Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new Padding(7, 4, 7, 2);
            this.groupBox1.Size = new Size(0x33a, 0x44);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Or, enter the URI (or file path) of a LINQPad Samples Library:";
            this.txtSampleURI.Dock = DockStyle.Top;
            this.txtSampleURI.Location = new Point(7, 0x16);
            this.txtSampleURI.Margin = new Padding(3, 4, 3, 4);
            this.txtSampleURI.Name = "txtSampleURI";
            this.txtSampleURI.Size = new Size(0x32c, 0x19);
            this.txtSampleURI.TabIndex = 1;
            this.txtSampleURI.TextChanged += new EventHandler(this.txtSampleURI_TextChanged);
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnCancel, 2, 0);
            this.panOKCancel.Controls.Add(this.btnOK, 1, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(8, 0x28f);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 4, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x33a, 0x27);
            this.panOKCancel.TabIndex = 3;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x2ea, 7);
            this.btnCancel.Margin = new Padding(3, 3, 1, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0x295, 7);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 0x1d);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.lblSpacer.Dock = DockStyle.Bottom;
            this.lblSpacer.Location = new Point(8, 0x243);
            this.lblSpacer.Name = "lblSpacer";
            this.lblSpacer.Size = new Size(0x33a, 8);
            this.lblSpacer.TabIndex = 4;
            this.panBrowser.BorderStyle = BorderStyle.FixedSingle;
            this.panBrowser.Controls.Add(this.webBrowser);
            this.panBrowser.Dock = DockStyle.Fill;
            this.panBrowser.Location = new Point(8, 0x1b);
            this.panBrowser.Name = "panBrowser";
            this.panBrowser.Size = new Size(0x33a, 0x228);
            this.panBrowser.TabIndex = 5;
            this.webBrowser.Dock = DockStyle.Fill;
            this.webBrowser.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser.Location = new Point(0, 0);
            this.webBrowser.MinimumSize = new Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScriptErrorsSuppressed = true;
            this.webBrowser.Size = new Size(0x338, 550);
            this.webBrowser.TabIndex = 0;
            this.webBrowser.WebBrowserShortcutsEnabled = false;
            this.llBrowse.AutoSize = true;
            this.llBrowse.Dock = DockStyle.Top;
            this.llBrowse.Location = new Point(7, 0x2f);
            this.llBrowse.Name = "llBrowse";
            this.llBrowse.Size = new Size(0x3e, 0x13);
            this.llBrowse.TabIndex = 2;
            this.llBrowse.TabStop = true;
            this.llBrowse.Text = "&Browse...";
            this.llBrowse.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llBrowse_LinkClicked);
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x34a, 0x2bf);
            base.ControlBox = false;
            base.Controls.Add(this.panBrowser);
            base.Controls.Add(this.lblSpacer);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.label1);
            base.Location = new Point(0, 0);
            base.Margin = new Padding(3, 4, 3, 4);
            base.Name = "BrowseSamplesForm";
            base.Padding = new Padding(8, 6, 8, 9);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Browse Sample Libraries";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panOKCancel.ResumeLayout(false);
            this.panBrowser.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void llBrowse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = null;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Browse Sample Library";
                dialog.DefaultExt = ".zip";
                dialog.Filter = "LINQPad Sample Library|*.zip";
                if (!((dialog.ShowDialog(this) == DialogResult.OK) && File.Exists(dialog.FileName)))
                {
                    return;
                }
                path = dialog.FileName;
            }
            if (File.Exists(path))
            {
                this.txtSampleURI.Text = path;
                base.DialogResult = DialogResult.OK;
            }
        }

        private void txtSampleURI_TextChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string source = e.Url.ToString();
            if (source.ToLowerInvariant().Contains(".zip"))
            {
                if (source.Contains<char>('#'))
                {
                    string[] strArray = source.Split(new char[] { '#' });
                    this._libraryName = strArray[0];
                    this.txtSampleURI.Text = strArray[1];
                }
                else
                {
                    this.txtSampleURI.Text = source;
                }
                e.Cancel = true;
                base.DialogResult = DialogResult.OK;
            }
        }

        public string LibraryName
        {
            get
            {
                return this._libraryName;
            }
        }

        public string SamplesUri
        {
            get
            {
                return this.txtSampleURI.Text.Trim();
            }
        }
    }
}

