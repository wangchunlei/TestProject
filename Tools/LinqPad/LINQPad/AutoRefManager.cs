namespace LINQPad
{
    using ActiproBridge;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    internal class AutoRefManager
    {
        private static string _cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"LINQPad\AutoCompletionCache" + ((Environment.Version.Major == 4) ? "40" : "") + @"\namespaces.dat");
        private static volatile ILookup<string, string> _caseLookup;
        private static volatile Dictionary<string, string[]> _refLookup;
        private static string _runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
        private static readonly string ExtraRefs = "PresentationCore\r\nPresentationFramework\r\nPresentationFramework.Aero\r\nPresentationFramework.Classic\r\nPresentationFramework.Luna\r\nPresentationFramework.Royale\r\nPresentationUI\r\nReachFramework\r\nSystem.Activities\r\nSystem.Activities.Core.Presentation\r\nSystem.Activities.DurableInstancing\r\nSystem.Activities.Presentation\r\nSystem.AddIn.Contract\r\nSystem.AddIn\r\nSystem.ComponentModel.Composition\r\nSystem.ComponentModel.DataAnnotations\r\nSystem.configuration\r\nSystem.Data.Services.Client\r\nSystem.Data.Services\r\nSystem.DirectoryServices.AccountManagement\r\nSystem.DirectoryServices\r\nSystem.DirectoryServices.Protocols\r\nSystem.Dynamic\r\nSystem.EnterpriseServices\r\nSystem.IdentityModel\r\nSystem.IdentityModel.Selectors\r\nSystem.Management\r\nSystem.Management.Instrumentation\r\nSystem.Messaging\r\nSystem.Net\r\nSystem.Numerics\r\nSystem.Printing\r\nSystem.Runtime.Caching\r\nSystem.Runtime.DurableInstancing\r\nSystem.Runtime.Remoting\r\nSystem.Runtime.Serialization\r\nSystem.Runtime.Serialization.Formatters.Soap\r\nSystem.Security\r\nSystem.ServiceModel\r\nSystem.ServiceModel.Activation\r\nSystem.ServiceModel.Activities\r\nSystem.ServiceModel.Channels\r\nSystem.ServiceModel.Discovery\r\nSystem.ServiceModel.Routing\r\nSystem.ServiceModel.Web\r\nSystem.ServiceProcess\r\nSystem.Speech\r\nSystem.Web\r\nSystem.Web.ApplicationServices\r\nSystem.Web.DataVisualization\r\nSystem.Web.DynamicData\r\nSystem.Web.Entity.Design\r\nSystem.Web.Entity\r\nSystem.Web.Extensions\r\nSystem.Web.Mobile\r\nSystem.Web.Mvc\r\nSystem.Web.RegularExpressions\r\nSystem.Web.Services\r\nSystem.Windows.Forms\r\nSystem.Windows.Forms.DataVisualization\r\nSystem.Windows.Input.Manipulations\r\nSystem.Windows.Presentation\r\nSystem.Workflow.Activities\r\nSystem.Workflow.ComponentModel\r\nSystem.Workflow.Runtime\r\nSystem.Xaml\r\nUIAutomationClient\r\nUIAutomationClientsideProviders\r\nUIAutomationProvider\r\nUIAutomationTypes\r\nWindowsBase\r\nWindowsFormsIntegration";

        private static void CreateCache(object delay)
        {
            if (delay is int)
            {
                Thread.Sleep((int) delay);
            }
            try
            {
                if (_refLookup == null)
                {
                    using (DomainIsolator isolator = new DomainIsolator("AutoRef Type Populator"))
                    {
                        isolator.Domain.DoCallBack(new CrossAppDomainDelegate(Program.AddLINQPadAssemblyResolver));
                        isolator.Domain.DoCallBack(new CrossAppDomainDelegate(AutoRefManager.Populate));
                    }
                    PopulateFromCache();
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception);
            }
        }

        private static string FindFullPath(string file)
        {
            file = file.Trim();
            file = file + ".dll";
            string path = Path.Combine(_runtimeDir, file);
            if (File.Exists(path))
            {
                return path;
            }
            if (Environment.Version.Major == 2)
            {
                string str3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Reference Assemblies\Microsoft\Framework");
                path = Path.Combine(str3, @"v3.0\" + file);
                if (File.Exists(path))
                {
                    return path;
                }
                path = Path.Combine(str3, @"v3.5\" + file);
                if (File.Exists(path))
                {
                    return path;
                }
                return file;
            }
            path = Path.Combine(_runtimeDir, @"WPF\" + file);
            if (File.Exists(path))
            {
                return path;
            }
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft ASP.NET\ASP.NET MVC 3\Assemblies\" + file);
            if (File.Exists(path))
            {
                return path;
            }
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft ASP.NET\ASP.NET MVC 2\Assemblies\" + file);
            if (File.Exists(path))
            {
                return path;
            }
            return file;
        }

        private static Assembly GetAssembly(string name)
        {
            try
            {
                string path = FindFullPath(name);
                if (!File.Exists(path))
                {
                    return null;
                }
                return Assembly.LoadFrom(path);
            }
            catch
            {
                return null;
            }
        }

        private static Type[] GetExportedTypes(Assembly a)
        {
            try
            {
                return a.GetExportedTypes();
            }
            catch
            {
                return null;
            }
        }

        public static void Initialize(int delay)
        {
            TypeResolver.WarmupEngine(MyExtensions.AdditionalRefs);
            if ((_refLookup == null) && !PopulateFromCache())
            {
                new Thread(new ParameterizedThreadStart(AutoRefManager.CreateCache), delay) { Name = "AutoRef Type Populator", IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
            }
        }

        private static void Populate()
        {
            Dictionary<string, string[]> graph = (from g in from f in ExtraRefs.Split(new string[] { "\r\n" }, StringSplitOptions.None)
                select f.Trim() into file
                let a = GetAssembly(file)
                where a != null
                let types = GetExportedTypes(a)
                where types != null
                from t in types
                let name = t.FullName.Split(new char[] { '.' }).Last<string>().Replace('+', '.')
                group new { Namespace = t.Namespace, file = file } by name.Split(new char[] { '`' }).First<string>()
                select new { TypeName = g.Key, Locations = g.Distinct() } into g
                orderby g.TypeName
                select g).ToDictionary(g => g.TypeName, g => (from l in g.Locations select l.Namespace + ";" + l.file).ToArray<string>());
            using (FileStream stream = File.Create(_cachePath))
            {
                new BinaryFormatter().Serialize(stream, graph);
            }
        }

        private static bool PopulateFromCache()
        {
            if (!File.Exists(_cachePath))
            {
                return false;
            }
            try
            {
                Dictionary<string, string[]> dictionary;
                using (FileStream stream = File.OpenRead(_cachePath))
                {
                    dictionary = (Dictionary<string, string[]>) new BinaryFormatter().Deserialize(stream);
                }
                if (dictionary == null)
                {
                    return false;
                }
                ILookup<string, string> lookup1 = dictionary.Keys.ToLookup<string, string>(s => s, StringComparer.InvariantCultureIgnoreCase);
                TypeResolver.AutoRefCaseLookup = lookup1;
                _caseLookup = lookup1;
                TypeResolver.AutoRefLookup = dictionary;
                _refLookup = dictionary;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

