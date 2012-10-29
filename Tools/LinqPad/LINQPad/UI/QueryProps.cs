namespace LINQPad.UI
{
    using ActiproBridge;
    using LINQPad;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Xml.Linq;

    internal class QueryProps : BaseForm
    {
        private bool _activated;
        private static Action _addMoreRefsAction;
        private static ContextMenuStrip _addRefMenuStrip = new ContextMenuStrip();
        private static Action _browseRefsFolderAction;
        private Graphics _measureGraphics;
        private string _origNS;
        private QueryCore _query;
        private bool _refChanges;
        private Font _underlineFont;
        private Button btnAdd;
        private Button btnBrowse;
        private Button btnCancel;
        private Button btnOK;
        private Button btnRemove;
        private Button btnSaveAsSnippet;
        private Button btnSetAsDefault;
        private CheckBox chkIncludePredicateBuilder;
        private CheckBox chkNoShadow;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private IContainer components = null;
        private FlowLayoutPanel flowLayoutPanel1;
        private GroupBox groupBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private LinkLabel llPrefsAdvanced;
        private ListView lvRefs;
        private TableLayoutPanel panAddRemoveRef;
        private TableLayoutPanel panOKCancel;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabControl tc;
        private TextBox txtNS;

        static QueryProps()
        {
            _addRefMenuStrip.Items.Add("Add more references from this folder", null, (sender, e) => _addMoreRefsAction());
            _addRefMenuStrip.Items.Add("Browse this folder", null, (sender, e) => _browseRefsFolderAction());
        }

        public QueryProps(QueryCore q)
        {
            this._query = q;
            this.InitializeComponent();
            foreach (string str in q.AdditionalReferences)
            {
                this.AddListViewAssembly(str);
            }
            foreach (string str in q.AdditionalGACReferences)
            {
                this.AddListViewAssembly(new AssemblyName(str));
            }
            if (q.IsMyExtensions)
            {
                Label label = new Label {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    Text = "My Extensions: References that you add here will apply to all queries",
                    Padding = new Padding(0, 0, 0, 3),
                    Font = new Font(FontManager.GetDefaultFont(), FontStyle.Bold)
                };
                this.tabPage1.Controls.Add(label);
                this.btnSetAsDefault.Enabled = false;
            }
            else
            {
                QueryCore query = MyExtensions.Query;
                if (query != null)
                {
                    foreach (string str in query.AdditionalReferences)
                    {
                        this.FlagAsInherited(this.AddListViewAssembly(str));
                    }
                    foreach (string str in query.AdditionalGACReferences)
                    {
                        this.FlagAsInherited(this.AddListViewAssembly(new AssemblyName(str)));
                    }
                }
            }
            this.txtNS.Lines = q.AdditionalNamespaces.ToArray<string>();
            this.chkIncludePredicateBuilder.Checked = q.IncludePredicateBuilder;
            this._origNS = this.txtNS.Text.Trim();
            this.EnableControls();
            this.lvRefs.Sorting = SortOrder.Ascending;
            this.lvRefs.MouseMove += new MouseEventHandler(this.lvRefs_MouseMove);
            this.lvRefs.MouseDown += new MouseEventHandler(this.lvRefs_MouseDown);
            this.tc.TabPages[2].Dispose();
        }

        private ListViewItem AddListViewAssembly(AssemblyName name)
        {
            ListViewItem item;
            string path = string.IsNullOrEmpty(name.CodeBase) ? name.FullName : name.CodeBase;
            if (string.IsNullOrEmpty(name.CodeBase))
            {
                item = new ListViewItem(new string[] { name.Name, (name.Version == null) ? "" : name.Version.ToString() });
            }
            else
            {
                item = new ListViewItem(new string[] { Path.GetFileName(path), Path.GetDirectoryName(path) }) {
                    UseItemStyleForSubItems = false
                };
                if (Directory.Exists(Path.GetDirectoryName(path)))
                {
                    if (this._underlineFont == null)
                    {
                        this._underlineFont = new Font(this.Font, FontStyle.Underline);
                    }
                    item.SubItems[1].Font = this._underlineFont;
                    item.SubItems[1].ForeColor = Color.Blue;
                }
                else
                {
                    item.SubItems[1].ForeColor = Color.Red;
                }
                if (!File.Exists(path))
                {
                    item.SubItems[0].ForeColor = Color.Red;
                }
            }
            item.Tag = name;
            item.Name = path;
            this.lvRefs.Items.Add(item);
            return item;
        }

        private ListViewItem AddListViewAssembly(string path)
        {
            AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(path)) {
                CodeBase = path
            };
            return this.AddListViewAssembly(name);
        }

        private void Browse()
        {
            this.Browse(null);
        }

        private void Browse(string folder)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (folder != null)
                {
                    dialog.InitialDirectory = folder;
                }
                dialog.DefaultExt = ".dll";
                dialog.Filter = "Assemblies|*.dll;*.exe;*.winmd";
                dialog.Multiselect = true;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.lvRefs.SelectedItems.Clear();
                    foreach (string str in dialog.FileNames)
                    {
                        this._refChanges = true;
                        foreach (ListViewItem item in this.GetItemsWithSameIdentity(str).ToArray<ListViewItem>())
                        {
                            this.lvRefs.Items.Remove(item);
                        }
                        this.AddListViewAssembly(str).Selected = true;
                    }
                    this.SortAssemblies();
                    this.EnableControls();
                    this.btnOK.Focus();
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (AddAssembly assembly = new AddAssembly())
            {
                if (assembly.ShowDialog(this) != DialogResult.Cancel)
                {
                    if (assembly.Browse)
                    {
                        this.Browse();
                        this.btnOK.Focus();
                    }
                    else
                    {
                        this.lvRefs.SelectedItems.Clear();
                        foreach (AssemblyName name in assembly.GetChosenAssemblies())
                        {
                            this._refChanges = true;
                            foreach (ListViewItem item in this.GetItemsWithSameIdentity(name).ToArray<ListViewItem>())
                            {
                                this.lvRefs.Items.Remove(item);
                            }
                            this.AddListViewAssembly(name).Selected = true;
                        }
                        this.SortAssemblies();
                        this.EnableControls();
                        this.btnOK.Focus();
                    }
                }
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.Browse();
            this.btnOK.Focus();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (this.lvRefs.SelectedItems.Count != 0)
            {
                this._refChanges = true;
                foreach (ListViewItem item in new ArrayList(this.lvRefs.SelectedItems))
                {
                    if (item.Tag != null)
                    {
                        this.lvRefs.Items.Remove(item);
                    }
                }
                if (this.lvRefs.FocusedItem != null)
                {
                    this.lvRefs.FocusedItem.Selected = true;
                }
                this.lvRefs.Focus();
            }
        }

        private void btnSaveAsSnippet_Click(object sender, EventArgs e)
        {
            if (!MainForm.Instance.ShowLicensee)
            {
                MessageBox.Show("This feature requires an autocompletion license.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    try
                    {
                        dialog.InitialDirectory = UserOptions.Instance.GetCustomSnippetsFolder(true);
                    }
                    catch
                    {
                    }
                    dialog.Title = "Save As Snippet";
                    dialog.DefaultExt = "snippet";
                    dialog.Filter = "Code snippet files (*.snippet)|*.snippet";
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        this.SaveAsSnippet(dialog.FileName);
                    }
                }
                this.btnOK.Focus();
            }
        }

        private void btnSetAsDefault_Click(object sender, EventArgs e)
        {
            QueryCore.SetDefaultQueryProps(this.GetRefs(false).ToArray(), this.GetRefs(true).ToArray(), this.GetImports().ToArray(), this.chkIncludePredicateBuilder.Checked);
            MessageBox.Show("Done!", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                if (this._measureGraphics != null)
                {
                    this._measureGraphics.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void EnableControls()
        {
            if (this.lvRefs.SelectedItems.Count > 0)
            {
            }
            this.btnRemove.Enabled = (CS$<>9__CachedAnonymousMethodDelegate26 == null) && this.lvRefs.SelectedItems.Cast<ListViewItem>().All<ListViewItem>(CS$<>9__CachedAnonymousMethodDelegate26);
            this.btnSaveAsSnippet.Enabled = (this._query.IsMyExtensions || (this.lvRefs.Items.Count <= 0)) ? (this.txtNS.Text.Trim().Length > 0) : true;
        }

        private void FlagAsInherited(ListViewItem item)
        {
            item.Tag = null;
            item.ForeColor = item.SubItems[1].ForeColor = Color.Gray;
            item.Text = "(" + item.Text + " - from My Extensions)";
        }

        private ListViewItem GetHyperlinkedItemAtPoint(Point p)
        {
            ListViewItem itemAt = this.lvRefs.GetItemAt(p.X, p.Y);
            if (itemAt == null)
            {
                return null;
            }
            ListViewItem.ListViewSubItem subItemAt = itemAt.GetSubItemAt(p.X, p.Y);
            if (!((subItemAt != null) && subItemAt.Font.Underline))
            {
                return null;
            }
            if (this._measureGraphics == null)
            {
                this._measureGraphics = base.CreateGraphics();
            }
            int width = this._measureGraphics.MeasureString(subItemAt.Text, subItemAt.Font, subItemAt.Bounds.Width).ToSize().Width;
            if (p.X > (subItemAt.Bounds.X + width))
            {
                return null;
            }
            return itemAt;
        }

        private List<string> GetImports()
        {
            return (from s in from s in this.txtNS.Lines select s.Trim()
                select s.EndsWith(";") ? ((IEnumerable<string>) s.Substring(0, s.Length - 1)) : ((IEnumerable<string>) s) into s
                where s.Length > 0
                select s).ToList<string>();
        }

        private IEnumerable<ListViewItem> GetItemsWithSameIdentity(AssemblyName name)
        {
            string str = string.IsNullOrEmpty(name.CodeBase) ? name.FullName : name.CodeBase;
            return this.GetItemsWithSameIdentity(str);
        }

        private IEnumerable<ListViewItem> GetItemsWithSameIdentity(string name)
        {
            if (name.Contains<char>(','))
            {
                name = name.Split(new char[] { ',' }).First<string>();
            }
            else
            {
                name = Path.GetFileNameWithoutExtension(name);
            }
            return (from item in this.lvRefs.Items.OfType<ListViewItem>()
                let an = item.Tag as AssemblyName
                where an != null
                let identity = string.IsNullOrEmpty(an.CodeBase) ? ((IEnumerable<ListViewItem>) an.FullName.Split(new char[] { ',' }).First<string>()) : ((IEnumerable<ListViewItem>) Path.GetFileNameWithoutExtension(an.CodeBase))
                where string.Equals(name, identity, StringComparison.InvariantCultureIgnoreCase)
                select item);
        }

        private List<string> GetRefs(bool gac)
        {
            return (from item in (from item in this.lvRefs.Items.OfType<ListViewItem>() select item.Tag).OfType<AssemblyName>()
                where gac == string.IsNullOrEmpty(item.CodeBase)
                select gac ? ((IEnumerable<string>) item.FullName) : ((IEnumerable<string>) item.CodeBase)).ToList<string>();
        }

        private void InitializeComponent()
        {
            this.tc = new TabControl();
            this.tabPage1 = new TabPage();
            this.lvRefs = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.columnHeader2 = new ColumnHeader();
            this.panAddRemoveRef = new TableLayoutPanel();
            this.btnBrowse = new Button();
            this.btnAdd = new Button();
            this.btnRemove = new Button();
            this.chkIncludePredicateBuilder = new CheckBox();
            this.tabPage2 = new TabPage();
            this.txtNS = new TextBox();
            this.label1 = new Label();
            this.tabPage3 = new TabPage();
            this.groupBox1 = new GroupBox();
            this.flowLayoutPanel1 = new FlowLayoutPanel();
            this.label3 = new Label();
            this.llPrefsAdvanced = new LinkLabel();
            this.label2 = new Label();
            this.chkNoShadow = new CheckBox();
            this.panOKCancel = new TableLayoutPanel();
            this.btnSaveAsSnippet = new Button();
            this.btnCancel = new Button();
            this.btnOK = new Button();
            this.btnSetAsDefault = new Button();
            this.tc.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panAddRemoveRef.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.tc.Controls.Add(this.tabPage1);
            this.tc.Controls.Add(this.tabPage2);
            this.tc.Controls.Add(this.tabPage3);
            this.tc.Dock = DockStyle.Fill;
            this.tc.Location = new Point(5, 6);
            this.tc.Name = "tc";
            this.tc.SelectedIndex = 0;
            this.tc.Size = new Size(0x2a5, 550);
            this.tc.TabIndex = 0;
            this.tc.SelectedIndexChanged += new EventHandler(this.tc_SelectedIndexChanged);
            this.tabPage1.Controls.Add(this.lvRefs);
            this.tabPage1.Controls.Add(this.panAddRemoveRef);
            this.tabPage1.Location = new Point(4, 0x1a);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new Padding(5, 6, 5, 5);
            this.tabPage1.Size = new Size(0x29d, 520);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Additional References";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.lvRefs.Columns.AddRange(new ColumnHeader[] { this.columnHeader1, this.columnHeader2 });
            this.lvRefs.Dock = DockStyle.Fill;
            this.lvRefs.FullRowSelect = true;
            this.lvRefs.HideSelection = false;
            this.lvRefs.Location = new Point(5, 6);
            this.lvRefs.Name = "lvRefs";
            this.lvRefs.Size = new Size(0x293, 0x1da);
            this.lvRefs.TabIndex = 0;
            this.lvRefs.UseCompatibleStateImageBehavior = false;
            this.lvRefs.View = View.Details;
            this.lvRefs.SelectedIndexChanged += new EventHandler(this.lvRefs_SelectedIndexChanged);
            this.lvRefs.KeyDown += new KeyEventHandler(this.lvRefs_KeyDown);
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 0x107;
            this.columnHeader2.Text = "Location";
            this.columnHeader2.Width = 0x2bb;
            this.panAddRemoveRef.AutoSize = true;
            this.panAddRemoveRef.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panAddRemoveRef.ColumnCount = 4;
            this.panAddRemoveRef.ColumnStyles.Add(new ColumnStyle());
            this.panAddRemoveRef.ColumnStyles.Add(new ColumnStyle());
            this.panAddRemoveRef.ColumnStyles.Add(new ColumnStyle());
            this.panAddRemoveRef.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panAddRemoveRef.Controls.Add(this.btnBrowse, 0, 0);
            this.panAddRemoveRef.Controls.Add(this.btnAdd, 0, 0);
            this.panAddRemoveRef.Controls.Add(this.btnRemove, 2, 0);
            this.panAddRemoveRef.Controls.Add(this.chkIncludePredicateBuilder, 3, 0);
            this.panAddRemoveRef.Dock = DockStyle.Bottom;
            this.panAddRemoveRef.Location = new Point(5, 480);
            this.panAddRemoveRef.Name = "panAddRemoveRef";
            this.panAddRemoveRef.Padding = new Padding(0, 3, 0, 0);
            this.panAddRemoveRef.RowCount = 1;
            this.panAddRemoveRef.RowStyles.Add(new RowStyle());
            this.panAddRemoveRef.Size = new Size(0x293, 0x23);
            this.panAddRemoveRef.TabIndex = 1;
            this.btnBrowse.Location = new Point(0x58, 6);
            this.btnBrowse.Margin = new Padding(0, 3, 3, 0);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new Size(0x55, 0x1d);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "&Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
            this.btnAdd.Location = new Point(0, 6);
            this.btnAdd.Margin = new Padding(0, 3, 3, 0);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new Size(0x55, 0x1d);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "&Add...";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);
            this.btnRemove.Location = new Point(0xb3, 6);
            this.btnRemove.Margin = new Padding(3, 3, 3, 0);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new Size(0x55, 0x1d);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "&Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new EventHandler(this.btnRemove_Click);
            this.chkIncludePredicateBuilder.AutoSize = true;
            this.chkIncludePredicateBuilder.Dock = DockStyle.Right;
            this.chkIncludePredicateBuilder.Location = new Point(0x1d0, 6);
            this.chkIncludePredicateBuilder.Name = "chkIncludePredicateBuilder";
            this.chkIncludePredicateBuilder.Padding = new Padding(0x12, 3, 0, 0);
            this.chkIncludePredicateBuilder.Size = new Size(0xc0, 0x1a);
            this.chkIncludePredicateBuilder.TabIndex = 3;
            this.chkIncludePredicateBuilder.Text = "Include PredicateBuilder";
            this.chkIncludePredicateBuilder.UseVisualStyleBackColor = true;
            this.tabPage2.Controls.Add(this.txtNS);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Location = new Point(4, 0x1a);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new Padding(5, 6, 5, 6);
            this.tabPage2.Size = new Size(0x29d, 520);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Additional Namespace Imports";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.txtNS.AcceptsReturn = true;
            this.txtNS.Dock = DockStyle.Fill;
            this.txtNS.Location = new Point(5, 0x1d);
            this.txtNS.Multiline = true;
            this.txtNS.Name = "txtNS";
            this.txtNS.ScrollBars = ScrollBars.Both;
            this.txtNS.Size = new Size(0x293, 0x1e5);
            this.txtNS.TabIndex = 1;
            this.txtNS.WordWrap = false;
            this.txtNS.TextChanged += new EventHandler(this.txtNS_TextChanged);
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(5, 6);
            this.label1.Margin = new Padding(0, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 4);
            this.label1.Size = new Size(0xf9, 0x17);
            this.label1.TabIndex = 0;
            this.label1.Text = "List each namespace on a separate line:";
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Location = new Point(4, 0x1a);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new Padding(10);
            this.tabPage3.Size = new Size(0x29d, 520);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Advanced";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.chkNoShadow);
            this.groupBox1.Dock = DockStyle.Top;
            this.groupBox1.Location = new Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new Padding(12, 5, 5, 10);
            this.groupBox1.Size = new Size(0x289, 120);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Assembly Shadowing";
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.llPrefsAdvanced);
            this.flowLayoutPanel1.Dock = DockStyle.Top;
            this.flowLayoutPanel1.Location = new Point(12, 0x5b);
            this.flowLayoutPanel1.Margin = new Padding(0, 3, 3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new Size(0x278, 0x13);
            this.flowLayoutPanel1.TabIndex = 3;
            this.label3.AutoSize = true;
            this.label3.Dock = DockStyle.Top;
            this.label3.Location = new Point(0, 0);
            this.label3.Margin = new Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x14f, 0x13);
            this.label3.TabIndex = 3;
            this.label3.Text = "You can globally disable assembly shadowing in";
            this.llPrefsAdvanced.AutoSize = true;
            this.llPrefsAdvanced.Location = new Point(0x14f, 0);
            this.llPrefsAdvanced.Margin = new Padding(0);
            this.llPrefsAdvanced.Name = "llPrefsAdvanced";
            this.llPrefsAdvanced.Size = new Size(0xb8, 0x13);
            this.llPrefsAdvanced.TabIndex = 4;
            this.llPrefsAdvanced.TabStop = true;
            this.llPrefsAdvanced.Text = "Edit | Preferences | Advanced";
            this.llPrefsAdvanced.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llPrefsAdvanced_LinkClicked);
            this.label2.AutoSize = true;
            this.label2.Dock = DockStyle.Top;
            this.label2.Location = new Point(12, 0x30);
            this.label2.Name = "label2";
            this.label2.Padding = new Padding(0, 0, 0, 5);
            this.label2.Size = new Size(0x22c, 0x2b);
            this.label2.TabIndex = 1;
            this.label2.Text = "Shadowing avoids locking referenced assemblies so you can rebuild them in Visual Studio.\r\nShadowing may be incompatible with frameworks that dynamically load assemblies.";
            this.chkNoShadow.AutoSize = true;
            this.chkNoShadow.Dock = DockStyle.Top;
            this.chkNoShadow.Location = new Point(12, 0x17);
            this.chkNoShadow.Name = "chkNoShadow";
            this.chkNoShadow.Padding = new Padding(0, 0, 0, 2);
            this.chkNoShadow.Size = new Size(0x278, 0x19);
            this.chkNoShadow.TabIndex = 0;
            this.chkNoShadow.Text = "Disable assembly shadowing for this query";
            this.chkNoShadow.UseVisualStyleBackColor = true;
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 4;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnSaveAsSnippet, 0, 0);
            this.panOKCancel.Controls.Add(this.btnCancel, 3, 0);
            this.panOKCancel.Controls.Add(this.btnOK, 2, 0);
            this.panOKCancel.Controls.Add(this.btnSetAsDefault, 0, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(5, 0x22c);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 4, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x2a5, 0x27);
            this.panOKCancel.TabIndex = 1;
            this.btnSaveAsSnippet.Location = new Point(0xd9, 7);
            this.btnSaveAsSnippet.Name = "btnSaveAsSnippet";
            this.btnSaveAsSnippet.Size = new Size(0x8a, 0x1d);
            this.btnSaveAsSnippet.TabIndex = 1;
            this.btnSaveAsSnippet.Text = "&Save as snippet...";
            this.btnSaveAsSnippet.UseVisualStyleBackColor = true;
            this.btnSaveAsSnippet.Click += new EventHandler(this.btnSaveAsSnippet_Click);
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x255, 7);
            this.btnCancel.Margin = new Padding(3, 3, 1, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0x200, 7);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 0x1d);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnSetAsDefault.Location = new Point(0, 7);
            this.btnSetAsDefault.Margin = new Padding(0, 3, 3, 3);
            this.btnSetAsDefault.Name = "btnSetAsDefault";
            this.btnSetAsDefault.Size = new Size(0xd3, 0x1d);
            this.btnSetAsDefault.TabIndex = 0;
            this.btnSetAsDefault.Text = "Set as default for new queries";
            this.btnSetAsDefault.UseVisualStyleBackColor = true;
            this.btnSetAsDefault.Click += new EventHandler(this.btnSetAsDefault_Click);
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x2af, 600);
            base.ControlBox = false;
            base.Controls.Add(this.tc);
            base.Controls.Add(this.panOKCancel);
            base.Location = new Point(0, 0);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "QueryProps";
            base.Padding = new Padding(5, 6, 5, 5);
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Query Properties";
            this.tc.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.panAddRemoveRef.ResumeLayout(false);
            this.panAddRemoveRef.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void llPrefsAdvanced_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (OptionsForm form = new OptionsForm(5))
            {
                form.ShowDialog();
            }
        }

        private void lvRefs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                this.btnRemove_Click(sender, e);
            }
        }

        private void lvRefs_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right))
            {
                Action action = null;
                Action action2 = null;
                ListViewItem item = this.GetHyperlinkedItemAtPoint(e.Location);
                if (item != null)
                {
                    if (action == null)
                    {
                        action = () => this.Browse(item.SubItems[1].Text);
                    }
                    _addMoreRefsAction = action;
                    if (action2 == null)
                    {
                        action2 = () => Process.Start(item.SubItems[1].Text);
                    }
                    _browseRefsFolderAction = action2;
                    _addRefMenuStrip.Show(Control.MousePosition);
                }
            }
        }

        private void lvRefs_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.GetHyperlinkedItemAtPoint(e.Location) != null)
            {
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void lvRefs_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        protected override void OnActivated(EventArgs e)
        {
            if (!this._activated)
            {
                this._activated = true;
                this.btnAdd.Focus();
            }
            base.OnActivated(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if ((base.DialogResult == DialogResult.OK) && ((this._refChanges || (this._origNS != this.txtNS.Text.Trim())) || (this._query.IncludePredicateBuilder != this.chkIncludePredicateBuilder.Checked)))
            {
                using (this._query.TransactChanges())
                {
                    this._query.AdditionalReferences = this.GetRefs(false).ToArray();
                    this._query.AdditionalGACReferences = this.GetRefs(true).ToArray();
                    this._query.SortReferences();
                    this._query.AdditionalNamespaces = this.GetImports().ToArray();
                    this._query.IncludePredicateBuilder = this.chkIncludePredicateBuilder.Checked;
                    this._query.IsModified = true;
                    this._query.OnQueryChanged();
                }
            }
            base.OnClosed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.lvRefs.Columns[0].Width = this.lvRefs.ClientSize.Width / 2;
            Rectangle workingArea = Screen.GetWorkingArea(this);
            int num = (base.Width * 3) / 2;
            base.Width = Math.Min(workingArea.Width - 10, num);
            base.Left = (workingArea.Width - base.Width) / 2;
        }

        private void SaveAsSnippet(string filePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            XNamespace ns = "http://schemas.microsoft.com/VisualStudio/2005/CodeSnippet";
            XElement element = new XElement((XName) (ns + "CodeSnippets"), new XElement((XName) (ns + "CodeSnippet"), new object[] { new XAttribute("Format", "1.0.0"), new XElement((XName) (ns + "Header"), new object[] { new XElement((XName) (ns + "Title"), fileNameWithoutExtension + " - assemblies and namespaces"), new XElement((XName) (ns + "Description"), fileNameWithoutExtension + " - assemblies and namespaces"), new XElement((XName) (ns + "Shortcut"), fileNameWithoutExtension.Replace(" ", "")), new XElement((XName) (ns + "SnippetTypes"), new XElement((XName) (ns + "SnippetType"), "Expansion")), this.chkNoShadow.Checked ? new XElement((XName) (ns + "Keywords"), new XElement((XName) (ns + "Keyword"), "LINQPad.NoShadowing")) : null }), new XElement((XName) (ns + "Snippet"), new object[] { new XElement((XName) (ns + "References"), from r in this.GetRefs(false).Concat<string>(this.GetRefs(true)) select new XElement((XName) (ns + "Reference"), new XElement((XName) (ns + "Assembly"), r))), new XElement((XName) (ns + "Imports"), from import in this.GetImports() select new XElement((XName) (ns + "Import"), new XElement((XName) (ns + "Namespace"), import))), new XElement((XName) (ns + "Code"), new object[] { new XAttribute("Language", "csharp"), new XCData("$end$") }) }) }));
            try
            {
                element.Save(filePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error saving file: " + exception.Message);
                return;
            }
            SnippetManager.ClearSnippetCache();
            SnippetManager.set_LINQPadSnippetsFolder(UserOptions.Instance.GetCustomSnippetsFolder(false));
        }

        private void SortAssemblies()
        {
        }

        private void tc_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void txtNS_TextChanged(object sender, EventArgs e)
        {
            if (this.txtNS.Text.Contains("using ") || this.txtNS.Text.Contains("using\t"))
            {
                string[] source = this.txtNS.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                if (source.Any<string>(l => l.StartsWith("using ") || l.StartsWith("using\t")))
                {
                    this.txtNS.Text = string.Join("\r\n", (from l in source select (l.StartsWith("using ") || l.StartsWith("using\t")) ? ((IEnumerable<string>) l.Substring(6).TrimStart(new char[0])) : ((IEnumerable<string>) l)).ToArray<string>());
                }
            }
            this.EnableControls();
        }
    }
}

