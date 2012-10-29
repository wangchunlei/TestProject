namespace LINQPad.Extensibility.DataContext
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class DynamicDataContextDriver : DataContextDriver
    {
        protected DynamicDataContextDriver()
        {
        }

        public virtual DateTime? GetLastSchemaUpdate(IConnectionInfo cxInfo)
        {
            return null;
        }

        public abstract List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName);
    }
}

