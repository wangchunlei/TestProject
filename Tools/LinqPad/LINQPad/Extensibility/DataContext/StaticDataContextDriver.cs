namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class StaticDataContextDriver : DataContextDriver
    {
        protected StaticDataContextDriver()
        {
        }

        public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
        {
            if (!base.AreRepositoriesEquivalent(c1, c2))
            {
                return false;
            }
            return c1.CustomTypeInfo.IsEquivalent(c2.CustomTypeInfo);
        }

        internal Type GetCustomType(IConnectionInfo c)
        {
            if ((c.CustomTypeInfo.CustomAssemblyPath.Length == 0) || (c.CustomTypeInfo.CustomTypeName.Length == 0))
            {
                return null;
            }
            return Assembly.LoadFrom(c.CustomTypeInfo.CustomAssemblyPath).GetType(c.CustomTypeInfo.CustomTypeName);
        }

        internal List<ExplorerItem> GetSchema(IConnectionInfo c)
        {
            Type customType = this.GetCustomType(c);
            if (customType == null)
            {
                throw new DisplayToUserException("Cannot instantiate type '" + c.CustomTypeInfo.GetCustomTypeDescription() + "'.");
            }
            return this.GetSchema(c, customType);
        }

        public abstract List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType);
        protected internal object InstantiateBaseContext(IConnectionInfo cxInfo)
        {
            return Activator.CreateInstance(DataContextDriver.LoadAssemblySafely(cxInfo.CustomTypeInfo.CustomAssemblyPath).GetType(cxInfo.CustomTypeInfo.CustomTypeName, true), this.GetContextConstructorArguments(cxInfo));
        }
    }
}

