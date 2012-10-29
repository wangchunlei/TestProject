namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public abstract class SchemaObject
    {
        public readonly IDictionary<int, Column> Columns = new Dictionary<int, Column>();
        public readonly string DotNetName;
        public bool HasKey;
        public string OriginalName;
        public readonly IList<Parameter> Parameters = new List<Parameter>();
        public string PropertyName;
        public Parameter ReturnInfo;
        public readonly string SchemaName;
        public string SingularName;
        public readonly string SqlName;

        protected SchemaObject(string schemaName, string sqlName, string dotNetName)
        {
            this.SchemaName = schemaName;
            this.SqlName = sqlName;
            this.DotNetName = this.PropertyName = dotNetName;
        }

        public static SchemaObject Create(Column c, string dotNetName)
        {
            switch (c.ObjectKind)
            {
                case DbObjectKind.Table:
                    return new Table(c.SchemaName, c.ObjectName, dotNetName);

                case DbObjectKind.View:
                    return new View(c.SchemaName, c.ObjectName, dotNetName);

                case DbObjectKind.StoredProc:
                    return new StoredProc(c.SchemaName, c.ObjectName, dotNetName);

                case DbObjectKind.ScalarFunction:
                    return new ScalarFunction(c.SchemaName, c.ObjectName, dotNetName);

                case DbObjectKind.TableFunction:
                    return new TableFunction(c.SchemaName, c.ObjectName, dotNetName);
            }
            return null;
        }

        public IEnumerable<Column> ColumnsInOrder
        {
            get
            {
                return (from c in this.Columns.Values
                    orderby c.ColumnOrdinal
                    select c);
            }
        }

        public abstract string DDLEditName { get; }

        public abstract string DisplayGroupName { get; }

        public abstract bool HasColumns { get; }

        public abstract bool HasParameters { get; }

        public abstract bool IsPluralizable { get; }

        public abstract DbObjectKind Kind { get; }

        public abstract bool NeedsCustomType { get; }

        public abstract string PropertyTypeDescription { get; }

        public virtual bool SupportsDDLEditing
        {
            get
            {
                return false;
            }
        }
    }
}

