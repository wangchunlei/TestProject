namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class TreeStateMemento : TreeStateMementoItem, IDisposable
    {
        private Point _scrollPos;
        private TreeView _treeView;
        public readonly TreeNode Node;

        public TreeStateMemento(TreeNode node) : base(node)
        {
            this.Node = node;
            this._treeView = node.TreeView;
            if (this._treeView != null)
            {
                this._scrollPos = Native.GetScrollPos(this._treeView);
                this._treeView.BeginUpdate();
            }
        }

        public void Dispose()
        {
            try
            {
                base.Restore(this.Node);
                if (this._treeView != null)
                {
                    Native.SetScrollPos(this._treeView, this._scrollPos);
                }
            }
            finally
            {
                if (this._treeView != null)
                {
                    this._treeView.EndUpdate();
                }
            }
        }
    }
}

