namespace LINQPad.UI
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class DockHandle : Control
    {
        private bool _mouseIn;
        private static StringFormat VerticalFormat = new StringFormat(StringFormatFlags.DirectionVertical);

        public DockHandle()
        {
            base.ResizeRedraw = true;
        }

        protected override void OnClick(EventArgs e)
        {
            MainForm.Instance.ToggleExplorerVisibility();
            base.OnClick(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = (this.Font.Height * 150) / 100;
            if (base.Width != num)
            {
                base.Width = num;
            }
            base.OnLayout(levent);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this._mouseIn = true;
            base.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this._mouseIn = false;
            base.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(Point.Empty, base.Size);
            Color color = this._mouseIn ? Color.FromArgb(170, 220, 0xf5) : Color.FromArgb(210, 210, 210);
            Color color2 = this._mouseIn ? Color.FromArgb(0xeb, 0xf5, 0xff) : Color.FromArgb(230, 230, 230);
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, color, color2, 0f))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
            string text = "Show Explorer Panels  (Shift+F8)";
            int num = Convert.ToInt32(e.Graphics.MeasureString(text, this.Font, base.Width, VerticalFormat).Height);
            e.Graphics.DrawString(text, this.Font, Brushes.Black, (float) (base.Width / 7), (float) ((base.Height - num) / 2), VerticalFormat);
            base.OnPaint(e);
        }
    }
}

