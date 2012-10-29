namespace LINQPad.Schema
{
    using LINQPad.Extensibility.DataContext.DbSchema;
    using System;

    [Serializable]
    public class SqlColumn : Column
    {
        public int ObjectID;

        public string GetFullSqlTypeDeclaration()
        {
            return base.GetFullSqlTypeDeclaration();
        }
    }
}

