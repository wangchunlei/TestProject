namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class DataContextBase : DataContext
    {
        internal static TextWriter SqlLog;

        public DataContextBase(IDbConnection connection) : base(WrapConnection(connection))
        {
            this.Init();
        }

        public DataContextBase(string fileOrConnectionString) : base(GetConnection(fileOrConnectionString))
        {
            this.Init();
        }

        public DataContextBase(IDbConnection connection, MappingSource mappingSource) : base(WrapConnection(connection), mappingSource)
        {
            this.Init();
        }

        public DataContextBase(string fileOrConnectionString, MappingSource mappingSource) : base(GetConnection(fileOrConnectionString), mappingSource)
        {
            this.Init();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public ReturnDataSet ExecuteStoredProcedure(string name, params object[] args)
        {
            bool flag;
            ReturnDataSet set;
            DbCommand command = this.Connection.CreateCommand();
            command.CommandText = name;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 0;
            if (!(flag = this.Connection.State == ConnectionState.Open))
            {
                this.Connection.Open();
            }
            try
            {
                SqlCommand proxy = command as SqlCommand;
                if ((proxy == null) && (command is LINQPadDbCommand))
                {
                    proxy = ((LINQPadDbCommand) command).Proxy as SqlCommand;
                }
                if (proxy == null)
                {
                    return new ReturnDataSet();
                }
                SqlCommandBuilder.DeriveParameters(proxy);
                int num = 1;
                List<DbParameter> list = new List<DbParameter>();
                foreach (object obj2 in args)
                {
                    if (num == command.Parameters.Count)
                    {
                        break;
                    }
                    object obj3 = obj2;
                    IOptional optional = obj3 as IOptional;
                    if (optional != null)
                    {
                        if (!optional.HasValue)
                        {
                            num++;
                            continue;
                        }
                        obj3 = optional.Value;
                    }
                    if (obj3 is XNode)
                    {
                        obj3 = obj3.ToString();
                    }
                    else if (obj3 is Binary)
                    {
                        obj3 = ((Binary) obj3).ToArray();
                    }
                    command.Parameters[num].Value = (obj3 == null) ? DBNull.Value : obj3;
                    num++;
                }
                foreach (DbParameter parameter in list)
                {
                    proxy.Parameters.Remove(parameter);
                }
                LINQPadDbController.DbCommandExecuting(proxy);
                SqlDataAdapter adapter = new SqlDataAdapter(proxy);
                ReturnDataSet dataSet = new ReturnDataSet();
                Stopwatch stopwatch = Stopwatch.StartNew();
                adapter.Fill(dataSet);
                stopwatch.Stop();
                if (proxy.Parameters[0].Value is int)
                {
                    dataSet.ReturnValue = (int) proxy.Parameters[0].Value;
                }
                Dictionary<string, object> source = new Dictionary<string, object>();
                foreach (SqlParameter parameter2 in proxy.Parameters)
                {
                    if ((parameter2.Direction == ParameterDirection.InputOutput) || (parameter2.Direction == ParameterDirection.Output))
                    {
                        source[parameter2.ParameterName] = parameter2.Value;
                    }
                }
                if (source.Any<KeyValuePair<string, object>>())
                {
                    dataSet.OutputParameters = source;
                }
                int num3 = 0;
                foreach (DataTable table in dataSet.Tables)
                {
                    table.TableName = "Result Set " + num3++;
                }
                LINQPadDbController.DbCommandFinished(stopwatch.Elapsed);
                set = dataSet;
            }
            finally
            {
                if (!flag)
                {
                    this.Connection.Close();
                }
            }
            return set;
        }

        public static IDbConnection GetConnection(string fileOrConnectionString)
        {
            return GetConnection(fileOrConnectionString, null);
        }

        public static IDbConnection GetConnection(string fileOrConnectionString, string provider)
        {
            if (fileOrConnectionString == null)
            {
                return null;
            }
            string connectionString = GetConnectionString(fileOrConnectionString);
            if (!(((provider == null) || !ProviderNames.IsSqlCE(provider)) ? IsCompactDatabase(connectionString) : true))
            {
                return new LINQPadDbConnection(new SqlConnection(connectionString));
            }
            if (!((provider != null) && ProviderNames.IsSqlCE40(provider)))
            {
                return new SqlCE35Factory().GetCx(connectionString);
            }
            return new SqlCE40Factory().GetCx(connectionString);
        }

        private static string GetConnectionString(string fileOrConnectionString)
        {
            if (fileOrConnectionString.IndexOf('=') > 0)
            {
                return fileOrConnectionString;
            }
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            if (fileOrConnectionString.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add("AttachDBFileName", fileOrConnectionString);
                builder.Add("Server", @"localhost\sqlexpress");
                builder.Add("Integrated Security", "SSPI");
                builder.Add("User Instance", "true");
                builder.Add("MultipleActiveResultSets", "true");
            }
            else
            {
                if (!fileOrConnectionString.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
                {
                    return fileOrConnectionString;
                }
                builder.Add("Data Source", fileOrConnectionString);
            }
            return builder.ToString();
        }

        private void Init()
        {
            base.CommandTimeout = 0;
            if (!(base.Connection is LINQPadDbConnection))
            {
                base.Log = SqlLog;
            }
        }

        private static bool IsCompactDatabase(string cxString)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder {
                ConnectionString = cxString
            };
            if (!builder.ContainsKey("Data Source"))
            {
                return false;
            }
            string str = (string) builder["Data Source"];
            if (str == null)
            {
                return false;
            }
            return str.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase);
        }

        private static IDbConnection WrapConnection(IDbConnection cx)
        {
            if (cx is LINQPadDbConnection)
            {
                return cx;
            }
            SqlConnection proxy = cx as SqlConnection;
            if (proxy == null)
            {
                return cx;
            }
            return new LINQPadDbConnection(proxy);
        }

        public DbConnection Connection
        {
            get
            {
                if (base.Connection is LINQPadDbConnection)
                {
                    return ((LINQPadDbConnection) base.Connection).Proxy;
                }
                return base.Connection;
            }
        }

        public DbTransaction Transaction
        {
            get
            {
                if (base.Transaction is LINQPadDbTransaction)
                {
                    return ((LINQPadDbTransaction) base.Transaction).Proxy;
                }
                return base.Transaction;
            }
            set
            {
                if (!(!(value is LINQPadDbTransaction) && (base.Connection is LINQPadDbConnection)))
                {
                    base.Transaction = value;
                }
                else
                {
                    base.Transaction = new LINQPadDbTransaction((LINQPadDbConnection) base.Connection, value);
                }
            }
        }
    }
}

