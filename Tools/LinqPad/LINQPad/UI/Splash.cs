namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class Splash : Form
    {
        private IContainer components = null;
        private Label label2;
        private Label lblLoading;
        private Label lblTitle;
        private Label lblVersion;
        private Panel panText;
        private PictureBox pictureBox;

        public Splash()
        {
            this.InitializeComponent();
            try
            {
                this.lblTitle.Font = new Font("Verdana", 19.8f, FontStyle.Bold);
            }
            catch
            {
            }
            this.lblVersion.Text = this.lblVersion.Text + Program.VersionString;
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
            this.pictureBox = new PictureBox();
            this.panText = new Panel();
            this.lblVersion = new Label();
            this.label2 = new Label();
            this.lblTitle = new Label();
            this.lblLoading = new Label();
            ((ISupportInitialize) this.pictureBox).BeginInit();
            this.panText.SuspendLayout();
            base.SuspendLayout();
            this.pictureBox.BackColor = Color.Transparent;
            this.pictureBox.Dock = DockStyle.Left;
            this.pictureBox.Image = Resources.LINQPadLarge;
            this.pictureBox.Location = new Point(8, 8);
            this.pictureBox.Margin = new Padding(2, 2, 2, 2);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new Size(0x9b, 0x9e);
            this.pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 4;
            this.pictureBox.TabStop = false;
            this.panText.BackColor = Color.Transparent;
            this.panText.Controls.Add(this.lblVersion);
            this.panText.Controls.Add(this.label2);
            this.panText.Controls.Add(this.lblTitle);
            this.panText.Dock = DockStyle.Fill;
            this.panText.Location = new Point(0xa3, 8);
            this.panText.Margin = new Padding(2, 2, 2, 2);
            this.panText.Name = "panText";
            this.panText.Padding = new Padding(30, 0x20, 30, 0x20);
            this.panText.Size = new Size(0xf2, 0x9e);
            this.panText.TabIndex = 8;
            this.lblVersion.AutoSize = true;
            this.lblVersion.Dock = DockStyle.Top;
            this.lblVersion.ForeColor = Color.FromArgb(0x40, 0x40, 0x40);
            this.lblVersion.Location = new Point(30, 0x4d);
            this.lblVersion.Margin = new Padding(2, 0, 2, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new Size(0x36, 30);
            this.lblVersion.TabIndex = 9;
            this.lblVersion.Text = "\r\n Version ";
            this.lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            this.label2.AutoSize = true;
            this.label2.Dock = DockStyle.Top;
            this.label2.ForeColor = Color.FromArgb(0x40, 0x40, 0x40);
            this.label2.Location = new Point(30, 0x2f);
            this.label2.Margin = new Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0xad, 30);
            this.label2.TabIndex = 8;
            this.label2.Text = "\r\n \x00a9 2007-2012 Joseph Albahari";
            this.label2.TextAlign = ContentAlignment.MiddleCenter;
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = DockStyle.Top;
            this.lblTitle.ForeColor = Color.FromArgb(50, 50, 200);
            this.lblTitle.Location = new Point(30, 0x20);
            this.lblTitle.Margin = new Padding(2, 0, 2, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(0x39, 15);
            this.lblTitle.TabIndex = 7;
            this.lblTitle.Text = "LINQPad";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.lblLoading.AutoSize = true;
            this.lblLoading.BackColor = Color.Transparent;
            this.lblLoading.Dock = DockStyle.Bottom;
            this.lblLoading.ForeColor = Color.FromArgb(0x40, 0x40, 0x40);
            this.lblLoading.Location = new Point(8, 0xa6);
            this.lblLoading.Margin = new Padding(2, 0, 2, 0);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new Size(0x3d, 15);
            this.lblLoading.TabIndex = 10;
            this.lblLoading.Text = "Loading...";
            this.lblLoading.TextAlign = ContentAlignment.MiddleCenter;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            base.ClientSize = new Size(0x19d, 0xbd);
            base.ControlBox = false;
            base.Controls.Add(this.panText);
            base.Controls.Add(this.pictureBox);
            base.Controls.Add(this.lblLoading);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.Margin = new Padding(2, 2, 2, 2);
            base.Name = "Splash";
            base.Padding = new Padding(8, 8, 8, 8);
            base.StartPosition = FormStartPosition.CenterScreen;
            ((ISupportInitialize) this.pictureBox).EndInit();
            this.panText.ResumeLayout(false);
            this.panText.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Rectangle clientRectangle = base.ClientRectangle;
            if ((clientRectangle.Width > 0) && (clientRectangle.Height > 0))
            {
                using (Brush brush = new LinearGradientBrush(clientRectangle, Color.FromArgb(220, 220, 220), Color.White, 225f))
                {
                    e.Graphics.FillRectangle(brush, clientRectangle);
                }
            }
        }

        internal void UpdateMessage(string msg)
        {
            MethodInvoker method = delegate {
                try
                {
                    this.lblLoading.Text = msg;
                    this.lblLoading.Update();
                }
                catch
                {
                }
            };
            if (!base.IsDisposed)
            {
                try
                {
                    if (base.InvokeRequired)
                    {
                        base.BeginInvoke(method);
                    }
                    else
                    {
                        method();
                    }
                }
                catch
                {
                }
            }
        }
    }
}

