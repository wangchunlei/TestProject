namespace LINQPad.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ErrorForm : Form
    {
        private Button btnCancel;
        private Button btnSubmit;
        private IContainer components = null;
        private Label label1;
        private Label label2;
        private Label lblDetails;
        private Label lblError;
        private LinkLabel llMore;
        private FlowLayoutPanel panDetails;
        private Panel panel1;
        private Panel panel2;
        private TextBox txtAdditionalInfo;
        private TextBox txtError;

        public ErrorForm(string error, string details)
        {
            this.InitializeComponent();
            this.txtError.Text = error;
            if (string.IsNullOrEmpty(details))
            {
                this.llMore.Hide();
            }
            else if (details.Length < 500)
            {
                this.lblDetails.Text = details;
            }
            else
            {
                this.lblDetails.Text = details.Substring(0, 0x1ef) + "...";
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
            this.lblError = new Label();
            this.txtError = new TextBox();
            this.label1 = new Label();
            this.label2 = new Label();
            this.txtAdditionalInfo = new TextBox();
            this.btnSubmit = new Button();
            this.btnCancel = new Button();
            this.panel1 = new Panel();
            this.panel2 = new Panel();
            this.llMore = new LinkLabel();
            this.panDetails = new FlowLayoutPanel();
            this.lblDetails = new Label();
            this.panel1.SuspendLayout();
            this.panDetails.SuspendLayout();
            base.SuspendLayout();
            this.lblError.AutoSize = true;
            this.lblError.Dock = DockStyle.Top;
            this.lblError.Location = new Point(6, 6);
            this.lblError.Margin = new Padding(2, 0, 2, 0);
            this.lblError.Name = "lblError";
            this.lblError.Padding = new Padding(0, 0, 0, 2);
            this.lblError.Size = new Size(0x7f, 0x11);
            this.lblError.TabIndex = 3;
            this.lblError.Text = "An error has occurred:";
            this.txtError.Dock = DockStyle.Top;
            this.txtError.Location = new Point(6, 0x17);
            this.txtError.Margin = new Padding(2);
            this.txtError.Name = "txtError";
            this.txtError.ReadOnly = true;
            this.txtError.Size = new Size(0x1a7, 20);
            this.txtError.TabIndex = 4;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(6, 0x55);
            this.label1.Margin = new Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 12, 0, 0);
            this.label1.Size = new Size(0x1a7, 0x35);
            this.label1.TabIndex = 5;
            this.label1.Text = "To automatically report the error and stack trace, click the \"Submit\" button. You can optionally enter additional information in the box below.";
            this.label2.AutoSize = true;
            this.label2.Dock = DockStyle.Top;
            this.label2.Location = new Point(6, 0x8a);
            this.label2.Margin = new Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Padding = new Padding(0, 0, 0, 2);
            this.label2.Size = new Size(0xb8, 0x11);
            this.label2.TabIndex = 0;
            this.label2.Text = "Additional Information (optional):";
            this.txtAdditionalInfo.Dock = DockStyle.Fill;
            this.txtAdditionalInfo.Location = new Point(6, 0x9b);
            this.txtAdditionalInfo.Margin = new Padding(2);
            this.txtAdditionalInfo.Multiline = true;
            this.txtAdditionalInfo.Name = "txtAdditionalInfo";
            this.txtAdditionalInfo.Size = new Size(0x1a7, 0x43);
            this.txtAdditionalInfo.TabIndex = 0;
            this.btnSubmit.DialogResult = DialogResult.OK;
            this.btnSubmit.Dock = DockStyle.Right;
            this.btnSubmit.Location = new Point(0x111, 8);
            this.btnSubmit.Margin = new Padding(2);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new Size(0x49, 0x19);
            this.btnSubmit.TabIndex = 1;
            this.btnSubmit.Text = "&Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Dock = DockStyle.Right;
            this.btnCancel.Location = new Point(350, 8);
            this.btnCancel.Margin = new Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x49, 0x19);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.panel1.Controls.Add(this.btnSubmit);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = DockStyle.Bottom;
            this.panel1.Location = new Point(6, 0xde);
            this.panel1.Margin = new Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new Padding(0, 8, 0, 0);
            this.panel1.Size = new Size(0x1a7, 0x21);
            this.panel1.TabIndex = 6;
            this.panel2.Dock = DockStyle.Right;
            this.panel2.Location = new Point(0x15a, 8);
            this.panel2.Margin = new Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new Size(4, 0x19);
            this.panel2.TabIndex = 3;
            this.llMore.AutoSize = true;
            this.llMore.Dock = DockStyle.Top;
            this.llMore.Location = new Point(6, 0x41);
            this.llMore.Name = "llMore";
            this.llMore.Padding = new Padding(0, 5, 0, 0);
            this.llMore.Size = new Size(0x6a, 20);
            this.llMore.TabIndex = 7;
            this.llMore.TabStop = true;
            this.llMore.Text = "View More Details";
            this.llMore.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llMore_LinkClicked);
            this.panDetails.AutoSize = true;
            this.panDetails.Controls.Add(this.lblDetails);
            this.panDetails.Dock = DockStyle.Top;
            this.panDetails.Location = new Point(6, 0x2b);
            this.panDetails.Name = "panDetails";
            this.panDetails.Padding = new Padding(0, 7, 0, 0);
            this.panDetails.Size = new Size(0x1a7, 0x16);
            this.panDetails.TabIndex = 8;
            this.panDetails.Visible = false;
            this.lblDetails.AutoSize = true;
            this.lblDetails.Location = new Point(13, 7);
            this.lblDetails.Margin = new Padding(13, 0, 13, 0);
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Size = new Size(0x10, 15);
            this.lblDetails.TabIndex = 0;
            this.lblDetails.Text = "...";
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x1b3, 0x105);
            base.ControlBox = false;
            base.Controls.Add(this.txtAdditionalInfo);
            base.Controls.Add(this.panel1);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.llMore);
            base.Controls.Add(this.panDetails);
            base.Controls.Add(this.txtError);
            base.Controls.Add(this.lblError);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Margin = new Padding(2);
            base.Name = "ErrorForm";
            base.Padding = new Padding(6);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "LINQPad - Error";
            this.panel1.ResumeLayout(false);
            this.panDetails.ResumeLayout(false);
            this.panDetails.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void llMore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.llMore.Hide();
            this.panDetails.Show();
            base.Height += this.panDetails.Height;
        }

        public string AdditionalInfo
        {
            get
            {
                return this.txtAdditionalInfo.Text.Trim();
            }
        }
    }
}

