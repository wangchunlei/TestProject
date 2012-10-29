namespace LINQPad.UI.SchemaTreeInternal
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    internal class BaseNode : TreeNode
    {
        public BaseNode(string text) : base(text)
        {
        }

        protected internal virtual void OnAfterExpand()
        {
        }

        protected internal virtual void OnBeforeExpand()
        {
        }

        public virtual void SetSqlMode(bool isSqlMode)
        {
            foreach (BaseNode node in base.Nodes.OfType<BaseNode>())
            {
                node.SetSqlMode(isSqlMode);
            }
        }

        public virtual string DragText
        {
            get
            {
                return null;
            }
        }
    }
}

