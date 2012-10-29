namespace LINQPad.UI
{
    using ActiproBridge;
    using ActiproSoftware.SyntaxEditor;
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;

    [ComVisible(true)]
    internal class MainForm : BaseForm
    {
        private static bool _activated;
        private static bool _active;
        private static bool _appActive;
        private Timer _assemblyUnlockTimer;
        private Queue<QueryControl> _cacheQueryControls;
        private Timer _collectTimer;
        private DockHandle _dockHandle;
        private Timer _fakeActiveFormTimer;
        private bool _firstOpen;
        internal bool _isPremium;
        private FormWindowState _lastFormState;
        private int _lastMaxQueryRows;
        private RunnableQuery _lastQuery;
        private QueryControl _lastQueryControl;
        private FormWindowState _lastWindowState;
        private ToolStripSeparator _mruSeparator;
        private Timer _newQueryCacheTimer;
        private int _pendingGC;
        private Dictionary<RunnableQuery, TabPage> _queryPages;
        private bool _restart;
        private LINQPad.UI.ResultsDockForm _resultsDockForm;
        private bool _showLicensee;
        private bool _transparencyEnabled;
        private bool _treeViewQuerySelectSuspended;
        private int _untitledCount;
        private bool _updateAvailable;
        internal static readonly ManualResetEvent AppStarted = new ManualResetEvent(false);
        private ClearButton btnCloseAppMsg;
        private Button btnRestart;
        private ToolStripMenuItem bugReportToolStripMenuItem;
        private ToolStripMenuItem c30InANutshellToolStripMenuItem;
        private ToolStripMenuItem cancelToolStripMenuItem;
        private ToolStripMenuItem cloneQueryMenuItem;
        private ToolStripMenuItem closeAllToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private IContainer components;
        private ToolStripMenuItem copyPlainToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem cutToolStripMenuItem;
        private EditManager editManager;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem executeToolStripMenuItem;
        private ToolStripSeparator exitSeparator;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem fAQToolStripMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem findAllToolStripMenuItem;
        private ToolStripMenuItem findAndReplaceToolStripMenuItem;
        private ToolStripMenuItem findNextToolStripMenuItem;
        private ToolStripMenuItem findPreviousToolStripMenuItem;
        private ToolStripMenuItem forumToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        internal static MainForm Instance;
        public bool IsSplitting;
        private Label label1;
        private Label lblAppMessage;
        private Label lblMessage;
        private ToolStripMenuItem lINQGeneralToolStripMenuItem;
        private ToolStripMenuItem lINQToEntitiesToolStripMenuItem;
        private ToolStripMenuItem lINQToSQLToolStripMenuItem;
        private ToolStripMenuItem lINQToXMLToolStripMenuItem;
        private LinkLabel llOrganize;
        private LinkLabel llSetFolder;
        private MenuStripEx mainMenu;
        private ToolStripMenuItem miActivateAutocompletion;
        private ToolStripMenuItem miActivateReflector;
        private ToolStripMenuItem miAutocompletion;
        private ToolStripMenuItem miAutoScroll;
        private ToolStripMenuItem miCheckForUpdates;
        private ToolStripMenuItem miCompleteParameters;
        private ToolStripMenuItem miCompleteWord;
        private ToolStripMenuItem miCopyMarkdown;
        private ToolStripMenuItem microsoftLINQForumsToolStripMenuItem;
        private ToolStripMenuItem miCustomerService;
        private ToolStripMenuItem miExecCmd;
        private ToolStripMenuItem miExecutionTracking;
        private ToolStripMenuItem miGC;
        private ToolStripSeparator miGCSeparator;
        private ToolStripMenuItem miGridResults;
        private ToolStripMenuItem miHideExplorers;
        private ToolStripMenuItem miIncrementalSearch;
        private ToolStripMenuItem miInsertSnippet;
        private ToolStripMenuItem miJumpToExecutionPoint;
        private ToolStripMenuItem miListMembers;
        private ToolStripMenuItem miListTables;
        private ToolStripMenuItem miManageActivations;
        private ToolStripMenuItem miMemberHelp;
        private ToolStripMenuItem miPasswordManager;
        private ToolStripMenuItem miPasteEscapedString;
        private ToolStripSeparator miSep1;
        private ToolStripMenuItem miSuggestion;
        private ToolStripMenuItem miSurroundWith;
        private ToolStripMenuItem miTextResults;
        private ToolStripMenuItem miUndockResults;
        private ToolStripMenuItem miUnload;
        private ToolStripMenuItem miVerticalResults;
        private ToolStripMenuItem navigateToToolStripMenuItem;
        private ToolStripMenuItem newQuerySamePropsItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem outliningToolStripMenuItem;
        private TabPage pagMyQueries;
        private TabPage pagSamples;
        private Panel panAppMessage;
        private Panel panLeft;
        private TableLayoutPanel panMyQueryOptions;
        private Panel panSpacer1;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripMenuItem pLINQToolStripMenuItem;
        private ToolStripMenuItem queryToolStripMenuItem;
        private ToolStripMenuItem reactiveFrameworkToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private SampleQueries sampleQueries;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private SchemaTree schemaTree;
        private ToolStripMenuItem selectAllToolStripMenuItem;
        private ToolStripSeparator sepOutlining;
        private ToolStripMenuItem shortcutsMenuItem;
        private ToolStripMenuItem showHideResultsToolStripMenuItem;
        private SplitContainer splitLeft;
        private ToolStripMenuItem streamInsightToolStripMenuItem;
        private QueryTabControl tcQueries;
        private TabControl tcQueryTrees;
        private ToolStripMenuItem toggleAllOutliningToolStripMenuItem;
        private ToolStripMenuItem toggleOutliningExpansionToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem10;
        private ToolStripSeparator toolStripMenuItem11;
        private ToolStripSeparator toolStripMenuItem12;
        private ToolStripSeparator toolStripMenuItem13;
        private ToolStripSeparator toolStripMenuItem14;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripSeparator toolStripMenuItem6;
        private ToolStripSeparator toolStripMenuItem7;
        private ToolStripSeparator toolStripMenuItem8;
        private ToolStripSeparator toolStripMenuItem9;
        private ToolStripMenuItem toolStripMenuItemAdvanced;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private MyQueries tvMyQueries;
        private ToolStripMenuItem undoToolStripMenuItem;
        private SplitContainer verticalSplit;
        private ToolStripMenuItem viewSamplesToolStripMenuItem;
        private ToolStripMenuItem whatsNewtoolStripMenuItem;

        public MainForm(string queryToLoad, bool runQuery)
        {
            Func<Screen, bool> predicate = null;
            Action a = null;
            SplitterCancelEventHandler handler = null;
            Action action2 = null;
            Action action3 = null;
            EventHandler handler2 = null;
            EventHandler handler3 = null;
            EventHandler handler4 = null;
            EventHandler handler5 = null;
            EventHandler handler6 = null;
            EventHandler handler7 = null;
            this._untitledCount = 1;
            this._firstOpen = true;
            this._queryPages = new Dictionary<RunnableQuery, TabPage>();
            this._cacheQueryControls = new Queue<QueryControl>();
            this._transparencyEnabled = false;
            int? maxQueryRows = UserOptions.Instance.MaxQueryRows;
            this._lastMaxQueryRows = maxQueryRows.HasValue ? maxQueryRows.GetValueOrDefault() : 0x3e8;
            this._lastFormState = FormWindowState.Normal;
            this.components = null;
            this.InitializeComponent();
            base.Icon = Resources.LINQPad;
            LINQPad.UI.ResultsDockForm form = new LINQPad.UI.ResultsDockForm(this) {
                Owner = this
            };
            this._resultsDockForm = form;
            try
            {
                this.llOrganize.UseCompatibleTextRendering = true;
                this.llSetFolder.UseCompatibleTextRendering = true;
            }
            catch
            {
            }
            if (!this.RestoreWindow())
            {
                base.Location = new Point(20, 20);
            }
            this.mainMenu.RenderMode = ToolStripRenderMode.System;
            this.miUnload.Text = "Cancel All Threads and Reset";
            this.miUnload.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F5;
            this.UpdateHotkeys();
            Rectangle testBounds = base.Bounds;
            testBounds.Inflate(-10, -10);
            if (!Screen.AllScreens.Any<Screen>(s => s.Bounds.Contains(testBounds)))
            {
                base.ClientSize = new Size((base.ClientSize.Width * 3) / 4, (base.ClientSize.Height * 3) / 4);
                if (predicate == null)
                {
                    predicate = s => s.Bounds.Contains(base.Bounds);
                }
                if (!Screen.AllScreens.Any<Screen>(predicate))
                {
                    this._lastWindowState = FormWindowState.Maximized;
                    base.WindowState = FormWindowState.Maximized;
                }
            }
            this.schemaTree.NewQuery += new EventHandler<NewQueryArgs>(this.schemaTree_NewQuery);
            this.schemaTree.CxEdited += new EventHandler(this.schemaTree_CxEdited);
            this.schemaTree.RepositoryDeleted += new EventHandler<RepositoryArgs>(this.schemaTree_RepositoryDeleted);
            this.schemaTree.StaticSchemaRepositoryChanged += new EventHandler<RepositoryArgs>(this.schemaTree_StaticSchemaRepositoryChanged);
            Instance = this;
            if (!(!string.IsNullOrEmpty(queryToLoad) && File.Exists(queryToLoad)))
            {
                this.AddQueryPage();
            }
            if (a == null)
            {
                a = delegate {
                    if (this.CurrentQueryControl != null)
                    {
                        this.CurrentQueryControl.FocusQuery();
                    }
                };
            }
            Program.RunOnWinFormsTimer(a);
            this.DisplayMessageCore("Activate premium features", true, true, true);
            this.RepopulateSchemaTree();
            bool flag = false;
            if (!string.IsNullOrEmpty(queryToLoad))
            {
                flag = this.OpenQuery(queryToLoad, true) != null;
            }
            this.verticalSplit.Layout += new LayoutEventHandler(this.verticalSplit_Layout);
            this.verticalSplit.SplitterMoved += new SplitterEventHandler(this.verticalSplit_SplitterMoved);
            if (handler == null)
            {
                handler = delegate (object sender, SplitterCancelEventArgs e) {
                    if (Control.MouseButtons == MouseButtons.Left)
                    {
                        this.IsSplitting = true;
                    }
                };
            }
            this.verticalSplit.SplitterMoving += handler;
            this.tvMyQueries.MouseDown += new MouseEventHandler(this.tvMyQueries_MouseDown);
            this.sampleQueries.MouseDown += new MouseEventHandler(this.sampleQueries_MouseDown);
            if (Program.PresentationMode)
            {
                this.tvMyQueries.KeyDown += new KeyEventHandler(this.tv_KeyDown);
                this.sampleQueries.KeyDown += new KeyEventHandler(this.tv_KeyDown);
                this.schemaTree.KeyDown += new KeyEventHandler(this.tv_KeyDown);
            }
            this._lastQueryControl = this.CurrentQueryControl;
            LicenseManager.GetLicensee();
            if (action2 == null)
            {
                action2 = () => base.Invoke(new Action(this.RestoreActivationMessage));
            }
            WSAgent.MessageChanged = action2;
            base.KeyPreview = true;
            this.Text = this.Text + " 4";
            if ((flag && runQuery) && (this.CurrentQueryControl != null))
            {
                this.CurrentQueryControl.Run();
            }
            if (flag && (this.CurrentQueryControl != null))
            {
                if (action3 == null)
                {
                    action3 = delegate {
                        if (this.CurrentQueryControl != null)
                        {
                            this.CurrentQueryControl.FixOutliningStartupBug();
                        }
                    };
                }
                Program.RunOnWinFormsTimer(action3);
            }
            if (handler2 == null)
            {
                handler2 = delegate (object sender, EventArgs e) {
                    if (this.CurrentQueryControl != null)
                    {
                        this.CurrentQueryControl.AncestorMoved();
                    }
                };
            }
            EventHandler handler8 = handler2;
            for (Control control = this.tcQueries; control != null; control = control.Parent)
            {
                control.Move += handler8;
            }
            Timer timer = new Timer {
                Interval = 0xbb8
            };
            this._collectTimer = timer;
            if (handler3 == null)
            {
                handler3 = delegate (object sender, EventArgs e) {
                    if (this._pendingGC > 0)
                    {
                        GC.Collect();
                        if (--this._pendingGC == 0)
                        {
                            this._collectTimer.Stop();
                        }
                    }
                };
            }
            this._collectTimer.Tick += handler3;
            Timer timer2 = new Timer {
                Interval = 300
            };
            this._assemblyUnlockTimer = timer2;
            this._assemblyUnlockTimer.Tick += new EventHandler(this._assemblyUnlockTimer_Tick);
            Timer timer3 = new Timer {
                Interval = 0x1388
            };
            this._newQueryCacheTimer = timer3;
            this._newQueryCacheTimer.Tick += new EventHandler(this._newQueryCacheTimer_Tick);
            this._newQueryCacheTimer.Start();
            Timer timer4 = new Timer {
                Interval = 300
            };
            this._fakeActiveFormTimer = timer4;
            if (handler4 == null)
            {
                handler4 = (sender, e) => this.InactivateTitleBarNecessary();
            }
            this._fakeActiveFormTimer.Tick += handler4;
            if (Application.RenderWithVisualStyles)
            {
                this.tvMyQueries.Dock = DockStyle.None;
                this.tvMyQueries.BorderStyle = BorderStyle.None;
                this.pagMyQueries.Layout += new LayoutEventHandler(this.pagMyQueries_Layout);
                this.sampleQueries.BorderStyle = BorderStyle.None;
                this.pagSamples.Padding = new Padding(0, 3, 0, 0);
            }
            this.tvMyQueries.QueryRenamed += new RenamedEventHandler(this.tvMyQueries_QueryRenamed);
            this.tvMyQueries.QueryPotentiallyMoved += new Action<string, string>(this.tvMyQueries_QueryPotentiallyMoved);
            this.miAutoScroll.Checked = UserOptionsLive.Instance.AutoScrollResults;
            if (handler5 == null)
            {
                handler5 = (sender, e) => this.ApplyEditToPlugin = (this.CurrentQueryControl != null) && this.CurrentQueryControl.WasPluginRecentlyFocused;
            }
            this.mainMenu.MenuActivate += handler5;
            if (handler6 == null)
            {
                handler6 = delegate (object sender, EventArgs e) {
                    if (this.ApplyEditToPlugin)
                    {
                        QueryControl qc = this.CurrentQueryControl;
                        Program.RunOnWinFormsTimer(delegate {
                            this.ApplyEditToPlugin = false;
                            if ((this.CurrentQueryControl == qc) && qc.IsPluginSelected())
                            {
                                this.CurrentQueryControl.FocusSelectedPlugin();
                            }
                        }, 50);
                    }
                };
            }
            this.mainMenu.MenuDeactivate += handler6;
            if (handler7 == null)
            {
                handler7 = delegate (object sender, EventArgs e) {
                    if (!base.IsDisposed)
                    {
                        foreach (QueryControl control in this.GetQueryControls())
                        {
                            control.RefreshOptimizeQuery();
                            control.Query.RequiresRecompilation = true;
                        }
                    }
                };
            }
            UserOptionsLive.Instance.OptimizeQueriesChanged += handler7;
        }

        private void _assemblyUnlockTimer_Tick(object sender, EventArgs e)
        {
            if (!_active)
            {
                try
                {
                    string currentProcessName = Native.GetCurrentProcessName();
                    if (currentProcessName != null)
                    {
                        currentProcessName = currentProcessName.ToLowerInvariant();
                    }
                    if ((currentProcessName != null) && ((((currentProcessName == "devenv.exe") || (currentProcessName == "vswinexpress.exe")) || (currentProcessName == "vcsexpress.exe")) || (currentProcessName == "vbexpress.exe")))
                    {
                        this._assemblyUnlockTimer.Stop();
                        foreach (QueryControl control in this.GetQueryControlsWithReleasableAssemblyLocks())
                        {
                            control.Query.Cancel(false, true);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void _newQueryCacheTimer_Tick(object sender, EventArgs e)
        {
            if (this._cacheQueryControls.Count < 3)
            {
                this._cacheQueryControls.Enqueue(new QueryControl(new RunnableQuery(), this.schemaTree));
            }
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.OnIdle();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Form form = new About())
            {
                form.ShowDialog(this);
            }
        }

        internal void ActivateAutocompletion()
        {
            using (RegisterForm form = new RegisterForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string text = "Activation successful.";
                    MessageBox.Show(text, "LINQPad Activation", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.FocusQuery();
            }
        }

        internal void ActivateMyQueries()
        {
            this.tcQueryTrees.SelectedIndex = 0;
        }

        internal void AddNamespaceToQuery(string item)
        {
            if ((this.CurrentQuery != null) && !this.CurrentQuery.AdditionalNamespaces.Contains<string>(item))
            {
                this.CurrentQuery.AdditionalNamespaces = this.CurrentQuery.AdditionalNamespaces.Concat<string>(new string[] { item }).ToArray<string>();
                this.CurrentQueryControl.UpdateAutocompletionService();
            }
        }

        private QueryControl AddQueryPage()
        {
            return this.AddQueryPage(false, false, "", null);
        }

        private QueryControl AddQueryPage(RunnableQuery q, bool focusEllipses, bool afterCurrent)
        {
            QueryControl control;
            if ((this.NextCacheQueryControl == null) || (this.NextCacheQueryControl.Query != q))
            {
                control = new QueryControl(q, this.schemaTree);
            }
            else
            {
                control = this._cacheQueryControls.Dequeue();
            }
            if (this.VerticalResultsLayout)
            {
                control.SetVerticalLayout();
            }
            control.Dock = DockStyle.Fill;
            control.QueryClosed += new EventHandler(this.qc_QueryClosed);
            q.QueryChanged += new EventHandler<QueryChangedEventArgs>(this.q_QueryChanged);
            if (Program.PresentationMode)
            {
                control.NextQueryRequest += new EventHandler(this.qc_NextQueryRequest);
                control.PreviousQueryRequest += new EventHandler(this.qc_PreviousQueryRequest);
            }
            control.UpdateEditorZoom();
            if (this.ResultsDockForm.AreResultsTorn)
            {
                this.ResultsDockForm.CheckQuery(control);
            }
            TabPage tabPage = new TabPage(q.Name);
            int left = Control.DefaultFont.Height / 5;
            tabPage.Padding = new Padding(left, 1, left, 0);
            tabPage.UseVisualStyleBackColor = true;
            tabPage.Controls.Add(control);
            this._queryPages[q] = tabPage;
            this.tcQueries.SuspendLayout();
            try
            {
                if (afterCurrent && (this.tcQueries.TabPages.Count > 0))
                {
                    this.tcQueries.TabPages.Insert(this.tcQueries.SelectedIndex + 1, tabPage);
                }
                else
                {
                    this.tcQueries.TabPages.Add(tabPage);
                }
                this.tcQueries.SelectedTab = tabPage;
            }
            finally
            {
                this.tcQueries.ResumeLayout();
            }
            control.FocusQuery(focusEllipses);
            control.FixEditorScrollBars();
            this.UpdateMessagePosition();
            this.schemaTree.UpdateSqlMode((this.CurrentQueryControl == null) ? null : this.CurrentQueryControl.Query);
            return control;
        }

        private QueryControl AddQueryPage(bool noPin, bool copyProps, string initialText)
        {
            return this.AddQueryPage(noPin, copyProps, initialText, null);
        }

        private QueryControl AddQueryPage(bool noPin, bool copyProps, string initialText, QueryLanguage? forcedQueryKind)
        {
            return this.AddQueryPage(noPin, copyProps, initialText, forcedQueryKind, null, false, false, false);
        }

        public QueryControl AddQueryPage(bool noPin, bool copyProps, string initialText, QueryLanguage? forcedQueryKind, string name, bool noTemplateText, bool focusEllipses, bool intoGrids)
        {
            RunnableQuery q = (this.NextCacheQueryControl == null) ? new RunnableQuery() : this.NextCacheQueryControl.Query;
            q.ReadDefaultSettings();
            q.Source = initialText;
            Repository repository = ((this.CurrentQuery == null) || !copyProps) ? this.schemaTree.GetCurrentRepository(true) : this.CurrentQuery.Repository;
            if (repository != null)
            {
                q.Repository = repository;
            }
            if (forcedQueryKind.HasValue)
            {
                q.QueryKind = forcedQueryKind.Value;
            }
            if ((this.CurrentQuery != null) && copyProps)
            {
                q.AdditionalNamespaces = this.CurrentQuery.AdditionalNamespaces;
                q.AdditionalReferences = this.CurrentQuery.AdditionalReferences;
                q.AdditionalGACReferences = this.CurrentQuery.AdditionalGACReferences;
                if (!forcedQueryKind.HasValue)
                {
                    q.QueryKind = this.CurrentQuery.QueryKind;
                }
                q.IncludePredicateBuilder = this.CurrentQuery.IncludePredicateBuilder;
                q.ToDataGrids = this.CurrentQuery.ToDataGrids;
            }
            else
            {
                QueryLanguage? nullable;
                if ((!forcedQueryKind.HasValue && UserOptions.Instance.DefaultQueryLanguage.HasValue) && ((((QueryLanguage) (nullable = UserOptions.Instance.DefaultQueryLanguage).GetValueOrDefault()) != QueryLanguage.Expression) || !nullable.HasValue))
                {
                    q.QueryKind = UserOptions.Instance.DefaultQueryLanguage.Value;
                }
                else if (!(forcedQueryKind.HasValue || (this.CurrentQuery == null)) && this.CurrentQuery.QueryKind.ToString().StartsWith("VB", StringComparison.Ordinal))
                {
                    q.QueryKind = QueryLanguage.VBExpression;
                }
            }
            if (string.IsNullOrEmpty(name))
            {
                q.Name = "Query " + this._untitledCount++;
            }
            else
            {
                q.Name = name;
            }
            if (noPin)
            {
                q.Pinned = false;
            }
            if (intoGrids)
            {
                q.ToDataGrids = true;
            }
            QueryControl control = this.AddQueryPage(q, focusEllipses, copyProps);
            if (!noTemplateText)
            {
                control.CheckToFromProgramLanguage(QueryLanguage.Expression, q.QueryKind, false, q.Repository != null);
            }
            q.IsModified = false;
            return control;
        }

        internal void AddReferenceToQuery(string item, bool addIndirectRefs)
        {
            if (this.CurrentQuery != null)
            {
                item = PathHelper.ResolveReference(item);
                using (this.CurrentQuery.TransactChanges())
                {
                    this.CurrentQuery.AddRefIfNotPresent(false, new string[] { item });
                    if (addIndirectRefs && File.Exists(item))
                    {
                        this.CurrentQuery.AddRefIfNotPresent(false, (from r in AssemblyProber.GetRefs(item).Refs select PathHelper.ResolveReference(r)).ToArray<string>());
                    }
                    this.CurrentQuery.SortReferences();
                }
                this.CurrentQueryControl.UpdateAutocompletionService();
            }
        }

        internal QueryControl AddSqlQueryPage(string sql)
        {
            return this.AddQueryPage(false, true, sql, 8);
        }

        internal void AdvancedQueryProps()
        {
            if (this.CurrentQuery != null)
            {
                using (Form form = new QueryProps(this.CurrentQuery))
                {
                    form.ShowDialog(this);
                }
            }
        }

        private void btnCloseAppMsg_Click(object sender, EventArgs e)
        {
            this.panAppMessage.Hide();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            this.Restart();
        }

        private void bugReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Feedback().Show();
        }

        private void c30InANutshellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://www.albahari.com/lpnutshell");
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.Cancel(false);
            }
        }

        internal void CheckForMaxQueryCountChange()
        {
            int? maxQueryRows = UserOptions.Instance.MaxQueryRows;
            if (this._lastMaxQueryRows != (maxQueryRows.HasValue ? maxQueryRows.GetValueOrDefault() : 0x3e8))
            {
                maxQueryRows = UserOptions.Instance.MaxQueryRows;
                this._lastMaxQueryRows = maxQueryRows.HasValue ? maxQueryRows.GetValueOrDefault() : 0x3e8;
                foreach (QueryControl control in this.GetQueryControls())
                {
                    control.Query.PolluteCachedDomain(true);
                }
            }
        }

        internal void CheckForQueryFolderChange()
        {
            this.tvMyQueries.CheckForQueryFolderChange();
        }

        internal void ChooseOpenQuery()
        {
            QueryControlToStringWrapper[] items = (from qc in this.GetQueryControls().OrderBy<QueryControl, string>(qc => qc.Query.Name, StringComparer.CurrentCultureIgnoreCase) select new QueryControlToStringWrapper(qc)).ToArray<QueryControlToStringWrapper>();
            if (items.Length != 0)
            {
                using (ListPicker picker = new ListPicker(items))
                {
                    picker.Text = "Activate Query";
                    if (picker.ShowDialog() == DialogResult.OK)
                    {
                        this.CurrentQueryControl = ((QueryControlToStringWrapper) picker.SelectedItem).QueryControl;
                    }
                }
            }
        }

        internal void ClearAllConnections(Repository repository)
        {
            foreach (QueryControl control in this.GetQueryControls())
            {
                if (control.Query.Repository == repository)
                {
                    control.Cancel(true);
                }
            }
            if (repository.DriverLoader.IsValid)
            {
                repository.DriverLoader.Driver.ClearConnectionPools(repository);
                if (repository.DriverLoader.InternalID != null)
                {
                    try
                    {
                        repository.DriverLoader.ClearDriverDomain();
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal void CloneQuery()
        {
            string initialText = "";
            int? nullable = null;
            if ((this.CurrentQueryControl != null) && (this.CurrentQueryControl.SelectedQueryText.Length > 1))
            {
                initialText = this.CurrentQueryControl.SelectedQueryText;
            }
            else if (this.CurrentQuery != null)
            {
                initialText = this.CurrentQuery.Source;
                nullable = new int?(this.CurrentQueryControl.CaretOffset);
            }
            QueryControl control = this.AddQueryPage(false, true, initialText);
            if (nullable.HasValue)
            {
                try
                {
                    control.CaretOffset = nullable.Value;
                }
                catch
                {
                }
            }
        }

        private void cloneQueryMenuItem_Click(object sender, EventArgs e)
        {
            this.CloneQuery();
        }

        internal bool CloseAll(QueryControl butThis, bool promptOnly)
        {
            Func<QueryControl, bool> predicate = null;
            IEnumerable<QueryControl> source = from qc in this.GetQueryControls()
                where (qc.Query.IsModified && (qc.Query.Source.Trim().Length > 0)) && (qc != butThis)
                select qc;
            if (source.Any<QueryControl>())
            {
                using (SaveChanges changes = new SaveChanges(from qc in source select qc.Query.Name))
                {
                    switch (changes.ShowDialog(this))
                    {
                        case DialogResult.Cancel:
                            return false;

                        case DialogResult.Yes:
                            foreach (QueryControl control in source)
                            {
                                this.tcQueries.SelectedTab = (TabPage) control.Parent;
                                if (!control.Save())
                                {
                                    return false;
                                }
                                this.UpdateQueryUI(control.Query);
                            }
                            break;
                    }
                }
            }
            if (!promptOnly)
            {
                if (predicate == null)
                {
                    predicate = qc => qc != butThis;
                }
                foreach (QueryControl control in this.GetQueryControls().Where<QueryControl>(predicate))
                {
                    control.Close();
                }
                if (butThis == null)
                {
                    this._untitledCount = 1;
                    this.AddQueryPage();
                }
            }
            return true;
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.CloseAll(null, false);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.TryClose();
            }
        }

        private void DeactivateAutocompletion()
        {
            string str = "This removes your autocompletion license from this computer";
            if (LicenseManager.LicenseKind > 1)
            {
                str = str + ".";
            }
            else
            {
                str = str + " so you can transfer it to another machine. A single-user\r\nlicense allows up to 6 transfers over a 12-month period.";
            }
            if (MessageBox.Show(str + "\r\n\r\nDo you wish to proceed?", "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                WSAgent.Remove(false);
                this.RestoreActivationMessage();
            }
        }

        private void DisplayHtmlFile(string name)
        {
            byte[] buffer;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LINQPad." + name))
            {
                buffer = new BinaryReader(stream).ReadBytes((int) stream.Length);
            }
            this.DisplayHtmlFile(buffer, name);
        }

        private void DisplayHtmlFile(byte[] data, string name)
        {
            string path = Path.Combine(Path.GetTempPath(), "LINQPad");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string str2 = Path.Combine(path, name);
            File.WriteAllBytes(str2, data);
            Thread.Sleep(100);
            WebHelper.LaunchBrowser(str2);
        }

        private void DisplayMessage(string msg, bool isLink, bool bold)
        {
            if (msg.StartsWith("Licensed to", StringComparison.Ordinal))
            {
                this.ShowLicensee = true;
                About.LicenseeMsg = msg;
            }
            if (base.InvokeRequired)
            {
                base.BeginInvoke(delegate {
                    try
                    {
                        this.DisplayMessageCore(msg, isLink, bold, true);
                    }
                    catch
                    {
                    }
                });
            }
            else
            {
                this.DisplayMessageCore(msg, isLink, bold, true);
            }
        }

        private void DisplayMessageCore(string msg, bool isLink, bool bold, bool notIfUpdateAvailable)
        {
            if (!base.IsDisposed && (!notIfUpdateAvailable || !this._updateAvailable))
            {
                if (this.lblMessage != null)
                {
                    this.lblMessage.Dispose();
                    this.lblMessage = null;
                }
                if (isLink)
                {
                    LinkLabel label = new LinkLabel();
                    label.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lblMessage_LinkClicked);
                    this.lblMessage = label;
                    try
                    {
                        this.lblMessage.Font = new Font("Verdana", 8f);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    this.lblMessage = new Label();
                    this.lblMessage.ForeColor = bold ? Color.Maroon : Color.Black;
                    try
                    {
                        this.lblMessage.Font = new Font("Verdana", 8f, bold ? FontStyle.Bold : FontStyle.Regular);
                    }
                    catch
                    {
                    }
                }
                this.lblMessage.AutoSize = true;
                this.lblMessage.Top = 3;
                this.lblMessage.Text = msg;
                this.verticalSplit.Panel2.Controls.Add(this.lblMessage);
                this.lblMessage.BringToFront();
                this.UpdateMessagePosition();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            try
            {
                base.Dispose(disposing);
            }
            catch
            {
            }
        }

        internal void EditorBackColorChanged()
        {
            foreach (QueryControl control in this.GetQueryControlsWithCache())
            {
                control.EditorBackColorChanged();
            }
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            this.miPasteEscapedString.Enabled = ((Clipboard.ContainsData(DataFormats.Text) && (this.CurrentQueryControl != null)) && this.CurrentQueryControl.IsEditorFocused()) && (this.CurrentQuery.QueryKind <= QueryLanguage.VBProgram);
        }

        private void executeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.Run();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void fAQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://www.linqpad.net/FAQ.aspx");
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            this.newQuerySamePropsItem.Enabled = this.cloneQueryMenuItem.Enabled = this.saveToolStripMenuItem.Enabled = this.CurrentQueryControl != null;
            this.saveAsToolStripMenuItem.Enabled = (this.CurrentQueryControl != null) && !this.CurrentQueryControl.Query.IsMyExtensions;
            try
            {
                this.UpdateMRU();
            }
            catch (Exception exception)
            {
                Log.Write(exception, "MainForm File DropDownOpening");
            }
        }

        private void findAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SearchQueries queries = new SearchQueries(false))
            {
                queries.ShowDialog(this);
            }
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.FindReplace();
            }
        }

        private void findNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.FindNext();
            }
        }

        public RunnableQuery FindOpenQuery(string path)
        {
            return this._queryPages.Keys.FirstOrDefault<RunnableQuery>(q => (q.FilePath.ToLowerInvariant() == path.ToLowerInvariant()));
        }

        public QueryControl FindOpenQueryControl(string path)
        {
            return this.GetQueryControls().FirstOrDefault<QueryControl>(q => string.Equals(q.Query.FilePath, path, StringComparison.InvariantCultureIgnoreCase));
        }

        private RunnableQuery FindOpenSampleQuery(TreeNode source)
        {
            return this._queryPages.Keys.FirstOrDefault<RunnableQuery>(q => (q.Predefined && (q.UISource == source)));
        }

        private void findPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.FindPrevious();
            }
        }

        internal void FocusQueries()
        {
            if (this.tvMyQueries.Focused || this.sampleQueries.Focused)
            {
                this.tcQueryTrees.SelectedIndex = (this.tcQueryTrees.SelectedIndex == 0) ? 1 : 0;
            }
            if (this.tcQueryTrees.SelectedIndex == 0)
            {
                this.tvMyQueries.Focus();
            }
            else
            {
                this.sampleQueries.Focus();
            }
        }

        internal void FocusSchemaExplorer()
        {
            this.schemaTree.Focus();
        }

        internal void ForceQueryRecompilations()
        {
            foreach (QueryControl control in this.GetQueryControls())
            {
                control.Query.RequiresRecompilation = true;
            }
        }

        private void forumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://www.albahari.com/linqpadforum");
        }

        internal QueryControl GetQueryControl(TabPage page)
        {
            return (page.Controls[0] as QueryControl);
        }

        internal IEnumerable<QueryControl> GetQueryControls()
        {
            return (from page in this.tcQueries.TabPages.OfType<TabPage>()
                from qc in page.Controls.OfType<QueryControl>()
                select qc);
        }

        internal IEnumerable<QueryControl> GetQueryControlsWithCache()
        {
            return this.GetQueryControls().Concat<QueryControl>(this._cacheQueryControls);
        }

        private IEnumerable<QueryControl> GetQueryControlsWithReleasableAssemblyLocks()
        {
            return this.GetQueryControls().Where<QueryControl>(delegate (QueryControl q) {
                if ((q.Query.QueryKind != QueryLanguage.SQL) && (CS$<>9__CachedAnonymousMethodDelegate32 == null))
                {
                    CS$<>9__CachedAnonymousMethodDelegate32 = r => ShadowAssemblyManager.IsShadowable(r);
                }
                return (((q.Query.AdditionalReferences.Any<string>(CS$<>9__CachedAnonymousMethodDelegate32) || ((q.Query.Repository != null) && !q.Query.Repository.DynamicSchema)) && !q.HasPluginControls) && !q.Query.IsRunning);
            });
        }

        private void helpToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            this.miActivateAutocompletion.Text = this.ShowLicensee ? "Remove License..." : "Upgrade to LINQPad Pro or Premium...";
            this.miManageActivations.Enabled = this.ShowLicensee;
            this.miMemberHelp.Enabled = this.miActivateReflector.Enabled = this.CurrentQueryControl != null;
        }

        internal void HideExplorer()
        {
            base.SuspendLayout();
            this.verticalSplit.SuspendLayout();
            this.tcQueries.Parent = this;
            this.mainMenu.Parent = this;
            this.verticalSplit.Hide();
            if ((this.lblMessage != null) && (this.lblMessage.Parent != null))
            {
                this.lblMessage.Parent = this;
                this.lblMessage.BringToFront();
                this.UpdateMessagePosition();
            }
            DockHandle handle = new DockHandle {
                Dock = DockStyle.Left
            };
            this._dockHandle = handle;
            base.Controls.Add(this._dockHandle);
            this._dockHandle.SendToBack();
            this.mainMenu.SendToBack();
            this.miHideExplorers.Visible = false;
            this.verticalSplit.ResumeLayout();
            base.ResumeLayout();
        }

        internal void InactivateTitleBarNecessary()
        {
            if (base.IsDisposed || this.IsActive)
            {
                this._fakeActiveFormTimer.Stop();
            }
            else
            {
                try
                {
                    if (!string.Equals(Native.GetCurrentProcessName(), "linqpad.exe", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._fakeActiveFormTimer.Stop();
                        Native.SendMessage(base.Handle, 0x86, IntPtr.Zero, IntPtr.Zero);
                    }
                }
                catch
                {
                    this._fakeActiveFormTimer.Stop();
                }
            }
        }

        internal void InformAboutUpdate()
        {
            if (!base.IsDisposed && (UpdateAgent.GetLaterExe() != null))
            {
                this.lblAppMessage.Text = "A newer version of LINQPad has just been downloaded.";
                this.panAppMessage.Show();
                if (this.lblMessage != null)
                {
                    this.lblMessage.Dispose();
                    this.lblMessage = null;
                }
            }
        }

        internal void InformPromoOK()
        {
        }

        internal void InformUpdateInProgress()
        {
            try
            {
                this._updateAvailable = true;
                this.DisplayMessageCore("Downloading update... ", false, true, false);
            }
            catch
            {
            }
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(MainForm));
            this.verticalSplit = new SplitContainer();
            this.panLeft = new Panel();
            this.splitLeft = new SplitContainer();
            this.schemaTree = new SchemaTree();
            this.tcQueryTrees = new TabControl();
            this.pagMyQueries = new TabPage();
            this.tvMyQueries = new MyQueries();
            this.panMyQueryOptions = new TableLayoutPanel();
            this.llSetFolder = new LinkLabel();
            this.llOrganize = new LinkLabel();
            this.pagSamples = new TabPage();
            this.sampleQueries = new SampleQueries();
            this.label1 = new Label();
            this.mainMenu = new MenuStripEx();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.newToolStripMenuItem = new ToolStripMenuItem();
            this.newQuerySamePropsItem = new ToolStripMenuItem();
            this.cloneQueryMenuItem = new ToolStripMenuItem();
            this.openToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator2 = new ToolStripSeparator();
            this.closeToolStripMenuItem = new ToolStripMenuItem();
            this.closeAllToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator = new ToolStripSeparator();
            this.saveToolStripMenuItem = new ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem13 = new ToolStripSeparator();
            this.miPasswordManager = new ToolStripMenuItem();
            this.exitSeparator = new ToolStripSeparator();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.editToolStripMenuItem = new ToolStripMenuItem();
            this.undoToolStripMenuItem = new ToolStripMenuItem();
            this.redoToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator3 = new ToolStripSeparator();
            this.cutToolStripMenuItem = new ToolStripMenuItem();
            this.copyToolStripMenuItem = new ToolStripMenuItem();
            this.copyPlainToolStripMenuItem = new ToolStripMenuItem();
            this.miCopyMarkdown = new ToolStripMenuItem();
            this.pasteToolStripMenuItem = new ToolStripMenuItem();
            this.miPasteEscapedString = new ToolStripMenuItem();
            this.toolStripSeparator4 = new ToolStripSeparator();
            this.selectAllToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem6 = new ToolStripSeparator();
            this.findAndReplaceToolStripMenuItem = new ToolStripMenuItem();
            this.findNextToolStripMenuItem = new ToolStripMenuItem();
            this.findPreviousToolStripMenuItem = new ToolStripMenuItem();
            this.miIncrementalSearch = new ToolStripMenuItem();
            this.navigateToToolStripMenuItem = new ToolStripMenuItem();
            this.findAllToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem10 = new ToolStripSeparator();
            this.outliningToolStripMenuItem = new ToolStripMenuItem();
            this.toggleOutliningExpansionToolStripMenuItem = new ToolStripMenuItem();
            this.toggleAllOutliningToolStripMenuItem = new ToolStripMenuItem();
            this.sepOutlining = new ToolStripSeparator();
            this.miAutocompletion = new ToolStripMenuItem();
            this.miCompleteWord = new ToolStripMenuItem();
            this.miListMembers = new ToolStripMenuItem();
            this.miListTables = new ToolStripMenuItem();
            this.toolStripMenuItem11 = new ToolStripSeparator();
            this.miCompleteParameters = new ToolStripMenuItem();
            this.toolStripMenuItem9 = new ToolStripSeparator();
            this.miInsertSnippet = new ToolStripMenuItem();
            this.miSurroundWith = new ToolStripMenuItem();
            this.miExecCmd = new ToolStripMenuItem();
            this.toolStripMenuItem2 = new ToolStripSeparator();
            this.optionsToolStripMenuItem = new ToolStripMenuItem();
            this.queryToolStripMenuItem = new ToolStripMenuItem();
            this.executeToolStripMenuItem = new ToolStripMenuItem();
            this.cancelToolStripMenuItem = new ToolStripMenuItem();
            this.miUnload = new ToolStripMenuItem();
            this.miGCSeparator = new ToolStripSeparator();
            this.showHideResultsToolStripMenuItem = new ToolStripMenuItem();
            this.miUndockResults = new ToolStripMenuItem();
            this.miVerticalResults = new ToolStripMenuItem();
            this.toolStripMenuItem14 = new ToolStripSeparator();
            this.miTextResults = new ToolStripMenuItem();
            this.miGridResults = new ToolStripMenuItem();
            this.miSep1 = new ToolStripSeparator();
            this.miAutoScroll = new ToolStripMenuItem();
            this.miExecutionTracking = new ToolStripMenuItem();
            this.miJumpToExecutionPoint = new ToolStripMenuItem();
            this.toolStripMenuItem8 = new ToolStripSeparator();
            this.miGC = new ToolStripMenuItem();
            this.toolStripMenuItem1 = new ToolStripSeparator();
            this.toolStripMenuItemAdvanced = new ToolStripMenuItem();
            this.helpToolStripMenuItem = new ToolStripMenuItem();
            this.miMemberHelp = new ToolStripMenuItem();
            this.miActivateReflector = new ToolStripMenuItem();
            this.toolStripMenuItem12 = new ToolStripSeparator();
            this.whatsNewtoolStripMenuItem = new ToolStripMenuItem();
            this.shortcutsMenuItem = new ToolStripMenuItem();
            this.fAQToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem4 = new ToolStripSeparator();
            this.viewSamplesToolStripMenuItem = new ToolStripMenuItem();
            this.bugReportToolStripMenuItem = new ToolStripMenuItem();
            this.miSuggestion = new ToolStripMenuItem();
            this.miCustomerService = new ToolStripMenuItem();
            this.forumToolStripMenuItem = new ToolStripMenuItem();
            this.microsoftLINQForumsToolStripMenuItem = new ToolStripMenuItem();
            this.lINQGeneralToolStripMenuItem = new ToolStripMenuItem();
            this.lINQToSQLToolStripMenuItem = new ToolStripMenuItem();
            this.lINQToEntitiesToolStripMenuItem = new ToolStripMenuItem();
            this.lINQToXMLToolStripMenuItem = new ToolStripMenuItem();
            this.pLINQToolStripMenuItem = new ToolStripMenuItem();
            this.reactiveFrameworkToolStripMenuItem = new ToolStripMenuItem();
            this.streamInsightToolStripMenuItem = new ToolStripMenuItem();
            this.c30InANutshellToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem7 = new ToolStripSeparator();
            this.miActivateAutocompletion = new ToolStripMenuItem();
            this.miManageActivations = new ToolStripMenuItem();
            this.miCheckForUpdates = new ToolStripMenuItem();
            this.toolStripMenuItem3 = new ToolStripSeparator();
            this.toolStripMenuItem5 = new ToolStripMenuItem();
            this.miHideExplorers = new ToolStripMenuItem();
            this.tcQueries = new QueryTabControl();
            this.panAppMessage = new Panel();
            this.btnRestart = new Button();
            this.panSpacer1 = new Panel();
            this.lblAppMessage = new Label();
            this.btnCloseAppMsg = new ClearButton();
            this.editManager = new EditManager(this.components);
            this.verticalSplit.Panel1.SuspendLayout();
            this.verticalSplit.Panel2.SuspendLayout();
            this.verticalSplit.SuspendLayout();
            this.panLeft.SuspendLayout();
            this.splitLeft.Panel1.SuspendLayout();
            this.splitLeft.Panel2.SuspendLayout();
            this.splitLeft.SuspendLayout();
            this.tcQueryTrees.SuspendLayout();
            this.pagMyQueries.SuspendLayout();
            this.panMyQueryOptions.SuspendLayout();
            this.pagSamples.SuspendLayout();
            this.mainMenu.SuspendLayout();
            this.panAppMessage.SuspendLayout();
            base.SuspendLayout();
            this.verticalSplit.Dock = DockStyle.Fill;
            this.verticalSplit.Location = new Point(0, 0);
            this.verticalSplit.Margin = new Padding(4);
            this.verticalSplit.Name = "verticalSplit";
            this.verticalSplit.Panel1.Controls.Add(this.panLeft);
            this.verticalSplit.Panel1.Controls.Add(this.mainMenu);
            this.verticalSplit.Panel2.Controls.Add(this.tcQueries);
            this.verticalSplit.Panel2.Controls.Add(this.panAppMessage);
            this.verticalSplit.Panel2.Padding = new Padding(0, 2, 1, 1);
            this.verticalSplit.Size = new Size(0x413, 0x397);
            this.verticalSplit.SplitterDistance = 0xf7;
            this.verticalSplit.SplitterWidth = 5;
            this.verticalSplit.TabIndex = 0;
            this.verticalSplit.SplitterMoved += new SplitterEventHandler(this.verticalSplit_SplitterMoved_1);
            this.panLeft.Controls.Add(this.splitLeft);
            this.panLeft.Dock = DockStyle.Fill;
            this.panLeft.Location = new Point(0, 0x1b);
            this.panLeft.Name = "panLeft";
            this.panLeft.Padding = new Padding(4, 4, 0, 1);
            this.panLeft.Size = new Size(0xf7, 0x37c);
            this.panLeft.TabIndex = 2;
            this.splitLeft.Dock = DockStyle.Fill;
            this.splitLeft.Location = new Point(4, 4);
            this.splitLeft.Name = "splitLeft";
            this.splitLeft.Orientation = Orientation.Horizontal;
            this.splitLeft.Panel1.Controls.Add(this.schemaTree);
            this.splitLeft.Panel1.Padding = new Padding(0, 0, 1, 0);
            this.splitLeft.Panel2.Controls.Add(this.tcQueryTrees);
            this.splitLeft.Size = new Size(0xf3, 0x377);
            this.splitLeft.SplitterDistance = 0x207;
            this.splitLeft.SplitterWidth = 7;
            this.splitLeft.TabIndex = 3;
            this.splitLeft.SplitterMoved += new SplitterEventHandler(this.splitLeft_SplitterMoved);
            this.schemaTree.Dock = DockStyle.Fill;
            this.schemaTree.FullRowSelect = true;
            this.schemaTree.ImageIndex = 0;
            this.schemaTree.Location = new Point(0, 0);
            this.schemaTree.Margin = new Padding(4);
            this.schemaTree.Name = "schemaTree";
            this.schemaTree.SelectedImageIndex = 0;
            this.schemaTree.ShowNodeToolTips = true;
            this.schemaTree.Size = new Size(0xf2, 0x207);
            this.schemaTree.TabIndex = 0;
            this.tcQueryTrees.Controls.Add(this.pagMyQueries);
            this.tcQueryTrees.Controls.Add(this.pagSamples);
            this.tcQueryTrees.Dock = DockStyle.Fill;
            this.tcQueryTrees.Location = new Point(0, 0);
            this.tcQueryTrees.Name = "tcQueryTrees";
            this.tcQueryTrees.SelectedIndex = 0;
            this.tcQueryTrees.Size = new Size(0xf3, 0x169);
            this.tcQueryTrees.TabIndex = 0;
            this.tcQueryTrees.SizeChanged += new EventHandler(this.tcQueryTrees_SizeChanged);
            this.pagMyQueries.Controls.Add(this.tvMyQueries);
            this.pagMyQueries.Controls.Add(this.panMyQueryOptions);
            this.pagMyQueries.Location = new Point(4, 0x1a);
            this.pagMyQueries.Name = "pagMyQueries";
            this.pagMyQueries.Padding = new Padding(3, 2, 4, 4);
            this.pagMyQueries.Size = new Size(0xeb, 0x14b);
            this.pagMyQueries.TabIndex = 0;
            this.pagMyQueries.Text = "My Queries";
            this.pagMyQueries.UseVisualStyleBackColor = true;
            this.tvMyQueries.Dock = DockStyle.Fill;
            this.tvMyQueries.HideSelection = false;
            this.tvMyQueries.ImageIndex = 0;
            this.tvMyQueries.Location = new Point(3, 0x15);
            this.tvMyQueries.Name = "tvMyQueries";
            this.tvMyQueries.SelectedImageIndex = 0;
            this.tvMyQueries.Size = new Size(0xe4, 0x132);
            this.tvMyQueries.TabIndex = 1;
            this.tvMyQueries.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(this.tvMyQueries_NodeMouseDoubleClick);
            this.tvMyQueries.AfterSelect += new TreeViewEventHandler(this.tvMyQueries_AfterSelect);
            this.tvMyQueries.KeyPress += new KeyPressEventHandler(this.tvMyQueries_KeyPress);
            this.tvMyQueries.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.tvMyQueries_NodeMouseClick);
            this.panMyQueryOptions.AutoSize = true;
            this.panMyQueryOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panMyQueryOptions.ColumnCount = 2;
            this.panMyQueryOptions.ColumnStyles.Add(new ColumnStyle());
            this.panMyQueryOptions.ColumnStyles.Add(new ColumnStyle());
            this.panMyQueryOptions.Controls.Add(this.llSetFolder, 0, 0);
            this.panMyQueryOptions.Controls.Add(this.llOrganize, 0, 0);
            this.panMyQueryOptions.Dock = DockStyle.Top;
            this.panMyQueryOptions.Location = new Point(3, 2);
            this.panMyQueryOptions.Name = "panMyQueryOptions";
            this.panMyQueryOptions.RowCount = 1;
            this.panMyQueryOptions.RowStyles.Add(new RowStyle());
            this.panMyQueryOptions.Size = new Size(0xe4, 0x13);
            this.panMyQueryOptions.TabIndex = 2;
            this.llSetFolder.AutoSize = true;
            this.llSetFolder.Dock = DockStyle.Right;
            this.llSetFolder.Location = new Point(0x95, 0);
            this.llSetFolder.Margin = new Padding(3, 0, 0, 0);
            this.llSetFolder.Name = "llSetFolder";
            this.llSetFolder.Size = new Size(0x4f, 0x13);
            this.llSetFolder.TabIndex = 1;
            this.llSetFolder.TabStop = true;
            this.llSetFolder.Text = "Set Folder...";
            this.llSetFolder.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llSetFolder_LinkClicked);
            this.llOrganize.AutoSize = true;
            this.llOrganize.Dock = DockStyle.Left;
            this.llOrganize.Location = new Point(0, 0);
            this.llOrganize.Margin = new Padding(0, 0, 9, 0);
            this.llOrganize.Name = "llOrganize";
            this.llOrganize.Size = new Size(0x49, 0x13);
            this.llOrganize.TabIndex = 0;
            this.llOrganize.TabStop = true;
            this.llOrganize.Text = "Organize...";
            this.llOrganize.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            this.pagSamples.Controls.Add(this.sampleQueries);
            this.pagSamples.Controls.Add(this.label1);
            this.pagSamples.Location = new Point(4, 0x1a);
            this.pagSamples.Name = "pagSamples";
            this.pagSamples.Padding = new Padding(3, 5, 4, 4);
            this.pagSamples.Size = new Size(0xeb, 0x14b);
            this.pagSamples.TabIndex = 1;
            this.pagSamples.Text = "Samples";
            this.pagSamples.UseVisualStyleBackColor = true;
            this.sampleQueries.Dock = DockStyle.Fill;
            this.sampleQueries.HideSelection = false;
            this.sampleQueries.ImageIndex = 0;
            this.sampleQueries.Location = new Point(3, 5);
            this.sampleQueries.Name = "sampleQueries";
            this.sampleQueries.SelectedImageIndex = 0;
            this.sampleQueries.Size = new Size(0xe4, 0x142);
            this.sampleQueries.TabIndex = 1;
            this.sampleQueries.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(this.sampleQueries_NodeMouseDoubleClick);
            this.sampleQueries.AfterSelect += new TreeViewEventHandler(this.sampleQueries_AfterSelect);
            this.sampleQueries.KeyPress += new KeyPressEventHandler(this.sampleQueries_KeyPress);
            this.sampleQueries.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.sampleQueries_NodeMouseClick);
            this.label1.AutoSize = true;
            this.label1.Location = new Point(1, 10);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x70, 0x13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Coming shortly...";
            this.mainMenu.BackColor = Color.Transparent;
            this.mainMenu.Items.AddRange(new ToolStripItem[] { this.fileToolStripMenuItem, this.editToolStripMenuItem, this.queryToolStripMenuItem, this.helpToolStripMenuItem, this.miHideExplorers });
            this.mainMenu.Location = new Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Padding = new Padding(5, 2, 0, 2);
            this.mainMenu.Size = new Size(0xf7, 0x1b);
            this.mainMenu.TabIndex = 1;
            this.mainMenu.Text = "menuStrip1";
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.newToolStripMenuItem, this.newQuerySamePropsItem, this.cloneQueryMenuItem, this.openToolStripMenuItem, this.toolStripSeparator2, this.closeToolStripMenuItem, this.closeAllToolStripMenuItem, this.toolStripSeparator, this.saveToolStripMenuItem, this.saveAsToolStripMenuItem, this.toolStripMenuItem13, this.miPasswordManager, this.exitSeparator, this.exitToolStripMenuItem });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(0x29, 0x17);
            this.fileToolStripMenuItem.Text = "&File";
            this.fileToolStripMenuItem.DropDownOpening += new EventHandler(this.fileToolStripMenuItem_DropDownOpening);
            this.newToolStripMenuItem.Image = (Image) manager.GetObject("newToolStripMenuItem.Image");
            this.newToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            this.newToolStripMenuItem.Size = new Size(340, 0x18);
            this.newToolStripMenuItem.Text = "&New Query";
            this.newToolStripMenuItem.Click += new EventHandler(this.newToolStripMenuItem_Click);
            this.newQuerySamePropsItem.Image = Resources.NewQuerySameProps;
            this.newQuerySamePropsItem.Name = "newQuerySamePropsItem";
            this.newQuerySamePropsItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
            this.newQuerySamePropsItem.Size = new Size(340, 0x18);
            this.newQuerySamePropsItem.Text = "New Query, same properties";
            this.newQuerySamePropsItem.Click += new EventHandler(this.newQuerySamePropsItem_Click);
            this.cloneQueryMenuItem.Image = Resources.CloneQuery;
            this.cloneQueryMenuItem.Name = "cloneQueryMenuItem";
            this.cloneQueryMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
            this.cloneQueryMenuItem.Size = new Size(340, 0x18);
            this.cloneQueryMenuItem.Text = "Clone Query";
            this.cloneQueryMenuItem.Click += new EventHandler(this.cloneQueryMenuItem_Click);
            this.openToolStripMenuItem.Image = (Image) manager.GetObject("openToolStripMenuItem.Image");
            this.openToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            this.openToolStripMenuItem.Size = new Size(340, 0x18);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new EventHandler(this.openToolStripMenuItem_Click);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new Size(0x151, 6);
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F4;
            this.closeToolStripMenuItem.Size = new Size(340, 0x18);
            this.closeToolStripMenuItem.Text = "&Close";
            this.closeToolStripMenuItem.Click += new EventHandler(this.closeToolStripMenuItem_Click);
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F4;
            this.closeAllToolStripMenuItem.Size = new Size(340, 0x18);
            this.closeAllToolStripMenuItem.Text = "C&lose All Queries";
            this.closeAllToolStripMenuItem.Click += new EventHandler(this.closeAllToolStripMenuItem_Click);
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new Size(0x151, 6);
            this.saveToolStripMenuItem.Image = (Image) manager.GetObject("saveToolStripMenuItem.Image");
            this.saveToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            this.saveToolStripMenuItem.Size = new Size(340, 0x18);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new EventHandler(this.saveToolStripMenuItem_Click);
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new Size(340, 0x18);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            this.saveAsToolStripMenuItem.Click += new EventHandler(this.saveAsToolStripMenuItem_Click);
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new Size(0x151, 6);
            this.miPasswordManager.Name = "miPasswordManager";
            this.miPasswordManager.Size = new Size(340, 0x18);
            this.miPasswordManager.Text = "Password Manager";
            this.miPasswordManager.Click += new EventHandler(this.miPasswordManager_Click);
            this.exitSeparator.Name = "exitSeparator";
            this.exitSeparator.Size = new Size(0x151, 6);
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            this.exitToolStripMenuItem.ShowShortcutKeys = false;
            this.exitToolStripMenuItem.Size = new Size(340, 0x18);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new EventHandler(this.exitToolStripMenuItem_Click);
            this.editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 
                this.undoToolStripMenuItem, this.redoToolStripMenuItem, this.toolStripSeparator3, this.cutToolStripMenuItem, this.copyToolStripMenuItem, this.copyPlainToolStripMenuItem, this.miCopyMarkdown, this.pasteToolStripMenuItem, this.miPasteEscapedString, this.toolStripSeparator4, this.selectAllToolStripMenuItem, this.toolStripMenuItem6, this.findAndReplaceToolStripMenuItem, this.findNextToolStripMenuItem, this.findPreviousToolStripMenuItem, this.miIncrementalSearch, 
                this.navigateToToolStripMenuItem, this.findAllToolStripMenuItem, this.toolStripMenuItem10, this.outliningToolStripMenuItem, this.sepOutlining, this.miAutocompletion, this.miExecCmd, this.toolStripMenuItem2, this.optionsToolStripMenuItem
             });
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new Size(0x2c, 0x17);
            this.editToolStripMenuItem.Text = "&Edit";
            this.editToolStripMenuItem.DropDownOpening += new EventHandler(this.editToolStripMenuItem_DropDownOpening);
            this.undoToolStripMenuItem.Image = Resources.Undo;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
            this.undoToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.undoToolStripMenuItem.Text = "&Undo ";
            this.redoToolStripMenuItem.Image = Resources.Redo;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
            this.redoToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.redoToolStripMenuItem.Text = "&Redo ";
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new Size(0x17d, 6);
            this.cutToolStripMenuItem.Image = (Image) manager.GetObject("cutToolStripMenuItem.Image");
            this.cutToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            this.cutToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.cutToolStripMenuItem.Text = "Cu&t";
            this.copyToolStripMenuItem.Image = (Image) manager.GetObject("copyToolStripMenuItem.Image");
            this.copyToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            this.copyToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.copyPlainToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            this.copyPlainToolStripMenuItem.Name = "copyPlainToolStripMenuItem";
            this.copyPlainToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.Shift | Keys.C;
            this.copyPlainToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.copyPlainToolStripMenuItem.Text = "Copy without formatting";
            this.miCopyMarkdown.Name = "miCopyMarkdown";
            this.miCopyMarkdown.ShortcutKeys = Keys.Control | Keys.Shift | Keys.M;
            this.miCopyMarkdown.Size = new Size(0x180, 0x18);
            this.miCopyMarkdown.Text = "Copy for &Markdown/StackOverflow";
            this.pasteToolStripMenuItem.Image = (Image) manager.GetObject("pasteToolStripMenuItem.Image");
            this.pasteToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            this.pasteToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.pasteToolStripMenuItem.Text = "&Paste";
            this.miPasteEscapedString.Name = "miPasteEscapedString";
            this.miPasteEscapedString.ShortcutKeys = Keys.Alt | Keys.Shift | Keys.V;
            this.miPasteEscapedString.Size = new Size(0x180, 0x18);
            this.miPasteEscapedString.Text = "Paste as &Escaped String";
            this.miPasteEscapedString.Click += new EventHandler(this.miPasteEscapedString_Click);
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new Size(0x17d, 6);
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new Size(0x17d, 6);
            this.findAndReplaceToolStripMenuItem.Image = Resources.FindReplace;
            this.findAndReplaceToolStripMenuItem.Name = "findAndReplaceToolStripMenuItem";
            this.findAndReplaceToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F;
            this.findAndReplaceToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.findAndReplaceToolStripMenuItem.Text = "&Find and Replace...";
            this.findAndReplaceToolStripMenuItem.Click += new EventHandler(this.findAndReplaceToolStripMenuItem_Click);
            this.findNextToolStripMenuItem.Image = Resources.FindNext;
            this.findNextToolStripMenuItem.Name = "findNextToolStripMenuItem";
            this.findNextToolStripMenuItem.ShortcutKeys = Keys.F3;
            this.findNextToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.findNextToolStripMenuItem.Text = "Find Next";
            this.findNextToolStripMenuItem.Click += new EventHandler(this.findNextToolStripMenuItem_Click);
            this.findPreviousToolStripMenuItem.Image = Resources.FindPrevious;
            this.findPreviousToolStripMenuItem.Name = "findPreviousToolStripMenuItem";
            this.findPreviousToolStripMenuItem.ShortcutKeys = Keys.Shift | Keys.F3;
            this.findPreviousToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.findPreviousToolStripMenuItem.Text = "Find Previous";
            this.findPreviousToolStripMenuItem.Click += new EventHandler(this.findPreviousToolStripMenuItem_Click);
            this.miIncrementalSearch.Name = "miIncrementalSearch";
            this.miIncrementalSearch.ShortcutKeys = Keys.Control | Keys.I;
            this.miIncrementalSearch.Size = new Size(0x180, 0x18);
            this.miIncrementalSearch.Text = "Incremental Search";
            this.miIncrementalSearch.Click += new EventHandler(this.miIncrementalSearch_Click);
            this.navigateToToolStripMenuItem.Name = "navigateToToolStripMenuItem";
            this.navigateToToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+,";
            this.navigateToToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Oemcomma;
            this.navigateToToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.navigateToToolStripMenuItem.Text = "Navigate To...";
            this.navigateToToolStripMenuItem.Click += new EventHandler(this.navigateToToolStripMenuItem_Click);
            this.findAllToolStripMenuItem.Name = "findAllToolStripMenuItem";
            this.findAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F;
            this.findAllToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.findAllToolStripMenuItem.Text = "Search All Queries / Samples...";
            this.findAllToolStripMenuItem.Click += new EventHandler(this.findAllToolStripMenuItem_Click);
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new Size(0x17d, 6);
            this.outliningToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.toggleOutliningExpansionToolStripMenuItem, this.toggleAllOutliningToolStripMenuItem });
            this.outliningToolStripMenuItem.Name = "outliningToolStripMenuItem";
            this.outliningToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.outliningToolStripMenuItem.Text = "Outlining";
            this.toggleOutliningExpansionToolStripMenuItem.Name = "toggleOutliningExpansionToolStripMenuItem";
            this.toggleOutliningExpansionToolStripMenuItem.Size = new Size(0xf5, 0x18);
            this.toggleOutliningExpansionToolStripMenuItem.Text = "Toggle Outlining Expansion";
            this.toggleOutliningExpansionToolStripMenuItem.Click += new EventHandler(this.toggleOutliningExpansionToolStripMenuItem_Click);
            this.toggleAllOutliningToolStripMenuItem.Name = "toggleAllOutliningToolStripMenuItem";
            this.toggleAllOutliningToolStripMenuItem.Size = new Size(0xf5, 0x18);
            this.toggleAllOutliningToolStripMenuItem.Text = "Toggle All Outlining";
            this.toggleAllOutliningToolStripMenuItem.Click += new EventHandler(this.toggleAllOutliningToolStripMenuItem_Click);
            this.sepOutlining.Name = "sepOutlining";
            this.sepOutlining.Size = new Size(0x17d, 6);
            this.miAutocompletion.DropDownItems.AddRange(new ToolStripItem[] { this.miCompleteWord, this.miListMembers, this.miListTables, this.toolStripMenuItem11, this.miCompleteParameters, this.toolStripMenuItem9, this.miInsertSnippet, this.miSurroundWith });
            this.miAutocompletion.Name = "miAutocompletion";
            this.miAutocompletion.ShortcutKeyDisplayString = "";
            this.miAutocompletion.Size = new Size(0x180, 0x18);
            this.miAutocompletion.Text = "Autocompletion";
            this.miCompleteWord.Name = "miCompleteWord";
            this.miCompleteWord.ShortcutKeyDisplayString = "Ctrl+Space";
            this.miCompleteWord.Size = new Size(370, 0x18);
            this.miCompleteWord.Text = "Complete Word";
            this.miCompleteWord.Click += new EventHandler(this.miCompleteWord_Click);
            this.miListMembers.Name = "miListMembers";
            this.miListMembers.Size = new Size(370, 0x18);
            this.miListMembers.Text = "List Members";
            this.miListMembers.Click += new EventHandler(this.miListMembers_Click);
            this.miListTables.Name = "miListTables";
            this.miListTables.ShortcutKeyDisplayString = "Ctrl+T";
            this.miListTables.Size = new Size(370, 0x18);
            this.miListTables.Text = "List Just Tables and Enumerable Objects";
            this.miListTables.Click += new EventHandler(this.miListTables_Click);
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new Size(0x16f, 6);
            this.miCompleteParameters.Name = "miCompleteParameters";
            this.miCompleteParameters.ShortcutKeyDisplayString = "Shift+Ctrl+Space";
            this.miCompleteParameters.Size = new Size(370, 0x18);
            this.miCompleteParameters.Text = "Parameter Info";
            this.miCompleteParameters.Click += new EventHandler(this.miCompleteParameters_Click);
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new Size(0x16f, 6);
            this.miInsertSnippet.Name = "miInsertSnippet";
            this.miInsertSnippet.Size = new Size(370, 0x18);
            this.miInsertSnippet.Text = "Insert Snippet...";
            this.miInsertSnippet.Click += new EventHandler(this.miInsertSnippet_Click);
            this.miSurroundWith.Name = "miSurroundWith";
            this.miSurroundWith.Size = new Size(370, 0x18);
            this.miSurroundWith.Text = "Surround With...";
            this.miSurroundWith.Click += new EventHandler(this.miSurroundWith_Click);
            this.miExecCmd.Name = "miExecCmd";
            this.miExecCmd.ShortcutKeyDisplayString = "Ctrl+Shift+X";
            this.miExecCmd.ShortcutKeys = Keys.Control | Keys.Shift | Keys.X;
            this.miExecCmd.Size = new Size(0x180, 0x18);
            this.miExecCmd.Text = "Execute shell command";
            this.miExecCmd.Click += new EventHandler(this.miExecCmd_Click);
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new Size(0x17d, 6);
            this.optionsToolStripMenuItem.Image = Resources.Preferences;
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new Size(0x180, 0x18);
            this.optionsToolStripMenuItem.Text = "Prefere&nces";
            this.optionsToolStripMenuItem.Click += new EventHandler(this.optionsToolStripMenuItem_Click);
            this.queryToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 
                this.executeToolStripMenuItem, this.cancelToolStripMenuItem, this.miUnload, this.miGCSeparator, this.showHideResultsToolStripMenuItem, this.miUndockResults, this.miVerticalResults, this.toolStripMenuItem14, this.miTextResults, this.miGridResults, this.miSep1, this.miAutoScroll, this.miExecutionTracking, this.miJumpToExecutionPoint, this.toolStripMenuItem8, this.miGC, 
                this.toolStripMenuItem1, this.toolStripMenuItemAdvanced
             });
            this.queryToolStripMenuItem.Name = "queryToolStripMenuItem";
            this.queryToolStripMenuItem.Size = new Size(0x3b, 0x17);
            this.queryToolStripMenuItem.Text = "&Query";
            this.queryToolStripMenuItem.DropDownOpened += new EventHandler(this.queryToolStripMenuItem_DropDownOpened);
            this.executeToolStripMenuItem.Image = Resources.Execute;
            this.executeToolStripMenuItem.Name = "executeToolStripMenuItem";
            this.executeToolStripMenuItem.ShortcutKeys = Keys.F5;
            this.executeToolStripMenuItem.Size = new Size(0x164, 0x18);
            this.executeToolStripMenuItem.Text = "E&xecute";
            this.executeToolStripMenuItem.Click += new EventHandler(this.executeToolStripMenuItem_Click);
            this.cancelToolStripMenuItem.Image = Resources.Cancel;
            this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
            this.cancelToolStripMenuItem.ShortcutKeys = Keys.Shift | Keys.F5;
            this.cancelToolStripMenuItem.Size = new Size(0x164, 0x18);
            this.cancelToolStripMenuItem.Text = "&Cancel";
            this.cancelToolStripMenuItem.Click += new EventHandler(this.cancelToolStripMenuItem_Click);
            this.miUnload.Name = "miUnload";
            this.miUnload.Size = new Size(0x164, 0x18);
            this.miUnload.Text = "Unload Query Application Domain";
            this.miUnload.Click += new EventHandler(this.miUnload_Click);
            this.miGCSeparator.Name = "miGCSeparator";
            this.miGCSeparator.Size = new Size(0x161, 6);
            this.showHideResultsToolStripMenuItem.Image = Resources.Results;
            this.showHideResultsToolStripMenuItem.Name = "showHideResultsToolStripMenuItem";
            this.showHideResultsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            this.showHideResultsToolStripMenuItem.Size = new Size(0x164, 0x18);
            this.showHideResultsToolStripMenuItem.Text = "Hide Results";
            this.showHideResultsToolStripMenuItem.Click += new EventHandler(this.showHideResultsToolStripMenuItem_Click);
            this.miUndockResults.Name = "miUndockResults";
            this.miUndockResults.ShortcutKeys = Keys.F8;
            this.miUndockResults.Size = new Size(0x164, 0x18);
            this.miUndockResults.Text = "Dock / Undock Results";
            this.miUndockResults.Click += new EventHandler(this.miUndockResults_Click);
            this.miVerticalResults.Name = "miVerticalResults";
            this.miVerticalResults.ShortcutKeys = Keys.Control | Keys.F8;
            this.miVerticalResults.Size = new Size(0x164, 0x18);
            this.miVerticalResults.Text = "Arrange Results Panel Vertically";
            this.miVerticalResults.Click += new EventHandler(this.miVerticalResults_Click);
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new Size(0x161, 6);
            this.miTextResults.Name = "miTextResults";
            this.miTextResults.ShortcutKeys = Keys.Control | Keys.Shift | Keys.T;
            this.miTextResults.Size = new Size(0x164, 0x18);
            this.miTextResults.Text = "Results to Rich Text";
            this.miTextResults.Click += new EventHandler(this.miTextResults_Click);
            this.miGridResults.Name = "miGridResults";
            this.miGridResults.ShortcutKeys = Keys.Control | Keys.Shift | Keys.G;
            this.miGridResults.Size = new Size(0x164, 0x18);
            this.miGridResults.Text = "Results to Data Grids";
            this.miGridResults.Click += new EventHandler(this.miGridResults_Click);
            this.miSep1.Name = "miSep1";
            this.miSep1.Size = new Size(0x161, 6);
            this.miAutoScroll.CheckOnClick = true;
            this.miAutoScroll.Name = "miAutoScroll";
            this.miAutoScroll.ShortcutKeys = Keys.Control | Keys.Shift | Keys.E;
            this.miAutoScroll.Size = new Size(0x164, 0x18);
            this.miAutoScroll.Text = "Auto Scroll Results to End";
            this.miAutoScroll.Click += new EventHandler(this.miAutoScroll_Click);
            this.miExecutionTracking.Name = "miExecutionTracking";
            this.miExecutionTracking.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;
            this.miExecutionTracking.Size = new Size(0x164, 0x18);
            this.miExecutionTracking.Text = "Auto Execution Tracking";
            this.miExecutionTracking.Click += new EventHandler(this.miExecutionTracking_Click);
            this.miJumpToExecutionPoint.Name = "miJumpToExecutionPoint";
            this.miJumpToExecutionPoint.ShortcutKeys = Keys.Control | Keys.Shift | Keys.J;
            this.miJumpToExecutionPoint.Size = new Size(0x164, 0x18);
            this.miJumpToExecutionPoint.Text = "Jump to Execution Point";
            this.miJumpToExecutionPoint.Click += new EventHandler(this.miJumpToExecutionPoint_Click);
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new Size(0x161, 6);
            this.miGC.Name = "miGC";
            this.miGC.ShortcutKeys = Keys.Alt | Keys.Shift | Keys.G;
            this.miGC.Size = new Size(0x164, 0x18);
            this.miGC.Text = "Trigger Garbage Collection Now";
            this.miGC.Click += new EventHandler(this.miGC_Click);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new Size(0x161, 6);
            this.toolStripMenuItemAdvanced.Image = Resources.AdvancedProperties;
            this.toolStripMenuItemAdvanced.Name = "toolStripMenuItemAdvanced";
            this.toolStripMenuItemAdvanced.ShortcutKeys = Keys.F4;
            this.toolStripMenuItemAdvanced.Size = new Size(0x164, 0x18);
            this.toolStripMenuItemAdvanced.Text = "Query Properties";
            this.toolStripMenuItemAdvanced.Click += new EventHandler(this.toolStripMenuItemAdvanced_Click);
            this.helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 
                this.miMemberHelp, this.miActivateReflector, this.toolStripMenuItem12, this.whatsNewtoolStripMenuItem, this.shortcutsMenuItem, this.fAQToolStripMenuItem, this.toolStripMenuItem4, this.viewSamplesToolStripMenuItem, this.bugReportToolStripMenuItem, this.miSuggestion, this.miCustomerService, this.forumToolStripMenuItem, this.microsoftLINQForumsToolStripMenuItem, this.c30InANutshellToolStripMenuItem, this.toolStripMenuItem7, this.miActivateAutocompletion, 
                this.miManageActivations, this.miCheckForUpdates, this.toolStripMenuItem3, this.toolStripMenuItem5
             });
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new Size(0x31, 0x17);
            this.helpToolStripMenuItem.Text = "&Help";
            this.helpToolStripMenuItem.DropDownOpening += new EventHandler(this.helpToolStripMenuItem_DropDownOpening);
            this.miMemberHelp.Name = "miMemberHelp";
            this.miMemberHelp.ShortcutKeyDisplayString = "F1";
            this.miMemberHelp.Size = new Size(340, 0x18);
            this.miMemberHelp.Text = "Help on Current Type/Member";
            this.miMemberHelp.Click += new EventHandler(this.miMemberHelp_Click);
            this.miActivateReflector.Name = "miActivateReflector";
            this.miActivateReflector.ShortcutKeyDisplayString = "Shift+F1";
            this.miActivateReflector.Size = new Size(340, 0x18);
            this.miActivateReflector.Text = "Reflect on Current Type/Member";
            this.miActivateReflector.Click += new EventHandler(this.miActivateReflector_Click);
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.Size = new Size(0x151, 6);
            this.whatsNewtoolStripMenuItem.Image = Resources.Add;
            this.whatsNewtoolStripMenuItem.Name = "whatsNewtoolStripMenuItem";
            this.whatsNewtoolStripMenuItem.Size = new Size(340, 0x18);
            this.whatsNewtoolStripMenuItem.Text = "What's New";
            this.whatsNewtoolStripMenuItem.Click += new EventHandler(this.whatsNewtoolStripMenuItem_Click);
            this.shortcutsMenuItem.Name = "shortcutsMenuItem";
            this.shortcutsMenuItem.Size = new Size(340, 0x18);
            this.shortcutsMenuItem.Text = "Keyboard/Mouse Shortcuts";
            this.shortcutsMenuItem.Click += new EventHandler(this.shortcutsMenuItem_Click);
            this.fAQToolStripMenuItem.Name = "fAQToolStripMenuItem";
            this.fAQToolStripMenuItem.Size = new Size(340, 0x18);
            this.fAQToolStripMenuItem.Text = "FAQ";
            this.fAQToolStripMenuItem.Click += new EventHandler(this.fAQToolStripMenuItem_Click);
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new Size(0x151, 6);
            this.viewSamplesToolStripMenuItem.Name = "viewSamplesToolStripMenuItem";
            this.viewSamplesToolStripMenuItem.Size = new Size(340, 0x18);
            this.viewSamplesToolStripMenuItem.Text = "View Samples";
            this.viewSamplesToolStripMenuItem.Click += new EventHandler(this.viewSamplesToolStripMenuItem_Click);
            this.bugReportToolStripMenuItem.Image = Resources.Feedback;
            this.bugReportToolStripMenuItem.Name = "bugReportToolStripMenuItem";
            this.bugReportToolStripMenuItem.Size = new Size(340, 0x18);
            this.bugReportToolStripMenuItem.Text = "Report Bug";
            this.bugReportToolStripMenuItem.Click += new EventHandler(this.bugReportToolStripMenuItem_Click);
            this.miSuggestion.Name = "miSuggestion";
            this.miSuggestion.Size = new Size(340, 0x18);
            this.miSuggestion.Text = "Make Suggestion";
            this.miSuggestion.Click += new EventHandler(this.miSuggestion_Click);
            this.miCustomerService.Name = "miCustomerService";
            this.miCustomerService.Size = new Size(340, 0x18);
            this.miCustomerService.Text = "Customer Service and Feedback Page";
            this.miCustomerService.Click += new EventHandler(this.miCustomerService_Click);
            this.forumToolStripMenuItem.Name = "forumToolStripMenuItem";
            this.forumToolStripMenuItem.Size = new Size(340, 0x18);
            this.forumToolStripMenuItem.Text = "LINQPad Forum";
            this.forumToolStripMenuItem.Click += new EventHandler(this.forumToolStripMenuItem_Click);
            this.microsoftLINQForumsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.lINQGeneralToolStripMenuItem, this.lINQToSQLToolStripMenuItem, this.lINQToEntitiesToolStripMenuItem, this.lINQToXMLToolStripMenuItem, this.pLINQToolStripMenuItem, this.reactiveFrameworkToolStripMenuItem, this.streamInsightToolStripMenuItem });
            this.microsoftLINQForumsToolStripMenuItem.Name = "microsoftLINQForumsToolStripMenuItem";
            this.microsoftLINQForumsToolStripMenuItem.Size = new Size(340, 0x18);
            this.microsoftLINQForumsToolStripMenuItem.Text = "Microsoft Forums";
            this.lINQGeneralToolStripMenuItem.Name = "lINQGeneralToolStripMenuItem";
            this.lINQGeneralToolStripMenuItem.Size = new Size(200, 0x18);
            this.lINQGeneralToolStripMenuItem.Text = "LINQ - General";
            this.lINQGeneralToolStripMenuItem.Click += new EventHandler(this.lINQGeneralToolStripMenuItem_Click);
            this.lINQToSQLToolStripMenuItem.Name = "lINQToSQLToolStripMenuItem";
            this.lINQToSQLToolStripMenuItem.Size = new Size(200, 0x18);
            this.lINQToSQLToolStripMenuItem.Text = "LINQ to SQL";
            this.lINQToSQLToolStripMenuItem.Click += new EventHandler(this.lINQToSQLToolStripMenuItem_Click);
            this.lINQToEntitiesToolStripMenuItem.Name = "lINQToEntitiesToolStripMenuItem";
            this.lINQToEntitiesToolStripMenuItem.Size = new Size(200, 0x18);
            this.lINQToEntitiesToolStripMenuItem.Text = "LINQ to Entities";
            this.lINQToEntitiesToolStripMenuItem.Click += new EventHandler(this.lINQToEntitiesToolStripMenuItem_Click);
            this.lINQToXMLToolStripMenuItem.Name = "lINQToXMLToolStripMenuItem";
            this.lINQToXMLToolStripMenuItem.Size = new Size(200, 0x18);
            this.lINQToXMLToolStripMenuItem.Text = "LINQ to XML";
            this.lINQToXMLToolStripMenuItem.Click += new EventHandler(this.lINQToXMLToolStripMenuItem_Click);
            this.pLINQToolStripMenuItem.Name = "pLINQToolStripMenuItem";
            this.pLINQToolStripMenuItem.Size = new Size(200, 0x18);
            this.pLINQToolStripMenuItem.Text = "PLINQ";
            this.pLINQToolStripMenuItem.Click += new EventHandler(this.pLINQToolStripMenuItem_Click);
            this.reactiveFrameworkToolStripMenuItem.Name = "reactiveFrameworkToolStripMenuItem";
            this.reactiveFrameworkToolStripMenuItem.Size = new Size(200, 0x18);
            this.reactiveFrameworkToolStripMenuItem.Text = "Reactive Framework";
            this.reactiveFrameworkToolStripMenuItem.Click += new EventHandler(this.reactiveFrameworkToolStripMenuItem_Click);
            this.streamInsightToolStripMenuItem.Name = "streamInsightToolStripMenuItem";
            this.streamInsightToolStripMenuItem.Size = new Size(200, 0x18);
            this.streamInsightToolStripMenuItem.Text = "StreamInsight";
            this.streamInsightToolStripMenuItem.Click += new EventHandler(this.streamInsightToolStripMenuItem_Click);
            this.c30InANutshellToolStripMenuItem.Name = "c30InANutshellToolStripMenuItem";
            this.c30InANutshellToolStripMenuItem.Size = new Size(340, 0x18);
            this.c30InANutshellToolStripMenuItem.Text = "C# 4.0 in a Nutshell";
            this.c30InANutshellToolStripMenuItem.Click += new EventHandler(this.c30InANutshellToolStripMenuItem_Click);
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new Size(0x151, 6);
            this.miActivateAutocompletion.Name = "miActivateAutocompletion";
            this.miActivateAutocompletion.Size = new Size(340, 0x18);
            this.miActivateAutocompletion.Text = "Upgrade to LINQPad Pro or Premium...";
            this.miActivateAutocompletion.Click += new EventHandler(this.miActivateAutocompletion_Click);
            this.miManageActivations.Name = "miManageActivations";
            this.miManageActivations.Size = new Size(340, 0x18);
            this.miManageActivations.Text = "Manage Activations...";
            this.miManageActivations.Click += new EventHandler(this.miManageActivations_Click);
            this.miCheckForUpdates.Image = Resources.CheckUpdates;
            this.miCheckForUpdates.Name = "miCheckForUpdates";
            this.miCheckForUpdates.Size = new Size(340, 0x18);
            this.miCheckForUpdates.Text = "Check For Updates";
            this.miCheckForUpdates.Click += new EventHandler(this.miCheckForUpdates_Click);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new Size(0x151, 6);
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new Size(340, 0x18);
            this.toolStripMenuItem5.Text = "&About LINQPad";
            this.toolStripMenuItem5.Click += new EventHandler(this.aboutToolStripMenuItem_Click);
            this.miHideExplorers.Alignment = ToolStripItemAlignment.Right;
            this.miHideExplorers.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.miHideExplorers.Image = Resources.Cross;
            this.miHideExplorers.ImageScaling = ToolStripItemImageScaling.None;
            this.miHideExplorers.Name = "miHideExplorers";
            this.miHideExplorers.Size = new Size(0x16, 0x17);
            this.miHideExplorers.Text = "miHideExplorers";
            this.miHideExplorers.Click += new EventHandler(this.miHideExplorers_Click);
            this.tcQueries.Dock = DockStyle.Fill;
            this.tcQueries.Location = new Point(0, 0x2c);
            this.tcQueries.Name = "tcQueries";
            this.tcQueries.SelectedIndex = 0;
            this.tcQueries.Size = new Size(790, 0x36a);
            this.tcQueries.TabIndex = 0;
            this.tcQueries.SelectedIndexChanged += new EventHandler(this.tcQueries_SelectedIndexChanged);
            this.panAppMessage.Controls.Add(this.btnRestart);
            this.panAppMessage.Controls.Add(this.panSpacer1);
            this.panAppMessage.Controls.Add(this.lblAppMessage);
            this.panAppMessage.Controls.Add(this.btnCloseAppMsg);
            this.panAppMessage.Dock = DockStyle.Top;
            this.panAppMessage.Location = new Point(0, 2);
            this.panAppMessage.Name = "panAppMessage";
            this.panAppMessage.Padding = new Padding(4, 5, 4, 5);
            this.panAppMessage.Size = new Size(790, 0x2a);
            this.panAppMessage.TabIndex = 1;
            this.panAppMessage.Visible = false;
            this.btnRestart.Dock = DockStyle.Right;
            this.btnRestart.Location = new Point(0x277, 5);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new Size(0x7d, 0x20);
            this.btnRestart.TabIndex = 3;
            this.btnRestart.Text = "Restart LINQPad";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new EventHandler(this.btnRestart_Click);
            this.panSpacer1.Dock = DockStyle.Right;
            this.panSpacer1.Location = new Point(0x2f4, 5);
            this.panSpacer1.Name = "panSpacer1";
            this.panSpacer1.Size = new Size(7, 0x20);
            this.panSpacer1.TabIndex = 2;
            this.lblAppMessage.Dock = DockStyle.Fill;
            this.lblAppMessage.Location = new Point(4, 5);
            this.lblAppMessage.Name = "lblAppMessage";
            this.lblAppMessage.Size = new Size(0x2f7, 0x20);
            this.lblAppMessage.TabIndex = 0;
            this.lblAppMessage.Text = "A newer version of LINQPad has just been downloaded.";
            this.lblAppMessage.TextAlign = ContentAlignment.MiddleLeft;
            this.btnCloseAppMsg.Checked = false;
            this.btnCloseAppMsg.Dock = DockStyle.Right;
            this.btnCloseAppMsg.Location = new Point(0x2fb, 5);
            this.btnCloseAppMsg.Name = "btnCloseAppMsg";
            this.btnCloseAppMsg.NoImageScale = false;
            this.btnCloseAppMsg.Size = new Size(0x17, 0x20);
            this.btnCloseAppMsg.TabIndex = 4;
            this.btnCloseAppMsg.Text = "clearButton1";
            this.btnCloseAppMsg.ToolTipText = "";
            this.btnCloseAppMsg.Click += new EventHandler(this.btnCloseAppMsg_Click);
            this.editManager.AllowForwarding = true;
            this.editManager.MenuItemCopy = this.copyToolStripMenuItem;
            this.editManager.MenuItemCopyMarkdown = this.miCopyMarkdown;
            this.editManager.MenuItemCopyPlain = this.copyPlainToolStripMenuItem;
            this.editManager.MenuItemCut = this.cutToolStripMenuItem;
            this.editManager.MenuItemEdit = this.editToolStripMenuItem;
            this.editManager.MenuItemPaste = this.pasteToolStripMenuItem;
            this.editManager.MenuItemRedo = this.redoToolStripMenuItem;
            this.editManager.MenuItemSelectAll = this.selectAllToolStripMenuItem;
            this.editManager.MenuItemUndo = this.undoToolStripMenuItem;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x413, 0x397);
            base.Controls.Add(this.verticalSplit);
            base.Location = new Point(0, 0);
            base.MainMenuStrip = this.mainMenu;
            base.Margin = new Padding(4);
            base.Name = "MainForm";
            base.StartPosition = FormStartPosition.Manual;
            this.Text = "LINQPad";
            this.verticalSplit.Panel1.ResumeLayout(false);
            this.verticalSplit.Panel1.PerformLayout();
            this.verticalSplit.Panel2.ResumeLayout(false);
            this.verticalSplit.ResumeLayout(false);
            this.panLeft.ResumeLayout(false);
            this.splitLeft.Panel1.ResumeLayout(false);
            this.splitLeft.Panel2.ResumeLayout(false);
            this.splitLeft.ResumeLayout(false);
            this.tcQueryTrees.ResumeLayout(false);
            this.pagMyQueries.ResumeLayout(false);
            this.pagMyQueries.PerformLayout();
            this.panMyQueryOptions.ResumeLayout(false);
            this.panMyQueryOptions.PerformLayout();
            this.pagSamples.ResumeLayout(false);
            this.pagSamples.PerformLayout();
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.panAppMessage.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void lblMessage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ActivateAutocompletion();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string defaultQueryFolder = Options.GetDefaultQueryFolder(true);
            if (defaultQueryFolder != null)
            {
                Process.Start(defaultQueryFolder);
            }
        }

        private void lINQGeneralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://social.msdn.microsoft.com/forums/en-US/linqprojectgeneral/threads/");
        }

        private void lINQToDataSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://social.msdn.microsoft.com/forums/en-US/adodotnetdataset/threads/");
        }

        private void lINQToEntitiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://social.msdn.microsoft.com/forums/en-US/adodotnetentityframework/threads/");
        }

        private void lINQToSQLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://social.msdn.microsoft.com/forums/en-US/linqtosql/threads/");
        }

        private void lINQToXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://social.msdn.microsoft.com/forums/en-US/xmlandnetfx/threads/");
        }

        private void llSetFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (OptionsForm form = new OptionsForm(3))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    this.PropagateOptions();
                }
            }
        }

        private void miActivateAutocompletion_Click(object sender, EventArgs e)
        {
            if (this.ShowLicensee)
            {
                this.DeactivateAutocompletion();
            }
            else
            {
                this.ActivateAutocompletion();
            }
        }

        private void miActivateReflector_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.ActivateReflector();
            }
        }

        private void miAutoScroll_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.ScrollResults(VerticalScrollAmount.Document, this.miAutoScroll.Checked, true, true);
            }
        }

        private void miCheckForUpdates_Click(object sender, EventArgs e)
        {
            using (CheckForUpdatesForm form = new CheckForUpdatesForm())
            {
                form.ShowDialog(this);
            }
        }

        private void miCompleteParameters_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.CompleteParam();
            }
        }

        private void miCompleteWord_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.CompleteWord();
            }
        }

        private void miCustomerService_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://www.linqpad.net/feedback.aspx");
        }

        private void miExecCmd_Click(object sender, EventArgs e)
        {
            string str;
            int num;
            QueryControl currentQueryControl = this.CurrentQueryControl;
            if ((currentQueryControl != null) && (currentQueryControl.Query.QueryKind > QueryLanguage.FSharpProgram))
            {
                currentQueryControl = null;
            }
            QueryControl control2 = currentQueryControl ?? this.AddQueryPage();
            if (control2.Query.QueryKind == QueryLanguage.Expression)
            {
                control2.Query.QueryKind = QueryLanguage.Statements;
            }
            else if (control2.Query.QueryKind == QueryLanguage.VBExpression)
            {
                control2.Query.QueryKind = QueryLanguage.VBStatements;
            }
            else if (control2.Query.QueryKind == QueryLanguage.FSharpExpression)
            {
                control2.Query.QueryKind = QueryLanguage.FSharpProgram;
            }
            if (!control2.Query.QueryKind.ToString().StartsWith("VB", StringComparison.Ordinal))
            {
                str = "Util.Cmd (@\"\");";
                num = -3;
            }
            else
            {
                str = "Util.Cmd (\"\")";
                num = -2;
            }
            control2.InsertText(str, num);
        }

        private void miExecutionTracking_Click(object sender, EventArgs e)
        {
            UserOptionsLive.Instance.ExecutionTrackingDisabled = !UserOptionsLive.Instance.ExecutionTrackingDisabled;
        }

        private void miGC_Click(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void miGridResults_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.Query.ToDataGrids = true;
            }
        }

        private void miHideExplorers_Click(object sender, EventArgs e)
        {
            this.ToggleExplorerVisibility();
        }

        private void miIncrementalSearch_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.PerformIncrementalSearch(false);
            }
        }

        private void miInsertSnippet_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.InsertSnippet(false);
            }
        }

        private void miJumpToExecutionPoint_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.JumpToExecutingLine();
            }
        }

        private void miListMembers_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.ListMembers();
            }
        }

        private void miListTables_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.ListTables();
            }
        }

        private void miManageActivations_Click(object sender, EventArgs e)
        {
            if (this.ShowLicensee)
            {
                WebHelper.LaunchBrowser("https://www.linqpad.net/licensing/ListActivations.aspx");
            }
        }

        private void miMemberHelp_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.ActivateHelp();
            }
        }

        private void miPasswordManager_Click(object sender, EventArgs e)
        {
            ManagePasswordsForm.ShowInstance();
        }

        private void miPasteEscapedString_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                string text = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (!string.IsNullOrEmpty(text))
                {
                    this.CurrentQueryControl.PasteIntoEditor(StringUtil.EscapeStringForLanguage(text, this.CurrentQuery.QueryKind));
                }
            }
        }

        private void miSuggestion_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://linqpad.uservoice.com");
        }

        private void miSurroundWith_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.InsertSnippet(true);
            }
        }

        private void miTextResults_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.Query.ToDataGrids = false;
            }
        }

        private void miUndockResults_Click(object sender, EventArgs e)
        {
            this.ToggleDockResults();
        }

        private void miUnload_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.Cancel(true);
            }
        }

        private void miVerticalResults_Click(object sender, EventArgs e)
        {
            this.ToggleVerticalResults();
        }

        private void navigateToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (NavigateToForm form = new NavigateToForm())
            {
                form.ShowDialog(this);
            }
        }

        internal QueryControl NewQuerySameProps(string initialText, bool noTemplateText)
        {
            return this.AddQueryPage(false, true, initialText, null, null, noTemplateText, false, false);
        }

        private void newQuerySamePropsItem_Click(object sender, EventArgs e)
        {
            this.NewQuerySameProps("", false);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.AddQueryPage();
        }

        public void NextQuery()
        {
            int selectedIndex = this.tcQueries.SelectedIndex;
            if ((selectedIndex != -1) && (this.tcQueries.TabPages.Count > 1))
            {
                if (selectedIndex == (this.tcQueries.TabPages.Count - 1))
                {
                    selectedIndex = 0;
                }
                else
                {
                    selectedIndex++;
                }
                this.tcQueries.SelectedIndex = selectedIndex;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            this._assemblyUnlockTimer.Stop();
            _active = true;
            foreach (QueryControl control in this.GetQueryControls())
            {
                if ((control.Query.QueryKind != QueryLanguage.SQL) && (PluginAssembly.GetCompatibleAssemblies(false).Any<string>() || (control.Query.AdditionalReferences.Length > 0)))
                {
                    control.CheckAutocompletionCache();
                }
            }
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.OnFormActivated();
            }
            base.OnActivated(e);
            if (!_activated)
            {
                _activated = true;
                AppStarted.Set();
                Program.RunOnWinFormsTimer(new Action(this.RecoverFiles));
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (QueryControl control in this.GetQueryControls())
            {
                try
                {
                    control.Query.ClearAutoSave();
                }
                catch
                {
                }
            }
            AutoSaver.Shutdown();
            try
            {
                SemanticParserService.Stop();
            }
            catch
            {
            }
            try
            {
                DCDriverLoader.Shutdown();
            }
            catch
            {
            }
            if (this._restart)
            {
                string laterExe = UpdateAgent.GetLaterExe();
                if (File.Exists(laterExe))
                {
                    Process.Start(laterExe, "-noforward -noupdate");
                }
            }
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!this.CloseAll(null, true))
            {
                e.Cancel = true;
                this._restart = false;
            }
            if (!e.Cancel)
            {
                if (Debugger.IsAttached)
                {
                    Thread.Sleep(500);
                }
                LocalUserOptions options = new LocalUserOptions();
                Rectangle rectangle = (base.WindowState == FormWindowState.Maximized) ? base.RestoreBounds : base.Bounds;
                options.WindowLeft = rectangle.Left;
                options.WindowTop = rectangle.Top;
                options.WindowWidth = rectangle.Width;
                options.WindowHeight = rectangle.Height;
                options.IsMaximized = base.WindowState == FormWindowState.Maximized;
                options.MainSplitterVert = ((float) this.verticalSplit.SplitterDistance) / ((float) this.verticalSplit.Width);
                options.MainSplitterHoriz = ((float) this.splitLeft.SplitterDistance) / ((float) this.splitLeft.Height);
                options.ExplorerPanelsHidden = this.IsExplorerHidden;
                options.VerticalResultsLayout = this.VerticalResultsLayout;
                try
                {
                    options.Save();
                }
                catch
                {
                }
            }
            base.OnClosing(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (!(!UserOptions.Instance.LockReferenceAssemblies || Program.PreserveAppDomains) && this.GetQueryControlsWithReleasableAssemblyLocks().Any<QueryControl>())
            {
                this._assemblyUnlockTimer.Start();
            }
            _active = false;
            base.OnDeactivate(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (this.IsExplorerHidden)
            {
                this.UpdateMessagePosition();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Program.Splash.Close();
            Program.Splash = null;
            base.OnLoad(e);
        }

        protected override void OnMove(EventArgs e)
        {
            bool flag = (base.WindowState != FormWindowState.Minimized) && (this._lastFormState == FormWindowState.Minimized);
            this._lastFormState = base.WindowState;
            base.OnMove(e);
            if (flag && (this.CurrentQueryControl != null))
            {
                this.CurrentQueryControl.NotifyWindowRestoreFromMinimize();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if ((this._lastWindowState == FormWindowState.Maximized) && (base.WindowState == FormWindowState.Normal))
            {
                this._lastWindowState = FormWindowState.Normal;
            }
            this._lastWindowState = base.WindowState;
            base.OnResize(e);
        }

        private void OpenMyQuery(TreeNode node)
        {
            MyQueries.FileNode node2 = node as MyQueries.FileNode;
            if (node2 != null)
            {
                string filePath = node2.FilePath;
                if ((filePath != null) && !(Path.GetExtension(filePath).ToLowerInvariant() != ".linq"))
                {
                    this.OpenQuery(filePath, UserOptions.Instance.OpenMyQueriesInNewTab);
                }
            }
        }

        private void OpenQuery()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (this._firstOpen)
                {
                    this._firstOpen = false;
                    string defaultQueryFolder = Options.GetDefaultQueryFolder(true);
                    if (defaultQueryFolder != null)
                    {
                        try
                        {
                            dialog.InitialDirectory = defaultQueryFolder;
                        }
                        catch
                        {
                        }
                    }
                }
                dialog.Title = "Open Query";
                dialog.DefaultExt = "linq";
                dialog.Filter = "LINQ query files (*.linq)|*.linq|SSMS files (*.sql)|*.sql|All files (*.*)|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string str2 in dialog.FileNames)
                    {
                        this.OpenQuery(str2, true);
                    }
                }
            }
        }

        public RunnableQuery OpenQuery(string filePath, bool pin)
        {
            bool flag;
            bool flag2;
            if (flag = filePath.Equals(MyExtensions.QueryFilePath, StringComparison.InvariantCultureIgnoreCase))
            {
                pin = true;
            }
            else if (!File.Exists(filePath))
            {
                return null;
            }
            RunnableQuery q = this.FindOpenQuery(filePath);
            if (q != null)
            {
                this.tcQueries.SelectedTab = this._queryPages[q];
                if (pin)
                {
                    q.Pinned = true;
                }
                return q;
            }
            if (!(((pin || (this.CurrentQuery == null)) || this.CurrentQuery.Pinned) || this.CurrentQuery.IsRunning))
            {
                q = this.CurrentQuery;
            }
            if (flag2 = q == null)
            {
                q = (this.NextCacheQueryControl == null) ? new RunnableQuery() : this.NextCacheQueryControl.Query;
            }
            if (!(!flag || File.Exists(filePath)))
            {
                q.FilePath = filePath;
            }
            else
            {
                try
                {
                    q.Open(filePath);
                    q.LastMoved = DateTime.MinValue;
                }
                catch (Exception exception)
                {
                    if (flag2 && (this.NextCacheQueryControl != null))
                    {
                        this._cacheQueryControls.Dequeue().Dispose();
                    }
                    if (!((exception is IOException) || (exception is UnauthorizedAccessException)))
                    {
                        throw;
                    }
                    MessageBox.Show("Cannot read file: " + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return null;
                }
            }
            if (flag)
            {
                q.IsMyExtensions = true;
                q.Repository = null;
                if (q.QueryKind != QueryLanguage.Program)
                {
                    q.QueryKind = QueryLanguage.Program;
                }
                if (q.Source.Trim().Length == 0)
                {
                    q.Source = q.GetMyExtensionsTemplate();
                    q.IsModified = false;
                    this.UpdateQueryUI(q);
                }
            }
            if (flag2)
            {
                this.AddQueryPage(q, false, false);
            }
            else
            {
                this.tcQueries.SelectedTab = this._queryPages[q];
                ((QueryControl) this.tcQueries.SelectedTab.Controls[0]).FocusQuery();
            }
            q.Pinned = pin;
            if (!(flag2 && !flag))
            {
                this.UpdateQueryUI(q);
            }
            return q;
        }

        internal void OpenSampleQuery(string id)
        {
            TreeNode node = this.sampleQueries.FindByID(id);
            if (node != null)
            {
                this.tcQueryTrees.SelectedIndex = 1;
                this.OpenSampleQuery(node, false);
            }
        }

        private void OpenSampleQuery(TreeNode node, bool pin)
        {
            SampleQueries.QueryNode node2 = node as SampleQueries.QueryNode;
            if (node2 != null)
            {
                this.OpenSampleQuery(node, node2.Text, node2.Content, pin);
            }
        }

        private void OpenSampleQuery(TreeNode source, string name, string content, bool pin)
        {
            RunnableQuery q = this.FindOpenSampleQuery(source);
            if (q != null)
            {
                this.tcQueries.SelectedTab = this._queryPages[q];
                if (pin)
                {
                    q.Pinned = true;
                }
            }
            else
            {
                bool flag;
                if (!(((pin || (this.CurrentQuery == null)) || this.CurrentQuery.Pinned) || this.CurrentQuery.IsRunning))
                {
                    q = this.CurrentQuery;
                }
                if (flag = q == null)
                {
                    q = (this.NextCacheQueryControl == null) ? new RunnableQuery() : this.NextCacheQueryControl.Query;
                }
                q.OpenSample(name, content);
                q.UISource = source;
                if (flag)
                {
                    this.AddQueryPage(q, false, false);
                }
                else
                {
                    this.tcQueries.SelectedTab = this._queryPages[q];
                    ((QueryControl) this.tcQueries.SelectedTab.Controls[0]).FocusQuery();
                }
                q.Pinned = pin;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenQuery();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OptionsForm form = new OptionsForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    this.PropagateOptions();
                }
            }
        }

        private void pagMyQueries_Layout(object sender, LayoutEventArgs e)
        {
            this.tvMyQueries.Bounds = new Rectangle(0, this.panMyQueryOptions.Height + 2, this.pagMyQueries.Width, (this.pagMyQueries.Height - this.panMyQueryOptions.Height) - 2);
        }

        private void pLINQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://social.msdn.microsoft.com/forums/en-US/parallelextensions/threads/");
        }

        public void PreviousQuery()
        {
            int selectedIndex = this.tcQueries.SelectedIndex;
            if ((selectedIndex != -1) && (this.tcQueries.TabPages.Count > 1))
            {
                if (selectedIndex == 0)
                {
                    selectedIndex = this.tcQueries.TabPages.Count - 1;
                }
                else
                {
                    selectedIndex--;
                }
                this.tcQueries.SelectedIndex = selectedIndex;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            this.IsSplitting = false;
            return (HotKeyManager.HandleKey(this, this.CurrentQueryControl, keyData) || base.ProcessCmdKey(ref msg, keyData));
        }

        public bool ProcessCmdKeyForMenu(ref Message m, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.F))
            {
                base.Activate();
                base.Focus();
                this.fileToolStripMenuItem.ShowDropDown();
                return true;
            }
            if (keyData == (Keys.Alt | Keys.E))
            {
                base.Activate();
                base.Focus();
                this.editToolStripMenuItem.ShowDropDown();
                return true;
            }
            if (keyData == (Keys.Alt | Keys.Q))
            {
                base.Activate();
                base.Focus();
                this.queryToolStripMenuItem.ShowDropDown();
                return true;
            }
            if (keyData == (Keys.Alt | Keys.H))
            {
                base.Activate();
                base.Focus();
                this.helpToolStripMenuItem.ShowDropDown();
                return true;
            }
            return this.mainMenu.ProcessCmdKey(ref m, keyData);
        }

        public void PropagateOptions()
        {
            foreach (QueryControl control in this.GetQueryControlsWithCache())
            {
                control.PropagateOptions();
            }
        }

        private void q_QueryChanged(object sender, QueryChangedEventArgs e)
        {
            this.UpdateQueryUI((RunnableQuery) sender);
        }

        private void qc_NextQueryRequest(object sender, EventArgs e)
        {
            TreeView tv = this.sampleQueries.Visible ? ((TreeView) this.sampleQueries) : ((TreeView) this.tvMyQueries);
            TreeViewHelper.MoveToNextLeafNode(tv);
        }

        private void qc_PreviousQueryRequest(object sender, EventArgs e)
        {
            TreeView tv = this.sampleQueries.Visible ? ((TreeView) this.sampleQueries) : ((TreeView) this.tvMyQueries);
            TreeViewHelper.MoveToPreviousLeafNode(tv);
        }

        private void qc_QueryClosed(object sender, EventArgs e)
        {
            this._pendingGC = 3;
            this._collectTimer.Start();
            QueryControl qc = sender as QueryControl;
            if (((qc != null) && (qc.Query != null)) && this._queryPages.ContainsKey(qc.Query))
            {
                this._resultsDockForm.QueryClosed(qc);
                TabPage page = this._queryPages[qc.Query];
                this._queryPages.Remove(qc.Query);
                qc.QueryClosed -= new EventHandler(this.qc_QueryClosed);
                if (qc.Query != null)
                {
                    qc.Query.QueryChanged -= new EventHandler<QueryChangedEventArgs>(this.q_QueryChanged);
                }
                if (Program.PresentationMode)
                {
                    qc.NextQueryRequest -= new EventHandler(this.qc_NextQueryRequest);
                    qc.PreviousQueryRequest -= new EventHandler(this.qc_PreviousQueryRequest);
                }
                int num = this.tcQueries.TabPages.IndexOf(page) - 1;
                if (num >= 0)
                {
                    this.tcQueries.SelectedIndex = num;
                }
                this.tcQueries.TabPages.Remove(page);
                page.Dispose();
            }
        }

        private void queryToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            this.executeToolStripMenuItem.Enabled = this.CurrentQueryControl != null;
            this.cancelToolStripMenuItem.Enabled = (this.CurrentQueryControl != null) && this.CurrentQuery.IsRunning;
            this.closeToolStripMenuItem.Enabled = this.CurrentQueryControl != null;
            this.toolStripMenuItemAdvanced.Enabled = (this.CurrentQueryControl != null) && !this.CurrentQuery.IsRunning;
            this.showHideResultsToolStripMenuItem.Enabled = (this.CurrentQueryControl != null) && !this._resultsDockForm.AreResultsTorn;
            this.miUnload.Enabled = (this.CurrentQueryControl != null) && this.CurrentQuery.HasDomain;
            this.miSep1.Enabled = this.CurrentQueryControl != null;
            this.miUndockResults.Enabled = (this.CurrentQueryControl != null) && (this._resultsDockForm.AreResultsTorn || (Screen.AllScreens.Length > 1));
            this.miUndockResults.Text = this._resultsDockForm.AreResultsTorn ? "Dock Results" : "Undock Results into Second Monitor";
            this.miTextResults.Enabled = this.miGridResults.Enabled = this.CurrentQueryControl != null;
            this.miTextResults.Checked = (this.CurrentQueryControl != null) && !this.CurrentQueryControl.Query.ToDataGrids;
            this.miGridResults.Checked = (this.CurrentQueryControl != null) && this.CurrentQueryControl.Query.ToDataGrids;
            if (this.CurrentQueryControl != null)
            {
                this.showHideResultsToolStripMenuItem.Text = this.CurrentQueryControl.AreResultsVisible() ? "Hide Results" : "Show Results";
            }
            this.miExecutionTracking.Text = UserOptionsLive.Instance.OptimizeQueries ? "Track Execution (optimizations must be disabled)" : "Auto Track Execution";
            this.miExecutionTracking.ShowShortcutKeys = !UserOptionsLive.Instance.OptimizeQueries;
            this.miExecutionTracking.Checked = !UserOptionsLive.Instance.ExecutionTrackingDisabled;
            this.miJumpToExecutionPoint.Enabled = (this.CurrentQuery != null) && this.CurrentQuery.IsRunning;
        }

        private void quickInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.QuickInfo();
            }
        }

        private void reactiveFrameworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://social.msdn.microsoft.com/Forums/pl/rx/threads");
        }

        private void RecoverFiles()
        {
            AutoSaver.RecoveryNode[] recoveryData = AutoSaver.GetRecoveryData();
            if ((recoveryData != null) && (recoveryData.Length != 0))
            {
                if (MessageBox.Show("LINQPad didn't shut down correctly last time it ran.\r\nRecover unsaved queries?", "LINQPad Auto Recover", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (AutoSaver.RecoveryNode node in recoveryData)
                    {
                        RunnableQuery q = node.ToQuery();
                        if (q != null)
                        {
                            if (string.IsNullOrEmpty(q.Name))
                            {
                                q.Name = "Query " + this._untitledCount++;
                            }
                            this.AddQueryPage(q, false, false);
                            this.UpdateQueryUI(q);
                        }
                    }
                }
                foreach (AutoSaver.RecoveryNode node in recoveryData)
                {
                    node.Dispose();
                }
                base.Show();
            }
        }

        internal void RefreshThirdPartySamples(string name)
        {
            this.sampleQueries.RefreshThirdPartySamples(name);
        }

        internal void RegisterUIActivity()
        {
            this._newQueryCacheTimer.Stop();
            this._newQueryCacheTimer.Start();
        }

        public void RepopulateSchemaTree()
        {
            this.schemaTree.PopulateFromDisk();
        }

        public void RequestTransparency()
        {
            if (!this._transparencyEnabled)
            {
                this._transparencyEnabled = true;
                try
                {
                    if (Screen.AllScreens.All<Screen>(s => s.BitsPerPixel >= 0x18))
                    {
                        if (typeof(SystemColors).GetProperties().Any<PropertyInfo>(p => Program.TransparencyKey.Equals(p.GetValue(null, null))))
                        {
                            Program.TransparencyKey = Program.LightTransparencyKey;
                        }
                        if (!typeof(SystemColors).GetProperties().Any<PropertyInfo>(p => Program.TransparencyKey.Equals(p.GetValue(null, null))))
                        {
                            base.TransparencyKey = this._resultsDockForm.TransparencyKey = Program.TransparencyKey;
                        }
                    }
                    else
                    {
                        using (Graphics g = base.CreateGraphics())
                        {
                            Color nearest = g.GetNearestColor(Program.TransparencyKey);
                            if (!typeof(SystemColors).GetProperties().Any<PropertyInfo>(p => nearest.Equals(g.GetNearestColor((Color) p.GetValue(null, null)))))
                            {
                                base.TransparencyKey = this._resultsDockForm.TransparencyKey = Program.TransparencyKey;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public void Restart()
        {
            this._restart = true;
            base.Close();
        }

        private void RestoreActivationMessage()
        {
            this.ShowLicensee = false;
            About.LicenseeMsg = null;
            this.DisplayMessageCore("Activate autocompletion", true, true, true);
        }

        private bool RestoreWindow()
        {
            try
            {
                LocalUserOptions instance = LocalUserOptions.Instance;
                if (instance == null)
                {
                    return false;
                }
                Rectangle rectangle = new Rectangle(instance.WindowLeft, instance.WindowTop, instance.WindowWidth, instance.WindowHeight);
                if ((rectangle.Width < 250) || (rectangle.Height < 200))
                {
                    return false;
                }
                Rectangle testBounds = rectangle;
                testBounds.Inflate(-10, -10);
                if (!Screen.AllScreens.Any<Screen>(s => s.Bounds.Contains(testBounds)))
                {
                    return false;
                }
                base.Bounds = rectangle;
                if (instance.IsMaximized)
                {
                    this._lastWindowState = FormWindowState.Maximized;
                    base.WindowState = FormWindowState.Maximized;
                }
                if (instance.ExplorerPanelsHidden)
                {
                    this.HideExplorer();
                }
                if (instance.VerticalResultsLayout)
                {
                    this.ToggleVerticalResults();
                }
                if ((instance.MainSplitterVert > 0f) && (instance.MainSplitterVert < 0.8f))
                {
                    this.verticalSplit.SplitterDistance = Convert.ToInt32((float) (instance.MainSplitterVert * this.verticalSplit.Width));
                }
                if ((instance.MainSplitterHoriz > 0f) && (instance.MainSplitterHoriz < 0.8f))
                {
                    this.splitLeft.SplitterDistance = Convert.ToInt32((float) (instance.MainSplitterHoriz * this.splitLeft.Height));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void sampleQueries_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!this._treeViewQuerySelectSuspended)
            {
                this._treeViewQuerySelectSuspended = true;
                try
                {
                    this.OpenSampleQuery(e.Node, false);
                    TreeNode node = e.Node;
                    if (e.Action == TreeViewAction.ByKeyboard)
                    {
                        this.sampleQueries.Focus();
                        this.sampleQueries.SelectedNode = node;
                    }
                }
                finally
                {
                    this._treeViewQuerySelectSuspended = false;
                }
            }
        }

        private void sampleQueries_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((e.KeyChar == ' ') || (e.KeyChar == '\r')) && (this.CurrentQueryControl != null))
            {
                this.CurrentQueryControl.FocusQuery();
            }
        }

        private void sampleQueries_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                TreeNode nodeAt = this.sampleQueries.GetNodeAt(e.Location);
                if (nodeAt != null)
                {
                    this.sampleQueries.SelectedNode = nodeAt;
                    this.OpenSampleQuery(nodeAt, true);
                    this.sampleQueries.SelectedNode = nodeAt;
                }
            }
        }

        private void sampleQueries_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
        }

        private void sampleQueries_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.OpenSampleQuery(e.Node, true);
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.FocusQuery();
            }
        }

        internal void Save()
        {
            if ((this.CurrentQueryControl != null) && this.CurrentQueryControl.Save())
            {
                this.UpdateQueryUI(this.CurrentQuery);
                this.UpdateTreeSelections();
            }
        }

        internal void SaveAs()
        {
            if (((this.CurrentQueryControl != null) && !this.CurrentQueryControl.Query.IsMyExtensions) && this.CurrentQueryControl.SaveAs())
            {
                this.UpdateQueryUI(this.CurrentQuery);
                this.UpdateTreeSelections();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveAs();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void schemaTree_CxEdited(object sender, EventArgs e)
        {
            if ((this.CurrentQueryControl != null) && (this.CurrentQueryControl.Query.Repository != null))
            {
                this.schemaTree.RegisterRepository(this.CurrentQueryControl.Query, false, true);
                this.CurrentQuery.ForceCxRefresh();
            }
        }

        private void schemaTree_NewQuery(object sender, NewQueryArgs e)
        {
            QueryControl qc = this.AddQueryPage(false, false, e.QueryText, e.Language, e.QueryName, false, true, e.IntoGrids);
            if (e.RunNow)
            {
                qc.Run();
            }
            else if (!((e.ShowParams || e.ListMembers) ? !this.ShowLicensee : true))
            {
                Program.RunOnWinFormsTimer(delegate {
                    if (!qc.IsDisposed)
                    {
                        try
                        {
                            if (e.ShowParams)
                            {
                                qc.CompleteParam();
                            }
                            else
                            {
                                qc.ListMembers();
                            }
                        }
                        catch
                        {
                        }
                    }
                });
            }
        }

        private void schemaTree_RepositoryDeleted(object sender, RepositoryArgs e)
        {
            if ((this.CurrentQuery != null) && (this.CurrentQuery.Repository == e.Repository))
            {
                this.CurrentQuery.Repository = null;
            }
        }

        private void schemaTree_StaticSchemaRepositoryChanged(object sender, RepositoryArgs e)
        {
            if (base.InvokeRequired)
            {
                base.BeginInvoke(() => this.StaticSchemaRepositoryChanged(e.Repository));
            }
            else
            {
                this.StaticSchemaRepositoryChanged(e.Repository);
            }
        }

        private void shortcutsMenuItem_Click(object sender, EventArgs e)
        {
            this.ShowKeyboardShortcuts();
        }

        internal void ShowExplorer()
        {
            base.SuspendLayout();
            this.verticalSplit.SuspendLayout();
            this.mainMenu.Parent = this.verticalSplit.Panel1;
            this.tcQueries.Parent = this.verticalSplit.Panel2;
            this.verticalSplit.Show();
            if ((this.lblMessage != null) && (this.lblMessage.Parent != null))
            {
                this.lblMessage.Parent = this.verticalSplit.Panel2;
                this.lblMessage.BringToFront();
                this.UpdateMessagePosition();
            }
            this._dockHandle.Dispose();
            this._dockHandle = null;
            this.miHideExplorers.Visible = true;
            this.verticalSplit.ResumeLayout();
            base.ResumeLayout();
        }

        private void showHideResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.ToggleResultsCollapse();
            }
        }

        internal void ShowKeyboardShortcuts()
        {
            string str;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LINQPad.KeyboardShortcuts.html"))
            {
                str = new StreamReader(stream).ReadToEnd();
            }
            if (HotKeyManager.UseStudioKeys)
            {
                str = str.Replace("Ctrl+E", "Ctrl+G").Replace("Ctrl+K", "Ctrl+E, C or Ctrl+K, C").Replace("Ctrl+U", "Ctrl+E, U or Ctrl+K, U").Replace("Ctrl+Alt+S", "Ctrl+W, L or Ctrl+Alt+S");
            }
            this.DisplayHtmlFile(Encoding.UTF8.GetBytes(str), "KeyboardShortcuts.html");
        }

        private void splitLeft_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.FocusQuery();
            }
            else if (this.tcQueries != null)
            {
                this.tcQueries.Focus();
            }
        }

        private void StaticSchemaRepositoryChanged(Repository r)
        {
            foreach (QueryControl control in this.GetQueryControls())
            {
                if (control.Query.Repository == r)
                {
                    control.CheckAutocompletionCache();
                }
            }
        }

        private void streamInsightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebHelper.LaunchBrowser("http://social.msdn.microsoft.com/Forums/en-US/streaminsight");
        }

        private void tcQueries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._lastQueryControl != null)
            {
                this._lastQueryControl.OnNoLongerSelectedPage();
            }
            this._lastQueryControl = this.CurrentQueryControl;
            this._lastQuery = (this._lastQueryControl == null) ? null : this._lastQueryControl.Query;
            this.UpdateTreeSelections();
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.OnNewlySelectedPage();
                if (this.CurrentQueryControl.Query.Repository != null)
                {
                    this.schemaTree.RegisterRepository(this.CurrentQueryControl.Query, false, true);
                }
            }
            this.UpdateMessagePosition();
            this._resultsDockForm.QueryActivated(this.CurrentQueryControl);
            this.schemaTree.UpdateSqlMode((this.CurrentQueryControl == null) ? null : this.CurrentQueryControl.Query);
        }

        private void tcQueryTrees_SizeChanged(object sender, EventArgs e)
        {
            foreach (TabPage page in this.tcQueryTrees.TabPages)
            {
                page.Invalidate();
            }
        }

        private void toggleAllOutliningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.ToggleAllOutlining();
            }
        }

        internal void ToggleAutoScrollResults(bool allowScrollToStart)
        {
            if ((this.CurrentQueryControl != null) && this.CurrentQueryControl.IsAutoScrollingResultsFromQuery())
            {
                this.miAutoScroll.Checked = false;
            }
            else
            {
                this.miAutoScroll.Checked = !this.miAutoScroll.Checked;
            }
            UserOptionsLive.Instance.AutoScrollResults = this.miAutoScroll.Checked;
            if ((this.CurrentQueryControl != null) && (allowScrollToStart || this.miAutoScroll.Checked))
            {
                this.CurrentQueryControl.OverrideAutoScrollFromQuery(this.miAutoScroll.Checked);
                this.CurrentQueryControl.ScrollResults(VerticalScrollAmount.Document, this.miAutoScroll.Checked, true, true);
            }
        }

        internal void ToggleDockResults()
        {
            if (this._resultsDockForm.AreResultsTorn || (Screen.AllScreens.Length > 1))
            {
                this._resultsDockForm.ToggleResultsDock();
            }
        }

        internal void ToggleExplorerVisibility()
        {
            if (this.IsExplorerHidden)
            {
                this.ShowExplorer();
            }
            else
            {
                this.HideExplorer();
            }
        }

        private void toggleOutliningExpansionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.ShowLicensee)
            {
                this.ActivateAutocompletion();
            }
            else if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.ToggleOutliningExpansion();
            }
        }

        internal void ToggleVerticalResults()
        {
            this.VerticalResultsLayout = !this.VerticalResultsLayout;
            this.miVerticalResults.Checked = this.VerticalResultsLayout;
            foreach (QueryControl control in this.GetQueryControlsWithCache())
            {
                if (this.VerticalResultsLayout)
                {
                    control.SetVerticalLayout();
                }
                else
                {
                    control.SetHorizontalLayout();
                }
            }
        }

        private void toolStripMenuItemAdvanced_Click(object sender, EventArgs e)
        {
            this.AdvancedQueryProps();
        }

        internal void TriggerGC(int count)
        {
            this._pendingGC = count;
            this._collectTimer.Start();
        }

        private void tv_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.PageDown) || (e.KeyData == Keys.PageUp))
            {
                e.Handled = true;
                if (this.CurrentQueryControl != null)
                {
                    this.CurrentQueryControl.Focus();
                }
                TreeView tv = (TreeView) sender;
                if (tv != this.schemaTree)
                {
                    if (e.KeyData == Keys.PageDown)
                    {
                        TreeViewHelper.MoveToNextLeafNode(tv);
                    }
                    else
                    {
                        TreeViewHelper.MoveToPreviousLeafNode(tv);
                    }
                }
            }
        }

        private void tvMyQueries_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!UserOptions.Instance.DoubleClickToOpenMyQueries && !this._treeViewQuerySelectSuspended)
            {
                this._treeViewQuerySelectSuspended = true;
                try
                {
                    if ((e.Action != TreeViewAction.ByKeyboard) || !UserOptions.Instance.OpenMyQueriesInNewTab)
                    {
                        this.OpenMyQuery(e.Node);
                        TreeNode node = e.Node;
                        if (e.Action == TreeViewAction.ByKeyboard)
                        {
                            this.tvMyQueries.Focus();
                            this.tvMyQueries.SelectedNode = node;
                        }
                    }
                }
                finally
                {
                    this._treeViewQuerySelectSuspended = false;
                }
            }
        }

        private void tvMyQueries_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == ' ') || (e.KeyChar == '\r'))
            {
                if (UserOptions.Instance.OpenMyQueriesInNewTab || UserOptions.Instance.DoubleClickToOpenMyQueries)
                {
                    this.OpenMyQuery(this.tvMyQueries.SelectedNode);
                }
                if (this.CurrentQueryControl != null)
                {
                    this.CurrentQueryControl.FocusQuery();
                }
            }
        }

        private void tvMyQueries_MouseDown(object sender, MouseEventArgs e)
        {
            EventHandler onClick = null;
            if (e.Button == MouseButtons.Right)
            {
                EventHandler handler2 = null;
                TreeNode node = this.tvMyQueries.GetNodeAt(e.Location);
                ContextMenuStrip m = new ContextMenuStrip();
                TreeNode oldNode = this.tvMyQueries.SelectedNode;
                if (node != null)
                {
                    this._treeViewQuerySelectSuspended = true;
                    this.tvMyQueries.SelectedNode = node;
                    this._treeViewQuerySelectSuspended = false;
                    if (node is MyQueries.FileNode)
                    {
                        if (handler2 == null)
                        {
                            handler2 = (sender, e) => this.tvMyQueries_NodeMouseDoubleClick(sender, new TreeNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.X, e.Y));
                        }
                        m.Items.Add("Open in new tab (middle-click)", Resources.Copy, handler2);
                    }
                }
                if (onClick == null)
                {
                    onClick = (sender, e) => this.tvMyQueries.RefreshTree();
                }
                m.Items.Add("Refresh tree", Resources.Refresh, onClick);
                m.Closing += delegate (object sender, ToolStripDropDownClosingEventArgs e) {
                    if (e.CloseReason != ToolStripDropDownCloseReason.ItemClicked)
                    {
                        this.tvMyQueries.SelectedNode = oldNode;
                    }
                };
                m.ItemClicked += (sender, e) => m.Dispose();
                m.Show(this.tvMyQueries, e.Location);
            }
            else if (e.Button == MouseButtons.Middle)
            {
                TreeNode nodeAt = this.tvMyQueries.GetNodeAt(e.Location);
                if ((nodeAt != null) && (nodeAt is MyQueries.FileNode))
                {
                    this.tvMyQueries_NodeMouseDoubleClick(sender, new TreeNodeMouseClickEventArgs(nodeAt, e.Button, e.Clicks, e.X, e.Y));
                }
            }
        }

        private void tvMyQueries_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
        }

        private void tvMyQueries_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            MyQueries.FileNode node = e.Node as MyQueries.FileNode;
            if (node != null)
            {
                string filePath = node.FilePath;
                if ((Path.GetExtension(filePath) != null) && !(Path.GetExtension(filePath).ToLowerInvariant() != ".linq"))
                {
                    this.OpenQuery(filePath, true);
                    if (this.CurrentQueryControl != null)
                    {
                        this.CurrentQueryControl.FocusQuery();
                    }
                }
            }
        }

        private void tvMyQueries_QueryPotentiallyMoved(string oldPath, string newPath)
        {
            try
            {
                QueryControl control = this.FindOpenQueryControl(oldPath);
                if (((control != null) && !string.Equals(Path.GetDirectoryName(oldPath), Path.GetDirectoryName(newPath), StringComparison.InvariantCultureIgnoreCase)) && (new FileInfo(newPath).Length == control.Query.LengthOnLastSave))
                {
                    control.Query.FilePath = newPath;
                    control.Query.LastMoved = DateTime.UtcNow;
                    this.UpdateQueryUI(control.Query);
                    MRU.QueryNames.RegisterUse(newPath);
                }
            }
            catch
            {
            }
        }

        private void tvMyQueries_QueryRenamed(object sender, RenamedEventArgs e)
        {
            try
            {
                QueryControl control = this.FindOpenQueryControl(e.OldFullPath);
                if (control != null)
                {
                    MRU.QueryNames.Unregister(e.OldFullPath);
                    control.Query.FilePath = e.FullPath;
                    this.UpdateQueryUI(control.Query);
                    MRU.QueryNames.RegisterUse(e.FullPath);
                }
            }
            catch
            {
            }
        }

        internal void UpdateHotkeys()
        {
            if (HotKeyManager.UseStudioKeys)
            {
                this.miListMembers.ShortcutKeyDisplayString = "Ctrl+K, L";
                this.miInsertSnippet.ShortcutKeyDisplayString = "Ctrl+K, X";
                this.miSurroundWith.ShortcutKeyDisplayString = "Ctrl+K, S";
                this.toggleOutliningExpansionToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+M, M";
                this.toggleAllOutliningToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+M, L";
            }
            else
            {
                this.miListMembers.ShortcutKeyDisplayString = "Ctrl+J";
                this.miInsertSnippet.ShortcutKeyDisplayString = "Ctrl+P";
                this.miSurroundWith.ShortcutKeyDisplayString = "Shift+Ctrl+P";
                this.toggleOutliningExpansionToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+M";
                this.toggleAllOutliningToolStripMenuItem.ShortcutKeyDisplayString = "Shift+Ctrl+M";
            }
        }

        private void UpdateMessagePosition()
        {
            if (this.lblMessage != null)
            {
                if (this.IsExplorerHidden)
                {
                    this.lblMessage.Left = base.ClientSize.Width - this.lblMessage.Width;
                    this.lblMessage.Visible = true;
                }
                else
                {
                    this.lblMessage.Left = this.tcQueries.Right - this.lblMessage.Width;
                    if (this.tcQueries.TabCount == 0)
                    {
                        this.lblMessage.Hide();
                    }
                    else
                    {
                        try
                        {
                            bool flag;
                            Rectangle tabRect = this.tcQueries.GetTabRect(this.tcQueries.TabCount - 1);
                            if ((flag = ((this.tcQueries.TabCount > 5) || ((tabRect.Right + this.tcQueries.Left) >= (this.lblMessage.Left - 10))) || ((this.tcQueries.TabCount > 2) && (base.ClientSize.Width < 900))) == this.lblMessage.Visible)
                            {
                                this.lblMessage.Visible = !flag;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void UpdateMRU()
        {
            foreach (MruToolStripMenuItem item in this.fileToolStripMenuItem.DropDownItems.OfType<MruToolStripMenuItem>().ToArray<MruToolStripMenuItem>())
            {
                this.fileToolStripMenuItem.DropDownItems.Remove(item);
            }
            if (this._mruSeparator != null)
            {
                this.fileToolStripMenuItem.DropDownItems.Remove(this._mruSeparator);
                this._mruSeparator = null;
            }
            int num2 = 0;
            foreach (string str in MRU.QueryNames.ReadNames())
            {
                MruToolStripMenuItem item2 = new MruToolStripMenuItem();
                string source = Regex.Replace(Regex.Replace(PathHelper.EncodeFolder(str).Replace("&", " ").Replace("<Personal>", "<Documents>"), Regex.Escape(@"<Documents>\LINQPad Queries\"), "", RegexOptions.IgnoreCase), Regex.Escape(@"<MyDocuments>\LINQPad Queries\"), "", RegexOptions.IgnoreCase);
                if ((source.Count<char>(c => (c == '\\')) > 5) && (source.Length > 60))
                {
                    source = Regex.Replace(source, @"(?<=\\.*\\.*\\).*(?=.*\\.*\\)", "...");
                }
                item2.Text = string.Concat(new object[] { "&", ++num2, " ", source });
                item2.ToolTipText = str;
                string temp = str;
                item2.Click += delegate (object sender, EventArgs e) {
                    if (!File.Exists(temp))
                    {
                        MessageBox.Show("That file no longer exists.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        MRU.QueryNames.Unregister(temp);
                    }
                    this.OpenQuery(temp, true);
                };
                this.fileToolStripMenuItem.DropDownItems.Insert(this.fileToolStripMenuItem.DropDownItems.Count - 3, item2);
            }
            if (num2 > 0)
            {
                this.fileToolStripMenuItem.DropDownItems.Insert(this.fileToolStripMenuItem.DropDownItems.Count - 3, this._mruSeparator = new ToolStripSeparator());
            }
        }

        internal void UpdateQueryUI(RunnableQuery q)
        {
            if ((q != null) && this._queryPages.ContainsKey(q))
            {
                string str = q.Name + (q.IsModified ? "*" : "") + (q.IsReadOnly ? " (read-only)" : ((q.LastMoved > DateTime.UtcNow.AddSeconds(-10.0)) ? " (moved)" : ""));
                if (str != this._queryPages[q].Text)
                {
                    this._queryPages[q].Text = str;
                }
            }
        }

        internal void UpdateQueryZoom()
        {
            foreach (QueryControl control in this.GetQueryControlsWithCache())
            {
                control.UpdateEditorZoom();
            }
        }

        private void UpdateTreeSelections()
        {
            this.sampleQueries.UpdateSelection(this.CurrentQuery);
            this.tvMyQueries.UpdateSelection(this.CurrentQuery);
        }

        private void verticalSplit_Layout(object sender, LayoutEventArgs e)
        {
            if (!this.IsExplorerHidden)
            {
                this.UpdateMessagePosition();
            }
        }

        private void verticalSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            this.UpdateMessagePosition();
            this.IsSplitting = false;
        }

        private void verticalSplit_SplitterMoved_1(object sender, SplitterEventArgs e)
        {
            if (this.CurrentQueryControl != null)
            {
                this.CurrentQueryControl.FocusQuery();
            }
            else if (this.tcQueries != null)
            {
                this.tcQueries.Focus();
            }
        }

        private void viewSamplesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tcQueryTrees.SelectedTab = this.pagSamples;
            foreach (TreeNode node in this.sampleQueries.Nodes)
            {
                node.Collapse();
            }
            foreach (TreeNode node in this.sampleQueries.Nodes)
            {
                node.Expand();
            }
        }

        private void whatsNewtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] bytes;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LINQPad.WhatsNew.html"))
            {
                bytes = new BinaryReader(stream).ReadBytes((int) stream.Length);
            }
            string s = Encoding.UTF8.GetString(bytes).Replace("id='LP4'", "id='LP4', style='visibility:hidden'");
            bytes = Encoding.UTF8.GetBytes(s);
            this.DisplayHtmlFile(bytes, "WhatsNew.html");
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1c)
            {
                _appActive = m.WParam != IntPtr.Zero;
            }
            try
            {
                if (!(((m.Msg != 0x112L) || !(m.WParam == new IntPtr(0xf120L))) ? (m.Msg != 0xa3L) : false) && (this.CurrentQueryControl != null))
                {
                    this.CurrentQueryControl.NotifyWindowRestoreFromMaximize();
                }
                if (!((((m.Msg != 0x86) || !(m.LParam == IntPtr.Zero)) || ((this.CurrentQueryControl == null) || !this.CurrentQueryControl.IsPluginSelected())) || (((Control.MouseButtons == MouseButtons.None) || !this.CurrentQueryControl.IsMouseInPlugIn()) ? (this.CurrentQueryControl.LastPluginFocus <= DateTime.UtcNow.AddMilliseconds(-100.0)) : false)))
                {
                    m.WParam = new IntPtr(1);
                    this._fakeActiveFormTimer.Start();
                }
            }
            catch
            {
            }
            base.WndProc(ref m);
        }

        internal bool ApplyEditToPlugin { get; private set; }

        internal bool AutoScrollResults
        {
            get
            {
                return this.miAutoScroll.Checked;
            }
            set
            {
                if (value != this.miAutoScroll.Checked)
                {
                    this.ToggleAutoScrollResults(true);
                }
            }
        }

        internal RunnableQuery CurrentQuery
        {
            get
            {
                if (base.InvokeRequired)
                {
                    return this._lastQuery;
                }
                QueryControl currentQueryControl = this.CurrentQueryControl;
                if (currentQueryControl == null)
                {
                    return null;
                }
                return currentQueryControl.Query;
            }
        }

        internal QueryControl CurrentQueryControl
        {
            get
            {
                if (((this.tcQueries.TabPages.Count == 0) || (this.tcQueries.SelectedTab == null)) || (this.tcQueries.SelectedTab.Controls.Count == 0))
                {
                    return null;
                }
                return (this.tcQueries.SelectedTab.Controls[0] as QueryControl);
            }
            set
            {
                TabPage page = this.tcQueries.TabPages.Cast<TabPage>().FirstOrDefault<TabPage>(page => page.Controls[0] == value);
                if (page != null)
                {
                    this.tcQueries.SelectedTab = page;
                }
            }
        }

        public bool IsActivated
        {
            get
            {
                return _activated;
            }
        }

        public bool IsActive
        {
            get
            {
                return _active;
            }
        }

        public bool IsAppActive
        {
            get
            {
                return _appActive;
            }
        }

        internal bool IsExplorerHidden
        {
            get
            {
                return ((this.tcQueries != null) && (this.tcQueries.Parent == this));
            }
        }

        internal bool IsPremium
        {
            get
            {
                return (this.ShowLicensee && this._isPremium);
            }
        }

        private QueryControl NextCacheQueryControl
        {
            get
            {
                return ((this._cacheQueryControls.Count == 0) ? null : this._cacheQueryControls.Peek());
            }
        }

        public bool OptimizeQueries
        {
            get
            {
                return UserOptionsLive.Instance.OptimizeQueries;
            }
            set
            {
                UserOptionsLive.Instance.OptimizeQueries = value;
            }
        }

        internal LINQPad.UI.ResultsDockForm ResultsDockForm
        {
            get
            {
                return this._resultsDockForm;
            }
        }

        internal bool ShowLicensee
        {
            get
            {
                return this._showLicensee;
            }
            private set
            {
                if (this._showLicensee != value)
                {
                    this._showLicensee = value;
                    foreach (QueryControl control in this.GetQueryControlsWithCache())
                    {
                        control.UpdateAutocompletionMsg();
                    }
                    if (value)
                    {
                        AutoRefManager.Initialize(0x3e8);
                    }
                    if (value)
                    {
                        TypeResolver.WarmupEngine(MyExtensions.AdditionalRefs);
                    }
                }
            }
        }

        public bool VerticalResultsLayout { get; private set; }

        private class MruToolStripMenuItem : ToolStripMenuItem
        {
        }

        private class QueryControlToStringWrapper
        {
            public readonly LINQPad.UI.QueryControl QueryControl;

            public QueryControlToStringWrapper(LINQPad.UI.QueryControl qc)
            {
                this.QueryControl = qc;
            }

            public override string ToString()
            {
                return this.QueryControl.Query.Name;
            }
        }
    }
}

