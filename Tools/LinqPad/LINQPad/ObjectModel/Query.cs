namespace LINQPad.ObjectModel
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;

    public class Query : ICustomMemberProvider
    {
        private QueryLanguage _language;
        private XElement _metaData;
        private bool _populated;
        private LINQPad.QueryCore _queryCore;
        private string _text = "";
        private static Regex _validQueryHeader = new Regex(@"(?i)^\s*<query");
        internal bool IsValid;

        protected Query()
        {
        }

        public object FormatMatches(MatchCollection matches)
        {
            string[] strArray;
            if ((matches == null) || (matches.Count == 0))
            {
                return null;
            }
            List<object> elements = new List<object>();
            int startIndex = 0;
            using (IEnumerator enumerator = matches.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Match current = (Match) enumerator.Current;
                    string str = this.Text.Substring(startIndex, current.Index - startIndex).Replace("\t", " ").Replace("\r", "");
                    if (str.Length > 0)
                    {
                        strArray = str.Split(new char[] { '\n' }).Select<string, string>(((Func<string, int, string>) ((l, n) => ((n == 0) ? l : l.TrimStart(new char[0]))))).ToArray<string>();
                        if ((strArray.Length > 4) && (startIndex > 0))
                        {
                            strArray = strArray.Take<string>(2).Concat<string>("...".Split(new char[0])).Concat<string>(strArray.Skip<string>((strArray.Length - 2)).Take<string>(2)).ToArray<string>();
                        }
                        else if ((strArray.Length > 2) && (startIndex == 0))
                        {
                            strArray = strArray.Skip<string>((strArray.Length - 2)).Take<string>(2).ToArray<string>();
                            if (strArray.First<string>().Trim().Length == 0)
                            {
                                strArray = strArray.Skip<string>(1).ToArray<string>();
                            }
                        }
                        elements.Add(string.Join("\r\n", strArray));
                    }
                    elements.Add(Util.Highlight(current.Value));
                    startIndex = current.Index + current.Length;
                    if (elements.Count > 6)
                    {
                        goto Label_01B9;
                    }
                }
                goto Label_01E8;
            Label_01B9:
                elements.Add("...");
                return Util.WordRun(false, elements);
            }
        Label_01E8:
            if (startIndex < this.Text.Length)
            {
                strArray = this.Text.Substring(startIndex).Replace("\t", " ").Replace("\r", "").Split(new char[] { '\n' }).Select<string, string>(((Func<string, int, string>) ((l, n) => ((n == 0) ? l : l.TrimStart(new char[0]))))).ToArray<string>();
                elements.Add(string.Join("\r\n", strArray.Take<string>(2).ToArray<string>()));
            }
            return Util.WordRun(false, elements);
        }

        public IConnectionInfo GetConnectionInfo()
        {
            if (this.HasQueryCore)
            {
                return this.QueryCore.Repository;
            }
            XElement store = this.MetaData.Element("Connection");
            if (store == null)
            {
                return null;
            }
            return new Repository(store);
        }

        protected virtual string GetData()
        {
            return null;
        }

        internal static IEnumerable<Query> GetSamples()
        {
            return InbuiltSampleQuery.GetAll().Concat<Query>(ImportedSampleQuery.GetAll());
        }

        IEnumerable<string> ICustomMemberProvider.GetNames()
        {
            return new string[] { "OpenLink", "Location", "Text", "Language", "FileReferences", "GacReferences", "NamespaceImports", "LastModified" };
        }

        IEnumerable<Type> ICustomMemberProvider.GetTypes()
        {
            return new Type[] { typeof(object), typeof(string), typeof(string), typeof(QueryLanguage), typeof(IEnumerable<string>), typeof(IEnumerable<string>), typeof(IEnumerable<string>), typeof(DateTime?) };
        }

        IEnumerable<object> ICustomMemberProvider.GetValues()
        {
            return new object[] { this.OpenLink, this.Location, (((this.Text == null) || (this.Text.Length <= 50)) ? this.Text : (this.Text.Substring(0, 0x2d) + "...")), this.Language, this.FileReferences, this.GacReferences, this.NamespaceImports, this.LastModified };
        }

        internal virtual void Open()
        {
        }

        private void Populate()
        {
            if (!this._populated && !this.HasQueryCore)
            {
                this.IsValid = this.Populate(this.GetData());
                this._populated = true;
            }
        }

        private bool Populate(string data)
        {
            XElement element;
            int lineNumber;
            if (!_validQueryHeader.IsMatch(data))
            {
                return false;
            }
            try
            {
                XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.None);
                Stream xmlFragment = new MemoryStream(Encoding.UTF8.GetBytes(data));
                XmlTextReader reader = new XmlTextReader(xmlFragment, XmlNodeType.Element, context);
                reader.MoveToContent();
                XmlReader reader2 = reader.ReadSubtree();
                StringBuilder output = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(output))
                {
                    writer.WriteNode(reader2, true);
                }
                element = XElement.Parse(output.ToString());
                lineNumber = reader.LineNumber;
            }
            catch (XmlException)
            {
                return false;
            }
            if (element.Attribute("Kind") != null)
            {
                try
                {
                    this._language = (QueryLanguage) Enum.Parse(typeof(QueryLanguage), (string) element.Attribute("Kind"), true);
                }
                catch (ArgumentException)
                {
                }
            }
            this._metaData = element;
            StringReader reader3 = new StringReader(data);
            for (int i = 0; i < lineNumber; i++)
            {
                reader3.ReadLine();
            }
            this._text = reader3.ReadToEnd().Trim();
            return true;
        }

        protected void SetDisplayPath(string displayPath)
        {
            this.Location = Path.GetDirectoryName(displayPath);
            this.Name = Path.GetFileNameWithoutExtension(displayPath);
        }

        internal virtual LINQPad.QueryCore ToQueryCore()
        {
            return new LINQPad.QueryCore { Source = this.Text, QueryKind = this.Language, AdditionalReferences = this.FileReferences.ToArray<string>(), AdditionalGACReferences = this.GacReferences.ToArray<string>(), AdditionalNamespaces = this.NamespaceImports.ToArray<string>(), Repository = (Repository) this.GetConnectionInfo() };
        }

        public virtual string FilePath { get; protected set; }

        public IEnumerable<string> FileReferences
        {
            get
            {
                if (this.HasQueryCore)
                {
                    return this.QueryCore.AdditionalReferences;
                }
                return (from e in this.MetaData.Elements("Reference") select PathHelper.DecodeFolder(e.Value));
            }
            set
            {
                this.QueryCore.AdditionalReferences = (value ?? ((IEnumerable<string>) new string[0])).ToArray<string>();
            }
        }

        public IEnumerable<string> GacReferences
        {
            get
            {
                if (this.HasQueryCore)
                {
                    return this.QueryCore.AdditionalGACReferences;
                }
                return (from e in this.MetaData.Elements("GACReference") select e.Value);
            }
            set
            {
                this.QueryCore.AdditionalGACReferences = (value ?? ((IEnumerable<string>) new string[0])).ToArray<string>();
            }
        }

        protected bool HasQueryCore
        {
            get
            {
                return (this._queryCore != null);
            }
        }

        public bool IsCSharp
        {
            get
            {
                return (this.Language <= QueryLanguage.Program);
            }
        }

        public bool IsSQL
        {
            get
            {
                return ((this.Language == QueryLanguage.SQL) || (this.Language == QueryLanguage.ESQL));
            }
        }

        public bool IsVB
        {
            get
            {
                return ((this.Language >= QueryLanguage.VBExpression) && (this.Language <= QueryLanguage.VBProgram));
            }
        }

        public QueryLanguage Language
        {
            get
            {
                if (this.HasQueryCore)
                {
                    return this.QueryCore.QueryKind;
                }
                this.Populate();
                return this._language;
            }
            set
            {
                this.QueryCore.QueryKind = value;
            }
        }

        public virtual DateTime? LastModified
        {
            get
            {
                return null;
            }
        }

        public string Location { get; private set; }

        private XElement MetaData
        {
            get
            {
                this.Populate();
                return this._metaData;
            }
        }

        public string Name { get; private set; }

        public IEnumerable<string> NamespaceImports
        {
            get
            {
                if (this.HasQueryCore)
                {
                    return this.QueryCore.AdditionalNamespaces;
                }
                return (from e in this.MetaData.Elements("Namespace") select e.Value);
            }
            set
            {
                this.QueryCore.AdditionalNamespaces = (value ?? ((IEnumerable<string>) new string[0])).ToArray<string>();
            }
        }

        public virtual object OpenLink
        {
            get
            {
                return null;
            }
        }

        internal LINQPad.QueryCore QueryCore
        {
            get
            {
                if (this._queryCore != null)
                {
                    return this._queryCore;
                }
                this.Populate();
                this._metaData = null;
                return (this._queryCore = this.ToQueryCore());
            }
        }

        public string Text
        {
            get
            {
                if (this.HasQueryCore)
                {
                    return this.QueryCore.Source;
                }
                this.Populate();
                return this._text;
            }
            set
            {
                this.QueryCore.Source = value;
            }
        }
    }
}

