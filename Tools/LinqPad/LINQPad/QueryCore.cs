namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;

    internal class QueryCore : IDisposable
    {
        private string[] _additionalGACReferences = new string[0];
        private string[] _additionalNamespaces = new string[0];
        private string[] _additionalReferences = new string[0];
        private object _autoSaveLock = new object();
        private AutoSaver.AutoSaveToken _autoSaver;
        private string _filePath = "";
        private bool _includePredicateBuilder;
        private bool _isDisposed;
        private bool _isModified;
        private bool _isMyExtensions;
        private string _lastCustomDataContextAssembly;
        private DateTime _lastCustomDataContextWrite;
        private string[] _lastIndirectRefs;
        private bool _modifiedSinceLastAutoSave;
        private string _name = "";
        private QueryChangedEventArgs _pendingQueryChangedArgs;
        private bool _pinned = true;
        private QueryLanguage _queryKind;
        private LINQPad.Repository _repository;
        private bool _requiresRecompilation = true;
        private string _source = "";
        private int _suspendEventCount;
        private bool _toDataGrids;
        private static Regex _validQueryHeader = new Regex(@"(?i)^\s*<query");
        internal object UISource;

        public event EventHandler<QueryChangedEventArgs> QueryChanged;

        public void AddRefIfNotPresent(bool replaceIfPresent, params string[] items)
        {
            List<string> source = this.AdditionalReferences.ToList<string>();
            foreach (string str in items)
            {
                Predicate<string> match = null;
                Func<string, bool> predicate = null;
                string file;
                if (File.Exists(str))
                {
                    file = Path.GetFileName(str);
                    if (file.ToLower() != "mscorlib.dll")
                    {
                        if (replaceIfPresent)
                        {
                            if (match == null)
                            {
                                match = r => string.Equals(Path.GetFileName(r), file, StringComparison.InvariantCultureIgnoreCase);
                            }
                            source.RemoveAll(match);
                        }
                        else
                        {
                            if (predicate == null)
                            {
                                predicate = r => string.Equals(Path.GetFileName(r), file, StringComparison.InvariantCultureIgnoreCase);
                            }
                            if (source.Any<string>(predicate))
                            {
                                continue;
                            }
                        }
                        if (!QueryCompiler.DefaultReferences.Any<string>(r => string.Equals(r, file, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            source.Add(str);
                        }
                    }
                }
            }
            this.AdditionalReferences = source.ToArray();
        }

        public void AutoSave(bool evenIfRecentlySaved)
        {
            lock (this._autoSaveLock)
            {
                if (((!this._isDisposed && this.IsModified) && this._modifiedSinceLastAutoSave) && (this.Source.Length >= 2))
                {
                    if (this._autoSaver == null)
                    {
                        this._autoSaver = new AutoSaver.AutoSaveToken(this);
                    }
                    this._autoSaver.Save(evenIfRecentlySaved);
                    this._modifiedSinceLastAutoSave = false;
                }
            }
        }

        internal void ClearAutoSave()
        {
            if (this._autoSaver != null)
            {
                this._autoSaver.Dispose();
                this._autoSaver = null;
            }
            this._modifiedSinceLastAutoSave = false;
        }

        public virtual void Dispose()
        {
            this.QueryChanged = null;
            this._isDisposed = true;
        }

        internal void ForceCxRefresh()
        {
            this.OnQueryChanged(false, true);
        }

        public DataContextDriver GetDriver(bool nullIfError)
        {
            if (this.Repository == null)
            {
                return null;
            }
            if (!(!nullIfError || this.Repository.DriverLoader.IsValid))
            {
                return null;
            }
            try
            {
                return this.Repository.DriverLoader.Driver;
            }
            catch
            {
                if (!nullIfError)
                {
                    throw;
                }
                return null;
            }
        }

        private XElement GetFileReferenceElement(string queryPath, string refPath)
        {
            XElement element = new XElement("Reference", PathHelper.EncodeFolder(refPath));
            if (!string.IsNullOrEmpty(queryPath))
            {
                try
                {
                    string str = Path.GetPathRoot(queryPath).ToUpperInvariant();
                    string str2 = Path.GetPathRoot(refPath).ToUpperInvariant();
                    if (str != str2)
                    {
                        return element;
                    }
                    if (queryPath.Substring(str.Length).Split(@"/\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First<string>().ToLowerInvariant() != refPath.Substring(str2.Length).Split(@"/\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First<string>().ToLowerInvariant())
                    {
                        return element;
                    }
                    string str3 = Uri.UnescapeDataString(new Uri(queryPath).MakeRelativeUri(new Uri(refPath)).ToString()).Replace('/', '\\');
                    if (!(string.IsNullOrEmpty(str3) || Path.IsPathRooted(str3)))
                    {
                        element.SetAttributeValue("Relative", str3);
                    }
                }
                catch (Exception exception)
                {
                    Log.Write(exception);
                }
            }
            return element;
        }

        public string GetMyExtensionsTemplate()
        {
            return GetMyExtensionsTemplate(this.QueryKind);
        }

        public static string GetMyExtensionsTemplate(QueryLanguage language)
        {
            return "void Main()\r\n{\r\n\t// Write code to test your extensions here. Press F5 to compile and run.\r\n}\r\n\r\npublic static class MyExtensions\r\n{\r\n\t// Write custom extension methods here. They will be available to all queries.\r\n\t\r\n}\r\n\r\n// You can also define non-static classes, enums, etc.";
        }

        public string[] GetStaticSchemaSameFolderReferences()
        {
            if (!(((this.Repository != null) && !this.Repository.DynamicSchema) && File.Exists(this.Repository.CustomAssemblyPath)))
            {
                return new string[0];
            }
            DateTime lastAccessTimeUtc = new FileInfo(this.Repository.CustomAssemblyPath).LastAccessTimeUtc;
            if ((this._lastCustomDataContextAssembly != this.Repository.CustomAssemblyPath) || (this._lastCustomDataContextWrite != lastAccessTimeUtc))
            {
                this._lastCustomDataContextAssembly = this.Repository.CustomAssemblyPath;
                this._lastCustomDataContextWrite = lastAccessTimeUtc;
                try
                {
                    this._lastIndirectRefs = new string[] { this.Repository.CustomAssemblyPath }.Concat<string>(AssemblyProber.GetSameFolderReferences(this.Repository.CustomAssemblyPath)).ToArray<string>();
                }
                catch (Exception exception)
                {
                    Log.Write(exception);
                    this._lastIndirectRefs = new string[] { this.Repository.CustomAssemblyPath };
                }
            }
            return this._lastIndirectRefs;
        }

        internal void OnQueryChanged()
        {
            this.OnQueryChanged(new QueryChangedEventArgs());
        }

        protected virtual void OnQueryChanged(QueryChangedEventArgs args)
        {
            if (this._suspendEventCount > 0)
            {
                if (this._pendingQueryChangedArgs == null)
                {
                    this._pendingQueryChangedArgs = args;
                }
                else
                {
                    this._pendingQueryChangedArgs.DbChanged |= args.DbChanged;
                    this._pendingQueryChangedArgs.SourceChanged |= args.SourceChanged;
                    this._pendingQueryChangedArgs.ReferencesChanged |= args.ReferencesChanged;
                    this._pendingQueryChangedArgs.NamespacesChanged |= args.NamespacesChanged;
                }
            }
            else
            {
                this._requiresRecompilation = true;
                this._modifiedSinceLastAutoSave = true;
                if (this.QueryChanged != null)
                {
                    this.QueryChanged(this, args);
                }
            }
        }

        private void OnQueryChanged(bool queryTextChanged, bool dbChanged)
        {
            this.OnQueryChanged(queryTextChanged, dbChanged, false, false);
        }

        private void OnQueryChanged(bool queryTextChanged, bool dbChanged, bool referencesChanged, bool namespacesChanged)
        {
            QueryChangedEventArgs args = new QueryChangedEventArgs {
                SourceChanged = queryTextChanged,
                DbChanged = dbChanged,
                ReferencesChanged = referencesChanged,
                NamespacesChanged = namespacesChanged
            };
            this.OnQueryChanged(args);
        }

        public void Open(string filePath)
        {
            using (this.TransactChanges())
            {
                string data = File.ReadAllText(filePath);
                this.LengthOnLastSave = (int) new FileInfo(filePath).Length;
                this.IsReadOnly = new FileInfo(filePath).IsReadOnly;
                if (!(this.ReadQuery(data, filePath) || !(Path.GetExtension(filePath).ToLowerInvariant() == ".sql")))
                {
                    this.QueryKind = QueryLanguage.SQL;
                }
                this.FilePath = filePath;
                try
                {
                    this.FileWriteTimeUtc = new DateTime?(new FileInfo(filePath).LastWriteTimeUtc);
                }
                catch
                {
                    this.FileWriteTimeUtc = null;
                }
            }
            this._modifiedSinceLastAutoSave = false;
            this.IsModified = false;
            this._requiresRecompilation = true;
            if (!filePath.Equals(MyExtensions.QueryFilePath, StringComparison.InvariantCultureIgnoreCase))
            {
                MRU.QueryNames.RegisterUse(filePath);
            }
        }

        public void OpenSample(string name, string content)
        {
            using (this.TransactChanges())
            {
                this.ReadQuery(content, null);
                this.Name = name;
                this.Predefined = true;
                this.IsReadOnly = false;
            }
            this._modifiedSinceLastAutoSave = false;
            this.IsModified = false;
            this._requiresRecompilation = true;
            this.ClearAutoSave();
        }

        public void ReadDefaultSettings()
        {
            this.ToDataGrids = UserOptions.Instance.ResultsInGrids;
            string path = Path.Combine(Program.UserDataFolder, "DefaultQuery.xml");
            if (File.Exists(path))
            {
                try
                {
                    XElement element = XElement.Load(path);
                    this.AdditionalReferences = (from refElement in element.Elements("Reference") select PathHelper.DecodeFolder((string) refElement)).ToArray<string>();
                    this.AdditionalGACReferences = (from refElement in element.Elements("GACReference") select (string) refElement).ToArray<string>();
                    this.AdditionalNamespaces = (from nsElement in element.Elements("Namespace") select (string) nsElement).ToArray<string>();
                    this.IncludePredicateBuilder = ((bool?) element.Element("IncludePredicateBuilder")) == true;
                }
                catch (Exception exception)
                {
                    Log.Write(exception);
                }
            }
        }

        private bool ReadQuery(string data, string relativeBase)
        {
            this.ClearAutoSave();
            this.FilePath = "";
            this.Name = "";
            this.FileWriteTimeUtc = null;
            this.Repository = null;
            this.IncludePredicateBuilder = false;
            this.ToDataGrids = false;
            this.Predefined = false;
            this.UISource = null;
            this.IsModified = false;
            this._requiresRecompilation = true;
            this.AdditionalReferences = new string[0];
            this.AdditionalGACReferences = new string[0];
            this.AdditionalNamespaces = new string[0];
            if (!_validQueryHeader.IsMatch(data))
            {
                this.Source = data;
                this.IsModified = false;
                return false;
            }
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
            XElement element = XElement.Parse(output.ToString());
            int lineNumber = reader.LineNumber;
            if (element.Attribute("Kind") != null)
            {
                try
                {
                    this.QueryKind = (QueryLanguage) Enum.Parse(typeof(QueryLanguage), (string) element.Attribute("Kind"), true);
                }
                catch (ArgumentException)
                {
                }
            }
            XElement store = element.Element("Connection");
            if (store != null)
            {
                LINQPad.Repository repository = new LINQPad.Repository(store) {
                    Persist = false
                };
                this.Repository = repository;
            }
            this.AdditionalReferences = (from refElement in element.Elements("Reference")
                select ResolveFileReference(relativeBase, refElement) into refPath
                where refPath != null
                select refPath).ToArray<string>();
            this.AdditionalGACReferences = (from refElement in element.Elements("GACReference") select (string) refElement).ToArray<string>();
            this.AdditionalNamespaces = (from nsElement in element.Elements("Namespace") select (string) nsElement).ToArray<string>();
            if (((bool?) element.Element("IncludePredicateBuilder")) == true)
            {
                this.IncludePredicateBuilder = true;
            }
            if ((element.Element("Output") != null) && (element.Element("Output").Value == "DataGrids"))
            {
                this.ToDataGrids = true;
            }
            StringReader reader3 = new StringReader(data);
            for (int i = 0; i < lineNumber; i++)
            {
                reader3.ReadLine();
            }
            this.Source = reader3.ReadToEnd().Trim();
            this.IsModified = false;
            return true;
        }

        public bool Recover(string filePath, string data, bool fileHasBeenTouched)
        {
            try
            {
                bool flag = (!fileHasBeenTouched && !string.IsNullOrEmpty(filePath)) && File.Exists(filePath);
                using (this.TransactChanges())
                {
                    this.IsReadOnly = flag && new FileInfo(filePath).IsReadOnly;
                    if (!this.ReadQuery(data, flag ? filePath : null))
                    {
                        return false;
                    }
                    this.FilePath = flag ? filePath : "";
                    if (flag)
                    {
                        try
                        {
                            this.FileWriteTimeUtc = new DateTime?(new FileInfo(filePath).LastWriteTimeUtc);
                        }
                        catch
                        {
                            this.FileWriteTimeUtc = null;
                        }
                    }
                }
                if (!flag && !string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        this.Name = Path.GetFileNameWithoutExtension(filePath);
                    }
                    catch
                    {
                    }
                }
                this._modifiedSinceLastAutoSave = true;
                this.IsModified = true;
                this.IsMyExtensions = this.FilePath.Equals(MyExtensions.QueryFilePath, StringComparison.InvariantCultureIgnoreCase);
                this._requiresRecompilation = true;
                return true;
            }
            catch (Exception exception)
            {
                Log.Write(exception, "Query.Recover");
                return false;
            }
        }

        private static string ResolveFileReference(string relativeBase, XElement refElement)
        {
            string path = PathHelper.DecodeFolder((string) refElement);
            if (PathHelper.PathHasInvalidChars(path))
            {
                return null;
            }
            if (!string.IsNullOrEmpty(relativeBase))
            {
                try
                {
                    string str3 = (string) refElement.Attribute("Relative");
                    if (string.IsNullOrEmpty(str3))
                    {
                        return path;
                    }
                    string localPath = new Uri(new Uri(relativeBase), str3).LocalPath;
                    if (File.Exists(localPath))
                    {
                        path = localPath;
                    }
                }
                catch
                {
                }
            }
            return path;
        }

        public void Save()
        {
            if (this._filePath.Length == 0)
            {
                throw new InvalidOperationException("Cannot save: no filepath");
            }
            this.SaveAs(this._filePath);
        }

        public void SaveAs(string filePath)
        {
            using (this.TransactChanges())
            {
                this.UISource = null;
                Exception exception = null;
                try
                {
                    string directoryName = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    using (StreamWriter writer = File.CreateText(filePath))
                    {
                        this.WriteTo(writer, filePath, Path.GetExtension(filePath).ToLowerInvariant() != ".sql");
                    }
                    try
                    {
                        this.FileWriteTimeUtc = new DateTime?(new FileInfo(filePath).LastWriteTimeUtc);
                    }
                    catch
                    {
                        this.FileWriteTimeUtc = null;
                    }
                }
                catch (UnauthorizedAccessException exception2)
                {
                    exception = exception2;
                }
                catch (IOException exception3)
                {
                    exception = exception3;
                }
                if (exception != null)
                {
                    MessageBox.Show("Cannot save file: " + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                this.IsModified = false;
                this.FilePath = filePath;
                this.Name = "";
                this.Predefined = false;
                this.Pinned = true;
                this.IsReadOnly = false;
                this.ClearAutoSave();
            }
            try
            {
                this.LengthOnLastSave = (int) new FileInfo(filePath).Length;
            }
            catch
            {
                this.LengthOnLastSave = 0;
            }
            if (!this.IsMyExtensions)
            {
                MRU.QueryNames.RegisterUse(filePath);
            }
        }

        public static void SetDefaultQueryProps(string[] references, string[] gacReferences, string[] imports, bool includePredicateBuilder)
        {
            if (!Directory.Exists(Program.UserDataFolder))
            {
                Directory.CreateDirectory(Program.UserDataFolder);
            }
            object[] content = new object[] { from s in references select new XElement("Reference", PathHelper.EncodeFolder(s)), from s in gacReferences select new XElement("GACReference", s), from s in imports select new XElement("Namespace", s), new XElement("IncludePredicateBuilder", includePredicateBuilder) };
            XElement element = new XElement("Query", content);
            string fileName = Path.Combine(Program.UserDataFolder, "DefaultQuery.xml");
            try
            {
                element.Save(fileName);
            }
            catch
            {
                Thread.Sleep(300);
                element.Save(fileName);
            }
        }

        public void SortReferences()
        {
        }

        internal IDisposable TransactChanges()
        {
            return new ChangeTx(this);
        }

        public void WriteTo(StreamWriter sr, string queryPathIfKnown, bool includeHeader)
        {
            Func<string, XElement> selector = null;
            if (includeHeader)
            {
                XmlWriterSettings settings2 = new XmlWriterSettings {
                    OmitXmlDeclaration = true,
                    CloseOutput = false,
                    Indent = true
                };
                using (XmlWriter writer = XmlWriter.Create(sr, settings2))
                {
                    object[] content = new object[7];
                    content[0] = (this.Repository == null) ? null : this.Repository.GetStore();
                    content[1] = new XAttribute("Kind", this.QueryKind);
                    content[2] = this.ToDataGrids ? new XElement("Output", "DataGrids") : null;
                    if (selector == null)
                    {
                        selector = s => this.GetFileReferenceElement(queryPathIfKnown, s);
                    }
                    content[3] = this.AdditionalReferences.Select<string, XElement>(selector);
                    content[4] = from s in this.AdditionalGACReferences select new XElement("GACReference", s);
                    content[5] = from s in this.AdditionalNamespaces select new XElement("Namespace", s);
                    content[6] = this.IncludePredicateBuilder ? new XElement("IncludePredicateBuilder", true) : null;
                    new XElement("Query", content).WriteTo(writer);
                }
                sr.WriteLine();
                sr.WriteLine();
            }
            sr.Write(Regex.Replace(this.Source, "(?<!\r)\n", "\r\n"));
        }

        public string[] AdditionalGACReferences
        {
            get
            {
                return this._additionalGACReferences;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("AdditionalGACReferences");
                }
                if ((this._additionalGACReferences != value) && !this._additionalGACReferences.SequenceEqual<string>(value))
                {
                    using (this.TransactChanges())
                    {
                        this._additionalGACReferences = value;
                        this.IsModified = true;
                        this.OnQueryChanged(false, false, true, false);
                    }
                }
            }
        }

        public string[] AdditionalNamespaces
        {
            get
            {
                return this._additionalNamespaces;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("AdditionalNamespaces");
                }
                if ((this._additionalNamespaces != value) && !this._additionalNamespaces.SequenceEqual<string>(value))
                {
                    using (this.TransactChanges())
                    {
                        this._additionalNamespaces = value;
                        this.IsModified = true;
                        this.OnQueryChanged(false, false, false, true);
                    }
                }
            }
        }

        public string[] AdditionalReferences
        {
            get
            {
                return this._additionalReferences;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("AdditionalReferences");
                }
                if ((this._additionalReferences != value) && !this._additionalReferences.SequenceEqual<string>(value))
                {
                    using (this.TransactChanges())
                    {
                        this._additionalReferences = value;
                        this.IsModified = true;
                        this.OnQueryChanged(false, false, true, false);
                    }
                }
            }
        }

        public string[] AllFileReferences
        {
            get
            {
                return this.AdditionalReferences;
            }
        }

        public string FilePath
        {
            get
            {
                return this._filePath;
            }
            internal set
            {
                if (!(this._filePath == value))
                {
                    this._filePath = value;
                    this.OnQueryChanged();
                }
            }
        }

        public DateTime? FileWriteTimeUtc { get; private set; }

        public bool IncludePredicateBuilder
        {
            get
            {
                return this._includePredicateBuilder;
            }
            set
            {
                if (this._includePredicateBuilder != value)
                {
                    this._includePredicateBuilder = value;
                    using (this.TransactChanges())
                    {
                        this.IsModified = true;
                        this.OnQueryChanged();
                    }
                }
            }
        }

        public bool IsDisposed
        {
            get
            {
                return this._isDisposed;
            }
        }

        public bool IsModified
        {
            get
            {
                return this._isModified;
            }
            set
            {
                if (value)
                {
                    this.Pinned = true;
                }
                this._isModified = value;
            }
        }

        public bool IsMyExtensions
        {
            get
            {
                return this._isMyExtensions;
            }
            set
            {
                if (this._isMyExtensions != value)
                {
                    this._isMyExtensions = value;
                    using (this.TransactChanges())
                    {
                        this.OnQueryChanged();
                    }
                }
            }
        }

        public bool IsReadOnly { get; private set; }

        public DateTimeOffset LastAutoSave
        {
            get
            {
                return ((this._autoSaver == null) ? DateTimeOffset.MinValue : this._autoSaver.LastAutoSave);
            }
        }

        public int LengthOnLastSave { get; private set; }

        public string Name
        {
            get
            {
                if (this.IsMyExtensions)
                {
                    return "My Extensions";
                }
                if (this._filePath.Length > 0)
                {
                    return ((Path.GetExtension(this._filePath).ToLowerInvariant() == ".sql") ? Path.GetFileName(this._filePath) : Path.GetFileNameWithoutExtension(this._filePath));
                }
                return this._name;
            }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    if (this._filePath.Length == 0)
                    {
                        using (this.TransactChanges())
                        {
                            this.OnQueryChanged();
                        }
                    }
                }
            }
        }

        public bool Pinned
        {
            get
            {
                return this._pinned;
            }
            set
            {
                if ((value || !this.IsModified) && (this._pinned != value))
                {
                    this._pinned = value;
                    using (this.TransactChanges())
                    {
                        this.OnQueryChanged();
                    }
                }
            }
        }

        public bool Predefined { get; private set; }

        public string QueryBaseClassName
        {
            get
            {
                return ((((this.Repository == null) || !this.Repository.DynamicSchema) || !this.Repository.IsAutoGenAssemblyAvailable) ? (((this.Repository == null) || this.Repository.DynamicSchema) ? null : this.Repository.CustomTypeName) : string.Join(".", new string[] { this.Repository.AutoGenNamespace, this.Repository.AutoGenTypeName }));
            }
        }

        public string QueryBaseClassNamespace
        {
            get
            {
                string queryBaseClassName = this.QueryBaseClassName;
                if (!(!string.IsNullOrEmpty(queryBaseClassName) && this.QueryBaseClassName.Contains(".")))
                {
                    return "";
                }
                return queryBaseClassName.Substring(0, queryBaseClassName.LastIndexOf('.'));
            }
        }

        public QueryLanguage QueryKind
        {
            get
            {
                return this._queryKind;
            }
            set
            {
                if (this._queryKind != value)
                {
                    this._queryKind = value;
                    using (this.TransactChanges())
                    {
                        this.IsModified = true;
                        this.OnQueryChanged();
                    }
                }
            }
        }

        public LINQPad.Repository Repository
        {
            get
            {
                return this._repository;
            }
            set
            {
                if (this._repository != value)
                {
                    LINQPad.Repository repository = this._repository;
                    this._repository = value;
                    if (!LINQPad.Repository.AreEquivalent(repository, this._repository))
                    {
                        using (this.TransactChanges())
                        {
                            this.IsModified = true;
                            this.OnQueryChanged(false, true);
                        }
                    }
                }
            }
        }

        public bool RequiresRecompilation
        {
            get
            {
                return this._requiresRecompilation;
            }
            set
            {
                this._requiresRecompilation = value;
            }
        }

        public string Source
        {
            get
            {
                return this._source;
            }
            set
            {
                if (this._source != value)
                {
                    this._source = value;
                    using (this.TransactChanges())
                    {
                        this.IsModified = true;
                        this.OnQueryChanged(true, false);
                    }
                }
            }
        }

        public bool ToDataGrids
        {
            get
            {
                return this._toDataGrids;
            }
            set
            {
                if (this._toDataGrids != value)
                {
                    this._toDataGrids = value;
                    using (this.TransactChanges())
                    {
                        this.IsModified = true;
                        this.OnQueryChanged();
                    }
                }
            }
        }

        private class ChangeTx : IDisposable
        {
            private QueryCore _query;

            internal ChangeTx(QueryCore query)
            {
                this._query = query;
                this._query._suspendEventCount++;
            }

            public void Dispose()
            {
                if (--this._query._suspendEventCount == 0)
                {
                    if (this._query._pendingQueryChangedArgs != null)
                    {
                        this._query.OnQueryChanged(this._query._pendingQueryChangedArgs);
                    }
                    this._query._pendingQueryChangedArgs = null;
                }
            }
        }
    }
}

