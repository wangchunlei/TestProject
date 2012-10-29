namespace LINQPad.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class ClearButton : Control
    {
        private bool _checked;
        private ButtonGlyph _glyph;
        private bool _hot;
        private bool _pressed;
        private ToolTip _toolTip = new ToolTip();

        public ClearButton()
        {
            base.ResizeRedraw = true;
            base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void Dispose(bool disposing)
        {
            this._toolTip.Dispose();
        }

        public static void DrawCloseGlyph(Graphics g, Color color, Rectangle bounds, bool limitSize, bool addMargin)
        {
            Rectangle rectangle = GetButtonGlyphBounds(bounds, limitSize, addMargin, 2, 1);
            if ((rectangle.Width >= 5) && (rectangle.Height >= 5))
            {
                using (Pen pen = new Pen(color, 2f))
                {
                    g.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
                    g.DrawLine(pen, rectangle.Right, rectangle.Top, rectangle.Left, rectangle.Bottom);
                }
            }
        }

        public static void DrawLeftTriangle(Graphics g, Color color, Rectangle bounds, bool limitSize, bool addMargin)
        {
            Rectangle rectangle = GetButtonGlyphBounds(bounds, limitSize, addMargin, 2, 0);
            if ((rectangle.Width >= 5) && (rectangle.Height >= 5))
            {
                rectangle.Width = (rectangle.Width * 4) / 5;
                rectangle.X += rectangle.Width / 6;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(color))
                {
                    Point[] points = new Point[] { new Point(rectangle.Left, rectangle.Top + (rectangle.Height / 2)), new Point(rectangle.Right, rectangle.Top), new Point(rectangle.Right, rectangle.Bottom) };
                    g.FillPolygon(brush, points);
                }
            }
        }

        public static void DrawPinGlyph(Graphics g, Color color, Rectangle bounds, bool limitSize, bool addMargin)
        {
            Rectangle rect = GetButtonGlyphBounds(bounds, limitSize, addMargin, 2, 1);
            if ((rect.Width >= 5) && (rect.Height >= 5))
            {
                Pen pen;
                if (rect.Width == 11)
                {
                    rect.Inflate(-1, -1);
                }
                using (pen = new Pen(color, 1f))
                {
                    g.DrawRectangle(pen, (int) (rect.Left + (rect.Width / 6)), (int) (rect.Top - 2), (int) ((rect.Width * 4) / 6), (int) (((rect.Height * 3) / 4) + 2));
                    g.DrawLine(pen, (int) (rect.Left + (rect.Width / 2)), (int) (rect.Top + ((rect.Height * 3) / 4)), (int) (rect.Left + (rect.Width / 2)), (int) (rect.Bottom + 1));
                }
                using (pen = new Pen(color, 2f))
                {
                    g.DrawLine(pen, rect.Left, rect.Top + ((rect.Height * 3) / 4), rect.Right, rect.Top + ((rect.Height * 3) / 4));
                }
                rect = new Rectangle(rect.Left + (rect.Width / 3), rect.Top - 2, (rect.Width * 3) / 6, ((rect.Height * 3) / 4) + 2);
                using (Brush brush = new LinearGradientBrush(rect, Color.Transparent, Color.Black, 0f))
                {
                    g.FillRectangle(brush, rect);
                }
            }
        }

        private static Rectangle GetButtonGlyphBounds(Rectangle r, bool limitSize, bool addMargin, int mod, int remainder)
        {
            int num = Math.Min(r.Width, r.Height);
            if (addMargin)
            {
                num -= Math.Max(8, num / 2);
            }
            if (limitSize)
            {
                num = Math.Min(num, (SystemInformation.CaptionButtonSize.Height * 3) / 5);
            }
            if (mod > 0)
            {
                int num2 = num % mod;
                if (num2 != remainder)
                {
                    num -= num2 - remainder;
                }
                if (num2 < remainder)
                {
                    num -= mod;
                }
            }
            return new Rectangle(r.X + ((r.Width - num) / 2), r.Y + ((r.Height - num) / 2), num, num);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this._pressed = true;
            base.Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this._hot = true;
            base.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this._hot = false;
            base.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this._pressed = false;
            base.Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle clientRectangle = base.ClientRectangle;
            if (this._hot || this._checked)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(0xde, 0xec, 0xff)))
                {
                    e.Graphics.FillRectangle(brush, clientRectangle);
                }
            }
            if (this.Image != null)
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                Rectangle rect = base.ClientRectangle;
                if (!((!this.NoImageScale || (rect.Width <= this.Image.Width)) ? (rect.Height <= this.Image.Height) : false))
                {
                    rect.X += (rect.Width - this.Image.Width) / 2;
                    rect.Y += (rect.Height - this.Image.Height) / 2;
                    rect.Width = this.Image.Width;
                    rect.Height = this.Image.Height;
                }
                if (base.Enabled)
                {
                    e.Graphics.DrawImage(this.Image, rect);
                    goto Label_01E1;
                }
                using (System.Drawing.Image image = this.Image.GetThumbnailImage(clientRectangle.Width, clientRectangle.Height, null, IntPtr.Zero))
                {
                    ControlPaint.DrawImageDisabled(e.Graphics, image, 0, 0, SystemColors.Control);
                    goto Label_01E1;
                }
            }
            if (this.Glyph == ButtonGlyph.Close)
            {
                DrawCloseGlyph(e.Graphics, SystemColors.WindowText, clientRectangle, true, true);
            }
            else if (this.Glyph == ButtonGlyph.Pin)
            {
                DrawPinGlyph(e.Graphics, SystemColors.WindowText, clientRectangle, true, true);
            }
            else if (this.Glyph == ButtonGlyph.LeftTriangle)
            {
                DrawLeftTriangle(e.Graphics, SystemColors.WindowText, clientRectangle, true, true);
            }
        Label_01E1:
            if ((this._hot || this._pressed) || this._checked)
            {
                ControlPaint.DrawBorder3D(e.Graphics, clientRectangle, (this._pressed || this._checked) ? Border3DStyle.SunkenOuter : Border3DStyle.RaisedOuter);
            }
            base.OnPaint(e);
        }

        public bool Checked
        {
            get
            {
                return this._checked;
            }
            set
            {
                this._checked = value;
                base.Invalidate();
            }
        }

        [DefaultValue(0)]
        public ButtonGlyph Glyph
        {
            get
            {
                return this._glyph;
            }
            set
            {
                this._glyph = value;
                base.Invalidate();
            }
        }

        [DefaultValue((string) null)]
        public System.Drawing.Image Image { get; set; }

        public bool NoImageScale { get; set; }

        public string ToolTipText
        {
            get
            {
                return this._toolTip.GetToolTip(this);
            }
            set
            {
                this._toolTip.SetToolTip(this, value);
            }
        }
    }
}

