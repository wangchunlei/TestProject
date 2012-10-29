namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Table : SchemaObject
    {
        public readonly List<Relationship> ChildRelations;
        public readonly List<Relationship> ParentRelations;

        public Table(string schemaName, string sqlName, string dotNetName) : base(schemaName, sqlName, dotNetName)
        {
            this.ParentRelations = new List<Relationship>();
            this.ChildRelations = new List<Relationship>();
        }

        public override string DDLEditName
        {
            get
            {
                return "TABLE";
            }
        }

        public override string DisplayGroupName
        {
            get
            {
                return "Tables";
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
                return true;
            }
        }

        public override DbObjectKind Kind
        {
            get
            {
                return DbObjectKind.Table;
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
    }
}

