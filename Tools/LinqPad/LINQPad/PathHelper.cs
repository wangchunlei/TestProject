namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    internal static class PathHelper
    {
        private static Regex _folderDecoder = new Regex("^<.+>", RegexOptions.IgnoreCase);
        private static string _programFiles;
        private static string _programFilesX64;
        private static string _programFilesX86;
        private static List<string> _tokens = new List<string>();
        private static Dictionary<string, string> _tokensToFolders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static string _windows;

        static PathHelper()
        {
            AddSpecialFolder("MyDocuments", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            AddSpecialFolder("TempFiles", Path.GetTempPath());
            AddSpecialFolder(Environment.SpecialFolder.ApplicationData);
            AddSpecialFolder(Environment.SpecialFolder.CommonApplicationData);
            AddSpecialFolder(Environment.SpecialFolder.LocalApplicationData);
            AddSpecialFolder(Environment.SpecialFolder.CommonProgramFiles);
            AddSpecialFolder("RuntimeDirectory", RuntimeEnvironment.GetRuntimeDirectory());
            if (ProgramFiles == ProgramFilesX86)
            {
                if (ProgramFilesX86.EndsWith("(x86)", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddSpecialFolder("ProgramFilesX86", ProgramFilesX86);
                    AddSpecialFolder("ProgramFilesX64", ProgramFilesX64);
                    AddSpecialFolder(Environment.SpecialFolder.ProgramFiles);
                }
                else
                {
                    AddSpecialFolder(Environment.SpecialFolder.ProgramFiles);
                    AddSpecialFolder("ProgramFilesX86", ProgramFilesX86);
                    AddSpecialFolder("ProgramFilesX64", ProgramFilesX64);
                }
            }
            else
            {
                AddSpecialFolder("ProgramFilesX64", ProgramFilesX64);
                AddSpecialFolder("ProgramFilesX86", ProgramFilesX86);
                AddSpecialFolder(Environment.SpecialFolder.ProgramFiles);
            }
        }

        private static void AddSpecialFolder(Environment.SpecialFolder sf)
        {
            try
            {
                AddSpecialFolder(sf.ToString(), Environment.GetFolderPath(sf));
            }
            catch
            {
            }
        }

        private static void AddSpecialFolder(string token, string location)
        {
            if (location.EndsWith(@"\"))
            {
                location = location.Substring(0, location.Length - 1);
            }
            token = "<" + token + ">";
            _tokens.Add(token);
            _tokensToFolders[token] = location;
        }

        public static string DecodeFolder(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            if (Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).EndsWith(" (x86)", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Replace("<ProgramFiles> (x86)", "<ProgramFiles>");
            }
            s = s.Replace("<Personal>", "<MyDocuments>");
            string path = DecodeFolderInternal(s);
            if (!File.Exists(path) && s.Contains("<ProgramFiles>"))
            {
                string str3;
                if (ProgramFiles != ProgramFilesX86)
                {
                    str3 = DecodeFolderInternal(s.Replace("<ProgramFiles>", "<ProgramFilesX86>"));
                    if (File.Exists(str3))
                    {
                        return str3;
                    }
                }
                else if (ProgramFiles != ProgramFilesX64)
                {
                    str3 = DecodeFolderInternal(s.Replace("<ProgramFiles>", "<ProgramFilesX64>"));
                    if (File.Exists(str3))
                    {
                        return str3;
                    }
                }
            }
            return path;
        }

        private static string DecodeFolderInternal(string s)
        {
            Match match = _folderDecoder.Match(s);
            if (!match.Success)
            {
                return s;
            }
            string str2 = "";
            _tokensToFolders.TryGetValue(match.Value, out str2);
            if (!string.IsNullOrEmpty(str2))
            {
                str2 = str2 + @"\";
            }
            string filePath = _folderDecoder.Replace(s, str2 ?? "");
            if (filePath.Length > 2)
            {
                filePath = filePath[0] + filePath.Substring(1).Replace(@"\\", @"\");
            }
            return FixReference(filePath);
        }

        public static string EncodeFolder(string p)
        {
            if (!string.IsNullOrEmpty(p))
            {
                foreach (string str2 in _tokens)
                {
                    string str3 = _tokensToFolders[str2];
                    if (str3.Equals(p, StringComparison.OrdinalIgnoreCase) || p.StartsWith(str3 + @"\", StringComparison.OrdinalIgnoreCase))
                    {
                        return (str2 + p.Substring(str3.Length));
                    }
                }
            }
            return p;
        }

        private static string FixReference(string filePath)
        {
            string runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            string str2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Reference Assemblies\Microsoft\Framework\");
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (folderPath.EndsWith(@"\"))
            {
                folderPath = folderPath.Substring(0, folderPath.Length - 1);
            }
            if (!folderPath.EndsWith("(x86)", StringComparison.InvariantCultureIgnoreCase))
            {
                folderPath = folderPath + " (x86)";
            }
            string str4 = folderPath + @"\Reference Assemblies\Microsoft\Framework\";
            bool flag = filePath.StartsWith(runtimeDirectory, StringComparison.InvariantCultureIgnoreCase);
            bool flag2 = filePath.StartsWith(str2, StringComparison.InvariantCultureIgnoreCase);
            bool flag3 = filePath.StartsWith(str4, StringComparison.InvariantCultureIgnoreCase);
            if ((flag || flag2) || flag3)
            {
                string str8;
                if (File.Exists(filePath))
                {
                    if (flag)
                    {
                        return filePath;
                    }
                    if ((Environment.Version.Major == 2) && (filePath.IndexOf(@"\.NETFramework\v4.0\") == -1))
                    {
                        return filePath;
                    }
                    if (((Environment.Version.Major == 4) && (filePath.IndexOf(@"\Framework\v3.0\") == -1)) && (filePath.IndexOf(@"\Framework\v3.5\") == -1))
                    {
                        return filePath;
                    }
                }
                string fileName = Path.GetFileName(filePath);
                string path = Path.Combine(runtimeDirectory, fileName);
                if (File.Exists(path))
                {
                    return path;
                }
                path = Path.Combine(runtimeDirectory, @"wpf\" + fileName);
                if (File.Exists(path))
                {
                    return path;
                }
                if (Environment.Version.Major == 2)
                {
                    str8 = Path.Combine(str2, @"v3.5\" + fileName);
                    if (File.Exists(str8))
                    {
                        return str8;
                    }
                    str8 = Path.Combine(str2, @"v3.0\" + fileName);
                    if (File.Exists(str8))
                    {
                        return str8;
                    }
                    return filePath;
                }
                if (Environment.Version.Major == 4)
                {
                    str8 = Path.Combine(str2, @".NETFramework\v4.0\" + fileName);
                    if (File.Exists(str8))
                    {
                        return str8;
                    }
                    str8 = Path.Combine(str4, @".NETFramework\v4.0\" + fileName);
                    if (File.Exists(str8))
                    {
                        return str8;
                    }
                }
            }
            return filePath;
        }

        public static string GetAssemblyFromFolderIfPresent(string candidateFolder, string simpleName, string fullPathIfKnown)
        {
            string str;
            if (!string.IsNullOrEmpty(fullPathIfKnown))
            {
                str = Path.Combine(candidateFolder, Path.GetFileName(fullPathIfKnown));
                if (File.Exists(str))
                {
                    return str;
                }
            }
            str = Path.Combine(candidateFolder, simpleName + ".dll");
            if (File.Exists(str))
            {
                return str;
            }
            str = Path.Combine(candidateFolder, simpleName + ".exe");
            if (File.Exists(str))
            {
                return str;
            }
            return null;
        }

        public static bool PathHasInvalidChars(string path)
        {
            for (int i = 0; i < path.Length; i++)
            {
                int num2 = path[i];
                if (((num2 == 0x22) || (num2 == 60)) || (((num2 == 0x3e) || (num2 == 0x7c)) || (num2 < 0x20)))
                {
                    return true;
                }
            }
            return false;
        }

        public static string ResolveReference(string referencePath)
        {
            if (!referencePath.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                if (referencePath.EndsWith(".winmd", StringComparison.InvariantCultureIgnoreCase))
                {
                    return referencePath;
                }
                if (!referencePath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    referencePath = referencePath + ".dll";
                }
                if (File.Exists(referencePath))
                {
                    return FixReference(referencePath);
                }
                string path = FixReference(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), Path.GetFileName(referencePath)));
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return referencePath;
        }

        public static string ProgramFiles
        {
            get
            {
                if (_programFiles == null)
                {
                    _programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                }
                return _programFiles;
            }
        }

        public static string ProgramFilesX64
        {
            get
            {
                if (_programFilesX64 == null)
                {
                    _programFilesX64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    if (_programFilesX64.EndsWith("(x86)", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string path = _programFilesX64.Substring(0, _programFilesX64.Length - 5).Trim();
                        if (Directory.Exists(path))
                        {
                            _programFilesX64 = path;
                        }
                    }
                }
                return _programFilesX64;
            }
        }

        public static string ProgramFilesX86
        {
            get
            {
                if (_programFilesX86 == null)
                {
                    _programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    if (!(!string.IsNullOrEmpty(_programFilesX86) && Directory.Exists(_programFilesX86)))
                    {
                        _programFilesX86 = ProgramFiles;
                    }
                }
                return _programFilesX86;
            }
        }

        public static string Windows
        {
            get
            {
                if (_windows == null)
                {
                    _windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                }
                return _windows;
            }
        }
    }
}

