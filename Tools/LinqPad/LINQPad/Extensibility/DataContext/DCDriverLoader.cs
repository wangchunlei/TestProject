namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Xml.Linq;

    [Serializable]
    internal class DCDriverLoader
    {
        private bool? _allowAssemblyShadowing;
        [NonSerialized]
        private XElement _config;
        [NonSerialized]
        private DataContextDriver _driver;
        private static Dictionary<string, AppDomain> _driverDomains = new Dictionary<string, AppDomain>();
        [NonSerialized]
        private Exception _lastLoadError;
        [NonSerialized]
        private DateTimeOffset? _resetError;
        public static DCDriverLoader DefaultLoader = new DCDriverLoader("");
        public readonly string FullTypeName;
        public readonly string InternalID;
        public readonly string PublicKeyToken;
        public readonly string SimpleAssemblyName;
        public static string ThirdPartyDriverFolder = Path.Combine(Program.AppDataFolder, @"Drivers\DataContext\4.0\");

        public DCDriverLoader(string id)
        {
            this.InternalID = id;
        }

        public DCDriverLoader(string simpleAssemblyName, string publicKeyToken, string fullTypeName)
        {
            if (string.IsNullOrEmpty(simpleAssemblyName))
            {
                throw new ArgumentException("simpleAssemblyName null or empty");
            }
            if (string.IsNullOrEmpty(publicKeyToken))
            {
                throw new ArgumentException("publicKeyToken null or empty");
            }
            if (string.IsNullOrEmpty(fullTypeName))
            {
                throw new ArgumentException("fullTypeName null or empty");
            }
            this.SimpleAssemblyName = simpleAssemblyName;
            this.PublicKeyToken = publicKeyToken;
            this.FullTypeName = fullTypeName;
        }

        public void ClearDriverDomain()
        {
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                string key = this.SimpleAssemblyName + " (" + this.PublicKeyToken + ")";
                if (_driverDomains.ContainsKey(key))
                {
                    Util.UnloadAppDomain(_driverDomains[key]);
                    _driverDomains.Remove(key);
                }
            }
        }

        public AppDomain CreateNewDriverDomain(string name, string baseFolderOverride, string configPath)
        {
            string appBase = !string.IsNullOrEmpty(baseFolderOverride) ? baseFolderOverride : ((this.InternalID == null) ? this.GetAssemblyFolder() : null);
            return AppDomainUtil.CreateDomain(name, configPath, appBase);
        }

        public override bool Equals(object obj)
        {
            DCDriverLoader loader = obj as DCDriverLoader;
            if (loader == null)
            {
                return false;
            }
            return (this.GetDescriptor() == loader.GetDescriptor());
        }

        public static DCDriverLoader FromXElement(XElement connectionDriverNode)
        {
            string str = (string) connectionDriverNode;
            if (string.IsNullOrEmpty(str))
            {
                return DefaultLoader;
            }
            string str2 = (string) connectionDriverNode.Attribute("Assembly");
            string publicKeyToken = (string) connectionDriverNode.Attribute("PublicKeyToken");
            if (string.IsNullOrEmpty(str2))
            {
                return new DCDriverLoader(str);
            }
            return new DCDriverLoader(str2, publicKeyToken, str);
        }

        public string GetAssemblyFolder()
        {
            if (this.InternalID != null)
            {
                return Path.GetDirectoryName(typeof(DCDriverLoader).Assembly.Location);
            }
            return (Path.Combine(ThirdPartyDriverFolder, this.SimpleAssemblyName) + " (" + this.PublicKeyToken + ")");
        }

        public string GetAssemblyPath()
        {
            if (this.InternalID != null)
            {
                return typeof(DCDriverLoader).Assembly.Location;
            }
            return (Path.Combine(this.GetAssemblyFolder(), this.SimpleAssemblyName) + ".dll");
        }

        public string GetDescriptor()
        {
            if (this.InternalID != null)
            {
                return this.InternalID;
            }
            return (this.SimpleAssemblyName.ToLowerInvariant() + ";" + this.PublicKeyToken.ToLowerInvariant() + ";" + this.FullTypeName);
        }

        private DataContextDriver GetDriver()
        {
            if (this.InternalID == null)
            {
                Assembly assembly;
                if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                {
                    string text1 = this.SimpleAssemblyName + " (" + this.PublicKeyToken + ")";
                    Instantiator instantiator = (Instantiator) this.GetDriverDomain().CreateInstanceFromAndUnwrap(typeof(Instantiator).Assembly.Location, typeof(Instantiator).FullName);
                    return instantiator.Instantiate(this.GetAssemblyPath(), this.FullTypeName);
                }
                if ((AppDomain.CurrentDomain.GetData("LINQPad.UseLoadFileForDCDriver") as bool?) == true)
                {
                    assembly = Assembly.LoadFile(this.GetAssemblyPath());
                }
                else
                {
                    assembly = Assembly.LoadFrom(this.GetAssemblyPath());
                }
                return (DataContextDriver) Activator.CreateInstance(assembly.GetType(this.FullTypeName));
            }
            if (this.InternalID.Length == 0)
            {
                return new LinqToSqlDynamicDriver();
            }
            if (this.InternalID.ToLowerInvariant() == "linqtosql")
            {
                return new LinqToSqlDriver();
            }
            if (this.InternalID.ToLowerInvariant() == "entityframework")
            {
                return new EntityFrameworkDriver();
            }
            if (this.InternalID.ToLowerInvariant() == "entityframeworkdbcontext")
            {
                return new EntityFrameworkDbContextDriver();
            }
            if (this.InternalID.ToLowerInvariant() == "astoriaauto")
            {
                return new AstoriaDynamicDriver();
            }
            if (this.InternalID.ToLowerInvariant() != "dallasauto")
            {
                throw new DisplayToUserException("Unknown DataContext driver '" + this.InternalID + "'");
            }
            return new DallasDynamicDriver();
        }

        private AppDomain GetDriverDomain()
        {
            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                return AppDomain.CurrentDomain;
            }
            string key = this.SimpleAssemblyName + " (" + this.PublicKeyToken + ")";
            if (_driverDomains.ContainsKey(key))
            {
                return _driverDomains[key];
            }
            return (_driverDomains[key] = this.CreateNewDriverDomain("DataContextDriver: " + key, null, null));
        }

        public override int GetHashCode()
        {
            return this.GetDescriptor().GetHashCode();
        }

        internal static string GetPublicKeyToken(AssemblyName name)
        {
            return string.Concat((from b in name.GetPublicKeyToken() select b.ToString("x2")).ToArray<string>());
        }

        internal string ShowConnectionDialog(string repositoryData, bool isNewRepository)
        {
            using (DomainIsolator isolator = new DomainIsolator(this.CreateNewDriverDomain("ShowConnectionDialog for " + this.SimpleAssemblyName, null, null)))
            {
                return isolator.GetInstance<Instantiator>().Instantiate(this.GetAssemblyPath(), this.FullTypeName).ShowConnectionDialog(repositoryData, isNewRepository);
            }
        }

        public static void Shutdown()
        {
            UnloadDomains(true);
        }

        public XElement ToXElement()
        {
            if (this.InternalID == null)
            {
                return new XElement("Driver", new object[] { new XAttribute("Assembly", this.SimpleAssemblyName), new XAttribute("PublicKeyToken", this.PublicKeyToken), this.FullTypeName });
            }
            return new XElement("Driver", this.InternalID);
        }

        public static void UnloadDomains(bool noThrow)
        {
            ManualResetEvent wh = new ManualResetEvent(false);
            new Thread(delegate {
                List<string> list = new List<string>();
                foreach (KeyValuePair<string, AppDomain> pair in _driverDomains)
                {
                    if (noThrow)
                    {
                        try
                        {
                            Util.UnloadAppDomain(pair.Value);
                        }
                        catch (Exception exception)
                        {
                            Log.Write(exception, "domain unload");
                        }
                    }
                    else
                    {
                        Util.UnloadAppDomain(pair.Value);
                    }
                    list.Add(pair.Key);
                }
                foreach (string str in list)
                {
                    _driverDomains.Remove(str);
                }
                wh.Set();
            }) { IsBackground = true, Name = "Domain Unloader" }.Start();
            wh.WaitOne(0x7d0, false);
        }

        public bool AllowAssemblyShadowing
        {
            get
            {
                if (!this._allowAssemblyShadowing.HasValue)
                {
                    try
                    {
                        bool? nullable = (bool?) this.Config.Element("AllowAssemblyShadowing");
                        this._allowAssemblyShadowing = new bool?(nullable.HasValue ? nullable.GetValueOrDefault() : true);
                    }
                    catch (Exception exception)
                    {
                        this._allowAssemblyShadowing = true;
                        Log.Write(exception, "Error reading DataContextDriver header.xml file for " + this.SimpleAssemblyName);
                    }
                }
                return this._allowAssemblyShadowing.Value;
            }
        }

        public XElement Config
        {
            get
            {
                if (this._config == null)
                {
                    this._config = new XElement("DataContextDriver");
                    if (this.IsInternalAuthor)
                    {
                        return this._config;
                    }
                    string path = Path.Combine(this.GetAssemblyFolder(), "header.xml");
                    if (!File.Exists(path))
                    {
                        return this._config;
                    }
                    try
                    {
                        this._config = XElement.Load(path);
                    }
                    catch (Exception exception)
                    {
                        Log.Write(exception, "Error reading DataContextDriver header.xml file for " + this.SimpleAssemblyName);
                    }
                }
                return this._config;
            }
        }

        public DataContextDriver Driver
        {
            get
            {
                DateTimeOffset now;
                DateTimeOffset? nullable;
                if (this._driver != null)
                {
                    try
                    {
                        string name = this._driver.Name;
                    }
                    catch (AppDomainUnloadedException)
                    {
                        this._driver = null;
                        this._lastLoadError = null;
                    }
                    if (this._driver != null)
                    {
                        return this._driver;
                    }
                }
                if (this._resetError.HasValue)
                {
                    now = DateTimeOffset.Now;
                    nullable = this._resetError;
                }
                if ((nullable.HasValue ? (now < nullable.GetValueOrDefault()) : false) && (this._lastLoadError != null))
                {
                    throw this._lastLoadError;
                }
                try
                {
                    this._driver = this.GetDriver();
                }
                catch (Exception exception)
                {
                    this._lastLoadError = exception;
                    this._resetError = new DateTimeOffset?(DateTimeOffset.Now.AddMilliseconds(100.0));
                    throw;
                }
                return this._driver;
            }
        }

        public bool IsInternalAuthor
        {
            get
            {
                return ((this.InternalID != null) || (this.SimpleAssemblyName.ToLowerInvariant() == "iqdriver"));
            }
        }

        public bool IsValid
        {
            get
            {
                if (this._driver == null)
                {
                    DateTimeOffset now;
                    DateTimeOffset? nullable;
                    if (this._resetError.HasValue)
                    {
                        now = DateTimeOffset.Now;
                        nullable = this._resetError;
                    }
                    if ((nullable.HasValue ? (now < nullable.GetValueOrDefault()) : false) && (this._lastLoadError != null))
                    {
                        return false;
                    }
                    try
                    {
                        if (!((this.InternalID != null) || File.Exists(this.GetAssemblyPath())))
                        {
                            return false;
                        }
                        this._driver = this.GetDriver();
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public Exception LoadError
        {
            get
            {
                return this._lastLoadError;
            }
        }

        public string SortOrder
        {
            get
            {
                if (this.InternalID != null)
                {
                    return ("1." + this.Driver.InternalSortOrder);
                }
                return ("2." + this.FullTypeName);
            }
        }

        private class Instantiator : MarshalByRefObject
        {
            public DataContextDriver Instantiate(string assemblyPath, string typeName)
            {
                return (DataContextDriver) Activator.CreateInstance(Assembly.LoadFrom(assemblyPath).GetType(typeName));
            }
        }
    }
}

