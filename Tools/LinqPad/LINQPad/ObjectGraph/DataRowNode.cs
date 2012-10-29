namespace LINQPad.ObjectGraph
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Data;

    internal class DataRowNode : MemberNode
    {
        public DataRowNode(ObjectNode parent, DataRow item, int maxDepth, DataContextDriver dcDriver) : base(parent, item, maxDepth, dcDriver)
        {
            base.Name = "DataRow";
            if (base.IsAtNestingLimit())
            {
                base.GraphTruncated = true;
            }
            else
            {
                foreach (DataColumn column in item.Table.Columns)
                {
                    object obj2 = item[column];
                    base.Members.Add(new MemberData(column.ColumnName, column.DataType, ObjectNode.Create(this, obj2, maxDepth, dcDriver)));
                }
                if ((base.Members.Count > 50) && (base.NestingDepth > 1))
                {
                    base.InitiallyHidden = true;
                }
            }
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

