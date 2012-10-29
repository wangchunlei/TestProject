namespace LINQPad.ExecutionModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    internal class ESqlQueryRunner : TextQueryRunner
    {
        public ESqlQueryRunner(Server server, string query) : base(server, query)
        {
        }

        public override object Run()
        {
            if (base.Server.Repository == null)
            {
                base.Server.PostStatus(new QueryStatusEventArgs("Could not execute", "Error: no database specified"));
                return null;
            }
            if (string.IsNullOrEmpty(base.query))
            {
                base.Server.PostStatus(new QueryStatusEventArgs("Could not execute", "Error: empty query"));
                return null;
            }
            QueryStatusEventArgs args = new QueryStatusEventArgs();
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                string customAssemblyPath = base.Server.Repository.CustomAssemblyPath;
                if (File.Exists(customAssemblyPath))
                {
                    Assembly.LoadFrom(customAssemblyPath);
                }
                stopwatch.Start();
                base.Server.Repository.DriverLoader.Driver.ExecuteESqlQuery(base.Server.Repository, base.query);
            }
            catch (ThreadAbortException)
            {
                stopwatch.Stop();
            }
            catch (Exception innerException)
            {
                stopwatch.Stop();
                if (innerException is TargetInvocationException)
                {
                    innerException = ((TargetInvocationException) innerException).InnerException;
                }
                args.ErrorMessage = innerException.GetType().Name + ": " + innerException.Message;
                args.StatusMessage = "Error running query";
                PropertyInfo property = innerException.GetType().GetProperty("Line");
                PropertyInfo info2 = innerException.GetType().GetProperty("Column");
                if ((property != null) && (property.PropertyType == typeof(int)))
                {
                    args.ErrorLine = (int) property.GetValue(innerException, null);
                }
                if ((info2 != null) && (info2.PropertyType == typeof(int)))
                {
                    args.ErrorColumn = (int) info2.GetValue(innerException, null);
                }
                Console.WriteLine(innerException);
            }
            finally
            {
                stopwatch.Stop();
                args.ExecTime = stopwatch.Elapsed;
            }
            base.Server.PostStatus(args);
            return null;
        }
    }
}

