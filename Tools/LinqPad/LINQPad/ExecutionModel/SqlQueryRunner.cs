namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class SqlQueryRunner : TextQueryRunner
    {
        public SqlQueryRunner(Server server, string query) : base(server, query)
        {
        }

        public override object Run()
        {
            if (base.Server.Repository == null)
            {
                base.Server.PostStatus(new QueryStatusEventArgs("Could not execute", "Error: no database specified"));
                return null;
            }
            QueryStatusEventArgs args = new QueryStatusEventArgs();
            int num = 0;
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                IDbConnection proxy = Server.OpenCachedCx(base.Server.Repository);
                if (proxy is LINQPadDbConnection)
                {
                    proxy = ((LINQPadDbConnection) proxy).Proxy;
                }
                MatchCollection matchs = Regex.Matches(base.query, @"^\s*go\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                int num2 = 0;
                int startIndex = 0;
                int num4 = 0;
                while (num2 != matchs.Count)
                {
                    string input = base.query.Substring(startIndex, matchs[num2].Index - startIndex) + "\r\n";
                    startIndex = matchs[num2].Index + matchs[num2].Length;
                Label_00DB:
                    if (input.Trim().Length > 0)
                    {
                        DbProviderFactory providerFactory = base.Server.Repository.GetProviderFactory(proxy as DbConnection);
                        DbDataAdapter adapter = providerFactory.CreateDataAdapter();
                        DbCommand command = providerFactory.CreateCommand();
                        if (command is LINQPadDbCommand)
                        {
                            command = ((LINQPadDbCommand) command).Proxy;
                        }
                        if (proxy.GetType().Name.ToString().IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            command.CommandText = input.Replace("\r\n", "\n");
                        }
                        else
                        {
                            command.CommandText = input;
                        }
                        command.Connection = (DbConnection) proxy;
                        command.CommandTimeout = 0;
                        adapter.SelectCommand = command;
                        base.Server.RunningCmd = command;
                        DataSet ds = new DataSet();
                        LINQPadDbController.SqlLog.WriteLine(Regex.Replace(input, "(?<!\r)\n", "\r\n").Trim() + "\r\n\r\nGO\r\n");
                        stopwatch.Start();
                        SqlConnection connection2 = proxy as SqlConnection;
                        SortedDictionary<int, StringBuilder> messages = new SortedDictionary<int, StringBuilder>();
                        SqlInfoMessageEventHandler handler = delegate (object sender, SqlInfoMessageEventArgs e) {
                            int key = ds.Tables.Count;
                            if (key == 0)
                            {
                                this.Server.ResultsWriter.WriteLine(e.Message);
                            }
                            else if (messages.ContainsKey(key))
                            {
                                messages[key].Append("\r\n" + e.Message);
                            }
                            else
                            {
                                messages[key] = new StringBuilder(e.Message);
                            }
                        };
                        try
                        {
                            if (connection2 != null)
                            {
                                connection2.InfoMessage += handler;
                            }
                            adapter.Fill(ds);
                        }
                        finally
                        {
                            stopwatch.Stop();
                            base.Server.RunningCmd = null;
                            if (connection2 != null)
                            {
                                connection2.InfoMessage -= handler;
                            }
                        }
                        int num5 = 0;
                        foreach (DataTable table in ds.Tables)
                        {
                            if (base.Server.WriteResultsToGrids)
                            {
                                table.Explore<DataTable>("Grid " + ++num4);
                            }
                            else
                            {
                                base.Server.ResultsWriter.WriteLine(table);
                            }
                            if (messages.ContainsKey(++num5))
                            {
                                base.Server.ResultsWriter.WriteLine(messages[num5].ToString());
                            }
                        }
                    }
                    num = base.query.Substring(0, startIndex).Count<char>(c => c == '\n');
                    if (num2++ < matchs.Count)
                    {
                        continue;
                    }
                    goto Label_039B;
                Label_0372:
                    input = base.query.Substring(startIndex);
                    goto Label_00DB;
                }
                goto Label_0372;
            Label_039B:
                args.StatusMessage = "Query successful";
                base.Server.ResultsWriter.Flush();
                if (base.Server.Repository.DynamicSchema)
                {
                    args.DataContextRefreshRequired = true;
                }
            }
            catch (SqlException exception)
            {
                args.ErrorMessage = string.Concat(new object[] { "Error ", exception.Number, ": ", exception.Message });
                args.StatusMessage = "Runtime error";
                args.ErrorLine = exception.LineNumber + num;
            }
            catch (Exception exception2)
            {
                args.ErrorMessage = exception2.Message;
                args.StatusMessage = "Runtime error";
            }
            args.ExecTime = stopwatch.Elapsed;
            base.Server.PostStatus(args);
            return null;
        }
    }
}

