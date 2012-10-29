namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class MRU
    {
        public static MRU AstoriaUriNames = new MRU("RecentAstoriaUris.txt");
        public static MRU AzureServerNames = new MRU("RecentAzureServers.txt");
        public readonly string BackingFileName;
        public readonly string BackingFilePath;
        public static MRU DallasUriNames = new MRU("RecentDallasUris.txt");
        public static MRU FindAll = new MRU("RecentFindAll.txt");
        public readonly int MaxItems;
        public static MRU NavigateTo = new MRU("RecentNavigateTo.txt");
        public static MRU PluginLocations = new MRU("PluginLocations.txt");
        public static MRU QueryLocations = new MRU("QueryLocations.txt");
        public static MRU QueryNames = new MRU("RecentQueries.txt", 9);
        public static MRU ServerNames = new MRU("RecentServers.txt");
        public static MRU SnippetLocations = new MRU("SnippetLocations.txt");

        public MRU(string backingFileName) : this(backingFileName, 20)
        {
        }

        public MRU(string backingFileName, int maxItems)
        {
            this.BackingFileName = backingFileName;
            this.BackingFilePath = Path.Combine(Program.UserDataFolder, this.BackingFileName);
            this.MaxItems = maxItems;
        }

        public string[] GetNames()
        {
            return this.ReadNames().ToArray();
        }

        public List<string> ReadNames()
        {
            List<string> list = new List<string>();
            if (!File.Exists(this.BackingFilePath))
            {
                return list;
            }
            try
            {
                return (from s in File.ReadAllText(this.BackingFilePath).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) select s.Trim()).ToList<string>();
            }
            catch
            {
                return list;
            }
        }

        public void RegisterUse(string name)
        {
            name = name.Trim();
            List<string> source = this.ReadNames();
            if (source.FirstOrDefault<string>(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)) != null)
            {
                source.Remove(name);
            }
            source.Insert(0, name);
            if (source.Count > this.MaxItems)
            {
                source.RemoveAt(source.Count - 1);
            }
            this.WriteNames(source);
        }

        public void Unregister(string name)
        {
            name = name.Trim();
            List<string> source = this.ReadNames();
            if (source.FirstOrDefault<string>(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)) != null)
            {
                source.Remove(name);
            }
            this.WriteNames(source);
        }

        private void WriteNames(IEnumerable<string> names)
        {
            try
            {
                string directoryName = Path.GetDirectoryName(this.BackingFilePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                File.WriteAllText(this.BackingFilePath, string.Join("\r\n", names.ToArray<string>()));
            }
            catch
            {
            }
        }
    }
}

