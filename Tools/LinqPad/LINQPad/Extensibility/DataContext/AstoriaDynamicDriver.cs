namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Services.Client;
    using System.Reflection;
    using System.Windows.Forms;

    internal class AstoriaDynamicDriver : DynamicDataContextDriver
    {
        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            return new string[] { "System.Data.Services.Client.dll" };
        }

        public override string GetConnectionDescription(IConnectionInfo r)
        {
            return null;
        }

        internal override Type GetContextBaseType()
        {
            return typeof(DataServiceContext);
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo r)
        {
            return new object[] { new Uri(r.DatabaseInfo.Server) };
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo r)
        {
            return new ParameterDescriptor[] { new ParameterDescriptor("serviceRoot", "System.Uri") };
        }

        public override IDbConnection GetIDbConnection(IConnectionInfo cxInfo)
        {
            return null;
        }

        internal override string GetImageKey(IConnectionInfo r)
        {
            return "Globe";
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            return new string[] { "System.Data.Services.Client" };
        }

        public override IEnumerable<string> GetNamespacesToRemove(IConnectionInfo cxInfo)
        {
            return new string[] { "System.Data.Linq", "System.Data.Linq.SqlClient" };
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo r, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            return AstoriaHelper.GetSchemaAndBuildAssembly(r, assemblyToBuild, ref nameSpace, ref typeName);
        }

        public override void InitializeContext(IConnectionInfo r, object context, QueryExecutionManager executionManager)
        {
            AstoriaHelper.InitializeContext(r, context, false);
        }

        public override void OnQueryFinishing(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            base.OnQueryFinishing(cxInfo, context, executionManager);
        }

        public override void PreprocessObjectToWrite(ref object objectToWrite, ObjectGraphInfo info)
        {
            AstoriaHelper.PreprocessObjectToWrite(ref objectToWrite, info);
        }

        public override bool ShowConnectionDialog(IConnectionInfo repository, bool isNewRepository)
        {
            using (CxForm form = new CxForm((Repository) repository, isNewRepository))
            {
                return (form.ShowDialog() == DialogResult.OK);
            }
        }

        public override string Author
        {
            get
            {
                return "(built in)";
            }
        }

        internal override string ContextBaseTypeName
        {
            get
            {
                return "System.Data.Services.Client.DataServiceContext";
            }
        }

        internal override string InternalID
        {
            get
            {
                return "AstoriaAuto";
            }
        }

        internal override int InternalSortOrder
        {
            get
            {
                return 30;
            }
        }

        internal override bool IsBuiltIn
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "WCF Data Services (OData)";
            }
        }

        internal override bool UsesDatabaseConnection
        {
            get
            {
                return false;
            }
        }
    }
}

