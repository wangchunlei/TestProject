namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using LINQPad.ExecutionModel;
    using LINQPad.Extensibility.Internal;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.EntityClient;
    using System.Data.Objects;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml.Linq;

    internal class EntityFrameworkDbContextDriver : StaticDataContextDriver
    {
        private static bool _cxFactoryPatched;
        private static Assembly _efAssembly;
        private static string _efDllPath;
        private static bool _efResolverInstalled;
        private static string _lastEfPath;
        private static string _lastEfPathGac;
        private static string _lastEfPathInput;

        public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
        {
            return ((c1.CustomTypeInfo.IsEquivalent(c2.CustomTypeInfo) && string.Equals(c1.AppConfigPath, c2.AppConfigPath, StringComparison.InvariantCultureIgnoreCase)) && string.Equals(c1.DatabaseInfo.CustomCxString, c2.DatabaseInfo.CustomCxString, StringComparison.InvariantCultureIgnoreCase));
        }

        private static void CheckEFAssemblyResolver(IConnectionInfo cxInfo)
        {
            try
            {
                string path = Path.Combine(Path.GetDirectoryName(cxInfo.CustomTypeInfo.CustomAssemblyPath), "EntityFramework.dll");
                if (File.Exists(path))
                {
                    _efDllPath = path;
                    InstallEFResolver();
                }
            }
            catch
            {
            }
        }

        private static void CheckForUnpatchedCx(object dbContext)
        {
            if (!(GetIDbConnection(dbContext) is LINQPadDbConnection))
            {
                LINQPadDbController.UnpatchProviderConfigTable();
            }
        }

        public override void ExecuteESqlQuery(IConnectionInfo cxInfo, string query)
        {
            EntityConnection connection;
            object dbContext = base.InstantiateBaseContext(cxInfo);
            CheckForUnpatchedCx(dbContext);
            string eSqlCxString = GetESqlCxString(dbContext);
            EntityConnectionStringBuilder builder = new EntityConnectionStringBuilder(eSqlCxString);
            string str2 = builder["metadata"] as string;
            if ((str2 != null) && str2.Trim().ToUpperInvariant().StartsWith("READER:"))
            {
                connection = new EntityConnection(GetObjectContext(dbContext).MetadataWorkspace, (DbConnection) GetIDbConnection(dbContext));
            }
            else
            {
                connection = new EntityConnection(eSqlCxString);
            }
            using (connection)
            {
                EntityCommand command = new EntityCommand(query, connection);
                connection.Open();
                command.ExecuteReader(CommandBehavior.SequentialAccess).Dump<EntityDataReader>();
            }
        }

        public override string GetAppConfigPath(IConnectionInfo cxInfo)
        {
            Func<XElement, bool> predicate = null;
            if (!((!string.IsNullOrWhiteSpace(cxInfo.AppConfigPath) && !string.IsNullOrWhiteSpace(cxInfo.CustomTypeInfo.CustomTypeName)) && File.Exists(cxInfo.AppConfigPath)))
            {
                return base.GetAppConfigPath(cxInfo);
            }
            string typeName = cxInfo.CustomTypeInfo.CustomTypeName.Split(new char[] { '.' }).Last<string>();
            XElement element = null;
            try
            {
                element = XElement.Load(cxInfo.AppConfigPath);
                IEnumerable<XElement> source = from x in element.Elements("connectionStrings") select x.Elements("add");
                if (source.Any<XElement>(cx => ((string) cx.Attribute("name")) == "UserQuery"))
                {
                    return base.GetAppConfigPath(cxInfo);
                }
                if (predicate == null)
                {
                    predicate = x => ((string) x.Attribute("name")) == typeName;
                }
                XElement content = source.FirstOrDefault<XElement>(predicate);
                if (content == null)
                {
                    return base.GetAppConfigPath(cxInfo);
                }
                content.AddAfterSelf(content);
                ((XElement) content.NextNode).Attribute("name").SetValue("UserQuery");
            }
            catch
            {
                return base.GetAppConfigPath(cxInfo);
            }
            string str2 = string.Concat((IEnumerable<string>) (from b in SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(cxInfo.AppConfigPath)).Take<byte>(8) select b.ToString("X2"))) + ".config";
            string fileName = Path.Combine(Program.TempFolder, str2);
            element.Save(fileName);
            return fileName;
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            string efPath = GetEfPath(cxInfo, false);
            if (efPath != null)
            {
                return new string[] { efPath };
            }
            return new string[] { "EntityFramework, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" };
        }

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return (cxInfo.CustomTypeInfo.CustomTypeName.Split(new char[] { '.' }).Last<string>() + " in " + cxInfo.CustomTypeInfo.CustomAssemblyPath.Split(new char[] { '\\' }).Last<string>());
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            if (string.IsNullOrEmpty(cxInfo.DatabaseInfo.CustomCxString))
            {
                return new object[0];
            }
            return new object[] { cxInfo.DatabaseInfo.CustomCxString };
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            if (string.IsNullOrEmpty(cxInfo.DatabaseInfo.CustomCxString))
            {
                return new ParameterDescriptor[0];
            }
            return new ParameterDescriptor[] { new ParameterDescriptor("param", "System.String") };
        }

        public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {
            if (objectToWrite == null)
            {
                return null;
            }
            if (!EntityFrameworkMemberProvider.IsEntity(objectToWrite.GetType()))
            {
                return null;
            }
            return new EntityFrameworkMemberProvider(objectToWrite);
        }

        private static Assembly GetEfAssembly(IConnectionInfo cxInfo)
        {
            if (_efAssembly != null)
            {
                return _efAssembly;
            }
            _efAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault<Assembly>(a => a.FullName.StartsWith("entityframework,", StringComparison.InvariantCultureIgnoreCase));
            if (_efAssembly != null)
            {
                return _efAssembly;
            }
            string efPath = GetEfPath(cxInfo, false);
            if (efPath == null)
            {
                return null;
            }
            if (efPath.Contains<char>(','))
            {
                return (_efAssembly = Assembly.Load(efPath));
            }
            return (_efAssembly = DataContextDriver.LoadAssemblySafely(efPath));
        }

        private static string GetEfPath(IConnectionInfo cxInfo, bool convertFullNameToGacPath)
        {
            string efVersion;
            string customAssemblyPath = cxInfo.CustomTypeInfo.CustomAssemblyPath;
            if (string.IsNullOrEmpty(customAssemblyPath))
            {
                return null;
            }
            try
            {
                string path = Path.Combine(Path.GetDirectoryName(customAssemblyPath), "EntityFramework.dll");
                if (File.Exists(path))
                {
                    return path;
                }
            }
            catch (ArgumentException)
            {
            }
            if (customAssemblyPath == _lastEfPathInput)
            {
                return (convertFullNameToGacPath ? _lastEfPathGac : _lastEfPath);
            }
            _lastEfPath = null;
            string shortName = Path.GetFileNameWithoutExtension(customAssemblyPath);
            Assembly customAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault<Assembly>(a => a.GetName().Name.Equals(shortName, StringComparison.InvariantCultureIgnoreCase));
            if (customAssembly != null)
            {
                efVersion = new EFVersionProber().GetEfVersion(customAssembly);
            }
            else
            {
                using (DomainIsolator isolator = new DomainIsolator("Probe EF version"))
                {
                    efVersion = isolator.GetInstance<EFVersionProber>().GetEfVersion(customAssemblyPath);
                }
            }
            _lastEfPath = efVersion;
            string str5 = _lastEfPathGac = GacResolver.FindPath(efVersion);
            _lastEfPathInput = customAssemblyPath;
            return (convertFullNameToGacPath ? str5 : efVersion);
        }

        private static string GetESqlCxString(object context)
        {
            return GetObjectContext(context).Connection.ConnectionString;
        }

        public override IDbConnection GetIDbConnection(IConnectionInfo cxInfo)
        {
            return GetIDbConnection(base.InstantiateBaseContext(cxInfo));
        }

        private static IDbConnection GetIDbConnection(object dbContext)
        {
            object obj2 = dbContext.GetType().GetProperty("Database").GetValue(dbContext, null);
            return (IDbConnection) obj2.GetType().GetProperty("Connection").GetValue(obj2, null);
        }

        internal override string GetImageKey(IConnectionInfo r)
        {
            return "EF";
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            return new string[] { "System.Data.Entity", "System.Data.Entity.Infrastructure", "System.Data.Entity.Validation", "System.Data.EntityClient", "System.Data.Metadata.Edm", "System.Data.Objects", "System.Data.Objects.DataClasses" };
        }

        public override IEnumerable<string> GetNamespacesToRemove(IConnectionInfo cxInfo)
        {
            return new string[] { "System.Data.Linq", "System.Data.Linq.SqlClient" };
        }

        private static ObjectContext GetObjectContext(object context)
        {
            ObjectContext context2;
            if (context == null)
            {
                return null;
            }
            Type type = context.GetType().GetInterface("System.Data.Entity.Infrastructure.IObjectContextAdapter");
            try
            {
                context2 = (ObjectContext) type.GetProperty("ObjectContext").GetValue(context, null);
            }
            catch (Exception exception)
            {
                throw GetRealException(exception);
            }
            return context2;
        }

        public override DbProviderFactory GetProviderFactory(IConnectionInfo cxInfo)
        {
            DbConnection iDbConnection = GetIDbConnection(cxInfo) as DbConnection;
            if (iDbConnection == null)
            {
                return null;
            }
            return DbProviderServices.GetProviderFactory(iDbConnection);
        }

        public static Exception GetRealException(Exception possiblyDistractingException)
        {
            Exception innerException = possiblyDistractingException;
            while (innerException.InnerException != null)
            {
                if (innerException is TargetInvocationException)
                {
                    innerException = innerException.InnerException;
                }
                else
                {
                    if (!(innerException is ProviderIncompatibleException) || !(GetRealException(innerException.InnerException) is DbException))
                    {
                        return innerException;
                    }
                    innerException = innerException.InnerException;
                }
            }
            return innerException;
        }

        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type t)
        {
            CheckEFAssemblyResolver(cxInfo);
            object[] contextConstructorArguments = this.GetContextConstructorArguments(cxInfo);
            return EntityFrameworkEdmReader.GetSchema(GetObjectContext(Activator.CreateInstance(t, contextConstructorArguments)));
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            CheckForUnpatchedCx(context);
        }

        private static void InstallEFResolver()
        {
            if (!_efResolverInstalled)
            {
                _efResolverInstalled = true;
                AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args) {
                    if (!(((args.Name == "EntityFramework, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") && !string.IsNullOrEmpty(_efDllPath)) && File.Exists(_efDllPath)))
                    {
                        return null;
                    }
                    return Assembly.LoadFrom(_efDllPath);
                };
            }
        }

        public static void PatchCxFactory(IConnectionInfo cxInfo)
        {
            if (!_cxFactoryPatched)
            {
                Assembly efAssembly = GetEfAssembly(cxInfo);
                if (efAssembly != null)
                {
                    Type interfaceType = efAssembly.GetType("System.Data.Entity.Infrastructure.IDbConnectionFactory");
                    if (interfaceType != null)
                    {
                        Type type = efAssembly.GetType("System.Data.Entity.Database");
                        if (type != null)
                        {
                            PropertyInfo property = type.GetProperty("DefaultConnectionFactory", BindingFlags.Public | BindingFlags.Static);
                            if (property != null)
                            {
                                object obj2 = property.GetValue(null, null);
                                _cxFactoryPatched = true;
                                if ((obj2 == null) || !(obj2.GetType().Name == "LINQPadDbConnectionFactory"))
                                {
                                    AppDomain currentDomain = AppDomain.CurrentDomain;
                                    AssemblyName name = new AssemblyName("LINQPad.EntityFrameworkBridge");
                                    TypeBuilder builder3 = currentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule("MainModule").DefineType("LINQPadDbConnectionFactory", TypeAttributes.Public);
                                    builder3.AddInterfaceImplementation(interfaceType);
                                    FieldBuilder field = builder3.DefineField("_innerFactory", interfaceType, FieldAttributes.Private);
                                    ILGenerator iLGenerator = builder3.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { interfaceType }).GetILGenerator();
                                    iLGenerator.Emit(OpCodes.Ldarg_0);
                                    iLGenerator.Emit(OpCodes.Ldarg_1);
                                    iLGenerator.Emit(OpCodes.Stfld, field);
                                    iLGenerator.Emit(OpCodes.Ret);
                                    iLGenerator = builder3.DefineMethod("CreateConnection", MethodAttributes.Virtual | MethodAttributes.Public, typeof(DbConnection), new Type[] { typeof(string) }).GetILGenerator();
                                    iLGenerator.Emit(OpCodes.Ldarg_1);
                                    iLGenerator.Emit(OpCodes.Ldarg_0);
                                    iLGenerator.Emit(OpCodes.Ldfld, field);
                                    iLGenerator.Emit(OpCodes.Call, typeof(EntityFrameworkFactoryConnectionHelper).GetMethod("CreateFactoryConnection", BindingFlags.Public | BindingFlags.Static));
                                    iLGenerator.Emit(OpCodes.Ret);
                                    object obj3 = Activator.CreateInstance(builder3.CreateType(), new object[] { obj2 });
                                    property.SetValue(null, obj3, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override bool ShowConnectionDialog(IConnectionInfo repository, bool isNewRepository)
        {
            using (DbContextCxForm form = new DbContextCxForm(repository))
            {
                return (form.ShowDialog() == DialogResult.OK);
            }
        }

        public void Test(IConnectionInfo cxInfo)
        {
            CheckEFAssemblyResolver(cxInfo);
            GetObjectContext(base.InstantiateBaseContext(cxInfo));
        }

        public override string Author
        {
            get
            {
                return "(built in)";
            }
        }

        internal override string InternalID
        {
            get
            {
                return "EntityFrameworkDbContext";
            }
        }

        internal override int InternalSortOrder
        {
            get
            {
                return 20;
            }
        }

        internal override bool IsBuiltIn
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "Entity Framework DbContext POCO (4.1/4.2/4.3)";
            }
        }

        private class EFVersionProber : MarshalByRefObject
        {
            public string GetEfVersion(Assembly customAssembly)
            {
                AssemblyName name = customAssembly.GetReferencedAssemblies().FirstOrDefault<AssemblyName>(a => a.Name.Equals("entityframework", StringComparison.InvariantCultureIgnoreCase));
                return ((name == null) ? null : name.FullName);
            }

            public string GetEfVersion(string customAssemPath)
            {
                return this.GetEfVersion(Assembly.ReflectionOnlyLoadFrom(customAssemPath));
            }
        }
    }
}

