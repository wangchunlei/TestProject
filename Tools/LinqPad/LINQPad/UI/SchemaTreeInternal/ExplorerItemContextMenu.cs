namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    internal class ExplorerItemContextMenu : ContextMenuStrip
    {
        private string _constructName;
        private ExplorerItem _item;
        private ExplorerItemNode _node;
        private string _schemaName;
        private SchemaTree _tree;

        public ExplorerItemContextMenu(SchemaTree tree, ExplorerItemNode node)
        {
            this._tree = tree;
            this._node = node;
            this._item = node.ExplorerItem;
            string dragText = node.ExplorerItem.DragText;
            if (string.IsNullOrEmpty(dragText))
            {
                dragText = node.Text;
            }
            if (this._item.Children != null)
            {
            }
            bool flag = (CS$<>9__CachedAnonymousMethodDelegate5 == null) && this._item.Children.Any<ExplorerItem>(CS$<>9__CachedAnonymousMethodDelegate5);
            if (((this._item.Icon == ExplorerIcon.StoredProc) || (this._item.Icon == ExplorerIcon.ScalarFunction)) || (this._item.Icon == ExplorerIcon.TableFunction))
            {
                string paramQuery = dragText = dragText + (!flag ? "()" : " (...)");
                QueryLanguage exprLanguage = UserOptions.Instance.IsVBDefault ? QueryLanguage.VBExpression : QueryLanguage.Expression;
                this.Items.Add(paramQuery, null, delegate (object sender, EventArgs e) {
                    NewQueryArgs args = new NewQueryArgs(paramQuery, new QueryLanguage?(exprLanguage)) {
                        ShowParams = true
                    };
                    tree.OnNewQuery(args);
                });
                if (this._item.Icon != ExplorerIcon.TableFunction)
                {
                    this.AddDDL();
                    return;
                }
            }
            IEnumerable<string> columnNames = from n in node.Nodes.OfType<ExplorerItemNode>()
                where (n.ExplorerItem.Kind == ExplorerItemKind.Property) && !string.IsNullOrEmpty(n.DragText)
                select n.DragText;
            QueryableMenuHelper.AddQueryableItems(tree, node, this, dragText, columnNames);
            this.AddDDL();
        }

        private void AddDDL()
        {
            ExplorerItem item = this._node.ExplorerItem;
            if (item.SupportsDDLEditing)
            {
                this._schemaName = "dbo";
                ExplorerItemNode parent = this._node.Parent.Parent as ExplorerItemNode;
                if ((parent != null) && (parent.ExplorerItem.Kind == ExplorerItemKind.Schema))
                {
                    this._schemaName = parent.ExplorerItem.SqlName ?? (parent.ExplorerItem.DragText ?? parent.ExplorerItem.Text);
                }
                this._constructName = (item.Icon == ExplorerIcon.StoredProc) ? "PROCEDURE" : ((item.Icon == ExplorerIcon.View) ? "View" : "FUNCTION");
                string str = (item.Icon == ExplorerIcon.StoredProc) ? "Stored Proc" : "Function";
                if (this.Items.Count > 0)
                {
                    this.Items.Add("-");
                }
                this.Items.Add("Edit " + str + " Definition", null, delegate (object sender, EventArgs e) {
                    NewQueryArgs args = new NewQueryArgs(new Func<string>(this.SpHelpText)) {
                        Language = 8,
                        QueryName = "(" + item.SqlName + ")"
                    };
                    this._tree.OnNewQuery(args);
                });
                this.Items.Add("Drop " + str, null, delegate (object sender, EventArgs e) {
                    if (MessageBox.Show("Drop " + item.SqlName + " - are you sure?", "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        this.DropObject();
                    }
                });
            }
        }

        private void DropObject()
        {
            this.ProcessDDL<string>(delegate (RepositoryNode rNode) {
                Action method = null;
                using (IDbConnection connection = rNode.Repository.Open())
                {
                    IDbCommand command = connection.CreateCommand();
                    string str = "[" + this._schemaName + "].[" + this._item.SqlName + "]";
                    command.CommandText = "DROP " + this._constructName + " " + str;
                    command.CommandTimeout = 0x1d4c;
                    command.ExecuteNonQuery();
                    if (method == null)
                    {
                        method = () => rNode.RefreshData(true);
                    }
                    rNode.TreeView.BeginInvoke(method);
                    return "";
                }
            });
        }

        private TResult ProcessDDL<TResult>(Func<RepositoryNode, TResult> action)
        {
            TreeNode parent = this._node;
            while (!(parent is RepositoryNode))
            {
                parent = parent.Parent;
                if (parent == null)
                {
                    return default(TResult);
                }
            }
            RepositoryNode rNode = (RepositoryNode) parent;
            string errorMsg = null;
            TResult result = default(TResult);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            new Thread(delegate {
                try
                {
                    result = action(rNode);
                }
                catch (Exception exception)
                {
                    errorMsg = exception.Message;
                }
                finally
                {
                    waitHandle.Set();
                }
            }) { Name = "DDL Query", IsBackground = true }.Start();
            if (!waitHandle.WaitOne(0x1b58, false))
            {
                MessageBox.Show("The server timed out.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return default(TResult);
            }
            if (errorMsg != null)
            {
                MessageBox.Show("Error: " + errorMsg, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return default(TResult);
            }
            return result;
        }

        private string SpHelpText()
        {
            return this.ProcessDDL<string>(delegate (RepositoryNode rNode) {
                using (IDbConnection connection = rNode.Repository.Open())
                {
                    IDbCommand command = connection.CreateCommand();
                    string str = this._schemaName + "." + this._item.SqlName;
                    command.CommandText = "sp_helptext [" + str + "]";
                    command.CommandTimeout = 0x1d4c;
                    bool flag = false;
                    StringBuilder builder = new StringBuilder();
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string str2 = reader[0].ToString();
                            if (!(flag || !str2.TrimStart(new char[0]).StartsWith("CREATE", StringComparison.OrdinalIgnoreCase)))
                            {
                                str2 = "ALTER " + str2.TrimStart(new char[0]).Substring(7);
                                flag = true;
                            }
                            builder.Append(str2);
                        }
                    }
                    return builder.ToString().Trim();
                }
            });
        }
    }
}

