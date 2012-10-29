namespace LINQPad.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class TextBoxForm : BaseForm
    {
        private Button btnCancel;
        private Button btnOK;
        private IContainer components;
        private Label lblPrompt;
        private TableLayoutPanel panOKCancel;
        private TextBox textBox;

        public TextBoxForm(string prompt) : this(prompt, false)
        {
        }

        public TextBoxForm(string prompt, bool password)
        {
            this.components = null;
            this.InitializeComponent();
            if (password)
            {
                this.textBox.PasswordChar = '*';
            }
            this.lblPrompt.Text = prompt;
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
            this.lblPrompt = new Label();
            this.textBox = new TextBox();
            this.panOKCancel.SuspendLayout();
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
            this.panOKCancel.Dock = DockStyle.Top;
            this.panOKCancel.Location = new Point(7, 0x36);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 10, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(390, 0x2b);
            this.panOKCancel.TabIndex = 1;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0xe2, 13);
            this.btnOK.Margin = new Padding(3, 3, 3, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 30);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x137, 13);
            this.btnCancel.Margin = new Padding(3, 3, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Dock = DockStyle.Top;
            this.lblPrompt.Location = new Point(7, 7);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Padding = new Padding(0, 1, 0, 2);
            this.lblPrompt.Size = new Size(0x2d, 0x16);
            this.lblPrompt.TabIndex = 3;
            this.lblPrompt.Text = "Name";
            this.textBox.Dock = DockStyle.Top;
            this.textBox.Location = new Point(7, 0x1d);
            this.textBox.Name = "textBox";
            this.textBox.Size = new Size(390, 0x19);
            this.textBox.TabIndex = 0;
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x194, 0x6d);
            base.ControlBox = false;
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.textBox);
            base.Controls.Add(this.lblPrompt);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Margin = new Padding(3, 4, 3, 4);
            base.Name = "TextBoxForm";
            base.Padding = new Padding(7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "LINQPad";
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            base.ClientSize = new Size(base.ClientSize.Width, this.panOKCancel.Bottom + base.Padding.Bottom);
        }

        public string UserText
        {
            get
            {
                return this.textBox.Text;
            }
            set
            {
                this.textBox.Text = value;
            }
        }
    }
}

