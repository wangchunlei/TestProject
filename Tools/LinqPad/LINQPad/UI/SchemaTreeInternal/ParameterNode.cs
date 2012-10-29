namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad.Extensibility.DataContext.DbSchema;
    using LINQPad.UI;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ParameterNode : TreeNode
    {
        public ParameterNode(Parameter param, string text) : base(text)
        {
            base.NodeFont = SchemaTree.ItalicFont;
            base.ForeColor = (SystemColors.Window.GetBrightness() < 0.5f) ? Color.FromArgb(110, 140, 0xff) : Color.FromArgb(60, 80, 150);
            base.ImageKey = base.SelectedImageKey = "Parameter";
        }
    }
}

