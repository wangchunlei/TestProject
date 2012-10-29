namespace LINQPad.ObjectModel
{
    using LINQPad.ObjectGraph;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class InbuiltSampleQuery : Query
    {
        private object _openLink;
        private string _resourceName;
        private const string _samplesPrefix = "LINQPad.SampleQueries4.";

        private InbuiltSampleQuery(string resourceName)
        {
            base.SetDisplayPath(GetName(resourceName));
            this._resourceName = resourceName;
            this._openLink = new SampleCommandLink(this._resourceName, base.Name);
        }

        internal static IEnumerable<Query> GetAll()
        {
            return (from s in typeof(Query).Assembly.GetManifestResourceNames()
                where s.StartsWith("LINQPad.SampleQueries4.", StringComparison.Ordinal)
                orderby s
                select new InbuiltSampleQuery(s));
        }

        protected override string GetData()
        {
            using (Stream stream = typeof(Query).Assembly.GetManifestResourceStream(this._resourceName))
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }

        private static string GetName(string resourceName)
        {
            string str = resourceName.Substring("LINQPad.SampleQueries4.".Length);
            if (str.ToLowerInvariant().EndsWith(".linq"))
            {
                str = str.Substring(0, str.Length - 5);
            }
            string[] strArray = (from s in str.Replace("_", " ").Replace("CSharp", "C#").Replace("HYPHEN", "-").Replace("BANG", "!").Split(new char[] { '.' }) select s.Substring(3).Replace("POINT", ".").Trim()).ToArray<string>();
            return string.Join(@"\", strArray);
        }

        internal override void Open()
        {
            MainForm.Instance.OpenSampleQuery(this._resourceName);
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

