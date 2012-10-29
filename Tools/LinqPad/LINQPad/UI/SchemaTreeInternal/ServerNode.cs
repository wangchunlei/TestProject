namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Windows.Forms;

    internal class ServerNode : RepositoryNode
    {
        public ServerNode(Repository r) : base(r)
        {
        }

        private void AddNodes(IEnumerable<TreeNode> nodes)
        {
            Dictionary<string, TreeNode> dictionary = new Dictionary<string, TreeNode>();
            foreach (TreeNode node in base.Nodes)
            {
                if (!string.IsNullOrEmpty(node.Name))
                {
                    dictionary.Add(node.Name, node);
                }
            }
            TreeView treeView = base.TreeView;
            try
            {
                if (treeView != null)
                {
                    treeView.BeginUpdate();
                }
                foreach (TreeNode node in nodes)
                {
                    if (dictionary.ContainsKey(node.Name))
                    {
                        dictionary.Remove(node.Name);
                    }
                    else
                    {
                        base.Nodes.Add(node);
                    }
                }
                foreach (TreeNode node in dictionary.Values)
                {
                    base.Nodes.Remove(node);
                    IDisposable disposable2 = node as IDisposable;
                    if (disposable2 != null)
                    {
                        disposable2.Dispose();
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
            if (!(base.IsExpanded || !base.ExpandOnUpdate))
            {
                base.Expand();
            }
        }

        protected override void Populate()
        {
            Exception exception;
            try
            {
                object obj2;
                List<TreeNode> list = new List<TreeNode>();
                bool flag = false;
                try
                {
                    using (IDbConnection connection = base.Repository.Open())
                    {
                        if (((base.TreeView != null) && !base.TreeView.IsDisposed) && base.TreeView.IsHandleCreated)
                        {
                            base.TreeView.BeginInvoke(new Action<string>(this.SetStatus), new object[] { "Populating" });
                        }
                        string cmdText = base.Repository.IsAzure ? "select name from sys.databases" : "dbo.sp_MShasdbaccess";
                        using (SqlDataReader reader = new SqlCommand(cmdText, (SqlConnection) connection).ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Repository r = base.Repository.CreateChild(reader[0].ToString());
                                list.Add(new DynamicSchemaNode(r));
                            }
                        }
                        lock ((obj2 = base.Locker))
                        {
                            if ((base.Worker != Thread.CurrentThread) || !base.WaitForTreeView())
                            {
                                return;
                            }
                            base.TreeView.BeginInvoke(new Action<IEnumerable<TreeNode>>(this.AddNodes), new object[] { list });
                            base.TreeView.BeginInvoke(new Action<string>(this.SetStatus), new object[] { "" });
                        }
                        flag = true;
                    }
                }
                catch (Exception exception1)
                {
                    exception = exception1;
                    if (!base.WaitForTreeView())
                    {
                        return;
                    }
                    lock ((obj2 = base.Locker))
                    {
                        if (base.Worker != Thread.CurrentThread)
                        {
                            return;
                        }
                        base.TreeView.BeginInvoke(new Action<string>(this.SetStatus), new object[] { "Error: " + exception.Message });
                    }
                }
                finally
                {
                    base.Populating = false;
                }
                lock ((obj2 = base.Locker))
                {
                    if (base.Worker == Thread.CurrentThread)
                    {
                        base.TreeView.BeginInvoke(new Action<bool>(this.SetComplete), new object[] { flag });
                    }
                }
            }
            catch (Exception exception2)
            {
                exception = exception2;
                Program.ProcessException(exception);
            }
        }

        protected override string FailedImageKey
        {
            get
            {
                return (base.Repository.IsAzure ? "FailedAzureConnection" : "FailedConnection");
            }
        }

        protected override string NormalImageKey
        {
            get
            {
                return (base.Repository.IsAzure ? "AzureConnection" : "Connection");
            }
        }
    }
}

