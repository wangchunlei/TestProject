namespace LINQPad
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class LINQPadDbController
    {
        private static bool _ce35Patched;
        private static bool _ce40Patched;
        private static DataTable _originalConfigTable;
        private static bool _patchRan;
        private static bool _sqlPatched;

        internal static void DbCommandExecuting(DbCommand cmd)
        {
            if (ExecutionEngine.SqlTranslationsEnabled)
            {
                TextWriter sqlLog = SqlLog;
                if (sqlLog != null)
                {
                    lock (sqlLog)
                    {
                        bool flag2 = false;
                        if ((cmd.Connection is SqlConnection) || ((cmd.Connection is LINQPadDbConnection) && (((LINQPadDbConnection) cmd.Connection).Proxy is SqlConnection)))
                        {
                            try
                            {
                                string serverVersion = cmd.Connection.ServerVersion;
                                if (!(string.IsNullOrEmpty(serverVersion) || (int.Parse(serverVersion.Split(new char[] { '.' })[0]) < 10)))
                                {
                                    flag2 = true;
                                }
                            }
                            catch
                            {
                            }
                        }
                        if (SqlLog.GetStringBuilder().Length > 0)
                        {
                            sqlLog.WriteLine("GO\r\n");
                        }
                        bool flag3 = false;
                        if (cmd.Parameters.Count > 0)
                        {
                            sqlLog.WriteLine("-- Region Parameters");
                            foreach (DbParameter parameter in cmd.Parameters)
                            {
                                SqlParameter parameter2 = parameter as SqlParameter;
                                if (parameter2 == null)
                                {
                                    sqlLog.WriteLine(string.Concat(new object[] { "-- ", parameter.ParameterName, ": ", parameter.DbType, " [", parameter.Value, "]" }));
                                }
                                else if ((parameter2.Value == null) && ((parameter2.Direction == ParameterDirection.Input) || (parameter2.Direction == ParameterDirection.InputOutput)))
                                {
                                    flag3 = true;
                                }
                                else
                                {
                                    sqlLog.WriteLine(SqlParameterFormatter.GetInitializer(parameter2, flag2));
                                }
                            }
                            sqlLog.WriteLine("-- EndRegion");
                        }
                        if (cmd.CommandType == CommandType.Text)
                        {
                            sqlLog.WriteLine(cmd.CommandText);
                        }
                        else if (cmd.CommandType == CommandType.TableDirect)
                        {
                            sqlLog.WriteLine("select * from " + cmd.CommandText);
                        }
                        else
                        {
                            DbParameter parameter3 = cmd.Parameters.OfType<DbParameter>().FirstOrDefault<DbParameter>(p => p.Direction == ParameterDirection.ReturnValue);
                            StringBuilder builder = new StringBuilder("exec ");
                            if (parameter3 != null)
                            {
                                builder.Append(parameter3.ParameterName + " = ");
                            }
                            builder.Append(cmd.CommandText);
                            int num = 0;
                            foreach (DbParameter parameter in cmd.Parameters)
                            {
                                if ((parameter.Direction != ParameterDirection.ReturnValue) && (parameter.Value != null))
                                {
                                    builder.Append(((num++ > 0) ? "," : "") + " " + (flag3 ? (parameter.ParameterName + "=") : "") + parameter.ParameterName + ((parameter.Direction == ParameterDirection.Input) ? "" : " OUTPUT"));
                                }
                            }
                            sqlLog.WriteLine(builder.ToString());
                        }
                    }
                }
            }
        }

        internal static void DbCommandFinished(TimeSpan timeSpan)
        {
        }

        internal static void InstallCustomProviders()
        {
            if (!_patchRan)
            {
                _patchRan = true;
                string informationalVersion = ((AssemblyInformationalVersionAttribute) typeof(DbProviderFactories).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true)[0]).InformationalVersion;
                string str2 = "4.0";
                string name = "GetProviderTable";
                if (informationalVersion.StartsWith(str2, StringComparison.Ordinal))
                {
                    try
                    {
                        bool flag = LINQPadSqlClientProviderFactory.OriginalFactory != null;
                        bool flag2 = LINQPadSqlCE35ProviderFactory.OriginalFactory != null;
                        bool flag3 = LINQPadSqlCE40ProviderFactory.OriginalFactory != null;
                        if ((flag || flag2) || flag3)
                        {
                            object obj2 = typeof(DbProviderFactories).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
                            DataTable data = obj2 as DataTable;
                            DataTable table2 = data.Copy();
                            if (flag && PatchProviderConfigTable(data, "System.Data.SqlClient", typeof(LINQPadSqlClientProviderFactory)))
                            {
                                _sqlPatched = true;
                            }
                            if (flag2 && PatchProviderConfigTable(data, "System.Data.SqlServerCe.3.5", typeof(LINQPadSqlCE35ProviderFactory)))
                            {
                                _ce35Patched = true;
                            }
                            if (flag3 && PatchProviderConfigTable(data, "System.Data.SqlServerCe.4.0", typeof(LINQPadSqlCE40ProviderFactory)))
                            {
                                _ce40Patched = true;
                            }
                            if (_sqlPatched || (_ce35Patched | _ce40Patched))
                            {
                                string str4 = "_providerTable";
                                typeof(DbProviderFactories).GetField(str4, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, obj2);
                                _originalConfigTable = table2;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Write(exception, "InstallCustomDbProviders");
                    }
                }
            }
        }

        private static bool PatchProviderConfigTable(DataTable data, string invariantName, Type newType)
        {
            if (data == null)
            {
                return false;
            }
            DataTable table = data;
            DataRow row = table.Rows.OfType<DataRow>().FirstOrDefault<DataRow>(r => invariantName.Equals(r["InvariantName"]));
            if (row == null)
            {
                return false;
            }
            object[] objArray = row.ItemArray.ToArray<object>();
            row.Delete();
            DataRow row2 = table.NewRow();
            row2.ItemArray = objArray;
            row2["AssemblyQualifiedName"] = newType.AssemblyQualifiedName;
            table.Rows.Add(row2);
            data.AcceptChanges();
            return true;
        }

        internal static void UnpatchProviderConfigTable()
        {
            if (_originalConfigTable != null)
            {
                try
                {
                    typeof(DbProviderFactories).GetField("_providerTable", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _originalConfigTable);
                    _originalConfigTable = null;
                    _ce35Patched = _ce40Patched = _sqlPatched = _patchRan = false;
                }
                catch
                {
                }
            }
        }

        public static bool IsSqlCE35Patched
        {
            get
            {
                return _ce35Patched;
            }
        }

        public static bool IsSqlCE40Patched
        {
            get
            {
                return _ce40Patched;
            }
        }

        public static bool IsSqlPatched
        {
            get
            {
                return _sqlPatched;
            }
        }

        public static StringWriter SqlLog
        {
            [CompilerGenerated]
            get
            {
                return <SqlLog>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <SqlLog>k__BackingField = value;
            }
        }
    }
}

