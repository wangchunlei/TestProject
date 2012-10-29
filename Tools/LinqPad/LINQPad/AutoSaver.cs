namespace LINQPad
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class AutoSaver
    {
        private static FileStream _backupHandle;
        private static bool _enabled;
        private static EventWaitHandle _waitHandle;

        public static RecoveryNode[] GetRecoveryData()
        {
            try
            {
                if (!Directory.Exists(Program.TempFolderBase))
                {
                    return new RecoveryNode[0];
                }
                var source = (from baseDir in new DirectoryInfo(Program.TempFolderBase).GetDirectories()
                    where baseDir.Exists
                    where IsAutoSaveFolderAbandoned(baseDir.Name)
                    orderby baseDir.LastWriteTimeUtc descending
                    select new { baseDir = baseDir, files = baseDir.GetFiles("*.autosave") }).ToArray();
                var typeArray2 = source;
                int index = 0;
                while (true)
                {
                    if (index >= typeArray2.Length)
                    {
                        break;
                    }
                    var type = typeArray2[index];
                    if (type.files.Length == 0)
                    {
                        try
                        {
                            type.baseDir.Delete(true);
                        }
                        catch
                        {
                        }
                    }
                    index++;
                }
                if (CS$<>9__CachedAnonymousMethodDelegatec == null)
                {
                    CS$<>9__CachedAnonymousMethodDelegatec = autoSave => autoSave.files;
                }
                return (from rNode in source.SelectMany(CS$<>9__CachedAnonymousMethodDelegatec, (autoSave, file) => new RecoveryNode(file.FullName))
                    where rNode.IsValid
                    select rNode).ToArray<RecoveryNode>();
            }
            catch (Exception exception)
            {
                Log.Write(exception);
                return new RecoveryNode[0];
            }
        }

        private static bool IsAutoSaveFolderAbandoned(string dirName)
        {
            if (string.Equals(dirName, Program.AppInstanceID, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            bool createdNew = false;
            try
            {
                using (new EventWaitHandle(false, EventResetMode.ManualReset, "LINQPad.Alive." + dirName, out createdNew))
                {
                    if (!createdNew)
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            try
            {
                string path = Path.Combine(Program.TempFolderBase, dirName + @"\handle");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static void Shutdown()
        {
            try
            {
                if (_backupHandle != null)
                {
                    _backupHandle.Close();
                }
                _backupHandle = null;
                if (_waitHandle != null)
                {
                    _waitHandle.Close();
                }
                _waitHandle = null;
            }
            catch (Exception exception)
            {
                Log.Write(exception, "AutoSaver.Shutdown");
            }
        }

        public static void Start()
        {
            if (_waitHandle == null)
            {
                Exception exception;
                try
                {
                    _waitHandle = new EventWaitHandle(true, EventResetMode.ManualReset, "LINQPad.Alive." + Program.AppInstanceID);
                }
                catch (Exception exception1)
                {
                    exception = exception1;
                    Log.Write(exception, "AutoSaver.Start");
                    return;
                }
                try
                {
                    if (!Directory.Exists(Program.TempFolder))
                    {
                        Directory.CreateDirectory(Program.TempFolder);
                    }
                    _backupHandle = File.Create(Path.Combine(Program.TempFolder, "handle"));
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    Log.Write(exception, "AutoSaver.Start");
                    return;
                }
                _enabled = true;
            }
        }

        public class AutoSaveToken : IDisposable
        {
            private string _filename = (TempFileRef.GetRandomName(8) + ".autosave");
            private FileStream _fileStream;
            private QueryCore _query;

            public AutoSaveToken(QueryCore q)
            {
                if (AutoSaver._enabled)
                {
                    this._query = q;
                    try
                    {
                        this._fileStream = File.Create(this.FilePath);
                    }
                    catch
                    {
                    }
                }
            }

            public void Dispose()
            {
                if (this.Enabled)
                {
                    try
                    {
                        this._fileStream.Close();
                    }
                    catch
                    {
                    }
                    this._fileStream = null;
                    try
                    {
                        if (File.Exists(this.FilePath))
                        {
                            File.Delete(this.FilePath);
                        }
                    }
                    catch
                    {
                        Thread.Sleep(100);
                        try
                        {
                            if (File.Exists(this.FilePath))
                            {
                                File.Delete(this.FilePath);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }

            public void Save(bool force)
            {
                if (this.Enabled)
                {
                    try
                    {
                        if (force || (this.LastAutoSave <= DateTimeOffset.Now.AddSeconds(-3.0)))
                        {
                            this._fileStream.Position = 0L;
                            this._fileStream.SetLength(0L);
                            StreamWriter sr = new StreamWriter(this._fileStream);
                            sr.WriteLine(this._query.FilePath ?? "");
                            sr.WriteLine(!this._query.FileWriteTimeUtc.HasValue ? "" : this._query.FileWriteTimeUtc.Value.ToString("s"));
                            this._query.WriteTo(sr, this._query.FilePath, true);
                            sr.Flush();
                            this._fileStream.Flush();
                            this.LastAutoSave = DateTimeOffset.Now;
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Write(exception);
                        this.Dispose();
                    }
                }
            }

            private bool Enabled
            {
                get
                {
                    return (this._fileStream != null);
                }
            }

            private string FilePath
            {
                get
                {
                    return Path.Combine(Program.TempFolder, this._filename);
                }
            }

            public DateTimeOffset LastAutoSave { get; private set; }
        }

        public class RecoveryNode : IDisposable
        {
            private StreamReader _reader;
            public readonly bool FileHasBeenTouched;
            public readonly bool IsValid;
            public readonly string QueryPath;
            public readonly string RecoveryPath;

            public RecoveryNode(string recoveryPath)
            {
                this.RecoveryPath = recoveryPath;
                FileStream stream = null;
                try
                {
                    stream = File.Open(recoveryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    this._reader = new StreamReader(stream);
                    this.QueryPath = this._reader.ReadLine();
                    if (this.QueryPath.EndsWith(".fw35.linq", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.RecoveryPath = null;
                        this.Dispose();
                    }
                    else
                    {
                        string s = this._reader.ReadLine();
                        if ((((s.Length > 0) && (this.QueryPath != null)) && (this.QueryPath.Length > 0)) && File.Exists(this.QueryPath))
                        {
                            DateTime time = DateTime.Parse(s);
                            TimeSpan span = (TimeSpan) (new FileInfo(this.QueryPath).LastWriteTimeUtc - time);
                            this.FileHasBeenTouched = span.TotalSeconds > 2.0;
                        }
                        this.IsValid = true;
                    }
                }
                catch
                {
                    if (stream != null)
                    {
                        try
                        {
                            stream.Close();
                        }
                        catch
                        {
                        }
                    }
                    this.Dispose();
                }
            }

            public void Dispose()
            {
                if (this._reader != null)
                {
                    try
                    {
                        this._reader.Close();
                    }
                    catch
                    {
                    }
                    this._reader = null;
                }
                try
                {
                    if ((this.RecoveryPath != null) && File.Exists(this.RecoveryPath))
                    {
                        File.Delete(this.RecoveryPath);
                    }
                }
                catch
                {
                    Thread.Sleep(100);
                    try
                    {
                        if (File.Exists(this.RecoveryPath))
                        {
                            File.Delete(this.RecoveryPath);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            private string GetQueryData()
            {
                string str;
                if (this._reader == null)
                {
                    return null;
                }
                try
                {
                    str = this._reader.ReadToEnd();
                }
                finally
                {
                    this.Dispose();
                }
                return str;
            }

            public RunnableQuery ToQuery()
            {
                RunnableQuery query = new RunnableQuery();
                if (!query.Recover(this.QueryPath, this.GetQueryData(), this.FileHasBeenTouched))
                {
                    return null;
                }
                return query;
            }
        }
    }
}

