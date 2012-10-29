namespace LINQPad.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class PanelEx : Panel
    {
        private Color _borderColor;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.BorderColor != Color.Empty)
            {
                using (Pen pen = new Pen(this.BorderColor))
                {
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, base.ClientSize.Width - 1, base.ClientSize.Height - 1));
                }
            }
            base.OnPaint(e);
        }

        public Color BorderColor
        {
            get
            {
                return this._borderColor;
            }
            set
            {
                if (this._borderColor != value)
                {
                    this._borderColor = value;
                    base.ResizeRedraw = this.DoubleBuffered = this._borderColor != Color.Empty;
                    if (this._borderColor != Color.Empty)
                    {
                        base.Padding = new Padding(1);
                    }
                    else
                    {
                        base.Padding = new Padding(0);
                    }
                }
            }
        }
    }
}

