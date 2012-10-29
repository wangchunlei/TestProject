namespace LINQPad.Extensibility.DataContext
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class ExplorerItem
    {
        internal bool SupportsDDLEditing;
        [NonSerialized]
        public object Tag;

        internal ExplorerItem()
        {
        }

        public ExplorerItem(string text, ExplorerItemKind kind, ExplorerIcon icon)
        {
            this.Text = text;
            this.Kind = kind;
            this.Icon = icon;
        }

        public List<ExplorerItem> Children { get; set; }

        public string DragText { get; set; }

        public ExplorerItem HyperlinkTarget { get; set; }

        public ExplorerIcon Icon { get; set; }

        public bool IsEnumerable { get; set; }

        public ExplorerItemKind Kind { get; set; }

        public string SqlName { get; set; }

        public string SqlTypeDeclaration { get; set; }

        public string Text { get; set; }

        public string ToolTipText { get; set; }
    }
}

