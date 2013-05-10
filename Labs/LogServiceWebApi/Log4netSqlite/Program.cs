using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;

namespace Log4netSqlite
{
    public class Program
    {
        private static readonly string connectionString = @"Data Source=|DataDirectory|\Logs\DebugLog.db;Synchronous=Off";
        private static readonly object _locker = new object();
        private static SQLiteConnection sqLiteConnection;

        public static void PersistentData()
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            bool exist = File.Exists(Path.Combine(dataDirectory, @"Logs\DebugLog.db"));
            if (!exist)
            {
                System.IO.Directory.CreateDirectory("Logs");
                SQLiteConnection.CreateFile(@"Logs\DebugLog.db");
                sqLiteConnection = new SQLiteConnection(connectionString);

                sqLiteConnection.Open();
                //create user table
                var userTableSql = @"CREATE TABLE [log4net]
                                    (
                                      [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                                      [appdomain] varchar, 
                                      [aspnetcache] varchar, 
                                      [aspnetcontext] varchar, 
                                      [aspnetrequest] varchar, 
                                      [aspnetsession] varchar, 
                                      [date] datetime, 
                                      [exception] varchar, 
                                      [file] varchar, 
                                      [identity] varchar, 
                                      [location] varchar, 
                                      [level] varchar, 
                                      [line] integer, 
                                      [logger] varchar, 
                                      [message] varchar, 
                                      [method] varchar, 
                                      [ndc] varchar, 
                                      [property] varchar, 
                                      [stacktrace] varchar, 
                                      [stacktracedetail] varchar, 
                                      [timestamp] bigint, 
                                      [thread] varchar, 
                                      [type] varchar, 
                                      [username] varchar, 
                                      [utcdate] datetime, 
                                      [appfree1] varchar, 
                                      [appfree2] varchar, 
                                      [appfree3] varchar);";
                ExecuteSql(userTableSql);
            }
            else
            {
                sqLiteConnection = new SQLiteConnection(connectionString);
                sqLiteConnection.Open();
            }
        }
        private static void ExecuteSql(string sqlScript)
        {
            lock (_locker)
            {
                try
                {
                    //sqLiteConnection.Open();
                    var command = new SQLiteCommand(sqlScript, sqLiteConnection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    //sqLiteConnection.Close();
                }
            }
        }
        public static ExpandoObject Expando(IDictionary<string, object> dictionary)
        {
            ExpandoObject expandoObject = new ExpandoObject();
            IDictionary<string, object> objects = expandoObject;

            foreach (var item in dictionary)
            {
                bool processed = false;

                if (item.Value is IDictionary<string, object>)
                {
                    objects.Add(item.Key, Expando((IDictionary<string, object>)item.Value));
                    processed = true;
                }
                else if (item.Value is ICollection)
                {
                    List<object> itemList = new List<object>();

                    foreach (var item2 in (ICollection)item.Value)

                        if (item2 is IDictionary<string, object>)
                            itemList.Add(Expando((IDictionary<string, object>)item2));
                        else
                            itemList.Add(Expando(new Dictionary<string, object> { { "Unknown", item2 } }));

                    if (itemList.Count > 0)
                    {
                        objects.Add(item.Key, itemList);
                        processed = true;
                    }
                }

                if (!processed)
                    objects.Add(item);
            }

            return expandoObject;
        }
        public static IQueryable<dynamic> Query(string filter)
        {
            var users = new List<Dictionary<string, dynamic>>();
            lock (_locker)
            {
                try
                {
                    //sqLiteConnection.Open();
                    string selectStr = string.Format(@"select * from [log4net] where 1=1 " + filter);
                    var command = new SQLiteCommand(selectStr, sqLiteConnection);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var user = new Dictionary<string, dynamic>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            user.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        users.Add(user);
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    //sqLiteConnection.Close();
                }
            }
            return users.Select(u => Expando(u)).AsQueryable();
        }
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", "Logs");
            PersistentData();
            var configFile = Assembly.GetExecutingAssembly().GetManifestResourceStream("Log4netSqlite.log4net.config");
            log4net.Config.XmlConfigurator.Configure(configFile);
            var logger = log4net.LogManager.GetLogger("test");
            logger.Debug("123");
            logger.Warn("warn");
            var abc = Query("");
        }
    }
}
