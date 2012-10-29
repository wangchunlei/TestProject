namespace LINQPad.ObjectGraph
{
    using System;

    [MetaGraphNode]
    internal class ImageBlob
    {
        public readonly byte[] Data;

        public ImageBlob(byte[] data)
        {
            this.Data = data;
        }
    }
}

