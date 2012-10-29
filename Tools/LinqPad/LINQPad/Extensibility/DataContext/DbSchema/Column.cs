namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class Column
    {
        internal string ClrObjectName;
        public Type ClrType;
        public int ColumnID;
        public string ColumnName = "";
        public int ColumnOrdinal;
        public bool IsAutoGen;
        public bool IsComputed;
        public bool IsKey;
        public bool IsNullable;
        public bool IsTimeStamp;
        public int Length;
        internal SchemaObject Object;
        public DbObjectKind ObjectKind;
        public string ObjectName;
        public int Precision;
        public int Scale;
        public string SchemaName = "";
        public DbTypeInfo SqlType;

        public virtual string GetFullSqlTypeDeclaration()
        {
            object obj2;
            string name = this.SqlType.Name;
            if (this.SqlType.VariableScale)
            {
                obj2 = name;
                name = string.Concat(new object[] { obj2, "(", this.Precision, ",", this.Scale, ")" });
            }
            else if (this.SqlType.VariablePrecision && (this.Precision > 0))
            {
                obj2 = name;
                name = string.Concat(new object[] { obj2, "(", this.Precision, ")" });
            }
            else if (this.SqlType.VariablePrecision)
            {
                name = name + "(MAX)";
            }
            if (!this.IsNullable)
            {
                name = name + " NOT NULL";
            }
            return name;
        }

        internal string CSharpTypeName
        {
            get
            {
                if (this.ClrType == null)
                {
                    return "";
                }
                if (this.ClrType.IsGenericType && (this.ClrType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    return (this.ClrType.GetGenericArguments()[0].Name + "?");
                }
                return this.ClrType.Name;
            }
        }

        internal string FieldName
        {
            get
            {
                return ("_" + this.PropertyName);
            }
        }

        public string ObjectKey
        {
            get
            {
                return (this.SchemaName + "." + this.ObjectName);
            }
        }

        public string PropertyName { get; internal set; }
    }
}

