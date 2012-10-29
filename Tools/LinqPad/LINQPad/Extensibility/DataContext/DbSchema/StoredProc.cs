namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;

    [Serializable]
    public class StoredProc : SchemaObject
    {
        public StoredProc(string schemaName, string sqlName, string dotNetName) : base(schemaName, sqlName, dotNetName)
        {
        }

        public override string DDLEditName
        {
            get
            {
                return "PROCEDURE";
            }
        }

        public override string DisplayGroupName
        {
            get
            {
                return "Stored Procs";
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
                return DbObjectKind.StoredProc;
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

