namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;

    internal class StaticSchemaNode : RepositoryNode
    {
        private FileSystemWatcher _assemblyWatcher;
        private bool _isDisposed;
        private bool _populated;

        public StaticSchemaNode(Repository r) : base(r)
        {
            base.Nodes.Add("(Populating)").NodeFont = SchemaTree.BaseFont;
            this.SetToolTip();
            base.ImageKey = base.SelectedImageKey = this.NormalImageKey;
            this.CreateFileWatcher();
        }

        private void ClearToolTip()
        {
            base.ToolTipText = "";
        }

        private void CreateFileWatcher()
        {
            lock (base.Locker)
            {
                try
                {
                    if ((!string.IsNullOrEmpty(base.Repository.CustomAssemblyPath) && Path.IsPathRooted(base.Repository.CustomAssemblyPath)) && File.Exists(base.Repository.CustomAssemblyPath))
                    {
                        FileSystemWatcher watcher = this._assemblyWatcher = new FileSystemWatcher(Path.GetDirectoryName(base.Repository.CustomAssemblyPath), Path.GetFileName(base.Repository.CustomAssemblyPath));
                        FileSystemEventHandler handler = delegate (object sender, FileSystemEventArgs e) {
                            Action method = null;
                            try
                            {
                                if (this._isDisposed)
                                {
                                    watcher.Dispose();
                                    this._assemblyWatcher = null;
                                }
                                else
                                {
                                    if (this._populated)
                                    {
                                        if (method == null)
                                        {
                                            method = () => this.RefreshData(true);
                                        }
                                        this.TreeView.BeginInvoke(method);
                                    }
                                    RepositoryArgs args = new RepositoryArgs {
                                        Repository = this.Repository
                                    };
                                    ((SchemaTree) this.TreeView).OnStaticSchemaRepositoryChanged(args);
                                }
                            }
                            catch (Exception exception)
                            {
                                Log.Write(exception, "StaticSchemaNode CreateFileWatcher Invoke");
                            }
                        };
                        this._assemblyWatcher.Changed += handler;
                        this._assemblyWatcher.Created += handler;
                        this._assemblyWatcher.EnableRaisingEvents = true;
                    }
                }
                catch (Exception exception)
                {
                    Log.Write(exception, "StaticSchemaNode CreateFileWatcher");
                }
            }
        }

        public override void Dispose()
        {
            this._isDisposed = true;
            FileSystemWatcher watcher = this._assemblyWatcher;
            if (watcher != null)
            {
                watcher.Dispose();
            }
            this._assemblyWatcher = null;
            base.Dispose();
        }

        private List<ExplorerItem> GetExplorerItems()
        {
            using (DomainIsolator isolator = new DomainIsolator(base.Repository.CreateSchemaAndRunnerDomain("Custom schema resolver", false, false)))
            {
                return (isolator.GetInstance<SchemaRetriever>().GetSchema(base.Repository) ?? new List<ExplorerItem>());
            }
        }

        protected internal override void OnBeforeExpand()
        {
            if (!this._populated)
            {
                this.RefreshData(false);
            }
            base.OnBeforeExpand();
        }

        protected override void Populate()
        {
            if (base.Repository.DriverLoader.IsValid)
            {
                object obj2;
                FileSystemWatcher watcher = this._assemblyWatcher;
                if (watcher != null)
                {
                    watcher.Dispose();
                }
                this._assemblyWatcher = null;
                try
                {
                    this._populated = true;
                    if (this._isDisposed || !base.WaitForTreeView())
                    {
                        return;
                    }
                    base.TreeView.Invoke(new Action<string>(this.SetStatus), new object[] { "Populating" });
                    if (base.Worker != Thread.CurrentThread)
                    {
                        return;
                    }
                    List<ExplorerItem> explorerItems = this.GetExplorerItems();
                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter(obj2 = base.Locker, ref lockTaken);
                        if ((base.Worker != Thread.CurrentThread) || this._isDisposed)
                        {
                            return;
                        }
                        TreeNode[] nodes = base.ToExplorerItemNodes(explorerItems);
                        base.TreeView.BeginInvoke(() => this.AddNodes(nodes));
                        base.TreeView.BeginInvoke(new Action<string>(this.SetStatus), new object[] { "" });
                        base.TreeView.BeginInvoke(new Action<bool>(this.SetComplete), new object[] { true });
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(obj2);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (!base.WaitForTreeView())
                    {
                        return;
                    }
                    lock ((obj2 = base.Locker))
                    {
                        if ((base.Worker != Thread.CurrentThread) || this._isDisposed)
                        {
                            return;
                        }
                        base.TreeView.BeginInvoke(new Action<bool>(this.SetComplete), new object[] { false });
                        Exception innerException = exception;
                        if ((exception is TargetInvocationException) && (exception.InnerException != null))
                        {
                            innerException = exception.InnerException;
                        }
                        base.TreeView.BeginInvoke(new Action<string>(this.SetStatus), new object[] { "Error: " + innerException.Message });
                        TreeNodeCollection collection1 = base.Nodes;
                        base.TreeView.BeginInvoke(new Action(collection1.Clear));
                        base.TreeView.BeginInvoke(new Action(this.ClearToolTip));
                    }
                }
                finally
                {
                    base.Populating = false;
                }
                this.CreateFileWatcher();
            }
        }

        private void SetToolTip()
        {
            if ((base.Repository.DriverLoader.InternalID != null) && base.Repository.DriverLoader.IsValid)
            {
                base.ToolTipText = base.Repository.GetFriendlyName(Repository.FriendlyNameMode.FullTooltip);
            }
        }

        internal override void UpdateText()
        {
            base.UpdateText();
            this.SetToolTip();
        }

        protected override string FailedImageKey
        {
            get
            {
                return (((base.Repository == null) || !base.Repository.DriverLoader.IsValid) ? "CustomError" : base.Repository.DriverLoader.Driver.GetFailedImageKey(base.Repository));
            }
        }

        protected override string NormalImageKey
        {
            get
            {
                return ((base.Repository == null) ? "Custom" : (!base.Repository.DriverLoader.IsValid ? "CustomError" : base.Repository.DriverLoader.Driver.GetImageKey(base.Repository)));
            }
        }

        protected override string PopulatingMessage
        {
            get
            {
                return "Populating";
            }
        }

        private class SchemaRetriever : MarshalByRefObject
        {
            public List<ExplorerItem> GetSchema(Repository r)
            {
                StaticDataContextDriver driver = (StaticDataContextDriver) r.DriverLoader.Driver;
                return driver.GetSchema(r);
            }
        }
    }
}

