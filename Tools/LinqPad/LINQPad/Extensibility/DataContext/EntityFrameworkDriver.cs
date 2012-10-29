namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using LINQPad.ExecutionModel;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Linq;
    using System.Windows.Forms;

    internal class EntityFrameworkDriver : StaticDataContextDriver
    {
        public override void ExecuteESqlQuery(IConnectionInfo cxInfo, string query)
        {
            EntityFrameworkHelper.ExecuteESqlQuery(this.GetCxStringForESql(cxInfo), query);
        }

        public override string GetConnectionDescription(IConnectionInfo r)
        {
            return null;
        }

        internal override Type GetContextBaseType()
        {
            return typeof(ObjectContext);
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo c)
        {
            return new object[] { this.GetCxStringForESql(c) };
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo r)
        {
            return new ParameterDescriptor[] { new ParameterDescriptor("connectionString", "System.String") };
        }

        public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {
            if (objectToWrite == null)
            {
                return null;
            }
            if (!EntityFrameworkMemberProvider.IsEntity(objectToWrite.GetType()))
            {
                return null;
            }
            return new EntityFrameworkMemberProvider(objectToWrite);
        }

        private string GetCxStringForESql(IConnectionInfo c)
        {
            return ("Provider=" + c.DatabaseInfo.Provider + "; Provider Connection String='" + c.DatabaseInfo.GetCxString() + "'; metadata=" + c.CustomTypeInfo.CustomMetadataPath);
        }

        internal override string GetImageKey(IConnectionInfo r)
        {
            return "EF";
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            if (GacResolver.IsEntityFrameworkAvailable)
            {
                return new string[] { "System.Data.EntityClient", "System.Data.Metadata.Edm", "System.Data.Objects", "System.Data.Objects.DataClasses" };
            }
            return null;
        }

        public override IEnumerable<string> GetNamespacesToRemove(IConnectionInfo cxInfo)
        {
            return new string[] { "System.Data.Linq", "System.Data.Linq.SqlClient" };
        }

        public override List<ExplorerItem> GetSchema(IConnectionInfo r, Type t)
        {
            List<ExplorerItem> schema = EntityFrameworkHelper.GetSchema(t);
            if ((from ei in schema
                where ei.Kind == ExplorerItemKind.QueryableObject
                select ei).All<ExplorerItem>(ei => ei.Children.Count == 0))
            {
                try
                {
                    ObjectContext objectContext = Activator.CreateInstance(t, this.GetContextConstructorArguments(r)) as ObjectContext;
                    if (objectContext != null)
                    {
                        return EntityFrameworkEdmReader.GetSchema(objectContext);
                    }
                }
                catch
                {
                }
            }
            return schema;
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
                return "System.Data.Objects.ObjectContext";
            }
        }

        internal override string InternalID
        {
            get
            {
                return "EntityFramework";
            }
        }

        internal override int InternalSortOrder
        {
            get
            {
                return 20;
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
                return "Entity Framework";
            }
        }
    }
}

