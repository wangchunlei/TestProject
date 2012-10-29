namespace LINQPad.ObjectGraph
{
    using LINQPad;
    using System;

    internal class MultiDimArrayNode : ObjectNode
    {
        public readonly Array Data;
        public readonly Type ElementType;
        public readonly string Name;

        public MultiDimArrayNode(ObjectNode parent, Array array) : this(parent, array, null)
        {
        }

        public MultiDimArrayNode(ObjectNode parent, Array array, string name) : base(parent, array, 0, null)
        {
            if (name == null)
            {
                this.Name = array.GetType().GetElementType().FormatTypeName() + "[,]";
            }
            else
            {
                this.Name = name;
            }
            this.ElementType = array.GetType().GetElementType();
            this.Data = array;
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

