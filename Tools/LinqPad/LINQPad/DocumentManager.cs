namespace LINQPad
{
    using ActiproBridge;
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.DotNet.Dom;
    using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
    using LINQPad.ExecutionModel;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;

    internal class DocumentManager : IDisposable
    {
        private DataContextManager _dcManager;
        private ActiproSoftware.SyntaxEditor.Document _document;
        private static Dictionary<string, SyntaxLanguage> _dynamicLanguages = new Dictionary<string, SyntaxLanguage>();
        private SpanIndicatorLayer _executedSelectionLayer;
        private Control _hostControl;
        private SyntaxLanguage _language;
        private Repository _lastRepository;
        private SpanIndicatorLayer _mainErrorLayer;
        private RunnableQuery _query;
        private SpanIndicatorLayer _stackTraceLayer;
        private ActiproBridge.TypeResolver _typeResolver;
        private SpanIndicatorLayer _uriLayer;
        private SpanIndicatorLayer _warningsLayer;
        private static float? _windowBrightness;

        static DocumentManager()
        {
            if (typeof(AssemblyCodeRepository).GetEvent("AppDomainCreated") != null)
            {
                AddResolveHandler();
            }
        }

        public DocumentManager(RunnableQuery q, Control hostControl)
        {
            this._query = q;
            this._hostControl = hostControl;
            this._document = new ActiproSoftware.SyntaxEditor.Document();
            this._document.get_SpanIndicatorLayers().Add(this._mainErrorLayer = new SpanIndicatorLayer("errors", 0x3e8));
            this._document.get_SpanIndicatorLayers().Add(this._warningsLayer = new SpanIndicatorLayer("warnings", 0x3e8));
            this._document.get_SpanIndicatorLayers().Add(this._executedSelectionLayer = new SpanIndicatorLayer("executedSelection", 0));
            this._document.get_SpanIndicatorLayers().Add(this._uriLayer = new SpanIndicatorLayer("uriLayer", 0));
            this._document.get_SpanIndicatorLayers().Add(this._stackTraceLayer = new SpanIndicatorLayer("stackTrace", 0));
            if (UserOptions.Instance.TabSize.HasValue)
            {
                this._document.set_TabSize(UserOptions.Instance.TabSizeActual);
            }
            if (UserOptions.Instance.ConvertTabsToSpaces)
            {
                this._document.set_AutoConvertTabsToSpaces(true);
            }
            this._typeResolver = new ActiproBridge.TypeResolver(delegate (string name) {
                if (Program.Splash != null)
                {
                    Program.Splash.UpdateMessage("Performing one-time build of autocompletion cache: " + name);
                }
            }, MyExtensions.AdditionalRefs);
            this.ConfigureLanguage();
            this.ConfigureResolver();
        }

        private static void AddResolveHandler()
        {
            try
            {
                AssemblyCodeRepository.add_AppDomainCreated(new AppDomainEventHandler(null, (IntPtr) AssemblyCodeRepository_AppDomainCreated));
            }
            catch
            {
            }
        }

        private static void AssemblyCodeRepository_AppDomainCreated(object sender, AppDomainEventArgs e)
        {
            e.get_Domain().AssemblyResolve += new ResolveEventHandler(Program.FindAssem);
        }

        public bool CheckForRepositoryChange()
        {
            if (this._query.QueryKind >= QueryLanguage.SQL)
            {
                return false;
            }
            if (this._lastRepository == this._query.Repository)
            {
                return false;
            }
            this._lastRepository = this._query.Repository;
            if (this._dcManager != null)
            {
                this._dcManager.Dispose();
            }
            this._dcManager = null;
            this.ResetDCAssembly();
            if (!((this._query.Repository != null) && this._query.Repository.DynamicSchema))
            {
                this.ConfigureLanguage();
                this.ConfigureResolver();
                this._typeResolver.set_CurrentDCNamespace(this._query.QueryBaseClassNamespace);
                this._typeResolver.set_CurrentDCTypeName(this._query.QueryBaseClassName);
                this._typeResolver.set_DataContextBuilding(false);
                this._typeResolver.set_CurrentDCError(null);
                return true;
            }
            this._typeResolver.set_DataContextBuilding(true);
            this._typeResolver.set_CurrentDCError(null);
            this._dcManager = DataContextManager.SubscribeToDataContextChanges(this._query.Repository, new DataContextCallback(this.DataContextInfoUpdated));
            this._dcManager.GetDataContextInfo(SchemaChangeTestMode.None);
            return true;
        }

        public void ConfigureLanguage()
        {
            if ((this._query.QueryKind >= QueryLanguage.FSharpExpression) && this._document.get_SpanIndicatorLayers().Contains("Syntax error"))
            {
                this._document.get_SpanIndicatorLayers().get_Item("Syntax error").Clear();
            }
            QueryCompiler compiler = QueryCompiler.Create(this._query, false);
            this._language = this.GetLanguage();
            this._document.ResetLanguage();
            if (this._query.QueryKind == QueryLanguage.SQL)
            {
                string str;
                this._document.set_FooterText((string) (str = null));
                this._document.set_HeaderText(str);
            }
            else
            {
                this._document.set_HeaderText(compiler.GetHeader(this._query));
                this._document.set_FooterText(compiler.GetFooter(this._query));
            }
            this._document.set_Language(this._language);
            this._document.Reparse();
        }

        public void ConfigureResolver()
        {
            UniqueStringCollection strings = new UniqueStringCollection(this._query.AllFileReferences.Concat<string>(PluginAssembly.GetCompatibleAssemblies(this._query.IsMyExtensions)), new FileNameComparer());
            if (this._query.Repository != null)
            {
                strings.AddRange(this._query.Repository.GetDriverAssemblies());
            }
            UniqueStringCollection strings2 = new UniqueStringCollection(this._query.AdditionalGACReferences);
            if (!(this._query.IsMyExtensions || (MyExtensions.Query == null)))
            {
                strings.AddRange(MyExtensions.Query.AllFileReferences);
                strings2.AddRange(MyExtensions.Query.AdditionalGACReferences);
            }
            bool flag = ((this._query.Repository != null) && this._query.Repository.DriverLoader.IsValid) && (this._query.Repository.DriverLoader.Driver is LinqToSqlDynamicDriver);
            string fileOrPath = this._typeResolver.Configure(strings, strings2, (this._query.Repository != null) && !this._query.Repository.DynamicSchema, flag);
            if (((fileOrPath != null) && (this._query.Repository != null)) && this._query.Repository.DynamicSchema)
            {
                this._typeResolver.set_DataContextBuilding(false);
                if ((this._query.Repository.IntellicachePath == null) || (this._query.Repository.IntellicachePath.FullPath != fileOrPath))
                {
                    this._query.Repository.IntellicachePath = new TempFileRef(fileOrPath);
                }
            }
        }

        private void DataContextInfoUpdated(DataContextInfo m)
        {
            // This item is obfuscated and can not be translated.
            if ((this._dcManager == null) || (m.Repository != this._dcManager.Repository))
            {
                return;
            }
        Label_0051:
            if (this._hostControl != null)
            {
            }
            if (0 == 0)
            {
                if (this._hostControl != null)
                {
                    this._hostControl.BeginInvoke(new DataContextCallback(this.UpdateDC), new object[] { m });
                }
            }
            else if (!this._hostControl.IsDisposed)
            {
                Thread.Sleep(100);
                goto Label_0051;
            }
        }

        public void Dispose()
        {
            if (this._typeResolver != null)
            {
                this._typeResolver.Dispose();
            }
            if (this._dcManager != null)
            {
                this._dcManager.Dispose();
            }
            this._dcManager = null;
            this._hostControl = null;
        }

        private SyntaxLanguage GetCSharpLanguage()
        {
            CustomCSharpSyntaxLanguage language = new CustomCSharpSyntaxLanguage(this._typeResolver);
            language.get_HighlightingStyles().get_Item("StringStyle").set_ForeColor((WindowBrightness < 0.4f) ? Color.FromArgb(220, 100, 100) : Color.FromArgb(220, 20, 20));
            language.get_HighlightingStyles().get_Item("CommentStyle").set_ForeColor((WindowBrightness < 0.4f) ? Color.FromArgb(100, 200, 110) : Color.Green);
            language.get_HighlightingStyles().get_Item("KeywordStyle").set_ForeColor((WindowBrightness < 0.4f) ? Color.FromArgb(130, 170, 0xff) : Color.Blue);
            language.get_HighlightingStyles().get_Item("NumberStyle").set_ForeColor((WindowBrightness < 0.4f) ? Color.FromArgb(180, 80, 230) : Color.FromArgb(200, 30, 250));
            (from s in language.get_HighlightingStyles().Cast<HighlightingStyle>() select s.get_Key()).ToArray<string>();
            if (WindowBrightness < 0.4f)
            {
                language.get_HighlightingStyles().Add(new HighlightingStyle("BracketHighlightingStyle", "Bracket Highlighted Text", Color.White, Color.DarkRed));
            }
            return language;
        }

        public static SyntaxLanguage GetDynamicLanguage(string name, float windowBrightness)
        {
            SyntaxLanguage language2;
            string key = name + windowBrightness;
            if (_dynamicLanguages.ContainsKey(key))
            {
                return _dynamicLanguages[key];
            }
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LINQPad.Lexers." + name + ".xml"))
            {
                language2 = DynamicSyntaxLanguage.LoadFromXml(stream, 0);
            }
            foreach (HighlightingStyle style in language2.get_HighlightingStyles())
            {
                if ((style.get_ForeColor().GetBrightness() < 0.2f) && (windowBrightness < 0.4f))
                {
                    style.set_ForeColor(Color.White);
                }
                else if ((windowBrightness < 0.4f) && (style.get_ForeColor() == Color.Blue))
                {
                    style.set_ForeColor(Color.FromArgb(130, 170, 0xff));
                }
                else if ((windowBrightness < 0.4f) && ((style.get_ForeColor() == Color.Red) || (style.get_ForeColor() == Color.FromArgb(220, 20, 20))))
                {
                    style.set_ForeColor(Color.FromArgb(210, 90, 90));
                }
                else if ((windowBrightness < 0.4f) && (style.get_ForeColor() == Color.Green))
                {
                    style.set_ForeColor(Color.FromArgb(100, 200, 110));
                }
            }
            _dynamicLanguages[key] = language2;
            return language2;
        }

        private SyntaxLanguage GetLanguage()
        {
            SyntaxLanguage dynamicLanguage;
            if (this._query.QueryKind == QueryLanguage.SQL)
            {
                dynamicLanguage = GetDynamicLanguage("SQL", WindowBrightness);
            }
            else if (this._query.QueryKind == QueryLanguage.ESQL)
            {
                dynamicLanguage = GetDynamicLanguage("ESQL", WindowBrightness);
            }
            else if (this._query.QueryKind.ToString().StartsWith("VB", StringComparison.Ordinal))
            {
                dynamicLanguage = this.GetVBLanguage();
            }
            else
            {
                if (this._query.QueryKind.ToString().StartsWith("FSharp", StringComparison.Ordinal))
                {
                    dynamicLanguage = GetDynamicLanguage("FSharp", WindowBrightness);
                    if (WindowBrightness < 0.4f)
                    {
                        dynamicLanguage.get_HighlightingStyles().get_Item("StringDefaultStyle").set_ForeColor(Color.FromArgb(210, 90, 90));
                        dynamicLanguage.get_HighlightingStyles().get_Item("CommentDefaultStyle").set_ForeColor(Color.FromArgb(90, 200, 100));
                        dynamicLanguage.get_HighlightingStyles().get_Item("CommentDelimiterStyle").set_ForeColor(Color.FromArgb(90, 200, 100));
                        dynamicLanguage.get_HighlightingStyles().get_Item("ReservedWordStyle").set_ForeColor(Color.FromArgb(120, 150, 0xff));
                        dynamicLanguage.get_HighlightingStyles().get_Item("NumberStyle").set_ForeColor(Color.FromArgb(180, 80, 230));
                    }
                    return dynamicLanguage;
                }
                dynamicLanguage = this.GetCSharpLanguage();
            }
            DotNetSyntaxLanguage language3 = dynamicLanguage as DotNetSyntaxLanguage;
            if (language3 != null)
            {
                language3.set_IntelliPromptQuickInfoEnabled(true);
                language3.set_IntelliPromptParameterInfoEnabled(true);
                language3.set_IntelliPromptMemberListEnabled(true);
            }
            foreach (HighlightingStyle style in dynamicLanguage.get_HighlightingStyles())
            {
                if ((style.get_ForeColor().GetBrightness() < 0.2f) && (WindowBrightness < 0.4f))
                {
                    style.set_ForeColor((WindowBrightness < 0.2f) ? Color.FromArgb(230, 230, 230) : Color.White);
                }
            }
            return dynamicLanguage;
        }

        private SyntaxLanguage GetVBLanguage()
        {
            CustomVBSyntaxLanguage language = new CustomVBSyntaxLanguage(this._typeResolver);
            if (WindowBrightness < 0.4f)
            {
                language.get_HighlightingStyles().get_Item("StringStyle").set_ForeColor(Color.FromArgb(210, 90, 90));
                language.get_HighlightingStyles().get_Item("CommentStyle").set_ForeColor(Color.FromArgb(100, 200, 110));
                language.get_HighlightingStyles().get_Item("KeywordStyle").set_ForeColor(Color.FromArgb(130, 170, 0xff));
                language.get_HighlightingStyles().get_Item("NumberStyle").set_ForeColor(Color.FromArgb(180, 80, 230));
            }
            return language;
        }

        private void ResetDCAssembly()
        {
            if (((this._query.Repository != null) && !this._query.Repository.DynamicSchema) && File.Exists(this._query.Repository.CustomAssemblyPath))
            {
                this._typeResolver.set_CurrentDCAssemblyPath(this._query.Repository.CustomAssemblyPath);
            }
            else
            {
                this._typeResolver.set_CurrentDCAssemblyPath(null);
            }
            this._typeResolver.set_CurrentDCError(null);
            this._typeResolver.set_CurrentDCNamespace(this._query.QueryBaseClassNamespace);
            this._typeResolver.set_CurrentDCTypeName(this._query.QueryBaseClassName);
            this.ConfigureResolver();
            this._document.Reparse();
        }

        public void ResetSharedReferences(string[] extraSharedRefs)
        {
            this._typeResolver.ResetSharedReferences(extraSharedRefs);
        }

        public static void ResetWindowBrightness()
        {
            _windowBrightness = null;
        }

        private void UpdateDC(DataContextInfo dc)
        {
            try
            {
                this._typeResolver.set_CurrentDCAssemblyPath((dc.AssemblyPath == null) ? null : dc.AssemblyPath.FullPath);
                this._typeResolver.set_CurrentDCError(dc.Error);
                this._query.PolluteCachedDomain(false);
                this._typeResolver.set_CurrentDCNamespace(this._query.QueryBaseClassNamespace);
                this._typeResolver.set_CurrentDCTypeName(this._query.QueryBaseClassName);
                this.ConfigureLanguage();
                this.ConfigureResolver();
            }
            catch (Exception exception)
            {
                Program.ProcessException(exception);
            }
        }

        public ActiproSoftware.SyntaxEditor.Document Document
        {
            get
            {
                return this._document;
            }
        }

        public SpanIndicatorLayer ExecutedSelectionLayer
        {
            get
            {
                return this._executedSelectionLayer;
            }
        }

        public SpanIndicatorLayer MainErrorLayer
        {
            get
            {
                return this._mainErrorLayer;
            }
        }

        public SpanIndicatorLayer StackTraceLayer
        {
            get
            {
                return this._stackTraceLayer;
            }
        }

        internal ActiproBridge.TypeResolver TypeResolver
        {
            get
            {
                return this._typeResolver;
            }
        }

        public SpanIndicatorLayer UriLayer
        {
            get
            {
                return this._uriLayer;
            }
        }

        public SpanIndicatorLayer WarningsLayer
        {
            get
            {
                return this._warningsLayer;
            }
        }

        private static float WindowBrightness
        {
            get
            {
                if (!_windowBrightness.HasValue)
                {
                    _windowBrightness = new float?(UserOptions.Instance.ActualEditorBackColor.GetBrightness());
                }
                return _windowBrightness.Value;
            }
        }
    }
}

