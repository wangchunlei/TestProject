namespace LINQPad.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    internal class SaveChanges : BaseForm
    {
        private Button btnCancel;
        private Button btnNo;
        private Button btnYes;
        private IContainer components = null;
        private Label label1;
        private ListBox lstItems;
        private TableLayoutPanel panOKCancel;

        public SaveChanges(IEnumerable<string> files)
        {
            this.InitializeComponent();
            this.lstItems.Items.AddRange(files.ToArray<string>());
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
            this.label1 = new Label();
            this.lstItems = new ListBox();
            this.panOKCancel = new TableLayoutPanel();
            this.btnCancel = new Button();
            this.btnNo = new Button();
            this.btnYes = new Button();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(9, 11);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 5);
            this.label1.Size = new Size(0xf5, 0x18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Save changes to the following queries?";
            this.lstItems.Dock = DockStyle.Fill;
            this.lstItems.FormattingEnabled = true;
            this.lstItems.IntegralHeight = false;
            this.lstItems.ItemHeight = 0x11;
            this.lstItems.Location = new Point(9, 0x23);
            this.lstItems.Name = "lstItems";
            this.lstItems.Size = new Size(0x155, 0xd3);
            this.lstItems.TabIndex = 1;
            this.lstItems.TabStop = false;
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnCancel, 2, 0);
            this.panOKCancel.Controls.Add(this.btnNo, 1, 0);
            this.panOKCancel.Controls.Add(this.btnYes, 0, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(9, 0xf6);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 7, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x155, 0x2a);
            this.panOKCancel.TabIndex = 0;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x105, 10);
            this.btnCancel.Margin = new Padding(3, 3, 1, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnNo.DialogResult = DialogResult.No;
            this.btnNo.Location = new Point(0xb0, 10);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new Size(0x4f, 0x1d);
            this.btnNo.TabIndex = 1;
            this.btnNo.Text = "&No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnYes.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnYes.DialogResult = DialogResult.Yes;
            this.btnYes.Location = new Point(0x5b, 10);
            this.btnYes.Margin = new Padding(0, 3, 3, 3);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new Size(0x4f, 0x1d);
            this.btnYes.TabIndex = 0;
            this.btnYes.Text = "&Yes";
            this.btnYes.UseVisualStyleBackColor = true;
            base.AcceptButton = this.btnYes;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x167, 0x127);
            base.ControlBox = false;
            base.Controls.Add(this.lstItems);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.label1);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "SaveChanges";
            base.Padding = new Padding(9, 11, 9, 7);
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "LINQPad";
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}

