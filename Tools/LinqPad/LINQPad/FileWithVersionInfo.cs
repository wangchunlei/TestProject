namespace LINQPad
{
    using System;
    using System.IO;

    internal class FileWithVersionInfo
    {
        public readonly string FilePath;
        public readonly DateTime LastWriteTimeUtc;
        public readonly long Length;

        public FileWithVersionInfo(string filePath)
        {
            this.FilePath = filePath;
            FileInfo info = new FileInfo(this.FilePath);
            this.LastWriteTimeUtc = info.LastWriteTimeUtc;
            this.Length = info.Length;
        }

        public FileWithVersionInfo(string filePath, DateTime lastWriteTimeUtc, long length)
        {
            this.FilePath = filePath;
            this.LastWriteTimeUtc = lastWriteTimeUtc;
            this.Length = length;
        }

        public override bool Equals(object obj)
        {
            FileWithVersionInfo info = obj as FileWithVersionInfo;
            if (info == null)
            {
                return false;
            }
            return ((string.Equals(this.FilePath, info.FilePath, StringComparison.InvariantCultureIgnoreCase) && (this.LastWriteTimeUtc == info.LastWriteTimeUtc)) && (this.Length == info.Length));
        }

        public override int GetHashCode()
        {
            return (this.FilePath.GetHashCode() + (0x1f * this.LastWriteTimeUtc.GetHashCode()));
        }

        public override string ToString()
        {
            return this.FilePath;
        }
    }
}

