namespace LINQPad.UI
{
    using ActiproBridge;
    using LINQPad;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    internal class RegisterForm : BaseForm
    {
        private bool _activating;
        private bool _cancelled;
        private string _origText;
        private BackgroundWorker _worker;
        private Button btnActivate;
        private Button btnCancel;
        private Button btnProxy;
        private IContainer components;
        private Label lblCode;
        private Label lblNeedsInternet;
        private LinkLabel llGetActivationCode;
        private LinkLabel llRecoverLostCode;
        private TableLayoutPanel panOKCancel;
        private RadioButton rbAll;
        private RadioButton rbMe;
        private TextBox txtCode;

        public RegisterForm()
        {
            EventHandler handler = null;
            this._worker = new BackgroundWorker();
            this.components = null;
            this.InitializeComponent();
            handler = delegate (object sender, EventArgs e) {
                if (((this.txtCode.Text.Trim().Length == 0x1a) && (this.txtCode.Text.Trim()[5] == '-')) && "+-".Contains<char>(this.txtCode.Text.Trim()[11]))
                {
                    this.txtCode.Text = this.txtCode.Text.Trim().Substring(0, 11);
                }
            };
            this.txtCode.TextChanged += handler;
            this._origText = this.Text;
            try
            {
                this.rbMe.Text = this.rbMe.Text + " (" + Environment.UserName + ")";
            }
            catch
            {
            }
            try
            {
                this.lblNeedsInternet.Font = new Font(this.lblNeedsInternet.Font, FontStyle.Bold);
            }
            catch
            {
            }
            this._worker.WorkerSupportsCancellation = true;
            this._worker.DoWork += new DoWorkEventHandler(this._worker_DoWork);
            this._worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this._worker_RunWorkerCompleted);
        }

