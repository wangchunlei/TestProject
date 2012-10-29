namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;

    [Serializable]
    public class View : SchemaObject
    {
        public View(string schemaName, string sqlName, string dotNetName) : base(schemaName, sqlName, dotNetName)
        {
        }

        public override string DDLEditName
        {
            get
            {
                return "VIEW";
            }
        }

        public override string DisplayGroupName
        {
            get
            {
                return "Views";
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
                return false;
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
                return DbObjectKind.View;
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
                return ("Table <" + base.DotNetName + ">");
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

