namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Forms;

    internal abstract class BitmapBookmarkLineIndicator : BookmarkLineIndicator
    {
        public float Opacity;

        public BitmapBookmarkLineIndicator(string name, int displayPriority) : base(name, displayPriority, Color.Goldenrod, Color.Gold)
        {
            this.Opacity = 1f;
        }

        public override void DrawGlyph(PaintEventArgs e, Rectangle bounds)
        {
            if (this.Opacity > 0f)
            {
                bounds.Inflate(-1, -1);
                int width = bounds.Width;
                if (bounds.Height > width)
                {
                    bounds.Inflate(0, (width - bounds.Height) / 2);
                }
                if (this.Opacity == 1f)
                {
                    e.Graphics.DrawImage(this.Bitmap, bounds);
                }
                else
                {
                    ColorMatrix newColorMatrix = new ColorMatrix {
                        Matrix33 = this.Opacity
                    };
                    ImageAttributes imageAttr = new ImageAttributes();
                    imageAttr.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    e.Graphics.DrawImage(this.Bitmap, bounds, 0, 0, this.Bitmap.Width, this.Bitmap.Height, GraphicsUnit.Pixel, imageAttr);
                }
            }
        }

        protected abstract System.Drawing.Bitmap Bitmap { get; }
    }
}

