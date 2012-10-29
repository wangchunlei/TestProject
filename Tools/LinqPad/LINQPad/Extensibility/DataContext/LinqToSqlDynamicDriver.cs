namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext.DbSchema;
    using LINQPad.Schema;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.SqlClient;
    using System.Reflection;
    using System.Windows.Forms;

    internal class LinqToSqlDynamicDriver : DynamicDataContextDriver
    {
        public override void ClearConnectionPools(IConnectionInfo c)
        {
            if (c.DatabaseInfo.IsSqlServer)
            {
                try
                {
                    using (SqlConnection connection = (SqlConnection) this.GetIDbConnection(c))
                    {
                        SqlConnection.ClearPool(connection);
                    }
                }
                catch
                {
                }
            }
        }

        public override string GetConnectionDescription(IConnectionInfo r)
        {
            return null;
        }

        internal override Type GetContextBaseType()
        {
            return typeof(DataContext);
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo r)
        {
            return new object[] { DataContextBase.GetConnection(r.DatabaseInfo.GetCxString(), r.DatabaseInfo.Provider) };
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo r)
        {
            return new ParameterDescriptor[] { new ParameterDescriptor("connection", "System.Data.IDbConnection") };
        }

        internal override string GetFailedImageKey(IConnectionInfo r)
        {
            return (r.DatabaseInfo.DbVersion.StartsWith("Azure", StringComparison.InvariantCultureIgnoreCase) ? "FailedAzureDatabase" : "FailedDatabase");
        }

        internal override string GetImageKey(IConnectionInfo r)
        {
            return (r.DatabaseInfo.DbVersion.StartsWith("Azure", StringComparison.InvariantCultureIgnoreCase) ? "AzureDatabase" : (r.DatabaseInfo.IsSqlCE ? "Compact" : "Database"));
        }

        public override DateTime? GetLastSchemaUpdate(IConnectionInfo r)
        {
            DateTime? nullable2;
            if (!r.DatabaseInfo.IsSqlServer)
            {
                return null;
            }
            using (SqlConnection connection = (SqlConnection) ((Repository) r).Open())
            {
                SqlCommand command = new SqlCommand("select max (modify_date) from sys.objects", connection);
                try
                {
                    nullable2 = new DateTime?((DateTime) command.ExecuteScalar());
                }
                catch
                {
                    nullable2 = null;
                }
            }
            return nullable2;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo r, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<ExplorerItem> GetSchemaAndBuildAssembly(Repository r, AssemblyName target, string nameSpace, string typeName)
        {
            Database mainSchema = r.IsSqlCE ? new SqlCeSchemaReader().GetDatabase(r) : (r.IsAzure ? new AzureSchemaReader().GetDatabase(r) : new SqlServerSchemaReader().GetDatabase(r));
            EmissionsGenerator.Generate(mainSchema, target, r.GetCxString(), nameSpace, typeName);
            return mainSchema.GetExplorerSchema(true, null, false);
        }

        internal override bool IsQueryable(Repository r)
        {
            return (((r.Database.Length > 0) || (r.AttachFile && (r.AttachFileName.Length > 0))) || !r.IsSqlServer);
        }

        public override void OnQueryFinishing(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            base.OnQueryFinishing(cxInfo, context, executionManager);
        }

        public override bool ShowConnectionDialog(IConnectionInfo repository, bool isNewRepository)
        {
            using (CxForm form = new CxForm((Repository) repository, isNewRepository))
            {
                return (form.ShowDialog() == DialogResult.OK);
            }
        }

        public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        {
            DataContext context2 = context as DataContext;
            if (context2 != null)
            {
                try
                {
                    context2.Dispose();
                }
                catch
                {
                }
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
                return "System.Data.Linq.DataContext";
            }
        }

        internal override string InternalID
        {
            get
            {
                return "";
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
                return "LINQ to SQL";
            }
        }
    }
}

