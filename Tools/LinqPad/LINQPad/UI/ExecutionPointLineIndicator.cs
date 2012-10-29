namespace LINQPad.UI
{
    using System;
    using System.Drawing;

    internal class ExecutionPointLineIndicator : BitmapBookmarkLineIndicator
    {
        private static System.Drawing.Bitmap _bmp = Resources.ExecutionPoint;

        public ExecutionPointLineIndicator() : base("ExecutionPoint", 5)
        {
        }

        protected override System.Drawing.Bitmap Bitmap
        {
            get
            {
                return _bmp;
            }
        }
    }
}

