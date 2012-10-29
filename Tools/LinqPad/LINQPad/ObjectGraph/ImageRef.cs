namespace LINQPad.ObjectGraph
{
    using LINQPad;
    using System;
    using System.IO;

    [MetaGraphNode]
    internal class ImageRef
    {
        public readonly System.Uri Uri;

        public ImageRef(System.Uri uri)
        {
            this.Uri = uri;
        }

        public byte[] Download()
        {
            if (this.Uri.IsFile)
            {
                return File.ReadAllBytes(this.Uri.LocalPath);
            }
            return WebHelper.GetWebClient().DownloadData(this.Uri);
        }
    }
}

