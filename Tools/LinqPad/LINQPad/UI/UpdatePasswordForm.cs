namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class UpdatePasswordForm : BaseForm
    {
        private Button btnCancel;
        private Button btnDontAsk;
        private Button btnOK;
        private IContainer components = null;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private TableLayoutPanel panOKCancel;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox txtConfirm;
        private TextBox txtNew;
        private TextBox txtOld;
        private TextBox txtServer;
        private TextBox txtUser;

        public UpdatePasswordForm(string server, string user)
        {
            this.InitializeComponent();
            this.txtServer.Text = server;
            this.txtUser.Text = user;
        }

        private void btnDontAsk_Click(object sender, EventArgs e)
        {
            UserOptions.Instance.NoSqlPasswordExpiryPrompts = true;
            UserOptions.Instance.Save();
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
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.btnDontAsk = new Button();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.label1 = new Label();
            this.label2 = new Label();
            this.label3 = new Label();
            this.label4 = new Label();
            this.label5 = new Label();
            this.label6 = new Label();
            this.txtServer = new TextBox();
            this.txtUser = new TextBox();
            this.txtOld = new TextBox();
            this.txtNew = new TextBox();
            this.txtConfirm = new TextBox();
            this.panOKCancel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 4;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnOK, 2, 0);
            this.panOKCancel.Controls.Add(this.btnCancel, 3, 0);
            this.panOKCancel.Controls.Add(this.btnDontAsk, 0, 0);
            this.panOKCancel.Dock = DockStyle.Top;
            this.panOKCancel.Location = new Point(7, 0xff);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 5, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x1c1, 0x26);
            this.panOKCancel.TabIndex = 1;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0x11a, 8);
            this.btnOK.Margin = new Padding(3, 3, 3, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 30);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x16f, 8);
            this.btnCancel.Margin = new Padding(3, 3, 3, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnDontAsk.DialogResult = DialogResult.Cancel;
            this.btnDontAsk.Location = new Point(3, 8);
            this.btnDontAsk.Margin = new Padding(3, 3, 0, 0);
            this.btnDontAsk.Name = "btnDontAsk";
            this.btnDontAsk.Size = new Size(0xd4, 30);
            this.btnDontAsk.TabIndex = 3;
            this.btnDontAsk.Text = "Cancel and Don't Ask Again";
            this.btnDontAsk.UseVisualStyleBackColor = true;
            this.btnDontAsk.Click += new EventHandler(this.btnDontAsk_Click);
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.txtServer, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtUser, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtOld, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.txtNew, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtConfirm, 1, 5);
            this.tableLayoutPanel1.Dock = DockStyle.Top;
            this.tableLayoutPanel1.Location = new Point(7, 7);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new Padding(0, 0, 0, 10);
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.Size = new Size(0x1c1, 0xf8);
            this.tableLayoutPanel1.TabIndex = 0;
            this.label1.Anchor = AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label1, 2);
            this.label1.Location = new Point(0x65, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 10, 0, 20);
            this.label1.Size = new Size(0xf7, 0x44);
            this.label1.TabIndex = 6;
            this.label1.Text = "Your SQL Server password has expired.\r\nLINQPad can reset it for you.";
            this.label1.TextAlign = ContentAlignment.TopCenter;
            this.label2.Anchor = AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(3, 0x4a);
            this.label2.Name = "label2";
            this.label2.Size = new Size(50, 0x13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Server:";
            this.label3.Anchor = AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new Point(3, 0x69);
            this.label3.Margin = new Padding(3, 0, 3, 15);
            this.label3.Name = "label3";
            this.label3.Size = new Size(40, 0x13);
            this.label3.TabIndex = 9;
            this.label3.Text = "User:";
            this.label4.Anchor = AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new Point(3, 0x97);
            this.label4.Name = "label4";
            this.label4.Size = new Size(0x60, 0x13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Old Password:";
            this.label5.Anchor = AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new Point(3, 0xb6);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x65, 0x13);
            this.label5.TabIndex = 2;
            this.label5.Text = "New Password:";
            this.label6.Anchor = AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new Point(3, 0xd5);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0x9a, 0x13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Confirm New Password:";
            this.txtServer.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtServer.Location = new Point(0xa3, 0x47);
            this.txtServer.Name = "txtServer";
            this.txtServer.ReadOnly = true;
            this.txtServer.Size = new Size(0x11b, 0x19);
            this.txtServer.TabIndex = 8;
            this.txtUser.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtUser.Location = new Point(0xa3, 0x66);
            this.txtUser.Margin = new Padding(3, 3, 3, 0x12);
            this.txtUser.Name = "txtUser";
            this.txtUser.ReadOnly = true;
            this.txtUser.Size = new Size(0x11b, 0x19);
            this.txtUser.TabIndex = 10;
            this.txtOld.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtOld.Location = new Point(0xa3, 0x94);
            this.txtOld.Name = "txtOld";
            this.txtOld.PasswordChar = '*';
            this.txtOld.Size = new Size(0x11b, 0x19);
            this.txtOld.TabIndex = 1;
            this.txtNew.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtNew.Location = new Point(0xa3, 0xb3);
            this.txtNew.Name = "txtNew";
            this.txtNew.PasswordChar = '*';
            this.txtNew.Size = new Size(0x11b, 0x19);
            this.txtNew.TabIndex = 3;
            this.txtConfirm.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtConfirm.Location = new Point(0xa3, 210);
            this.txtConfirm.Name = "txtConfirm";
            this.txtConfirm.PasswordChar = '*';
            this.txtConfirm.Size = new Size(0x11b, 0x19);
            this.txtConfirm.TabIndex = 5;
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x1cf, 0x12b);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.tableLayoutPanel1);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "UpdatePasswordForm";
            base.Padding = new Padding(7, 7, 7, 8);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Update Password";
            base.TopMost = true;
            this.panOKCancel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((base.DialogResult == DialogResult.OK) && (this.txtNew.Text != this.txtConfirm.Text))
            {
                MessageBox.Show("Passwords do not match", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            base.ClientSize = new Size(base.ClientSize.Width, this.panOKCancel.Bottom + base.Padding.Bottom);
        }

        public string NewPassword
        {
            get
            {
                return this.txtNew.Text;
            }
        }

        public string OldPassword
        {
            get
            {
                return this.txtOld.Text;
            }
        }
    }
}

