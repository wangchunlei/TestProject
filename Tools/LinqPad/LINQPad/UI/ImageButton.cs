namespace LINQPad.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ImageButton : Button
    {
        private bool _pressed;

        public ImageButton()
        {
            if (!Application.RenderWithVisualStyles)
            {
                base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                this._pressed = true;
                base.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this._pressed = false;
            base.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Application.RenderWithVisualStyles)
            {
                e.Graphics.Clear(SystemColors.Control);
                ControlPaint.DrawBorder3D(e.Graphics, base.ClientRectangle, this._pressed ? Border3DStyle.Sunken : Border3DStyle.Raised);
                Rectangle clientRectangle = base.ClientRectangle;
                if (clientRectangle.Width > base.Image.Width)
                {
                    clientRectangle.Inflate((base.Image.Width - clientRectangle.Width) / 2, 0);
                }
                if (clientRectangle.Height > base.Image.Height)
                {
                    clientRectangle.Inflate(0, (base.Image.Height - clientRectangle.Height) / 2);
                }
                if (base.Enabled)
                {
                    e.Graphics.DrawImage(base.Image, clientRectangle);
                }
                else
                {
                    ControlPaint.DrawImageDisabled(e.Graphics, base.Image, clientRectangle.X, clientRectangle.Y, SystemColors.Control);
                }
            }
            else
            {
                base.OnPaint(e);
            }
        }
    }
}

