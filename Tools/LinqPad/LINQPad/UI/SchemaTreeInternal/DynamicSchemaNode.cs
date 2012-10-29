namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    internal class DynamicSchemaNode : RepositoryNode, IDisposable
    {
        private string _baseImageKey;
        private DataContextManager _dcManager;
        private List<TreeNode> _dormantNodes;
        private bool _error;
        private bool _init;
        private int _lastSchemaHash;
        private bool _needsRefresh;

        public DynamicSchemaNode(Repository r) : base(r)
        {
            base.Nodes.Add("(Populating)").NodeFont = SchemaTree.BaseFont;
            base.ImageKey = base.SelectedImageKey = this._baseImageKey = r.DriverLoader.IsValid ? r.DriverLoader.Driver.GetImageKey(r) : "CustomError";
            base.Name = base.Text;
        }

        private void DataContextInfoUpdated(DataContextInfo m)
        {
            if (this._dcManager != null)
            {
                this.UpdateNode(m);
            }
        }

        public override void Dispose()
        {
            if (this._dcManager != null)
            {
                this._dcManager.Dispose();
                this._dcManager = null;
            }
            base.Dispose();
        }

        protected internal override void Init()
        {
            if (!base.Repository.DriverLoader.IsValid)
            {
                this.SetError("Driver Error: " + base.Repository.DriverLoader.LoadError.Message);
            }
            else
            {
                this._init = true;
                base.SetStatus("Connecting");
                base.ForeColor = Color.Gray;
                base.ImageKey = base.SelectedImageKey = "Hourglass";
                this.InitDCManager(false);
            }
            this._needsRefresh = false;
            this._dormantNodes = null;
        }

        private void InitDCManager(bool forceRefresh)
        {
            this._dcManager = DataContextManager.SubscribeToDataContextChanges(base.Repository, new DataContextCallback(this.DataContextInfoUpdated));
            this._dcManager.GetDataContextInfo(forceRefresh ? SchemaChangeTestMode.ForceRefresh : SchemaChangeTestMode.TestAndFailPositive);
            this._dcManager.Tag = "DynamicSchemaNode";
        }

        internal bool IsDirty()
        {
            return this._needsRefresh;
        }

        internal void MarkDirty()
        {
            this._needsRefresh = true;
        }

        protected internal override void OnAfterExpand()
        {
            if (this._dormantNodes != null)
            {
                base.TreeView.Update();
                try
                {
                    base.TreeView.BeginUpdate();
                    base.Nodes.Clear();
                    base.Nodes.AddRange(this._dormantNodes.ToArray());
                }
                finally
                {
                    base.TreeView.EndUpdate();
                }
                this._dormantNodes = null;
            }
            base.OnAfterExpand();
        }

        protected internal override void OnBeforeCollapse()
        {
            base.OnBeforeCollapse();
            this._dormantNodes = new List<TreeNode>(base.Nodes.OfType<TreeNode>());
            base.Nodes.Clear();
            base.Nodes.Add("<loading>");
        }

        protected internal override void OnBeforeExpand()
        {
            if (!this._init)
            {
                this.Init();
            }
            else if (this._needsRefresh || this._error)
            {
                this.RefreshData(this._needsRefresh);
            }
            base.OnBeforeExpand();
        }

        protected override void OnIdle()
        {
            if (!base.IsExpanded && ((this._dcManager == null) || !this._dcManager.HasOtherSubscribers))
            {
                this.ReleaseResources();
            }
        }

        protected override void Populate()
        {
        }

        protected internal override void RefreshData(bool forceRefresh)
        {
            this._dormantNodes = null;
            TreeView treeView = base.TreeView;
            try
            {
                if (treeView != null)
                {
                    treeView.BeginUpdate();
                }
                if (!this._init)
                {
                    this.Init();
                }
                else
                {
                    this._needsRefresh = false;
                    base.SetStatus("Connecting");
                    base.ImageKey = base.SelectedImageKey = "Hourglass";
                    if ((this._dcManager == null) || this._dcManager.IsDisposed)
                    {
                        this.InitDCManager(forceRefresh);
                    }
                    else
                    {
                        this._dcManager.GetDataContextInfo(forceRefresh ? SchemaChangeTestMode.ForceRefresh : SchemaChangeTestMode.TestAndFailPositive);
                    }
                }
            }
            finally
            {
                if (treeView != null)
                {
                    treeView.EndUpdate();
                }
            }
        }

        public override void ReleaseResources()
        {
            base.Nodes.Clear();
            this._dormantNodes = null;
            this._lastSchemaHash = 0;
            if (this._dcManager != null)
            {
                this._dcManager.Dispose();
                this._dcManager = null;
            }
            base.Nodes.Add("(Populating)");
            this._init = false;
            GC.Collect();
        }

        private void SetDone()
        {
            base.ImageKey = base.SelectedImageKey = this._error ? base.Repository.DriverLoader.Driver.GetFailedImageKey(base.Repository) : this._baseImageKey;
            base.ForeColor = this._error ? Color.Red : SystemColors.WindowText;
        }

        private void SetError(string msg)
        {
            base.SetStatus("Error: " + msg);
            base.ImageKey = base.SelectedImageKey = base.Repository.DriverLoader.IsValid ? base.Repository.DriverLoader.Driver.GetFailedImageKey(base.Repository) : "CustomError";
            base.ForeColor = Color.Red;
            if (base.IsExpanded)
            {
                base.Collapse();
            }
        }

        protected internal override void ShowErrorIfUninitialized(string msg)
        {
            if (!this._init)
            {
                base.Nodes.Clear();
                this.SetError(msg);
            }
        }

        private void UpdateNode(DataContextInfo m)
        {
            TreeNode[] nodes;
            this._error = false;
            if (m.Busy)
            {
                if (((base.TreeView != null) && !base.TreeView.IsDisposed) && base.TreeView.IsHandleCreated)
                {
                    base.SetStatusInvoke(m.Status);
                }
            }
            else
            {
                this._error = m.Error != null;
                if (this._error)
                {
                    if (!((base.TreeView == null) || base.TreeView.IsDisposed))
                    {
                        base.WaitForTreeView();
                        base.TreeView.Invoke(new Action<string>(this.SetError), new object[] { m.Error });
                    }
                }
                else
                {
                    if (((base.TreeView != null) && !base.TreeView.IsDisposed) && base.TreeView.IsHandleCreated)
                    {
                        base.SetStatusInvoke("");
                    }
                    if ((m.Schema != null) && (m.Schema.GetHashCode() != this._lastSchemaHash))
                    {
                        this._lastSchemaHash = m.Schema.GetHashCode();
                        this._dormantNodes = null;
                        nodes = base.ToExplorerItemNodes(m.Schema);
                        if ((base.TreeView != null) && !base.TreeView.IsDisposed)
                        {
                            base.WaitForTreeView();
                            base.TreeView.Invoke(() => this.AddNodes(nodes));
                            base.TreeView.Invoke(new Action(this.SetDone));
                        }
                    }
                }
            }
        }

        public bool Initialized
        {
            get
            {
                return this._init;
            }
        }
    }
}

