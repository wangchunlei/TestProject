namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    internal class ClrQueryRunner : QueryRunner
    {
        private bool _executionTrackingEnabled;
        private Assembly _queryAssem;

        public ClrQueryRunner(Server server, bool executionTrackingEnabled) : base(server)
        {
            this._executionTrackingEnabled = executionTrackingEnabled;
        }

        public override void Prepare()
        {
            if (this._executionTrackingEnabled)
            {
                try
                {
                    new StackTrace(true);
                }
                catch
                {
                }
            }
            string queryAssemblyPath = base.Server.QueryAssemblyPath;
            if (Path.GetFileNameWithoutExtension(queryAssemblyPath).StartsWith("myextensions.fw", StringComparison.OrdinalIgnoreCase))
            {
                this._queryAssem = Assembly.Load(File.ReadAllBytes(queryAssemblyPath));
            }
            else
            {
                try
                {
                    this._queryAssem = Assembly.LoadFrom(queryAssemblyPath);
                }
                catch
                {
                    if (!File.Exists(queryAssemblyPath))
                    {
                        Log.Write("File: " + queryAssemblyPath + " does not exist and so cannot be loaded.");
                    }
                    else
                    {
                        Log.Write("File: " + queryAssemblyPath + " exists but cannot be loaded.");
                    }
                    Thread.Sleep(100);
                    this._queryAssem = Assembly.LoadFrom(queryAssemblyPath);
                }
            }
            Server.CurrentQueryAssemblyPath = this._queryAssem.Location;
            string directoryName = Path.GetDirectoryName(queryAssemblyPath);
            foreach (AssemblyName name in this._queryAssem.GetReferencedAssemblies())
            {
                if (name.Name.StartsWith("TypedDataContext", StringComparison.Ordinal))
                {
                    Assembly.LoadFrom(Path.Combine(directoryName, name.Name + ".dll"));
                }
            }
        }

        public override object Run()
        {
            Action cleanup = null;
            QueryExecutionManager executionManager = new QueryExecutionManager(DataContextBase.SqlLog);
            object[] queryArgs = null;
            DataContextDriver dcDriver = base.Server.ResultsWriter.DCDriver = base.Server.DataContextDriver;
            if (dcDriver != null)
            {
                queryArgs = dcDriver.GetContextConstructorArguments(base.Server.Repository);
            }
            Type type = (this._queryAssem == null) ? null : this._queryAssem.GetType("UserQuery");
            object clrQuery = null;
            if (type != null)
            {
                clrQuery = Activator.CreateInstance(type, queryArgs);
            }
            if ((dcDriver != null) && (clrQuery != null))
            {
                dcDriver.InitializeContext(base.Server.Repository, clrQuery, executionManager);
                if (cleanup == null)
                {
                    cleanup = delegate {
                        try
                        {
                            if (!this.Server.HasUserCacheChanged)
                            {
                                dcDriver.TearDownContext(this.Server.Repository, clrQuery, executionManager, queryArgs);
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.Write(exception, "Calling custom driver TearDownContext");
                        }
                    };
                }
                base.Server.AddFinalCleanup(cleanup);
            }
            base.Server.RegisterQueryStart();
            object obj2 = null;
            if (clrQuery != null)
            {
                MethodInfo method = null;
                try
                {
                    method = clrQuery.GetType().GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                catch
                {
                }
                if (method == null)
                {
                    method = clrQuery.GetType().GetMethod("RunUserAuthoredQuery", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                obj2 = method.Invoke(clrQuery, null);
                if ((dcDriver == null) || (executionManager == null))
                {
                    return obj2;
                }
                try
                {
                    dcDriver.OnQueryFinishing(base.Server.Repository, clrQuery, executionManager);
                    return obj2;
                }
                catch
                {
                    return obj2;
                }
            }
            if ((this._queryAssem != null) && (this._queryAssem.EntryPoint != null))
            {
                this._queryAssem.EntryPoint.Invoke(null, null);
            }
            return obj2;
        }
    }
}

