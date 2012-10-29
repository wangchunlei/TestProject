namespace LINQPad.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class ThreadExceptionForm : BaseForm
    {
        private string _message;
        private Button btnClose;
        private Button btnCopy;
        private IContainer components;
        private Label lblTitle;
        private TableLayoutPanel panOKCancel;
        private TextBox txtMessage;

        public ThreadExceptionForm(Exception ex) : this(ex, null)
        {
        }

        public ThreadExceptionForm(Exception ex, string title)
        {
            this.components = null;
            this.InitializeComponent();
            if (!string.IsNullOrEmpty(title))
            {
                this.lblTitle.Text = title;
            }
            string str = "";
            for (int i = 0; ex != null; i++)
            {
                if (i > 0)
                {
                    string str2 = "".PadRight(i * 3, ' ');
                    string str3 = str;
                    str = str3 + "\r\n\r\n" + str2 + "INNER EXCEPTION:\r\n" + str2;
                }
                str = str + ex.GetType().FullName + ": " + ex.Message;
                if (ex.StackTrace != null)
                {
                    str = str + "\r\n\r\n" + ex.StackTrace;
                }
                ex = ex.InnerException;
            }
            this.txtMessage.Text = this._message = str;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(delegate {
                try
                {
                    Clipboard.SetText(this._message);
                }
                catch
                {
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
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
            this.lblTitle = new Label();
            this.txtMessage = new TextBox();
            this.panOKCancel = new TableLayoutPanel();
            this.btnClose = new Button();
            this.btnCopy = new Button();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = DockStyle.Top;
            this.lblTitle.Location = new Point(7, 7);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new Padding(0, 0, 0, 5);
            this.lblTitle.Size = new Size(0x162, 0x18);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "An exception has been thrown on a user-created thread.";
            this.txtMessage.Dock = DockStyle.Fill;
            this.txtMessage.Location = new Point(7, 0x1f);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = ScrollBars.Vertical;
            this.txtMessage.Size = new Size(0x202, 0xeb);
            this.txtMessage.TabIndex = 2;
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnClose, 2, 0);
            this.panOKCancel.Controls.Add(this.btnCopy, 1, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(7, 0x10a);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 4, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x202, 0x27);
            this.panOKCancel.TabIndex = 0;
            this.btnClose.DialogResult = DialogResult.OK;
            this.btnClose.Location = new Point(0x1b2, 7);
            this.btnClose.Margin = new Padding(3, 3, 1, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(0x4f, 0x1d);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnCopy.Location = new Point(0xe1, 7);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new Size(0xcb, 0x1d);
            this.btnCopy.TabIndex = 1;
            this.btnCopy.Text = "C&opy details to clipboard";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new EventHandler(this.btnCopy_Click);
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnClose;
            base.ClientSize = new Size(0x210, 0x138);
            base.Controls.Add(this.txtMessage);
            base.Controls.Add(this.lblTitle);
            base.Controls.Add(this.panOKCancel);
            base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.Name = "ThreadExceptionForm";
            base.Padding = new Padding(7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "LINQPad - Unhandled Thread Exception";
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.btnClose.Focus();
        }
    }
}

