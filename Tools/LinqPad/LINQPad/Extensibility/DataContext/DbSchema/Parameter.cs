namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class Parameter
    {
        public Type ClrType;
        public bool IsFunction;
        public bool IsIn;
        public bool IsOut;
        public bool IsResult;
        public bool IsValid;
        public DbTypeInfo ParamDbType;
        public string ParamName;
        public int ParamOrdinal;
        public string RoutineName;
        public string RoutineSchema;

        public string ClrName { get; internal set; }

        internal string ClrTypeName
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
    }
}

