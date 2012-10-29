namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.ObjectModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    internal class MyQueries : TreeView
    {
        private bool _addingNodes;
        private HashSet<string> _expandedPaths = new HashSet<string>();
        private Dictionary<string, FileNode> _fileNodes = new Dictionary<string, FileNode>(StringComparer.OrdinalIgnoreCase);
        private string _lastMoveFilePath;
        private DateTime _lastMoveUtc;
        private string _lastQueryFolderRoot;
        private string _lastSelectedFilePath;
        private TreeNode _mouseDownNode;
        private MyExtensionsNode _myExtensionsNode;
        private QueryCore _selectedQuery;
        private bool _unshown = true;
        private FileSystemWatcher _watcher;
        private IContainer components;
        private ImageList imageList;

        public event Action<string, string> QueryPotentiallyMoved;

        public event RenamedEventHandler QueryRenamed;

        public MyQueries()
        {
            this.Font = FontManager.GetDefaultFont();
            this.InitializeComponent();
            this.InitWatcher();
            base.ImageList = TreeViewHelper.UpscaleImages(this.Font, this.imageList);
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (this.IsChangeInteresting(e.FullPath))
                {
                    base.BeginInvoke(new MethodInvoker(this.TryRefreshTree));
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception, "MyQueries waiter changed");
            }
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            MethodInvoker method = null;
            try
            {
                if (this.IsChangeInteresting(e.FullPath))
                {
                    base.BeginInvoke(new MethodInvoker(this.TryRefreshTree));
                    if (((this._lastMoveFilePath != null) && (this._lastMoveUtc > DateTime.UtcNow.AddSeconds(-2.0))) && (Path.GetFileName(this._lastMoveFilePath) == Path.GetFileName(e.FullPath)))
                    {
                        if (method == null)
                        {
                            method = () => this.QueryPotentiallyMoved(this._lastMoveFilePath, e.FullPath);
                        }
                        base.BeginInvoke(method);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception, "MyQueries waiter created");
            }
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (this.IsChangeInteresting(e.FullPath))
                {
                    this._lastMoveUtc = DateTime.UtcNow;
                    this._lastMoveFilePath = e.FullPath;
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception, "MyQueries waiter deleted");
            }
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            MethodInvoker method = null;
            try
            {
                if (this.IsChangeInteresting(e.OldFullPath) || this.IsChangeInteresting(e.FullPath))
                {
                    if (method == null)
                    {
                        method = delegate {
                            this.TryRefreshTree();
                            this.QueryRenamed(this, e);
                        };
                    }
                    base.BeginInvoke(method);
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception, "MyQueries waiter renamed");
            }
        }

        private void BuildExpandedPaths(TreeNodeCollection nodes)
        {
            foreach (FileSystemNode node in nodes.OfType<FileSystemNode>())
            {
                if (node.IsExpanded)
                {
                    this._expandedPaths.Add(node.FilePath);
                    this.BuildExpandedPaths(node.Nodes);
                }
            }
        }

        public void CheckForQueryFolderChange()
        {
            Options.RefreshDefaultQueryFolder();
            string defaultQueryFolder = Options.GetDefaultQueryFolder(true);
            if (defaultQueryFolder != this._lastQueryFolderRoot)
            {
                this._lastQueryFolderRoot = defaultQueryFolder;
                if (this._watcher == null)
                {
                    this.InitWatcher();
                    try
                    {
                        this._watcher.EnableRaisingEvents = true;
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }
                else if (defaultQueryFolder == null)
                {
                    this._watcher.Dispose();
                    this._watcher = null;
                }
                else
                {
                    this._watcher.Path = defaultQueryFolder;
                }
                this.RefreshTree();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this._watcher != null)
            {
                this._watcher.Dispose();
            }
            if (this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private FileNode FindQueryByPath(string path)
        {
            Func<FileNode, bool> predicate = null;
            FileNode node;
            try
            {
                if (!path.StartsWith(Options.DefaultQueryFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                path = path.Substring(Options.DefaultQueryFolder.Length);
                if (path.StartsWith(@"\"))
                {
                    path = path.Substring(1);
                }
                TreeNodeCollection source = base.Nodes[0].Nodes;
                while (path.Contains(@"\"))
                {
                    string dir = path.Substring(0, path.IndexOf('\\'));
                    FolderNode node2 = source.OfType<FolderNode>().FirstOrDefault<FolderNode>(n => string.Equals(n.Text, dir, StringComparison.OrdinalIgnoreCase));
                    if (node2 == null)
                    {
                        goto Label_01AF;
                    }
                    if ((node2.Nodes.Count == 1) && (node2.Nodes[0].Text == ""))
                    {
                        node2.Nodes.Clear();
                        this.RecurseFileSystem(node2.Nodes, node2.Directory);
                    }
                    path = path.Substring(path.IndexOf('\\') + 1);
                    source = node2.Nodes;
                }
                if (path.Length == 0)
                {
                    return null;
                }
                path = Path.GetFileName(path);
                if (predicate == null)
                {
                    predicate = n => string.Equals(n.FileName, path, StringComparison.OrdinalIgnoreCase);
                }
                return source.OfType<FileNode>().FirstOrDefault<FileNode>(predicate);
            Label_01AF:
                node = null;
            }
            catch (Exception exception)
            {
                Log.Write(exception);
                node = null;
            }
            return node;
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(MyQueries));
            this.imageList = new ImageList(this.components);
            base.SuspendLayout();
            this.imageList.ImageStream = (ImageListStreamer) manager.GetObject("imageList.ImageStream");
            this.imageList.TransparentColor = Color.Transparent;
            this.imageList.Images.SetKeyName(0, "Folder");
            this.imageList.Images.SetKeyName(1, "FolderOpen");
            this.imageList.Images.SetKeyName(2, "Query");
            this.imageList.Images.SetKeyName(3, "Add");
            base.HideSelection = false;
            base.ResumeLayout(false);
        }

        private void InitWatcher()
        {
            string defaultQueryFolder = Options.GetDefaultQueryFolder(true);
            if (defaultQueryFolder != null)
            {
                try
                {
                    this._watcher = new FileSystemWatcher(defaultQueryFolder);
                }
                catch
                {
                    return;
                }
                this._watcher.IncludeSubdirectories = true;
                this._watcher.Created += new FileSystemEventHandler(this._watcher_Created);
                this._watcher.Renamed += new RenamedEventHandler(this._watcher_Renamed);
                this._watcher.Deleted += new FileSystemEventHandler(this._watcher_Deleted);
                this._watcher.Changed += new FileSystemEventHandler(this._watcher_Changed);
                this._watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
            }
        }

        private bool IsChangeInteresting(string path)
        {
            if ((Directory.Exists(path) || path.EndsWith(@"\", StringComparison.Ordinal)) || (base.Nodes.Find(path.ToLowerInvariant(), true).Length > 0))
            {
                return true;
            }
            string str = Path.GetExtension(path).ToLowerInvariant();
            return ((str == ".linq") || (str == ".lin"));
        }

        protected override void OnAfterCollapse(TreeViewEventArgs e)
        {
            e.Node.ImageKey = e.Node.SelectedImageKey = "Folder";
            base.OnAfterCollapse(e);
        }

        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            e.Node.ImageKey = e.Node.SelectedImageKey = "FolderOpen";
            base.OnAfterExpand(e);
        }

        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            FolderNode node = e.Node as FolderNode;
            if (((!this._addingNodes && (node != null)) && (node.Nodes.Count == 1)) && (node.Nodes[0].Text == ""))
            {
                node.Nodes.Clear();
                this.RecurseFileSystem(node.Nodes, node.Directory);
            }
            base.OnBeforeExpand(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this._mouseDownNode = base.GetNodeAt(e.X, e.Y);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this._mouseDownNode = null;
        }

        protected override void OnNodeMouseDoubleClick(TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == this._mouseDownNode)
            {
                base.OnNodeMouseDoubleClick(e);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (base.Visible && this._unshown)
            {
                this._unshown = false;
                if (!base.DesignMode)
                {
                    this.RefreshTree();
                    if (this._watcher != null)
                    {
                        try
                        {
                            this._watcher.EnableRaisingEvents = true;
                        }
                        catch (FileNotFoundException)
                        {
                        }
                    }
                }
            }
            base.OnVisibleChanged(e);
        }

        private void RecurseFileSystem(TreeNodeCollection nodes, DirectoryInfo dir)
        {
            List<FileSystemNode> list = new List<FileSystemNode>();
            IEnumerable<DirectoryInfo> enumerable = new DirectoryInfo[0];
            try
            {
                enumerable = from d in dir.GetDirectories()
                    orderby ImportedSampleQuery.Get3rdPartySort(d.Name.ToUpperInvariant())
                    select d;
            }
            catch
            {
            }
            foreach (DirectoryInfo info in enumerable)
            {
                if ((info.Attributes & FileAttributes.Hidden) == 0)
                {
                    FolderNode item = new FolderNode(info);
                    if (this._expandedPaths.Contains(info.FullName))
                    {
                        this.RecurseFileSystem(item.Nodes, info);
                        item.Expand();
                    }
                    else
                    {
                        item.Nodes.Add("");
                    }
                    if (item.FilePath == this._lastSelectedFilePath)
                    {
                        base.SelectedNode = item;
                    }
                    list.Add(item);
                }
            }
            IEnumerable<FileInfo> enumerable2 = new FileInfo[0];
            try
            {
                enumerable2 = from f in dir.GetFiles("*.linq")
                    orderby ImportedSampleQuery.Get3rdPartySort(f.Name.ToUpperInvariant())
                    select f;
            }
            catch
            {
            }
            foreach (FileInfo info2 in enumerable2)
            {
                if ((info2.Attributes & FileAttributes.Hidden) == 0)
                {
                    FileNode node2 = new FileNode(info2.FullName);
                    list.Add(node2);
                    this._fileNodes[info2.FullName] = node2;
                    if (info2.FullName == this._lastSelectedFilePath)
                    {
                        base.SelectedNode = node2;
                    }
                }
            }
            this._addingNodes = true;
            try
            {
                nodes.AddRange(list.ToArray());
            }
            finally
            {
                this._addingNodes = false;
            }
        }

        public void RefreshTree()
        {
            string defaultQueryFolder = Options.GetDefaultQueryFolder(true);
            this._lastSelectedFilePath = null;
            if (base.SelectedNode is FileSystemNode)
            {
                this._lastSelectedFilePath = ((FileSystemNode) base.SelectedNode).FilePath;
            }
            this._expandedPaths.Clear();
            this.BuildExpandedPaths(base.Nodes);
            this._fileNodes.Clear();
            base.Nodes.Clear();
            TreeNode node = base.Nodes.Add(Options.IsDefaultQueryFolder ? "My Queries" : Options.DefaultQueryFolder);
            if (defaultQueryFolder != null)
            {
                DirectoryInfo dir = new DirectoryInfo(defaultQueryFolder);
                this.RecurseFileSystem(node.Nodes, dir);
                if ((base.SelectedNode == null) && (this._selectedQuery != null))
                {
                    this.UpdateSelection(this._selectedQuery);
                }
                base.Nodes.Add(this._myExtensionsNode = new MyExtensionsNode());
                node.Expand();
            }
        }

        private void TryRefreshTree()
        {
            try
            {
                this.RefreshTree();
            }
            catch
            {
            }
        }

        internal void UpdateSelection(QueryCore query)
        {
            this._selectedQuery = query;
            if (((query == null) || query.Predefined) || (query.FilePath.Length == 0))
            {
                if (base.SelectedNode != null)
                {
                    base.SelectedNode = null;
                }
            }
            else if ((this._myExtensionsNode != null) && (query.FilePath == this._myExtensionsNode.FilePath))
            {
                base.SelectedNode = this._myExtensionsNode;
            }
            else if (!this._fileNodes.ContainsKey(query.FilePath))
            {
                FileNode node = this.FindQueryByPath(query.FilePath);
                if (node != base.SelectedNode)
                {
                    base.SelectedNode = node;
                }
            }
            else
            {
                FileNode node2 = this._fileNodes[query.FilePath];
                if (node2 != base.SelectedNode)
                {
                    base.SelectedNode = node2;
                }
            }
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

        internal class FileNode : MyQueries.FileSystemNode
        {
            private readonly string _filePath;

            public FileNode(string filePath)
            {
                this._filePath = filePath;
                base.Text = Path.GetFileNameWithoutExtension(filePath);
                base.Name = filePath.ToLowerInvariant();
                base.ImageKey = base.SelectedImageKey = "Query";
            }

            public string FileName
            {
                get
                {
                    return Path.GetFileName(this.FilePath);
                }
            }

            public override string FilePath
            {
                get
                {
                    return this._filePath;
                }
            }
        }

        internal abstract class FileSystemNode : TreeNode
        {
            public abstract string FilePath { get; }
        }

        internal class FolderNode : MyQueries.FileSystemNode
        {
            public readonly DirectoryInfo Directory;

            public FolderNode(DirectoryInfo dir)
            {
                this.Directory = dir;
                base.Text = dir.Name;
                base.Name = dir.FullName.ToLowerInvariant();
                if (base.Name.EndsWith(@"\"))
                {
                    base.Name = base.Name.Substring(0, base.Name.Length - 1);
                }
                base.ImageKey = base.SelectedImageKey = "Folder";
            }

            public override string FilePath
            {
                get
                {
                    return this.Directory.FullName;
                }
            }
        }

        internal class MyExtensionsNode : MyQueries.FileNode
        {
            public MyExtensionsNode() : base("My Extensions.linq")
            {
                base.Name = this.FilePath.ToLowerInvariant();
            }

            public override string FilePath
            {
                get
                {
                    return MyExtensions.QueryFilePath;
                }
            }
        }
    }
}

