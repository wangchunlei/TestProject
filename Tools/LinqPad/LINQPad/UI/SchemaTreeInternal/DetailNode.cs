namespace LINQPad.UI.SchemaTreeInternal
{
    using System;

    internal class DetailNode : BaseNode
    {
        public DetailNode(string text) : base(text)
        {
        }

        public override string DragText
        {
            get
            {
                return base.Text;
            }
        }
    }
}

