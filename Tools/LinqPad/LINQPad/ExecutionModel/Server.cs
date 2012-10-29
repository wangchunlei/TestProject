namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.ObjectGraph.Formatters;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    internal class Server : MarshalByRefObject, IDisposable
    {
        private string[] _additionalRefs;
        private string _assemblyPath;
        private static volatile IDbConnection _cachedCx;
        private static object _cachedCxLock = new object();
        private static LINQPad.Repository _cachedRepos;
        private int _cacheVersionAtStart;
        private object _cancelLocker = new object();
        private volatile bool _cancelRequest;
        private volatile Client _client;
        private bool _compilationHadWarnings;
        private bool _completionCleanupComplete;
        private PluginForm _currentPluginForm;
        private static volatile Server _currentServer;
        private LINQPad.Extensibility.DataContext.DataContextDriver _dataContextDriver;
        private volatile bool _disposed;
        private static UnhandledExceptionEventHandler _domainExceptionHandler;
        private Stopwatch _executionStopwatch = new Stopwatch();
        private int _executionTrackDeadlocks;
        private AutoResetEvent _executionTrackReady;
        private AutoResetEvent _executionTrackSignal;
        private List<object> _explorables;
        private bool _faulted;
        private Action _finalCleanup;
        private bool _finished;
        private bool _finishing;
        private int _lastLambdaLength;
        private DateTime _lastMessageLoopIdle;
        private int _lastResultsLength;
        private int _lastSpecialMessageLine = -1;
        private int _lastSqlLogLength;
        private int _lineOffset;
        private static bool _linqPadDbProvidersInstalled;
        private static Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
        private List<Action> _messageLoopActions = new List<Action>();
        private bool _messageLoopFailed;
        private bool _queryStarted;
        private EventWaitHandle _readLineComplete = new AutoResetEvent(false);
        private volatile string _readLineData;
        private object _readLineLocker = new object();
        private LINQPad.Repository _repository;
        private object _resultsLocker = new object();
        private volatile IDbCommand _runningCmd;
        private string _sqlQuery;
        private object _statusLocker = new object();
        private bool _suppressErrors;
        private volatile Thread _uiThread;
        private volatile Thread _worker;
        public List<IDisposable> Disposables;
        public XhtmlWriter LambdaFormatter;
        public LINQPad.UI.PluginWindowManager PluginWindowManager;
        public Countdown QueryCompletionCountdown;
        public XhtmlWriter ResultsWriter;

        static Server()
        {
            System.AppDomain.CurrentDomain.DomainUnload += delegate (object sender, EventArgs e) {
                IDbConnection connection = _cachedCx;
                if (connection != null)
                {
                    try
                    {
                        connection.Dispose();
                    }
                    catch
                    {
                    }
                }
            };
            Program.AddLINQPadAssemblyResolver();
            System.AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args) {
                Server server = _currentServer;
                if (server == null)
                {
                    return null;
                }
                return server.CurrentDomain_AssemblyResolve(sender, args);
            };
            try
            {
                Application.EnableVisualStyles();
            }
            catch
            {
            }
        }

        public void AddFinalCleanup(Action cleanup)
        {
            this._finalCleanup = (Action) Delegate.Combine(this._finalCleanup, cleanup);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            this._lastMessageLoopIdle = DateTime.UtcNow;
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (!(e.Exception is ThreadAbortException))
            {
                QueryStatusEventArgs args = this.GetStatusArgs(e.Exception, false, true);
                this.PostStatus(args);
            }
        }

        public void CallCustomClick(int id)
        {
            object obj;
            if ((this._explorables != null) && (this._explorables.Count > id))
            {
                obj = this._explorables[id];
                if (obj != null)
                {
                    this.RunOnMessageLoopThread(delegate {
                        obj.Explore<object>(null);
                        this._client.OnCustomClickComplete();
                    });
                }
            }
        }

        public void Cancel(bool onlyIfRunning, bool killAppDomain)
        {
            if (this._executionTrackSignal != null)
            {
                try
                {
                    this._executionTrackSignal.Set();
                }
                catch
                {
                }
            }
            Thread.MemoryBarrier();
            if (!(!this._finishing || this._finished))
            {
                Thread.Sleep(200);
            }
            lock (this._statusLocker)
            {
                if (this._finished && onlyIfRunning)
                {
                    return;
                }
                this._finishing = true;
                this._finished = true;
                this._cancelRequest = true;
                this._client = null;
                if (!(((this._worker == null) || !this._worker.IsAlive) ? ((this._uiThread == null) || !this._uiThread.IsAlive) : false))
                {
                    this.WorkerThreadAborted = true;
                }
                if (this._executionTrackSignal != null)
                {
                    try
                    {
                        this._executionTrackSignal.Set();
                    }
                    catch
                    {
                    }
                }
            }
            Thread thread = new Thread(delegate {
                try
                {
                    Monitor.TryEnter(this._cancelLocker, 0x1388);
                    this.End(killAppDomain);
                }
                catch
                {
                }
                finally
                {
                    try
                    {
                        Monitor.Exit(this._cancelLocker);
                    }
                    catch
                    {
                    }
                }
            }) {
                IsBackground = true,
                Name = "Server Cancel"
            };
            thread.Start();
        }

        public static void Create()
        {
            System.AppDomain.CurrentDomain.SetData("instance", new Server());
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName name = new AssemblyName(args.Name);
            return this.ShadowLoad(name.Name, name.Version, null, args.RequestingAssembly);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exceptionObject = e.ExceptionObject as Exception;
            if (exceptionObject != null)
            {
                exceptionObject.Data["LINQPadHandled"] = null;
                if ((((this._assemblyPath != null) && !this.MessageLoopEnded) && !this._disposed) && !this._suppressErrors)
                {
                    QueryStatusEventArgs args = this.GetStatusArgs(exceptionObject, false, false);
                    if (Thread.CurrentThread != this._worker)
                    {
                        args.ErrorMessage = args.ErrorMessage + (!string.IsNullOrEmpty(Thread.CurrentThread.Name) ? (" [Thread = " + Thread.CurrentThread.Name + "]") : (Thread.CurrentThread.IsThreadPoolThread ? " [Source = Pooled Thread]" : " [Source = Worker Thread]"));
                    }
                    if (!string.IsNullOrEmpty(args.ErrorFileName))
                    {
                        try
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this._assemblyPath);
                            string b = Path.GetFileNameWithoutExtension(args.ErrorFileName);
                            if (!string.Equals(fileNameWithoutExtension, b, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return;
                            }
                        }
                        catch
                        {
                        }
                    }
                    try
                    {
                        this.GetStatusArgs(exceptionObject, false, true);
                        this.PostStatus(args);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void Dispose()
        {
            this._disposed = true;
            this.EndMessageLoop();
            this._client = null;
            this.PluginWindowManager = null;
            if (this._executionTrackSignal != null)
            {
                try
                {
                    this._executionTrackSignal.Set();
                }
                catch
                {
                }
            }
            try
            {
                RemotingServices.Disconnect(this);
            }
            catch
            {
            }
        }

        private void End(bool killAppDomain)
        {
            ThreadStart start = null;
            ThreadStart start2 = null;
            if (this.InMessageLoop && (this._currentPluginForm == null))
            {
                this.EndMessageLoop();
            }
            if (this.QueryCompletionCountdown.Value > 0)
            {
                this.QueryCompletionCountdown.Reset();
                if ((this._worker != null) && this._worker.IsAlive)
                {
                    this._worker.Join(200);
                }
            }
            IDbCommand cmd = this._runningCmd;
            if (cmd == null)
            {
                cmd = LINQPadDbConnection.LastCommand;
                if ((cmd != null) && ((cmd.Connection == null) || (cmd.Connection.State == ConnectionState.Closed)))
                {
                    cmd = null;
                }
            }
            Thread thread = null;
            if (cmd != null)
            {
                if (start == null)
                {
                    start = delegate {
                        try
                        {
                            cmd.Cancel();
                            Thread.Sleep(200);
                        }
                        catch
                        {
                        }
                    };
                }
                thread = new Thread(start) {
                    IsBackground = true,
                    Name = "DbCommand Cancel"
                };
                thread.Start();
            }
            Thread thread3 = null;
            Process p = Util.CurrentExternalProcess;
            if ((p != null) && !p.HasExited)
            {
                if (start2 == null)
                {
                    start2 = delegate {
                        try
                        {
                            p.Kill();
                            Util.CurrentExternalProcess = null;
                        }
                        catch
                        {
                        }
                    };
                }
                thread3 = new Thread(start2) {
                    IsBackground = true,
                    Name = "External process terminator"
                };
                thread3.Start();
            }
            bool killWorker = ((this._worker == null) || !this._worker.IsAlive) ? ((this._uiThread != null) && this._uiThread.IsAlive) : true;
            if (((!killAppDomain && this.InMessageLoop) && (this._currentPluginForm != null)) && ((DateTime.UtcNow - this._lastMessageLoopIdle).TotalMilliseconds < 500.0))
            {
                killWorker = false;
            }
            if (!(killWorker || killAppDomain))
            {
                this.WorkerThreadAborted = false;
            }
            Thread thread6 = new Thread(delegate {
                this.PerformCompletionCleanup(killWorker || killAppDomain, true);
                if (killAppDomain)
                {
                    try
                    {
                        PluginForm form = this._currentPluginForm;
                        if ((form != null) && this.InMessageLoop)
                        {
                            form.InvokeCloseWithShutdown();
                            if ((this._worker != null) && this._worker.IsAlive)
                            {
                                this._worker.Join(500);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                this.PerformFinalCleanup();
            }) {
                IsBackground = true,
                Name = "Query Cancellation Cleanup"
            };
            thread6.Start();
            thread6.Join(700);
            if (!(!killAppDomain && this.InMessageLoop) && IsWpfLoaded())
            {
                ShutdownWPF(false);
            }
            if (thread != null)
            {
                thread.Join(0x5dc);
            }
            if (thread3 != null)
            {
                thread3.Join(500);
            }
            if (((this._worker != null) && (killWorker || ((this._worker.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted))) && this.KillThread(this._worker))
            {
                this._worker = null;
            }
            if (((this._uiThread != null) && killWorker) && ((this._uiThread == this._worker) || this.KillThread(this._uiThread)))
            {
                this._uiThread = null;
            }
            if (killAppDomain && !System.AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                if (IsWpfLoaded())
                {
                    ShutdownWPF(true);
                }
                Thread.Sleep(100);
                try
                {
                    Util.UnloadAppDomain(System.AppDomain.CurrentDomain, 20);
                }
                catch
                {
                }
            }
        }

        public void EndMessageLoop()
        {
            if (this.InMessageLoop)
            {
                try
                {
                    if (this._currentPluginForm != null)
                    {
                        this._currentPluginForm.InvokeCloseWithShutdown();
                        if (this.MessageLoopStartedWithoutForm)
                        {
                            Application.Exit();
                        }
                    }
                    else
                    {
                        if (IsWpfLoaded())
                        {
                            WpfBridge.ShutdownCurrentDispatcher();
                        }
                        Application.Exit();
                    }
                    this._currentPluginForm = null;
                }
                catch
                {
                }
            }
        }

        internal static void EnsureStaticConstructorRuns()
        {
        }

        public void ExecuteClrQuery(Client client, LINQPad.Repository r, string assemblyPath, int lineOffset, string[] additionalRefs, bool mta, bool compilationHadWarnings, string queryPath, string queryName, LINQPad.UI.PluginWindowManager pluginWinManager, bool executionTrackingEnabled)
        {
            this.IsClrQuery = true;
            this._client = client;
            _currentServer = null;
            this._repository = r;
            this._assemblyPath = assemblyPath;
            this._lineOffset = lineOffset;
            this._additionalRefs = additionalRefs;
            this._compilationHadWarnings = compilationHadWarnings;
            this._lastLambdaLength = 0;
            this._lastSqlLogLength = 0;
            this._lastResultsLength = 0;
            this._cacheVersionAtStart = UserCache.CacheVersion;
            this.PluginWindowManager = pluginWinManager;
            CurrentQueryAdditionalRefs = this._additionalRefs;
            Util.Progress = null;
            Util.CurrentQueryPath = (queryPath == "") ? null : queryPath;
            Util.CurrentQueryName = queryName;
            PluginServer.CustomCount = 1;
            UserCache.ClearSession();
            Thread thread = new Thread(() => this.StartQuery(new ClrQueryRunner(this, executionTrackingEnabled))) {
                IsBackground = true,
                Name = System.AppDomain.CurrentDomain.FriendlyName
            };
            this._worker = thread;
            if (!mta)
            {
                this._worker.SetApartmentState(ApartmentState.STA);
            }
            this._worker.Start();
        }

        public void ExecuteSqlQuery(QueryLanguage language, LINQPad.Repository repository, string query, string[] refs, Client client, LINQPad.UI.PluginWindowManager pluginWinManager)
        {
            QueryRunner runner;
            this.IsClrQuery = false;
            Util.Progress = null;
            this._repository = repository;
            this._sqlQuery = query;
            this._client = client;
            this._additionalRefs = refs;
            CurrentQueryAdditionalRefs = this._additionalRefs;
            this.PluginWindowManager = pluginWinManager;
            if (language == QueryLanguage.ESQL)
            {
                runner = new ESqlQueryRunner(this, query);
            }
            else
            {
                runner = new SqlQueryRunner(this, query);
            }
            Thread thread = new Thread(() => this.StartQuery(runner)) {
                Name = ((language == QueryLanguage.ESQL) ? "E" : "") + "SQL Query Runner",
                IsBackground = true
            };
            this._worker = thread;
            this._worker.SetApartmentState(ApartmentState.STA);
            this._worker.Start();
        }

        private QueryStatusEventArgs GetExceptionArgs(Exception ex)
        {
            this._faulted = true;
            QueryStatusEventArgs completionArgs = new QueryStatusEventArgs();
            try
            {
                completionArgs.ErrorMessage = ex.GetType().Name + ": " + ex.Message;
                if (this._queryStarted)
                {
                    completionArgs.StatusMessage = "Error running query";
                    if (!(this.PopulateErrorSource(ex, completionArgs) || !(ex is AggregateException)))
                    {
                        this.PopulateErrorSource(ex.InnerException, completionArgs);
                    }
                    return completionArgs;
                }
                completionArgs.StatusMessage = "Error starting query";
            }
            catch (Exception exception)
            {
                try
                {
                    Log.Write(exception);
                    completionArgs.StatusMessage = "Error running query";
                }
                catch
                {
                }
            }
            return completionArgs;
        }

        public ExecutionTrackInfo GetMainThreadPosition(bool interactive)
        {
            Func<StackFrame, RowColumn> selector = null;
            if (!((this._executionTrackDeadlocks <= 1) || interactive))
            {
                return null;
            }
            if (this.InMessageLoop)
            {
                return null;
            }
            if (this._executionTrackSignal == null)
            {
                this._executionTrackSignal = new AutoResetEvent(false);
                this._executionTrackReady = new AutoResetEvent(false);
                new Thread(new ThreadStart(this.StartExecutionTrackingBackstop)) { IsBackground = true, Priority = ThreadPriority.AboveNormal, Name = "Execution Tracking Backstop" }.Start();
            }
            ExecutionTrackInfo info2 = new ExecutionTrackInfo();
            StackTrace trace = null;
            if (!this._executionTrackReady.WaitOne(interactive ? 300 : 10))
            {
                return null;
            }
            this._executionTrackSignal.Set();
            Stopwatch stopwatch = Stopwatch.StartNew();
            int num = this._executionTrackDeadlocks;
            try
            {
                this._worker.Suspend();
                trace = new StackTrace(this._worker, true);
            }
            catch
            {
            }
            finally
            {
                try
                {
                    this._worker.Resume();
                }
                catch
                {
                    trace = null;
                }
            }
            stopwatch.Stop();
            info2.Cost = (int) stopwatch.ElapsedMilliseconds;
            if (this._executionTrackDeadlocks > num)
            {
                Log.Write("Execution tracking deadlock");
            }
            if ((num == 0) && (this._executionTrackDeadlocks > 0))
            {
                info2.Cost = 100;
            }
            if (trace != null)
            {
                StackFrame[] queryFrames = GetQueryFrames(trace);
                if (queryFrames.Length != 0)
                {
                    if (selector == null)
                    {
                        selector = f => new RowColumn { Row = f.GetFileLineNumber() - this._lineOffset, Column = f.GetFileColumnNumber() };
                    }
                    info2.MainThreadStack = queryFrames.Select<StackFrame, RowColumn>(selector).ToArray<RowColumn>();
                }
            }
            return info2;
        }

        private static StackFrame[] GetQueryFrames(StackTrace trace)
        {
            if (trace == null)
            {
                return new StackFrame[0];
            }
            List<StackFrame> list = new List<StackFrame>();
            try
            {
                foreach (StackFrame frame in trace.GetFrames())
                {
                    MethodBase method = frame.GetMethod();
                    if ((((method != null) && (method.DeclaringType != null)) && (method.DeclaringType.Assembly != null)) && method.DeclaringType.Assembly.FullName.StartsWith("query_"))
                    {
                        list.Add(frame);
                    }
                }
            }
            catch
            {
            }
            return list.ToArray();
        }

        public ResultData GetResults(bool onlyIfChanged)
        {
            lock (this._resultsLocker)
            {
                if (!(!onlyIfChanged || this.HaveResultsChanged()))
                {
                    return null;
                }
                ResultData data3 = new ResultData {
                    AutoScrollResults = Util.AutoScrollResults
                };
                try
                {
                    data3.SQL = LINQPadDbController.SqlLog.ToString();
                    this._lastSqlLogLength = data3.SQL.Length;
                }
                catch
                {
                }
                try
                {
                    data3.Lambda = this.LambdaFormatter.ToString(out this._lastLambdaLength);
                }
                catch
                {
                }
                try
                {
                    data3.Output = this.ResultsWriter.ToString(out this._lastResultsLength);
                }
                catch
                {
                }
                data3.MessageLoopFailed = this._messageLoopFailed;
                this._messageLoopFailed = false;
                return data3;
            }
        }

        private QueryStatusEventArgs GetStatusArgs(Exception ex, bool completion, bool dumpExceptions)
        {
            QueryStatusEventArgs exceptionArgs;
            if (ex != null)
            {
                ex = this.StripFrivolousWrappings(ex);
                exceptionArgs = this.GetExceptionArgs(ex);
                if (dumpExceptions)
                {
                    try
                    {
                        Console.WriteLine(ex);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                exceptionArgs = new QueryStatusEventArgs();
                if (!(!completion || this._faulted))
                {
                    exceptionArgs.StatusMessage = this._compilationHadWarnings ? "Query completed successfully with warnings" : "Query successful";
                }
            }
            exceptionArgs.ExecutionComplete = completion;
            if (this._queryStarted && completion)
            {
                exceptionArgs.ExecTime = this._executionStopwatch.Elapsed - this.ResultsWriter.FormattingTime;
                if (exceptionArgs.ExecTime < TimeSpan.Zero)
                {
                    exceptionArgs.ExecTime = TimeSpan.Zero;
                }
            }
            if (IsWpfLoaded() && HasWpfMsgLoopRun())
            {
                exceptionArgs.AppDomainRecycleSuggested = true;
            }
            return exceptionArgs;
        }

        private Exception GetTaskException(Task task)
        {
            Exception exception = task.Exception;
            if (exception != null)
            {
                return exception.InnerException;
            }
            if (!task.IsCanceled)
            {
                return null;
            }
            exception = new TaskCanceledException(task);
            try
            {
                MethodInfo info = task.GetType().GetMethod("GetCancellationExceptionDispatchInfo", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
                if (info == null)
                {
                    return exception;
                }
                object obj2 = info.Invoke(task, null);
                if (obj2 == null)
                {
                    return exception;
                }
                MethodInfo info2 = obj2.GetType().GetMethod("Throw", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
                if (info2 == null)
                {
                    return exception;
                }
                try
                {
                    info2.Invoke(obj2, null);
                }
                catch (Exception exception3)
                {
                    return exception3;
                }
            }
            catch
            {
            }
            return exception;
        }

        private static bool HasWpfMsgLoopRun()
        {
            try
            {
                return WpfBridge.HasWpfMsgLoopRun();
            }
            catch
            {
                return false;
            }
        }

        public bool HaveResultsChanged()
        {
            bool flag;
            if (((LINQPadDbController.SqlLog == null) || (this.ResultsWriter == null)) || (this.LambdaFormatter == null))
            {
                return false;
            }
            lock (this._resultsLocker)
            {
                try
                {
                    if (LINQPadDbController.SqlLog.GetStringBuilder().Length != this._lastSqlLogLength)
                    {
                        return true;
                    }
                    if (this.LambdaFormatter.GetLength() != this._lastLambdaLength)
                    {
                        return true;
                    }
                    if (this.ResultsWriter.GetLength() != this._lastResultsLength)
                    {
                        return true;
                    }
                    flag = false;
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void InstallDbProviders(LINQPad.Repository r)
        {
            if (!_linqPadDbProvidersInstalled)
            {
                LINQPadDbController.InstallCustomProviders();
                _linqPadDbProvidersInstalled = true;
                if ((r != null) && (r.DriverLoader.InternalID == "EntityFrameworkDbContext"))
                {
                    try
                    {
                        EntityFrameworkDbContextDriver.PatchCxFactory(r);
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal static bool IsWpfLoaded()
        {
            try
            {
                return System.AppDomain.CurrentDomain.GetAssemblies().Any<Assembly>(a => a.FullName.StartsWith("WindowsBase,", StringComparison.InvariantCultureIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private bool KillThread(Thread thread)
        {
            if (thread != null)
            {
                try
                {
                    if (((thread.ThreadState & ThreadState.Suspended) > ThreadState.Running) || ((thread.ThreadState & ThreadState.SuspendRequested) > ThreadState.Running))
                    {
                        try
                        {
                            thread.Resume();
                        }
                        catch
                        {
                        }
                    }
                    thread.Abort();
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        public void NotifyPluginFormCreated(PluginForm form)
        {
            lock (this._statusLocker)
            {
                if ((this._currentPluginForm == null) && this.InMessageLoop)
                {
                    this._currentPluginForm = form;
                }
            }
        }

        public static IDbConnection OpenCachedCx(LINQPad.Repository r)
        {
            lock (_cachedCxLock)
            {
                if (_cachedCx != null)
                {
                    bool flag2 = (_cachedRepos == null) || !_cachedRepos.IsEquivalent(r);
                    if (!((_cachedCx.State != ConnectionState.Open) || flag2))
                    {
                        return _cachedCx;
                    }
                    if ((_cachedCx.State != ConnectionState.Closed) || flag2)
                    {
                        try
                        {
                            _cachedCx.Dispose();
                        }
                        catch
                        {
                        }
                        _cachedCx = null;
                    }
                }
                if (_cachedCx == null)
                {
                    _cachedCx = r.Open(true);
                    _cachedRepos = r;
                }
                else
                {
                    _cachedCx.Open();
                }
                return _cachedCx;
            }
        }

        private void PerformCompletionCleanup(bool formClosing, bool cancelled)
        {
            try
            {
                List<IDisposable> list;
                int count = 0;
                lock ((list = this.Disposables))
                {
                    if (this._completionCleanupComplete)
                    {
                        return;
                    }
                    this._completionCleanupComplete = true;
                    count = this.Disposables.Count;
                }
                if (count > 0)
                {
                    IDisposable[] disposableArray;
                    lock ((list = this.Disposables))
                    {
                        disposableArray = this.Disposables.ToArray();
                        this.Disposables.Clear();
                    }
                    foreach (IDisposable disposable in disposableArray)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch
                        {
                        }
                    }
                }
                if ((this.InMessageLoop && (this._currentPluginForm != null)) && (this.PluginWindowManager != null))
                {
                    try
                    {
                        Action action = null;
                        foreach (PluginControl pic in this.PluginWindowManager.GetControls())
                        {
                            try
                            {
                                if (action == null)
                                {
                                    action = () => pic.OutputPanel.OnQueryEnded(formClosing, cancelled);
                                }
                                Action method = action;
                                if (pic.Control.IsHandleCreated)
                                {
                                    pic.Control.BeginInvoke(method).AsyncWaitHandle.WaitOne(500, false);
                                }
                                else if (!formClosing)
                                {
                                    this.RunOnMessageLoopThread(method);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        private void PerformFinalCleanup()
        {
            try
            {
                Action action = null;
                lock (this._statusLocker)
                {
                    action = this._finalCleanup;
                    this._finalCleanup = null;
                }
                if (action != null)
                {
                    try
                    {
                        action();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        private bool PopulateErrorSource(Exception ex, QueryStatusEventArgs completionArgs)
        {
            Func<StackFrame, int> selector = null;
            StackTrace trace = new StackTrace(ex, true);
            StackFrame[] queryFrames = GetQueryFrames(trace);
            if (queryFrames.Length > 0)
            {
                completionArgs.ErrorLine = queryFrames[0].GetFileLineNumber() - this._lineOffset;
                completionArgs.ErrorColumn = queryFrames[0].GetFileColumnNumber();
                completionArgs.ErrorFileName = queryFrames[0].GetFileName();
                if (queryFrames.Length > 1)
                {
                    if (selector == null)
                    {
                        selector = f => f.GetFileLineNumber() - this._lineOffset;
                    }
                    completionArgs.StackTraceLines = queryFrames.Skip<StackFrame>(1).Select<StackFrame, int>(selector).ToArray<int>();
                    completionArgs.StackTraceColumns = (from f in queryFrames.Skip<StackFrame>(1) select f.GetFileColumnNumber()).ToArray<int>();
                }
                return true;
            }
            if (ex.StackTrace != null)
            {
                int num;
                Match match = Regex.Match(ex.StackTrace.Replace("\r\n", ""), @"Server stack trace:.*?at UserQuery\..+? in (.+?\.cs):line (\d+)");
                if ((match.Success && (match.Groups.Count == 3)) && int.TryParse(match.Groups[2].Value, out num))
                {
                    completionArgs.ErrorLine = num - this._lineOffset;
                    completionArgs.ErrorFileName = match.Groups[1].Value;
                    return true;
                }
            }
            return false;
        }

        public void PostError(Exception ex)
        {
            this.PostStatus(this.GetStatusArgs(ex, false, true));
        }

        public void PostStatus(QueryStatusEventArgs args)
        {
            Client client;
            lock (this._statusLocker)
            {
                client = this._client;
                if ((this._cancelRequest || (client == null)) || (this._finished && string.IsNullOrEmpty(args.ErrorMessage)))
                {
                    return;
                }
                if (args.ExecutionComplete)
                {
                    this._executionStopwatch.Stop();
                    this._finished = true;
                    if (this._executionTrackSignal != null)
                    {
                        try
                        {
                            this._executionTrackSignal.Set();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            client.OnQueryStatusChanged(args);
        }

        private PluginForm PrepareMessageLoop()
        {
            Action[] actionArray;
            this._uiThread = Thread.CurrentThread;
            if ((SynchronizationContext.Current == null) || (SynchronizationContext.Current.GetType() == typeof(SynchronizationContext)))
            {
                Util.CreateSynchronizationContext(false, false);
            }
            this.SyncContext = SynchronizationContext.Current;
            lock (this._messageLoopActions)
            {
                actionArray = this._messageLoopActions.ToArray();
            }
            foreach (Action action in actionArray)
            {
                action();
            }
            PluginForm form = (this.PluginWindowManager == null) ? null : this.PluginWindowManager.Form;
            lock (this._statusLocker)
            {
                this._currentPluginForm = form;
                this.MessageLoopStartedWithoutForm = form == null;
                this.InMessageLoop = true;
            }
            return form;
        }

        public string ReadLine()
        {
            return this.ReadLine(null, null, null);
        }

        public string ReadLine(string prompt, string defaultValue, IEnumerable<string> options)
        {
            lock (this._readLineLocker)
            {
                string[] strArray = null;
                if (options != null)
                {
                    strArray = options.Take<string>(0x2710).ToArray<string>();
                }
                Client client = this._client;
                if (client == null)
                {
                    return null;
                }
                client.OnReadLineRequested(prompt, defaultValue, strArray);
                this._readLineComplete.WaitOne();
                return this._readLineData;
            }
        }

        public void ReadLineCompleted(string text)
        {
            this._readLineData = text;
            this._readLineComplete.Set();
        }

        public void RegisterQueryStart()
        {
            this._executionStopwatch.Start();
            lock (this._statusLocker)
            {
                this._queryStarted = true;
            }
        }

        public void ReportSpecialMessage(StackTrace trace, string status, string message, bool isWarning, bool isInfo)
        {
            QueryStatusEventArgs args = new QueryStatusEventArgs(status, message) {
                ExecutionComplete = false
            };
            StackFrame[] queryFrames = GetQueryFrames(trace);
            if (queryFrames.Length > 0)
            {
                StackFrame frame = queryFrames[0];
                args.ErrorLine = frame.GetFileLineNumber() - this._lineOffset;
                args.ErrorColumn = frame.GetFileColumnNumber();
                args.ErrorFileName = frame.GetFileName();
                args.IsInfo = isInfo;
                args.IsWarning = isWarning;
                if (args.ErrorLine == this._lastSpecialMessageLine)
                {
                    return;
                }
                this._lastSpecialMessageLine = args.ErrorLine;
            }
            if (this._client != null)
            {
                this._client.OnQueryStatusChanged(args);
            }
        }

        private void RunMessageLoop(PluginForm pluginForm, bool queryCompleteOnReady)
        {
            if (pluginForm != null)
            {
                ThreadPool.QueueUserWorkItem(delegate (object _) {
                    try
                    {
                        if (this._currentPluginForm.Ready.WaitOne(0x4e20, false))
                        {
                            if (this._client != null)
                            {
                                this._client.OnPluginsReady();
                            }
                            if (queryCompleteOnReady)
                            {
                                this.QueryCompletionCountdown.Decrement();
                            }
                        }
                    }
                    catch
                    {
                    }
                });
            }
            Application.ThreadException += new ThreadExceptionEventHandler(this.Application_ThreadException);
            Application.Idle += new EventHandler(this.Application_Idle);
            try
            {
                if (this.MessageLoopStartedWithoutForm)
                {
                    Application.Run();
                }
                else
                {
                    Application.Run(pluginForm);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                this.PostStatus(this.GetStatusArgs(exception, true, true));
                this._messageLoopFailed = true;
            }
            finally
            {
                this.InMessageLoop = false;
                this.MessageLoopEnded = true;
                this._currentPluginForm = null;
                Application.ThreadException -= new ThreadExceptionEventHandler(this.Application_ThreadException);
                Application.Idle -= new EventHandler(this.Application_Idle);
                if (!this._cancelRequest)
                {
                    this.PerformFinalCleanup();
                }
            }
        }

        public void RunOnMessageLoopThread(Action action)
        {
            SendOrPostCallback d = null;
            lock (this._statusLocker)
            {
                if (this._cancelRequest)
                {
                    return;
                }
            }
            lock (this._messageLoopActions)
            {
                if (this.SyncContext == null)
                {
                    this._messageLoopActions.Add(action);
                }
                else
                {
                    if (d == null)
                    {
                        d = _ => action();
                    }
                    this.SyncContext.Post(d, null);
                }
            }
        }

        private void RunQuery(QueryRunner runner)
        {
            Action<Task> continuationAction = null;
            Action a = null;
            object obj2;
            runner.Prepare();
            this.QueryCompletionCountdown = new Countdown();
            this.Disposables = new List<IDisposable>();
            this.LambdaFormatter = new XhtmlWriter(false, false);
            LINQPadDbController.SqlLog = new StringWriterEx(0xf4240);
            DataContextBase.SqlLog = LINQPadDbController.SqlLog;
            this.ResultsWriter = new XhtmlWriter(true, true);
            this._explorables = this.ResultsWriter.Explorables;
            Console.SetOut(this.ResultsWriter);
            Console.SetIn(new ConsoleTextReader(new Func<string>(this.ReadLine)));
            Debug.Listeners.Clear();
            Debug.Listeners.Add(new TextWriterTraceListener(this.ResultsWriter));
            XhtmlFormatter.ResetTableID();
            lock ((obj2 = this._statusLocker))
            {
                if (this._cancelRequest)
                {
                    return;
                }
            }
            object obj3 = null;
            try
            {
                obj3 = runner.Run();
            }
            catch (Exception exception)
            {
                this.PostStatus(this.GetStatusArgs(exception, false, true));
            }
            if (obj3 is Task)
            {
                this.QueryCompletionCountdown.Increment();
                QueryStatusEventArgs args = new QueryStatusEventArgs("Query continuing asynchronously...") {
                    ExecutionComplete = false,
                    Async = true
                };
                this.PostStatus(args);
                if (continuationAction == null)
                {
                    continuationAction = delegate (Task ant) {
                        try
                        {
                            Exception ex = this.GetTaskException(ant);
                            if (ex != null)
                            {
                                this._faulted = true;
                                this.PostStatus(this.GetStatusArgs(ex, true, true));
                            }
                        }
                        catch
                        {
                        }
                        this.QueryCompletionCountdown.Decrement();
                    };
                }
                ((Task) obj3).ContinueWith(continuationAction);
            }
            if (this.QueryCompletionCountdown.Value <= 0)
            {
                this._executionStopwatch.Stop();
            }
            lock ((obj2 = this._statusLocker))
            {
                if (this._cancelRequest)
                {
                    return;
                }
            }
            PluginForm pluginForm = null;
            pluginForm = this.PrepareMessageLoop();
            if (pluginForm != null)
            {
                this.QueryCompletionCountdown.Increment();
            }
            if (this.QueryCompletionCountdown.Value <= 0)
            {
                this.PostStatus(this.GetStatusArgs(null, true, true));
                this.PerformCompletionCleanup(false, false);
            }
            else
            {
                QueryStatusEventArgs args2 = new QueryStatusEventArgs("Query continuing asynchronously...") {
                    ExecutionComplete = false,
                    Async = true
                };
                this.PostStatus(args2);
                if (a == null)
                {
                    a = delegate {
                        this.PostStatus(this.GetStatusArgs(null, true, true));
                        this.PerformCompletionCleanup(false, false);
                    };
                }
                this.QueryCompletionCountdown.ContinueWith(a);
            }
            this.RunMessageLoop(pluginForm, true);
        }

        public Assembly ShadowLoad(string simpleName, Version versionIfKnown, string filePathIfKnown, Assembly requestingAssembly)
        {
            simpleName = simpleName.ToLowerInvariant();
            Assembly assembly = null;
            if (!_loadedAssemblies.TryGetValue(simpleName, out assembly))
            {
                Assembly assembly3 = (from a in System.AppDomain.CurrentDomain.GetAssemblies()
                    let name = a.GetName()
                    where simpleName.Equals(name.Name, StringComparison.OrdinalIgnoreCase) && ((versionIfKnown == null) || versionIfKnown.Equals(name.Version))
                    select a).FirstOrDefault<Assembly>();
                if (assembly3 != null)
                {
                    return assembly3;
                }
                string path = null;
                bool flag = false;
                string data = System.AppDomain.CurrentDomain.GetData("LINQPad.ShadowPath") as string;
                if (!(string.IsNullOrEmpty(data) || !Directory.Exists(data)))
                {
                    path = PathHelper.GetAssemblyFromFolderIfPresent(data, simpleName, filePathIfKnown);
                }
                if (path == null)
                {
                    path = PathHelper.GetAssemblyFromFolderIfPresent(System.AppDomain.CurrentDomain.BaseDirectory, simpleName, filePathIfKnown);
                }
                if (path == null)
                {
                    string str3 = System.AppDomain.CurrentDomain.GetData("LINQPad.StaticDCFolder") as string;
                    if (!(string.IsNullOrEmpty(str3) || !Directory.Exists(str3)))
                    {
                        path = PathHelper.GetAssemblyFromFolderIfPresent(str3, simpleName, filePathIfKnown);
                        if (path != null)
                        {
                            flag = true;
                        }
                    }
                }
                if (path == null)
                {
                    string str4 = System.AppDomain.CurrentDomain.GetData("LINQPad.DCDriverFolder") as string;
                    if (!(string.IsNullOrEmpty(str4) || !Directory.Exists(str4)))
                    {
                        path = PathHelper.GetAssemblyFromFolderIfPresent(str4, simpleName, filePathIfKnown);
                        if (path != null)
                        {
                            flag = true;
                        }
                    }
                }
                if (((path == null) && (filePathIfKnown != null)) && File.Exists(filePathIfKnown))
                {
                    path = filePathIfKnown;
                    flag = true;
                }
                if (!(((path != null) || (requestingAssembly == null)) || string.IsNullOrEmpty(requestingAssembly.Location)))
                {
                    path = PathHelper.GetAssemblyFromFolderIfPresent(Path.GetDirectoryName(requestingAssembly.Location), simpleName, filePathIfKnown);
                    if (path != null)
                    {
                        flag = true;
                    }
                }
                if (this._additionalRefs == null)
                {
                    this._additionalRefs = new string[0];
                }
                if (path == null)
                {
                    path = this._additionalRefs.FirstOrDefault<string>(r => Path.GetFileNameWithoutExtension(r).ToLowerInvariant() == simpleName);
                }
                if ((path == null) && (filePathIfKnown != null))
                {
                    path = filePathIfKnown;
                }
                if (((path == null) && (CurrentQueryAssemblyPath != null)) && CurrentQueryAssemblyPath.ToLowerInvariant().Contains(simpleName.ToLowerInvariant()))
                {
                    path = CurrentQueryAssemblyPath;
                }
                if (path == null)
                {
                    return null;
                }
                assembly = flag ? Assembly.LoadFile(path) : Assembly.LoadFrom(path);
                _loadedAssemblies[simpleName] = assembly;
            }
            return assembly;
        }

        private static void ShutdownWPF(bool waitUntilDone)
        {
            try
            {
                WpfBridge.ShutdownWPF(waitUntilDone);
            }
            catch
            {
            }
        }

        private void StartExecutionTrackingBackstop()
        {
            try
            {
            Label_0002:
                this._executionTrackReady.Set();
                this._executionTrackSignal.WaitOne();
                if (this._worker == null)
                {
                    return;
                }
                Thread.Sleep(200);
                if (this._worker == null)
                {
                    return;
                }
                ThreadState threadState = this._worker.ThreadState;
                if (((threadState & ThreadState.Suspended) > ThreadState.Running) || ((threadState & ThreadState.SuspendRequested) > ThreadState.Running))
                {
                    try
                    {
                        this._worker.Resume();
                    }
                    catch
                    {
                    }
                    goto Label_0095;
                }
            Label_007A:
                if (!this._finishing && !this._finished)
                {
                    goto Label_0002;
                }
                return;
            Label_0095:
                this._executionTrackDeadlocks++;
                goto Label_007A;
            }
            catch
            {
            }
        }

        private void StartQuery(QueryRunner runner)
        {
            object obj2;
            lock ((obj2 = this._statusLocker))
            {
                if (this._cancelRequest)
                {
                    return;
                }
            }
            _currentServer = this;
            this.ServerThread = Thread.CurrentThread;
            if ((this._repository != null) && this._repository.DriverLoader.IsValid)
            {
                this._dataContextDriver = this._repository.DriverLoader.Driver;
            }
            if ((this._dataContextDriver is EntityFrameworkDriver) || (this._dataContextDriver is EntityFrameworkDbContextDriver))
            {
                this.InstallDbProviders(this._repository);
            }
            lock ((obj2 = this._statusLocker))
            {
                if (this._cancelRequest)
                {
                    return;
                }
            }
            if (_domainExceptionHandler != null)
            {
                System.AppDomain.CurrentDomain.UnhandledException -= _domainExceptionHandler;
            }
            System.AppDomain.CurrentDomain.UnhandledException += (_domainExceptionHandler = new UnhandledExceptionEventHandler(this.CurrentDomain_UnhandledException));
            try
            {
                this.RunQuery(runner);
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                this.MessageLoopEnded = true;
                this.PostStatus(this.GetStatusArgs(exception, true, true));
                this.PerformCompletionCleanup(true, false);
                this.PerformFinalCleanup();
            }
        }

        private Exception StripFrivolousWrappings(Exception ex)
        {
            if (ex.InnerException != null)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }
                if (ex.InnerException == null)
                {
                    return ex;
                }
                if ((this._dataContextDriver is EntityFrameworkDbContextDriver) && (ex.GetType().Name == "ProviderIncompatibleException"))
                {
                    ex = ex.InnerException;
                }
            }
            return ex;
        }

        public void SuppressErrors()
        {
            this._suppressErrors = true;
        }

        public System.AppDomain AppDomain
        {
            get
            {
                return System.AppDomain.CurrentDomain;
            }
        }

        public bool CancelRequest
        {
            get
            {
                return this._cancelRequest;
            }
        }

        public static string[] CurrentQueryAdditionalRefs
        {
            [CompilerGenerated]
            get
            {
                return <CurrentQueryAdditionalRefs>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <CurrentQueryAdditionalRefs>k__BackingField = value;
            }
        }

        public static string CurrentQueryAssemblyPath
        {
            [CompilerGenerated]
            get
            {
                return <CurrentQueryAssemblyPath>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <CurrentQueryAssemblyPath>k__BackingField = value;
            }
        }

        public static Server CurrentServer
        {
            get
            {
                return _currentServer;
            }
        }

        public LINQPad.Extensibility.DataContext.DataContextDriver DataContextDriver
        {
            get
            {
                return this._dataContextDriver;
            }
        }

        public bool Finished
        {
            get
            {
                lock (this._statusLocker)
                {
                    return this._finished;
                }
            }
        }

        public bool HasUserCacheChanged
        {
            get
            {
                return (this._cacheVersionAtStart != UserCache.CacheVersion);
            }
        }

        public bool InMessageLoop { get; private set; }

        public bool IsClrQuery { get; private set; }

        public bool IsOnMainQueryThread
        {
            get
            {
                return (Thread.CurrentThread == this._worker);
            }
        }

        public bool IsUserCachePresent
        {
            get
            {
                return (UserCache.CacheVersion > 0);
            }
        }

        internal int LineOffset
        {
            get
            {
                return this._lineOffset;
            }
        }

        public bool MessageLoopEnded { get; private set; }

        public bool MessageLoopStartedWithoutForm { get; private set; }

        public int? Progress
        {
            get
            {
                return Util.Progress;
            }
        }

        public string QueryAssemblyPath
        {
            get
            {
                return this._assemblyPath;
            }
        }

        public LINQPad.Repository Repository
        {
            get
            {
                return this._repository;
            }
        }

        public IDbCommand RunningCmd
        {
            get
            {
                return this._runningCmd;
            }
            set
            {
                this._runningCmd = value;
            }
        }

        public Thread ServerThread { get; private set; }

        public SynchronizationContext SyncContext { get; private set; }

        public bool WorkerThreadAborted { get; private set; }

        public bool WriteResultsToGrids { get; set; }
    }
}

