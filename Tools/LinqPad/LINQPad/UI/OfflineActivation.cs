namespace LINQPad.UI
{
    using ActiproBridge;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class OfflineActivation : BaseForm
    {
        private bool _forAll;
        private Button btnActivate;
        private Button btnCancel;
        private IContainer components = null;
        private Label label1;
        private TableLayoutPanel panOKCancel;
        private TextBox txtCode;

        public OfflineActivation(bool forAll)
        {
            this._forAll = forAll;
            this.InitializeComponent();
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            this.btnCancel.Enabled = false;
            this.txtCode.Enabled = false;
            this.btnActivate.Enabled = false;
            base.Update();
            LicenseManager.Licensee = null;
            WSAgent.RegisterOffline(this.txtCode.Text, this._forAll);
            if (!string.IsNullOrEmpty(LicenseManager.Licensee))
            {
                MessageBox.Show("Activation Successful");
                base.Close();
            }
            else
            {
                MessageBox.Show("Invalid Activation Code");
            }
            this.btnCancel.Enabled = true;
            this.txtCode.Enabled = true;
            this.btnActivate.Enabled = true;
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
            this.btnCancel = new Button();
            this.btnActivate = new Button();
            this.label1 = new Label();
            this.txtCode = new TextBox();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnCancel, 2, 0);
            this.panOKCancel.Controls.Add(this.btnActivate, 1, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(6, 210);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 3, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x1ac, 0x27);
            this.panOKCancel.TabIndex = 2;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x15c, 6);
            this.btnCancel.Margin = new Padding(3, 3, 1, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnActivate.Enabled = false;
            this.btnActivate.Location = new Point(0x107, 6);
            this.btnActivate.Name = "btnActivate";
            this.btnActivate.Size = new Size(0x4f, 30);
            this.btnActivate.TabIndex = 1;
            this.btnActivate.Text = "Activate";
            this.btnActivate.UseVisualStyleBackColor = true;
            this.btnActivate.Click += new EventHandler(this.btnActivate_Click);
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 3);
            this.label1.Size = new Size(0x29, 0x16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Code";
            this.txtCode.Dock = DockStyle.Fill;
            this.txtCode.Location = new Point(6, 0x1d);
            this.txtCode.Multiline = true;
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new Size(0x1ac, 0xb5);
            this.txtCode.TabIndex = 1;
            this.txtCode.TextChanged += new EventHandler(this.txtCode_TextChanged);
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(440, 0x100);
            base.ControlBox = false;
            base.Controls.Add(this.txtCode);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.panOKCancel);
            base.Name = "OfflineActivation";
            base.Padding = new Padding(6, 7, 6, 7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Offline Activation";
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            this.btnActivate.Enabled = this.txtCode.Text.Length > 1;
        }
    }
}

