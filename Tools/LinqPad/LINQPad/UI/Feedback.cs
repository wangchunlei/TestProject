namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    internal class Feedback : BaseForm
    {
        private Button btnCancel;
        private Button btnProxy;
        private Button btnSubmit;
        private CheckBox chkSubmitQuery;
        private IContainer components = null;
        private Label label1;
        private Label label2;
        private Panel panControls;
        private Panel panel1;
        private Panel panMain;
        private TextBox txtEmail;
        private TextBox txtFeedback;

        public Feedback()
        {
            this.InitializeComponent();
            base.Icon = Resources.LINQPad;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void btnProxy_Click(object sender, EventArgs e)
        {
            using (ProxyForm form = new ProxyForm())
            {
                form.ShowDialog();
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            this.btnSubmit.Enabled = false;
            this.txtFeedback.Enabled = false;
            this.Text = "Submitting....";
            string queryData = null;
            if (this.chkSubmitQuery.Checked)
            {
                try
                {
                    MemoryStream stream = new MemoryStream();
                    StreamWriter sr = new StreamWriter(stream);
                    RunnableQuery query = MainForm.Instance.CurrentQueryControl.Query;
                    query.WriteTo(sr, query.FilePath, true);
                    sr.Flush();
                    queryData = Convert.ToBase64String(stream.ToArray());
                }
                catch
                {
                }
            }
            new Thread(delegate {
                try
                {
                    Program.SubmitFeedback("LINQPad Feedback", this.txtEmail.Text.Trim(), this.txtFeedback.Text.Trim(), queryData);
                    if (!this.IsDisposed)
                    {
                        this.Invoke(new MethodInvoker(this.Success));
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Could not submit: " + exception.Message);
                    if (!this.IsDisposed)
                    {
                        try
                        {
                            this.Invoke(new MethodInvoker(this.Failure));
                        }
                        catch
                        {
                        }
                    }
                }
            }) { Name = "Feedback submission", IsBackground = true }.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Failure()
        {
            if (!base.IsDisposed)
            {
                this.Text = "Your Feedback";
                this.btnSubmit.Enabled = true;
                this.txtFeedback.Enabled = true;
            }
        }

        private void InitializeComponent()
        {
            this.txtFeedback = new TextBox();
            this.panControls = new Panel();
            this.btnProxy = new Button();
            this.btnSubmit = new Button();
            this.panel1 = new Panel();
            this.btnCancel = new Button();
            this.panMain = new Panel();
            this.label1 = new Label();
            this.txtEmail = new TextBox();
            this.label2 = new Label();
            this.chkSubmitQuery = new CheckBox();
            this.panControls.SuspendLayout();
            this.panMain.SuspendLayout();
            base.SuspendLayout();
            this.txtFeedback.AcceptsReturn = true;
            this.txtFeedback.Dock = DockStyle.Fill;
            this.txtFeedback.Location = new Point(7, 0x1d);
            this.txtFeedback.Multiline = true;
            this.txtFeedback.Name = "txtFeedback";
            this.txtFeedback.ScrollBars = ScrollBars.Vertical;
            this.txtFeedback.Size = new Size(420, 0x99);
            this.txtFeedback.TabIndex = 1;
            this.txtFeedback.TextChanged += new EventHandler(this.txtFeedback_TextChanged);
            this.panControls.Controls.Add(this.btnProxy);
            this.panControls.Controls.Add(this.btnSubmit);
            this.panControls.Controls.Add(this.panel1);
            this.panControls.Controls.Add(this.btnCancel);
            this.panControls.Dock = DockStyle.Bottom;
            this.panControls.Location = new Point(0, 0x111);
            this.panControls.Name = "panControls";
            this.panControls.Padding = new Padding(7, 8, 7, 8);
            this.panControls.Size = new Size(0x1b2, 0x2e);
            this.panControls.TabIndex = 1;
            this.btnProxy.Dock = DockStyle.Left;
            this.btnProxy.Location = new Point(7, 8);
            this.btnProxy.Name = "btnProxy";
            this.btnProxy.Size = new Size(150, 30);
            this.btnProxy.TabIndex = 3;
            this.btnProxy.Text = "Use Web Proxy...";
            this.btnProxy.UseVisualStyleBackColor = true;
            this.btnProxy.Click += new EventHandler(this.btnProxy_Click);
            this.btnSubmit.Dock = DockStyle.Right;
            this.btnSubmit.Enabled = false;
            this.btnSubmit.Location = new Point(0x108, 8);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new Size(0x4f, 30);
            this.btnSubmit.TabIndex = 0;
            this.btnSubmit.Text = "&Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new EventHandler(this.btnSubmit_Click);
            this.panel1.Dock = DockStyle.Right;
            this.panel1.Location = new Point(0x157, 8);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(5, 30);
            this.panel1.TabIndex = 2;
            this.btnCancel.Dock = DockStyle.Right;
            this.btnCancel.Location = new Point(0x15c, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.panMain.Controls.Add(this.txtFeedback);
            this.panMain.Controls.Add(this.label1);
            this.panMain.Controls.Add(this.txtEmail);
            this.panMain.Controls.Add(this.label2);
            this.panMain.Controls.Add(this.chkSubmitQuery);
            this.panMain.Dock = DockStyle.Fill;
            this.panMain.Location = new Point(0, 0);
            this.panMain.Name = "panMain";
            this.panMain.Padding = new Padding(7, 8, 7, 8);
            this.panMain.Size = new Size(0x1b2, 0x111);
            this.panMain.TabIndex = 0;
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Bottom;
            this.label1.Location = new Point(7, 0xb6);
            this.label1.Margin = new Padding(0, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 7, 0, 2);
            this.label1.Size = new Size(0xc1, 0x1c);
            this.label1.TabIndex = 2;
            this.label1.Text = "Your e-mail address (optional)";
            this.txtEmail.Dock = DockStyle.Bottom;
            this.txtEmail.Location = new Point(7, 210);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new Size(420, 0x19);
            this.txtEmail.TabIndex = 3;
            this.label2.AutoSize = true;
            this.label2.Dock = DockStyle.Top;
            this.label2.Location = new Point(7, 8);
            this.label2.Margin = new Padding(0, 0, 3, 0);
            this.label2.Name = "label2";
            this.label2.Padding = new Padding(0, 0, 0, 2);
            this.label2.Size = new Size(0x3f, 0x15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Message";
            this.chkSubmitQuery.AutoSize = true;
            this.chkSubmitQuery.Dock = DockStyle.Bottom;
            this.chkSubmitQuery.Location = new Point(7, 0xeb);
            this.chkSubmitQuery.Name = "chkSubmitQuery";
            this.chkSubmitQuery.Padding = new Padding(2, 7, 0, 0);
            this.chkSubmitQuery.Size = new Size(420, 30);
            this.chkSubmitQuery.TabIndex = 4;
            this.chkSubmitQuery.Text = "S&ubmit Current Query";
            this.chkSubmitQuery.UseVisualStyleBackColor = true;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x1b2, 0x13f);
            base.Controls.Add(this.panMain);
            base.Controls.Add(this.panControls);
            base.Name = "Feedback";
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Your Feedback";
            this.panControls.ResumeLayout(false);
            this.panMain.ResumeLayout(false);
            this.panMain.PerformLayout();
            base.ResumeLayout(false);
        }

        protected override void OnActivated(EventArgs e)
        {
            this.chkSubmitQuery.Enabled = (MainForm.Instance != null) && (MainForm.Instance.CurrentQueryControl != null);
            base.OnActivated(e);
        }

        private void Success()
        {
            if (!base.IsDisposed)
            {
                MessageBox.Show("Feedback submission successful.", "Thank you", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                base.Close();
            }
        }

        private void txtFeedback_TextChanged(object sender, EventArgs e)
        {
            this.btnSubmit.Enabled = this.txtFeedback.Text.Trim().Length > 0;
        }
    }
}

