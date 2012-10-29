namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    internal class ThirdPartyDriverProber : MarshalByRefObject
    {
        private List<DCDriverInfo> _drivers = new List<DCDriverInfo>();

        public static DCDriverInfo[] GetDriversInAssembly(string assemblyPath)
        {
            using (DomainIsolator isolator = new DomainIsolator("DataContextDriver prober"))
            {
                return isolator.GetInstance<ThirdPartyDriverProber>().GetDriversInAssemblyInternal(assemblyPath);
            }
        }

        private DCDriverInfo[] GetDriversInAssemblyInternal(Assembly assem)
        {
            string name = assem.GetName().Name;
            string pkToken = DCDriverLoader.GetPublicKeyToken(assem.GetName());
            return (from t in assem.GetExportedTypes()
                where !t.IsAbstract && typeof(DataContextDriver).IsAssignableFrom(t)
                let driver = InstantiateDCDriver(t)
                let loader = new DCDriverLoader(name, pkToken, t.FullName)
                select new DCDriverInfo { Loader = loader, Name = driver.Name, Author = driver.Author, Version = driver.Version.ToString(), IsAuto = driver is DynamicDataContextDriver }).ToArray<DCDriverInfo>();
        }

        private DCDriverInfo[] GetDriversInAssemblyInternal(string assemblyPath)
        {
            return this.GetDriversInAssemblyInternal(Assembly.LoadFrom(assemblyPath));
        }

        public static DCDriverInfo[] GetThirdPartyDrivers()
        {
            using (DomainIsolator isolator = new DomainIsolator("DataContextDriver prober"))
            {
                return isolator.GetInstance<ThirdPartyDriverProber>().GetThirdPartyDriversInternal();
            }
        }

        private DCDriverInfo[] GetThirdPartyDriversInternal()
        {
            this._drivers.Clear();
            DirectoryInfo info = new DirectoryInfo(DCDriverLoader.ThirdPartyDriverFolder);
            if (info.Exists)
            {
                DirectoryInfo[] directories = info.GetDirectories();
                int index = 0;
                while (true)
                {
                    if (index >= directories.Length)
                    {
                        break;
                    }
                    DirectoryInfo info2 = directories[index];
                    string str = info2.Name.Trim();
                    string pkToken = "";
                    if (!Regex.IsMatch(info2.Name, @".+\(.+\)$"))
                    {
                        Log.Write("Skipped badly named driver directory '" + info2.Name + "' - directory must be named 'assemblyName (public-key-token)'");
                    }
                    else
                    {
                        pkToken = str.Substring(str.IndexOf('(') + 1).Trim();
                        pkToken = pkToken.Substring(0, pkToken.Length - 1).Trim();
                        string path = Path.Combine(info2.FullName, str.Substring(0, str.IndexOf('(')).Trim() + ".dll");
                        if (!File.Exists(path))
                        {
                            Log.Write("Error loading driver: file '" + path + "' does not exist");
                        }
                        else
                        {
                            try
                            {
                                this.ProbeAssembly(path, pkToken);
                            }
                            catch (Exception exception)
                            {
                                Log.Write(exception, "Error reading driver assembly '" + path + "'");
                            }
                        }
                    }
                    index++;
                }
            }
            return this._drivers.ToArray();
        }

        private static DataContextDriver InstantiateDCDriver(Type t)
        {
            DataContextDriver driver;
            try
            {
                driver = (DataContextDriver) Activator.CreateInstance(t);
            }
            catch (Exception innerException)
            {
                if (innerException is TargetInvocationException)
                {
                    innerException = innerException.InnerException;
                }
                throw new Exception("Error instantiating type '" + t.FullName + "' - " + innerException.Message, innerException);
            }
            return driver;
        }

        private void ProbeAssembly(string targetFile, string pkToken)
        {
            Assembly assem = Assembly.LoadFrom(targetFile);
            string publicKeyToken = DCDriverLoader.GetPublicKeyToken(assem.GetName());
            if (publicKeyToken.Length == 0)
            {
                Log.Write("Error loading driver '" + targetFile + "' - assembly is not strongly named.");
            }
            else if (pkToken.ToLowerInvariant() != publicKeyToken.ToLowerInvariant())
            {
                Log.Write("Error loading driver '" + targetFile + "' - bad directory name. Directory should be named: '" + Path.GetFileNameWithoutExtension(targetFile) + " (" + publicKeyToken + ")'");
            }
            else
            {
                this._drivers.AddRange(this.GetDriversInAssemblyInternal(assem));
            }
        }
    }
}

