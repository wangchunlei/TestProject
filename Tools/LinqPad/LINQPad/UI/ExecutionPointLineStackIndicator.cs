namespace LINQPad.UI
{
    using System;
    using System.Drawing;

    internal class ExecutionPointLineStackIndicator : BitmapBookmarkLineIndicator
    {
        private static System.Drawing.Bitmap _bmp = Resources.ExecutionPointStack;

        public ExecutionPointLineStackIndicator() : base("ExecutionPointStack", 4)
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

