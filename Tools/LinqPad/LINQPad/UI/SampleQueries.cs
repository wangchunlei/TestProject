namespace LINQPad.UI
{
    using Ionic.Zip;
    using LINQPad;
    using LINQPad.ObjectModel;
    using LINQPad.Properties;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Linq;

    internal class SampleQueries : TreeView
    {
        private TreeNode _mouseDownNode;
        private static Random _random = new Random();
        private Dictionary<string, TreeNode> _samplesByName = new Dictionary<string, TreeNode>();
        private static Regex _sortPrefixWithBrackets = new Regex(@"^\[[0-9\.]+\]\s*");
        private bool _unshown = true;
        private IContainer components;
        private ImageList imageList;

        public SampleQueries()
        {
            this.Font = FontManager.GetDefaultFont();
            this.InitializeComponent();
            base.ImageList = TreeViewHelper.UpscaleImages(this.Font, this.imageList);
        }

        private void DownloadSamples(string uri, string libName)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), @"LINQPad\TempSampleQueries" + _random.Next(0xf4240) + ".zip");
            BackgroundWorker worker = new BackgroundWorker {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            worker.DoWork += delegate (object sender, DoWorkEventArgs e) {
                string path = Path.GetDirectoryName(tempPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                WebClient webClient = WebHelper.GetWebClient();
                webClient.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e) {
                    if (worker.IsBusy)
                    {
                        worker.ReportProgress(e.ProgressPercentage);
                    }
                };
                webClient.DownloadFileAsync(new Uri(uri), tempPath);
                while (webClient.IsBusy)
                {
                    if (worker.CancellationPending)
                    {
                        webClient.CancelAsync();
                        break;
                    }
                    Thread.Sleep(100);
                }
            };
            using (WorkerForm form = new WorkerForm(worker, "Downloading...", true))
            {
                if (form.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }
            try
            {
                if (string.IsNullOrEmpty(libName))
                {
                    try
                    {
                        libName = Path.GetFileNameWithoutExtension(new Uri(uri).Segments.Last<string>());
                    }
                    catch
                    {
                    }
                }
                if (this.UnzipFile(tempPath, libName))
                {
                    MessageBox.Show("Samples successfully loaded.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception, "Unzip samples");
                MessageBox.Show("Error while unpacking sample queries: " + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        internal TreeNode FindByID(string id)
        {
            if (this._unshown)
            {
                this._unshown = false;
                this.LoadData();
            }
            return this.FindByID(base.Nodes, id);
        }

        private TreeNode FindByID(TreeNodeCollection nodes, string id)
        {
            QueryNode node = nodes.OfType<QueryNode>().FirstOrDefault<QueryNode>(n => n.ID == id);
            if (node != null)
            {
                return node;
            }
            foreach (DirectoryNode node3 in nodes.OfType<DirectoryNode>())
            {
                if (node3.Nodes.Count == 0)
                {
                    return null;
                }
                TreeNode node4 = this.FindByID(node3.Nodes, id);
                if (node4 != null)
                {
                    return node4;
                }
            }
            return null;
        }

        internal IEnumerable<RunnableQuery> GetAllQueries(TreeNodeCollection nodes)
        {
            return new <GetAllQueries>d__12(-2) { <>4__this = this, <>3__nodes = nodes };
        }

        private TreeNode GetDirectoryNode(TreeNodeCollection nodes, string path)
        {
            while (true)
            {
                string[] strArray = path.Split(new char[] { '/' }, 2);
                string text = _sortPrefixWithBrackets.Replace(strArray[0], "");
                TreeNode node = nodes[text];
                if (node == null)
                {
                    node = new ThirdPartyDirectoryNode(text);
                    if (nodes == base.Nodes)
                    {
                        base.Nodes.Insert(base.Nodes.Count - 2, node);
                    }
                    else
                    {
                        nodes.Add(node);
                    }
                }
                if (!strArray[1].Contains("/"))
                {
                    return node;
                }
                nodes = node.Nodes;
                path = strArray[1];
            }
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

        private void Load(TreeNodeCollection parent, string name, string id, string content)
        {
            name = name.Substring(3);
            int num = name.Count<char>(c => c == '.');
            if (num != 0)
            {
                if (num == 1)
                {
                    QueryNode node = new QueryNode(Path.GetFileNameWithoutExtension(name), content, id);
                    parent.Add(node);
                }
                else
                {
                    string[] strArray = name.Split(new char[] { '.' }, 2);
                    TreeNode node2 = parent[strArray[0]];
                    if (node2 == null)
                    {
                        node2 = new DirectoryNode(strArray[0]);
                        parent.Add(node2);
                    }
                    this.Load(node2.Nodes, strArray[1], id, content);
                }
            }
        }

        private void Load3rdPartyData()
        {
            string path = Path.Combine(Program.UserDataFolder, "Samples");
            if (Directory.Exists(path))
            {
                foreach (string str2 in Directory.GetDirectories(path))
                {
                    string str3 = Path.Combine(str2, "queries.zip");
                    if (File.Exists(str3))
                    {
                        try
                        {
                            this.Load3rdPartySamples(str3);
                        }
                        catch (Exception exception)
                        {
                            Log.Write(exception, "SampleQueries Load3rdPartyData");
                        }
                    }
                }
            }
        }

        private void Load3rdPartySamples(string target)
        {
            if (File.Exists(target))
            {
                string name = new DirectoryInfo(Path.GetDirectoryName(target)).Name;
                if (base.Nodes[name] != null)
                {
                    base.Nodes.Remove(base.Nodes[name]);
                }
                using (ZipFile file = new ZipFile(target))
                {
                    foreach (ZipEntry entry in from i in file
                        where !i.get_IsDirectory() && (Path.GetExtension(i.get_FileName()).ToLowerInvariant() == ".linq")
                        orderby ImportedSampleQuery.Get3rdPartySort(i.get_FileName().ToUpperInvariant())
                        select i)
                    {
                        this.GetDirectoryNode(base.Nodes, name + "/" + entry.get_FileName()).Nodes.Add(new ThirdPartyQueryNode(ImportedSampleQuery.Get3rdPartyFileName(entry.get_FileName()), target, entry.get_FileName()));
                    }
                }
            }
        }

        private void LoadData()
        {
            string samplesPrefix = "LINQPad.SampleQueries4.";
            Assembly assembly = base.GetType().Assembly;
            foreach (string str in from s in assembly.GetManifestResourceNames()
                where s.StartsWith(samplesPrefix, StringComparison.Ordinal)
                orderby s
                select s)
            {
                using (Stream stream = assembly.GetManifestResourceStream(str))
                {
                    this.Load(base.Nodes, str.Substring(samplesPrefix.Length).Replace("_", " ").Replace("POINT", "\x00b7").Replace("CSharp", "C#").Replace("FSharp", "F#").Replace("HYPHEN", "-").Replace("BANG", "!"), str, new StreamReader(stream).ReadToEnd());
                }
            }
            base.Nodes.Add(new BrowseLibrariesNode());
            base.Nodes.Add(new SearchSamplesNode());
            this.Load3rdPartyData();
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this._mouseDownNode = base.GetNodeAt(e.X, e.Y);
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.Cursor = Cursors.Default;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            TreeNode nodeAt = base.GetNodeAt(e.Location);
            Cursor hand = Cursors.Default;
            if ((nodeAt != null) && (nodeAt.NodeFont == SchemaTree.UnderlineFont))
            {
                hand = Cursors.Hand;
            }
            if (this.Cursor != hand)
            {
                this.Cursor = hand;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this._mouseDownNode = null;
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            EventHandler onClick = null;
            if ((e.Node is BrowseLibrariesNode) && (e.Button == MouseButtons.Left))
            {
                using (BrowseSamplesForm form = new BrowseSamplesForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        this.DownloadSamples(form.SamplesUri, form.LibraryName);
                    }
                }
            }
            if ((e.Node is SearchSamplesNode) && (e.Button == MouseButtons.Left))
            {
                using (SearchQueries queries = new SearchQueries(true))
                {
                    queries.ShowDialog(MainForm.Instance);
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                ToolStripItemClickedEventHandler handler2 = null;
                base.SelectedNode = e.Node;
                ContextMenuStrip m = new ContextMenuStrip();
                if ((e.Node is ThirdPartyDirectoryNode) && (e.Node.Parent == null))
                {
                    if (onClick == null)
                    {
                        onClick = delegate (object sender, EventArgs e) {
                            string str = Path.Combine(Program.UserDataFolder, @"Samples\" + e.Node.Text);
                            string path = Path.Combine(str, "queries.zip");
                            try
                            {
                                File.Delete(path);
                                Directory.Delete(str, true);
                            }
                            catch
                            {
                            }
                            this.Nodes.Remove(e.Node);
                        };
                    }
                    m.Items.Add("Delete", Resources.Delete, onClick);
                }
                if (m.Items.Count > 0)
                {
                    m.Show(this, e.Location);
                    if (handler2 == null)
                    {
                        handler2 = (sender, e) => m.Dispose();
                    }
                    m.ItemClicked += handler2;
                }
            }
            base.OnNodeMouseClick(e);
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
                    this.LoadData();
                }
            }
            base.OnVisibleChanged(e);
        }

        internal void RefreshThirdPartySamples(string name)
        {
            this.Load3rdPartySamples(Path.Combine(Program.UserDataFolder, @"Samples\" + name + @"\queries.zip"));
        }

        private bool UnzipFile(string tempZipPath, string libName)
        {
            ZipFile file;
            bool flag2;
            string _name = libName;
            using (file = new ZipFile(tempZipPath))
            {
                ZipEntry entry = file.get_Item("header.xml");
                if (entry != null)
                {
                    MemoryStream stream = new MemoryStream();
                    entry.Extract(stream);
                    stream.Position = 0L;
                    string str = (string) XElement.Load(new StreamReader(stream)).Element("Name");
                    if (!string.IsNullOrEmpty(str))
                    {
                        _name = str;
                    }
                }
            }
            if (string.IsNullOrEmpty(_name))
            {
                MessageBox.Show("Sample library metadata file header.xml is not present or missing the Name element.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            _name = _name.Trim();
            new Thread(delegate {
                try
                {
                    WebHelper.GetWebClient().DownloadString("http://www.linqpad.net/RichClient/GetSample.aspx?lib=" + Uri.EscapeDataString(_name));
                }
                catch
                {
                }
            }) { IsBackground = true, Priority = ThreadPriority.Lowest, Name = "GetSample" }.Start();
            string targetFolder = Path.Combine(Program.UserDataFolder, @"Samples\" + _name);
            string path = Path.Combine(targetFolder, "queries.zip");
            if (File.Exists(path))
            {
                if (MessageBox.Show("This library has already been downloaded. Overwrite?", "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return false;
                }
                new FileInfo(path).IsReadOnly = false;
            }
            else if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            WorkingForm form = new WorkingForm("Extracting files...", 0x1388);
            form.Show();
            form.Update();
            try
            {
                File.Copy(tempZipPath, path, true);
                List<FileRetryForm.FileActionInfo> actions = new List<FileRetryForm.FileActionInfo>();
                using (file = new ZipFile(path))
                {
                    foreach (ZipEntry entry2 in file)
                    {
                        if (!((entry2.get_IsDirectory() || entry2.get_LocalFileName().Contains("/")) || entry2.get_LocalFileName().ToLowerInvariant().EndsWith(".linq")))
                        {
                            ZipEntry localEntry = entry2;
                            actions.Add(new FileRetryForm.FileActionInfo(localEntry.get_FileName(), delegate {
                                localEntry.Extract(targetFolder, true);
                            }));
                        }
                    }
                    flag2 = FileRetryForm.TryProcessFiles(actions, new Action(form.Close));
                }
            }
            finally
            {
                form.Close();
            }
            try
            {
                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }
            }
            catch
            {
            }
            if (!string.IsNullOrEmpty(_name))
            {
                MainForm.Instance.RefreshThirdPartySamples(_name);
            }
            return flag2;
        }

        internal void UpdateSelection(QueryCore query)
        {
            if (((query == null) || !query.Predefined) || (query.UISource == null))
            {
                if (base.SelectedNode != null)
                {
                    base.SelectedNode = null;
                }
            }
            else
            {
                QueryNode uISource = query.UISource as QueryNode;
                if (((uISource != base.SelectedNode) && (uISource != null)) && (base.SelectedNode != uISource))
                {
                    base.SelectedNode = uISource;
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

        [CompilerGenerated]
        private sealed class <GetAllQueries>d__12 : IEnumerable<RunnableQuery>, IEnumerable, IEnumerator<RunnableQuery>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private RunnableQuery <>2__current;
            public TreeNodeCollection <>3__nodes;
            public SampleQueries <>4__this;
            public IEnumerator<SampleQueries.QueryNode> <>7__wrap17;
            public IEnumerator<SampleQueries.DirectoryNode> <>7__wrap18;
            public IEnumerator<RunnableQuery> <>7__wrap19;
            private int <>l__initialThreadId;
            public SampleQueries.QueryNode <node>5__13;
            public SampleQueries.DirectoryNode <node>5__15;
            public RunnableQuery <q>5__14;
            public RunnableQuery <query>5__16;
            public SampleQueries.<>c__DisplayClass10 CS$<>8__locals11;
            public Func<char, bool> CS$<>9__CachedAnonymousMethodDelegatef;
            public TreeNodeCollection nodes;

            [DebuggerHidden]
            public <GetAllQueries>d__12(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    int num;
                    bool flag = true;
                    switch (this.<>1__state)
                    {
                        case 1:
                            break;

                        case 2:
                            goto Label_0191;

                        case -1:
                            return false;

                        default:
                            if (this.$__disposing)
                            {
                                return false;
                            }
                            this.CS$<>9__CachedAnonymousMethodDelegatef = null;
                            this.CS$<>8__locals11 = new SampleQueries.<>c__DisplayClass10();
                            this.CS$<>8__locals11.invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
                            this.<>7__wrap17 = this.nodes.OfType<SampleQueries.QueryNode>().GetEnumerator();
                            break;
                    }
                    try
                    {
                        num = this.<>1__state;
                        if (num == 1)
                        {
                            if (this.$__disposing)
                            {
                                return false;
                            }
                            this.<>1__state = 0;
                        }
                        if (this.<>7__wrap17.MoveNext())
                        {
                            this.<node>5__13 = this.<>7__wrap17.Current;
                            this.<q>5__14 = new RunnableQuery();
                            this.<q>5__14.OpenSample(this.<node>5__13.Name, this.<node>5__13.Content);
                            if (this.CS$<>9__CachedAnonymousMethodDelegatef == null)
                            {
                                this.CS$<>9__CachedAnonymousMethodDelegatef = new Func<char, bool>(this.CS$<>8__locals11.<GetAllQueries>b__e);
                            }
                            this.<q>5__14.Name = new string(this.<node>5__13.Text.Where<char>(this.CS$<>9__CachedAnonymousMethodDelegatef).ToArray<char>());
                            this.<>2__current = this.<q>5__14;
                            this.<>1__state = 1;
                            flag = false;
                            return true;
                        }
                    }
                    finally
                    {
                        if (flag && (this.<>7__wrap17 != null))
                        {
                            this.<>7__wrap17.Dispose();
                        }
                    }
                    this.<>7__wrap18 = this.nodes.OfType<SampleQueries.DirectoryNode>().GetEnumerator();
                Label_0191:
                    try
                    {
                        num = this.<>1__state;
                        if (num != 2)
                        {
                            goto Label_025B;
                        }
                    Label_01A2:
                        try
                        {
                            num = this.<>1__state;
                            if (num == 2)
                            {
                                if (this.$__disposing)
                                {
                                    return false;
                                }
                                this.<>1__state = 0;
                            }
                            if (!this.<>7__wrap19.MoveNext())
                            {
                                goto Label_025B;
                            }
                            this.<query>5__16 = this.<>7__wrap19.Current;
                            this.<>2__current = this.<query>5__16;
                            this.<>1__state = 2;
                            flag = false;
                            return true;
                        }
                        finally
                        {
                            if (flag && (this.<>7__wrap19 != null))
                            {
                                this.<>7__wrap19.Dispose();
                            }
                        }
                    Label_0224:
                        this.<node>5__15 = this.<>7__wrap18.Current;
                        this.<>7__wrap19 = this.<>4__this.GetAllQueries(this.<node>5__15.Nodes).GetEnumerator();
                        goto Label_01A2;
                    Label_025B:
                        if (this.<>7__wrap18.MoveNext())
                        {
                            goto Label_0224;
                        }
                    }
                    finally
                    {
                        if (flag && (this.<>7__wrap18 != null))
                        {
                            this.<>7__wrap18.Dispose();
                        }
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<RunnableQuery> IEnumerable<RunnableQuery>.GetEnumerator()
            {
                SampleQueries.<GetAllQueries>d__12 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new SampleQueries.<GetAllQueries>d__12(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.nodes = this.<>3__nodes;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<LINQPad.RunnableQuery>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            RunnableQuery IEnumerator<RunnableQuery>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        private class BrowseLibrariesNode : TreeNode
        {
            public BrowseLibrariesNode() : base("Download more samples...")
            {
                base.NodeFont = SchemaTree.UnderlineFont;
                base.ForeColor = SystemColors.HotTrack;
                base.ImageKey = base.SelectedImageKey = "Add";
            }
        }

        private class DirectoryNode : TreeNode
        {
            public DirectoryNode(string text) : base(text.Replace('\x00b7', '.').Trim())
            {
                base.ImageKey = base.SelectedImageKey = "Folder";
                base.Name = text;
            }
        }

        public class QueryNode : TreeNode
        {
            private string _content;
            public readonly string ID;

            public QueryNode(string text, string content, string id) : base(text.Replace('\x00b7', '.').Trim())
            {
                this._content = content;
                this.ID = id;
                base.ImageKey = base.SelectedImageKey = "Query";
            }

            public virtual string Content
            {
                get
                {
                    return this._content;
                }
            }
        }

        private class SearchSamplesNode : TreeNode
        {
            public SearchSamplesNode() : base("Search samples...")
            {
                base.NodeFont = SchemaTree.UnderlineFont;
                base.ForeColor = SystemColors.HotTrack;
                base.ImageKey = base.SelectedImageKey = "Add";
            }
        }

        private class ThirdPartyDirectoryNode : SampleQueries.DirectoryNode
        {
            public ThirdPartyDirectoryNode(string text) : base(text)
            {
            }
        }

        public class ThirdPartyQueryNode : SampleQueries.QueryNode
        {
            private string _content;
            private string _pathWithinZip;
            private string _zipFilePath;

            public ThirdPartyQueryNode(string text, string zipFilePath, string pathWithinZip) : base(text, null, zipFilePath + "/" + pathWithinZip)
            {
                this._zipFilePath = zipFilePath;
                this._pathWithinZip = pathWithinZip;
            }

            public override string Content
            {
                get
                {
                    if (this._content == null)
                    {
                        MemoryStream stream = new MemoryStream();
                        using (ZipFile file = new ZipFile(this._zipFilePath))
                        {
                            file.get_Item(this._pathWithinZip).Extract(stream);
                        }
                        stream.Position = 0L;
                        this._content = new StreamReader(stream).ReadToEnd();
                    }
                    return this._content;
                }
            }
        }
    }
}

