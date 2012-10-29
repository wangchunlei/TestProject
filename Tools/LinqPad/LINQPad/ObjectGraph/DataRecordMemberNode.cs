namespace LINQPad.ObjectGraph
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Data;

    internal class DataRecordMemberNode : MemberNode
    {
        public DataRecordMemberNode(ObjectNode parent, Type[] types, IDataRecord r, int maxDepth, DataContextDriver dcDriver) : base(parent, r, maxDepth, dcDriver)
        {
            if (base.IsAtNestingLimit())
            {
                base.GraphTruncated = true;
            }
            else
            {
                for (int i = 0; i < r.FieldCount; i++)
                {
                    object item = r.GetValue(i);
                    Type type = null;
                    if (types != null)
                    {
                        type = types[i];
                    }
                    else if (item != null)
                    {
                        type = item.GetType();
                    }
                    if (!(item is IDataRecord))
                    {
                    }
                    base.Members.Add(new MemberData(r.GetName(i), type, ObjectNode.Create(this, item, maxDepth, base.DCDriver)));
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

