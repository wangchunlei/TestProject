namespace LINQPad.ObjectGraph
{
    using System;

    internal class EmptyNode : ObjectNode
    {
        public EmptyNode() : base(null, null, 0, null)
        {
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

