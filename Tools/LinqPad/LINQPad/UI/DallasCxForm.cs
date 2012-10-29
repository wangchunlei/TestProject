namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Linq;

    internal class DallasCxForm : BaseForm
    {
        private IConnectionInfo _cxInfo;
        private string _lastKey;
        private volatile Thread _testCxThread;
        private volatile bool _testSuccessful;
        private Button btnCancel;
        private Button btnOK;
        private Button btnTest;
        private ComboBox cboServer;
        private CheckBox chkRemember;
        private IContainer components = null;
        private GroupBox groupBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label lblWarning;
        private LinkLabel llCopyLast;
        private LinkLabel llFindKey;
        private LinkLabel llMarketPlace;
        private LinkLabel llProxy;
        private LinkLabel llSignUp;
        private Panel panel1;
        private Panel panel4;
        private Panel panOKCancel;
        private FlowLayoutPanel panWarning;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel6;
        private TextBox txtDisplayName;
        private TextBox txtKey;

        public DallasCxForm(IConnectionInfo cxInfo)
        {
            this._cxInfo = cxInfo;
            this.InitializeComponent();
            this.cboServer.Text = this._cxInfo.DatabaseInfo.Server ?? "";
            this.txtDisplayName.Text = this._cxInfo.DisplayName;
            this.txtKey.Text = this._cxInfo.Decrypt((string) this._cxInfo.DriverData.Element("AccountKey"));
            if (this.txtKey.Text.Length == 0)
            {
                Repository repository = Repository.FromDisk().LastOrDefault<Repository>(r => r.DriverLoader.InternalID == "DallasAuto");
                if (repository != null)
                {
                    string str = (string) repository.DriverData.Element("AccountKey");
                    if (!string.IsNullOrEmpty(str))
                    {
                        this._lastKey = cxInfo.Decrypt(str);
                    }
                }
            }
            this.llCopyLast.Enabled = !string.IsNullOrEmpty(this._lastKey);
            string[] names = MRU.DallasUriNames.GetNames();
            if (this.cboServer.Text.Length == 0)
            {
                this.cboServer.Text = (names.Length == 0) ? "https://" : names[0];
                this.chkRemember.Checked = true;
            }
            this.cboServer.Items.AddRange(names);
            this.chkRemember.Checked = this._cxInfo.Persist;
            try
            {
                this.lblWarning.Font = new Font(this.Font, FontStyle.Bold);
            }
            catch
            {
            }
            this.panWarning.Dispose();
            this.EnableControls();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (this._testCxThread != null)
            {
                this._testCxThread = null;
            }
            else
            {
                Repository r = new Repository();
                this.UpdateRepository(r);
                Thread thread = new Thread(() => this.TestCx(r)) {
                    Name = "Cx Tester",
                    IsBackground = true
                };
                this._testCxThread = thread;
                this._testCxThread.Start();
            }
            this.EnableControls();
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
            bool flag = this._testCxThread != null;
            this.Text = flag ? "Testing connection..." : (this._testSuccessful ? "Connection Successful!" : "DataMarket Connection");
            this.btnTest.Text = flag ? "S&top" : "&Test";
            this.btnOK.Enabled = this.btnTest.Enabled = this.IsDataValid();
        }

        private void EnableControls(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void InitializeComponent()
        {
            this.panOKCancel = new Panel();
            this.btnTest = new Button();
            this.btnOK = new Button();
            this.panel4 = new Panel();
            this.btnCancel = new Button();
            this.panel1 = new Panel();
            this.chkRemember = new CheckBox();
            this.cboServer = new ComboBox();
            this.groupBox1 = new GroupBox();
            this.tableLayoutPanel6 = new TableLayoutPanel();
            this.llSignUp = new LinkLabel();
            this.label3 = new Label();
            this.tableLayoutPanel5 = new TableLayoutPanel();
            this.llFindKey = new LinkLabel();
            this.label2 = new Label();
            this.txtKey = new TextBox();
            this.tableLayoutPanel2 = new TableLayoutPanel();
            this.label1 = new Label();
            this.llCopyLast = new LinkLabel();
            this.tableLayoutPanel3 = new TableLayoutPanel();
            this.llProxy = new LinkLabel();
            this.label4 = new Label();
            this.tableLayoutPanel4 = new TableLayoutPanel();
            this.llMarketPlace = new LinkLabel();
            this.label5 = new Label();
            this.label6 = new Label();
            this.label7 = new Label();
            this.txtDisplayName = new TextBox();
            this.panWarning = new FlowLayoutPanel();
            this.lblWarning = new Label();
            this.panOKCancel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panWarning.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.Controls.Add(this.btnTest);
            this.panOKCancel.Controls.Add(this.btnOK);
            this.panOKCancel.Controls.Add(this.panel4);
            this.panOKCancel.Controls.Add(this.btnCancel);
            this.panOKCancel.Dock = DockStyle.Top;
            this.panOKCancel.Location = new Point(10, 410);
            this.panOKCancel.Margin = new Padding(4, 5, 4, 0);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 6, 0, 0);
            this.panOKCancel.Size = new Size(0x1dc, 0x23);
            this.panOKCancel.TabIndex = 6;
            this.btnTest.Dock = DockStyle.Left;
            this.btnTest.Location = new Point(0, 6);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new Size(0x55, 0x1d);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "&Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new EventHandler(this.btnTest_Click);
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Dock = DockStyle.Right;
            this.btnOK.Location = new Point(300, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x55, 0x1d);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.panel4.Dock = DockStyle.Right;
            this.panel4.Location = new Point(0x181, 6);
            this.panel4.Margin = new Padding(4, 5, 4, 5);
            this.panel4.Name = "panel4";
            this.panel4.Size = new Size(6, 0x1d);
            this.panel4.TabIndex = 2;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Dock = DockStyle.Right;
            this.btnCancel.Location = new Point(0x187, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x55, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.panel1.Controls.Add(this.chkRemember);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(10, 370);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x1dc, 40);
            this.panel1.TabIndex = 5;
            this.chkRemember.AutoSize = true;
            this.chkRemember.Dock = DockStyle.Right;
            this.chkRemember.Location = new Point(0x11d, 0);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Padding = new Padding(0, 10, 0, 0);
            this.chkRemember.Size = new Size(0xbf, 40);
            this.chkRemember.TabIndex = 7;
            this.chkRemember.Text = "&Remember this connection";
            this.chkRemember.UseVisualStyleBackColor = true;
            this.cboServer.Dock = DockStyle.Top;
            this.cboServer.Location = new Point(10, 0x20);
            this.cboServer.Name = "cboServer";
            this.cboServer.Size = new Size(0x1dc, 0x19);
            this.cboServer.TabIndex = 0;
            this.cboServer.Text = "https://";
            this.cboServer.TextChanged += new EventHandler(this.EnableControls);
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.tableLayoutPanel6);
            this.groupBox1.Controls.Add(this.tableLayoutPanel5);
            this.groupBox1.Controls.Add(this.txtKey);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Dock = DockStyle.Top;
            this.groupBox1.Location = new Point(10, 0x89);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new Padding(12, 5, 11, 7);
            this.groupBox1.Size = new Size(0x1dc, 0x7f);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Authentication";
            this.tableLayoutPanel6.AutoSize = true;
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel6.Controls.Add(this.llSignUp, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel6.Dock = DockStyle.Top;
            this.tableLayoutPanel6.Location = new Point(12, 0x62);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.Padding = new Padding(0, 0, 0, 3);
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel6.Size = new Size(0x1c5, 0x16);
            this.tableLayoutPanel6.TabIndex = 0x1d;
            this.llSignUp.Anchor = AnchorStyles.Left;
            this.llSignUp.AutoSize = true;
            this.llSignUp.Location = new Point(0x99, 0);
            this.llSignUp.Margin = new Padding(0);
            this.llSignUp.Name = "llSignUp";
            this.llSignUp.Size = new Size(0x86, 0x13);
            this.llSignUp.TabIndex = 0;
            this.llSignUp.TabStop = true;
            this.llSignUp.Text = "Click here to sign up";
            this.llSignUp.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llSignUp_LinkClicked);
            this.label3.Anchor = AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new Point(0, 0);
            this.label3.Margin = new Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x99, 0x13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Don't have an account?";
            this.tableLayoutPanel5.AutoSize = true;
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel5.Controls.Add(this.llFindKey, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel5.Dock = DockStyle.Top;
            this.tableLayoutPanel5.Location = new Point(12, 0x45);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.Padding = new Padding(0, 5, 0, 5);
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel5.Size = new Size(0x1c5, 0x1d);
            this.tableLayoutPanel5.TabIndex = 0x1c;
            this.llFindKey.Anchor = AnchorStyles.Left;
            this.llFindKey.AutoSize = true;
            this.llFindKey.Location = new Point(130, 5);
            this.llFindKey.Margin = new Padding(0);
            this.llFindKey.Name = "llFindKey";
            this.llFindKey.Size = new Size(0xac, 0x13);
            this.llFindKey.TabIndex = 0;
            this.llFindKey.TabStop = true;
            this.llFindKey.Text = "Click here to get your keys";
            this.llFindKey.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llFindKey_LinkClicked);
            this.label2.Anchor = AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0, 5);
            this.label2.Margin = new Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new Size(130, 0x13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Existing subscribers:";
            this.txtKey.Dock = DockStyle.Top;
            this.txtKey.Location = new Point(12, 0x2c);
            this.txtKey.Name = "txtKey";
            this.txtKey.PasswordChar = '*';
            this.txtKey.Size = new Size(0x1c5, 0x19);
            this.txtKey.TabIndex = 0;
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.llCopyLast, 1, 0);
            this.tableLayoutPanel2.Dock = DockStyle.Top;
            this.tableLayoutPanel2.Location = new Point(12, 0x17);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel2.Size = new Size(0x1c5, 0x15);
            this.tableLayoutPanel2.TabIndex = 0x1a;
            this.label1.Anchor = AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0, 0);
            this.label1.Margin = new Padding(0, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 2);
            this.label1.Size = new Size(0x58, 0x15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Account Key:";
            this.llCopyLast.Anchor = AnchorStyles.Right;
            this.llCopyLast.AutoSize = true;
            this.llCopyLast.Location = new Point(0x11b, 0);
            this.llCopyLast.Margin = new Padding(3, 0, 0, 0);
            this.llCopyLast.Name = "llCopyLast";
            this.llCopyLast.Padding = new Padding(0, 0, 0, 2);
            this.llCopyLast.Size = new Size(170, 0x15);
            this.llCopyLast.TabIndex = 13;
            this.llCopyLast.TabStop = true;
            this.llCopyLast.Text = "Copy from last connection";
            this.llCopyLast.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llCopyLast_LinkClicked);
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel3.Controls.Add(this.llProxy, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel3.Dock = DockStyle.Top;
            this.tableLayoutPanel3.Location = new Point(10, 11);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel3.Size = new Size(0x1dc, 0x15);
            this.tableLayoutPanel3.TabIndex = 7;
            this.llProxy.Anchor = AnchorStyles.Right;
            this.llProxy.AutoSize = true;
            this.llProxy.Location = new Point(0x161, 0);
            this.llProxy.Margin = new Padding(3, 0, 0, 0);
            this.llProxy.Name = "llProxy";
            this.llProxy.Padding = new Padding(0, 0, 0, 2);
            this.llProxy.Size = new Size(0x7b, 0x15);
            this.llProxy.TabIndex = 0;
            this.llProxy.TabStop = true;
            this.llProxy.Text = "Web Proxy Setup...";
            this.llProxy.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llProxy_LinkClicked);
            this.label4.Anchor = AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new Point(0, 0);
            this.label4.Margin = new Padding(0, 0, 3, 0);
            this.label4.Name = "label4";
            this.label4.Padding = new Padding(0, 0, 0, 2);
            this.label4.Size = new Size(0x9d, 0x15);
            this.label4.TabIndex = 0;
            this.label4.Text = "DataMarket Service URI:";
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel4.Controls.Add(this.llMarketPlace, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel4.Dock = DockStyle.Top;
            this.tableLayoutPanel4.Location = new Point(10, 0x39);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.Padding = new Padding(0, 13, 0, 10);
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel4.Size = new Size(0x1dc, 80);
            this.tableLayoutPanel4.TabIndex = 1;
            this.llMarketPlace.Anchor = AnchorStyles.Top;
            this.llMarketPlace.AutoSize = true;
            this.llMarketPlace.Location = new Point(0x91, 0x33);
            this.llMarketPlace.Margin = new Padding(0);
            this.llMarketPlace.Name = "llMarketPlace";
            this.llMarketPlace.Size = new Size(0xb9, 0x13);
            this.llMarketPlace.TabIndex = 20;
            this.llMarketPlace.TabStop = true;
            this.llMarketPlace.Text = "Windows Azure DataMarket ";
            this.llMarketPlace.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llMarketplace_LinkClicked);
            this.label5.Anchor = AnchorStyles.Top;
            this.label5.AutoSize = true;
            this.label5.Location = new Point(0x33, 13);
            this.label5.Margin = new Padding(0);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x175, 0x13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Not subscribed to Windows DataMarket? Discover a variety";
            this.label5.TextAlign = ContentAlignment.TopCenter;
            this.label6.Anchor = AnchorStyles.Top;
            this.label6.AutoSize = true;
            this.label6.Location = new Point(0x70, 0x20);
            this.label6.Margin = new Padding(0);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0xfb, 0x13);
            this.label6.TabIndex = 3;
            this.label6.Text = "of free and commercial data sets on the";
            this.label6.TextAlign = ContentAlignment.TopCenter;
            this.label7.AutoSize = true;
            this.label7.Dock = DockStyle.Top;
            this.label7.Location = new Point(10, 0x108);
            this.label7.Name = "label7";
            this.label7.Padding = new Padding(0, 12, 0, 2);
            this.label7.Size = new Size(0xfc, 0x21);
            this.label7.TabIndex = 3;
            this.label7.Text = "Friendly name for connection (optional):";
            this.txtDisplayName.Dock = DockStyle.Top;
            this.txtDisplayName.Location = new Point(10, 0x129);
            this.txtDisplayName.Name = "txtDisplayName";
            this.txtDisplayName.Size = new Size(0x1dc, 0x19);
            this.txtDisplayName.TabIndex = 4;
            this.panWarning.AutoSize = true;
            this.panWarning.Controls.Add(this.lblWarning);
            this.panWarning.Dock = DockStyle.Top;
            this.panWarning.Location = new Point(10, 0x142);
            this.panWarning.Name = "panWarning";
            this.panWarning.Padding = new Padding(0, 10, 0, 0);
            this.panWarning.Size = new Size(0x1dc, 0x30);
            this.panWarning.TabIndex = 9;
            this.lblWarning.AutoSize = true;
            this.lblWarning.Location = new Point(3, 10);
            this.lblWarning.Margin = new Padding(3, 0, 7, 0);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new Size(0x1bd, 0x26);
            this.lblWarning.TabIndex = 9;
            this.lblWarning.Text = "Note: DataMarket + OData work better in LINQPad for Framework 4.0.\r\nYou can download LINQPad 4.x at www.linqpad.net.";
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x1f0, 0x209);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.panel1);
            base.Controls.Add(this.panWarning);
            base.Controls.Add(this.txtDisplayName);
            base.Controls.Add(this.label7);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.tableLayoutPanel4);
            base.Controls.Add(this.cboServer);
            base.Controls.Add(this.tableLayoutPanel3);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "DallasCxForm";
            base.Padding = new Padding(10, 11, 10, 11);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "DataMarket Connection";
            this.panOKCancel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.panWarning.ResumeLayout(false);
            this.panWarning.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private bool IsDataValid()
        {
            return (this.cboServer.Text.Trim().Length > "https://".Length);
        }

        private void llCopyLast_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.txtKey.Text = this._lastKey;
        }

        private void llFindKey_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebHelper.LaunchBrowser("https://datamarket.azure.com/account/keys");
        }

        private void llMarketplace_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebHelper.LaunchBrowser("https://datamarket.azure.com");
        }

        private void llProxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (ProxyForm form = new ProxyForm())
            {
                form.ShowDialog();
            }
        }

        private void llSignUp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebHelper.LaunchBrowser("https://datamarket.azure.com/register");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!((base.DialogResult != DialogResult.OK) || this.IsDataValid()))
            {
                base.DialogResult = DialogResult.Cancel;
            }
            if (base.DialogResult == DialogResult.OK)
            {
                this.UpdateRepository(this._cxInfo);
                MRU.DallasUriNames.RegisterUse(this._cxInfo.DatabaseInfo.Server);
            }
            base.OnClosing(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if ((this.btnOK != null) && (this.btnOK.Parent != null))
            {
                int height = (this.btnOK.Bottom + this.btnOK.Parent.Top) + base.Padding.Bottom;
                if (base.ClientSize.Height != height)
                {
                    base.ClientSize = new Size(base.ClientSize.Width, height);
                }
            }
        }

        private void TestCx(Repository r)
        {
            try
            {
                this._testSuccessful = false;
                Stopwatch stopwatch = Stopwatch.StartNew();
                string text = TestDallas(r);
                if (((this._testCxThread == Thread.CurrentThread) && !base.IsDisposed) && base.Visible)
                {
                    this._testCxThread = null;
                    this._testSuccessful = text == null;
                    base.Invoke(new MethodInvoker(this.EnableControls));
                    if (text != null)
                    {
                        MessageBox.Show(text, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        if (stopwatch.ElapsedMilliseconds < 300L)
                        {
                            Thread.Sleep(300);
                        }
                        MessageBox.Show("Connection Successful.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
            catch (Exception exception)
            {
                Program.ProcessException(exception);
            }
        }

        private static string TestDallas(Repository r)
        {
            try
            {
                return AstoriaHelper.TestConnection(r);
            }
            catch (Exception exception)
            {
                Log.Write(exception);
                return exception.Message;
            }
        }

        private void UpdateRepository(IConnectionInfo cx)
        {
            cx.DatabaseInfo.Server = this.cboServer.Text.Trim();
            cx.DisplayName = this.txtDisplayName.Text.Trim();
            XElement element = cx.DriverData.Element("AccountKey");
            if (element != null)
            {
                element.Remove();
            }
            if (this.txtKey.Text.Trim().Length > 0)
            {
                cx.DriverData.SetElementValue("AccountKey", cx.Encrypt(this.txtKey.Text.Trim()));
            }
            cx.Persist = this.chkRemember.Checked;
        }
    }
}

