namespace LINQPad
{
    using System;
    using System.IO;

    internal static class Options
    {
        public static readonly string CustomStyleSheetLocation = Path.Combine(Program.UserDataFolder, "resultstyles.css");
        public static string DefaultQueryFolder;
        internal static bool IsDefaultQueryFolder;

        static Options()
        {
            RefreshDefaultQueryFolder();
        }

        public static string GetDefaultQueryFolder(bool createIfNotThere)
        {
            string defaultQueryFolder = DefaultQueryFolder;
            if (!Directory.Exists(defaultQueryFolder))
            {
                if (!createIfNotThere)
                {
                    return null;
                }
                try
                {
                    Directory.CreateDirectory(defaultQueryFolder);
                }
                catch
                {
                    return null;
                }
            }
            return defaultQueryFolder;
        }

        internal static void RefreshDefaultQueryFolder()
        {
            string str = null;
            string path = Path.Combine(Program.UserDataFolder, "querypath.txt");
            if (File.Exists(path))
            {
                try
                {
                    str = File.ReadAllText(path);
                }
                catch
                {
                }
            }
            IsDefaultQueryFolder = (str == null) || (str.Trim().Length == 0);
            if (IsDefaultQueryFolder)
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "LINQPad Queries");
            }
            if (!(str == DefaultQueryFolder))
            {
                DefaultQueryFolder = str;
            }
        }
    }
}

