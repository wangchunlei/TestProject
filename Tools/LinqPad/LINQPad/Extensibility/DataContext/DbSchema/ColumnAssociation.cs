namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using System;

    public class ColumnAssociation
    {
        public string ChildColumn;
        public string ChildSchema = "";
        public string ChildTable;
        public string ParentColumn;
        public string ParentSchema = "";
        public string ParentTable;
        public string RelationshipName;
    }
}

