namespace LINQPad
{
    using System;
    using System.Drawing;
    using System.IO;

    internal class UserOptions : SerializableOptions
    {
        private static UserOptions _instance;
        [Serialize]
        public bool CompileVBInStrictMode;
        [Serialize]
        public bool ConvertTabsToSpaces;
        [Serialize]
        public string CustomSnippetsFolder;
        [Serialize]
        public QueryLanguage? DefaultQueryLanguage;
        [Serialize]
        public bool DisableLambdaSnippets;
        [Serialize]
        public bool DoubleClickToOpenMyQueries;
        [Serialize]
        public string EditorBackColor;
        public static readonly string FileName = Path.Combine(Program.UserDataFolder, "RoamingUserOptions.xml");
        [Serialize]
        public bool FreshAppDomains;
        [Serialize]
        public bool LockReferenceAssemblies;
        [Serialize]
        public bool MARS;
        [Serialize]
        public uint? MaxColumnWidthInLists;
        [Serialize]
        public int? MaxQueryRows;
        [Serialize]
        public bool MTAThreadingMode;
        [Serialize]
        public bool NativeHotKeys;
        [Serialize]
        public bool NoNativeKeysQuestion;
        [Serialize]
        public bool NoSqlPasswordExpiryPrompts;
        [Serialize]
        public bool OpenMyQueriesInNewTab;
        [Serialize]
        public bool PassiveAutocompletion;
        [Serialize]
        public string PluginsFolder = null;
        [Serialize]
        public bool PresentationMode;
        [Serialize]
        public bool PreserveAppDomains;
        [Serialize]
        public bool ResultsInGrids;
        [Serialize]
        public bool ShowLineNumbersInEditor;
        [Serialize]
        public byte? TabSize;

        public string GetCustomSnippetsFolder(bool createIfNotThere)
        {
            string customSnippetsFolder = this.CustomSnippetsFolder;
            if (string.IsNullOrEmpty(customSnippetsFolder))
            {
                customSnippetsFolder = this.GetDefaultCustomSnippetsFolder();
            }
            if (!Directory.Exists(customSnippetsFolder))
            {
                if (!createIfNotThere)
                {
                    return null;
                }
                try
                {
                    Directory.CreateDirectory(customSnippetsFolder);
                }
                catch
                {
                    return null;
                }
            }
            return customSnippetsFolder;
        }

        public string GetDefaultCustomSnippetsFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "LINQPad Snippets");
        }

        public string GetDefaultPluginsFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "LINQPad Plugins");
        }

        public string GetPluginsFolder(bool nullIfNotExists)
        {
            string pluginsFolder = this.PluginsFolder;
            if (string.IsNullOrEmpty(pluginsFolder))
            {
                pluginsFolder = this.GetDefaultPluginsFolder();
            }
            if (!(Directory.Exists(pluginsFolder) || !nullIfNotExists))
            {
                return null;
            }
            return pluginsFolder;
        }

        public Color ActualEditorBackColor
        {
            get
            {
                if (string.IsNullOrEmpty(this.EditorBackColor))
                {
                    return SystemColors.Window;
                }
                try
                {
                    return ColorTranslator.FromHtml(this.EditorBackColor);
                }
                catch
                {
                    return SystemColors.Window;
                }
            }
        }

        public override string FullPath
        {
            get
            {
                return FileName;
            }
        }

        public static UserOptions Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = new UserOptions();
                        _instance.Deserialize();
                    }
                    catch
                    {
                    }
                }
                return _instance;
            }
        }

        public bool IsVBDefault
        {
            get
            {
                QueryLanguage? defaultQueryLanguage = this.DefaultQueryLanguage;
                return (defaultQueryLanguage.HasValue ? defaultQueryLanguage.GetValueOrDefault() : QueryLanguage.Expression).ToString().ToLowerInvariant().StartsWith("vb");
            }
        }

        public byte TabSizeActual
        {
            get
            {
                byte? tabSize;
                if (this.TabSize.HasValue)
                {
                    tabSize = this.TabSize;
                }
                if (((tabSize.GetValueOrDefault() < 2) && tabSize.HasValue) || (((tabSize = this.TabSize).GetValueOrDefault() > 15) && tabSize.HasValue))
                {
                    return 4;
                }
                return this.TabSize.Value;
            }
        }
    }
}

