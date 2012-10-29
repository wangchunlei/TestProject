namespace LINQPad.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    internal class TreeStateMementoItem
    {
        private readonly Dictionary<string, TreeStateMementoItem> ExpandedNodes;
        private readonly string Text;

        public TreeStateMementoItem(TreeNode node)
        {
            this.Text = node.Text;
            this.ExpandedNodes = (from n in node.Nodes.Cast<TreeNode>()
                where n.IsExpanded
                group n by n.Text).ToDictionary<IGrouping<string, TreeNode>, string, TreeStateMementoItem>(g => g.Key, g => new TreeStateMementoItem(g.First<TreeNode>()));
        }

        public void Restore(TreeNode node)
        {
            foreach (TreeNode node2 in node.Nodes)
            {
                TreeStateMementoItem item;
                if (this.ExpandedNodes.TryGetValue(node2.Text, out item))
                {
                    item.Restore(node2);
                    node2.Expand();
                }
            }
        }
    }
}

