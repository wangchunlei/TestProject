namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class ExceptionLineIndicator : BookmarkLineIndicator
    {
        public ExceptionLineIndicator() : base("Exception", 10, Color.Maroon, Color.Red)
        {
        }

        public override void DrawGlyph(PaintEventArgs e, Rectangle bounds)
        {
            GraphicsPath path;
            int num = Math.Max(0, (bounds.Width - bounds.Height) / 2);
            int num2 = Math.Max(0, (bounds.Height - bounds.Width) / 2);
            bounds.Inflate(-num, -num2);
            if ((bounds.Width % 2) == 1)
            {
                bounds.Width--;
            }
            if ((bounds.Height % 2) == 1)
            {
                bounds.Height--;
            }
            if (bounds.Width > bounds.Height)
            {
                bounds.Width = bounds.Height;
            }
            if (bounds.Height > bounds.Width)
            {
                bounds.Height = bounds.Width;
            }
            bounds.Inflate(-2, -2);
            int width = bounds.Width;
            int num4 = width / 3;
            int num5 = width / 4;
            int num6 = bounds.X + (bounds.Width / 2);
            float x = bounds.X + (0.5f * (bounds.Width + 3));
            float num8 = x - 3f;
            SmoothingMode smoothingMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            using (path = new GraphicsPath())
            {
                Point[] points = new Point[] { new Point(bounds.Left + num4, bounds.Bottom), new Point(bounds.Right - num4, bounds.Bottom), new Point(bounds.Right, bounds.Bottom - num4), new Point(bounds.Right, bounds.Top + num4), new Point(bounds.Right - num4, bounds.Top), new Point(bounds.Left + num4, bounds.Top), new Point(bounds.Left, bounds.Top + num4), new Point(bounds.Left, bounds.Bottom - num4) };
                path.AddLines(points);
                path.CloseAllFigures();
                e.Graphics.FillPath(Brushes.Red, path);
                e.Graphics.DrawPath(Pens.DarkGray, path);
            }
            using (path = new GraphicsPath())
            {
                PointF[] tfArray = new PointF[] { new PointF(num8, (float) ((bounds.Top + num5) - 1)), new PointF(x, (float) ((bounds.Top + num5) - 1)), new PointF(num6 + 1.2f, (-0.5f + bounds.Top) + ((bounds.Height * 2f) / 3f)), new PointF(num6 - 1.2f, (-0.5f + bounds.Top) + ((bounds.Height * 2f) / 3f)) };
                path.AddLines(tfArray);
                path.CloseAllFigures();
                e.Graphics.FillPath(Brushes.White, path);
            }
            RectangleF rect = new RectangleF((float) num6, (float) (bounds.Bottom - num5), 0f, 0f);
            rect.Inflate(1.5f, 1.5f);
            e.Graphics.FillEllipse(Brushes.White, rect);
            e.Graphics.SmoothingMode = smoothingMode;
        }
    }
}

