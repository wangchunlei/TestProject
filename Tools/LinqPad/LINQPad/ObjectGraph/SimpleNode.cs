namespace LINQPad.ObjectGraph
{
    using System;

    internal class SimpleNode : ObjectNode
    {
        public readonly SimpleNodeKind NodeKind;
        public readonly string Text;
        public readonly string ToolTip;

        public SimpleNode(ObjectNode parent, string text) : this(parent, text, null)
        {
        }

        public SimpleNode(ObjectNode parent, string text, string tip) : this(parent, text, tip, SimpleNodeKind.Data)
        {
        }

        public SimpleNode(ObjectNode parent, string text, string tip, SimpleNodeKind nodeKind) : base(parent, text, 0, null)
        {
            if (text == null)
            {
                this.NodeKind = SimpleNodeKind.Metadata;
                this.Text = "null";
            }
            else
            {
                this.Text = text;
                this.NodeKind = nodeKind;
            }
            this.ToolTip = tip;
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

