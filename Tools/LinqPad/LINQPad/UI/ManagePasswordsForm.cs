namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    internal class ManagePasswordsForm : BaseForm
    {
        private static ManagePasswordsForm _instance;
        private Button btnAdd;
        private Button btnChange;
        private Button btnClose;
        private Button btnDelete;
        private Button btnDeleteAll;
        private ColumnHeader colName;
        private IContainer components = null;
        private Label label1;
        private ListView lvPasswords;
        private TableLayoutPanel panOKCancel;

        public ManagePasswordsForm()
        {
            this.InitializeComponent();
            base.Icon = MainForm.Instance.Icon;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = null;
            TextBoxForm form;
            using (form = new TextBoxForm("Enter password name (e.g: azure.mystorage)"))
            {
                if ((form.ShowDialog() != DialogResult.OK) || (form.UserText.Trim().Length == 0))
                {
                    return;
                }
                name = form.UserText.Trim();
            }
            string password = null;
            using (form = new TextBoxForm("Enter password", true))
            {
                if (form.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                password = form.UserText;
            }
            PasswordManager.SetPassword(name, password);
            this.Populate();
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (this.lvPasswords.SelectedItems.Count == 1)
            {
                this.lvPasswords.Focus();
                string text = this.lvPasswords.SelectedItems[0].Text;
                string password = null;
                using (TextBoxForm form = new TextBoxForm("Enter new password", true))
                {
                    if ((form.ShowDialog() != DialogResult.OK) || (form.UserText.Length == 0))
                    {
                        return;
                    }
                    password = form.UserText;
                }
                PasswordManager.SetPassword(text, password);
                this.Populate();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (this.lvPasswords.SelectedItems.Count == 1)
            {
                this.lvPasswords.Focus();
                PasswordManager.DeletePassword(this.lvPasswords.SelectedItems[0].Text);
                this.Populate();
            }
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete all passwords - are you sure?", "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (ListViewItem item in this.lvPasswords.Items)
                {
                    PasswordManager.DeletePassword(item.Text);
                }
            }
            this.Populate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void EnableControls()
        {
            this.btnDelete.Enabled = this.btnChange.Enabled = this.lvPasswords.SelectedItems.Count > 0;
            this.btnDeleteAll.Enabled = this.lvPasswords.Items.Count > 0;
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ManagePasswordsForm));
            this.panOKCancel = new TableLayoutPanel();
            this.btnDelete = new Button();
            this.btnDeleteAll = new Button();
            this.btnChange = new Button();
            this.btnClose = new Button();
            this.btnAdd = new Button();
            this.label1 = new Label();
            this.lvPasswords = new ListView();
            this.colName = new ColumnHeader();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 6;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnDelete, 2, 0);
            this.panOKCancel.Controls.Add(this.btnDeleteAll, 3, 0);
            this.panOKCancel.Controls.Add(this.btnChange, 1, 0);
            this.panOKCancel.Controls.Add(this.btnClose, 5, 0);
            this.panOKCancel.Controls.Add(this.btnAdd, 0, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(5, 0x1b4);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 4, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x25d, 0x27);
            this.panOKCancel.TabIndex = 1;
            this.btnDelete.Location = new Point(0xf9, 7);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new Size(90, 0x1d);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "&Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);
            this.btnDeleteAll.Location = new Point(0x159, 7);
            this.btnDeleteAll.Name = "btnDeleteAll";
            this.btnDeleteAll.Size = new Size(0x5c, 0x1d);
            this.btnDeleteAll.TabIndex = 3;
            this.btnDeleteAll.Text = "Delete All";
            this.btnDeleteAll.UseVisualStyleBackColor = true;
            this.btnDeleteAll.Click += new EventHandler(this.btnDeleteAll_Click);
            this.btnChange.Location = new Point(0x60, 7);
            this.btnChange.Name = "btnChange";
            this.btnChange.Size = new Size(0x93, 0x1d);
            this.btnChange.TabIndex = 1;
            this.btnChange.Text = "&Change Password";
            this.btnChange.UseVisualStyleBackColor = true;
            this.btnChange.Click += new EventHandler(this.btnChange_Click);
            this.btnClose.DialogResult = DialogResult.Cancel;
            this.btnClose.Location = new Point(0x20d, 7);
            this.btnClose.Margin = new Padding(3, 3, 1, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(0x4f, 0x1d);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.btnAdd.Location = new Point(0, 7);
            this.btnAdd.Margin = new Padding(0, 3, 3, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new Size(90, 0x1d);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "&Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(5, 6);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 5);
            this.label1.Size = new Size(0x216, 0x3e);
            this.label1.TabIndex = 3;
            this.label1.Text = manager.GetString("label1.Text");
            this.lvPasswords.Columns.AddRange(new ColumnHeader[] { this.colName });
            this.lvPasswords.Dock = DockStyle.Fill;
            this.lvPasswords.FullRowSelect = true;
            this.lvPasswords.HideSelection = false;
            this.lvPasswords.Location = new Point(5, 0x44);
            this.lvPasswords.MultiSelect = false;
            this.lvPasswords.Name = "lvPasswords";
            this.lvPasswords.Size = new Size(0x25d, 0x170);
            this.lvPasswords.TabIndex = 0;
            this.lvPasswords.UseCompatibleStateImageBehavior = false;
            this.lvPasswords.View = View.Details;
            this.lvPasswords.Resize += new EventHandler(this.lvPasswords_Resize);
            this.lvPasswords.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(this.lvPasswords_ItemSelectionChanged);
            this.colName.Text = "Name";
            this.colName.Width = 500;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnClose;
            base.ClientSize = new Size(0x267, 480);
            base.Controls.Add(this.lvPasswords);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.panOKCancel);
            base.KeyPreview = true;
            base.Name = "ManagePasswordsForm";
            base.Padding = new Padding(5, 6, 5, 5);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Manage Passwords";
            base.KeyDown += new KeyEventHandler(this.ManagePasswordsForm_KeyDown);
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void lvPasswords_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.EnableControls();
        }

        private void lvPasswords_Resize(object sender, EventArgs e)
        {
            this.colName.Width = this.lvPasswords.ClientSize.Width - 1;
        }

        private void ManagePasswordsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                this.btnDelete_Click(this, EventArgs.Empty);
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.Populate();
        }

        private void Populate()
        {
            Func<ListViewItem, bool> predicate = null;
            string selectedText = (this.lvPasswords.SelectedItems.Count == 1) ? this.lvPasswords.SelectedItems[0].Text : null;
            this.lvPasswords.Items.Clear();
            this.lvPasswords.Items.AddRange((from s in PasswordManager.GetAllPasswordNames()
                orderby s
                select new ListViewItem(s)).ToArray<ListViewItem>());
            if (selectedText != null)
            {
                if (predicate == null)
                {
                    predicate = i => i.Text == selectedText;
                }
                ListViewItem item = this.lvPasswords.Items.Cast<ListViewItem>().FirstOrDefault<ListViewItem>(predicate);
                if (item != null)
                {
                    item.Focused = true;
                    item.Selected = true;
                }
            }
            if ((this.lvPasswords.SelectedItems.Count == 0) && (this.lvPasswords.Items.Count > 0))
            {
                this.lvPasswords.Items[0].Focused = true;
                this.lvPasswords.Items[0].Selected = true;
            }
            this.EnableControls();
        }

        public static void ShowInstance()
        {
            if ((_instance == null) || _instance.IsDisposed)
            {
                _instance = new ManagePasswordsForm();
            }
            _instance.Show();
        }
    }
}

