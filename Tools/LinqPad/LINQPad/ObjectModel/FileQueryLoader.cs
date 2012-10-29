namespace LINQPad.ObjectModel
{
    using LINQPad.ObjectGraph;
    using LINQPad.UI;
    using System;
    using System.IO;

    internal class FileQueryLoader : LazyQueryLoader
    {
        private object _openLink;

        public FileQueryLoader(Query q) : base(q)
        {
            this._openLink = new FileCommandLink(q.FilePath, q.Name);
        }

        public override string GetData()
        {
            try
            {
                return File.ReadAllText(base.Query.FilePath);
            }
            catch
            {
                return "";
            }
        }

        public override void Open()
        {
            MainForm.Instance.OpenQuery(base.Query.FilePath, true);
        }

        public override object OpenLink
        {
            get
            {
                return this._openLink;
            }
        }
    }
}

