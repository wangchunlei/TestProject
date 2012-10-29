namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using LINQPad.ExecutionModel;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    public abstract class DataContextDriver : MarshalByRefObject
    {
        private string[] _assembliesToAdd;
        internal DCDriverLoader _loader;

        protected DataContextDriver()
        {
        }

        public virtual bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
        {
            if (!c1.DatabaseInfo.IsEquivalent(c2.DatabaseInfo))
            {
                return false;
            }
            return (this.IsBuiltIn || (c1.DriverData.ToString() == c2.DriverData.ToString()));
        }

        public virtual void ClearConnectionPools(IConnectionInfo cxInfo)
        {
        }

        public virtual void DisplayObjectInGrid(object objectToDisplay, GridOptions options)
        {
            ExplorerGrid.Display(objectToDisplay, options);
        }

        public virtual void ExecuteESqlQuery(IConnectionInfo cxInfo, string query)
        {
            throw new Exception("ESQL queries are not supported for this type of connection");
        }

        public static string FormatTypeName(Type t, bool includeNamespace)
        {
            return t.FormatTypeName(includeNamespace);
        }

        public virtual string GetAppConfigPath(IConnectionInfo cxInfo)
        {
            return cxInfo.AppConfigPath;
        }

        [Obsolete("Use the overload that accepts an IConnectionInfo instead", false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IEnumerable<string> GetAssembliesToAdd()
        {
            return new string[0];
        }

        public virtual IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            return this.GetAssembliesToAdd();
        }

        public abstract string GetConnectionDescription(IConnectionInfo cxInfo);
        internal virtual Type GetContextBaseType()
        {
            return typeof(object);
        }

        public virtual object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            return null;
        }

        public virtual ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            return null;
        }

        public virtual ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {
            return null;
        }

        public string GetDriverFolder()
        {
            return Path.GetDirectoryName(base.GetType().Assembly.Location);
        }

        internal virtual string GetFailedImageKey(IConnectionInfo cxInfo)
        {
            return "CustomError";
        }

        public virtual IDbConnection GetIDbConnection(IConnectionInfo cxInfo)
        {
            if (string.IsNullOrEmpty(cxInfo.DatabaseInfo.GetCxString()))
            {
                throw new DisplayToUserException("A valid database connection string could not be obtained.");
            }
            DbConnection connection = this.GetProviderFactory(cxInfo).CreateConnection();
            connection.ConnectionString = cxInfo.DatabaseInfo.GetCxString();
            return connection;
        }

        internal virtual string GetImageKey(IConnectionInfo cxInfo)
        {
            return "Custom";
        }

        [Obsolete("Use the overload that accepts an IConnectionInfo instead", false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IEnumerable<string> GetNamespacesToAdd()
        {
            return new string[0];
        }

        public virtual IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            return this.GetNamespacesToAdd();
        }

        internal string[] GetNamespacesToAddInternal(IConnectionInfo cxInfo)
        {
            return (this.GetNamespacesToAdd(cxInfo) ?? ((IEnumerable<string>) new string[0])).ToArray<string>();
        }

        [Obsolete("Use the overload that accepts an IConnectionInfo instead", false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IEnumerable<string> GetNamespacesToRemove()
        {
            return new string[0];
        }

        public virtual IEnumerable<string> GetNamespacesToRemove(IConnectionInfo cxInfo)
        {
            return this.GetNamespacesToRemove();
        }

        internal string[] GetNamespacesToRemoveInternal(IConnectionInfo cxInfo)
        {
            return (this.GetNamespacesToRemove(cxInfo) ?? ((IEnumerable<string>) new string[0])).ToArray<string>();
        }

        public virtual DbProviderFactory GetProviderFactory(IConnectionInfo cxInfo)
        {
            DbProviderFactory factory;
            try
            {
                factory = DbProviderFactories.GetFactory(cxInfo.DatabaseInfo.Provider);
            }
            catch (ArgumentException exception)
            {
                throw new DisplayToUserException(exception.Message, exception);
            }
            return factory;
        }

        public virtual void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public object InvokeCustomOption(string optionName, params object[] data)
        {
            return null;
        }

        internal virtual bool IsQueryable(Repository r)
        {
            return true;
        }

        public static Assembly LoadAssemblySafely(string fullFilePath)
        {
            if (Server.CurrentServer == null)
            {
                return Assembly.LoadFrom(fullFilePath);
            }
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(fullFilePath);
            return Server.CurrentServer.ShadowLoad(assemblyName.Name, assemblyName.Version, fullFilePath, null);
        }

        public virtual object OnCustomEvent(string eventName, params object[] data)
        {
            return null;
        }

        public virtual void OnQueryFinishing(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
        }

        public virtual void PreprocessObjectToWrite(ref object objectToWrite, ObjectGraphInfo info)
        {
        }

        public abstract bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection);
        internal string ShowConnectionDialog(string repositoryData, bool isNewRepository)
        {
            Repository cxInfo = new Repository(XElement.Parse(repositoryData));
            if (this.ShowConnectionDialog(cxInfo, isNewRepository))
            {
                return cxInfo.GetStore().ToString();
            }
            return null;
        }

        public virtual void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        {
        }

        internal string[] TryGetFullAssembliesToAdd(string staticDataContextFolder, IConnectionInfo cxInfo)
        {
            var selector = null;
            if (this.IsBuiltIn)
            {
                if (this._assembliesToAdd != null)
                {
                    return this._assembliesToAdd;
                }
                if (this.AssembliesToAddError != null)
                {
                    return null;
                }
            }
            try
            {
                IEnumerable<string> source = from a in this.GetAssembliesToAdd(cxInfo) ?? ((IEnumerable<string>) new string[0]) select a.Contains<char>(',') ? ((IEnumerable<string>) (GacResolver.FindPath(a) ?? (a.Split(new char[] { ',' }).First<string>() + ".dll"))) : ((IEnumerable<string>) a);
                string driverFolder = this.GetDriverFolder();
                if (!((!this.IsBuiltIn && !string.IsNullOrEmpty(driverFolder)) && Directory.Exists(driverFolder)))
                {
                    driverFolder = null;
                }
                if (selector == null)
                {
                    selector = a => new { a = a, fullPath1 = string.IsNullOrEmpty(staticDataContextFolder) ? "" : Path.Combine(staticDataContextFolder, a) };
                }
                this._assembliesToAdd = (from <>h__TransparentIdentifier0 in source.Select(selector)
                    let fullPath2 = string.IsNullOrEmpty(driverFolder) ? ((IEnumerable<string>) "") : ((IEnumerable<string>) Path.Combine(driverFolder, <>h__TransparentIdentifier0.a))
                    select ((<>h__TransparentIdentifier0.fullPath1 == "") || !File.Exists(<>h__TransparentIdentifier0.fullPath1)) ? (((fullPath2 == "") || !File.Exists(fullPath2)) ? ((IEnumerable<string>) <>h__TransparentIdentifier0.a) : ((IEnumerable<string>) fullPath2)) : ((IEnumerable<string>) <>h__TransparentIdentifier0.fullPath1)).Union<string>(new string[] { base.GetType().Assembly.Location }).ToArray<string>();
            }
            catch (Exception exception)
            {
                this.AssembliesToAddError = exception;
            }
            return this._assembliesToAdd;
        }

        internal Exception AssembliesToAddError { get; private set; }

        public abstract string Author { get; }

        internal virtual string ContextBaseTypeName
        {
            get
            {
                return "System.Object";
            }
        }

        public virtual bool DisallowQueryDisassembly
        {
            get
            {
                return false;
            }
        }

        internal virtual string InternalID
        {
            get
            {
                return null;
            }
        }

        internal virtual int InternalSortOrder
        {
            get
            {
                return 0;
            }
        }

        internal virtual bool IsBuiltIn
        {
            get
            {
                return false;
            }
        }

        internal DCDriverLoader Loader
        {
            get
            {
                if (this._loader == null)
                {
                    if (this.InternalID != null)
                    {
                        this._loader = new DCDriverLoader(this.InternalID);
                    }
                    else
                    {
                        AssemblyName name = base.GetType().Assembly.GetName();
                        this._loader = new DCDriverLoader(name.Name, DCDriverLoader.GetPublicKeyToken(name), base.GetType().FullName);
                    }
                }
                return this._loader;
            }
            set
            {
                this._loader = value;
            }
        }

        public abstract string Name { get; }

        internal virtual bool UsesDatabaseConnection
        {
            get
            {
                return true;
            }
        }

        public virtual System.Version Version
        {
            get
            {
                AssemblyFileVersionAttribute attribute = (AssemblyFileVersionAttribute) base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).FirstOrDefault<object>();
                if (attribute == null)
                {
                    return new System.Version("1.0.0.0");
                }
                return new System.Version(attribute.Version);
            }
        }
    }
}

