namespace LINQPad.ObjectGraph
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections;

    internal class ListPayloadNode : ListNode
    {
        public ListPayloadNode(ObjectNode parent, IEnumerable list, int maxDepth, DataContextDriver dcDriver, string name, ObjectNode payload, string payLoadName) : base(parent, list, maxDepth, dcDriver, name)
        {
            base.GroupKey = payload;
            base.GroupKeyText = payLoadName;
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

