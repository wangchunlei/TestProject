namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal static class GacResolver
    {
        private static Dictionary<string, string> _lookup = new Dictionary<string, string>();

        public static string FindPath(string fullName)
        {
            lock (_lookup)
            {
                if (!_lookup.ContainsKey(fullName))
                {
                    using (DomainIsolator isolator = new DomainIsolator("GAC Resolver"))
                    {
                        isolator.Domain.SetData("fullname", fullName);
                        isolator.Domain.SetData("path", null);
                        isolator.Domain.DoCallBack(new CrossAppDomainDelegate(GacResolver.Resolve));
                        return (_lookup[fullName] = (string) isolator.Domain.GetData("path"));
                    }
                }
                return _lookup[fullName];
            }
        }

        public static bool IsLINQPadGaced()
        {
            bool flag;
            try
            {
                using (DomainIsolator isolator = new DomainIsolator("LINQPad GAC Tester"))
                {
                    flag = isolator.GetInstance<LINQPadGacTester>().IsGaced(new LINQPadGacTester().GetFileVersion());
                }
            }
            catch (TypeLoadException)
            {
                flag = true;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        private static void Resolve()
        {
            try
            {
                Assembly assembly;
                string data = (string) AppDomain.CurrentDomain.GetData("fullname");
                try
                {
                    assembly = Assembly.ReflectionOnlyLoad(data);
                }
                catch (FileNotFoundException)
                {
                    if (data != "EntityFramework, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
                    {
                        return;
                    }
                    assembly = Assembly.ReflectionOnlyLoad("EntityFramework, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                }
                string text1 = (assembly.Location == "") ? null : assembly.Location;
                AppDomain.CurrentDomain.SetData("path", assembly.Location);
            }
            catch
            {
            }
        }

        public static bool IsEntityFrameworkAvailable
        {
            get
            {
                return true;
            }
        }

        private class LINQPadGacTester : MarshalByRefObject
        {
            public string GetFileVersion()
            {
                return Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).Cast<AssemblyFileVersionAttribute>().Single<AssemblyFileVersionAttribute>().Version;
            }

            public bool IsGaced(string callingVersion)
            {
                return (callingVersion != this.GetFileVersion());
            }
        }
    }
}

