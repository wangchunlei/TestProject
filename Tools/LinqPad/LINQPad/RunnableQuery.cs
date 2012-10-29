namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class RunnableQuery : QueryCore
    {
        private AppDomain _cachedDomain;
        private bool _cachedDomainPolluted;
        private Client _client;
        private long _cumulativeFootprint;
        private static bool _executeInLocalDomain;
        private object _executionLock = new object();
        private FileWithVersionInfo[] _lastNonvolatileRefs;
        private static List<TempFileRef> _lastRemotelyLoadedAssemblies = new List<TempFileRef>();
        private FileWithVersionInfo[] _lastVolatileRefs;
        private int _loadedAssemblyCount;
        private QueryCore _querySnapshot;
        private bool _queryStarting;
        private Stopwatch _queryWatch = new Stopwatch();
        private List<TempFileRef> _remotelyLoadedAssemblies = new List<TempFileRef>();
        private string _shadowFolder;
        public DateTime LastMoved;
        internal const int MaxAssemblies = 40;
        internal const int MaxFootprint = 0x1312d00;
        internal LINQPad.UI.PluginWindowManager PluginWindowManager;

        public event EventHandler CustomClickCompleted;

        public event EventHandler PluginsReady;

        public event EventHandler<QueryCompilationEventArgs> QueryCompiled;

        public event EventHandler<QueryStatusEventArgs> QueryCompleted;

        public event EventHandler<ReadLineEventArgs> ReadLineRequested;

        private void _client_CustomClickComplete(object sender, EventArgs e)
        {
            lock (this._executionLock)
            {
                if (sender != this._client)
                {
                    return;
                }
            }
            if (this.CustomClickCompleted != null)
            {
                this.CustomClickCompleted(this, e);
            }
        }

        private void _client_PluginsReady(object sender, EventArgs e)
        {
            lock (this._executionLock)
            {
                if (sender != this._client)
                {
                    return;
                }
            }
            this.OnPluginsReady();
        }

        private void _client_QueryCompiled(object sender, QueryCompilationEventArgs e)
        {
            lock (this._executionLock)
            {
                if (this._client == sender)
                {
                    this._queryStarting = false;
                    if (e.Errors.HasErrors)
                    {
                        this._queryWatch.Stop();
                    }
                    else
                    {
                        if (e.AssemblyPDB != null)
                        {
                            this.RegisterRemoteAssemblyFile(e.AssemblyPDB);
                        }
                        if (e.DataContextDLL != null)
                        {
                            this.RegisterRemoteAssemblyFile(e.DataContextDLL);
                        }
                        if (e.AssemblyDLL != null)
                        {
                            this.RegisterRemoteAssemblyFile(e.AssemblyDLL);
                        }
                        if (e.AssemblyPDB != null)
                        {
                            this.RegisterRemoteAssemblyFile(e.AssemblyPDB);
                        }
                        this.AddAssemblyPressure((int) new FileInfo(e.AssemblyDLL.FullPath).Length);
                        base.RequiresRecompilation = false;
                    }
                    if (this.QueryCompiled != null)
                    {
                        this.QueryCompiled(this, e);
                    }
                }
            }
        }

        private void _client_QueryCompleted(object sender, QueryStatusEventArgs e)
        {
            lock (this._executionLock)
            {
                if (sender != this._client)
                {
                    return;
                }
                if (e.ExecutionComplete)
                {
                    this._queryWatch.Stop();
                }
                this._queryStarting = false;
                if (e.AppDomainRecycleSuggested)
                {
                    this.PolluteCachedDomain(false);
                }
            }
            this.OnQueryCompleted(e);
            if (e.DataContextRefreshRequired && this._querySnapshot.Repository.IsEquivalent(base.Repository))
            {
                DataContextManager.RefreshDataContextInfo(base.Repository, SchemaChangeTestMode.TestAndFailNegative);
            }
        }

        private void _client_ReadLineRequested(object sender, ReadLineEventArgs e)
        {
            lock (this._executionLock)
            {
                if (this._client != e.Client)
                {
                    return;
                }
            }
            this.OnReadLineRequested(e);
        }

        internal void AddAssemblyPressure(int bytes)
        {
            lock (this._executionLock)
            {
                this._loadedAssemblyCount++;
                this._cumulativeFootprint += bytes;
            }
        }

        public bool AllowShadow()
        {
            return (!UserOptions.Instance.LockReferenceAssemblies && (((base.Repository == null) || (base.Repository.DriverLoader == null)) || base.Repository.DriverLoader.AllowAssemblyShadowing));
        }

        public void CallCustomClick(int id)
        {
            new Thread(delegate {
                try
                {
                    Client client = this._client;
                    if (client != null)
                    {
                        client.CallCustomClick(id);
                    }
                }
                catch (AppDomainUnloadedException)
                {
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }) { IsBackground = true, Name = "Custom Click " + id }.Start();
        }

        public void Cancel(bool onlyIfRunning, bool killAppDomain)
        {
            lock (this._executionLock)
            {
                if ((this._client != null) && (this.IsRunning || !onlyIfRunning))
                {
                    this._queryWatch.Stop();
                    this._client.Cancel(onlyIfRunning, killAppDomain);
                    if (killAppDomain)
                    {
                        this._client.Dispose();
                        this._client = null;
                        this._cachedDomain = null;
                    }
                    this._queryStarting = false;
                }
            }
        }

        private void CopyFileToShadow(string lastShadow, FileWithVersionInfo r)
        {
            string fileName = Path.GetFileName(r.FilePath);
            string destFileName = Path.Combine(this._shadowFolder, fileName);
            if (lastShadow != null)
            {
                try
                {
                    string str3 = Path.Combine(lastShadow, fileName);
                    FileInfo info = new FileInfo(str3);
                    if ((info.Exists && (info.LastWriteTimeUtc == r.LastWriteTimeUtc)) && (info.Length == r.Length))
                    {
                        using (File.Open(str3, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                        }
                        File.Move(str3, destFileName);
                        return;
                    }
                }
                catch
                {
                }
            }
            try
            {
                File.Copy(r.FilePath, destFileName);
            }
            catch (IOException exception)
            {
                if (!exception.Message.Contains(destFileName))
                {
                    throw;
                }
                string message = "Shadow-copy error: " + exception.Message;
                try
                {
                    FileInfo info2 = new FileInfo(r.FilePath);
                    FileInfo info3 = new FileInfo(destFileName);
                    if (info3.Exists)
                    {
                        object obj2 = message;
                        message = string.Concat(new object[] { obj2, " [new:", info3.LastWriteTime, ",", info3.Length, "]" });
                        obj2 = message;
                        message = string.Concat(new object[] { obj2, " [old:", info2.LastWriteTime, ",", info2.Length, "]" });
                    }
                }
                catch
                {
                }
                throw new Exception(message, exception);
            }
        }

        private AppDomain CreateAppDomain(Repository repository, string name)
        {
            AppDomain domain;
            this._loadedAssemblyCount = 0;
            this._cumulativeFootprint = 0L;
            this._cachedDomainPolluted = false;
            _lastRemotelyLoadedAssemblies = this._remotelyLoadedAssemblies;
            this._remotelyLoadedAssemblies = new List<TempFileRef>();
            string data = this.AllowShadow() ? this._shadowFolder : null;
            if (repository != null)
            {
                domain = repository.CreateSchemaAndRunnerDomain("LINQPad Query Server [" + name + "]", data != null, true);
            }
            else
            {
                domain = AppDomainUtil.CreateDomain("LINQPad Query Server [" + name + "]");
            }
            if (data != null)
            {
                domain.SetData("LINQPad.ShadowPath", data);
            }
            return domain;
        }

        internal Server CreateServer(Client client)
        {
            lock (this._executionLock)
            {
                if (client != this._client)
                {
                    return null;
                }
                if (!_executeInLocalDomain)
                {
                    if (this._cachedDomain == null)
                    {
                        this._cachedDomain = this.CreateAppDomain(base.Repository, base.Name);
                    }
                    try
                    {
                        return this.GetServer();
                    }
                    catch (AppDomainUnloadedException)
                    {
                        this._cachedDomain = this.CreateAppDomain(base.Repository, base.Name);
                        return this.GetServer();
                    }
                }
                return new Server();
            }
        }

        private void CreateShadow()
        {
            string lastShadow = this._shadowFolder;
            if ((this._lastVolatileRefs != null) && (this._lastVolatileRefs.Length > 0))
            {
                do
                {
                    this._shadowFolder = Path.Combine(Program.TempFolder, "shadow_" + TempFileRef.GetRandomName(6));
                }
                while (Directory.Exists(this._shadowFolder));
                Directory.CreateDirectory(this._shadowFolder);
                foreach (FileWithVersionInfo info in this._lastVolatileRefs)
                {
                    this.CopyFileToShadow(lastShadow, info);
                }
            }
            else
            {
                this._shadowFolder = null;
            }
            if (lastShadow != null)
            {
                this.DeleteShadow(lastShadow);
            }
        }

        private void DeleteShadow(string shadowFolder)
        {
            Program.RunOnThreadingTimer(delegate {
                Action a = null;
                try
                {
                    Directory.Delete(shadowFolder, true);
                }
                catch
                {
                    if (a == null)
                    {
                        a = delegate {
                            try
                            {
                                Directory.Delete(shadowFolder, true);
                            }
                            catch
                            {
                            }
                        };
                    }
                    Program.RunOnThreadingTimer(a, 0x2710);
                }
            }, 0x7d0);
        }

        public override void Dispose()
        {
            if (!base.IsDisposed)
            {
                this.QueryCompiled = null;
                this.QueryCompleted = null;
                this.PluginsReady = null;
                this.CustomClickCompleted = null;
                this.ReadLineRequested = null;
                _lastRemotelyLoadedAssemblies = this._remotelyLoadedAssemblies;
                lock (this._executionLock)
                {
                    this.Cancel(false, true);
                }
                if (this._shadowFolder != null)
                {
                    this.DeleteShadow(this._shadowFolder);
                }
                base.Dispose();
            }
        }

        private void GetAssemblyReferenceMap(out FileWithVersionInfo[] volatileRefs, out FileWithVersionInfo[] nonvolatileRefs)
        {
            string fileName = null;
            UniqueStringCollection strings = new UniqueStringCollection(base.AllFileReferences, new FileNameComparer());
            strings.AddRange(PluginAssembly.GetCompatibleAssemblies(base.IsMyExtensions));
            if ((base.Repository != null) && !base.Repository.DynamicSchema)
            {
                strings.Add(base.Repository.CustomAssemblyPath);
                strings.AddRange(base.Repository.GetDriverAssemblies());
                if (base.Repository.DriverLoader.IsValid && (base.Repository.DriverLoader.InternalID == null))
                {
                    try
                    {
                        fileName = Path.GetFileName(base.Repository.DriverLoader.GetAssemblyPath());
                    }
                    catch
                    {
                    }
                }
            }
            List<FileWithVersionInfo> list = new List<FileWithVersionInfo>();
            List<string> assemblyFilePaths = new List<string>();
            foreach (string str2 in strings)
            {
                try
                {
                    if (Path.IsPathRooted(str2) && File.Exists(str2))
                    {
                        if (!(ShadowAssemblyManager.IsShadowable(str2) && !Path.GetFileName(str2).Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(new FileWithVersionInfo(str2));
                        }
                        else
                        {
                            assemblyFilePaths.Add(str2);
                        }
                    }
                }
                catch
                {
                }
            }
            nonvolatileRefs = (from r in list
                orderby r.FilePath
                select r).ToArray<FileWithVersionInfo>();
            volatileRefs = AssemblyProber.GetSameFolderAssemblyReferenceChain(assemblyFilePaths);
        }

        public ExecutionTrackInfo GetMainThreadPosition(bool interactive)
        {
            if (this.ExecutionProgress != LINQPad.ExecutionModel.ExecutionProgress.Executing)
            {
                return null;
            }
            Client client = this._client;
            if (client == null)
            {
                return null;
            }
            return client.GetMainThreadPosition(interactive);
        }

        public ResultData GetResultData()
        {
            try
            {
                Client client = this._client;
                if (client == null)
                {
                    return null;
                }
                return client.GetResults();
            }
            catch
            {
                return null;
            }
        }

        private Server GetServer()
        {
            this._cachedDomain.DoCallBack(new CrossAppDomainDelegate(Server.Create));
            return (Server) this._cachedDomain.GetData("instance");
        }

        private QueryCore GetSnapshotForExecution()
        {
            QueryCore core = new QueryCore();
            using (core.TransactChanges())
            {
                core.AdditionalGACReferences = base.AdditionalGACReferences;
                core.AdditionalNamespaces = base.AdditionalNamespaces;
                core.AdditionalReferences = base.AdditionalReferences;
                core.IncludePredicateBuilder = base.IncludePredicateBuilder;
                core.ToDataGrids = base.ToDataGrids;
                core.FilePath = base.FilePath;
                core.Name = base.Name;
                core.QueryKind = base.QueryKind;
                core.Repository = base.Repository;
                core.Source = base.Source;
                core.IsMyExtensions = base.IsMyExtensions;
            }
            return core;
        }

        public bool HaveResultsChanged()
        {
            bool flag2;
            try
            {
                lock (this._executionLock)
                {
                    if (!((this._client != null) && this._client.HaveResultsChanged()))
                    {
                        return false;
                    }
                    flag2 = true;
                }
            }
            catch
            {
                flag2 = false;
            }
            return flag2;
        }

        private bool IsUserCachePresent()
        {
            lock (this._executionLock)
            {
                return ((this._client != null) && this._client.IsUserCachePresent());
            }
        }

        private bool NeedsRefresh(FileWithVersionInfo[] oldRefs, FileWithVersionInfo[] newRefs)
        {
            if (oldRefs == null)
            {
                return true;
            }
            HashSet<string> oldNames = new HashSet<string>(from r in oldRefs select r.FilePath, StringComparer.InvariantCultureIgnoreCase);
            return !(from r in newRefs
                where oldNames.Contains(r.FilePath)
                select r).SequenceEqual<FileWithVersionInfo>(oldRefs);
        }

        private void OnPluginsReady()
        {
            if (this.PluginsReady != null)
            {
                this.PluginsReady(this, EventArgs.Empty);
            }
        }

        protected override void OnQueryChanged(QueryChangedEventArgs args)
        {
            if (!base.IsDisposed)
            {
                base.OnQueryChanged(args);
                if (args.DbChanged)
                {
                    this.PolluteCachedDomain(false);
                }
            }
        }

        private void OnQueryCompleted(QueryStatusEventArgs args)
        {
            if (this.QueryCompleted != null)
            {
                this.QueryCompleted(this, args);
            }
        }

        internal void OnReadLineRequested(ReadLineEventArgs args)
        {
            if (this.ReadLineRequested != null)
            {
                this.ReadLineRequested(this, args);
            }
        }

        internal void PolluteCachedDomain(bool evenInPreserveMode)
        {
            if (evenInPreserveMode || !Program.PreserveAppDomains)
            {
                lock (this._executionLock)
                {
                    if (this._cachedDomain != null)
                    {
                        this._cachedDomainPolluted = true;
                    }
                }
            }
        }

        internal void ReadLineCompleted(Client client, string text)
        {
            lock (this._executionLock)
            {
                if (this._client == client)
                {
                    this._client.ReadLineCompleted(text);
                }
            }
        }

        internal void RegisterRemoteAssemblyFile(TempFileRef a)
        {
            lock (this._executionLock)
            {
                if (!this._remotelyLoadedAssemblies.Contains(a))
                {
                    this._remotelyLoadedAssemblies.Add(a);
                }
            }
        }

        public void Run(string querySelection, bool compileOnly, QueryCompilationEventArgs lastCompiler, LINQPad.UI.PluginWindowManager winManager)
        {
            lock (this._executionLock)
            {
                FileWithVersionInfo[] infoArray;
                FileWithVersionInfo[] infoArray2;
                if (this.IsRunning)
                {
                    return;
                }
                this.GetAssemblyReferenceMap(out infoArray, out infoArray2);
                bool flag2 = this.NeedsRefresh(this._lastVolatileRefs, infoArray);
                bool flag3 = this.NeedsRefresh(this._lastNonvolatileRefs, infoArray2);
                bool flag4 = (!flag2 && (this._lastVolatileRefs != null)) && (this._lastVolatileRefs.Length > infoArray.Length);
                this._lastVolatileRefs = infoArray;
                this._lastNonvolatileRefs = infoArray2;
                if (flag2 || flag3)
                {
                    this.PolluteCachedDomain(true);
                }
                if (this._client != null)
                {
                    if ((this._cachedDomainPolluted || Program.FreshAppDomains) || ((!Program.PreserveAppDomains && !this.IsUserCachePresent()) && (((this._loadedAssemblyCount > 40) || (this._cumulativeFootprint > 0x1312d00L)) || this._client.LastWorkerThreadAborted())))
                    {
                        this._client.Cancel(false, true);
                        this._cachedDomain = null;
                    }
                    this._client.Dispose();
                }
                this._queryStarting = true;
                this._querySnapshot = this.GetSnapshotForExecution();
                if ((querySelection != null) && (this._querySnapshot.QueryKind == QueryLanguage.Program))
                {
                    if (querySelection.TrimEnd(new char[0]).EndsWith(";"))
                    {
                        this._querySnapshot.QueryKind = QueryLanguage.Statements;
                    }
                    else if (!(querySelection.Trim().Contains("\n") && querySelection.Contains<char>('{')))
                    {
                        this._querySnapshot.QueryKind = QueryLanguage.Expression;
                    }
                }
                else if (((querySelection != null) && (this._querySnapshot.QueryKind == QueryLanguage.Statements)) && !querySelection.Contains<char>(';'))
                {
                    this._querySnapshot.QueryKind = QueryLanguage.Expression;
                }
                this._client = new Client(this._querySnapshot, querySelection, compileOnly, lastCompiler, new Func<Client, Server>(this.CreateServer), winManager);
                this._client.QueryCompiled += new EventHandler<QueryCompilationEventArgs>(this._client_QueryCompiled);
                this._client.QueryCompleted += new EventHandler<QueryStatusEventArgs>(this._client_QueryCompleted);
                this._client.ReadLineRequested += new EventHandler<ReadLineEventArgs>(this._client_ReadLineRequested);
                this._client.PluginsReady += new EventHandler(this._client_PluginsReady);
                this._client.CustomClickComplete += new EventHandler(this._client_CustomClickComplete);
                if (this.AllowShadow())
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    if (flag2)
                    {
                        this.CreateShadow();
                    }
                    else if (flag4)
                    {
                        this.UpdateShadow();
                    }
                    stopwatch.Stop();
                }
            }
            ThreadPool.QueueUserWorkItem(delegate (object _) {
                try
                {
                    base.AutoSave(true);
                }
                catch
                {
                }
            });
            new Thread(new ThreadStart(this._client.Start)) { IsBackground = true, Name = "Query Starter" }.Start();
            this._queryWatch.Reset();
            this._queryWatch.Start();
        }

        public void SuppressErrorsOnExistingClient()
        {
            lock (this._executionLock)
            {
                if (this._client != null)
                {
                    this._client.SuppressErrors();
                }
            }
        }

        private void UpdateShadow()
        {
            foreach (FileWithVersionInfo info in this._lastVolatileRefs)
            {
                string fileName = Path.GetFileName(info.FilePath);
                string path = Path.Combine(this._shadowFolder, fileName);
                if (!File.Exists(path))
                {
                    File.Copy(info.FilePath, path);
                }
            }
        }

        public LINQPad.ExecutionModel.ExecutionProgress ExecutionProgress
        {
            get
            {
                lock (this._executionLock)
                {
                    if (this._client == null)
                    {
                        return (this._queryStarting ? LINQPad.ExecutionModel.ExecutionProgress.Starting : LINQPad.ExecutionModel.ExecutionProgress.Finished);
                    }
                    return this._client.ExecutionProgress;
                }
            }
        }

        public bool HasDomain
        {
            get
            {
                lock (this._executionLock)
                {
                    return (this._cachedDomain != null);
                }
            }
        }

        public bool IsRunning
        {
            get
            {
                lock (this._executionLock)
                {
                    return ((this._client != null) && !this._client.Finished);
                }
            }
        }

        public bool MessageLoopEnded
        {
            get
            {
                lock (this._executionLock)
                {
                    return (((this._cachedDomain == null) || (this._client == null)) || this._client.MessageLoopEnded);
                }
            }
        }

        public bool MessageLoopStartedWithoutForm
        {
            get
            {
                lock (this._executionLock)
                {
                    return (((this._cachedDomain != null) && (this._client != null)) && this._client.MessageLoopStartedWithoutForm);
                }
            }
        }

        public int? Progress
        {
            get
            {
                lock (this._executionLock)
                {
                    return ((this._client == null) ? null : this._client.Progress);
                }
            }
        }

        public TimeSpan TotalRunningTime
        {
            get
            {
                lock (this._executionLock)
                {
                    return this._queryWatch.Elapsed;
                }
            }
        }
    }
}

