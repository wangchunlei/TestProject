namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad.UI;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class AddCxNode : TreeNode
    {
        public AddCxNode()
        {
            base.NodeFont = SchemaTree.UnderlineFont;
            base.ForeColor = SystemColors.HotTrack;
            base.Text = "Add connection";
            base.ImageKey = base.SelectedImageKey = "Add";
        }
    }
}

