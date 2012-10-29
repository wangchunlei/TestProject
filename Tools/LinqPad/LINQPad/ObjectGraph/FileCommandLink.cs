namespace LINQPad.ObjectGraph
{
    using System;

    [MetaGraphNode]
    internal class FileCommandLink
    {
        public readonly string FilePath;
        public readonly string Text;

        public FileCommandLink(string filePath, string text)
        {
            this.FilePath = filePath;
            this.Text = text;
        }
    }
}