        public RegisterForm(string code) : this()
        {
            this.txtCode.PasswordChar = '*';
            this.txtCode.Text = code.Trim();
            this.btnActivate_Click(this, EventArgs.Empty);
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WSAgent.Register(this.ActivationCode, this.rbAll.Checked, e);
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!this._cancelled)
            {
                this._activating = false;
                this.EnableControls();
                if (e.Cancelled)
                {
                    this.Text = this._origText;
                    MessageBox.Show("Canceled.");
                }
                else if (e.Error != null)
                {
                    Log.Write(e.Error, "RegisterForm RunWorkerCompleted");
                    Program.ProcessException(e.Error);
                }
                else if (e.Result is bool)
                {
                    if ((bool) e.Result)
                    {
                        base.DialogResult = DialogResult.OK;
                        base.Close();
                    }
                    else if (MessageBox.Show("This activation key has been used on more computers than is permitted by the license.\r\n\r\nWould you like to view/edit your activations?", "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        WebHelper.LaunchBrowser("https://www.linqpad.net/licensing/ListActivations.aspx");
                    }
                }
                else if (e.Result is string)
                {
                    this.Text = this._origText;
                    MessageBox.Show("Error: " + ((string) e.Result));
                    if (WSAgent.FailedRegBlob != null)
                    {
                        try
                        {
                            Clipboard.SetText(WSAgent.FailedRegBlob);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            this._activating = true;
            this.Text = "Working...";
            this.EnableControls();
            base.Update();
            this._worker.RunWorkerAsync();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this._activating)
            {
                this._cancelled = true;
                this._activating = false;
                this._worker.CancelAsync();
                this.EnableControls();
            }
        }

        private void btnProxy_Click(object sender, EventArgs e)
        {
            using (ProxyForm form = new ProxyForm())
            {
                form.ShowDialog(this);
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

        private void EnableControls()
        {
            this.txtCode.Enabled = this.btnProxy.Enabled = this.rbAll.Enabled = this.rbMe.Enabled = !this._activating;
            this.btnActivate.Enabled = !this._activating && (this.txtCode.Text.Trim().Length >= 1);
        }

        private void InitializeComponent()
        {
            this.lblCode = new Label();
            this.txtCode = new TextBox();
            this.panOKCancel = new TableLayoutPanel();
            this.btnCancel = new Button();
            this.btnActivate = new Button();
            this.rbMe = new RadioButton();
            this.rbAll = new RadioButton();
            this.lblNeedsInternet = new Label();
            this.btnProxy = new Button();
            this.llGetActivationCode = new LinkLabel();
            this.llRecoverLostCode = new LinkLabel();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.lblCode.AutoSize = true;
            this.lblCode.Location = new Point(0x12, 0x13);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new Size(0x89, 0x13);
            this.lblCode.TabIndex = 0;
            this.lblCode.Text = "Your activation code:";
            this.txtCode.Location = new Point(0x15, 40);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new Size(0x84, 0x19);
            this.txtCode.TabIndex = 1;
            this.txtCode.TextChanged += new EventHandler(this.txtCode_TextChanged);
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnCancel, 2, 0);
            this.panOKCancel.Controls.Add(this.btnActivate, 1, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(6, 0x114);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 3, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x183, 0x27);
            this.panOKCancel.TabIndex = 6;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x133, 6);
            this.btnCancel.Margin = new Padding(3, 3, 1, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.btnActivate.Enabled = false;
            this.btnActivate.Location = new Point(0xde, 6);
            this.btnActivate.Name = "btnActivate";
            this.btnActivate.Size = new Size(0x4f, 30);
            this.btnActivate.TabIndex = 1;
            this.btnActivate.Text = "Activate";
            this.btnActivate.UseVisualStyleBackColor = true;
            this.btnActivate.Click += new EventHandler(this.btnActivate_Click);
            this.rbMe.AutoSize = true;
            this.rbMe.Checked = true;
            this.rbMe.Location = new Point(0x15, 0x57);
            this.rbMe.Name = "rbMe";
            this.rbMe.Size = new Size(0xcd, 0x17);
            this.rbMe.TabIndex = 2;
            this.rbMe.TabStop = true;
            this.rbMe.Text = "Register LINQPad just for me";
            this.rbMe.UseVisualStyleBackColor = true;
            this.rbAll.AutoSize = true;
            this.rbAll.Location = new Point(0x15, 0x74);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new Size(0xf5, 0x17);
            this.rbAll.TabIndex = 3;
            this.rbAll.TabStop = true;
            this.rbAll.Text = "Register everyone on this computer";
            this.rbAll.UseVisualStyleBackColor = true;
            this.lblNeedsInternet.AutoSize = true;
            this.lblNeedsInternet.Location = new Point(0x15, 0xa2);
            this.lblNeedsInternet.Name = "lblNeedsInternet";
            this.lblNeedsInternet.Size = new Size(0x100, 0x13);
            this.lblNeedsInternet.TabIndex = 4;
            this.lblNeedsInternet.Text = "Activation requires Internet connectivity.";
            this.lblNeedsInternet.MouseDown += new MouseEventHandler(this.lblNeedsInternet_MouseDown);
            this.btnProxy.Location = new Point(0x15, 0xb9);
            this.btnProxy.Name = "btnProxy";
            this.btnProxy.Size = new Size(0x107, 0x1d);
            this.btnProxy.TabIndex = 5;
            this.btnProxy.Text = "Specify Web Proxy Server...";
            this.btnProxy.UseVisualStyleBackColor = true;
            this.btnProxy.Click += new EventHandler(this.btnProxy_Click);
            this.llGetActivationCode.AutoSize = true;
            this.llGetActivationCode.Location = new Point(0xd9, 0x13);
            this.llGetActivationCode.Name = "llGetActivationCode";
            this.llGetActivationCode.Size = new Size(0x8d, 0x16);
            this.llGetActivationCode.TabIndex = 7;
            this.llGetActivationCode.TabStop = true;
            this.llGetActivationCode.Text = "Get an Activation Code";
            this.llGetActivationCode.UseCompatibleTextRendering = true;
            this.llGetActivationCode.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llGetActivationCode_LinkClicked);
            this.llRecoverLostCode.AutoSize = true;
            this.llRecoverLostCode.Location = new Point(0xd9, 0x2c);
            this.llRecoverLostCode.Name = "llRecoverLostCode";
            this.llRecoverLostCode.Size = new Size(0x73, 0x16);
            this.llRecoverLostCode.TabIndex = 8;
            this.llRecoverLostCode.TabStop = true;
            this.llRecoverLostCode.Text = "Recover Lost Code";
            this.llRecoverLostCode.UseCompatibleTextRendering = true;
            this.llRecoverLostCode.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llRecoverLostCode_LinkClicked);
            base.AcceptButton = this.btnActivate;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x18f, 0x142);
            base.ControlBox = false;
            base.Controls.Add(this.llRecoverLostCode);
            base.Controls.Add(this.llGetActivationCode);
            base.Controls.Add(this.btnProxy);
            base.Controls.Add(this.lblNeedsInternet);
            base.Controls.Add(this.rbAll);
            base.Controls.Add(this.rbMe);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.txtCode);
            base.Controls.Add(this.lblCode);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "RegisterForm";
            base.Padding = new Padding(6, 7, 6, 7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Activate Autocompletion and Premium Features";
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void lblNeedsInternet_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                using (OfflineActivation activation = new OfflineActivation(this.rbAll.Checked))
                {
                    activation.ShowDialog(this);
                }
                base.Close();
            }
        }

        private void llGetActivationCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebHelper.LaunchBrowser("http://www.linqpad.net/Purchase.aspx");
        }

        private void llRecoverLostCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebHelper.LaunchBrowser("https://www.linqpad.net/licensing/CustomerService.aspx");
        }

        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private string ActivationCode
        {
            get
            {
                return this.txtCode.Text.Replace("-", "").Trim().ToUpperInvariant();
            }
        }
    }
}

