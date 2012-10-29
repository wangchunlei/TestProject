namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;

    [Serializable]
    public class TableFunction : SchemaObject
    {
        public TableFunction(string schemaName, string sqlName, string dotNetName) : base(schemaName, sqlName, dotNetName + "Result")
        {
            base.PropertyName = dotNetName;
        }

        public override string DDLEditName
        {
            get
            {
                return "FUNCTION";
            }
        }

        public override string DisplayGroupName
        {
            get
            {
                return "Functions";
            }
        }

        public override bool HasColumns
        {
            get
            {
                return true;
            }
        }

        public override bool HasParameters
        {
            get
            {
                return true;
            }
        }

        public override bool IsPluralizable
        {
            get
            {
                return false;
            }
        }

        public override DbObjectKind Kind
        {
            get
            {
                return DbObjectKind.TableFunction;
            }
        }

        public override bool NeedsCustomType
        {
            get
            {
                return true;
            }
        }

        public override string PropertyTypeDescription
        {
            get
            {
                return ("IQueryable <" + base.DotNetName + ">");
            }
        }

        public override bool SupportsDDLEditing
        {
            get
            {
                return true;
            }
        }
    }
}

