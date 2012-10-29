namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;

    [Serializable]
    public class DbTypeInfo
    {
        public readonly string Name;
        public readonly System.Type Type;
        public readonly bool VariablePrecision;
        public readonly bool VariableScale;

        public DbTypeInfo(string name, System.Type type) : this(name, type, false, false)
        {
        }

        public DbTypeInfo(string name, System.Type type, bool variablePrecision) : this(name, type, variablePrecision, false)
        {
        }

        public DbTypeInfo(string name, System.Type type, bool variablePrecision, bool variableScale)
        {
            this.Name = name;
            this.Type = type;
            this.VariablePrecision = variablePrecision;
            this.VariableScale = variableScale;
        }
    }
}

