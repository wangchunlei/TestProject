namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    internal class ExplorerItemNode : BaseNode
    {
        private bool _dormantChildren;
        private LINQPad.Extensibility.DataContext.ExplorerItem _explorerItem;
        private Repository _repository;
        private bool _sqlMode;

        public ExplorerItemNode(Repository r, LINQPad.Extensibility.DataContext.ExplorerItem m) : base(GetItemText(m, UseSqlMode(r)))
        {
            this._repository = r;
            this._explorerItem = m;
            this._sqlMode = UseSqlMode(r);
            base.ImageKey = base.SelectedImageKey = m.Icon.ToString();
            base.ToolTipText = m.ToolTipText;
            if (m.HyperlinkTarget != null)
            {
                base.NodeFont = SchemaTree.UnderlineFont;
            }
            else if (m.Kind == ExplorerItemKind.Parameter)
            {
                base.NodeFont = SchemaTree.ItalicFont;
            }
            else if (m.Kind != ExplorerItemKind.QueryableObject)
            {
                base.NodeFont = SchemaTree.BaseFont;
            }
            if (m.Kind == ExplorerItemKind.Category)
            {
                base.ForeColor = (SystemColors.Window.GetBrightness() < 0.5f) ? Color.FromArgb(120, 0xff, 0xff) : Color.FromArgb(0, 120, 120);
            }
            else if (m.Kind == ExplorerItemKind.CollectionLink)
            {
                base.ForeColor = (SystemColors.Window.GetBrightness() < 0.5f) ? Color.LightGreen : Color.Green;
            }
            else if (m.Kind == ExplorerItemKind.Parameter)
            {
                base.ForeColor = (SystemColors.Window.GetBrightness() < 0.5f) ? Color.FromArgb(110, 140, 0xff) : Color.FromArgb(60, 80, 150);
            }
            else if (m.Kind == ExplorerItemKind.Property)
            {
                if (SystemColors.Window.GetBrightness() > 0.8f)
                {
                    base.ForeColor = Color.FromArgb(40, 40, 40);
                }
            }
            else if (m.Kind == ExplorerItemKind.ReferenceLink)
            {
                base.ForeColor = (SystemColors.Window.GetBrightness() < 0.5f) ? Color.RoyalBlue : Color.Blue;
            }
            else if (m.Kind == ExplorerItemKind.Schema)
            {
                base.ForeColor = (SystemColors.Window.GetBrightness() < 0.5f) ? Color.Orchid : Color.Purple;
            }
            if (m.Icon == ExplorerIcon.Inherited)
            {
                base.ForeColor = Color.FromArgb(base.ForeColor.R + ((0xff - base.ForeColor.R) / 3), base.ForeColor.G + ((0xff - base.ForeColor.G) / 3), base.ForeColor.B + ((0xff - base.ForeColor.B) / 3));
            }
            bool flag = m.Kind == ExplorerItemKind.QueryableObject;
            if (m.Children != null)
            {
                foreach (LINQPad.Extensibility.DataContext.ExplorerItem item in m.Children)
                {
                    if (flag)
                    {
                        this._dormantChildren = true;
                    }
                    else
                    {
                        base.Nodes.Add(new ExplorerItemNode(this._repository, item));
                    }
                }
            }
            if (this._dormantChildren)
            {
                base.Nodes.Add(" ");
            }
        }

        private static string GetItemText(LINQPad.Extensibility.DataContext.ExplorerItem item, bool sqlMode)
        {
            if (!(sqlMode && !string.IsNullOrEmpty(item.SqlName)))
            {
                return item.Text;
            }
            string sqlName = item.SqlName;
            if (!string.IsNullOrEmpty(item.SqlTypeDeclaration))
            {
                sqlName = sqlName + " (" + item.SqlTypeDeclaration + ")";
            }
            return sqlName;
        }

        protected internal override void OnBeforeExpand()
        {
            Func<LINQPad.Extensibility.DataContext.ExplorerItem, ExplorerItemNode> selector = null;
            if (this._dormantChildren)
            {
                try
                {
                    base.TreeView.BeginUpdate();
                    base.Nodes.Clear();
                    if (selector == null)
                    {
                        selector = c => new ExplorerItemNode(this._repository, c);
                    }
                    base.Nodes.AddRange(this._explorerItem.Children.Select<LINQPad.Extensibility.DataContext.ExplorerItem, ExplorerItemNode>(selector).ToArray<ExplorerItemNode>());
                    this._dormantChildren = false;
                }
                finally
                {
                    base.TreeView.EndUpdate();
                }
            }
            base.OnBeforeExpand();
        }

        internal bool ProcessClick()
        {
            // This item is obfuscated and can not be translated.
            if (this._explorerItem.HyperlinkTarget == null)
            {
                return false;
            }
            for (TreeNode node = base.Parent; node == null; node = node.Parent)
            {
            Label_0024:
                if (0 == 0)
                {
                    RepositoryNode node2 = node as RepositoryNode;
                    if (node2 == null)
                    {
                        return false;
                    }
                    ExplorerItemNode node3 = node2.FindExplorerItemNode(this._explorerItem.HyperlinkTarget);
                    if (node3 == null)
                    {
                        return false;
                    }
                    base.TreeView.SelectedNode = node3;
                    if (node3.Nodes.Count > 0)
                    {
                        node3.Nodes.Cast<TreeNode>().Take<TreeNode>(7).Last<TreeNode>().EnsureVisible();
                    }
                    return true;
                }
            }
            goto Label_0024;
        }

        public override void SetSqlMode(bool isSqlMode)
        {
            if (this._sqlMode != isSqlMode)
            {
                this._sqlMode = isSqlMode;
                if (!string.IsNullOrEmpty(this._explorerItem.SqlName))
                {
                    base.Text = GetItemText(this._explorerItem, isSqlMode);
                }
                foreach (BaseNode node in base.Nodes.OfType<BaseNode>())
                {
                    node.SetSqlMode(isSqlMode);
                }
            }
        }

        private static bool UseSqlMode(Repository r)
        {
            if (UserOptions.Instance.DefaultQueryLanguage.HasValue && (((QueryLanguage) UserOptions.Instance.DefaultQueryLanguage.Value) == QueryLanguage.SQL))
            {
                return ((((MainForm.Instance == null) || (MainForm.Instance.CurrentQuery == null)) || (MainForm.Instance.CurrentQuery.Repository != r)) || (MainForm.Instance.CurrentQuery.QueryKind == QueryLanguage.SQL));
            }
            return ((((MainForm.Instance != null) && (MainForm.Instance.CurrentQuery != null)) && (MainForm.Instance.CurrentQuery.Repository == r)) && (MainForm.Instance.CurrentQuery.QueryKind == QueryLanguage.SQL));
        }

        public override string DragText
        {
            get
            {
                if (((!string.IsNullOrEmpty(this._explorerItem.SqlName) && (MainForm.Instance != null)) && (MainForm.Instance.CurrentQuery != null)) && (MainForm.Instance.CurrentQuery.QueryKind == QueryLanguage.SQL))
                {
                    return this._explorerItem.SqlName;
                }
                return this._explorerItem.DragText;
            }
        }

        public LINQPad.Extensibility.DataContext.ExplorerItem ExplorerItem
        {
            get
            {
                return this._explorerItem;
            }
        }
    }
}

