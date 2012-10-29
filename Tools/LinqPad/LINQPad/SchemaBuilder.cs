namespace LINQPad
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    internal class SchemaBuilder : MarshalByRefObject
    {
        public IEnumerable<ExplorerItem> GetSchemaAndBuildAssembly(string repositoryData, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName, bool allowOneToOne, out IDictionary<string, object> sessionData)
        {
            IEnumerable<ExplorerItem> enumerable2;
            Program.AddLINQPadAssemblyResolver();
            Program.AllowOneToOne = allowOneToOne;
            try
            {
                IEnumerable<ExplorerItem> enumerable;
                Repository r = new Repository(XElement.Parse(repositoryData));
                DynamicDataContextDriver driver = (DynamicDataContextDriver) r.DriverLoader.Driver;
                if (driver is LinqToSqlDynamicDriver)
                {
                    enumerable = ((LinqToSqlDynamicDriver) driver).GetSchemaAndBuildAssembly(r, assemblyToBuild, nameSpace, typeName);
                }
                else
                {
                    enumerable = driver.GetSchemaAndBuildAssembly(r, assemblyToBuild, ref nameSpace, ref typeName);
                }
                sessionData = null;
                if (r.SessionData.Count > 0)
                {
                    sessionData = r.SessionData;
                }
                enumerable2 = enumerable;
            }
            catch (Exception exception)
            {
                if (exception.GetType().FullName == "System.Data.SqlServerCe.SqlCeException")
                {
                    throw new DisplayToUserException("SqlCeException: " + exception.Message);
                }
                if (!this.IsForeignException(exception))
                {
                    throw;
                }
                Log.Write(exception, "Custom Data Context Driver");
                string msg = exception.GetType().Name + " - " + exception.Message;
                if (exception.InnerException != null)
                {
                    msg = msg + " (" + exception.InnerException.Message + ")";
                }
                throw new DisplayToUserException(msg);
            }
            return enumerable2;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        private bool IsForeignException(Exception ex)
        {
            if (ex == null)
            {
                return false;
            }
            Assembly assembly = base.GetType().Assembly;
            Assembly assembly2 = ex.GetType().Assembly;
            return ((assembly2 == null) || ((!(assembly2 != assembly) || assembly2.GlobalAssemblyCache) ? this.IsForeignException(ex.InnerException) : true));
        }
    }
}

