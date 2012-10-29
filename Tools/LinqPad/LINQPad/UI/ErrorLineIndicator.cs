namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class ErrorLineIndicator : BookmarkLineIndicator
    {
        public ErrorLineIndicator() : base("Error", 10, Color.Maroon, Color.Red)
        {
        }

        public override void DrawGlyph(PaintEventArgs e, Rectangle bounds)
        {
            int num = Math.Max(0, (bounds.Width - bounds.Height) / 2);
            int num2 = Math.Max(0, (bounds.Height - bounds.Width) / 2);
            bounds.Inflate(-num, -num2);
            if (bounds.Width > bounds.Height)
            {
                bounds.Width--;
            }
            if (bounds.Height > bounds.Width)
            {
                bounds.Height--;
            }
            bounds.Inflate(-2, -2);
            int width = bounds.Width;
            int num4 = width / 3;
            SmoothingMode smoothingMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.FillEllipse(Brushes.Red, bounds);
            e.Graphics.DrawEllipse(Pens.Maroon, bounds);
            using (Pen pen = new Pen(Color.White, (float) Math.Max(2, width / 7)))
            {
                e.Graphics.DrawLine(pen, (int) (bounds.X + num4), (int) (bounds.Y + num4), (int) (bounds.Right - num4), (int) (bounds.Bottom - num4));
                e.Graphics.DrawLine(pen, (int) (bounds.Right - num4), (int) (bounds.Y + num4), (int) (bounds.X + num4), (int) (bounds.Bottom - num4));
            }
            e.Graphics.SmoothingMode = smoothingMode;
        }
    }
}

