namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class WarningLineIndicator : BookmarkLineIndicator
    {
        public WarningLineIndicator() : base("Warning", 9, Color.Goldenrod, Color.Gold)
        {
        }

        public override void DrawGlyph(PaintEventArgs e, Rectangle bounds)
        {
            GraphicsPath path;
            Point[] pointArray;
            SolidBrush brush;
            int num = Math.Max(0, (bounds.Height - bounds.Width) / 2);
            bounds.Inflate(0, -num);
            bounds.Inflate(-2, -2);
            int num2 = Math.Min(bounds.Width, bounds.Height);
            int num3 = 2;
            int num4 = num2 / 4;
            int x = bounds.X + (bounds.Width / 2);
            int num6 = bounds.X + ((bounds.Width + 2) / 2);
            int num7 = num6 - 2;
            SmoothingMode smoothingMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            using (path = new GraphicsPath())
            {
                pointArray = new Point[] { new Point(bounds.Left + num3, bounds.Bottom), new Point(bounds.Right - num3, bounds.Bottom), new Point(bounds.Right, bounds.Bottom - num3), new Point(num6, bounds.Top), new Point(num7, bounds.Top), new Point(bounds.Left, bounds.Bottom - num3) };
                path.AddLines(pointArray);
                path.CloseAllFigures();
                using (brush = new SolidBrush(Color.FromArgb(120, Color.Gold)))
                {
                    using (Pen pen = new Pen(Color.FromArgb(150, Color.Goldenrod)))
                    {
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
            using (path = new GraphicsPath())
            {
                pointArray = new Point[] { new Point(num7, bounds.Top + num4), new Point(num6, bounds.Top + num4), new Point(x, bounds.Top + ((bounds.Height * 2) / 3)) };
                path.AddLines(pointArray);
                path.CloseAllFigures();
                using (brush = new SolidBrush(Color.FromArgb(40, 40, 40)))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }
            Rectangle rect = new Rectangle(x, bounds.Bottom - num4, 0, 0);
            rect.Inflate(1, 1);
            e.Graphics.FillEllipse(Brushes.Black, rect);
            e.Graphics.SmoothingMode = smoothingMode;
        }
    }
}

