namespace LINQPad.UI
{
    using LINQPad;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    internal class AddAssembly : BaseForm
    {
        private static string _baseRefPath = Path.Combine(PathHelper.ProgramFilesX86, "Reference Assemblies");
        private static List<AssemblyName> _gacAssemblies = new List<AssemblyName>();
        private static bool _lastIncludeGAC;
        private static bool _lastIncludeLatest = true;
        private static AssemblyName[] _latestGacAssemblies = new AssemblyName[0];
        private static string[] _latestRegFileAssemblies = new string[0];
        private static bool _populated;
        private static List<string> _refFileAssemblies = new List<string>();
        private static List<string> _regFileAssemblies = new List<string>();
        public bool Browse;
        private Button btnBrowse;
        private Button btnCancel;
        private Button btnOK;
        private Button btnRefresh;
        private ColumnHeader chDir;
        private CheckBox chkGAC;
        private CheckBox chkLatest;
        private ColumnHeader chName;
        private IContainer components = null;
        private ListView lvRefs;
        private TableLayoutPanel panOKCancel;
        private TableLayoutPanel panOptions;
        private TextBox txtFilter;
        private BackgroundWorker workerPopulate;

        public AddAssembly()
        {
            this.InitializeComponent();
            this.chkGAC.Checked = _lastIncludeGAC;
            this.chkLatest.Checked = _lastIncludeLatest;
            if (!_populated)
            {
                this.Start();
            }
            else
            {
                this.Text = "Add Custom Assembly References";
                this.PopulateList();
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.lvRefs.SelectedItems.Clear();
            this.Browse = true;
            base.DialogResult = DialogResult.OK;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!this.workerPopulate.IsBusy)
            {
                this.lvRefs.Items.Clear();
                _gacAssemblies.Clear();
                _latestGacAssemblies = new AssemblyName[0];
                _refFileAssemblies.Clear();
                _regFileAssemblies.Clear();
                _latestRegFileAssemblies = new string[0];
                _populated = false;
                this.Start();
            }
        }

        private void chkClient_Click(object sender, EventArgs e)
        {
            this.PopulateList();
            _lastIncludeLatest = this.chkLatest.Checked;
            this.txtFilter.Focus();
        }

        private void chkGAC_Click(object sender, EventArgs e)
        {
            this.PopulateList();
            _lastIncludeGAC = this.chkGAC.Checked;
            this.txtFilter.Focus();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private AssemblyName GetAssemblyName(string path)
        {
            try
            {
                return new AssemblyName(Path.GetFileNameWithoutExtension(path)) { CodeBase = path };
            }
            catch
            {
                return null;
            }
        }

        public List<AssemblyName> GetChosenAssemblies()
        {
            return (from item in this.lvRefs.SelectedItems.OfType<ListViewItem>() select item.Tag).OfType<AssemblyName>().ToList<AssemblyName>();
        }

        private IEnumerable<string> GetExtraAssemblies()
        {
            Exception exception;
            List<string> list = new List<string>();
            try
            {
                string name = string.Concat(new object[] { @"SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v", Environment.Version.Major, ".", Environment.Version.Minor, ".", Environment.Version.Build, @"\AssemblyFoldersEx" });
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(name, false))
                {
                    foreach (string str2 in key.GetSubKeyNames())
                    {
                        RegistryKey key2 = key.OpenSubKey(str2);
                        try
                        {
                            string path = key2.GetValue(null) as string;
                            if (((path != null) && !path.StartsWith(_baseRefPath, StringComparison.InvariantCultureIgnoreCase)) && Directory.Exists(path))
                            {
                                list.AddRange(Directory.GetFiles(path, "*.dll"));
                            }
                        }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                            Log.Write(exception);
                        }
                        finally
                        {
                            if (key2 != null)
                            {
                                key2.Dispose();
                            }
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                exception = exception2;
                Log.Write(exception);
            }
            return list;
        }

        private IEnumerable<string> GetRefAssemblies(string path)
        {
            if (!Directory.Exists(path))
            {
                return new string[0];
            }
            if (path.ToLowerInvariant().Contains(@"\temporary asp.net files"))
            {
                return new string[0];
            }
            if (path.ToLowerInvariant().Contains(@"\setupcache"))
            {
                return new string[0];
            }
            if (path.ToLowerInvariant().Contains(@"framework\v3.5"))
            {
                return new string[0];
            }
            IEnumerable<string> first = from f in Directory.GetFiles(path, "*.dll")
                where !f.ToLowerInvariant().EndsWith(".resources.dll")
                select f;
            foreach (string str in Directory.GetDirectories(path))
            {
                first = first.Concat<string>(this.GetRefAssemblies(str));
            }
            return first;
        }

        private void InitializeComponent()
        {
            ListViewItem item = new ListViewItem("Populating...");
            this.panOKCancel = new TableLayoutPanel();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.btnBrowse = new Button();
            this.lvRefs = new ListView();
            this.chName = new ColumnHeader();
            this.chDir = new ColumnHeader();
            this.workerPopulate = new BackgroundWorker();
            this.chkLatest = new CheckBox();
            this.chkGAC = new CheckBox();
            this.panOptions = new TableLayoutPanel();
            this.txtFilter = new TextBox();
            this.btnRefresh = new Button();
            this.panOKCancel.SuspendLayout();
            this.panOptions.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 4;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnRefresh, 1, 0);
            this.panOKCancel.Controls.Add(this.btnOK, 2, 0);
            this.panOKCancel.Controls.Add(this.btnCancel, 3, 0);
            this.panOKCancel.Controls.Add(this.btnBrowse, 0, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(6, 630);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 5, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x45e, 0x25);
            this.panOKCancel.TabIndex = 2;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0x3ba, 8);
            this.btnOK.Margin = new Padding(3, 3, 3, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 0x1d);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x40f, 8);
            this.btnCancel.Margin = new Padding(3, 3, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnBrowse.Location = new Point(0, 8);
            this.btnBrowse.Margin = new Padding(0, 3, 3, 0);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new Size(0x58, 0x1d);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "&Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
            this.lvRefs.Columns.AddRange(new ColumnHeader[] { this.chName, this.chDir });
            this.lvRefs.Dock = DockStyle.Fill;
            this.lvRefs.FullRowSelect = true;
            this.lvRefs.HideSelection = false;
            this.lvRefs.Items.AddRange(new ListViewItem[] { item });
            this.lvRefs.Location = new Point(6, 0x26);
            this.lvRefs.Name = "lvRefs";
            this.lvRefs.Size = new Size(0x45e, 0x250);
            this.lvRefs.TabIndex = 1;
            this.lvRefs.UseCompatibleStateImageBehavior = false;
            this.lvRefs.View = View.Details;
            this.lvRefs.ItemActivate += new EventHandler(this.lvRefs_ItemActivate);
            this.lvRefs.ColumnClick += new ColumnClickEventHandler(this.lvRefs_ColumnClick);
            this.chName.Text = "Name";
            this.chName.Width = 0x187;
            this.chDir.Text = "Location";
            this.chDir.Width = 0x2b3;
            this.workerPopulate.DoWork += new DoWorkEventHandler(this.workerPopulate_DoWork);
            this.workerPopulate.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.workerPopulate_RunWorkerCompleted);
            this.chkLatest.Anchor = AnchorStyles.Left;
            this.chkLatest.AutoSize = true;
            this.chkLatest.Location = new Point(0x3a8, 3);
            this.chkLatest.Margin = new Padding(3, 3, 0, 1);
            this.chkLatest.Name = "chkLatest";
            this.chkLatest.Size = new Size(0xb6, 0x17);
            this.chkLatest.TabIndex = 2;
            this.chkLatest.TabStop = false;
            this.chkLatest.Text = "Show &latest versions only";
            this.chkLatest.UseVisualStyleBackColor = true;
            this.chkLatest.Click += new EventHandler(this.chkClient_Click);
            this.chkGAC.Anchor = AnchorStyles.Left;
            this.chkGAC.AutoSize = true;
            this.chkGAC.Location = new Point(0x300, 3);
            this.chkGAC.Margin = new Padding(12, 3, 3, 1);
            this.chkGAC.Name = "chkGAC";
            this.chkGAC.Size = new Size(0xa2, 0x17);
            this.chkGAC.TabIndex = 1;
            this.chkGAC.TabStop = false;
            this.chkGAC.Text = "Show &GAC assemblies";
            this.chkGAC.UseVisualStyleBackColor = true;
            this.chkGAC.Click += new EventHandler(this.chkGAC_Click);
            this.panOptions.AutoSize = true;
            this.panOptions.ColumnCount = 3;
            this.panOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOptions.ColumnStyles.Add(new ColumnStyle());
            this.panOptions.ColumnStyles.Add(new ColumnStyle());
            this.panOptions.Controls.Add(this.chkGAC, 1, 0);
            this.panOptions.Controls.Add(this.chkLatest, 2, 0);
            this.panOptions.Controls.Add(this.txtFilter, 0, 0);
            this.panOptions.Dock = DockStyle.Top;
            this.panOptions.Location = new Point(6, 7);
            this.panOptions.Name = "panOptions";
            this.panOptions.Padding = new Padding(0, 0, 0, 4);
            this.panOptions.RowCount = 1;
            this.panOptions.RowStyles.Add(new RowStyle());
            this.panOptions.Size = new Size(0x45e, 0x1f);
            this.panOptions.TabIndex = 0;
            this.txtFilter.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this.txtFilter.Location = new Point(0, 1);
            this.txtFilter.Margin = new Padding(0);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new Size(0x2f4, 0x19);
            this.txtFilter.TabIndex = 0;
            this.txtFilter.TextChanged += new EventHandler(this.txtFilter_TextChanged);
            this.txtFilter.KeyDown += new KeyEventHandler(this.txtFilter_KeyDown);
            this.btnRefresh.Location = new Point(0x5b, 8);
            this.btnRefresh.Margin = new Padding(0, 3, 3, 0);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(0x58, 0x1d);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "&Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x46a, 0x2a2);
            base.ControlBox = false;
            base.Controls.Add(this.lvRefs);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.panOptions);
            base.Location = new Point(0, 0);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "AddAssembly";
            base.Padding = new Padding(6, 7, 6, 7);
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Add Custom Assembly References (Working...)";
            this.panOKCancel.ResumeLayout(false);
            this.panOptions.ResumeLayout(false);
            this.panOptions.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void lvRefs_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.lvRefs.ListViewItemSorter = new ComparisonComparer<ListViewItem>((o1, o2) => o1.SubItems[e.Column].Text.CompareTo(o2.SubItems[e.Column].Text));
            this.lvRefs.Sort();
        }

        private void lvRefs_ItemActivate(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Rectangle workingArea = Screen.GetWorkingArea(this);
            if (base.Width > (workingArea.Width - 10))
            {
                base.Width = workingArea.Width - 10;
                base.Left = (workingArea.Width - base.Width) / 2;
                this.lvRefs.Columns[0].Width = this.lvRefs.ClientSize.Width / 3;
            }
        }

        private void PopulateLatestFiles()
        {
            _latestRegFileAssemblies = new string[0];
            _latestGacAssemblies = new AssemblyName[0];
            _latestGacAssemblies = (from g in _gacAssemblies
                group g by g.Name into g
                select (from a in g
                    orderby a.Version descending
                    select a).Take<AssemblyName>(1)).ToArray<AssemblyName>();
            var enumerable = from f in _regFileAssemblies
                group f by Path.GetFileName(f) into g
                let multi = g.Count<string>() > 1
                select new { Key = g.Key, VersionedPaths = (from f in g select new VersionedFilePath(f, multi)).ToArray<VersionedFilePath>() };
            IEnumerable<string> first = from g in enumerable
                select from p in g.VersionedPaths
                    where p.IsValid
                    select p into valid
                where valid.Count<VersionedFilePath>() > 0
                select (from f in valid
                    orderby f.Version descending
                    select f).First<VersionedFilePath>().FullPath;
            IEnumerable<string> second = from g in enumerable
                select from p in g.VersionedPaths
                    where !p.IsValid
                    select p.FullPath into invalid
                from x in invalid
                select x;
            _latestRegFileAssemblies = (from f in first.Concat<string>(second)
                orderby f
                select f).ToArray<string>();
        }

        private void PopulateList()
        {
            string filter;
            if (!base.IsDisposed && !this.lvRefs.IsDisposed)
            {
                this.lvRefs.Items.Clear();
                if (((_refFileAssemblies.Count == 0) && (_regFileAssemblies.Count == 0)) && _populated)
                {
                    this.lvRefs.Items.Add("<Unable to retrieve reference assemblies>");
                }
                if (((_gacAssemblies.Count == 0) && this.chkGAC.Checked) && _populated)
                {
                    this.lvRefs.Items.Add("<Unable to retrieve GAC assemblies>");
                }
                IEnumerable<string> first = _refFileAssemblies;
                if (this.chkLatest.Checked && (_latestRegFileAssemblies.Length > 0))
                {
                    first = first.Concat<string>(_latestRegFileAssemblies);
                }
                else
                {
                    first = first.Concat<string>(_regFileAssemblies);
                }
                IEnumerable<ListViewItem> enumerable2 = from s in first
                    where !s.ToLowerInvariant().Contains(@"\profile\client\") && !s.ToLowerInvariant().Contains(@"\profile\server core\")
                    select new ListViewItem(new string[] { Path.GetFileName(s), Path.GetDirectoryName(s) }) { Tag = this.GetAssemblyName(s) };
                IEnumerable<ListViewItem> second = from a in (!this.chkLatest.Checked || (_latestGacAssemblies.Length <= 0)) ? ((IEnumerable<ListViewItem>) _gacAssemblies) : ((IEnumerable<ListViewItem>) ((IEnumerable<AssemblyName>) _latestGacAssemblies))
                    where this.chkGAC.Checked
                    select new ListViewItem(new string[] { a.Name + ".dll", a.Version.ToString() }) { Tag = a };
                filter = this.txtFilter.Text.Trim().ToLowerInvariant();
                IOrderedEnumerable<ListViewItem> source = from li in (from fi in enumerable2
                    where fi.Tag != null
                    select fi).Concat<ListViewItem>(second)
                    where (filter.Length < 2) || li.Text.ToLowerInvariant().Contains(filter)
                    orderby Path.GetFileNameWithoutExtension(li.Text).ToLowerInvariant()
                    select li;
                this.lvRefs.Items.AddRange(source.ToArray<ListViewItem>());
                if (this.lvRefs.Items.Count == 1)
                {
                    this.lvRefs.Items[0].Focused = true;
                    this.lvRefs.Items[0].Selected = true;
                }
                else if ((this.lvRefs.Items.Count > 1) && (filter.Length >= 2))
                {
                    ListViewItem item = this.lvRefs.FindItemWithText(filter, false, 0, true) ?? this.lvRefs.Items[0];
                    item.Focused = true;
                    item.Selected = true;
                    item.EnsureVisible();
                }
            }
        }

        private void Start()
        {
            this.btnRefresh.Enabled = false;
            this.workerPopulate.RunWorkerAsync();
        }

        private void txtFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.Down) || (e.KeyData == Keys.Up))
            {
                e.Handled = true;
                this.lvRefs.Focus();
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            this.PopulateList();
        }

        private void workerPopulate_DoWork(object sender, DoWorkEventArgs e)
        {
            string runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            IEnumerable<string> refAssemblies = this.GetRefAssemblies(runtimeDirectory);
            HashSet<string> coreFilesSet = new HashSet<string>(from p in refAssemblies select Path.GetFileName(p), StringComparer.OrdinalIgnoreCase);
            IEnumerable<string> second = from p in this.GetRefAssemblies(_baseRefPath)
                where !coreFilesSet.Contains(Path.GetFileName(p))
                select p;
            _refFileAssemblies = refAssemblies.Concat<string>(second).ToList<string>();
            if (!base.IsHandleCreated)
            {
                Thread.Sleep(500);
            }
            if (base.IsHandleCreated)
            {
                Exception exception;
                base.BeginInvoke(new Action(this.PopulateList));
                try
                {
                    _regFileAssemblies = this.GetExtraAssemblies().Distinct<string>().Except<string>(_refFileAssemblies).ToList<string>();
                }
                catch (Exception exception1)
                {
                    exception = exception1;
                    Log.Write(exception);
                }
                try
                {
                    Fusion.IAssemblyEnum enum2;
                    Fusion.IAssemblyName name;
                    List<string> source = new List<string>();
                    Fusion.CreateAssemblyEnum(out enum2, IntPtr.Zero, null, Fusion.ASM_CACHE_FLAGS.ASM_CACHE_GAC, IntPtr.Zero);
                    while (enum2.GetNextAssembly(IntPtr.Zero, out name, 0) == 0)
                    {
                        name.GetDisplayName(null, 0, 0);
                        StringBuilder szDisplayName = new StringBuilder((int) pccDisplayName);
                        name.GetDisplayName(szDisplayName, ref pccDisplayName, 0);
                        source.Add(szDisplayName.ToString());
                    }
                    _gacAssemblies = (from n in source.Distinct<string>()
                        select new AssemblyName(n) into a
                        where !a.Name.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase)
                        select a).ToList<AssemblyName>();
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    Log.Write(exception);
                }
                try
                {
                    this.PopulateLatestFiles();
                }
                catch (Exception exception3)
                {
                    exception = exception3;
                    Log.Write(exception);
                }
                _populated = true;
            }
        }

        private void workerPopulate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!base.IsDisposed && !this.lvRefs.IsDisposed)
            {
                if (e.Error != null)
                {
                    Program.ProcessException(e.Error);
                }
                else
                {
                    if ((_refFileAssemblies.Count == 0) && (_regFileAssemblies.Count == 0))
                    {
                        this.chkGAC.Checked = true;
                    }
                    this.PopulateList();
                }
                this.Text = "Add Custom Assembly References";
                this.btnRefresh.Enabled = true;
            }
        }

        private class VersionedFilePath
        {
            public readonly string FileName;
            public readonly string FullPath;
            public readonly bool IsValid;
            public readonly System.Version Version;

            public VersionedFilePath(string fullPath, bool split)
            {
                Func<string, bool> versionPredicateBase;
                this.FullPath = fullPath;
                this.FileName = Path.GetFileName(fullPath);
                if (split)
                {
                    string[] source = fullPath.Split(new char[] { '\\' });
                    versionPredicateBase = delegate (string s) {
                        if (s.StartsWith("v", StringComparison.OrdinalIgnoreCase) && (s.Length > 1))
                        {
                        }
                        return (CS$<>9__CachedAnonymousMethodDelegate52 == null) && s.Substring(1).All<char>(CS$<>9__CachedAnonymousMethodDelegate52);
                    };
                    Func<string, bool> predicate = versionPredicateBase;
                    int num = source.Count<string>(predicate);
                    if ((num != 0) && (num <= 2))
                    {
                        if (num == 2)
                        {
                            string exclude = (from p in source.Where<string>(predicate)
                                orderby p.Count<char>(c => c == '.')
                                select p).First<string>();
                            predicate = s => !(s != exclude) ? false : versionPredicateBase(s);
                            if (source.Count<string>(predicate) != 1)
                            {
                                return;
                            }
                        }
                        if (source.Any<string>(predicate))
                        {
                            string str = source.First<string>(predicate).Substring(1);
                            int num2 = str.Count<char>(c => c == '.');
                            if (num2 <= 3)
                            {
                                System.Version version;
                                for (int i = num2; i < 3; i++)
                                {
                                    str = str + ".0";
                                }
                                try
                                {
                                    version = new System.Version(str);
                                }
                                catch
                                {
                                    return;
                                }
                                this.Version = version;
                                this.IsValid = true;
                            }
                        }
                    }
                }
            }
        }
    }
}

