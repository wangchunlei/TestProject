namespace LINQPad
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class DataContextManager : IDisposable
    {
        private static Timer _cleanupTmr = new Timer(delegate (object _) {
            try
            {
                Cleanup();
            }
            catch
            {
            }
        });
        private static DataContextCallback _globalErrorHandlers = delegate (DataContextInfo param0) {
        };
        private bool _isDisposed;
        private Runner _runner;
        private static Dictionary<LINQPad.Repository, Runner> _runners = new Dictionary<LINQPad.Repository, Runner>();
        private DataContextCallback _subscription;
        public readonly LINQPad.Repository Repository;
        public string Tag;

        static DataContextManager()
        {
            _cleanupTmr.Change(0xea60, 0xea60);
        }

        private DataContextManager(LINQPad.Repository r, DataContextCallback subscription)
        {
            this.Repository = r;
            this._subscription = subscription;
            lock (_runners)
            {
                if (_runners.TryGetValue(r, out this._runner))
                {
                    this._runner.RefCount++;
                }
                else
                {
                    _runners[this.Repository] = this._runner = new Runner(this.Repository);
                }
            }
            if (this._subscription != null)
            {
                this._runner.DataContextChanged += this._subscription;
            }
        }

        private static void Cleanup()
        {
            lock (_runners)
            {
                Runner[] runnerArray = (from r in _runners.Values
                    where r.RefCount <= 0
                    select r).ToArray<Runner>();
                int length = runnerArray.Length;
                TimeSpan span = TimeSpan.FromMinutes(2.0);
                foreach (Runner runner in from r in runnerArray
                    orderby r.LastUsed
                    select r)
                {
                    if (length-- < 10)
                    {
                        span = TimeSpan.FromHours(2.0);
                    }
                    if (runner.LastUsed < (DateTime.UtcNow - span))
                    {
                        _runners.Remove(runner.Repository);
                        runner.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                this._isDisposed = true;
                if (this._runner != null)
                {
                    if (this._subscription != null)
                    {
                        this._runner.DataContextChanged -= this._subscription;
                    }
                    this._subscription = null;
                    lock (_runners)
                    {
                        this._runner.LastUsed = DateTime.UtcNow;
                        this._runner.RefCount--;
                        if (this._runner.RefCount < 0)
                        {
                            this._runner.RefCount = 0;
                        }
                    }
                    this._runner = null;
                }
            }
        }

        public void GetDataContextInfo(SchemaChangeTestMode schemaTestMode)
        {
            this.GetDataContextInfo(this._subscription, schemaTestMode);
        }

        private void GetDataContextInfo(DataContextCallback callback, SchemaChangeTestMode schemaTestMode)
        {
            this._runner.Start(callback, schemaTestMode);
        }

        public static void GetDataContextInfo(LINQPad.Repository r, DataContextCallback callback, SchemaChangeTestMode schemaTestMode)
        {
            DataContextManager manager = new DataContextManager(r, null);
            DataContextCallback callback2 = delegate (DataContextInfo info) {
                manager.Dispose();
                if (callback != null)
                {
                    callback(info);
                }
            };
            if (!((callback != null) || manager.HasOtherSubscribers))
            {
                manager.Dispose();
            }
            else
            {
                manager.GetDataContextInfo(callback2, schemaTestMode);
            }
        }

        public static void RefreshDataContextInfo(LINQPad.Repository r, SchemaChangeTestMode schemaTestMode)
        {
            if (schemaTestMode == SchemaChangeTestMode.None)
            {
                throw new ArgumentException("schemaTestMode cannot be None when calling RefreshDataContextInfo");
            }
            GetDataContextInfo(r, null, schemaTestMode);
        }

        public static void RefreshIfInUse(LINQPad.Repository r)
        {
            lock (_runners)
            {
                Runner runner;
                if (!_runners.TryGetValue(r, out runner))
                {
                    return;
                }
                if (runner.RefCount <= 0)
                {
                    _runners.Remove(runner.Repository);
                    runner.Dispose();
                    return;
                }
            }
            RefreshDataContextInfo(r, SchemaChangeTestMode.ForceRefresh);
        }

        public static void SubscribeToAllErrors(DataContextCallback callback)
        {
            _globalErrorHandlers = (DataContextCallback) Delegate.Combine(_globalErrorHandlers, callback);
        }

        public static DataContextManager SubscribeToDataContextChanges(LINQPad.Repository r, DataContextCallback subscription)
        {
            return new DataContextManager(r, subscription);
        }

        public bool HasOtherSubscribers
        {
            get
            {
                return (this._runner.RefCount > 1);
            }
        }

        public bool IsDisposed
        {
            get
            {
                return this._isDisposed;
            }
        }

        private class Runner : IDisposable
        {
            private TempFileRef _assemblyPath;
            private bool _busy;
            private DataContextCallback _changeSubscribers;
            private volatile bool _disposed;
            private string _error;
            private DateTime _lastSchemaChange;
            private DateTime _lastUsed;
            private object _locker = new object();
            private DataContextCallback _oneOffSubscribers;
            private LINQPad.Repository _repository;
            private IEnumerable<ExplorerItem> _schema;
            private string _status;
            private Thread _worker;
            internal int RefCount = 1;

            internal event DataContextCallback DataContextChanged
            {
                add
                {
                    lock (this._locker)
                    {
                        this._changeSubscribers = (DataContextCallback) Delegate.Combine(this._changeSubscribers, value);
                    }
                }
                remove
                {
                    lock (this._locker)
                    {
                        this._changeSubscribers = (DataContextCallback) Delegate.Remove(this._changeSubscribers, value);
                    }
                }
            }

            internal Runner(LINQPad.Repository r)
            {
                this._repository = r;
                this._lastUsed = DateTime.UtcNow;
            }

            public void Dispose()
            {
                this._disposed = true;
                this._changeSubscribers = (DataContextCallback) (this._oneOffSubscribers = null);
            }

            internal DataContextInfo GetDCInfo()
            {
                lock (this._locker)
                {
                    this._lastUsed = DateTime.UtcNow;
                    return new DataContextInfo(this._repository, this._busy, this._status, this._error, this._assemblyPath, this._schema);
                }
            }

            private void GetDCInfo(SchemaChangeTestMode schemaTestMode)
            {
                Exception exception;
                bool flag = true;
                try
                {
                    object obj2;
                    try
                    {
                        bool flag2;
                        if (this._disposed)
                        {
                            return;
                        }
                        if (!((flag2 = (this._schema == null) || (schemaTestMode == SchemaChangeTestMode.ForceRefresh)) || (schemaTestMode == SchemaChangeTestMode.None)))
                        {
                            bool? nullable = this.HasSchemaChanged();
                            flag2 = nullable.HasValue ? nullable.GetValueOrDefault() : (schemaTestMode == SchemaChangeTestMode.TestAndFailPositive);
                        }
                        if (flag2)
                        {
                            this.UpdateSchema();
                        }
                        else
                        {
                            flag = false;
                        }
                        if (this._lastSchemaChange == new DateTime())
                        {
                            this.HasSchemaChanged();
                        }
                    }
                    catch (Exception exception1)
                    {
                        exception = exception1;
                        if (!(!this._repository.DriverLoader.IsInternalAuthor || IsUserError(exception)))
                        {
                            Program.ProcessException(exception);
                        }
                        else
                        {
                            Log.Write(exception, "Error opening DataContext");
                        }
                        lock ((obj2 = this._locker))
                        {
                            if (this._worker == Thread.CurrentThread)
                            {
                                this._error = (exception is AccessViolationException) ? "An AccessViolationException was thrown when trying to open the connection." : exception.Message;
                                this._repository.IsAutoGenAssemblyAvailable = false;
                                this._repository.AutoGenAssemblyFailed = true;
                            }
                        }
                    }
                    Delegate[] invocationList = null;
                    Delegate[] delegateArray2 = null;
                    DataContextInfo dCInfo = null;
                    lock ((obj2 = this._locker))
                    {
                        if (this._worker != Thread.CurrentThread)
                        {
                            return;
                        }
                        this._status = "";
                        this._busy = false;
                        dCInfo = this.GetDCInfo();
                        if (flag && (this._changeSubscribers != null))
                        {
                            invocationList = this._changeSubscribers.GetInvocationList();
                            foreach (DataContextCallback callback in this._changeSubscribers.GetInvocationList())
                            {
                                if (this._oneOffSubscribers == null)
                                {
                                    break;
                                }
                                this._oneOffSubscribers = (DataContextCallback) Delegate.Remove(this._oneOffSubscribers, callback);
                            }
                        }
                        if (this._oneOffSubscribers != null)
                        {
                            delegateArray2 = this._oneOffSubscribers.GetInvocationList();
                        }
                        this._oneOffSubscribers = null;
                    }
                    if (invocationList != null)
                    {
                        foreach (DataContextCallback callback2 in invocationList)
                        {
                            callback2(dCInfo);
                        }
                    }
                    if (delegateArray2 != null)
                    {
                        foreach (DataContextCallback callback2 in delegateArray2)
                        {
                            callback2(dCInfo);
                        }
                    }
                    if (dCInfo.Error != null)
                    {
                        DataContextManager._globalErrorHandlers(dCInfo);
                    }
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    Program.ProcessException(exception);
                }
            }

            private bool? HasSchemaChanged()
            {
                if (!this._repository.DriverLoader.IsValid)
                {
                    return false;
                }
                DateTime? lastSchemaUpdate = ((DynamicDataContextDriver) this._repository.DriverLoader.Driver).GetLastSchemaUpdate(this._repository);
                if (!lastSchemaUpdate.HasValue)
                {
                    return null;
                }
                lock (this._locker)
                {
                    if (this._worker != Thread.CurrentThread)
                    {
                        return false;
                    }
                    if (lastSchemaUpdate.Value == this._lastSchemaChange)
                    {
                        return false;
                    }
                    this._lastSchemaChange = lastSchemaUpdate.Value;
                }
                return true;
            }

            private static bool IsUserError(Exception ex)
            {
                return ((((((ex is SqlException) || (ex is UriFormatException)) || ((ex is SqlCeNotInstalledException) || ex.GetType().Name.StartsWith("SqlCe", StringComparison.Ordinal))) || ((ex.GetType().Name.StartsWith("Oracle", StringComparison.Ordinal) || ex.GetType().Name.StartsWith("mysql", StringComparison.OrdinalIgnoreCase)) || ex.GetType().Name.StartsWith("sqlite", StringComparison.OrdinalIgnoreCase))) || ((((ex is InvalidOperationException) && ex.Message.Contains("Make sure that Oracle Client Software is installed")) || ((ex is ArgumentException) && ex.StackTrace.Contains("MySql.Data.MySqlClient.MySqlConnection.set_ConnectionString"))) || (((ex is ArgumentException) && ex.StackTrace.Contains("System.Data.SQLite.SQLiteConnection.Open()")) || ((ex is AccessViolationException) || (ex is WebException))))) || (ex is DisplayToUserException));
            }

            private void NotifyChange()
            {
                lock (this._locker)
                {
                    if ((this._worker == Thread.CurrentThread) && (this._changeSubscribers != null))
                    {
                        this._changeSubscribers(this.GetDCInfo());
                    }
                }
            }

            internal void Start(DataContextCallback callback, SchemaChangeTestMode schemaTestMode)
            {
                ThreadStart start = null;
                bool flag = false;
                lock (this._locker)
                {
                    this._lastUsed = DateTime.UtcNow;
                    if (callback != null)
                    {
                        this._oneOffSubscribers = (DataContextCallback) Delegate.Combine(this._oneOffSubscribers, callback);
                    }
                    if (!(this._busy && (schemaTestMode != SchemaChangeTestMode.ForceRefresh)))
                    {
                        this._busy = flag = true;
                        this._error = null;
                        this._status = "Connecting";
                    }
                    if (flag)
                    {
                        if (start == null)
                        {
                            start = () => this.GetDCInfo(schemaTestMode);
                        }
                        Thread thread = new Thread(start) {
                            IsBackground = true,
                            Name = "DataContext Refresh",
                            Priority = ThreadPriority.BelowNormal
                        };
                        this._worker = thread;
                        this._worker.Start();
                    }
                }
            }

            private void UpdateSchema()
            {
                object obj2;
                LINQPad.Repository repository;
                lock ((obj2 = this._locker))
                {
                    if ((this._worker != Thread.CurrentThread) || this._disposed)
                    {
                        return;
                    }
                    this._status = "Populating";
                }
                this.NotifyChange();
                TempFileRef random = TempFileRef.GetRandom("TypedDataContext", ".dll");
                AssemblyName assemblyToBuild = new AssemblyName(Path.GetFileNameWithoutExtension(random.FileName)) {
                    CodeBase = random.FullPath
                };
                lock ((obj2 = this._locker))
                {
                    if ((this._worker != Thread.CurrentThread) || this._disposed)
                    {
                        return;
                    }
                    repository = this._repository.Clone();
                }
                IEnumerable<ExplorerItem> enumerable = null;
                string nameSpace = "LINQPad.User";
                string typeName = "TypedDataContext";
                IDictionary<string, object> sessionData = null;
                using (DomainIsolator isolator = new DomainIsolator(repository.DriverLoader.CreateNewDriverDomain("LINQPad Schema Generator", null, null)))
                {
                    enumerable = isolator.GetInstance<SchemaBuilder>().GetSchemaAndBuildAssembly(repository.GetStore().ToString(), assemblyToBuild, ref nameSpace, ref typeName, Program.AllowOneToOne, out sessionData);
                }
                lock ((obj2 = this._locker))
                {
                    if (this._worker == Thread.CurrentThread)
                    {
                        this._repository.AutoGenNamespace = nameSpace;
                        this._repository.AutoGenTypeName = typeName;
                        this._repository.IsAutoGenAssemblyAvailable = true;
                        if (sessionData != null)
                        {
                            this._repository.SessionData = sessionData;
                        }
                        this._schema = enumerable;
                        this._assemblyPath = random;
                    }
                }
            }

            internal DateTime LastUsed
            {
                get
                {
                    lock (this._locker)
                    {
                        return this._lastUsed;
                    }
                }
                set
                {
                    lock (this._locker)
                    {
                        this._lastUsed = value;
                    }
                }
            }

            internal LINQPad.Repository Repository
            {
                get
                {
                    return this._repository;
                }
            }
        }
    }
}

