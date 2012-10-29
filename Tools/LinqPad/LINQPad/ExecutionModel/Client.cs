namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Remoting;
    using System.Threading;

    internal class Client : MarshalByRefObject, IDisposable
    {
        private bool _cancelRequest;
        private bool _compileOnly;
        private LINQPad.ExecutionModel.ExecutionProgress _executionProgress;
        private bool _isDisposed;
        private QueryCompilationEventArgs _lastCompilation;
        private object _locker = new object();
        private bool _partialSource;
        private PluginWindowManager _pluginWinManager;
        private QueryCore _query;
        private Server _server;
        private Func<Client, Server> _serverGenerator;
        private string _source;

        public event EventHandler CustomClickComplete;

        public event EventHandler PluginsReady;

        public event EventHandler<QueryCompilationEventArgs> QueryCompiled;

        public event EventHandler<QueryStatusEventArgs> QueryCompleted;

        public event EventHandler<ReadLineEventArgs> ReadLineRequested;

        public Client(QueryCore query, string querySelection, bool compileOnly, QueryCompilationEventArgs lastCompilation, Func<Client, Server> serverGenerator, PluginWindowManager pluginWinManager)
        {
            this._query = query;
            this._partialSource = !string.IsNullOrEmpty(querySelection);
            this._source = this._partialSource ? querySelection : this._query.Source;
            this._compileOnly = compileOnly;
            this._lastCompilation = lastCompilation;
            this._serverGenerator = serverGenerator;
            this._pluginWinManager = pluginWinManager;
        }

        public void CallCustomClick(int id)
        {
            Server server;
            lock (this._locker)
            {
                server = this._server;
            }
            if (server != null)
            {
                server.CallCustomClick(id);
            }
        }

        public void Cancel(bool onlyIfRunning, bool killAppDomain)
        {
            Server server;
            lock (this._locker)
            {
                this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                if (!(killAppDomain || !this._cancelRequest))
                {
                    return;
                }
                this._cancelRequest = true;
                server = this._server;
            }
            if (server != null)
            {
                server.Cancel(onlyIfRunning, killAppDomain);
            }
            if (killAppDomain)
            {
                this.ClearServer();
            }
        }

        private void ClearServer()
        {
            try
            {
                if (this._server != null)
                {
                    this._server.Dispose();
                }
            }
            catch
            {
            }
            this._server = null;
        }

        private void CompileAndRun(DataContextInfo dcInfo)
        {
            try
            {
                object obj2;
                QueryCore core;
                lock ((obj2 = this._locker))
                {
                    core = this._query;
                    if ((core == null) || this._cancelRequest)
                    {
                        return;
                    }
                }
                if ((dcInfo != null) && (dcInfo.Error != null))
                {
                    this.OnQueryCompleted("Connection error", dcInfo.Error);
                }
                else
                {
                    DataContextDriver driver = core.GetDriver(true);
                    if ((driver != null) && (driver.AssembliesToAddError != null))
                    {
                        this.OnQueryCompleted("Error loading custom driver assemblies", driver.AssembliesToAddError.Message);
                    }
                    else
                    {
                        QueryCompiler c = null;
                        if (((this._lastCompilation != null) && (this._lastCompilation.AssemblyDLL != null)) && File.Exists(this._lastCompilation.AssemblyDLL.FullPath))
                        {
                            c = this._lastCompilation.Compiler;
                            if (!((((dcInfo != null) || (this._lastCompilation.DataContextDLL == null)) && ((dcInfo == null) || (this._lastCompilation.DataContextDLL != null))) ? (((dcInfo == null) || (this._lastCompilation.DataContextDLL == null)) || (dcInfo.AssemblyPath == this._lastCompilation.DataContextDLL)) : false))
                            {
                                c = null;
                            }
                        }
                        bool flag2 = false;
                        if (c == null)
                        {
                            this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Compiling;
                            c = QueryCompiler.Create(core, true);
                            lock ((obj2 = this._locker))
                            {
                                if (this._cancelRequest)
                                {
                                    return;
                                }
                            }
                            c.Compile(this._source, core, (dcInfo == null) ? null : dcInfo.AssemblyPath);
                            lock ((obj2 = this._locker))
                            {
                                if (this._cancelRequest)
                                {
                                    return;
                                }
                                if (c.Errors.HasErrors)
                                {
                                    this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                                    this.QueryCompleted = null;
                                    this.PluginsReady = null;
                                    this.CustomClickComplete = null;
                                }
                            }
                            this.OnQueryCompiled(new QueryCompilationEventArgs(c, (dcInfo == null) ? null : dcInfo.AssemblyPath, this._partialSource));
                            flag2 = true;
                        }
                        lock ((obj2 = this._locker))
                        {
                            if ((this._cancelRequest || c.Errors.HasErrors) || this._compileOnly)
                            {
                                this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                                return;
                            }
                        }
                        Server server = this._serverGenerator(this);
                        if (server != null)
                        {
                            lock ((obj2 = this._locker))
                            {
                                if (this._cancelRequest)
                                {
                                    return;
                                }
                                if (this._server != server)
                                {
                                    this.ClearServer();
                                }
                                this._server = server;
                                this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Executing;
                            }
                            bool flag7 = c.References.Any<string>(r => r.EndsWith(".winmd", StringComparison.InvariantCultureIgnoreCase));
                            server.WriteResultsToGrids = core.ToDataGrids;
                            server.ExecuteClrQuery(this, core.Repository, c.OutputFile.FullPath, c.LineOffset, c.References.ToArray<string>(), Program.MTAMode || flag7, flag2 && c.Errors.HasWarnings, core.FilePath, core.Name, this._pluginWinManager, !UserOptionsLive.Instance.ExecutionTrackingDisabled);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                try
                {
                    this.OnQueryCompleted("Unable to execute query", "");
                }
                catch
                {
                }
                Program.ProcessException(exception);
            }
        }

        public void Dispose()
        {
            lock (this._locker)
            {
                if (!this._isDisposed)
                {
                    this._isDisposed = true;
                    this.ReadLineRequested = null;
                    this.QueryCompiled = null;
                    this.QueryCompleted = null;
                    this.PluginsReady = null;
                    this.CustomClickComplete = null;
                    this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                    this._cancelRequest = true;
                    this.ClearServer();
                    this._query = null;
                    this._source = null;
                    try
                    {
                        RemotingServices.Disconnect(this);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public ExecutionTrackInfo GetMainThreadPosition(bool interactive)
        {
            Server server;
            lock (this._locker)
            {
                server = this._server;
            }
            if (server == null)
            {
                return null;
            }
            try
            {
                return server.GetMainThreadPosition(interactive);
            }
            catch
            {
                return null;
            }
        }

        public ResultData GetResults()
        {
            Server server;
            lock (this._locker)
            {
                server = this._server;
                if ((server == null) || this._cancelRequest)
                {
                    return null;
                }
            }
            try
            {
                return server.GetResults(true);
            }
            catch (AppDomainUnloadedException)
            {
                return null;
            }
        }

        private void GotDataContext(DataContextInfo dcInfo)
        {
            lock (this._locker)
            {
                if (this._cancelRequest)
                {
                    return;
                }
            }
            new Thread(() => this.CompileAndRun(dcInfo)) { Name = "DataContext Query Compiler", IsBackground = true }.Start();
        }

        internal bool HasUserCacheChanged()
        {
            try
            {
                return ((this._server != null) && this._server.HasUserCacheChanged);
            }
            catch
            {
                return false;
            }
        }

        public bool HaveResultsChanged()
        {
            Server server;
            lock (this._locker)
            {
                server = this._server;
                if ((server == null) || this._cancelRequest)
                {
                    return false;
                }
            }
            try
            {
                return server.HaveResultsChanged();
            }
            catch (AppDomainUnloadedException)
            {
                return false;
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal bool IsUserCachePresent()
        {
            try
            {
                return ((this._server != null) && this._server.IsUserCachePresent);
            }
            catch
            {
                return false;
            }
        }

        public bool LastWorkerThreadAborted()
        {
            try
            {
                return ((this._server != null) && this._server.WorkerThreadAborted);
            }
            catch
            {
                return true;
            }
        }

        internal void OnCustomClickComplete()
        {
            EventHandler customClickComplete = this.CustomClickComplete;
            if (customClickComplete != null)
            {
                customClickComplete(this, EventArgs.Empty);
            }
        }

        internal void OnPluginsReady()
        {
            EventHandler pluginsReady = this.PluginsReady;
            if (pluginsReady != null)
            {
                pluginsReady(this, EventArgs.Empty);
            }
            this.PluginsReady = null;
        }

        protected virtual void OnQueryCompiled(QueryCompilationEventArgs e)
        {
            EventHandler<QueryCompilationEventArgs> queryCompiled = this.QueryCompiled;
            if (queryCompiled != null)
            {
                queryCompiled(this, e);
            }
            this.QueryCompiled = null;
        }

        protected virtual void OnQueryCompleted(QueryStatusEventArgs e)
        {
            lock (this._locker)
            {
                if (e.ExecutionComplete)
                {
                    this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                }
                else if (e.Async && (this._executionProgress < LINQPad.ExecutionModel.ExecutionProgress.Finished))
                {
                    this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Async;
                }
            }
            EventHandler<QueryStatusEventArgs> queryCompleted = this.QueryCompleted;
            if (queryCompleted != null)
            {
                queryCompleted(this, e);
            }
            this.QueryCompiled = null;
        }

        private void OnQueryCompleted(string statusMessage, string errorMessage)
        {
            this.OnQueryCompleted(new QueryStatusEventArgs(statusMessage, errorMessage));
        }

        internal void OnQueryStatusChanged(QueryStatusEventArgs args)
        {
            lock (this._locker)
            {
                if (this._cancelRequest)
                {
                    return;
                }
                if (args.ExecutionComplete)
                {
                    this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                }
            }
            this.OnQueryCompleted(args);
        }

        internal void OnReadLineRequested(string prompt, string defaultValue, string[] options)
        {
            lock (this._locker)
            {
                if (this._cancelRequest)
                {
                    return;
                }
            }
            EventHandler<ReadLineEventArgs> readLineRequested = this.ReadLineRequested;
            if (readLineRequested != null)
            {
                ReadLineEventArgs e = new ReadLineEventArgs {
                    Client = this,
                    Prompt = prompt,
                    DefaultValue = defaultValue,
                    Options = options
                };
                readLineRequested(this, e);
            }
        }

        internal void ReadLineCompleted(string text)
        {
            Server server;
            lock (this._locker)
            {
                server = this._server;
                if (this._cancelRequest)
                {
                    return;
                }
            }
            if (server != null)
            {
                try
                {
                    server.ReadLineCompleted(text);
                }
                catch (AppDomainUnloadedException)
                {
                }
            }
        }

        public void Start()
        {
            ThreadStart start = null;
            object obj2;
            QueryCore core;
            lock ((obj2 = this._locker))
            {
                core = this._query;
                if ((core == null) || this._cancelRequest)
                {
                    return;
                }
                if (this._executionProgress != LINQPad.ExecutionModel.ExecutionProgress.Starting)
                {
                    throw new InvalidOperationException("Cannot call Start twice on Client");
                }
            }
            if ((core.QueryKind == QueryLanguage.SQL) || (core.QueryKind == QueryLanguage.ESQL))
            {
                if (this._compileOnly)
                {
                    this.OnQueryCompleted("", "");
                }
                else
                {
                    lock ((obj2 = this._locker))
                    {
                        this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Executing;
                    }
                    Server server = this._serverGenerator(this);
                    lock ((obj2 = this._locker))
                    {
                        if (this._cancelRequest || (server == null))
                        {
                            return;
                        }
                        if (this._server != server)
                        {
                            this.ClearServer();
                        }
                        this._server = server;
                    }
                    server.WriteResultsToGrids = core.ToDataGrids && (core.QueryKind == QueryLanguage.SQL);
                    UniqueStringCollection source = new UniqueStringCollection(new FileNameComparer());
                    if (core.GetDriver(true) != null)
                    {
                        try
                        {
                            source.AddRange(core.Repository.GetDriverAssemblies());
                            if (!((core.QueryKind != QueryLanguage.ESQL) || string.IsNullOrEmpty(core.Repository.CustomAssemblyPath)))
                            {
                                source.Add(core.Repository.CustomAssemblyPath);
                            }
                        }
                        catch
                        {
                        }
                    }
                    server.ExecuteSqlQuery(core.QueryKind, core.Repository, this._source, source.ToArray<string>(), this, this._pluginWinManager);
                }
            }
            else if ((core.Repository != null) && core.Repository.DynamicSchema)
            {
                lock ((obj2 = this._locker))
                {
                    this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.AwaitingDataContext;
                }
                DataContextManager.GetDataContextInfo(core.Repository, new DataContextCallback(this.GotDataContext), SchemaChangeTestMode.None);
            }
            else
            {
                if (start == null)
                {
                    start = () => this.CompileAndRun(null);
                }
                new Thread(start) { Name = "Query Compiler", IsBackground = true }.Start();
            }
        }

        public void SuppressErrors()
        {
            try
            {
                if (this._server != null)
                {
                    this._server.SuppressErrors();
                }
            }
            catch
            {
            }
        }

        private void ValidateExecutionProgress()
        {
            lock (this._locker)
            {
                if ((this._executionProgress == LINQPad.ExecutionModel.ExecutionProgress.Executing) || (this._executionProgress == LINQPad.ExecutionModel.ExecutionProgress.Async))
                {
                    if (this._server == null)
                    {
                        this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                    }
                    else
                    {
                        try
                        {
                            if (this._server.Finished)
                            {
                                this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                            }
                        }
                        catch (AppDomainUnloadedException)
                        {
                            this._executionProgress = LINQPad.ExecutionModel.ExecutionProgress.Finished;
                        }
                    }
                }
            }
        }

        public LINQPad.ExecutionModel.ExecutionProgress ExecutionProgress
        {
            get
            {
                lock (this._locker)
                {
                    this.ValidateExecutionProgress();
                    return this._executionProgress;
                }
            }
        }

        public bool Finished
        {
            get
            {
                return (this.ExecutionProgress == LINQPad.ExecutionModel.ExecutionProgress.Finished);
            }
        }

        public bool MessageLoopEnded
        {
            get
            {
                Server server;
                lock (this._locker)
                {
                    server = this._server;
                    if ((server == null) || this._cancelRequest)
                    {
                        return true;
                    }
                }
                try
                {
                    return server.MessageLoopEnded;
                }
                catch (AppDomainUnloadedException)
                {
                    return true;
                }
            }
        }

        public bool MessageLoopStartedWithoutForm
        {
            get
            {
                Server server;
                lock (this._locker)
                {
                    server = this._server;
                    if ((server == null) || this._cancelRequest)
                    {
                        return false;
                    }
                }
                try
                {
                    return server.MessageLoopStartedWithoutForm;
                }
                catch (AppDomainUnloadedException)
                {
                    return false;
                }
            }
        }

        public int? Progress
        {
            get
            {
                int? progress;
                lock (this._locker)
                {
                    try
                    {
                        if (this._server == null)
                        {
                            return null;
                        }
                        progress = this._server.Progress;
                    }
                    catch (AppDomainUnloadedException)
                    {
                        progress = null;
                    }
                }
                return progress;
            }
        }
    }
}

