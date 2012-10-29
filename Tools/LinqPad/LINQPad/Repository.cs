namespace LINQPad
{
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Linq;

    [Serializable]
    internal class Repository : ISerializable, IConnectionInfo, IDatabaseInfo, ICustomTypeInfo, IDynamicSchemaOptions, INotifyPropertyChanged, ICustomMemberProvider
    {
        private DCDriverLoader _driverLoader;
        private List<LinkedDatabase> _linkedDbs;
        private static AutoResetEvent _openThrottler = new AutoResetEvent(true);
        private string _password;
        private static object _passwordChangeLocker = new object();
        private static int _serialCount;
        private static Dictionary<string, object> _servers = new Dictionary<string, object>();
        private IDictionary<string, object> _sessionData;
        private XElement _store;
        private static Timer _tmr;
        internal static string[] AssembliesNeedingPathQualification = new string[0];
        internal bool AutoGenAssemblyFailed;
        internal string AutoGenNamespace;
        internal string AutoGenTypeName;
        internal bool DoNotSave;
        private const string ElementName = "Connection";
        internal TempFileRef IntellicachePath;
        internal bool IsAutoGenAssemblyAvailable;
        internal static Dictionary<string, bool> TypesNeedingAssemblyQualification = new Dictionary<string, bool>();

        public event PropertyChangedEventHandler PropertyChanged;

        public Repository() : this(new XElement("Connection"))
        {
        }

        public Repository(XElement store)
        {
            this.AutoGenNamespace = "LINQPad.User";
            this.AutoGenTypeName = "TypedDataContext";
            this._linkedDbs = new List<LinkedDatabase>();
            this._driverLoader = DCDriverLoader.DefaultLoader;
            this.LoadFromStore(store);
        }

        private Repository(SerializationInfo info, StreamingContext context)
        {
            this.AutoGenNamespace = "LINQPad.User";
            this.AutoGenTypeName = "TypedDataContext";
            this._linkedDbs = new List<LinkedDatabase>();
            this._driverLoader = DCDriverLoader.DefaultLoader;
            this.LoadFromStore(XElement.Parse(info.GetString("data")));
            this._sessionData = (Dictionary<string, object>) info.GetValue("sessionData", typeof(Dictionary<string, object>));
        }

        public static bool AreEquivalent(Repository r1, Repository r2)
        {
            if ((r1 == null) && (r2 == null))
            {
                return true;
            }
            if ((r1 == null) || (r2 == null))
            {
                return false;
            }
            return ((r1 == r2) || r1.IsEquivalent(r2));
        }

        private bool ChangeSqlPassword()
        {
            using (UpdatePasswordForm form = new UpdatePasswordForm(this.Server, this.UserName))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    Repository repository = this.Clone();
                    repository.Password = form.OldPassword;
                    try
                    {
                        SqlConnection.ChangePassword(repository.GetCxString(), form.NewPassword);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return false;
                    }
                    this.Password = form.NewPassword;
                    if (this.Parent == null)
                    {
                        this.SaveToDisk();
                    }
                }
            }
            return true;
        }

        public void ClearDatabaseProperties()
        {
            string str;
            this.UserName = str = "";
            this.Server = str = str;
            this.Provider = str = str;
            this.Password = str = str;
            this.AttachFileName = this.Database = str;
            this.MaxDatabaseSize = 0;
            this.UserInstance = false;
            this.ShowServer = false;
            this.NoPluralization = false;
            this.NoCapitalization = false;
            this.ExcludeRoutines = false;
            this.AttachFile = false;
        }

        public Repository Clone()
        {
            return new Repository(new XElement(this.GetStore())) { _driverLoader = this._driverLoader };
        }

        public Repository CreateChild(string databaseName)
        {
            Repository repository = this.Clone();
            repository.Database = databaseName;
            repository.Parent = this;
            repository.ShowServer = true;
            return repository;
        }

        public Repository CreateParent()
        {
            Repository repository = this.Clone();
            repository.Database = "";
            this.Parent = repository;
            return repository;
        }

        public AppDomain CreateSchemaAndRunnerDomain(string name, bool shadowingActive, bool queryExecutionContext)
        {
            string configPath = this.DriverLoader.IsValid ? this.DriverLoader.Driver.GetAppConfigPath(this) : this.AppConfigPath;
            string data = null;
            if (((this.DriverLoader.InternalID == null) && this.DriverLoader.IsValid) && (this.DriverLoader.Driver is StaticDataContextDriver))
            {
                try
                {
                    data = Path.GetDirectoryName(this.CustomAssemblyPath);
                }
                catch (ArgumentException)
                {
                }
                if (data == "")
                {
                    data = null;
                }
            }
            string baseFolderOverride = ((data == null) || shadowingActive) ? (((data == null) || !shadowingActive) ? null : AppDomain.CurrentDomain.BaseDirectory) : data;
            AppDomain domain = this.DriverLoader.CreateNewDriverDomain(name, baseFolderOverride, configPath);
            if (data != null)
            {
                domain.SetData("LINQPad.UseLoadFileForDCDriver", true);
                domain.SetData("LINQPad.DCDriverFolder", this.DriverLoader.GetAssemblyFolder());
                domain.SetData("LINQPad.StaticDCFolder", data);
                if (!queryExecutionContext)
                {
                    domain.DoCallBack(new CrossAppDomainDelegate(Repository.ResolveCustomAssemblies));
                }
            }
            return domain;
        }

        public string Decrypt(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            try
            {
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(s), base.GetType().Assembly.GetName().GetPublicKey(), DataProtectionScope.CurrentUser));
            }
            catch
            {
                return "";
            }
        }

        public string Encrypt(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            try
            {
                return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(s), base.GetType().Assembly.GetName().GetPublicKey(), DataProtectionScope.CurrentUser));
            }
            catch
            {
                return "";
            }
        }

        private static string FormatCustomCxString(string s)
        {
            try
            {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder {
                    ConnectionString = s
                };
                if (builder.ContainsKey("password"))
                {
                    builder.Remove("password");
                }
                if (builder.ContainsKey("pwd"))
                {
                    builder.Remove("pwd");
                }
                string[] toRemove = new string[] { "true", "false", "enabled" };
                s = string.Join(";", (from v in builder.Values.OfType<string>()
                    where !toRemove.Contains<string>(v.ToLowerInvariant()) && (v.Length > 2)
                    select v).ToArray<string>());
            }
            catch
            {
            }
            if (s.Length > 0x55)
            {
                s = s.Substring(0, 80) + "...";
            }
            return s;
        }

        public static List<Repository> FromDisk()
        {
            List<Repository> list = new List<Repository>();
            using (Mutex mutex = GetMutex())
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(3.0), false))
                {
                    return list;
                }
                try
                {
                    XElement xmlFromDisk = GetXmlFromDisk();
                    if (xmlFromDisk == null)
                    {
                        return list;
                    }
                    foreach (XElement element2 in xmlFromDisk.Elements("Connection"))
                    {
                        try
                        {
                            list.Add(new Repository(element2));
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return list;
        }

        private bool GetBool(string key)
        {
            bool? nullable = (bool?) this._store.Element(key);
            return (nullable.HasValue ? nullable.GetValueOrDefault() : false);
        }

        public IDbConnection GetConnection()
        {
            IDbConnection iDbConnection = this.DriverLoader.Driver.GetIDbConnection(this);
            if (iDbConnection == null)
            {
                throw new DisplayToUserException("The driver for this connection does not support SQL queries.");
            }
            return iDbConnection;
        }

        public string[] GetCustomTypesInAssembly()
        {
            return this.GetCustomTypesInAssembly(null);
        }

        public string[] GetCustomTypesInAssembly(string baseTypeName)
        {
            return AssemblyProber.GetTypeNames(this.CustomAssemblyPath, baseTypeName);
        }

        public string GetCxString()
        {
            string password;
            if (this.CustomCxString.Length > 0)
            {
                return this.CustomCxString;
            }
            if ((((this.DriverLoader != null) && (this.DriverLoader != null)) && (this.DriverLoader.SimpleAssemblyName != null)) && (this.DriverLoader.SimpleAssemblyName.ToLowerInvariant() == "iqdriver"))
            {
                throw new DisplayToUserException("Connection is missing a password or other information: right-click the connection and click 'Properties'");
            }
            if (this.IsSqlCE)
            {
                if (this.AttachFileName.Length == 0)
                {
                    return "";
                }
                string str2 = "Data Source=" + this.AttachFileName;
                if (this.Password.Length > 0)
                {
                    password = this.Password;
                    if (password.Contains<char>(';'))
                    {
                        password = "'" + password + "'";
                    }
                    str2 = str2 + ";Password=" + password;
                }
                if (this.MaxDatabaseSize > 0)
                {
                    str2 = str2 + ";Max Database Size=" + this.MaxDatabaseSize;
                }
                return str2;
            }
            if (this.Server.Length == 0)
            {
                return "";
            }
            string str4 = "Data Source=" + this.Server;
            if (this.SqlSecurity)
            {
                password = this.Password;
                if (password.Contains<char>('\''))
                {
                    password = '"' + password + '"';
                }
                else if (password.Contains<char>(';') || password.Contains<char>('"'))
                {
                    password = "'" + password + "'";
                }
                string str5 = str4;
                str4 = str5 + ";User ID=" + this.UserName + ";Password=" + password;
            }
            else
            {
                str4 = str4 + ";Integrated Security=SSPI";
            }
            if (!((!this.AttachFile || (this.AttachFileName.Length <= 0)) || this.IsAzure))
            {
                str4 = str4 + ";AttachDbFilename=" + this.AttachFileName;
            }
            if (!((!this.UserInstance || this.SqlSecurity) || this.IsAzure))
            {
                str4 = str4 + ";User Instance=True";
            }
            if (!((!this.AttachFile || this.IsAzure) ? (this.Database.Length <= 0) : true))
            {
                str4 = str4 + ";Initial Catalog=" + this.Database;
            }
            str4 = str4 + ";app=LINQPad";
            if (!string.IsNullOrEmpty(Util.CurrentQueryName))
            {
                str4 = str4 + " [" + Util.CurrentQueryName.Replace(";", "") + "]";
            }
            if (!this.IsAzure && !((!this.DriverLoader.IsValid || !(this.DriverLoader.Driver is EntityFrameworkDriver)) ? !Program.EnableMARS : false))
            {
                str4 = str4 + ";MultipleActiveResultSets=True";
            }
            if (this.IsAzure || this.EncryptTraffic)
            {
                str4 = str4 + ";Encrypt=true";
            }
            return str4;
        }

        public List<string> GetDatabaseList()
        {
            if (!this.IsSqlServer)
            {
                throw new NotSupportedException("This operation is supported only for SQL Server.");
            }
            List<string> list = new List<string>();
            string cmdText = this.IsAzure ? "select name from sys.databases" : "dbo.sp_MShasdbaccess";
            using (SqlConnection connection = new SqlConnection(this.GetCxString()))
            {
                connection.Open();
                using (SqlDataReader reader = new SqlCommand(cmdText, connection).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader[0].ToString());
                    }
                }
            }
            return list;
        }

        public string[] GetDriverAssemblies()
        {
            if (!this.DriverLoader.IsValid)
            {
                return new string[0];
            }
            string staticDataContextFolder = null;
            if ((this.DriverLoader.Driver is StaticDataContextDriver) && !string.IsNullOrEmpty(this.CustomAssemblyPath))
            {
                try
                {
                    staticDataContextFolder = Path.GetDirectoryName(this.CustomAssemblyPath);
                }
                catch (ArgumentException)
                {
                }
            }
            return (this.DriverLoader.Driver.TryGetFullAssembliesToAdd(staticDataContextFolder, this) ?? new string[0]);
        }

        public string GetFriendlyName()
        {
            return this.GetFriendlyName(FriendlyNameMode.Normal);
        }

        public string GetFriendlyName(FriendlyNameMode mode)
        {
            string connectionDescription;
            if (this.DriverLoader.InternalID == null)
            {
                if (!this.DriverLoader.IsValid)
                {
                    return ("(Unknown driver '" + this.DriverLoader.SimpleAssemblyName + "')");
                }
                if (!string.IsNullOrEmpty(this.DisplayName))
                {
                    return this.DisplayName;
                }
                try
                {
                    return this.DriverLoader.Driver.GetConnectionDescription(this);
                }
                catch (Exception exception)
                {
                    Log.Write(exception);
                    string message = exception.Message;
                    if (((exception is TargetInvocationException) || (exception is TypeInitializationException)) && (exception.InnerException != null))
                    {
                        message = exception.InnerException.Message;
                    }
                    return ("(Error loading driver: " + message + ")");
                }
            }
            if (string.IsNullOrEmpty(this.DisplayName))
            {
                connectionDescription = this.DriverLoader.Driver.GetConnectionDescription(this);
                if (!string.IsNullOrEmpty(connectionDescription))
                {
                    return connectionDescription;
                }
            }
            string customTypeName = "";
            string str5 = "";
            if (!this.DynamicSchema)
            {
                if (!string.IsNullOrEmpty(this.DisplayName))
                {
                    return this.DisplayName;
                }
                string key = this.CustomTypeName.Split(new char[] { '.' }).Last<string>();
                if (mode == FriendlyNameMode.Short)
                {
                    return key;
                }
                bool flag = true;
                bool flag2 = true;
                bool flag3 = false;
                if ((mode != FriendlyNameMode.FullTooltip) && (flag2 = TypesNeedingAssemblyQualification.TryGetValue(key, out flag)))
                {
                    try
                    {
                        flag3 = !AssembliesNeedingPathQualification.Contains<string>(Path.GetFileName(this.CustomAssemblyPath));
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                customTypeName = this.CustomTypeName;
                if (!flag)
                {
                    customTypeName = customTypeName.Split(new char[] { '.' }).Last<string>();
                }
                if (flag2)
                {
                    string customAssemblyPath = this.CustomAssemblyPath;
                    if (flag3)
                    {
                        try
                        {
                            customAssemblyPath = Path.GetFileName(customAssemblyPath);
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    customTypeName = customTypeName + " in " + customAssemblyPath;
                }
                if (mode == FriendlyNameMode.FullTooltip)
                {
                    customTypeName = customTypeName + "\r\nDatabase = ";
                }
                else
                {
                    customTypeName = customTypeName + " (";
                    str5 = ")";
                }
            }
            if (this.CustomCxString.Length > 0)
            {
                if (!string.IsNullOrEmpty(this.DisplayName))
                {
                    return this.DisplayName;
                }
                return FormatCustomCxString(this.CustomCxString);
            }
            if (this.AttachFile || this.IsSqlCE)
            {
                if (!string.IsNullOrEmpty(this.DisplayName))
                {
                    return this.DisplayName;
                }
                bool flag4 = true;
                try
                {
                    flag4 = Path.IsPathRooted(this.AttachFileName);
                }
                catch
                {
                }
                if (!(flag4 && (this.DynamicSchema || (mode == FriendlyNameMode.Short))))
                {
                    return (customTypeName + this.AttachFileName + str5);
                }
                string attachFileName = this.AttachFileName;
                string str9 = "";
                try
                {
                    attachFileName = Path.GetFileName(attachFileName);
                    if (mode != FriendlyNameMode.Short)
                    {
                        str9 = " in " + PathHelper.EncodeFolder(Path.GetDirectoryName(this.AttachFileName));
                    }
                }
                catch (ArgumentException)
                {
                }
                return (customTypeName + attachFileName + str9 + str5);
            }
            string str10 = string.Join(" + ", (from l in this._linkedDbs.Take<LinkedDatabase>(3) select l.ToString()).ToArray<string>());
            if (this._linkedDbs.Count > 3)
            {
                str10 = str10 + "...";
            }
            if (str10.Length > 0)
            {
                str10 = " + " + str10;
            }
            if (((mode == FriendlyNameMode.Normal) || (mode == FriendlyNameMode.Short)) && ((this.Database.Length > 0) && ((this.Parent != null) || (mode == FriendlyNameMode.Short))))
            {
                if (!string.IsNullOrEmpty(this.DisplayName))
                {
                    return ((this.Parent == null) ? this.DisplayName : (this.Database + str10));
                }
                return (customTypeName + this.Database + str5);
            }
            if (!string.IsNullOrEmpty(this.DisplayName))
            {
                connectionDescription = this.DisplayName;
                if ((this.Parent != null) && (this.Database.Length > 0))
                {
                    connectionDescription = connectionDescription + "." + this.Database;
                }
                return connectionDescription;
            }
            string server = this.Server;
            if (this.IsAzure && (mode != FriendlyNameMode.FullTooltip))
            {
                if (server.EndsWith(".database.windows.net", StringComparison.InvariantCultureIgnoreCase))
                {
                    server = server.Substring(0, server.Length - ".database.windows.net".Length);
                }
                if (server.StartsWith("tcp:", StringComparison.InvariantCultureIgnoreCase))
                {
                    server = server.Substring("tcp:".Length);
                }
            }
            string str12 = server + (this.SqlSecurity ? ("." + this.UserName) : "");
            if (this.Database.Length > 0)
            {
                str12 = str12 + "." + this.Database + str10;
            }
            return (customTypeName + str12 + str5);
        }

        private Guid GetGuid(string key)
        {
            return (Guid) this._store.Element(key);
        }

        private int GetInt(string key)
        {
            int? nullable = (int?) this._store.Element(key);
            return (nullable.HasValue ? nullable.GetValueOrDefault() : 0);
        }

        public List<LinkedDatabase> GetLinkedDatabaseList()
        {
            if (!this.IsSqlServer)
            {
                throw new NotSupportedException("This operation is supported only for SQL Server.");
            }
            StringBuilder builder = new StringBuilder();
            List<string> list = new List<string>();
            List<LinkedDatabase> list2 = new List<LinkedDatabase>();
            string cmdText = "select data_source from sys.servers where provider = 'SQLNCLI' and is_linked = 1";
            using (SqlConnection connection = new SqlConnection(this.GetCxString()))
            {
                SqlDataReader reader;
                connection.Open();
                using (reader = new SqlCommand(cmdText, connection).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string item = reader[0].ToString();
                        list.Add(item);
                        builder.AppendLine("select name from [" + item + "].[master].[sys].[databases]");
                    }
                }
                int num = 0;
                using (reader = new SqlCommand(builder.ToString(), connection).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list2.Add(new LinkedDatabase(list[num], reader[0].ToString()));
                        continue;
                    Label_00D6:
                        if (!reader.NextResult())
                        {
                            return list2;
                        }
                        num++;
                    }
                    goto Label_00D6;
                }
            }
        }

        private static Mutex GetMutex()
        {
            return new Mutex(true, "LINQPad.DefaultConnection");
        }

        public IEnumerable<string> GetNames()
        {
            List<string> list = new List<string>();
            Type[] typeArray2 = new Type[] { typeof(IDatabaseInfo), typeof(ICustomTypeInfo), typeof(IDynamicSchemaOptions) };
            foreach (Type t in typeArray2)
            {
                list.AddRange(from p in t.GetProperties() select t.Name.Substring(1) + "." + p.Name);
            }
            list.Add("AppConfigPath");
            list.Add("Persist");
            return list;
        }

        public IEnumerable<LinkedDatabase> GetOtherServerLinkedDatabases()
        {
            return (from l in this._linkedDbs
                where l.Server != null
                select l);
        }

        public DbProviderFactory GetProviderFactory()
        {
            return this.DriverLoader.Driver.GetProviderFactory(this);
        }

        public DbProviderFactory GetProviderFactory(DbConnection cx)
        {
            if (cx == null)
            {
                return this.GetProviderFactory();
            }
            PropertyInfo property = cx.GetType().GetProperty("DbProviderFactory", BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
            {
                return this.GetProviderFactory();
            }
            try
            {
                return (((DbProviderFactory) property.GetValue(cx, null)) ?? this.GetProviderFactory());
            }
            catch
            {
                return this.GetProviderFactory();
            }
        }

        private static Repository GetRepository(XElement e)
        {
            try
            {
                return new Repository(e);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Repository> GetSameServerLinkedRepositories()
        {
            XElement xe = this.GetStore();
            return (from db in this._linkedDbs
                where db.Server == null
                select new Repository(xe) { _linkedDbs = new List<LinkedDatabase>(), Database = db.Database, IncludeSystemObjects = false, AttachFile = false, AttachFileName = "" });
        }

        public string GetSortOrder()
        {
            if (this.DriverLoader.IsValid && (this.DriverLoader.Driver is LinqToSqlDynamicDriver))
            {
                return ((this.IsSqlCE ? "3" : (this.IsAzure ? "2" : "1")) + this.GetFriendlyName());
            }
            return this.GetFriendlyName();
        }

        public XElement GetStore()
        {
            this._store.Elements("LinkedDb").Remove<XElement>();
            this._store.Add(from db in this._linkedDbs select new XElement("LinkedDb", new object[] { db.Database, (db.Server == null) ? null : new XAttribute("Server", db.Server) }));
            if (this._store.Element("DriverData") != null)
            {
                this._store.Element("DriverData").Remove();
            }
            if ((this.DriverData != null) && (!this.DriverData.IsEmpty || this.DriverData.Attributes().Any<XAttribute>()))
            {
                this._store.Add(new XElement("DriverData", new object[] { this.DriverData.Elements(), this.DriverData.Attributes() }));
            }
            return this._store;
        }

        private string GetString(string key)
        {
            string str = (string) this._store.Element(key);
            return ((str == null) ? "" : str.Trim());
        }

        public IEnumerable<Type> GetTypes()
        {
            List<Type> list = new List<Type>();
            Type[] typeArray2 = new Type[] { typeof(IDatabaseInfo), typeof(ICustomTypeInfo), typeof(IDynamicSchemaOptions) };
            foreach (Type type in typeArray2)
            {
                list.AddRange(from p in type.GetProperties() select p.PropertyType);
            }
            list.Add(typeof(string));
            list.Add(typeof(bool));
            return list;
        }

        public IEnumerable<object> GetValues()
        {
            Func<PropertyInfo, object> selector = null;
            List<object> list = new List<object>();
            Type[] typeArray2 = new Type[] { typeof(IDatabaseInfo), typeof(ICustomTypeInfo), typeof(IDynamicSchemaOptions) };
            foreach (Type type in typeArray2)
            {
                if (selector == null)
                {
                    selector = p => p.GetValue(this, null);
                }
                list.AddRange(type.GetProperties().Select<PropertyInfo, object>(selector));
            }
            list.Add(this.AppConfigPath);
            list.Add(this.Persist);
            return list;
        }

        private static XElement GetXmlFromDisk()
        {
            XElement element = new XElement("Connections");
            if (File.Exists(FilePath) || File.Exists(OldFilePath))
            {
                try
                {
                    element = XElement.Load(File.Exists(FilePath) ? FilePath : OldFilePath);
                }
                catch
                {
                    Thread.Sleep(200);
                    try
                    {
                        element = XElement.Load(File.Exists(FilePath) ? FilePath : OldFilePath);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return element;
        }

        public bool IsEquivalent(Repository other)
        {
            if (other == null)
            {
                return false;
            }
            if (!(this.DriverLoader.IsValid && other.DriverLoader.IsValid))
            {
                return (this.GetStore().ToString() == other.GetStore().ToString());
            }
            if (!this.DriverLoader.Equals(other.DriverLoader))
            {
                return false;
            }
            return this.DriverLoader.Driver.AreRepositoriesEquivalent(this, other);
        }

        string ICustomTypeInfo.GetCustomTypeDescription()
        {
            bool flag = true;
            bool flag2 = true;
            bool flag3 = false;
            string key = this.CustomTypeName.Split(new char[] { '.' }).Last<string>();
            if (flag2 = TypesNeedingAssemblyQualification.TryGetValue(key, out flag))
            {
                try
                {
                    flag3 = !AssembliesNeedingPathQualification.Contains<string>(Path.GetFileName(this.CustomAssemblyPath));
                }
                catch (ArgumentException)
                {
                }
            }
            string customTypeName = this.CustomTypeName;
            if (!flag)
            {
                customTypeName = customTypeName.Split(new char[] { '.' }).Last<string>();
            }
            if (!flag2)
            {
                return customTypeName;
            }
            string customAssemblyPath = this.CustomAssemblyPath;
            if (flag3)
            {
                try
                {
                    customAssemblyPath = Path.GetFileName(customAssemblyPath);
                }
                catch (ArgumentException)
                {
                }
            }
            return (customTypeName + " in " + customAssemblyPath);
        }

        bool ICustomTypeInfo.IsEquivalent(ICustomTypeInfo other)
        {
            if (other == null)
            {
                return false;
            }
            if (!string.Equals(this.CustomAssemblyPath, other.CustomAssemblyPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!string.Equals(this.CustomMetadataPath, other.CustomMetadataPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return (this.CustomTypeName == other.CustomTypeName);
        }

        string IDatabaseInfo.GetDatabaseDescription()
        {
            if (this.CustomCxString.Length > 0)
            {
                return FormatCustomCxString(this.CustomCxString);
            }
            if (this.AttachFile || this.IsSqlCE)
            {
                return this.AttachFileName;
            }
            string str2 = this.Server + (this.SqlSecurity ? ("." + this.UserName) : "");
            if (this.Database.Length > 0)
            {
                str2 = str2 + "." + this.Database;
            }
            return str2;
        }

        bool IDatabaseInfo.IsEquivalent(IDatabaseInfo other)
        {
            if (other == null)
            {
                return false;
            }
            if ((this.CustomCxString.Length > 0) != (other.CustomCxString.Length > 0))
            {
                return false;
            }
            if (this.CustomCxString.Length > 0)
            {
                return (this.CustomCxString.ToLowerInvariant() == other.CustomCxString.ToLowerInvariant());
            }
            if (this.IsSqlCE != other.IsSqlCE)
            {
                return false;
            }
            if (this.IsSqlCE)
            {
                return (string.Equals(this.Provider, other.Provider, StringComparison.OrdinalIgnoreCase) && string.Equals(this.AttachFileName, other.AttachFileName, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.Equals(this.Server, other.Server, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (this.SqlSecurity != other.SqlSecurity)
            {
                return false;
            }
            if (!(!this.SqlSecurity || string.Equals(this.UserName, other.UserName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            if (this.AttachFile != other.AttachFile)
            {
                return false;
            }
            if (!(!this.AttachFile || string.Equals(this.AttachFileName, other.AttachFileName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            if (!(this.AttachFile || string.Equals(this.Database, other.Database, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            Repository repository = other as Repository;
            if (!(from d in this._linkedDbs
                orderby d
                select d).SequenceEqual<LinkedDatabase>((from d in repository._linkedDbs
                orderby d
                select d)))
            {
                return false;
            }
            return true;
        }

        public void LoadFromStore(XElement store)
        {
            this._store = store;
            Guid? nullable = (Guid?) this._store.Element("ID");
            if (!nullable.HasValue)
            {
                this.ID = Guid.NewGuid();
            }
            string str = (string) this._store.Element("Password");
            if (!string.IsNullOrEmpty(str))
            {
                this._password = this.Decrypt(str);
            }
            this.AttachFileName = this.AttachFileName;
            XElement other = store.Element("DriverData");
            this.DriverData = (other == null) ? new XElement("DriverData") : new XElement(other);
            this._driverLoader = DCDriverLoader.FromXElement(store.Element("Driver"));
            this._linkedDbs = (from e in store.Elements("LinkedDb")
                select new LinkedDatabase((string) e.Attribute("Server"), e.Value) into e
                orderby e
                select e).ToList<LinkedDatabase>();
        }

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, args);
            }
        }

        public IDbConnection Open()
        {
            return this.Open(false);
        }

        public IDbConnection Open(bool noThrottle)
        {
            object obj2;
            IDbConnection cx = this.GetConnection();
            if (!((!noThrottle && (this.CustomCxString.Length <= 0)) && this.Server.ToUpperInvariant().Contains("SQLEXPRESS")))
            {
                this.OpenCx(cx, noThrottle);
                return cx;
            }
            lock (_servers)
            {
                string server = this.Server;
                if (!_servers.ContainsKey(server))
                {
                    _servers.Add(server, new object());
                }
                obj2 = _servers[server];
            }
            lock (obj2)
            {
                this.OpenCx(cx, noThrottle);
            }
            return cx;
        }

        private void OpenCx(IDbConnection cx, bool noThrottle)
        {
            if (!noThrottle && (Environment.Version.Major < 4))
            {
                lock (_openThrottler)
                {
                    if (_tmr == null)
                    {
                        _tmr = new Timer(state => _openThrottler.Set(), null, 0x5dc, 500);
                    }
                }
                _openThrottler.WaitOne();
            }
            string password = this.Password;
        Label_008D:
            try
            {
                try
                {
                    cx.Open();
                }
                catch (IndexOutOfRangeException exception)
                {
                    throw new DisplayToUserException("SqlConnection has reported an internal error in the connection pool.\r\nYou may need to restart LINQPad.", exception);
                }
                catch (SqlException exception2)
                {
                    if (!UserOptions.Instance.NoSqlPasswordExpiryPrompts && ((exception2.Number == 0x4837) || (exception2.Number == 0x4838)))
                    {
                        lock (_passwordChangeLocker)
                        {
                            if (password != this.Password)
                            {
                                cx = this.GetConnection();
                                goto Label_008D;
                            }
                            if (!this.ChangeSqlPassword())
                            {
                                goto Label_008D;
                            }
                            cx = this.GetConnection();
                            cx.Open();
                            return;
                        }
                    }
                    throw;
                }
                return;
            }
            finally
            {
                _openThrottler.Set();
            }
            goto Label_008D;
        }

        private static void ResolveCustomAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args) {
                string str = new AssemblyName(args.Name).Name + ".dll";
                string path = Path.Combine((string) AppDomain.CurrentDomain.GetData("LINQPad.DCDriverFolder"), str);
                if (File.Exists(path))
                {
                    return Assembly.LoadFrom(path);
                }
                return null;
            };
        }

        public bool SaveToDisk()
        {
            Func<XElement, bool> predicate = null;
            if (this.DoNotSave)
            {
                return false;
            }
            using (Mutex mutex = GetMutex())
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(3.0), false))
                {
                    return false;
                }
                try
                {
                    if (!Directory.Exists(Program.UserDataFolder))
                    {
                        Directory.CreateDirectory(Program.UserDataFolder);
                    }
                    if (this.ID == Guid.Empty)
                    {
                        this.ID = Guid.NewGuid();
                    }
                    XElement xmlFromDisk = GetXmlFromDisk();
                    if (xmlFromDisk == null)
                    {
                        return false;
                    }
                    bool flag2 = false;
                    try
                    {
                        if (predicate == null)
                        {
                            predicate = delegate (XElement e) {
                                Guid? nullable = (Guid?) e.Element("ID");
                                return (nullable.HasValue ? nullable.GetValueOrDefault() : Guid.Empty) == this.ID;
                            };
                        }
                        XElement element2 = xmlFromDisk.Elements("Connection").FirstOrDefault<XElement>(predicate);
                        if (element2 != null)
                        {
                            element2.Remove();
                            flag2 = true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    if (!(flag2 || this.Persist))
                    {
                        return true;
                    }
                    if (this.Persist)
                    {
                        xmlFromDisk.Add(this.GetStore());
                    }
                    try
                    {
                        xmlFromDisk.Save(FilePath);
                    }
                    catch
                    {
                        Thread.Sleep(200);
                        try
                        {
                            xmlFromDisk.Save(FilePath);
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return true;
        }

        private void Set(string key, bool value)
        {
            if (!value)
            {
                this._store.Elements(key).Remove<XElement>();
            }
            else
            {
                this._store.SetElementValue(key, value);
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs(key));
        }

        private void Set(string key, Guid value)
        {
            this._store.SetElementValue(key, value);
            this.OnPropertyChanged(new PropertyChangedEventArgs(key));
        }

        private void Set(string key, int value)
        {
            if (value == 0)
            {
                this._store.Elements(key).Remove<XElement>();
            }
            else
            {
                this._store.SetElementValue(key, value);
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs(key));
        }

        private void Set(string key, string value)
        {
            if ((value == null) || (value.Trim().Length == 0))
            {
                this._store.Elements(key).Remove<XElement>();
            }
            else
            {
                this._store.SetElementValue(key, value);
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs(key));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _serialCount++;
            info.AddValue("data", this.GetStore().ToString());
            IDictionary<string, object> dictionary = this._sessionData;
            if ((dictionary != null) && (dictionary.Count == 0))
            {
                dictionary = null;
            }
            info.AddValue("sessionData", dictionary);
        }

        public override string ToString()
        {
            return this.GetFriendlyName(FriendlyNameMode.Standalone);
        }

        public void UpdateFromParent()
        {
            if (this.Parent != null)
            {
                this.Provider = this.Parent.Provider;
                this.Server = this.Parent.Server;
                this.AttachFile = this.Parent.AttachFile;
                this.UserInstance = this.Parent.UserInstance;
                this.AttachFileName = this.Parent.AttachFileName;
                this.SqlSecurity = this.Parent.SqlSecurity;
                this.UserName = this.Parent.UserName;
                this.Password = this.Parent.Password;
                this.NoPluralization = this.Parent.NoPluralization;
                this.NoCapitalization = this.Parent.NoCapitalization;
                this.ExcludeRoutines = this.Parent.ExcludeRoutines;
                this.CustomCxString = this.Parent.CustomCxString;
                this.MaxDatabaseSize = this.Parent.MaxDatabaseSize;
                this.DisplayName = this.Parent.DisplayName;
                this.IncludeSystemObjects = this.Parent.IncludeSystemObjects;
                this.EncryptTraffic = this.Parent.EncryptTraffic;
            }
        }

        public string AppConfigPath
        {
            get
            {
                return this.GetString("AppConfigPath");
            }
            set
            {
                this.Set("AppConfigPath", value);
            }
        }

        public bool AttachFile
        {
            get
            {
                return this.GetBool("AttachFile");
            }
            set
            {
                this.Set("AttachFile", value);
            }
        }

        public string AttachFileName
        {
            get
            {
                return PathHelper.DecodeFolder(this.GetString("AttachFileName"));
            }
            set
            {
                this.Set("AttachFileName", PathHelper.EncodeFolder(value));
            }
        }

        public string CustomAssemblyPath
        {
            get
            {
                string str = PathHelper.DecodeFolder(this.GetString("CustomAssemblyPathEncoded"));
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }
                return this.GetString("CustomAssemblyPath");
            }
            set
            {
                string str = PathHelper.EncodeFolder(value);
                this.Set("CustomAssemblyPathEncoded", (str == value) ? null : str);
                this.Set("CustomAssemblyPath", value);
            }
        }

        public string CustomCxString
        {
            get
            {
                string s = this.GetString("CustomCxString");
                if (this.EncryptCustomCxString)
                {
                    s = this.Decrypt(s);
                }
                return s;
            }
            set
            {
                string s = value;
                if (this.EncryptCustomCxString)
                {
                    s = this.Encrypt(s);
                }
                this.Set("CustomCxString", s);
            }
        }

        public string CustomMetadataPath
        {
            get
            {
                string str = PathHelper.DecodeFolder(this.GetString("CustomMetadataPathEncoded"));
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }
                return this.GetString("CustomMetadataPath");
            }
            set
            {
                string str = PathHelper.EncodeFolder(value);
                this.Set("CustomMetadataPathEncoded", (str == value) ? null : str);
                this.Set("CustomMetadataPath", value);
            }
        }

        public ICustomTypeInfo CustomTypeInfo
        {
            get
            {
                return this;
            }
        }

        public string CustomTypeName
        {
            get
            {
                return this.GetString("CustomTypeName");
            }
            set
            {
                this.Set("CustomTypeName", value);
            }
        }

        public string Database
        {
            get
            {
                return this.GetString("Database");
            }
            set
            {
                this.Set("Database", value);
            }
        }

        public IDatabaseInfo DatabaseInfo
        {
            get
            {
                return this;
            }
        }

        public string DbVersion
        {
            get
            {
                return this.GetString("DbVersion");
            }
            set
            {
                this.Set("DbVersion", value);
            }
        }

        public string DisplayName
        {
            get
            {
                return this.GetString("DisplayName");
            }
            set
            {
                this.Set("DisplayName", value);
            }
        }

        public XElement DriverData { get; set; }

        public DCDriverLoader DriverLoader
        {
            get
            {
                return this._driverLoader;
            }
            set
            {
                if (!object.Equals(value, this._driverLoader))
                {
                    this._driverLoader = value;
                    if (this._store.Element("Driver") != null)
                    {
                        this._store.Element("Driver").Remove();
                    }
                    XElement content = value.ToXElement();
                    if (content != null)
                    {
                        this._store.Add(content);
                    }
                    this.OnPropertyChanged(new PropertyChangedEventArgs("DriverLoader"));
                }
            }
        }

        public bool DynamicSchema
        {
            get
            {
                return (this._driverLoader.IsValid && (this._driverLoader.Driver is DynamicDataContextDriver));
            }
        }

        public IDynamicSchemaOptions DynamicSchemaOptions
        {
            get
            {
                return this;
            }
        }

        public bool EncryptCustomCxString
        {
            get
            {
                return this.GetBool("EncryptCustomCxString");
            }
            set
            {
                bool encryptCustomCxString = this.EncryptCustomCxString;
                if (value != encryptCustomCxString)
                {
                    string customCxString = this.CustomCxString;
                    this.Set("EncryptCustomCxString", value);
                    this.CustomCxString = customCxString;
                }
            }
        }

        public bool EncryptTraffic
        {
            get
            {
                return this.GetBool("EncryptTraffic");
            }
            set
            {
                this.Set("EncryptTraffic", value);
            }
        }

        public bool ExcludeRoutines
        {
            get
            {
                return this.GetBool("ExcludeRoutines");
            }
            set
            {
                this.Set("ExcludeRoutines", value);
            }
        }

        private static string FilePath
        {
            get
            {
                return Path.Combine(Program.UserDataFolder, "ConnectionsV2.xml");
            }
        }

        public Guid ID
        {
            get
            {
                return this.GetGuid("ID");
            }
            set
            {
                this.Set("ID", value);
            }
        }

        public bool IncludeSystemObjects
        {
            get
            {
                return this.GetBool("IncludeSystemObjects");
            }
            set
            {
                this.Set("IncludeSystemObjects", value);
            }
        }

        public bool IsAzure
        {
            get
            {
                return this.DbVersion.StartsWith("Azure", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public bool IsQueryable
        {
            get
            {
                return ((this._driverLoader.InternalID == null) || this._driverLoader.Driver.IsQueryable(this));
            }
        }

        public bool IsSqlCE
        {
            get
            {
                return ProviderNames.IsSqlCE(this.Provider);
            }
        }

        public bool IsSqlCE35
        {
            get
            {
                return ProviderNames.IsSqlCE35(this.Provider);
            }
        }

        public bool IsSqlCE40
        {
            get
            {
                return ProviderNames.IsSqlCE40(this.Provider);
            }
        }

        public bool IsSqlServer
        {
            get
            {
                return ProviderNames.IsSqlServer(this.Provider);
            }
        }

        public IEnumerable<LinkedDatabase> LinkedDatabases
        {
            get
            {
                return this._linkedDbs;
            }
            set
            {
                this._linkedDbs = (value ?? Enumerable.Empty<LinkedDatabase>()).ToList<LinkedDatabase>();
            }
        }

        public int MaxDatabaseSize
        {
            get
            {
                return this.GetInt("MaxDatabaseSize");
            }
            set
            {
                this.Set("MaxDatabaseSize", value);
            }
        }

        public bool NoCapitalization
        {
            get
            {
                return this.GetBool("NoCapitalization");
            }
            set
            {
                this.Set("NoCapitalization", value);
            }
        }

        public bool NoPluralization
        {
            get
            {
                return this.GetBool("NoPluralization");
            }
            set
            {
                this.Set("NoPluralization", value);
            }
        }

        private static string OldFilePath
        {
            get
            {
                return Path.Combine(Program.UserDataFolder, "connections.xml");
            }
        }

        public Repository Parent { get; private set; }

        public string Password
        {
            get
            {
                return (this._password ?? "");
            }
            set
            {
                string s = (value == null) ? null : value.Trim();
                if (!(this._password == s))
                {
                    this._password = s;
                    this.Set("Password", this.Encrypt(s));
                }
            }
        }

        public bool Persist
        {
            get
            {
                return this.GetBool("Persist");
            }
            set
            {
                this.Set("Persist", value);
            }
        }

        public string Provider
        {
            get
            {
                string str = this.GetString("Provider");
                switch (str)
                {
                    case "SQLCE":
                        str = "System.Data.SqlServerCe.3.5";
                        break;

                    case "":
                        str = "System.Data.SqlClient";
                        break;
                }
                return str;
            }
            set
            {
                if (string.Equals(value, "System.Data.SqlClient", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Set("Provider", "");
                }
                else
                {
                    this.Set("Provider", value);
                }
            }
        }

        public string Server
        {
            get
            {
                return this.GetString("Server");
            }
            set
            {
                this.Set("Server", value);
            }
        }

        public IDictionary<string, object> SessionData
        {
            get
            {
                if (this._sessionData == null)
                {
                    this._sessionData = new Dictionary<string, object>();
                }
                return this._sessionData;
            }
            internal set
            {
                this._sessionData = value;
            }
        }

        public bool ShowServer
        {
            get
            {
                return this.GetBool("ShowServer");
            }
            set
            {
                this.Set("ShowServer", value);
            }
        }

        public bool SqlSecurity
        {
            get
            {
                return this.GetBool("SqlSecurity");
            }
            set
            {
                this.Set("SqlSecurity", value);
            }
        }

        public bool UserInstance
        {
            get
            {
                return this.GetBool("UserInstance");
            }
            set
            {
                this.Set("UserInstance", value);
            }
        }

        public string UserName
        {
            get
            {
                return this.GetString("UserName");
            }
            set
            {
                this.Set("UserName", value);
            }
        }

        public enum FriendlyNameMode
        {
            Normal,
            Short,
            Standalone,
            FullTooltip
        }
    }
}

