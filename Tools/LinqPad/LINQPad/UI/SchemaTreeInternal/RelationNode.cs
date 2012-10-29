namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad.Extensibility.DataContext.DbSchema;
    using LINQPad.UI;
    using System;

    internal class RelationNode : BaseNode
    {
        public readonly Relationship Relation;
        public readonly int TreeOrderGroup;

        public RelationNode(Relationship r, string text, int treeOrderGroup) : base(text)
        {
            this.Relation = r;
            this.TreeOrderGroup = treeOrderGroup;
            base.NodeFont = SchemaTree.UnderlineFont;
        }
    }
}

