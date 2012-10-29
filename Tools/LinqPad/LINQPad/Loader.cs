namespace LINQPad
{
    using LINQPad.UI;
    using System;
    using System.Runtime.CompilerServices;

    internal static class Loader
    {
        [LoaderOptimization(LoaderOptimization.MultiDomainHost), STAThread]
        internal static void Main(string[] args)
        {
            if (!AlreadyRun)
            {
                AlreadyRun = true;
                if (PermissionsCheck.Demand() && VersionCheck.Check())
                {
                    ProgramStarter.Run(args);
                }
            }
        }

        internal static bool AlreadyRun
        {
            [CompilerGenerated]
            get
            {
                return <AlreadyRun>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                <AlreadyRun>k__BackingField = value;
            }
        }
    }
}

