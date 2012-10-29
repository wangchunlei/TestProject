namespace LINQPad.UI
{
    using LINQPad.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;

    internal class PermissionsCheck : Form
    {
        private Button btnClose;
        private IContainer components = null;
        private Label lbl;
        private Panel panClose;

        private PermissionsCheck()
        {
            this.InitializeComponent();
            try
            {
                base.Icon = Resources.LINQPad;
            }
            catch
            {
            }
        }

        public static bool Demand()
        {
            try
            {
                new PermissionSet(PermissionState.Unrestricted).Demand();
                return true;
            }
            catch
            {
                new PermissionsCheck().ShowDialog();
                return false;
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
            this.panClose = new Panel();
            this.btnClose = new Button();
            this.lbl = new Label();
            this.panClose.SuspendLayout();
            base.SuspendLayout();
            this.panClose.Controls.Add(this.btnClose);
            this.panClose.Dock = DockStyle.Bottom;
            this.panClose.Location = new Point(5, 0x159);
            this.panClose.Margin = new Padding(2, 2, 2, 2);
            this.panClose.Name = "panClose";
            this.panClose.Size = new Size(0x1b0, 0x1c);
            this.panClose.TabIndex = 0;
            this.btnClose.DialogResult = DialogResult.OK;
            this.btnClose.Dock = DockStyle.Right;
            this.btnClose.Location = new Point(370, 0);
            this.btnClose.Margin = new Padding(2, 2, 2, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(0x3e, 0x1c);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.lbl.Dock = DockStyle.Fill;
            this.lbl.Location = new Point(5, 0x18);
            this.lbl.Margin = new Padding(2, 0, 2, 0);
            this.lbl.Name = "lbl";
            this.lbl.Size = new Size(0x1b0, 0x141);
            this.lbl.TabIndex = 1;
            this.lbl.Text = "LINQPad will not run directly from a URI or network share.\r\n\r\nPlease save LINQPad.exe to the local hard disk, and then re-run.";
            this.lbl.TextAlign = ContentAlignment.MiddleCenter;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x1ba, 0x17b);
            base.Controls.Add(this.lbl);
            base.Controls.Add(this.panClose);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Margin = new Padding(2, 2, 2, 2);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "PermissionsCheck";
            base.Padding = new Padding(5, 0x18, 5, 6);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "LINQPad";
            this.panClose.ResumeLayout(false);
            base.ResumeLayout(false);
        }
    }
}

