namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Net;
    using System.Text;
    using System.Windows.Forms;

    internal class ProxyForm : BaseForm
    {
        private BackgroundWorker _currentWorker;
        private string _fontPath = Path.Combine(Program.LocalUserDataFolder, "proxy.xml");
        private bool _testing;
        private Button btnCancel;
        private Button btnOK;
        private Button btnTest;
        private CheckBox chkDisableExpect100;
        private IContainer components = null;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label lblExpect100;
        private TableLayoutPanel panOKCancel;
        private RadioButton rbAuto;
        private RadioButton rbManual;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox txtAddress;
        private TextBox txtDomain;
        private TextBox txtPassword;
        private TextBox txtPort;
        private TextBox txtUsername;

        public ProxyForm()
        {
            this.InitializeComponent();
            ProxyOptions instance = ProxyOptions.Instance;
            this.rbManual.Checked = instance.IsManual;
            this.txtAddress.Text = instance.Address ?? "";
            this.txtPort.Text = (instance.Port == 0) ? "" : instance.Port.ToString();
            this.txtDomain.Text = instance.Domain ?? "";
            this.txtUsername.Text = instance.Username ?? "";
            this.txtPassword.Text = instance.Password ?? "";
            this.chkDisableExpect100.Checked = instance.DisableExpect100Continue;
            this.EnableControls();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Save();
            base.DialogResult = DialogResult.OK;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            this._testing = true;
            this.EnableControls();
            base.Update();
            BackgroundWorker worker = this._currentWorker = new BackgroundWorker();
            worker.DoWork += delegate (object sender, DoWorkEventArgs e) {
                try
                {
                    WebClient webClient;
                    if (this.rbAuto.Checked || (this.txtAddress.Text.Trim().Length == 0))
                    {
                        webClient = new WebClient();
                    }
                    else
                    {
                        ProxyOptions p = new ProxyOptions();
                        this.WriteTo(p);
                        webClient = p.GetWebClient();
                    }
                    byte[] data = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
                    webClient.UploadData("http://www.linqpad.net", data);
                    if (!((this._currentWorker != worker) || this.IsDisposed))
                    {
                        MessageBox.Show("Successful!");
                    }
                }
                catch (Exception exception)
                {
                    if (!((this._currentWorker != worker) || this.IsDisposed))
                    {
                        MessageBox.Show("Error: " + exception.Message);
                    }
                }
            };
            worker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e) {
                if (!((worker != this._currentWorker) || this.IsDisposed))
                {
                    this._testing = false;
                    this.EnableControls();
                }
            };
            worker.RunWorkerAsync();
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
            this.txtAddress.Enabled = this.txtPort.Enabled = this.txtDomain.Enabled = this.txtUsername.Enabled = this.txtPassword.Enabled = this.chkDisableExpect100.Enabled = this.lblExpect100.Enabled = this.rbManual.Checked && !this._testing;
            this.btnTest.Enabled = this.rbAuto.Enabled = this.rbManual.Enabled = !this._testing;
        }

        private void InitializeComponent()
        {
            this.panOKCancel = new TableLayoutPanel();
            this.btnTest = new Button();
            this.btnCancel = new Button();
            this.btnOK = new Button();
            this.rbAuto = new RadioButton();
            this.rbManual = new RadioButton();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.label1 = new Label();
            this.label2 = new Label();
            this.label3 = new Label();
            this.label4 = new Label();
            this.txtAddress = new TextBox();
            this.txtDomain = new TextBox();
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.label5 = new Label();
            this.txtPort = new TextBox();
            this.chkDisableExpect100 = new CheckBox();
            this.lblExpect100 = new Label();
            this.panOKCancel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnTest, 0, 0);
            this.panOKCancel.Controls.Add(this.btnCancel, 2, 0);
            this.panOKCancel.Controls.Add(this.btnOK, 1, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(9, 0x17b);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 3, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x19e, 0x26);
            this.panOKCancel.TabIndex = 1;
            this.btnTest.Dock = DockStyle.Left;
            this.btnTest.Location = new Point(3, 6);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new Size(0x4f, 0x1d);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "&Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new EventHandler(this.btnTest_Click);
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x14e, 6);
            this.btnCancel.Margin = new Padding(3, 3, 1, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnOK.Location = new Point(0xf9, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 0x1d);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.rbAuto.AutoSize = true;
            this.rbAuto.Checked = true;
            this.tableLayoutPanel1.SetColumnSpan(this.rbAuto, 2);
            this.rbAuto.Location = new Point(3, 3);
            this.rbAuto.Name = "rbAuto";
            this.rbAuto.Size = new Size(0xc0, 0x17);
            this.rbAuto.TabIndex = 0;
            this.rbAuto.TabStop = true;
            this.rbAuto.Text = "Automatically Detect Proxy";
            this.rbAuto.UseVisualStyleBackColor = true;
            this.rbAuto.CheckedChanged += new EventHandler(this.rbAuto_CheckedChanged);
            this.rbManual.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.rbManual, 2);
            this.rbManual.Location = new Point(3, 0x20);
            this.rbManual.Name = "rbManual";
            this.rbManual.Padding = new Padding(0, 0, 0, 6);
            this.rbManual.Size = new Size(0xa7, 0x1d);
            this.rbManual.TabIndex = 1;
            this.rbManual.Text = "Manually Specify Proxy";
            this.rbManual.UseVisualStyleBackColor = true;
            this.rbManual.Click += new EventHandler(this.rbManual_Click);
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.Controls.Add(this.rbAuto, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.rbManual, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.txtAddress, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtDomain, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtUsername, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.txtPassword, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.txtPort, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.chkDisableExpect100, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this.lblExpect100, 1, 9);
            this.tableLayoutPanel1.Dock = DockStyle.Top;
            this.tableLayoutPanel1.Location = new Point(9, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 12;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            this.tableLayoutPanel1.Size = new Size(0x19e, 0x142);
            this.tableLayoutPanel1.TabIndex = 0;
            this.label1.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0x43, 0x40);
            this.label1.Margin = new Padding(0x12, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x60, 0x1f);
            this.label1.TabIndex = 2;
            this.label1.Text = "Proxy Address";
            this.label1.TextAlign = ContentAlignment.MiddleLeft;
            this.label2.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0x20, 0x7e);
            this.label2.Margin = new Padding(0x12, 0, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x83, 0x1f);
            this.label2.TabIndex = 6;
            this.label2.Text = "Domain (if required)";
            this.label2.TextAlign = ContentAlignment.MiddleLeft;
            this.label3.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            this.label3.AutoSize = true;
            this.label3.Location = new Point(0x12, 0x9d);
            this.label3.Margin = new Padding(0x12, 0, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x91, 0x1f);
            this.label3.TabIndex = 8;
            this.label3.Text = "Username (if required)";
            this.label3.TextAlign = ContentAlignment.MiddleLeft;
            this.label4.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            this.label4.AutoSize = true;
            this.label4.Location = new Point(0x16, 0xbc);
            this.label4.Margin = new Padding(0x12, 0, 3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new Size(0x8d, 0x1f);
            this.label4.TabIndex = 10;
            this.label4.Text = "Password (if required)";
            this.label4.TextAlign = ContentAlignment.MiddleLeft;
            this.txtAddress.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtAddress.Location = new Point(0xa9, 0x43);
            this.txtAddress.Margin = new Padding(3, 3, 0, 3);
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new Size(0xf5, 0x19);
            this.txtAddress.TabIndex = 3;
            this.txtDomain.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtDomain.Location = new Point(0xa9, 0x81);
            this.txtDomain.Margin = new Padding(3, 3, 0, 3);
            this.txtDomain.Name = "txtDomain";
            this.txtDomain.Size = new Size(0xf5, 0x19);
            this.txtDomain.TabIndex = 7;
            this.txtUsername.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtUsername.Location = new Point(0xa9, 160);
            this.txtUsername.Margin = new Padding(3, 3, 0, 3);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new Size(0xf5, 0x19);
            this.txtUsername.TabIndex = 9;
            this.txtPassword.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtPassword.Location = new Point(0xa9, 0xbf);
            this.txtPassword.Margin = new Padding(3, 3, 0, 3);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new Size(0xf5, 0x19);
            this.txtPassword.TabIndex = 11;
            this.label5.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            this.label5.AutoSize = true;
            this.label5.Location = new Point(90, 0x5f);
            this.label5.Margin = new Padding(0x12, 0, 3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x49, 0x1f);
            this.label5.TabIndex = 4;
            this.label5.Text = "Proxy Port";
            this.label5.TextAlign = ContentAlignment.MiddleLeft;
            this.txtPort.Location = new Point(0xa9, 0x62);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new Size(0x49, 0x19);
            this.txtPort.TabIndex = 5;
            this.chkDisableExpect100.AutoSize = true;
            this.chkDisableExpect100.Location = new Point(0xa9, 0x113);
            this.chkDisableExpect100.Margin = new Padding(3, 8, 3, 1);
            this.chkDisableExpect100.Name = "chkDisableExpect100";
            this.chkDisableExpect100.Size = new Size(0xcf, 0x17);
            this.chkDisableExpect100.TabIndex = 12;
            this.chkDisableExpect100.Text = "Disable Expect-100-Continue";
            this.chkDisableExpect100.UseVisualStyleBackColor = true;
            this.lblExpect100.AutoSize = true;
            this.lblExpect100.Location = new Point(0xa6, 0xe5);
            this.lblExpect100.Margin = new Padding(0, 10, 3, 0);
            this.lblExpect100.Name = "lblExpect100";
            this.lblExpect100.Size = new Size(0xf5, 0x26);
            this.lblExpect100.TabIndex = 13;
            this.lblExpect100.Text = "If you get error 417, tick the following option and then re-start LINQPad";
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x1b0, 0x1a9);
            base.ControlBox = false;
            base.Controls.Add(this.tableLayoutPanel1);
            base.Controls.Add(this.panOKCancel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ProxyForm";
            base.Padding = new Padding(9, 12, 9, 8);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Web Proxy Setup (global settings for LINQPad)";
            this.panOKCancel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void rbAuto_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbManual_Click(object sender, EventArgs e)
        {
            if (this.txtAddress.Text.Trim().Length == 0)
            {
                this.txtAddress.Text = "http://";
                this.txtAddress.Focus();
                this.txtAddress.SelectionStart = 7;
            }
        }

        private void Save()
        {
            this.WriteTo(ProxyOptions.Instance);
            ProxyOptions.Instance.Save();
        }

        private void WriteTo(ProxyOptions p)
        {
            p.IsManual = this.rbManual.Checked;
            p.Address = this.txtAddress.Text.Trim();
            ushort result = 0;
            ushort.TryParse(this.txtPort.Text, out result);
            p.Port = result;
            p.Domain = this.txtDomain.Text.Trim();
            p.Username = this.txtUsername.Text.Trim();
            p.Password = this.txtPassword.Text.Trim();
            p.DisableExpect100Continue = this.chkDisableExpect100.Checked;
        }
    }
}

