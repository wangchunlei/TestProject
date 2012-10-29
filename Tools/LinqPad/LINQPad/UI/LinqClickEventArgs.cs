namespace LINQPad.UI
{
    using System;

    internal class LinqClickEventArgs : EventArgs
    {
        public readonly System.Uri Uri;

        public LinqClickEventArgs(System.Uri uri)
        {
            this.Uri = uri;
        }
    }
}

