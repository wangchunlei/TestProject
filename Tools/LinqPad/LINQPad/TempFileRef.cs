namespace LINQPad
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;

    internal class TempFileRef
    {
        private int _deleteAttempt;
        private static RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        public bool AutoDelete = true;
        public readonly string FileName;
        public readonly string FullPath;

        public TempFileRef(string fileOrPath)
        {
            if (Path.IsPathRooted(fileOrPath))
            {
                this.FullPath = fileOrPath;
            }
            else
            {
                this.FullPath = Path.Combine(Program.TempFolder, fileOrPath);
            }
            this.FileName = Path.GetFileName(this.FullPath);
        }

        public static void DeleteAll()
        {
            if (!Debugger.IsAttached)
            {
                new Thread(delegate {
                    try
                    {
                        Directory.Delete(Program.TempFolder, true);
                    }
                    catch
                    {
                        Thread.Sleep(300);
                        try
                        {
                            Directory.Delete(Program.TempFolder, true);
                        }
                        catch
                        {
                        }
                    }
                    string[] files = Directory.GetFiles(Program.TempFolderBase, "query_*.*");
                    int index = 0;
                    while (true)
                    {
                        if (index >= files.Length)
                        {
                            break;
                        }
                        str = files[index];
                        try
                        {
                            File.Delete(str);
                        }
                        catch
                        {
                        }
                        index++;
                    }
                    foreach (string str in Directory.GetFiles(Program.TempFolderBase, "TypedDataContext_*.*"))
                    {
                        try
                        {
                            File.Delete(str);
                        }
                        catch
                        {
                        }
                    }
                }) { Name = "Temp File Cleanup" }.Start();
            }
        }

        ~TempFileRef()
        {
            if (this.AutoDelete && !Debugger.IsAttached)
            {
                try
                {
                    if (File.Exists(this.FullPath))
                    {
                        try
                        {
                            File.Delete(this.FullPath);
                        }
                        catch
                        {
                            if (this._deleteAttempt++ < 5)
                            {
                                GC.ReRegisterForFinalize(this);
                            }
                            else
                            {
                                Log.Write("Unable to delete file " + this.FullPath + " in finalizer");
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public static TempFileRef GetRandom(string prefix, string suffix)
        {
            return new TempFileRef(prefix + "_" + GetRandomName(6) + suffix);
        }

        public static string GetRandomName(int length)
        {
            byte[] data = new byte[length];
            lock (_rng)
            {
                _rng.GetBytes(data);
            }
            char[] chArray = new char[data.Length];
            int num = 0;
            foreach (byte num3 in data)
            {
                chArray[num++] = (char) (0x61 + (num3 % 0x1a));
            }
            return new string(chArray);
        }
    }
}

