namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad.Properties;
    using LINQPad.UI;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class NodeContextMenu : ContextMenuStrip
    {
        private RepositoryNode _node;
        private SchemaTree _tree;

        public NodeContextMenu(SchemaTree tree, RepositoryNode node)
        {
            EventHandler onClick = null;
            this._tree = tree;
            this._node = node;
            if ((node.Repository.DriverLoader.InternalID == null) && node.Repository.DriverLoader.IsValid)
            {
                ToolStripLabel label = new ToolStripLabel("Custom driver: " + node.Repository.DriverLoader.Driver.Name);
                this.Items.Add(label);
                this.Items.Add("-");
                label.Font = new Font(label.Font, FontStyle.Bold);
            }
            if (node.Repository.IsQueryable)
            {
                this.Items.Add("New Query", Resources.New, new EventHandler(this.NewQuery));
                this.Items.Add("-");
                if ((MainForm.Instance != null) && (MainForm.Instance.CurrentQueryControl != null))
                {
                    this.Items.Add("Use in Current Query", Resources.UseCurrentQuery, (sender, e) => MainForm.Instance.CurrentQueryControl.UseCurrentDb(false));
                    if (MainForm.Instance.CurrentQueryControl.Query.Repository == node.Repository)
                    {
                        this.Items[this.Items.Count - 1].Enabled = false;
                    }
                    this.Items.Add("-");
                }
                if (MainForm.Instance != null)
                {
                    if (onClick == null)
                    {
                        onClick = (sender, e) => MainForm.Instance.ClearAllConnections(node.Repository);
                    }
                    this.Items.Add("Close all connections", null, onClick);
                    this.Items.Add("-");
                }
            }
            this.Items.Add("Refresh", Resources.Refresh, new EventHandler(this.Refresh));
            if (node.Parent == null)
            {
                this.Items.Add("Delete Connection", Resources.Delete, new EventHandler(this.Delete));
            }
            if (!((this._node.Repository.DriverLoader.SimpleAssemblyName != null) && this._node.Repository.DriverLoader.SimpleAssemblyName.StartsWith("Mindscape.LightSpeed", StringComparison.InvariantCultureIgnoreCase)) && (this._node.Repository.Parent == null))
            {
                this.Items.Add("-");
                this.Items.Add("Rename Connection", null, new EventHandler(this.Rename));
                if (!string.IsNullOrEmpty(this._node.Repository.DisplayName))
                {
                    this.Items.Add("Reset Connection Name", null, new EventHandler(this.ResetName));
                }
            }
            if (node.Parent == null)
            {
                this.Items.Add("Create Similar Connection...", null, new EventHandler(this.CreateSimilar));
                this.Items.Add("-");
                this.Items.Add("Properties", Resources.AdvancedProperties, new EventHandler(this.Edit));
            }
        }

        private void CreateSimilar(object sender, EventArgs e)
        {
            this._tree.AddCxFromTemplate(this._node.Repository);
        }

        private void Delete(object sender, EventArgs e)
        {
            this._tree.Delete(this._node);
        }

        private void Edit(object sender, EventArgs e)
        {
            this._tree.Edit(this._node);
        }

        private void NewQuery(object sender, EventArgs e)
        {
            this._tree.OnNewQuery(new NewQueryArgs());
        }

        private void Refresh(object sender, EventArgs e)
        {
            this._node.RefreshData(true);
        }

        private void Rename(object sender, EventArgs e)
        {
            using (TextBoxForm form = new TextBoxForm("Enter custom name for connection:"))
            {
                form.UserText = this._node.Repository.DisplayName;
                if (form.ShowDialog(MainForm.Instance) != DialogResult.OK)
                {
                    return;
                }
                this._node.Repository.DisplayName = form.UserText;
            }
            this.SaveAndUpdateNode();
        }

        private void ResetName(object sender, EventArgs e)
        {
            this._node.Repository.DisplayName = "";
            this.SaveAndUpdateNode();
        }

        private void SaveAndUpdateNode()
        {
            this._node.Repository.SaveToDisk();
            this._node.UpdateText();
            foreach (TreeNode node in this._node.Nodes)
            {
                DynamicSchemaNode node2 = node as DynamicSchemaNode;
                if (node2 != null)
                {
                    node2.Repository.DisplayName = this._node.Repository.DisplayName;
                }
            }
            this._tree.InvokeCxEdited();
        }
    }
}

