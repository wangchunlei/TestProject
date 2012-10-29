namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI.SchemaTreeInternal;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    internal class SchemaTree : TreeView
    {
        private Point _lastMousePos;
        private Repository _lastNonSqlRepos;
        private TreeNode _lastSelectedNode;
        private Repository _lastSqlRepos;
        internal static System.Drawing.Font BaseFont = FontManager.GetDefaultFont();
        internal static System.Drawing.Font BoldFont = new System.Drawing.Font(BaseFont, FontStyle.Bold);
        private IContainer components;
        internal static Repository DragRepository;
        private ImageList imageList;
        internal static System.Drawing.Font ItalicFont = new System.Drawing.Font(BaseFont, FontStyle.Italic);
        internal static System.Drawing.Font UnderlineFont = new System.Drawing.Font(BaseFont, FontStyle.Underline);

        public event EventHandler CxEdited;

        public event EventHandler<NewQueryArgs> NewQuery;

        public event EventHandler<RepositoryArgs> RepositoryDeleted;

        public event EventHandler<RepositoryArgs> StaticSchemaRepositoryChanged;

        public SchemaTree()
        {
            this.InitializeComponent();
            this.Font = BoldFont;
            int num = (this.Font.Height * 6) / 5;
            if (num > 20)
            {
                this.ItemHeight = num;
            }
            base.ShowNodeToolTips = true;
            base.ImageList = TreeViewHelper.UpscaleImages(this.Font, this.imageList);
            base.HideSelection = true;
            base.Nodes.Add(new AddCxNode());
            DataContextManager.SubscribeToAllErrors(new DataContextCallback(this.DataContextGenerationError));
        }

        public void AddCx()
        {
            Repository repository2 = new Repository {
                Persist = true
            };
            if (RepositoryDialogManager.Show(repository2, true))
            {
                repository2.SaveToDisk();
                this.AddCx(repository2, true, true);
            }
        }

        public void AddCx(Repository r, bool selectNode, bool expandNode)
        {
            this.AddCx(r, null, selectNode, expandNode, false, true);
        }

        public void AddCx(Repository repos, Repository child, bool selectNode, bool expandNode, bool delaySchemaPopulation, bool updateAllNodeText)
        {
            if (repos.DriverLoader.IsValid)
            {
                RepositoryNode node;
                base.HideSelection = false;
                bool flag = false;
                if (!repos.DynamicSchema)
                {
                    node = new StaticSchemaNode(repos);
                }
                else if (repos.IsQueryable)
                {
                    node = new DynamicSchemaNode(repos);
                }
                else
                {
                    node = new ServerNode(repos);
                }
                try
                {
                    base.BeginUpdate();
                    base.Nodes.Add(node);
                    flag = (!repos.IsQueryable || expandNode) || !delaySchemaPopulation;
                    node.ExpandOnUpdate = expandNode;
                    DynamicSchemaNode node2 = null;
                    if (child != null)
                    {
                        DynamicSchemaNode node3 = new DynamicSchemaNode(child) {
                            ExpandOnUpdate = expandNode
                        };
                        node.Nodes.Add(node2 = node3);
                    }
                    if (flag)
                    {
                        node.Init();
                    }
                    if (selectNode)
                    {
                        base.SelectedNode = (node2 != null) ? node2 : node;
                    }
                    if (updateAllNodeText)
                    {
                        this.UpdateAllNodeText();
                    }
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        public void AddCxFromTemplate(Repository template)
        {
            Repository repository = template.Clone();
            repository.ID = Guid.NewGuid();
            if (RepositoryDialogManager.Show(repository, false))
            {
                repository.SaveToDisk();
                this.AddCx(repository, true, true);
            }
        }

        private void DataContextGenerationError(DataContextInfo dcInfo)
        {
            base.BeginInvoke(delegate {
                RepositoryNode node = this.GetAllRepositoryNodes(true).FirstOrDefault<RepositoryNode>(db => db.Repository == dcInfo.Repository);
                if (node != null)
                {
                    node.ShowErrorIfUninitialized(dcInfo.Error);
                }
            });
        }

        internal void Delete(RepositoryNode node)
        {
            if (node.Repository.Persist)
            {
                node.Repository.Persist = false;
                node.Repository.SaveToDisk();
            }
            base.Nodes.Remove(node);
            node.Dispose();
            RepositoryArgs args = new RepositoryArgs {
                Repository = node.Repository
            };
            this.OnRepositoryDeleted(args);
            this.UpdateAllNodeText();
        }

        internal void Edit(RepositoryNode node)
        {
            if (RepositoryDialogManager.Show(node.Repository, false))
            {
                this.UpdateRepositoryNode(node);
            }
        }

        internal IEnumerable<Repository> GetAllRepositories(bool queryableOnly)
        {
            return (from n in this.GetAllRepositoryNodes(queryableOnly) select n.Repository);
        }

        private IEnumerable<RepositoryNode> GetAllRepositoryNodes(bool queryableOnly)
        {
            IEnumerable<RepositoryNode> first = base.Nodes.OfType<RepositoryNode>();
            IEnumerable<RepositoryNode> second = from n in base.Nodes.OfType<RepositoryNode>()
                from d in n.Nodes.OfType<RepositoryNode>()
                select d;
            return (from n in first.Concat<RepositoryNode>(second)
                where !queryableOnly || n.Repository.IsQueryable
                orderby !n.Repository.DynamicSchema, n.Repository.ToString()
                select n);
        }

        public Repository GetCurrentRepository(bool queryableOnly)
        {
            TreeNode selectedNode = base.SelectedNode;
            while (!(selectedNode is RepositoryNode))
            {
                if (selectedNode == null)
                {
                    return null;
                }
                selectedNode = selectedNode.Parent;
            }
            RepositoryNode node2 = selectedNode as RepositoryNode;
            if (queryableOnly && ((node2 == null) || !node2.Repository.IsQueryable))
            {
                return null;
            }
            return node2.Repository;
        }

        private RepositoryNode GetQueryableRespositoryParent(TreeNode node)
        {
            while (node != null)
            {
                if (((node is RepositoryNode) && (((RepositoryNode) node).Repository != null)) && ((RepositoryNode) node).Repository.IsQueryable)
                {
                    return (RepositoryNode) node;
                }
                node = node.Parent;
            }
            return null;
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(SchemaTree));
            this.imageList = new ImageList(this.components);
            base.SuspendLayout();
            this.imageList.ImageStream = (ImageListStreamer) manager.GetObject("imageList.ImageStream");
            this.imageList.TransparentColor = Color.Transparent;
            this.imageList.Images.SetKeyName(0, "Hourglass");
            this.imageList.Images.SetKeyName(1, "Connection");
            this.imageList.Images.SetKeyName(2, "Database");
            this.imageList.Images.SetKeyName(3, "Table");
            this.imageList.Images.SetKeyName(4, "View");
            this.imageList.Images.SetKeyName(5, "Column");
            this.imageList.Images.SetKeyName(6, "Key");
            this.imageList.Images.SetKeyName(7, "ManyToOne");
            this.imageList.Images.SetKeyName(8, "OneToMany");
            this.imageList.Images.SetKeyName(9, "FailedConnection");
            this.imageList.Images.SetKeyName(10, "FailedDatabase");
            this.imageList.Images.SetKeyName(11, "TableFunction");
            this.imageList.Images.SetKeyName(12, "ScalarFunction");
            this.imageList.Images.SetKeyName(13, "StoredProc");
            this.imageList.Images.SetKeyName(14, "Parameter");
            this.imageList.Images.SetKeyName(15, "Add");
            this.imageList.Images.SetKeyName(0x10, "Compact");
            this.imageList.Images.SetKeyName(0x11, "Schema");
            this.imageList.Images.SetKeyName(0x12, "OneToOne");
            this.imageList.Images.SetKeyName(0x13, "ManyToMany");
            this.imageList.Images.SetKeyName(20, "EF");
            this.imageList.Images.SetKeyName(0x15, "L2S");
            this.imageList.Images.SetKeyName(0x16, "Globe");
            this.imageList.Images.SetKeyName(0x17, "CustomError");
            this.imageList.Images.SetKeyName(0x18, "Custom");
            this.imageList.Images.SetKeyName(0x19, "Inherited");
            this.imageList.Images.SetKeyName(0x1a, "AzureConnection");
            this.imageList.Images.SetKeyName(0x1b, "FailedAzureConnection");
            this.imageList.Images.SetKeyName(0x1c, "AzureDatabase");
            this.imageList.Images.SetKeyName(0x1d, "FailedAzureDatabase");
            this.imageList.Images.SetKeyName(30, "Dallas");
            this.imageList.Images.SetKeyName(0x1f, "LinkedDatabase");
            this.imageList.Images.SetKeyName(0x20, "Box");
            this.imageList.Images.SetKeyName(0x21, "Blank");
            base.ResumeLayout(false);
        }

        internal void InvokeCxEdited()
        {
            if (this.CxEdited != null)
            {
                this.CxEdited(this, EventArgs.Empty);
            }
        }

        protected override void OnAfterCollapse(TreeViewEventArgs e)
        {
            RepositoryNode node = e.Node as RepositoryNode;
            if (node != null)
            {
                node.ExpandOnUpdate = false;
            }
            base.OnAfterCollapse(e);
        }

        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            TreeNode nodeAt = base.GetNodeAt(this._lastMousePos);
            if (nodeAt == e.Node)
            {
                base.SelectedNode = e.Node;
            }
            RepositoryNode node2 = nodeAt as RepositoryNode;
            if (node2 != null)
            {
                node2.ExpandOnUpdate = true;
            }
            base.OnAfterExpand(e);
            if (e.Node is BaseNode)
            {
                ((BaseNode) e.Node).OnAfterExpand();
            }
        }

        protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
        {
            base.OnBeforeCollapse(e);
            if (e.Node is RepositoryNode)
            {
                ((RepositoryNode) e.Node).OnBeforeCollapse();
            }
        }

        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            base.OnBeforeExpand(e);
            if (e.Node is BaseNode)
            {
                ((BaseNode) e.Node).OnBeforeExpand();
            }
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            if (e.Node != null)
            {
                this._lastSelectedNode = e.Node;
            }
            base.OnBeforeSelect(e);
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            RepositoryNode item = e.Item as RepositoryNode;
            if (((item != null) && (item.Repository != null)) && item.Repository.IsQueryable)
            {
                base.SelectedNode = item;
                base.DoDragDrop(new RepositoryDragData(item.Repository), DragDropEffects.Link | DragDropEffects.Copy);
            }
            else
            {
                BaseNode node = e.Item as BaseNode;
                if ((node != null) && !string.IsNullOrEmpty(node.DragText))
                {
                    base.SelectedNode = node;
                    RepositoryNode queryableRespositoryParent = this.GetQueryableRespositoryParent(node);
                    if (queryableRespositoryParent != null)
                    {
                        DragRepository = queryableRespositoryParent.Repository;
                    }
                    try
                    {
                        base.DoDragDrop(node.DragText, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll);
                    }
                    finally
                    {
                        DragRepository = null;
                    }
                }
            }
            base.OnItemDrag(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.Cursor = Cursors.Default;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this._lastMousePos = e.Location;
            TreeNode nodeAt = base.GetNodeAt(e.Location);
            Cursor hand = Cursors.Default;
            if ((nodeAt != null) && (nodeAt.NodeFont == UnderlineFont))
            {
                hand = Cursors.Hand;
            }
            if (this.Cursor != hand)
            {
                this.Cursor = hand;
            }
            base.OnMouseMove(e);
        }

        protected internal virtual void OnNewQuery(NewQueryArgs args)
        {
            if (this.NewQuery != null)
            {
                this.NewQuery(this, args);
            }
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ShowContextMenu(e.Node, e.Location);
            }
            base.OnNodeMouseClick(e);
        }

        protected virtual void OnRepositoryDeleted(RepositoryArgs args)
        {
            if (this.RepositoryDeleted != null)
            {
                this.RepositoryDeleted(this, args);
            }
        }

        protected internal virtual void OnStaticSchemaRepositoryChanged(RepositoryArgs args)
        {
            if (this.StaticSchemaRepositoryChanged != null)
            {
                this.StaticSchemaRepositoryChanged(this, args);
            }
        }

        public void PopulateFromDisk()
        {
            base.Nodes.Clear();
            base.Nodes.Add(new AddCxNode());
            foreach (Repository repository in from c in Repository.FromDisk()
                orderby c.DriverLoader.SortOrder, c.GetSortOrder()
                select c)
            {
                this.AddCx(repository, null, false, false, true, false);
            }
            this.UpdateAllNodeText();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData == Keys.Apps) || (keyData == (Keys.Shift | Keys.F10)))
            {
                if (base.SelectedNode != null)
                {
                    Rectangle bounds = base.SelectedNode.Bounds;
                    this.ShowContextMenu(base.SelectedNode, new Point(bounds.Left + (bounds.Width / 2), bounds.Bottom));
                }
                return true;
            }
            if ((keyData == Keys.Space) || (keyData == Keys.Enter))
            {
                if (base.SelectedNode != null)
                {
                    this.ProcessLeftMouseDown(base.SelectedNode);
                }
                return true;
            }
            if (keyData == (Keys.Alt | Keys.Enter))
            {
                if (base.SelectedNode is RepositoryNode)
                {
                    this.Edit((RepositoryNode) base.SelectedNode);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private bool ProcessLeftMouseDown(TreeNode node)
        {
            var predicate = null;
            var func2 = null;
            RelationNode rNode = node as RelationNode;
            if (rNode != null)
            {
                TreeNode parent = rNode.Parent.Parent;
                if (!(parent is DynamicSchemaNode))
                {
                    parent = parent.Parent;
                }
                if (predicate == null)
                {
                    predicate = <>h__TransparentIdentifier50 => (<>h__TransparentIdentifier50.relationNode != rNode) && (<>h__TransparentIdentifier50.relationNode.Relation == rNode.Relation);
                }
                TreeNode node3 = (from <>h__TransparentIdentifier50 in (from tableNode in parent.Nodes.Cast<TreeNode>()
                    from relationNode in tableNode.Nodes.OfType<RelationNode>()
                    select new { tableNode = tableNode, relationNode = relationNode }).Where(predicate) select <>h__TransparentIdentifier50.tableNode).FirstOrDefault<TreeNode>();
                if (node3 == null)
                {
                    if (func2 == null)
                    {
                        func2 = <>h__TransparentIdentifier52 => (<>h__TransparentIdentifier52.relationNode != rNode) && (<>h__TransparentIdentifier52.relationNode.Relation == rNode.Relation);
                    }
                    node3 = (from <>h__TransparentIdentifier52 in (from schemaNode in parent.Nodes.Cast<TreeNode>()
                        from tableNode in schemaNode.Nodes.Cast<TreeNode>()
                        from relationNode in tableNode.Nodes.OfType<RelationNode>()
                        select new { <>h__TransparentIdentifier51 = <>h__TransparentIdentifier51, relationNode = relationNode }).Where(func2) select <>h__TransparentIdentifier52.<>h__TransparentIdentifier51.tableNode).FirstOrDefault<TreeNode>();
                }
                if (node3 != null)
                {
                    if (!node3.IsExpanded)
                    {
                        node3.Expand();
                    }
                    base.SelectedNode = node3;
                    if (node3.Nodes.Count > 0)
                    {
                        node3.Nodes.Cast<TreeNode>().Take<TreeNode>(7).Last<TreeNode>().EnsureVisible();
                    }
                    return true;
                }
            }
            else if (node is ExplorerItemNode)
            {
                if (((ExplorerItemNode) node).ProcessClick())
                {
                    return true;
                }
            }
            else if (node is AddCxNode)
            {
                this.AddCx();
                return true;
            }
            return false;
        }

        internal void RegisterRepository(QueryCore query, bool expand, bool onlyIfDirty)
        {
            Func<DynamicSchemaNode, bool> predicate = null;
            Func<RepositoryNode, bool> func2 = null;
            if ((query.Repository != null) && query.Repository.DriverLoader.IsValid)
            {
                RepositoryNode node = this.GetAllRepositoryNodes(true).FirstOrDefault<RepositoryNode>(db => db.Repository == query.Repository);
                if (node == null)
                {
                    Guid id = query.Repository.ID;
                    RepositoryNode node2 = base.Nodes.OfType<RepositoryNode>().FirstOrDefault<RepositoryNode>(n => n.Repository.ID == id);
                    if ((node2 != null) && node2.Repository.IsQueryable)
                    {
                        node = node2;
                    }
                    else if (node2 is ServerNode)
                    {
                        if (predicate == null)
                        {
                            predicate = db => (query.Repository != null) && (db.Repository.Database.ToLowerInvariant() == query.Repository.Database.ToLowerInvariant());
                        }
                        node = node2.Nodes.OfType<DynamicSchemaNode>().Where<DynamicSchemaNode>(predicate).FirstOrDefault<DynamicSchemaNode>();
                    }
                    if (node != null)
                    {
                        query.Repository = node.Repository;
                    }
                }
                if ((node == null) && !query.Predefined)
                {
                    if (func2 == null)
                    {
                        func2 = db => db.Repository.IsEquivalent(query.Repository);
                    }
                    node = this.GetAllRepositoryNodes(true).FirstOrDefault<RepositoryNode>(func2);
                    if (node != null)
                    {
                        query.Repository = node.Repository;
                    }
                }
                if (node != null)
                {
                    if (onlyIfDirty)
                    {
                        DynamicSchemaNode node3 = node as DynamicSchemaNode;
                        if ((node3 != null) && node3.IsDirty())
                        {
                            node.Expand();
                        }
                        return;
                    }
                    base.SelectedNode = node;
                    if (!(node.IsExpanded || !expand))
                    {
                        node.Expand();
                    }
                }
                if ((node == null) && (query.Repository != null))
                {
                    if (!((query.Repository.DynamicSchema && (query.Repository.Database.Length != 0)) && query.Repository.ShowServer))
                    {
                        this.AddCx(query.Repository, true, expand);
                    }
                    else
                    {
                        Repository repos = query.Repository.CreateParent();
                        this.AddCx(repos, query.Repository, true, expand, false, true);
                    }
                }
            }
        }

        public void ReselectRepository()
        {
            if (this._lastSelectedNode != null)
            {
                try
                {
                    base.SelectedNode = this._lastSelectedNode;
                }
                catch
                {
                }
            }
        }

        private void ShowContextMenu(TreeNode node, Point location)
        {
            ToolStripItemClickedEventHandler handler = null;
            base.SelectedNode = node;
            ContextMenuStrip m = null;
            if (node is RepositoryNode)
            {
                m = new NodeContextMenu(this, (RepositoryNode) node);
            }
            else if (node is ExplorerItemNode)
            {
                ExplorerItemNode node2 = (ExplorerItemNode) node;
                if ((node2.ExplorerItem.Kind == ExplorerItemKind.QueryableObject) && node2.ExplorerItem.IsEnumerable)
                {
                    m = new ExplorerItemContextMenu(this, node2);
                }
            }
            if (m != null)
            {
                m.Show(this, location);
                if (handler == null)
                {
                    handler = (sender, e) => m.Dispose();
                }
                m.ItemClicked += handler;
            }
        }

        public void UnselectRepository()
        {
            if (base.SelectedNode != null)
            {
                this._lastSelectedNode = base.SelectedNode;
                base.SelectedNode = null;
            }
        }

        internal void UpdateAllNodeText()
        {
            base.BeginUpdate();
            try
            {
                this.UpdateFullQualificationFlags();
                foreach (StaticSchemaNode node in base.Nodes.OfType<StaticSchemaNode>())
                {
                    node.UpdateText();
                }
            }
            finally
            {
                base.EndUpdate();
            }
        }

        private void UpdateFullQualificationFlags()
        {
            IEnumerable<Repository> enumerable = from n in base.Nodes.OfType<StaticSchemaNode>() select n.Repository;
            Repository.TypesNeedingAssemblyQualification = (from <>h__TransparentIdentifier2a in from r in enumerable
                where !r.DynamicSchema
                let dotPos = r.CustomTypeName.LastIndexOf('.')
                where dotPos > 0
                select <>h__TransparentIdentifier2a
                group <>h__TransparentIdentifier2a.r by <>h__TransparentIdentifier2a.r.CustomTypeName.Substring(<>h__TransparentIdentifier2a.dotPos + 1) into g
                where g.Count<Repository>() > 1
                select new { Key = g.Key, ByAssembly = from r in g group r by r.CustomAssemblyPath }).ToDictionary(r => r.Key, r => r.ByAssembly.Any<IGrouping<string, Repository>>(a => a.Count<Repository>() > 1));
            Func<string, string> tryGetFileName = delegate (string s) {
                try
                {
                    return Path.GetFileName(s);
                }
                catch
                {
                    return null;
                }
            };
            Repository.AssembliesNeedingPathQualification = (from r in from r in enumerable
                where !r.DynamicSchema
                select r
                group r.CustomAssemblyPath by tryGetFileName(r.CustomAssemblyPath) into g
                where g.Distinct<string>().Count<string>() > 1
                select g.Key).ToArray<string>();
        }

        internal void UpdateRepository(Repository r)
        {
            RepositoryNode node = this.GetAllRepositoryNodes(true).FirstOrDefault<RepositoryNode>(db => db.Repository == r);
            if (node != null)
            {
                this.UpdateRepositoryNode(node);
            }
        }

        private void UpdateRepositoryNode(RepositoryNode node)
        {
            node.Repository.SaveToDisk();
            string descriptor = node.Repository.DriverLoader.GetDescriptor();
            try
            {
                base.BeginUpdate();
                if (node.Repository.DriverLoader.GetDescriptor() != descriptor)
                {
                    base.Nodes.Remove(node);
                    node.Dispose();
                    this.AddCx(node.Repository, true, true);
                }
                else
                {
                    node.BaseText = node.Text = node.Name = node.Repository.GetFriendlyName();
                    node.RefreshData(true);
                    foreach (TreeNode node2 in node.Nodes)
                    {
                        DynamicSchemaNode node3 = node2 as DynamicSchemaNode;
                        if (node3 != null)
                        {
                            node3.Repository.UpdateFromParent();
                            DataContextManager.RefreshIfInUse(node3.Repository);
                        }
                    }
                }
                if (this.CxEdited != null)
                {
                    this.CxEdited(this, EventArgs.Empty);
                }
                this.UpdateAllNodeText();
            }
            finally
            {
                base.EndUpdate();
            }
        }

        internal void UpdateSqlMode(QueryCore query)
        {
            if (UserOptions.Instance.DefaultQueryLanguage.HasValue && (((QueryLanguage) UserOptions.Instance.DefaultQueryLanguage.Value) == QueryLanguage.SQL))
            {
                this.UpdateSqlModeForSqlDefault(query);
            }
            else
            {
                this.UpdateSqlModeForNonSqlDefault(query);
            }
        }

        private void UpdateSqlModeForNonSqlDefault(QueryCore query)
        {
            Func<RepositoryNode, bool> predicate = null;
            Func<RepositoryNode, bool> func2 = null;
            Repository sqlRepos = null;
            if ((query != null) && (query.QueryKind == QueryLanguage.SQL))
            {
                sqlRepos = query.Repository;
            }
            if (sqlRepos != this._lastSqlRepos)
            {
                try
                {
                    base.BeginUpdate();
                    if (this._lastSqlRepos != null)
                    {
                        if (predicate == null)
                        {
                            predicate = db => db.Repository == this._lastSqlRepos;
                        }
                        RepositoryNode node = this.GetAllRepositoryNodes(true).FirstOrDefault<RepositoryNode>(predicate);
                        if (node != null)
                        {
                            node.SetSqlMode(false);
                        }
                    }
                    this._lastSqlRepos = sqlRepos;
                    if (sqlRepos != null)
                    {
                        if (func2 == null)
                        {
                            func2 = db => db.Repository == sqlRepos;
                        }
                        RepositoryNode node2 = this.GetAllRepositoryNodes(true).FirstOrDefault<RepositoryNode>(func2);
                        if (node2 != null)
                        {
                            node2.SetSqlMode(true);
                        }
                    }
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        private void UpdateSqlModeForSqlDefault(QueryCore query)
        {
            Func<RepositoryNode, bool> predicate = null;
            Func<RepositoryNode, bool> func2 = null;
            Repository nonSqlRepos = null;
            if ((query != null) && (query.QueryKind != QueryLanguage.SQL))
            {
                nonSqlRepos = query.Repository;
            }
            if (nonSqlRepos != this._lastNonSqlRepos)
            {
                try
                {
                    base.BeginUpdate();
                    if (this._lastNonSqlRepos != null)
                    {
                        if (predicate == null)
                        {
                            predicate = db => db.Repository == this._lastNonSqlRepos;
                        }
                        RepositoryNode node = this.GetAllRepositoryNodes(true).FirstOrDefault<RepositoryNode>(predicate);
                        if (node != null)
                        {
                            node.SetSqlMode(true);
                        }
                    }
                    this._lastNonSqlRepos = nonSqlRepos;
                    if (nonSqlRepos != null)
                    {
                        if (func2 == null)
                        {
                            func2 = db => db.Repository == nonSqlRepos;
                        }
                        RepositoryNode node2 = this.GetAllRepositoryNodes(true).FirstOrDefault<RepositoryNode>(func2);
                        if (node2 != null)
                        {
                            node2.SetSqlMode(false);
                        }
                    }
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x201)
            {
                Point pt = base.PointToClient(Control.MousePosition);
                TreeNode nodeAt = base.GetNodeAt(pt);
                if (this.ProcessLeftMouseDown(nodeAt))
                {
                    return;
                }
            }
            base.WndProc(ref m);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Drawing.Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ItemHeight
        {
            get
            {
                return base.ItemHeight;
            }
            set
            {
                base.ItemHeight = value;
            }
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;
                if (value != null)
                {
                    base.Nodes.Clear();
                }
            }
        }
    }
}

