namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;

    [Serializable]
    public class ScalarFunction : SchemaObject
    {
        public ScalarFunction(string schemaName, string sqlName, string dotNetName) : base(schemaName, sqlName, dotNetName)
        {
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
                return false;
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
                return DbObjectKind.ScalarFunction;
            }
        }

        public override bool NeedsCustomType
        {
            get
            {
                return false;
            }
        }

        public override string PropertyTypeDescription
        {
            get
            {
                return "";
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

