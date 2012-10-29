namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal abstract class RepositoryNode : BaseNode
    {
        private object _locker;
        private Timer _resourcesTimer;
        private Thread _worker;
        public string BaseText;
        internal bool ExpandOnUpdate;
        public readonly LINQPad.Repository Repository;

        public RepositoryNode(LINQPad.Repository r) : base(r.GetFriendlyName())
        {
            EventHandler handler = null;
            this._locker = new object();
            Timer timer = new Timer {
                Interval = 0xea60
            };
            this._resourcesTimer = timer;
            this.Repository = r;
            base.NodeFont = SchemaTree.BaseFont;
            base.ImageKey = base.SelectedImageKey = this.NormalImageKey;
            this.BaseText = base.Text;
            handler = delegate (object sender, EventArgs e) {
                if (base.IsExpanded || this.Populating)
                {
                    this._resourcesTimer.Stop();
                }
                else
                {
                    this.OnIdle();
                }
            };
            this._resourcesTimer.Tick += handler;
        }

        protected void AddNodes(TreeNode[] nodes)
        {
            TreeView treeView = base.TreeView;
            using (new TreeStateMemento(this))
            {
                base.Nodes.Clear();
                base.Nodes.AddRange(nodes);
            }
            if (!(base.IsExpanded || !this.ExpandOnUpdate))
            {
                base.Expand();
            }
        }

        public virtual void Dispose()
        {
            this._resourcesTimer.Dispose();
            foreach (TreeNode node in base.Nodes)
            {
                if (node is IDisposable)
                {
                    ((IDisposable) node).Dispose();
                }
            }
        }

        internal ExplorerItemNode FindExplorerItemNode(ExplorerItem item)
        {
            return this.FindExplorerItemNode(this, item);
        }

        private ExplorerItemNode FindExplorerItemNode(TreeNode parent, ExplorerItem item)
        {
            foreach (ExplorerItemNode node in parent.Nodes.OfType<ExplorerItemNode>())
            {
                if (node.ExplorerItem == item)
                {
                    return node;
                }
            }
            foreach (TreeNode node3 in parent.Nodes)
            {
                ExplorerItemNode node4 = this.FindExplorerItemNode(node3, item);
                if (node4 != null)
                {
                    return node4;
                }
            }
            return null;
        }

        protected internal virtual void Init()
        {
            this.RefreshData(false);
        }

        protected internal override void OnAfterExpand()
        {
            base.OnAfterExpand();
            this._resourcesTimer.Stop();
        }

        protected internal virtual void OnBeforeCollapse()
        {
            foreach (TreeNode node in base.Nodes)
            {
                if (node.IsExpanded)
                {
                    node.Collapse();
                }
            }
            this._resourcesTimer.Start();
        }

        protected virtual void OnIdle()
        {
        }

        protected abstract void Populate();
        protected internal virtual void RefreshData(bool forceRefresh)
        {
            this._resourcesTimer.Stop();
            lock (this._locker)
            {
                if (!(!this.Populating || forceRefresh))
                {
                    return;
                }
                this.Populating = true;
                Thread thread = new Thread(new ThreadStart(this.Populate)) {
                    Name = "Repository Refresher",
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                };
                this._worker = thread;
            }
            this.SetStatus(this.PopulatingMessage);
            base.ImageKey = base.SelectedImageKey = "Hourglass";
            base.ForeColor = Color.Gray;
            this._worker.Start();
        }

        public virtual void ReleaseResources()
        {
        }

        protected void SetComplete(bool successful)
        {
            base.ImageKey = base.SelectedImageKey = successful ? this.NormalImageKey : this.FailedImageKey;
            base.ForeColor = successful ? SystemColors.WindowText : Color.Red;
        }

        protected void SetStatus(string status)
        {
            string baseText = this.BaseText;
            if (status.Length > 0)
            {
                baseText = baseText + " (" + status + ")";
            }
            base.Text = baseText;
        }

        protected void SetStatusInvoke(string status)
        {
            if (this.WaitForTreeView())
            {
                base.TreeView.BeginInvoke(new Action<string>(this.SetStatus), new object[] { status });
            }
        }

        protected internal virtual void ShowErrorIfUninitialized(string msg)
        {
        }

        protected ExplorerItemNode[] ToExplorerItemNodes(IEnumerable<ExplorerItem> explorerItems)
        {
            return (from item in explorerItems select new ExplorerItemNode(this.Repository, item)).ToArray<ExplorerItemNode>();
        }

        internal virtual void UpdateText()
        {
            try
            {
                base.Text = this.BaseText = this.Repository.GetFriendlyName();
            }
            catch (OutOfMemoryException)
            {
            }
        }

        protected bool WaitForTreeView()
        {
            for (int i = 0; i < 100; i++)
            {
                if ((base.TreeView == null) || base.TreeView.IsDisposed)
                {
                    return false;
                }
                if (base.TreeView.IsHandleCreated)
                {
                    return true;
                }
                Thread.Sleep(100);
            }
            return false;
        }

        protected virtual string FailedImageKey
        {
            get
            {
                return "FailedConnection";
            }
        }

        protected object Locker
        {
            get
            {
                return this._locker;
            }
        }

        protected virtual string NormalImageKey
        {
            get
            {
                return "Connection";
            }
        }

        protected bool Populating { get; set; }

        protected virtual string PopulatingMessage
        {
            get
            {
                return "Connecting";
            }
        }

        protected Thread Worker
        {
            get
            {
                return this._worker;
            }
        }
    }
}

