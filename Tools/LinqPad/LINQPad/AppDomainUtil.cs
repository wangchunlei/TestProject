namespace LINQPad
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    internal class AppDomainUtil
    {
        private static void ConfigureNewDomain(AppDomain domain)
        {
            domain.SetData("LINQPad.InstanceID", Program.AppInstanceID);
            domain.SetData("LINQPad.Path", typeof(AppDomainUtil).Assembly.Location);
            ((AddAssemblyResolver) domain.CreateInstanceFromAndUnwrap(typeof(AddAssemblyResolver).Assembly.Location, typeof(AddAssemblyResolver).FullName)).Go();
        }

        public static AppDomain CreateDomain(string friendlyName)
        {
            return CreateDomain(friendlyName, null, null);
        }

        private static AppDomain CreateDomain(string friendlyName, AppDomainSetup setup)
        {
            AppDomain domain = AppDomain.CreateDomain(friendlyName, null, setup, new PermissionSet(PermissionState.Unrestricted), new StrongName[0]);
            ConfigureNewDomain(domain);
            return domain;
        }

        public static AppDomain CreateDomain(string friendlyName, string configFile, string appBase)
        {
            AppDomainSetup appDomainSetup = GetAppDomainSetup(configFile, appBase);
            return CreateDomain(friendlyName, appDomainSetup);
        }

        private static AppDomainSetup GetAppDomainSetup(string configFile, string appBase)
        {
            AppDomainSetup setup2 = new AppDomainSetup {
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };
            if (!string.IsNullOrEmpty(appBase))
            {
                setup2.ApplicationBase = appBase;
            }
            else
            {
                setup2.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            }
            if (!(string.IsNullOrEmpty(configFile) || !File.Exists(configFile)))
            {
                setup2.ConfigurationFile = configFile;
                return setup2;
            }
            if (!string.IsNullOrEmpty(Program.QueryConfigFile))
            {
                setup2.ConfigurationFile = Program.QueryConfigFile;
            }
            return setup2;
        }

        private class AddAssemblyResolver : MarshalByRefObject
        {
            public void Go()
            {
                Program.AddLINQPadAssemblyResolver();
            }
        }
    }
}

