namespace LINQPad.UI
{
    using ActiproBridge;
    using ActiproSoftware.Drawing;
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.CSharp;
    using ActiproSoftware.SyntaxEditor.Addons.DotNet.Ast;
    using LINQPad;
    using LINQPad.ExecutionModel;
    using LINQPad.Properties;
    using Microsoft.Win32;
    using mscorlib;
    using System;
    using System.CodeDom.Compiler;
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
    using System.Xml.Linq;

    [ComVisible(true)]
    public class QueryControl : UserControl
    {
        private Timer _autoSaveTimer;
        private volatile bool _autoSaving;
        private bool? _autoScrollResultsFromQuery;
        private bool _browserHidden;
        private Timer _clockTimer;
        private bool _compileOnly;
        private List<BitmapBookmarkLineIndicator> _currentExecutionStack;
        private DataResultsWebBrowser _dataBrowser;
        private BrowserBorder _dataPanel;
        private DocumentManager _docMan;
        private QueryEditor _editor;
        private bool _enableUseCurrentDb;
        private Color? _errorSquigglyColor;
        private QueryLanguage _executingQueryLanguage;
        private bool _executingQueryOptimized;
        private Timer _executionTrackingTimer;
        private bool _firstExecutionTrack;
        private bool _firstResultsShow;
        private bool _gotPluginsReadyMessage;
        private volatile bool _gotQueryCompletionMessage;
        private Timer _ieComExceptionTimer;
        private ResultsWebBrowser _ilBrowser;
        private MemoryStream _ilData;
        private bool _ilDirty;
        private BrowserBorder _ilPanel;
        private ResultsWebBrowser _lambdaBrowser;
        private MemoryStream _lambdaData;
        private BrowserBorder _lambdaPanel;
        private bool? _lastAutoScrollResultsFromQuery;
        private QueryCompilationEventArgs _lastCompilation;
        private static string _lastDefaultQueryFolder;
        private int _lastExecutionLine;
        private int _lastExecutionLineCount;
        private ExecutionProgress? _lastExecutionProgress;
        private int _lastExecutionTrackCost1;
        private int _lastExecutionTrackCost2;
        private QueryLanguage _lastQueryKind;
        private string _lastRanSourceSelection;
        private static string _lastSaveFolder;
        private DateTime _lastServerAction;
        private ExecutionTrackInfo _lastTrackInfo;
        private long _memoryAtStart;
        private bool _modifiedWhenRunning;
        private MemoryStream _msData;
        private double _oldHorizontalSplitFraction;
        private double _oldVerticalSplitFraction;
        private bool _optimizeTipShown;
        private string _outputInfoMessage;
        private bool _pendingReflection;
        private bool _pendingResultsShow;
        private string _pendingSqlTranslation;
        private ContextMenuStrip _pluginWinButtonMenu;
        private List<ToolStripButton> _pluginWinButtons;
        private PluginWindowManager _pluginWinManager;
        private volatile bool _processingProvisionalData;
        private RunnableQuery _query;
        private int _queryCount;
        private int _querySelectionStartCol;
        private int _querySelectionStartRow;
        private Random _random;
        private ReadLinePanel _readLinePanel;
        private bool _readLinePanelVisible;
        private int _refreshTicksOnResults;
        private Timer _refreshTimer;
        private byte[] _resultsContent;
        private SchemaTree _schemaTree;
        private bool _splitterMovingViaToolStrip;
        private int _suppressPullCount;
        private bool _uberCancelMessage;
        private static bool _warnedAboutOptimizationTracking;
        private Color? _warningSquigglyColor;
        private ToolStripMenuItem btn1NestingLevel;
        private ToolStripMenuItem btn2NestingLevels;
        private ToolStripMenuItem btn3NestingLevels;
        private ToolStripButton btnActivateAutocompletion;
        private ToolStripMenuItem btnAllNestingLevels;
        private ToolStripDropDownButton btnAnalyze;
        private ToolStripDropDownButton btnArrange;
        private ImageButton btnCancel;
        private ClearButton btnClose;
        private ImageButton btnExecute;
        private ToolStripDropDownButton btnExport;
        private ToolStripMenuItem btnExportExcel;
        private ToolStripMenuItem btnExportExcelNoFormat;
        private ToolStripMenuItem btnExportHtml;
        private ToolStripMenuItem btnExportWord;
        private ToolStripMenuItem btnExportWordNoFormat;
        private ToolStripDropDownButton btnFormat;
        private ClearButton btnGrids;
        private ToolStripButton btnIL;
        private ToolStripButton btnLambda;
        private ClearButton btnPin;
        private ToolStripMenuItem btnResultFormattingPreferences;
        private ToolStripButton btnResults;
        private ToolStripButton btnSql;
        private ClearButton btnText;
        private ComboBox cboDb;
        private ComboBox cboLanguage;
        private IContainer components;
        private Label lblDb;
        private ToolStripStatusLabel lblElapsed;
        private ToolStripStatusLabel lblExecTime;
        private ToolStripStatusLabel lblFill;
        private ToolStripStatusLabel lblMiscStatus;
        private ToolStripStatusLabel lblOptimize;
        private ToolStripStatusLabel lblStatus;
        private Label lblSyncDb;
        private Label lblType;
        private ToolStripStatusLabel lblUberCancel;
        private FixedLinkLabel llDbUseCurrent;
        private ToolStripMenuItem miArrangeVertical;
        private ToolStripMenuItem miAutoScroll;
        private ToolStripMenuItem miHideResults;
        private ToolStripMenuItem miKeyboardShortcuts;
        private ToolStripMenuItem miOpenInSSMS;
        private ToolStripMenuItem miOpenSQLQueryNewTab;
        private ToolStripMenuItem miScrollEnd;
        private ToolStripMenuItem miScrollStart;
        private ToolStripMenuItem miUndock;
        private Panel panBottom;
        private Panel panCloseButton;
        private Panel panEditor;
        private Panel panError;
        private Panel panMain;
        private PanelEx panOutput;
        private Panel panTop;
        private TableLayoutPanel panTopControls;
        private ToolStripProgressBar queryProgressBar;
        private SplitContainer splitContainer;
        private StatusStrip statusStrip;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripSeparator toolStripSeparator1;
        private ToolTip toolTip;
        private ToolStripEx tsOutput;
        private TextBox txtError;
        private ActiproSoftware.SyntaxEditor.SyntaxEditor txtSQL;

        internal event EventHandler NextQueryRequest;

        internal event EventHandler PreviousQueryRequest;

        internal event EventHandler QueryClosed;

        internal QueryControl(RunnableQuery q, SchemaTree schemaTree)
        {
            EventHandler onClick = null;
            EventHandler handler2 = null;
            EventHandler handler3 = null;
            EventHandler handler4 = null;
            SplitterCancelEventHandler handler5 = null;
            this._browserHidden = true;
            Timer timer = new Timer {
                Interval = 0x2710,
                Enabled = true
            };
            this._autoSaveTimer = timer;
            this._executionTrackingTimer = new Timer();
            this._pluginWinButtons = new List<ToolStripButton>();
            this._query = new RunnableQuery();
            this.NextQueryRequest = delegate (object sender, EventArgs e) {
            };
            this.PreviousQueryRequest = delegate (object sender, EventArgs e) {
            };
            this._lambdaData = new MemoryStream();
            this._ilDirty = true;
            this._ilData = new MemoryStream();
            this._queryCount = 0;
            Timer timer2 = new Timer {
                Interval = 200
            };
            this._refreshTimer = timer2;
            Timer timer3 = new Timer {
                Interval = 200
            };
            this._clockTimer = timer3;
            this._firstResultsShow = true;
            this._lastQueryKind = QueryLanguage.SQL;
            this._suppressPullCount = 0;
            this._random = new Random();
            this.components = null;
            this._query = q;
            this._schemaTree = schemaTree;
            try
            {
                this.Font = FontManager.GetDefaultFont();
            }
            catch
            {
            }
            this.InitializeComponent();
            this.CheckIsMyExtensions();
            this._pluginWinManager = new PluginWindowManager(this);
            this._pluginWinButtonMenu = new ContextMenuStrip(this.components);
            if (onClick == null)
            {
                onClick = (sender, e) => this.CloseCurrentVisualizer();
            }
            this._pluginWinButtonMenu.Items.Add("Close visualizer (Shift+F4 or Middle-Click or Ctrl+Click)", Resources.Delete, onClick);
            if (this.btnExecute.Height > 0x20)
            {
                this.btnExecute.Height -= 3;
                this.cboLanguage.Margin = new Padding(this.cboLanguage.Margin.Left, 2, this.cboLanguage.Margin.Right, 4);
                this.cboDb.Margin = new Padding(this.cboDb.Margin.Left, 2, this.cboDb.Margin.Right, 4);
            }
            else if (this.btnExecute.Height > 0x1d)
            {
                this.btnExecute.Height -= 2;
            }
            else if (this.btnExecute.Height > 0x18)
            {
                this.btnExecute.Height--;
            }
            this.btnCancel.Height = this.btnExecute.Height;
            this.btnText.Height = this.btnGrids.Height = this.btnExecute.Height - 1;
            if ((IntPtr.Size == 4) && (this.btnExecute.Height > 0x1b))
            {
                Padding margin = this.cboLanguage.Margin;
                this.cboLanguage.Margin = new Padding(margin.Left, margin.Top, margin.Right, margin.Bottom - 1);
                margin = this.cboDb.Margin;
                this.cboDb.Margin = new Padding(margin.Left, margin.Top, margin.Right, margin.Bottom - 1);
            }
            if (this.btnExecute.Height > 40)
            {
                this.btnExecute.Image = ControlUtil.ResizeImage(this.btnExecute.Image, this.btnExecute.Width, this.btnExecute.Height, true);
                this.btnCancel.Image = ControlUtil.ResizeImage(this.btnCancel.Image, this.btnCancel.Width, this.btnCancel.Height, true);
                this.btnGrids.Image = ControlUtil.ResizeImage(this.btnGrids.Image, (this.btnGrids.Width * 3) / 4, (this.btnGrids.Height * 3) / 4, true);
                this.btnText.Image = ControlUtil.ResizeImage(this.btnText.Image, (this.btnText.Width * 3) / 4, (this.btnText.Height * 3) / 4, true);
            }
            this.UpdateAutocompletionMsg();
            try
            {
                this.txtError.Font = new Font("Verdana", 8.25f);
                this.txtSQL.Font = new Font("Verdana", 9.5f);
                this.btnActivateAutocompletion.Font = new Font("Verdana", 8f, FontStyle.Bold);
                this.lblOptimize.Font = new Font("Verdana", 7f, FontStyle.Bold);
            }
            catch
            {
            }
            this.txtSQL.get_Document().set_Language(DocumentManager.GetDynamicLanguage("SQL", SystemColors.Window.GetBrightness()));
            this.txtSQL.get_Document().get_Outlining().set_Mode(2);
            this.txtSQL.set_BracketHighlightingVisible(true);
            this.txtSQL.get_Document().set_ReadOnly(true);
            VisualStudio2005SyntaxEditorRenderer renderer = new VisualStudio2005SyntaxEditorRenderer();
            SimpleBorder border = new SimpleBorder();
            border.set_Style(0);
            renderer.set_Border(border);
            VisualStudio2005SyntaxEditorRenderer renderer2 = renderer;
            this.txtSQL.set_Renderer(renderer2);
            this._docMan = new DocumentManager(this._query, this);
            this.CreateEditor();
            this.PropagateOptions();
            this.UpdateEditorZoom();
            this.CreateBrowser();
            this._browserHidden = true;
            this.panBottom.BorderStyle = BorderStyle.None;
            this.tsOutput.BackColor = Color.Transparent;
            this.tsOutput.Renderer = new OutputToolsRenderer();
            this.statusStrip.BackColor = Color.Transparent;
            this.statusStrip.Padding = new Padding(this.statusStrip.Padding.Left, this.statusStrip.Padding.Top, this.statusStrip.Padding.Left, this.statusStrip.Padding.Bottom);
            this.PullData(QueryChangedEventArgs.Refresh);
            this.ToggleResultsCollapse();
            this._query.QueryCompiled += new EventHandler<QueryCompilationEventArgs>(this._query_QueryCompiled);
            this._query.PluginsReady += new EventHandler(this._query_PluginsReady);
            this._query.CustomClickCompleted += new EventHandler(this._query_CustomClickCompleted);
            this._query.QueryCompleted += new EventHandler<QueryStatusEventArgs>(this._query_QueryCompleted);
            this._query.QueryChanged += new EventHandler<QueryChangedEventArgs>(this._query_QueryChanged);
            this._query.ReadLineRequested += new EventHandler<ReadLineEventArgs>(this._query_ReadLineRequested);
            this._editor.TextChanged += new EventHandler(this._editor_TextChanged);
            this._editor.add_SelectionChanged(new SelectionEventHandler(this, (IntPtr) this._editor_SelectionChanged));
            this._editor.RepositoryDropped += new EventHandler<QueryEditor.RepositoryEventArgs>(this._editor_RepositoryDropped);
            this._schemaTree.AfterSelect += new TreeViewEventHandler(this._schemaTree_AfterSelect);
            this._docMan.CheckForRepositoryChange();
            this._refreshTimer.Tick += new EventHandler(this.RefreshTimer_Tick);
            this._clockTimer.Tick += new EventHandler(this.ClockTimer_Tick);
            this._autoSaveTimer.Tick += new EventHandler(this.AutoSaveTimer_Tick);
            if (handler2 == null)
            {
                handler2 = (sender, e) => this.ReportMainThreadPosition();
            }
            this._executionTrackingTimer.Tick += handler2;
            if (SystemColors.Window.GetBrightness() < 0.5f)
            {
                this.llDbUseCurrent.ForeColor = SystemColors.HotTrack;
            }
            this.ExtendOutputSplitter();
            if (handler3 == null)
            {
                handler3 = (sender, e) => this.UpdateErrorHeight();
            }
            this.panError.SizeChanged += handler3;
            this.statusStrip.Parent = null;
            base.Controls.Add(this.statusStrip);
            this.statusStrip.SendToBack();
            if (handler4 == null)
            {
                handler4 = (sender, e) => this.RequestWinManagerRelocation();
            }
            EventHandler handler6 = handler4;
            this.panOutput.Resize += handler6;
            for (Control control = this.panOutput; control != null; control = control.Parent)
            {
                control.Move += handler6;
            }
            this._query.PluginWindowManager = this._pluginWinManager;
            this.queryProgressBar.Margin = new Padding(3, 3, 0, 1);
            this.tsOutput.Padding = new Padding(0, 0, 0, 2);
            this.splitContainer.SplitterWidth--;
            if (handler5 == null)
            {
                handler5 = delegate (object sender, SplitterCancelEventArgs e) {
                    if (Control.MouseButtons == MouseButtons.Left)
                    {
                        MainForm.Instance.IsSplitting = true;
                    }
                    if ((this.panOutput.BackColor == MainForm.Instance.TransparencyKey) && (this.panOutput.BackColor != Program.LightTransparencyKey))
                    {
                        this.panOutput.BackColor = MainForm.Instance.TransparencyKey = Program.LightTransparencyKey;
                    }
                };
            }
            this.splitContainer.SplitterMoving += handler5;
            this.panOutput.BorderStyle = BorderStyle.None;
            this.panOutput.BorderColor = Color.FromArgb(160, 160, 160);
            this.lblSyncDb.Cursor = Cursors.Hand;
            this.toolTip.ShowAlways = true;
            this.EnableControls();
            this.panBottom.Layout += new LayoutEventHandler(this.panBottom_Layout);
        }

        private void _browser_LinqClicked(object sender, LinqClickEventArgs e)
        {
            WebHelper.LaunchBrowser(e.Uri.ToString());
        }

        private void _browser_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.F5)
            {
                e.IsInputKey = true;
                this.Run();
            }
            else if (e.KeyData == (Keys.Control | Keys.Shift | Keys.E))
            {
                MainForm.Instance.ToggleAutoScrollResults(true);
            }
        }

        private void _editor_RepositoryDropped(object sender, QueryEditor.RepositoryEventArgs e)
        {
            Func<LinkedDatabase, bool> predicate = null;
            if (!this._query.IsMyExtensions)
            {
                this.CheckToFromProgramLanguage(this._query.QueryKind, this._query.QueryKind, this._query.Repository != null, e.Repository != null);
                if (!((this._query.Repository != null) && e.Copy))
                {
                    this._query.Repository = e.Repository;
                }
                else
                {
                    if ((!e.Repository.IsSqlServer || !this._query.Repository.IsSqlServer) || (e.Repository.Server.ToLowerInvariant() != this._query.Repository.Server.ToLowerInvariant()))
                    {
                        MessageBox.Show("Multi-database queries are supported only for SQL Server databases on the same server (or linked servers).", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                    if (this._query.Repository.IsAzure || e.Repository.IsAzure)
                    {
                        MessageBox.Show("SQL Azure does not permit cross-database queries.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                    if (this._query.Repository.Database == e.Repository.Database)
                    {
                        return;
                    }
                    if (predicate == null)
                    {
                        predicate = d => string.IsNullOrEmpty(d.Server) && (d.Database == e.Repository.Database);
                    }
                    if (this._query.Repository.LinkedDatabases.Any<LinkedDatabase>(predicate))
                    {
                        return;
                    }
                    if (this._query.Repository.Parent != null)
                    {
                        Repository r = this._query.Repository.Clone();
                        r.ID = Guid.NewGuid();
                        r.ShowServer = false;
                        r.Persist = false;
                        r.LinkedDatabases = r.LinkedDatabases.Concat<LinkedDatabase>(new LinkedDatabase[] { new LinkedDatabase(e.Repository.Database) });
                        this._schemaTree.AddCx(r, false, false);
                        this._query.Repository = r;
                    }
                    else
                    {
                        this._query.Repository.LinkedDatabases = this._query.Repository.LinkedDatabases.Concat<LinkedDatabase>(new LinkedDatabase[] { new LinkedDatabase(e.Repository.Database) });
                        this._schemaTree.UpdateRepository(this._query.Repository);
                    }
                }
                this._editor.Focus();
            }
        }

        private void _editor_SelectionChanged(object sender, EventArgs e)
        {
            this.ClearQueryHighlight();
            if (this._docMan.ExecutedSelectionLayer.get_Count() > 0)
            {
                this._docMan.ExecutedSelectionLayer.Clear();
            }
        }

        private void _editor_TextChanged(object sender, EventArgs e)
        {
            this._modifiedWhenRunning = true;
            using (this.SuppressPull())
            {
                this._query.Source = this._editor.Text;
            }
            this.btnPin.Checked = this._query.Pinned;
            this.ClearQueryHighlight();
            if (this._docMan.StackTraceLayer.get_Count() > 0)
            {
                this._docMan.StackTraceLayer.Clear();
            }
            this.ClearExecutionTrackingIndicators();
        }

        private void _query_CustomClickCompleted(object sender, EventArgs e)
        {
            this._outputInfoMessage = null;
        }

        private void _query_PluginsReady(object sender, EventArgs e)
        {
            this.BeginInvoke(delegate {
                this._gotPluginsReadyMessage = true;
                foreach (PluginControl control in this._pluginWinManager.GetControls())
                {
                    this.CreatePluginWinButton(control, false, false);
                }
                if (this._pluginWinButtons.Any<ToolStripButton>())
                {
                    bool flag = MainForm.Instance.CurrentQueryControl == this;
                    if (this.btnResults.Checked && !this.panError.Visible)
                    {
                        if (flag)
                        {
                            base.FindForm().Activate();
                        }
                        this.SelectOutputPanel(this._pluginWinButtons[0], false);
                    }
                    else
                    {
                        this.UpdateOutputToolStripLayout();
                    }
                    if (flag)
                    {
                        base.FindForm().Activate();
                    }
                    else
                    {
                        this._pluginWinManager.Hide();
                    }
                }
            });
        }

        private void _query_QueryChanged(object sender, QueryChangedEventArgs e)
        {
            this.KillIEComExceptionTimer();
            this.PullData(e);
        }

        private void _query_QueryCompiled(object sender, QueryCompilationEventArgs e)
        {
            this.BeginInvoke(delegate {
                try
                {
                    this.QueryCompiled(e);
                }
                catch (Exception exception)
                {
                    Program.ProcessException(exception);
                }
            });
        }

        private void _query_QueryCompleted(object sender, QueryStatusEventArgs e)
        {
            if (e.ExecutionComplete)
            {
                this._clockTimer.Stop();
            }
            this.BeginInvoke(() => this.QueryCompleted(e));
        }

        private void _query_ReadLineRequested(object sender, ReadLineEventArgs e)
        {
            int num = this._queryCount;
            while (this._readLinePanelVisible)
            {
                Thread.Sleep(100);
                if (this._queryCount != num)
                {
                    return;
                }
            }
            base.BeginInvoke(() => this.ShowReadLinePanel(e.Client, e.Prompt, e.DefaultValue, e.Options));
        }

        private void _schemaTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.UpdateFocusedRepository();
        }

        internal void ActivateAndKillDomain()
        {
            base.FindForm().Activate();
            Program.RunOnWinFormsTimer(() => this.Cancel(true), 0x7d0);
        }

        internal void ActivateHelp()
        {
            if (!(MainForm.Instance.ShowLicensee || !(this._editor.get_SelectedView().get_SelectedText() == "")))
            {
                MessageBox.Show("This feature requires an Autocompletion license.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                MemberHelpInfo memberHelpInfo = this.GetMemberHelpInfo();
                string str = ((memberHelpInfo == null) || !memberHelpInfo.HasStrongName) ? null : memberHelpInfo.get_HelpSearchString();
                if (string.IsNullOrEmpty(str))
                {
                    str = (this._editor.get_SelectedView().get_SelectedText() ?? "").Trim();
                    if (str.Length == 0)
                    {
                        return;
                    }
                    if (this._query.QueryKind <= QueryLanguage.Program)
                    {
                        str = "C# " + str;
                    }
                    else if (this._query.QueryKind.ToString().StartsWith("VB"))
                    {
                        str = "VB " + str;
                    }
                    else if (this._query.QueryKind.ToString().StartsWith("FSharp"))
                    {
                        str = "F# " + str;
                    }
                    else if (this._query.QueryKind == QueryLanguage.SQL)
                    {
                        str = "\"SQL Server\" " + str;
                    }
                    else
                    {
                        str = this._query.QueryKind + " " + str;
                    }
                }
                WebHelper.LaunchBrowser("http://www.google.com/search?hl=en&q=" + Uri.EscapeDataString(str));
            }
        }

        public void ActivateReflector()
        {
            if (!MainForm.Instance.ShowLicensee)
            {
                MessageBox.Show("This feature requires an Autocompletion license.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                MemberHelpInfo memberHelpInfo = this.GetMemberHelpInfo();
                if ((memberHelpInfo != null) && !string.IsNullOrEmpty(memberHelpInfo.get_ReflectorCodeUri()))
                {
                    ReflectorAgent.ActivateReflector(memberHelpInfo);
                }
            }
        }

        internal void AncestorMoved()
        {
            this.RequestWinManagerRelocation();
        }

        internal bool AreResultsCollapsed()
        {
            return (this.panEditor.Parent == this.panMain);
        }

        internal bool AreResultsDetached()
        {
            return (this.panBottom.Parent != this.splitContainer.Panel2);
        }

        internal bool AreResultsVisible()
        {
            return (!this.AreResultsCollapsed() || this.AreResultsDetached());
        }

        internal void AttachResultsControl(Control c)
        {
            c.Parent = this.splitContainer.Panel2;
            if (this.AreResultsCollapsed())
            {
                this.ToggleResultsCollapse();
            }
            this.UpdateOutputVisibility();
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (!base.IsDisposed && !(!this._query.IsModified || this._autoSaving))
            {
                ThreadPool.QueueUserWorkItem(delegate (object param0) {
                    if (!this._autoSaving)
                    {
                        this._autoSaving = true;
                        try
                        {
                            this._query.AutoSave(false);
                        }
                        catch
                        {
                        }
                        finally
                        {
                            this._autoSaving = false;
                        }
                    }
                });
            }
        }

        private void BeginInvoke(Action a)
        {
            base.BeginInvoke(a);
        }

        private void btn1NestingLevel_Click(object sender, EventArgs e)
        {
            this.CollapseResultsTo(1);
        }

        private void btn2NestingLevels_Click(object sender, EventArgs e)
        {
            this.CollapseResultsTo(2);
        }

        private void btn3NestingLevels_Click(object sender, EventArgs e)
        {
            this.CollapseResultsTo(3);
        }

        private void btnActivateAutocompletion_Click(object sender, EventArgs e)
        {
            MainForm.Instance.ActivateAutocompletion();
        }

        private void btnAllNestingLevels_Click(object sender, EventArgs e)
        {
            this.CollapseResultsTo(null);
        }

        private void btnArrange_DropDownOpening(object sender, EventArgs e)
        {
            this.miUndock.Checked = MainForm.Instance.ResultsDockForm.AreResultsTorn;
            this.miArrangeVertical.Checked = MainForm.Instance.VerticalResultsLayout;
            this.miUndock.Enabled = MainForm.Instance.ResultsDockForm.AreResultsTorn || (Screen.AllScreens.Length > 1);
            this.miAutoScroll.Checked = MainForm.Instance.AutoScrollResults;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Cancel(false);
            this._editor.Focus();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.TryClose();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            this.Run();
        }

        private void btnExpandTypes_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            if ((this._msData != null) && (this._msData.Length > 0L))
            {
                this.OpenExcel(this.ExportResults(false));
            }
        }

        private void btnExportExcelNoFormat_Click(object sender, EventArgs e)
        {
            if ((this._msData != null) && (this._msData.Length > 0L))
            {
                this.OpenExcel(this.ExportResults(true));
            }
        }

        private void btnExportHtml_Click(object sender, EventArgs e)
        {
            if ((this._msData != null) && (this._msData.Length != 0L))
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Title = "Save Results";
                    dialog.DefaultExt = "html";
                    dialog.Filter = "HTML files (*.html)|*.html";
                    if ((dialog.ShowDialog() == DialogResult.OK) && !string.IsNullOrEmpty(dialog.FileName))
                    {
                        try
                        {
                            using (FileStream stream = File.Create(dialog.FileName))
                            {
                                this._msData.WriteTo(stream);
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show("Cannot write file: " + exception.Message);
                        }
                    }
                }
            }
        }

        private void btnExportWord_Click(object sender, EventArgs e)
        {
            if ((this._msData != null) && (this._msData.Length > 0L))
            {
                this.OpenWord(this.ExportResults(false));
            }
        }

        private void btnExportWordNoFormat_Click(object sender, EventArgs e)
        {
            if ((this._msData != null) && (this._msData.Length > 0L))
            {
                this.OpenWord(this.ExportResults(true));
            }
        }

        private void btnGrids_Click(object sender, EventArgs e)
        {
            this._query.ToDataGrids = true;
        }

        private void btnIL_Click(object sender, EventArgs e)
        {
            this.SelectILPanel(true);
        }

        private void btnLambda_Click(object sender, EventArgs e)
        {
            this.SelectLambdaPanel(true);
        }

        private void btnPin_Click(object sender, EventArgs e)
        {
            this._query.Pinned = !this._query.Pinned;
        }

        private void btnResultFormattingPreferences_Click(object sender, EventArgs e)
        {
            using (OptionsForm form = new OptionsForm(2))
            {
                if (form.ShowDialog(MainForm.Instance) == DialogResult.OK)
                {
                    this.PropagateOptions();
                }
            }
        }

        private void btnResults_Click(object sender, EventArgs e)
        {
            this.SelectResultsPanel(true);
        }

        private void btnSql_Click(object sender, EventArgs e)
        {
            this.SelectSqlPanel(true);
        }

        private void btnText_Click(object sender, EventArgs e)
        {
            this._query.ToDataGrids = false;
        }

        internal void Cancel(bool uberMode)
        {
            if (!base.IsDisposed)
            {
                this._outputInfoMessage = null;
                this._executionTrackingTimer.Stop();
                this.ClearExecutionTrackingIndicators();
                if (this._editor.get_Document().get_LineIndicators().get_Count() == 0)
                {
                    this._editor.set_IndicatorMarginVisible(false);
                }
                if ((((this._query.QueryKind != QueryLanguage.SQL) && (this._query.QueryKind != QueryLanguage.ESQL)) || this.btnCancel.Enabled) || uberMode)
                {
                    this.HideReadLinePanel();
                    this._pendingReflection = false;
                    if (!((uberMode || Program.PreserveAppDomains) || this._query.IsRunning))
                    {
                        uberMode = true;
                    }
                    if (uberMode)
                    {
                        this.ResetPluginManager(true);
                        this._query.Cancel(false, true);
                    }
                    else
                    {
                        this._query.Cancel(true, false);
                    }
                    if (this.btnCancel.Enabled || uberMode)
                    {
                        this.EnableControls();
                        this.lblElapsed.Visible = false;
                        this.lblStatus.Text = uberMode ? "Application Domain Unloaded" : "Query canceled";
                        this.lblUberCancel.Visible = false;
                    }
                }
            }
        }

        private void cboDb_DropDown(object sender, EventArgs e)
        {
            int num;
            this.UpdateRepositoryItems(true);
            Func<object, float> selector = null;
            using (Graphics g = this.cboDb.CreateGraphics())
            {
                if (selector == null)
                {
                    selector = item => g.MeasureString(item.ToString(), this.cboDb.Font).Width;
                }
                num = (((int) this.cboDb.Items.Cast<object>().Max<object>(selector)) + SystemInformation.VerticalScrollBarWidth) + 2;
            }
            Rectangle workingArea = Screen.FromControl(this.cboDb).WorkingArea;
            this.cboDb.DropDownWidth = Math.Max(this.cboDb.Width, Math.Min((workingArea.Right - this.cboDb.PointToScreen(Point.Empty).X) - 3, num));
        }

        private void cboDb_DropDownClosed(object sender, EventArgs e)
        {
            this._editor.Focus();
        }

        private void cboDb_Enter(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Alt)
            {
                this.cboDb.DroppedDown = true;
            }
        }

        private void cboDb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cboDb.SelectedItem != null)
            {
                using (this.SuppressPull())
                {
                    this.CheckToFromProgramLanguage();
                    if (this.cboDb.SelectedItem is string)
                    {
                        if ((((string) this.cboDb.SelectedItem) == "<None>") && (this._query.Repository != null))
                        {
                            this._query.Repository = null;
                        }
                        else
                        {
                            this.cboDb.SelectedIndex = 0;
                        }
                    }
                    Repository selectedItem = this.cboDb.SelectedItem as Repository;
                    if (selectedItem != null)
                    {
                        this._query.Repository = selectedItem;
                    }
                    this.lblSyncDb.Visible = this._query.Repository != null;
                }
                this.btnPin.Checked = this._query.Pinned;
                this.UpdateFocusedRepository();
                this._docMan.CheckForRepositoryChange();
                this._schemaTree.UpdateSqlMode(this._query);
            }
        }

        private void cboLanguage_SelectionChangeCommitted(object sender, EventArgs e)
        {
            this.CheckToFromProgramLanguage();
            this._schemaTree.UpdateSqlMode(this._query);
        }

        private void cboType_DropDownClosed(object sender, EventArgs e)
        {
            this._editor.Focus();
        }

        private void cboType_Enter(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Alt)
            {
                this.cboLanguage.DroppedDown = true;
            }
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._editor.get_IntelliPrompt().get_MemberList().get_Visible())
            {
                this._editor.get_IntelliPrompt().get_MemberList().Abort();
            }
            QueryLanguage language = this.IndexToQueryLanguage(this.cboLanguage.SelectedIndex);
            if (language != this._query.QueryKind)
            {
                this._query.QueryKind = language;
            }
        }

        private bool CheckAndPromptQueryDriver()
        {
            if ((this._query.Repository == null) || this._query.Repository.DriverLoader.IsValid)
            {
                return true;
            }
            if (MessageBox.Show("The database for this query relies on the following custom driver which has not been installed:\r\n\r\n  " + this._query.Repository.DriverLoader.SimpleAssemblyName + " (" + this._query.Repository.DriverLoader.PublicKeyToken + ")\r\n\r\nWould you like to view the publicly available drivers?", "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (BrowseDriversForm form = new BrowseDriversForm(true))
                {
                    form.ShowDialog(MainForm.Instance);
                    if (form.DoRestart)
                    {
                        MainForm.Instance.Restart();
                    }
                }
            }
            return false;
        }

        internal void CheckAutocompletionCache()
        {
            this._docMan.ConfigureResolver();
        }

        private void CheckIsMyExtensions()
        {
            if (this._query.IsMyExtensions)
            {
                if (this.cboLanguage.Items.Count > 1)
                {
                    this.cboLanguage.Items.Clear();
                    this.cboLanguage.Items.Add("C# Program");
                }
                this.cboDb.Enabled = false;
            }
        }

        private void CheckQueryRepositoryWithSchemaTree()
        {
            if ((this._query.Repository != null) && (this._query.Repository != this._schemaTree.GetCurrentRepository(true)))
            {
                bool isModified = this._query.IsModified;
                this._schemaTree.RegisterRepository(this._query, false, false);
                if (!(isModified || !this._query.IsModified))
                {
                    this._query.IsModified = false;
                }
            }
        }

        internal void CheckToFromProgramLanguage()
        {
            this.CheckToFromProgramLanguage(this._query.QueryKind, this.IndexToQueryLanguage(this.cboLanguage.SelectedIndex), this._query.Repository != null, this.cboDb.SelectedIndex > 0);
        }

        internal void CheckToFromProgramLanguage(QueryLanguage oldLanguage, QueryLanguage newLanguage, bool oldDC, bool newDC)
        {
            string str2;
            bool flag = (oldLanguage == QueryLanguage.FSharpProgram) && oldDC;
            bool flag2 = (newLanguage == QueryLanguage.FSharpProgram) && newDC;
            string str = UserOptions.Instance.ConvertTabsToSpaces ? "".PadRight(UserOptions.Instance.TabSizeActual) : "\t";
            if ((oldLanguage == QueryLanguage.Program) && (newLanguage != QueryLanguage.Program))
            {
                if (this._editor.Text.StartsWith("void Main", StringComparison.Ordinal) && this._editor.Text.TrimEnd(new char[0]).EndsWith("// Define other methods and classes here", StringComparison.Ordinal))
                {
                    str2 = Regex.Replace(Regex.Replace(this._editor.Text, @"^void Main\s*\(\s*\)\s*{\s*", ""), @"\s*}\s*// Define other methods and classes here\s*$", "").Replace("\n" + str, "\n");
                    this._editor.Text = str2;
                }
            }
            else if ((oldLanguage == QueryLanguage.VBProgram) && (newLanguage != QueryLanguage.VBProgram))
            {
                if (this._editor.Text.StartsWith("Sub Main", StringComparison.OrdinalIgnoreCase) && this._editor.Text.TrimEnd(new char[0]).EndsWith("' Define other methods and classes here", StringComparison.OrdinalIgnoreCase))
                {
                    str2 = Regex.Replace(Regex.Replace(this._editor.Text, @"^Sub Main\s*", "", RegexOptions.IgnoreCase), @"\s*End Sub\s*' Define other methods and classes here\s*$", "", RegexOptions.IgnoreCase).Replace("\n" + str, "\n");
                    this._editor.Text = str2;
                }
            }
            else if (!(!flag || flag2) && this._editor.Text.StartsWith("let dc = new TypedDataContext()", StringComparison.Ordinal))
            {
                this._editor.Text = this._editor.Text.Substring("let dc = new TypedDataContext()".Length).TrimStart(new char[0]);
            }
            if ((oldLanguage != QueryLanguage.Program) && (newLanguage == QueryLanguage.Program))
            {
                if (!((this._editor.Text.Contains("void Main") || this._editor.Text.ToUpperInvariant().Contains("SUB MAIN")) || this._editor.Text.Contains("Task Main")))
                {
                    this._editor.Text = "void Main()\n{\n" + str + this._query.Source.Replace("\n", "\n" + str) + "\n}\n\n// Define other methods and classes here\n";
                    this._editor.get_SelectedView().get_Selection().set_TextRange(new TextRange(14 + str.Length));
                }
            }
            else if ((oldLanguage != QueryLanguage.VBProgram) && (newLanguage == QueryLanguage.VBProgram))
            {
                if (!(this._editor.Text.ToUpperInvariant().Contains("SUB MAIN") || this._editor.Text.Contains("void Main")))
                {
                    this._editor.Text = "Sub Main\n" + str + this._query.Source.Replace("\n", "\n" + str) + "\nEnd Sub\n\n' Define other methods and classes here\n";
                    this._editor.get_SelectedView().get_Selection().set_TextRange(new TextRange(9 + str.Length));
                }
            }
            else if (!(flag || !flag2) && !this._editor.Text.Contains("let dc = new TypedDataContext()"))
            {
                this._editor.Text = "let dc = new TypedDataContext()\r\n\r\n" + this._query.Source;
                this._editor.get_SelectedView().get_Selection().set_TextRange(new TextRange("let dc = new TypedDataContext()".Length + 2));
            }
        }

        private void ClearExecutionTrackingIndicators()
        {
            if (this._currentExecutionStack != null)
            {
                foreach (BitmapBookmarkLineIndicator indicator in this._currentExecutionStack)
                {
                    this._editor.get_Document().get_LineIndicators().Remove(indicator);
                }
                this._currentExecutionStack = null;
            }
        }

        private void ClearQueryHighlight()
        {
            if (this._editor.get_CurrentLineHighlightingVisible())
            {
                this._editor.set_CurrentLineHighlightingVisible(false);
            }
        }

        private void ClearRegion()
        {
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            this.UpdateElapsed();
        }

        internal void Close()
        {
            this._query.ClearAutoSave();
            this.KillIEComExceptionTimer();
            if (this.QueryClosed != null)
            {
                this.QueryClosed(this, EventArgs.Empty);
            }
        }

        internal void CloseCurrentVisualizer()
        {
            this.CloseVisualizer(this.GetSelectedPluginControl());
        }

        internal void CloseVisualizer(PluginControl c)
        {
            if (c != null)
            {
                ToolStripButton button = (this.GetSelectedPluginControl() == c) ? this._pluginWinButtons.FirstOrDefault<ToolStripButton>(b => (b.Tag == c)) : null;
                int num = (button == null) ? -1 : this.tsOutput.Items.IndexOf(button);
                ToolStripButton selectedButton = (num < 1) ? null : (this.tsOutput.Items[num - 1] as ToolStripButton);
                try
                {
                    this._pluginWinManager.DisposeControl(c);
                    if (selectedButton != null)
                    {
                        this.SelectOutputPanel(selectedButton, false);
                    }
                }
                catch
                {
                }
            }
        }

        internal void CollapseResultsTo(int? depth)
        {
            this._dataBrowser.CollapseTo(depth);
        }

        internal void CompleteParam()
        {
            if (this._editor.get_Document().get_Language() is CSharpSyntaxLanguage)
            {
                this._editor.get_Document().get_Language().ShowIntelliPromptParameterInfo(this._editor);
            }
        }

        internal void CompleteWord()
        {
            if (this._editor.get_Document().get_Language() is CSharpSyntaxLanguage)
            {
                this._editor.get_Document().get_Language().IntelliPromptCompleteWord(this._editor);
            }
        }

        internal void CopyPlain()
        {
            this._editor.CopyPlain();
        }

        private void CreateBrowser()
        {
            this._dataBrowser = new DataResultsWebBrowser();
            this._lambdaBrowser = new ResultsWebBrowser();
            this._ilBrowser = new ResultsWebBrowser();
            this._dataBrowser.PreviewKeyDown += new PreviewKeyDownEventHandler(this._browser_PreviewKeyDown);
            this._dataBrowser.LinqClicked += new EventHandler<LinqClickEventArgs>(this._browser_LinqClicked);
            this._lambdaBrowser.PreviewKeyDown += new PreviewKeyDownEventHandler(this._browser_PreviewKeyDown);
            this._ilBrowser.PreviewKeyDown += new PreviewKeyDownEventHandler(this._browser_PreviewKeyDown);
            this._dataPanel = new BrowserBorder();
            BrowserBorder border = new BrowserBorder {
                BackColor = Control.DefaultBackColor
            };
            this._lambdaPanel = border;
            BrowserBorder border2 = new BrowserBorder {
                BackColor = Control.DefaultBackColor
            };
            this._ilPanel = border2;
            this._dataPanel.Controls.Add(this._dataBrowser);
            this._dataPanel.Dock = DockStyle.Fill;
            this._dataPanel.Hide();
            this._lambdaPanel.Controls.Add(this._lambdaBrowser);
            this._lambdaPanel.Dock = DockStyle.Fill;
            this._lambdaPanel.Hide();
            this._ilPanel.Controls.Add(this._ilBrowser);
            this._ilPanel.Dock = DockStyle.Fill;
            this._ilPanel.Hide();
            this.panOutput.Controls.Add(this._dataPanel);
            this.panOutput.Controls.Add(this._lambdaPanel);
            this.panOutput.Controls.Add(this._ilPanel);
            this._dataBrowser.ObjectForScripting = this;
        }

        private void CreateEditor()
        {
            EventHandler handler = null;
            EventHandler handler2 = null;
            EventHandler handler3 = null;
            this._editor = new QueryEditor();
            this._editor.UriLayer = this._docMan.UriLayer;
            this._editor.WarningsLayer = this._docMan.WarningsLayer;
            this._editor.MainErrorLayer = this._docMan.MainErrorLayer;
            this._editor.StackTraceLayer = this._docMan.StackTraceLayer;
            this._editor.set_Document(this._docMan.Document);
            if (Program.PresentationMode)
            {
                if (handler == null)
                {
                    handler = (sender, e) => this.NextQueryRequest(this, EventArgs.Empty);
                }
                this._editor.NextQueryRequest += handler;
                if (handler2 == null)
                {
                    handler2 = (sender, e) => this.PreviousQueryRequest(this, EventArgs.Empty);
                }
                this._editor.PreviousQueryRequest += handler2;
                if (handler3 == null)
                {
                    handler3 = delegate (object sender, EventArgs e) {
                        if (!(this.AreResultsDetached() || this.AreResultsCollapsed()))
                        {
                            this.ToggleResultsCollapse();
                        }
                    };
                }
                this._editor.EscapeRequest += handler3;
            }
            this._editor.KeyPress += delegate (object sender, KeyPressEventArgs e) {
                if (this._docMan != null)
                {
                    this._docMan.CheckForRepositoryChange();
                }
            };
            this.panEditor.Controls.Add(this._editor);
            this._editor.BringToFront();
        }

        private void CreatePluginWinButton(PluginControl c, bool activate, bool afterCurrent)
        {
            Action a = delegate {
                if (!this._pluginWinButtons.Any<ToolStripButton>(b => (b.Tag == c)))
                {
                    bool flag = !MainForm.Instance.IsActive && !MainForm.Instance.ResultsDockForm.IsActive;
                    ToolStripButton button = new ToolStripButton(c.Heading) {
                        DisplayStyle = ToolStripItemDisplayStyle.Text,
                        Margin = new Padding(0, 0, 0, 1),
                        Tag = c,
                        ToolTipText = c.ToolTipText
                    };
                    button.Click += delegate (object sender, EventArgs e) {
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            this.CloseVisualizer(c);
                        }
                        else
                        {
                            this.SelectOutputPanel(button, true);
                        }
                    };
                    button.MouseEnter += delegate (object sender, EventArgs e) {
                        this.tsOutput.Cursor = Cursors.Default;
                        try
                        {
                            button.ToolTipText = c.ToolTipText;
                            bool isActive = MainForm.Instance.IsActive;
                            if (!(string.IsNullOrEmpty(button.ToolTipText) || isActive))
                            {
                                this._pluginWinManager.ShowToolTip(button.ToolTipText);
                            }
                        }
                        catch
                        {
                        }
                    };
                    button.MouseLeave += delegate (object sender, EventArgs e) {
                        if (!(string.IsNullOrEmpty(c.ToolTipText) || MainForm.Instance.IsActive))
                        {
                            this._pluginWinManager.ShowToolTip(null);
                        }
                    };
                    button.MouseDown += delegate (object sender, MouseEventArgs e) {
                        if (e.Button == MouseButtons.Middle)
                        {
                            this.CloseVisualizer(c);
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            this.SelectOutputPanel(button, false);
                            Point p = this.tsOutput.PointToScreen(e.Location);
                            p.Offset(button.Bounds.Location);
                            Program.RunOnWinFormsTimer(delegate {
                                this.FocusQueryExplicit();
                                this._pluginWinButtonMenu.Show(p);
                            }, 50);
                        }
                    };
                    if ((afterCurrent && this.IsPluginSelected()) && flag)
                    {
                        int index = this._pluginWinButtons.IndexOf(this.GetSelectedPanelButton());
                        this._pluginWinButtons.Insert(index + 1, button);
                        this.tsOutput.Items.Insert(index + 3, button);
                    }
                    else
                    {
                        this._pluginWinButtons.Add(button);
                        this.tsOutput.Items.Insert(this.tsOutput.Items.IndexOf(this.btnLambda), button);
                    }
                    if (activate)
                    {
                        this.SelectOutputPanel(button, false);
                    }
                }
            };
            if (base.InvokeRequired)
            {
                this.BeginInvoke(a);
            }
            else
            {
                a();
            }
        }

        public bool CustomClick(string idString, bool graphTruncated)
        {
            int id;
            if (int.TryParse(idString, out id))
            {
                ContextMenuStrip strip = new ContextMenuStrip();
                strip.Items.Add(new ToolStripMenuItem("Explore " + (graphTruncated ? "all rows " : "") + "in grid", null, delegate (object sender, EventArgs e) {
                    if (!this._query.HasDomain)
                    {
                        MessageBox.Show("The query's application domain has been unloaded. Re-run the query to enable the object explorer.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else if (this._query.MessageLoopEnded)
                    {
                        MessageBox.Show("The query was faulted. Re-run to enable the object explorer.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        this.lblMiscStatus.Text = this._outputInfoMessage = "Querying...";
                        try
                        {
                            this.lblMiscStatus.Font = new Font(this.Font.FontFamily, 9f, FontStyle.Bold);
                        }
                        catch
                        {
                        }
                        this.lblMiscStatus.BackColor = Color.FromArgb(250, 250, 140);
                        this._query.CallCustomClick(id);
                    }
                }));
                strip.Show(Control.MousePosition);
            }
            return false;
        }

        private void DestroyBrowsers(bool recreate)
        {
            BrowserBorder border = this._lambdaPanel;
            BrowserBorder border2 = this._dataPanel;
            BrowserBorder border3 = this._ilPanel;
            ResultsWebBrowser browser = this._lambdaBrowser;
            DataResultsWebBrowser browser2 = this._dataBrowser;
            ResultsWebBrowser browser3 = this._ilBrowser;
            if (recreate)
            {
                this.CreateBrowser();
            }
            browser2.Dispose();
            browser.Dispose();
            browser3.Dispose();
            this.panOutput.Controls.Remove(border2);
            this.panOutput.Controls.Remove(border);
            this.panOutput.Controls.Remove(border3);
            border.Dispose();
            border2.Dispose();
            border3.Dispose();
        }

        internal Control DetachResultsControl()
        {
            if (!this.AreResultsCollapsed())
            {
                this.ToggleResultsCollapse();
            }
            this.panBottom.Parent = null;
            this.UpdateOutputVisibility();
            return this.panBottom;
        }

        private void DisplayError(string msg)
        {
            if ((msg == "InvalidOperationException: The calling thread must be STA, because many UI components require this.") || msg.StartsWith("ThreadStateException: Current thread must be set to single thread apartment (STA) mode before OLE calls can be made."))
            {
                if (UserOptions.Instance.MTAThreadingMode)
                {
                    msg = msg + "\r\n\r\n(You've got this error because you've requested MTA threads in LINQPad - go to Edit | Preferences | Advanced to switch this off.)";
                }
                else if (this._query.AllFileReferences.Any<string>(r => r.EndsWith(".winmd", StringComparison.InvariantCultureIgnoreCase)))
                {
                    msg = msg + "\r\n\r\n(You've got this error because you've referenced a Windows Runtime assembly which requires MTA mode.)";
                }
            }
            this.panError.Visible = true;
            this.txtError.Text = msg;
            this.UpdateErrorHeight();
        }

        private void DisplayUberCancel(bool highlight)
        {
            this._uberCancelMessage = true;
            this.lblExecTime.Visible = false;
            this.lblUberCancel.Visible = true;
            this.lblUberCancel.ForeColor = Color.DarkRed;
            if (highlight)
            {
                this.lblUberCancel.BackColor = Color.FromArgb(250, 250, 140);
                Timer tmr = new Timer {
                    Enabled = true,
                    Interval = 0x7d0
                };
                tmr.Tick += delegate (object sender, EventArgs e) {
                    tmr.Dispose();
                    if (!this.IsDisposed)
                    {
                        this.lblUberCancel.BackColor = Color.Transparent;
                    }
                };
            }
            else
            {
                this.lblUberCancel.BackColor = Color.Transparent;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.KillIEComExceptionTimer();
                if (this._dataBrowser != null)
                {
                    this._dataBrowser.ObjectForScripting = null;
                }
                this._pluginWinManager.Reset(false);
                this._query.Dispose();
                this._query = null;
                this._schemaTree.AfterSelect -= new TreeViewEventHandler(this._schemaTree_AfterSelect);
                if (this._docMan != null)
                {
                    this._docMan.Dispose();
                }
                this._docMan = null;
                if (this._readLinePanel != null)
                {
                    this._readLinePanel.Dispose();
                    this._readLinePanel = null;
                }
                this._dataBrowser.Dispose();
                this._lambdaBrowser.Dispose();
                this._ilBrowser.Dispose();
                this._dataPanel.Dispose();
                this._lambdaPanel.Dispose();
                this._ilPanel.Dispose();
                this._refreshTimer.Dispose();
                this._clockTimer.Dispose();
                this._autoSaveTimer.Dispose();
                this._executionTrackingTimer.Dispose();
                this._pluginWinManager.Dispose();
            }
            base.Dispose(disposing);
        }

        public bool DoesBrowserHaveFocus()
        {
            return ((((this._dataBrowser == null) || !this._dataBrowser.Focused) && ((this._lambdaBrowser == null) || !this._lambdaBrowser.Focused)) ? ((this._ilBrowser != null) && this._ilBrowser.Focused) : true);
        }

        internal void EditorBackColorChanged()
        {
            this._docMan.ConfigureLanguage();
            this._docMan.ConfigureResolver();
            this._editor.UpdateColors(true);
        }

        private void EnableControls()
        {
            bool queryRunning = this.QueryRunning;
            this.btnCancel.Enabled = queryRunning;
            this.btnExecute.Enabled = this.cboLanguage.Enabled = !queryRunning;
            this.cboDb.Enabled = !this._query.IsMyExtensions && !queryRunning;
            this.queryProgressBar.Visible = queryRunning && (this._query.ExecutionProgress != ExecutionProgress.Async);
        }

        public bool ExecuteQuery(string query, int queryKind)
        {
            this.KillIEComExceptionTimer();
            query = Encoding.UTF8.GetString(Convert.FromBase64String(query));
            QueryControl control = MainForm.Instance.NewQuerySameProps(query, true);
            if ((queryKind >= 0) && (queryKind <= 9))
            {
                control.Query.QueryKind = (QueryLanguage) queryKind;
            }
            control.Query.IsModified = false;
            control.Run();
            MainForm.Instance.UpdateQueryUI(control.Query);
            return false;
        }

        private string ExportResults(bool stripFormatting)
        {
            this._msData.Position = 0L;
            XDocument document = XDocument.Load(new StreamReader(this._msData));
            this._msData.Position = 0L;
            XNamespace namespace2 = "http://www.w3.org/1999/xhtml";
            document.Descendants((XName) (namespace2 + "script")).Remove<XElement>();
            (from el in document.Descendants((XName) (namespace2 + "span"))
                where ((string) el.Attribute("class")) == "typeglyph"
                select el).Remove<XElement>();
            (from a in document.Descendants().Attributes("style")
                where ((string) a) == "display:none"
                select a).Remove();
            if (stripFormatting)
            {
                document.Descendants((XName) (namespace2 + "style")).Remove<XElement>();
                (from tr in document.Descendants((XName) (namespace2 + "tr"))
                    where tr.Elements().Any<XElement>(td => ((string) td.Attribute("class")) == "typeheader")
                    select tr).Remove<XElement>();
                (from e in document.Descendants((XName) (namespace2 + "i"))
                    where e.Value == "null"
                    select e).Remove<XElement>();
            }
            foreach (XElement element in document.Descendants((XName) (namespace2 + "a")).ToArray<XElement>())
            {
                element.ReplaceWith(element.Nodes());
            }
            foreach (XElement element2 in (from e in document.Descendants((XName) (namespace2 + "table"))
                where ((string) e.Attribute("class")) == "headingpresenter"
                where e.Elements().Count<XElement>() == 2
                select e).ToArray<XElement>())
            {
                IEnumerable<XElement> enumerable = element2.Elements().First<XElement>().Elements();
                IEnumerable<XElement> content = element2.Elements().Skip<XElement>(1).First<XElement>().Elements();
                if (stripFormatting)
                {
                    element2.ReplaceWith(new object[] { enumerable, new XElement((XName) (namespace2 + "p"), content) });
                }
                else
                {
                    element2.ReplaceWith(new object[] { new XElement((XName) (namespace2 + "br")), new XElement((XName) (namespace2 + "span"), new object[] { new XAttribute("style", "color: green; font-weight:bold; font-size: 110%;"), enumerable }), content });
                }
            }
            foreach (XElement element3 in document.Descendants((XName) (namespace2 + "th")))
            {
                element3.Name = (XName) (namespace2 + "td");
                if (!(stripFormatting || (element3.Attribute("style") != null)))
                {
                    element3.Add(new XAttribute("style", "font-weight: bold; background-color: #ddd;"));
                }
            }
            string path = Path.ChangeExtension(Path.GetTempFileName(), ".html");
            File.WriteAllText(path, document.ToString().Replace("Ξ", "").Replace("▪", ""));
            return path;
        }

        private void ExtendOutputSplitter()
        {
            EventHandler handler = null;
            this.tsOutput.MouseDown += delegate (object sender, MouseEventArgs e) {
                if (((e.Button == MouseButtons.Left) && (this.tsOutput.GetItemAt(e.Location) == null)) && (this.splitContainer.Orientation != Orientation.Vertical))
                {
                    this._splitterMovingViaToolStrip = true;
                    MouseEventArgs args = new MouseEventArgs(e.Button, e.Clicks, e.X, this.splitContainer.SplitterRectangle.Bottom - 1, e.Delta);
                    MethodInfo method = this.splitContainer.GetType().GetMethod("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(this.splitContainer, new MouseEventArgs[] { args });
                    }
                }
            };
            this.tsOutput.MouseUp += delegate (object sender, MouseEventArgs e) {
                if (this._splitterMovingViaToolStrip)
                {
                    this._splitterMovingViaToolStrip = false;
                    MethodInfo method = this.splitContainer.GetType().GetMethod("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(this.splitContainer, new MouseEventArgs[] { e });
                    }
                }
            };
            this.tsOutput.MouseMove += delegate (object sender, MouseEventArgs e) {
                if (this.splitContainer.Orientation != Orientation.Vertical)
                {
                    this.tsOutput.Cursor = (this.tsOutput.GetItemAt(e.Location) != null) ? Cursors.Default : Cursors.HSplit;
                    if (this._splitterMovingViaToolStrip && (e.Button == MouseButtons.Left))
                    {
                        MethodInfo method = this.splitContainer.GetType().GetMethod("OnMouseMove", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (method != null)
                        {
                            method.Invoke(this.splitContainer, new MouseEventArgs[] { e });
                        }
                    }
                }
            };
            this.tsOutput.MouseLeave += (sender, e) => (this.tsOutput.Cursor = Cursors.Default);
            foreach (ToolStripItem item in this.tsOutput.Items)
            {
                if (handler == null)
                {
                    handler = (sender, e) => this.tsOutput.Cursor = Cursors.Default;
                }
                item.MouseEnter += handler;
            }
        }

        internal void FindNext()
        {
            if (QueryEditor.Options.get_FindText().Length == 0)
            {
                this.FindReplace();
            }
            else
            {
                bool flag = QueryEditor.Options.get_SearchUp();
                QueryEditor.Options.set_SearchUp(false);
                this._editor.get_SelectedView().get_FindReplace().Find(QueryEditor.Options);
                QueryEditor.Options.set_SearchUp(flag);
                this._editor.Focus();
            }
        }

        internal void FindNextSelected()
        {
            string str = (this._editor.get_SelectedView().get_SelectedText().Length > 0) ? this._editor.get_SelectedView().get_SelectedText() : this._editor.get_SelectedView().GetCurrentWordText();
            if (!string.IsNullOrEmpty(str))
            {
                QueryEditor.Options.set_FindText(str);
                this.FindNext();
            }
        }

        internal void FindPrevious()
        {
            if (QueryEditor.Options.get_FindText().Length == 0)
            {
                QueryEditor.Options.set_SearchUp(true);
                this.FindReplace();
            }
            else
            {
                bool flag = QueryEditor.Options.get_SearchUp();
                QueryEditor.Options.set_SearchUp(true);
                this._editor.get_SelectedView().get_FindReplace().Find(QueryEditor.Options);
                QueryEditor.Options.set_SearchUp(flag);
                this._editor.Focus();
            }
        }

        internal void FindPreviousSelected()
        {
            string str = (this._editor.get_SelectedView().get_SelectedText().Length > 0) ? this._editor.get_SelectedView().get_SelectedText() : this._editor.get_SelectedView().GetCurrentWordText();
            if (!string.IsNullOrEmpty(str))
            {
                QueryEditor.Options.set_FindText(str);
                this.FindPrevious();
            }
        }

        internal void FindReplace()
        {
            this._editor.Focus();
            if (string.IsNullOrEmpty(QueryEditor.Options.get_FindText()) || (this._editor.get_SelectedView().get_SelectedText().Length > 0))
            {
                string str = (this._editor.get_SelectedView().get_SelectedText().Length > 0) ? this._editor.get_SelectedView().get_SelectedText() : this._editor.get_SelectedView().GetCurrentWordText();
                if (!string.IsNullOrEmpty(str))
                {
                    QueryEditor.Options.set_FindText(str);
                }
            }
            using (LINQPadFindReplaceForm form = new LINQPadFindReplaceForm(this._editor, QueryEditor.Options))
            {
                form.ShowInTaskbar = false;
                try
                {
                    form.Font = FontManager.GetDefaultFont();
                }
                catch
                {
                }
                Button button = form.Controls.OfType<Button>().FirstOrDefault<Button>(b => b.Text.Contains("Mark"));
                if (button != null)
                {
                    button.Hide();
                }
                CheckBox box = form.Controls.OfType<CheckBox>().FirstOrDefault<CheckBox>(b => b.Text.Contains("hidden"));
                if (box != null)
                {
                    box.Hide();
                }
                CheckBox box2 = form.Controls.OfType<CheckBox>().FirstOrDefault<CheckBox>(b => b.Text.Contains("selection"));
                if ((box2 != null) && (box != null))
                {
                    box2.Location = box.Location;
                }
                form.ShowDialog(MainForm.Instance);
            }
        }

        internal void FixEditorScrollBars()
        {
            this._editor.set_ScrollBarType(6);
            this._editor.set_ScrollBarType(0);
        }

        internal void FixOutliningStartupBug()
        {
            this._editor.IsOutliningEnabled = (this._query.QueryKind == QueryLanguage.Program) || (this._query.QueryKind == QueryLanguage.VBProgram);
        }

        internal void FocusQuery()
        {
            this.FocusQuery(false);
        }

        internal void FocusQuery(bool focusEllipses)
        {
            if (((!base.IsDisposed && (this._editor != null)) && (this._editor.get_Document() != null)) && (this._docMan != null))
            {
                if ((!focusEllipses && this._readLinePanelVisible) && (this._readLinePanel != null))
                {
                    this._readLinePanel.FocusTextBox();
                }
                else
                {
                    this._editor.Focus();
                    if (focusEllipses)
                    {
                        string text = this._editor.get_Document().GetText(0);
                        int index = text.IndexOf('…');
                        if (index >= 0)
                        {
                            this._editor.get_Document().set_Text(text.Substring(0, index) + text.Substring(index + 1));
                            this._editor.get_Caret().set_Offset(index);
                        }
                        else
                        {
                            index = text.IndexOf("...");
                            if (index >= 0)
                            {
                                this._editor.get_SelectedView().get_Selection().set_StartOffset(index);
                                this._editor.get_SelectedView().get_Selection().set_EndOffset(index + 3);
                            }
                        }
                    }
                }
                this._docMan.CheckForRepositoryChange();
            }
        }

        internal void FocusQueryExplicit()
        {
            if (!((!this._readLinePanelVisible || (this._readLinePanel == null)) || this._readLinePanel.ContainsFocus))
            {
                this._readLinePanel.FocusTextBox();
            }
            else
            {
                this._editor.Focus();
            }
        }

        internal void FocusSelectedPlugin()
        {
            if (this._pluginWinManager != null)
            {
                PluginControl selectedPluginControl = this.GetSelectedPluginControl();
                if (selectedPluginControl != null)
                {
                    this.LastPluginFocus = DateTime.UtcNow;
                    this._pluginWinManager.Show(selectedPluginControl, true);
                }
            }
        }

        private static Control GetActiveControl()
        {
            Form activeForm = Form.ActiveForm;
            if (activeForm != null)
            {
                Control activeControl = activeForm.ActiveControl;
                if (activeControl != null)
                {
                    while (activeControl is ContainerControl)
                    {
                        Control control3 = ((ContainerControl) activeControl).ActiveControl;
                        if (control3 == null)
                        {
                            return activeControl;
                        }
                        activeControl = control3;
                    }
                    return activeControl;
                }
            }
            return null;
        }

        private Color GetErrorSquigglyColor()
        {
            if (!this._errorSquigglyColor.HasValue)
            {
                this._errorSquigglyColor = new Color?((UserOptions.Instance.ActualEditorBackColor.GetBrightness() < 0.4f) ? Color.FromArgb(140, 180, 0xff) : Color.Blue);
            }
            return this._errorSquigglyColor.Value;
        }

        private string GetErrorText(CompilerError error)
        {
            if (((this._query.AdditionalNamespaces.Length > 0) && (error.Line <= 0)) && ((error.ErrorNumber == "CS0246") || (error.ErrorNumber == "CS0234")))
            {
                return (Regex.Replace(error.ErrorText, @"\(.+\)", "") + " (Press F4 to check imported namespaces)");
            }
            return error.ErrorText;
        }

        private MemberHelpInfo GetMemberHelpInfo()
        {
            if (MainForm.Instance.ShowLicensee)
            {
                if (this._editor.get_IntelliPrompt().get_MemberList().get_Visible())
                {
                    CustomIntelliPromptMemberListItem item = this._editor.get_IntelliPrompt().get_MemberList().get_SelectedItem() as CustomIntelliPromptMemberListItem;
                    if ((item != null) && (item.MemberHelpInfo != null))
                    {
                        return item.MemberHelpInfo;
                    }
                }
                if (this._editor.get_IntelliPrompt().get_QuickInfo().get_Visible() && (AutocompletionManager.get_QuickInfoHelp() != null))
                {
                    return AutocompletionManager.get_QuickInfoHelp();
                }
                if (this._editor.get_IntelliPrompt().get_ParameterInfo().get_Visible())
                {
                    return AutocompletionManager.get_ParamInfoHelp();
                }
                CustomCSharpSyntaxLanguage language = this._editor.get_Document().get_Language() as CustomCSharpSyntaxLanguage;
                if (language != null)
                {
                    return language.GetMemberHelpInfoAtCaret(this._editor);
                }
            }
            return null;
        }

        private int GetOffsetFromRowCol(int row, int column)
        {
            if (this.Doc.get_Lines().get_Count() == 0)
            {
                return 0;
            }
            if (column == -1)
            {
                column = 0;
            }
            if ((this._querySelectionStartRow > 0) || (this._querySelectionStartCol > 0))
            {
                if (row == 0)
                {
                    column += this._querySelectionStartCol;
                }
                row += this._querySelectionStartRow;
            }
            if (row >= this.Doc.get_Lines().get_Count())
            {
                row = this.Doc.get_Lines().get_Count() - 1;
                if (row < 0)
                {
                    return -1;
                }
                column = Math.Max(0, this.Doc.get_Lines().get_Item(row).get_Length() - 1);
            }
            if ((row < 0) || (row >= this.Doc.get_Lines().get_Count()))
            {
                return -1;
            }
            DocumentLine line = this.Doc.get_Lines().get_Item(row);
            if (column > line.get_Length())
            {
                return -1;
            }
            return (line.get_StartOffset() + Math.Max(0, column));
        }

        private Control GetOutputControl(ToolStripButton selectedButton)
        {
            if (selectedButton == this.btnResults)
            {
                return this._dataBrowser;
            }
            if (selectedButton == this.btnLambda)
            {
                return this._lambdaBrowser;
            }
            if (selectedButton == this.btnSql)
            {
                return this.txtSQL;
            }
            if (selectedButton == this.btnIL)
            {
                return this._ilBrowser;
            }
            return null;
        }

        private Control GetOutputPanel(ToolStripButton selectedButton)
        {
            if (selectedButton == this.btnResults)
            {
                return this._dataPanel;
            }
            if (selectedButton == this.btnLambda)
            {
                return this._lambdaPanel;
            }
            if (selectedButton == this.btnSql)
            {
                return this.txtSQL;
            }
            if (selectedButton == this.btnIL)
            {
                return this._ilPanel;
            }
            return null;
        }

        private IEnumerable<ToolStripButton> GetPanelSelectorButtons(bool includePlugins)
        {
            ToolStripButton[] first = new ToolStripButton[] { this.btnResults, this.btnLambda, this.btnSql, this.btnIL };
            if (!includePlugins)
            {
                return first;
            }
            return first.Concat<ToolStripButton>(this._pluginWinButtons);
        }

        internal EditManager GetPluginEditManager()
        {
            if (this._pluginWinManager == null)
            {
                return null;
            }
            return this._pluginWinManager.EditManager;
        }

        private ToolStripButton GetSelectedPanelButton()
        {
            return this.GetPanelSelectorButtons(true).FirstOrDefault<ToolStripButton>(b => b.Checked);
        }

        private PluginControl GetSelectedPluginControl()
        {
            ToolStripButton selectedPanelButton = this.GetSelectedPanelButton();
            if (selectedPanelButton == null)
            {
                return null;
            }
            return (selectedPanelButton.Tag as PluginControl);
        }

        private string GetSSMSLocation()
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(".sql");
            if (key == null)
            {
                return null;
            }
            string name = key.GetValue(null) as string;
            if (name == null)
            {
                return null;
            }
            RegistryKey key2 = Registry.ClassesRoot.OpenSubKey(name);
            if (key2 == null)
            {
                return null;
            }
            key2 = key2.OpenSubKey(@"Shell\Open\Command");
            if (key2 == null)
            {
                return null;
            }
            string source = key2.GetValue(null) as string;
            if (!source.StartsWith("\"", StringComparison.Ordinal))
            {
                return null;
            }
            source = source.Substring(1);
            if (!source.Contains<char>('"'))
            {
                return null;
            }
            return source.Substring(0, source.IndexOf('"'));
        }

        private Color GetWarningSquigglyColor()
        {
            if (!this._warningSquigglyColor.HasValue)
            {
                this._warningSquigglyColor = new Color?((UserOptions.Instance.ActualEditorBackColor.GetBrightness() < 0.4f) ? Color.LightGreen : Color.Green);
            }
            return this._warningSquigglyColor.Value;
        }

        internal Rectangle GetWinManagerTargetBounds()
        {
            Rectangle rectangle;
            rectangle = new Rectangle(Point.Empty, this.panOutput.ClientSize) {
                X = rectangle.X + this.panOutput.Padding.Left,
                Y = rectangle.Y + this.panOutput.Padding.Top,
                Width = rectangle.Width - (this.panOutput.Padding.Left + this.panOutput.Padding.Right),
                Height = rectangle.Height - (this.panOutput.Padding.Top + this.panOutput.Padding.Bottom)
            };
            return this.panOutput.RectangleToScreen(rectangle);
        }

        public bool GotoRowColumn(int row, int column)
        {
            Action a = null;
            this.KillIEComExceptionTimer();
            int offsetFromRowCol = this.GetOffsetFromRowCol(row, column);
            if (offsetFromRowCol >= 0)
            {
                this._editor.get_Caret().set_Offset(offsetFromRowCol);
                this._editor.Focus();
                this._editor.set_CurrentLineHighlightingVisible(true);
                if (a == null)
                {
                    a = delegate {
                        this._editor.set_CurrentLineHighlightingVisible(false);
                    };
                }
                Program.RunOnWinFormsTimer(a, 300);
            }
            return false;
        }

        private void HideReadLinePanel()
        {
            if (this.lblStatus.Text == "Awaiting user input")
            {
                this.lblStatus.Text = "";
            }
            if (this._readLinePanelVisible)
            {
                this._readLinePanelVisible = false;
                this._readLinePanel.Parent = null;
            }
        }

        private void HighlightErrorOrWarning(int row, int column, string message, bool warning, bool compileTime, bool isMainError)
        {
            int offsetFromRowCol = this.GetOffsetFromRowCol(row, column);
            if (offsetFromRowCol != -1)
            {
                row += this._querySelectionStartRow;
                if (isMainError)
                {
                    if (this._editor.get_SelectedView().get_Selection().get_IsZeroLength())
                    {
                        this._editor.get_Caret().set_Offset(offsetFromRowCol);
                        this._editor.set_CurrentLineHighlightingVisible(true);
                    }
                    this._editor.CheckSmartTagAtLocation(offsetFromRowCol + 1);
                }
                if (!(!warning || string.IsNullOrEmpty(message)))
                {
                    message = "Warning: " + message;
                }
                if (column != -1)
                {
                    int num3 = this._editor.get_Document().GetWordTextRange(offsetFromRowCol).get_EndOffset() - offsetFromRowCol;
                    if ((num3 == 0) && (offsetFromRowCol > 0))
                    {
                        offsetFromRowCol--;
                        num3++;
                    }
                    if (num3 > 0)
                    {
                        Color color = warning ? this.GetWarningSquigglyColor() : this.GetErrorSquigglyColor();
                        SpanIndicatorLayer layer = isMainError ? this._docMan.MainErrorLayer : this._docMan.WarningsLayer;
                        SpanIndicator indicator = layer.GetIndicatorsForTextRange(new TextRange(offsetFromRowCol, offsetFromRowCol + num3)).FirstOrDefault<SpanIndicator>();
                        if (indicator is CompilerErrorSpanIndicator)
                        {
                            System.Boolean ReflectorVariable0;
                            if (message != null)
                            {
                                ReflectorVariable0 = true;
                            }
                            else
                            {
                                ReflectorVariable0 = false;
                            }
                            if (!(ReflectorVariable0 ? (((indicator.get_Tag() as string) ?? "").Split(new char[] { '\n' }).Length >= 5) : true))
                            {
                                indicator.set_Tag(((indicator.get_Tag() as string) ?? "") + "\n" + message);
                            }
                        }
                        else if (indicator == null)
                        {
                            try
                            {
                                CompilerErrorSpanIndicator indicator2 = new CompilerErrorSpanIndicator();
                                indicator2.set_UnderlineColor(color);
                                indicator2.set_Tag(message);
                                CompilerErrorSpanIndicator indicator3 = indicator2;
                                layer.Add(indicator3, offsetFromRowCol, num3);
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                    }
                }
                if (row >= 0)
                {
                    this._editor.set_IndicatorMarginVisible(true);
                    LineIndicator indicator4 = this._editor.get_Document().get_LineIndicators().GetIndicatorsForDocumentLine(row).FirstOrDefault<LineIndicator>(i => !(i is BitmapBookmarkLineIndicator));
                    if ((indicator4 != null) && (message != null))
                    {
                        if (string.IsNullOrEmpty(indicator4.get_Tag() as string))
                        {
                            indicator4.set_Tag(message);
                        }
                        else
                        {
                            indicator4.set_Tag(((string) indicator4.get_Tag()) + "\n" + message);
                        }
                    }
                    else
                    {
                        if (!(compileTime || warning))
                        {
                            indicator4 = new ExceptionLineIndicator();
                        }
                        else if (warning)
                        {
                            indicator4 = new WarningLineIndicator();
                        }
                        else
                        {
                            indicator4 = new ErrorLineIndicator();
                        }
                        indicator4.set_Tag(message);
                        this._editor.get_Document().get_LineIndicators().Add(indicator4, row);
                    }
                }
            }
        }

        private void HighlightStackTraceError(int row, int column)
        {
            int offsetFromRowCol = this.GetOffsetFromRowCol(row, column);
            if ((offsetFromRowCol != -1) && (column != -1))
            {
                int num3 = this._editor.get_Document().GetWordTextRange(offsetFromRowCol).get_EndOffset() - offsetFromRowCol;
                if ((num3 == 0) && (offsetFromRowCol > 0))
                {
                    offsetFromRowCol--;
                    num3++;
                }
                if (num3 > 0)
                {
                    SpanIndicatorLayer stackTraceLayer = this._docMan.StackTraceLayer;
                    if (stackTraceLayer.GetIndicatorsForTextRange(new TextRange(offsetFromRowCol, offsetFromRowCol + num3)).FirstOrDefault<SpanIndicator>() == null)
                    {
                        try
                        {
                            stackTraceLayer.Add(new GrayPinkBackgroundSpanIndicator(), offsetFromRowCol, num3);
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                }
            }
        }

        private QueryLanguage IndexToQueryLanguage(int index)
        {
            if (this._query.IsMyExtensions)
            {
                return QueryLanguage.Program;
            }
            QueryLanguage language2 = (QueryLanguage) index;
            if (language2 >= QueryLanguage.SQL)
            {
                language2 -= 2;
            }
            else if ((language2 == QueryLanguage.FSharpExpression) || (language2 == QueryLanguage.FSharpProgram))
            {
                language2 += 2;
            }
            return language2;
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            Document document = new Document();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(QueryControl));
            this.splitContainer = new SplitContainer();
            this.panEditor = new Panel();
            this.panError = new Panel();
            this.txtError = new TextBox();
            this.panBottom = new Panel();
            this.panOutput = new PanelEx();
            this.txtSQL = new ActiproSoftware.SyntaxEditor.SyntaxEditor();
            this.statusStrip = new StatusStrip();
            this.lblStatus = new ToolStripStatusLabel();
            this.queryProgressBar = new ToolStripProgressBar();
            this.lblExecTime = new ToolStripStatusLabel();
            this.lblFill = new ToolStripStatusLabel();
            this.lblMiscStatus = new ToolStripStatusLabel();
            this.lblUberCancel = new ToolStripStatusLabel();
            this.lblElapsed = new ToolStripStatusLabel();
            this.lblOptimize = new ToolStripStatusLabel();
            this.tsOutput = new ToolStripEx();
            this.btnArrange = new ToolStripDropDownButton();
            this.miHideResults = new ToolStripMenuItem();
            this.miUndock = new ToolStripMenuItem();
            this.miArrangeVertical = new ToolStripMenuItem();
            this.toolStripMenuItem3 = new ToolStripSeparator();
            this.miScrollStart = new ToolStripMenuItem();
            this.miScrollEnd = new ToolStripMenuItem();
            this.miAutoScroll = new ToolStripMenuItem();
            this.toolStripMenuItem4 = new ToolStripSeparator();
            this.miKeyboardShortcuts = new ToolStripMenuItem();
            this.btnResults = new ToolStripButton();
            this.btnLambda = new ToolStripButton();
            this.btnSql = new ToolStripButton();
            this.btnIL = new ToolStripButton();
            this.btnActivateAutocompletion = new ToolStripButton();
            this.btnAnalyze = new ToolStripDropDownButton();
            this.miOpenSQLQueryNewTab = new ToolStripMenuItem();
            this.miOpenInSSMS = new ToolStripMenuItem();
            this.btnExport = new ToolStripDropDownButton();
            this.btnExportExcelNoFormat = new ToolStripMenuItem();
            this.btnExportExcel = new ToolStripMenuItem();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.btnExportWordNoFormat = new ToolStripMenuItem();
            this.btnExportWord = new ToolStripMenuItem();
            this.toolStripMenuItem1 = new ToolStripSeparator();
            this.btnExportHtml = new ToolStripMenuItem();
            this.btnFormat = new ToolStripDropDownButton();
            this.btn1NestingLevel = new ToolStripMenuItem();
            this.btn2NestingLevels = new ToolStripMenuItem();
            this.btn3NestingLevels = new ToolStripMenuItem();
            this.btnAllNestingLevels = new ToolStripMenuItem();
            this.toolStripMenuItem2 = new ToolStripSeparator();
            this.btnResultFormattingPreferences = new ToolStripMenuItem();
            this.lblDb = new Label();
            this.cboLanguage = new ComboBox();
            this.cboDb = new ComboBox();
            this.lblType = new Label();
            this.panTopControls = new TableLayoutPanel();
            this.btnExecute = new ImageButton();
            this.btnCancel = new ImageButton();
            this.llDbUseCurrent = new FixedLinkLabel();
            this.lblSyncDb = new Label();
            this.btnText = new ClearButton();
            this.btnGrids = new ClearButton();
            this.panTop = new Panel();
            this.panCloseButton = new Panel();
            this.btnPin = new ClearButton();
            this.btnClose = new ClearButton();
            this.toolTip = new ToolTip(this.components);
            this.panMain = new Panel();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panEditor.SuspendLayout();
            this.panError.SuspendLayout();
            this.panBottom.SuspendLayout();
            this.panOutput.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.tsOutput.SuspendLayout();
            this.panTopControls.SuspendLayout();
            this.panTop.SuspendLayout();
            this.panCloseButton.SuspendLayout();
            this.panMain.SuspendLayout();
            base.SuspendLayout();
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.Location = new Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = Orientation.Horizontal;
            this.splitContainer.Panel1.Controls.Add(this.panEditor);
            this.splitContainer.Panel2.Controls.Add(this.panBottom);
            this.splitContainer.Size = new Size(0x2cf, 0x192);
            this.splitContainer.SplitterDistance = 0xc5;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 0;
            this.splitContainer.SplitterMoved += new SplitterEventHandler(this.splitContainer_SplitterMoved);
            this.panEditor.Controls.Add(this.panError);
            this.panEditor.Dock = DockStyle.Fill;
            this.panEditor.Location = new Point(0, 0);
            this.panEditor.Name = "panEditor";
            this.panEditor.Size = new Size(0x2cf, 0xc5);
            this.panEditor.TabIndex = 4;
            this.panError.BackColor = SystemColors.Info;
            this.panError.BorderStyle = BorderStyle.Fixed3D;
            this.panError.Controls.Add(this.txtError);
            this.panError.Dock = DockStyle.Top;
            this.panError.Location = new Point(0, 0);
            this.panError.Name = "panError";
            this.panError.Padding = new Padding(3);
            this.panError.Size = new Size(0x2cf, 0x27);
            this.panError.TabIndex = 3;
            this.panError.Visible = false;
            this.panError.Layout += new LayoutEventHandler(this.panError_Layout);
            this.txtError.BackColor = SystemColors.Info;
            this.txtError.BorderStyle = BorderStyle.None;
            this.txtError.Dock = DockStyle.Fill;
            this.txtError.ForeColor = SystemColors.InfoText;
            this.txtError.Location = new Point(3, 3);
            this.txtError.Margin = new Padding(2);
            this.txtError.Multiline = true;
            this.txtError.Name = "txtError";
            this.txtError.ReadOnly = true;
            this.txtError.Size = new Size(0x2c5, 0x1d);
            this.txtError.TabIndex = 0;
            this.panBottom.BorderStyle = BorderStyle.Fixed3D;
            this.panBottom.Controls.Add(this.panOutput);
            this.panBottom.Controls.Add(this.statusStrip);
            this.panBottom.Controls.Add(this.tsOutput);
            this.panBottom.Dock = DockStyle.Fill;
            this.panBottom.Location = new Point(0, 0);
            this.panBottom.Name = "panBottom";
            this.panBottom.Size = new Size(0x2cf, 200);
            this.panBottom.TabIndex = 0;
            this.panOutput.BorderColor = Color.Empty;
            this.panOutput.Controls.Add(this.txtSQL);
            this.panOutput.Dock = DockStyle.Fill;
            this.panOutput.Location = new Point(0, 0x1a);
            this.panOutput.Name = "panOutput";
            this.panOutput.Size = new Size(0x2cb, 0x92);
            this.panOutput.TabIndex = 1;
            this.panOutput.Layout += new LayoutEventHandler(this.panOutput_Layout);
            this.txtSQL.Dock = DockStyle.Fill;
            this.txtSQL.set_Document(document);
            this.txtSQL.set_IndicatorMarginVisible(false);
            this.txtSQL.Location = new Point(0, 0);
            this.txtSQL.Margin = new Padding(2);
            this.txtSQL.Name = "txtSQL";
            this.txtSQL.set_ScrollBarType(0);
            this.txtSQL.Size = new Size(0x2cb, 0x92);
            this.txtSQL.TabIndex = 0;
            this.statusStrip.Items.AddRange(new ToolStripItem[] { this.lblStatus, this.queryProgressBar, this.lblExecTime, this.lblFill, this.lblMiscStatus, this.lblUberCancel, this.lblElapsed, this.lblOptimize });
            this.statusStrip.Location = new Point(0, 0xac);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new Size(0x2cb, 0x18);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 2;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(0x2e, 0x13);
            this.lblStatus.Text = "Ready";
            this.queryProgressBar.Name = "queryProgressBar";
            this.queryProgressBar.Size = new Size(100, 0x12);
            this.queryProgressBar.Style = ProgressBarStyle.Marquee;
            this.queryProgressBar.Visible = false;
            this.lblExecTime.Name = "lblExecTime";
            this.lblExecTime.Size = new Size(0x11, 0x13);
            this.lblExecTime.Text = "  ";
            this.lblExecTime.Visible = false;
            this.lblFill.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.lblFill.Name = "lblFill";
            this.lblFill.Size = new Size(0x7a, 0x13);
            this.lblFill.Spring = true;
            this.lblMiscStatus.Alignment = ToolStripItemAlignment.Right;
            this.lblMiscStatus.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.lblMiscStatus.Name = "lblMiscStatus";
            this.lblMiscStatus.Size = new Size(0x15, 0x13);
            this.lblMiscStatus.Text = "   ";
            this.lblMiscStatus.TextAlign = ContentAlignment.MiddleRight;
            this.lblUberCancel.Alignment = ToolStripItemAlignment.Right;
            this.lblUberCancel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.lblUberCancel.Name = "lblUberCancel";
            this.lblUberCancel.Size = new Size(0x10a, 0x13);
            this.lblUberCancel.Text = "  Press Ctrl+Shift+F5 to cancel all threads ";
            this.lblUberCancel.TextAlign = ContentAlignment.MiddleRight;
            this.lblUberCancel.Visible = false;
            this.lblElapsed.Alignment = ToolStripItemAlignment.Right;
            this.lblElapsed.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.lblElapsed.Name = "lblElapsed";
            this.lblElapsed.Size = new Size(0x11, 0x13);
            this.lblElapsed.Text = "  ";
            this.lblElapsed.TextAlign = ContentAlignment.MiddleRight;
            this.lblOptimize.BorderSides = ToolStripStatusLabelBorderSides.All;
            this.lblOptimize.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.lblOptimize.ImageScaling = ToolStripItemImageScaling.None;
            this.lblOptimize.ImageTransparentColor = Color.White;
            this.lblOptimize.Margin = new Padding(0, 2, 0, 0);
            this.lblOptimize.Name = "lblOptimize";
            this.lblOptimize.Padding = new Padding(2, 0, 0, 0);
            this.lblOptimize.Size = new Size(0x4e, 0x17);
            this.lblOptimize.Text = "/optimize+";
            this.lblOptimize.MouseHover += new EventHandler(this.lblOptimize_MouseHover);
            this.lblOptimize.Paint += new PaintEventHandler(this.lblOptimize_Paint);
            this.lblOptimize.MouseEnter += new EventHandler(this.lblOptimize_MouseEnter);
            this.lblOptimize.MouseLeave += new EventHandler(this.lblOptimize_MouseLeave);
            this.lblOptimize.MouseDown += new MouseEventHandler(this.lblOptimize_MouseDown);
            this.lblOptimize.Click += new EventHandler(this.lblOptimize_Click);
            this.tsOutput.GripStyle = ToolStripGripStyle.Hidden;
            this.tsOutput.Items.AddRange(new ToolStripItem[] { this.btnArrange, this.btnResults, this.btnLambda, this.btnSql, this.btnIL, this.btnActivateAutocompletion, this.btnAnalyze, this.btnExport, this.btnFormat });
            this.tsOutput.Location = new Point(0, 0);
            this.tsOutput.Name = "tsOutput";
            this.tsOutput.Padding = new Padding(0, 0, 1, 2);
            this.tsOutput.RenderMode = ToolStripRenderMode.System;
            this.tsOutput.Size = new Size(0x2cb, 0x1a);
            this.tsOutput.TabIndex = 0;
            this.tsOutput.Text = "toolStrip1";
            this.tsOutput.MouseEnter += new EventHandler(this.tsOutput_MouseEnter);
            this.btnArrange.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.btnArrange.DropDownItems.AddRange(new ToolStripItem[] { this.miHideResults, this.miUndock, this.miArrangeVertical, this.toolStripMenuItem3, this.miScrollStart, this.miScrollEnd, this.miAutoScroll, this.toolStripMenuItem4, this.miKeyboardShortcuts });
            this.btnArrange.Image = Resources.DropArrow;
            this.btnArrange.ImageScaling = ToolStripItemImageScaling.None;
            this.btnArrange.Margin = new Padding(0, 1, 3, 2);
            this.btnArrange.Name = "btnArrange";
            this.btnArrange.Padding = new Padding(0, 3, 0, 2);
            this.btnArrange.ShowDropDownArrow = false;
            this.btnArrange.Size = new Size(0x12, 0x15);
            this.btnArrange.DropDownOpening += new EventHandler(this.btnArrange_DropDownOpening);
            this.miHideResults.Name = "miHideResults";
            this.miHideResults.ShortcutKeyDisplayString = "Ctrl+R";
            this.miHideResults.Size = new Size(0x142, 0x18);
            this.miHideResults.Text = "Hide Results Panel";
            this.miHideResults.Click += new EventHandler(this.miHideResults_Click);
            this.miUndock.Name = "miUndock";
            this.miUndock.ShortcutKeyDisplayString = "F8";
            this.miUndock.Size = new Size(0x142, 0x18);
            this.miUndock.Text = "Undock Panel into Second Monitor";
            this.miUndock.Click += new EventHandler(this.miUndock_Click);
            this.miArrangeVertical.Name = "miArrangeVertical";
            this.miArrangeVertical.ShortcutKeyDisplayString = "Ctrl+F8";
            this.miArrangeVertical.Size = new Size(0x142, 0x18);
            this.miArrangeVertical.Text = "Arrange Panel Vertically";
            this.miArrangeVertical.Click += new EventHandler(this.miArrangeVertical_Click);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new Size(0x13f, 6);
            this.miScrollStart.Name = "miScrollStart";
            this.miScrollStart.ShortcutKeyDisplayString = "Alt+Home";
            this.miScrollStart.Size = new Size(0x142, 0x18);
            this.miScrollStart.Text = "Scroll to Start";
            this.miScrollStart.Click += new EventHandler(this.miScrollStart_Click);
            this.miScrollEnd.Name = "miScrollEnd";
            this.miScrollEnd.ShortcutKeyDisplayString = "Alt+End";
            this.miScrollEnd.Size = new Size(0x142, 0x18);
            this.miScrollEnd.Text = "Scroll to End";
            this.miScrollEnd.Click += new EventHandler(this.miScrollEnd_Click);
            this.miAutoScroll.Name = "miAutoScroll";
            this.miAutoScroll.ShortcutKeyDisplayString = "Ctrl+Shift+E";
            this.miAutoScroll.Size = new Size(0x142, 0x18);
            this.miAutoScroll.Text = "Auto-Scroll Results to End";
            this.miAutoScroll.Click += new EventHandler(this.miAutoScroll_Click);
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new Size(0x13f, 6);
            this.miKeyboardShortcuts.Name = "miKeyboardShortcuts";
            this.miKeyboardShortcuts.Size = new Size(0x142, 0x18);
            this.miKeyboardShortcuts.Text = "See more keyboard shortcuts...";
            this.miKeyboardShortcuts.Click += new EventHandler(this.miKeyboardShortcuts_Click);
            this.btnResults.Checked = true;
            this.btnResults.CheckState = CheckState.Checked;
            this.btnResults.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnResults.Image = (Image) manager.GetObject("btnResults.Image");
            this.btnResults.ImageTransparentColor = Color.Magenta;
            this.btnResults.Margin = new Padding(0, 0, 0, 1);
            this.btnResults.Name = "btnResults";
            this.btnResults.Size = new Size(0x38, 0x17);
            this.btnResults.Text = "&Results";
            this.btnResults.Click += new EventHandler(this.btnResults_Click);
            this.btnLambda.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnLambda.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.btnLambda.Image = (Image) manager.GetObject("btnLambda.Image");
            this.btnLambda.ImageTransparentColor = Color.Magenta;
            this.btnLambda.Margin = new Padding(0, 0, 0, 1);
            this.btnLambda.Name = "btnLambda";
            this.btnLambda.Size = new Size(0x1b, 0x17);
            this.btnLambda.Text = " λ ";
            this.btnLambda.Click += new EventHandler(this.btnLambda_Click);
            this.btnSql.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnSql.Image = (Image) manager.GetObject("btnSql.Image");
            this.btnSql.ImageTransparentColor = Color.Magenta;
            this.btnSql.Margin = new Padding(0, 0, 0, 1);
            this.btnSql.Name = "btnSql";
            this.btnSql.Size = new Size(0x26, 0x17);
            this.btnSql.Text = "&SQL";
            this.btnSql.Click += new EventHandler(this.btnSql_Click);
            this.btnIL.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnIL.Image = (Image) manager.GetObject("btnIL.Image");
            this.btnIL.ImageTransparentColor = Color.Magenta;
            this.btnIL.Margin = new Padding(0, 0, 0, 1);
            this.btnIL.Name = "btnIL";
            this.btnIL.Size = new Size(0x20, 0x17);
            this.btnIL.Text = " &IL ";
            this.btnIL.Click += new EventHandler(this.btnIL_Click);
            this.btnActivateAutocompletion.Alignment = ToolStripItemAlignment.Right;
            this.btnActivateAutocompletion.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnActivateAutocompletion.ForeColor = Color.Blue;
            this.btnActivateAutocompletion.Image = (Image) manager.GetObject("btnActivateAutocompletion.Image");
            this.btnActivateAutocompletion.ImageTransparentColor = Color.Magenta;
            this.btnActivateAutocompletion.Margin = new Padding(5, 0, 0, 0);
            this.btnActivateAutocompletion.Name = "btnActivateAutocompletion";
            this.btnActivateAutocompletion.Size = new Size(0xa4, 0x18);
            this.btnActivateAutocompletion.Text = "Activate Autocompletion";
            this.btnActivateAutocompletion.Click += new EventHandler(this.btnActivateAutocompletion_Click);
            this.btnAnalyze.Alignment = ToolStripItemAlignment.Right;
            this.btnAnalyze.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnAnalyze.DropDownItems.AddRange(new ToolStripItem[] { this.miOpenSQLQueryNewTab, this.miOpenInSSMS });
            this.btnAnalyze.Image = (Image) manager.GetObject("btnAnalyze.Image");
            this.btnAnalyze.ImageTransparentColor = Color.Magenta;
            this.btnAnalyze.Margin = new Padding(2, 0, 0, 0);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new Size(0x62, 0x18);
            this.btnAnalyze.Text = "A&nalyze SQL";
            this.miOpenSQLQueryNewTab.Image = Resources.New;
            this.miOpenSQLQueryNewTab.Name = "miOpenSQLQueryNewTab";
            this.miOpenSQLQueryNewTab.Size = new Size(0x11d, 0x18);
            this.miOpenSQLQueryNewTab.Text = "Open as SQL Query in New Tab";
            this.miOpenSQLQueryNewTab.Click += new EventHandler(this.miOpenSQLQueryNewTab_Click);
            this.miOpenInSSMS.Image = Resources.SSMS;
            this.miOpenInSSMS.Name = "miOpenInSSMS";
            this.miOpenInSSMS.Size = new Size(0x11d, 0x18);
            this.miOpenInSSMS.Text = "Open in SQL Management Studio";
            this.miOpenInSSMS.Click += new EventHandler(this.miOpenInSSMS_Click);
            this.btnExport.Alignment = ToolStripItemAlignment.Right;
            this.btnExport.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnExport.DropDownItems.AddRange(new ToolStripItem[] { this.btnExportExcelNoFormat, this.btnExportExcel, this.toolStripSeparator1, this.btnExportWordNoFormat, this.btnExportWord, this.toolStripMenuItem1, this.btnExportHtml });
            this.btnExport.ImageTransparentColor = Color.Magenta;
            this.btnExport.Margin = new Padding(2, 0, 0, 0);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new Size(0x3d, 0x18);
            this.btnExport.Text = "Ex&port";
            this.btnExportExcelNoFormat.Image = Resources.Excel;
            this.btnExportExcelNoFormat.Name = "btnExportExcelNoFormat";
            this.btnExportExcelNoFormat.Size = new Size(0x115, 0x18);
            this.btnExportExcelNoFormat.Text = "Export to Excel";
            this.btnExportExcelNoFormat.Click += new EventHandler(this.btnExportExcelNoFormat_Click);
            this.btnExportExcel.Image = Resources.Excel;
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new Size(0x115, 0x18);
            this.btnExportExcel.Text = "Export to Excel With Formatting";
            this.btnExportExcel.Click += new EventHandler(this.btnExportExcel_Click);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new Size(0x112, 6);
            this.btnExportWordNoFormat.Image = Resources.Word;
            this.btnExportWordNoFormat.Name = "btnExportWordNoFormat";
            this.btnExportWordNoFormat.Size = new Size(0x115, 0x18);
            this.btnExportWordNoFormat.Text = "Export to Word";
            this.btnExportWordNoFormat.Click += new EventHandler(this.btnExportWordNoFormat_Click);
            this.btnExportWord.Image = Resources.Word;
            this.btnExportWord.Name = "btnExportWord";
            this.btnExportWord.Size = new Size(0x115, 0x18);
            this.btnExportWord.Text = "Export to Word With Formatting";
            this.btnExportWord.Click += new EventHandler(this.btnExportWord_Click);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new Size(0x112, 6);
            this.btnExportHtml.Name = "btnExportHtml";
            this.btnExportHtml.Size = new Size(0x115, 0x18);
            this.btnExportHtml.Text = "Export to HTML";
            this.btnExportHtml.Click += new EventHandler(this.btnExportHtml_Click);
            this.btnFormat.Alignment = ToolStripItemAlignment.Right;
            this.btnFormat.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnFormat.DropDownItems.AddRange(new ToolStripItem[] { this.btn1NestingLevel, this.btn2NestingLevels, this.btn3NestingLevels, this.btnAllNestingLevels, this.toolStripMenuItem2, this.btnResultFormattingPreferences });
            this.btnFormat.ImageTransparentColor = Color.Magenta;
            this.btnFormat.Margin = new Padding(2, 0, 0, 0);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new Size(0x42, 0x18);
            this.btnFormat.Text = "Format";
            this.btn1NestingLevel.Name = "btn1NestingLevel";
            this.btn1NestingLevel.ShortcutKeyDisplayString = "Alt+1";
            this.btn1NestingLevel.Size = new Size(0x126, 0x18);
            this.btn1NestingLevel.Text = "Collapse to 1 Nesting Level";
            this.btn1NestingLevel.Click += new EventHandler(this.btn1NestingLevel_Click);
            this.btn2NestingLevels.Name = "btn2NestingLevels";
            this.btn2NestingLevels.ShortcutKeyDisplayString = "Alt+2";
            this.btn2NestingLevels.Size = new Size(0x126, 0x18);
            this.btn2NestingLevels.Text = "Collapse to 2 Nesting Levels";
            this.btn2NestingLevels.Click += new EventHandler(this.btn2NestingLevels_Click);
            this.btn3NestingLevels.Name = "btn3NestingLevels";
            this.btn3NestingLevels.ShortcutKeyDisplayString = "Alt+3";
            this.btn3NestingLevels.Size = new Size(0x126, 0x18);
            this.btn3NestingLevels.Text = "Collapse to 3 Nesting Levels";
            this.btn3NestingLevels.Click += new EventHandler(this.btn3NestingLevels_Click);
            this.btnAllNestingLevels.Name = "btnAllNestingLevels";
            this.btnAllNestingLevels.ShortcutKeyDisplayString = "Alt+0";
            this.btnAllNestingLevels.Size = new Size(0x126, 0x18);
            this.btnAllNestingLevels.Text = "Show All Nesting Levels";
            this.btnAllNestingLevels.Click += new EventHandler(this.btnAllNestingLevels_Click);
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new Size(0x123, 6);
            this.btnResultFormattingPreferences.Name = "btnResultFormattingPreferences";
            this.btnResultFormattingPreferences.Size = new Size(0x126, 0x18);
            this.btnResultFormattingPreferences.Text = "Result Formatting Preferences...";
            this.btnResultFormattingPreferences.Click += new EventHandler(this.btnResultFormattingPreferences_Click);
            this.lblDb.Anchor = AnchorStyles.Left;
            this.lblDb.AutoSize = true;
            this.lblDb.Location = new Point(0x151, 6);
            this.lblDb.Margin = new Padding(0);
            this.lblDb.Name = "lblDb";
            this.lblDb.Size = new Size(60, 15);
            this.lblDb.TabIndex = 4;
            this.lblDb.Text = "&Database";
            this.lblDb.TextAlign = ContentAlignment.MiddleLeft;
            this.cboLanguage.Anchor = AnchorStyles.Left;
            this.cboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Items.AddRange(new object[] { "C# Expression", "C# Statement(s)", "C# Program", "VB Expression", "VB Statement(s)", "VB Program", "SQL", "ESQL", "F# Expression", "F# Program" });
            this.cboLanguage.Location = new Point(0xc6, 3);
            this.cboLanguage.Margin = new Padding(2, 2, 12, 2);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new Size(0x69, 0x15);
            this.cboLanguage.TabIndex = 3;
            this.cboLanguage.TabStop = false;
            this.cboLanguage.SelectionChangeCommitted += new EventHandler(this.cboLanguage_SelectionChangeCommitted);
            this.cboLanguage.Leave += new EventHandler(this.cboType_SelectedIndexChanged);
            this.cboLanguage.Enter += new EventHandler(this.cboType_Enter);
            this.cboLanguage.DropDownClosed += new EventHandler(this.cboType_DropDownClosed);
            this.cboDb.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this.cboDb.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDb.FormattingEnabled = true;
            this.cboDb.Location = new Point(0x18f, 3);
            this.cboDb.Margin = new Padding(2);
            this.cboDb.Name = "cboDb";
            this.cboDb.Size = new Size(0xbb, 0x15);
            this.cboDb.TabIndex = 5;
            this.cboDb.TabStop = false;
            this.cboDb.Leave += new EventHandler(this.cboDb_SelectedIndexChanged);
            this.cboDb.Enter += new EventHandler(this.cboDb_Enter);
            this.cboDb.DropDownClosed += new EventHandler(this.cboDb_DropDownClosed);
            this.cboDb.DropDown += new EventHandler(this.cboDb_DropDown);
            this.lblType.Anchor = AnchorStyles.Left;
            this.lblType.AutoSize = true;
            this.lblType.Location = new Point(0x7f, 6);
            this.lblType.Margin = new Padding(2, 0, 0, 0);
            this.lblType.Name = "lblType";
            this.lblType.Padding = new Padding(6, 0, 0, 0);
            this.lblType.Size = new Size(0x45, 15);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "&Language";
            this.lblType.TextAlign = ContentAlignment.MiddleLeft;
            this.panTopControls.AutoSize = true;
            this.panTopControls.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panTopControls.ColumnCount = 10;
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panTopControls.ColumnStyles.Add(new ColumnStyle());
            this.panTopControls.Controls.Add(this.btnExecute, 0, 0);
            this.panTopControls.Controls.Add(this.btnCancel, 1, 0);
            this.panTopControls.Controls.Add(this.lblDb, 7, 0);
            this.panTopControls.Controls.Add(this.lblType, 4, 0);
            this.panTopControls.Controls.Add(this.cboLanguage, 5, 0);
            this.panTopControls.Controls.Add(this.llDbUseCurrent, 9, 0);
            this.panTopControls.Controls.Add(this.cboDb, 8, 0);
            this.panTopControls.Controls.Add(this.lblSyncDb, 6, 0);
            this.panTopControls.Controls.Add(this.btnText, 2, 0);
            this.panTopControls.Controls.Add(this.btnGrids, 3, 0);
            this.panTopControls.Dock = DockStyle.Top;
            this.panTopControls.Location = new Point(0, 0);
            this.panTopControls.Margin = new Padding(2);
            this.panTopControls.Name = "panTopControls";
            this.panTopControls.Padding = new Padding(0, 0, 15, 1);
            this.panTopControls.RowCount = 1;
            this.panTopControls.RowStyles.Add(new RowStyle());
            this.panTopControls.Size = new Size(0x2a3, 0x1d);
            this.panTopControls.TabIndex = 8;
            this.btnExecute.Image = Resources.Execute;
            this.btnExecute.Location = new Point(0, 2);
            this.btnExecute.Margin = new Padding(0, 2, 1, 2);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new Size(30, 0x17);
            this.btnExecute.TabIndex = 0;
            this.btnExecute.TabStop = false;
            this.toolTip.SetToolTip(this.btnExecute, "Execute (F5)");
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new EventHandler(this.btnExecute_Click);
            this.btnCancel.Enabled = false;
            this.btnCancel.Image = Resources.Cancel;
            this.btnCancel.Location = new Point(0x21, 2);
            this.btnCancel.Margin = new Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(30, 0x17);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.TabStop = false;
            this.toolTip.SetToolTip(this.btnCancel, "Cancel (Shift+F5)");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.llDbUseCurrent.Anchor = AnchorStyles.Left;
            this.llDbUseCurrent.AutoSize = true;
            this.llDbUseCurrent.Cursor = Cursors.Hand;
            this.llDbUseCurrent.ForeColor = Color.Blue;
            this.llDbUseCurrent.Location = new Point(590, 6);
            this.llDbUseCurrent.Margin = new Padding(2, 0, 0, 0);
            this.llDbUseCurrent.Name = "llDbUseCurrent";
            this.llDbUseCurrent.Size = new Size(70, 15);
            this.llDbUseCurrent.TabIndex = 6;
            this.llDbUseCurrent.TabStop = true;
            this.llDbUseCurrent.Text = "Use current";
            this.llDbUseCurrent.TextAlign = ContentAlignment.MiddleLeft;
            this.llDbUseCurrent.LinkClicked += new EventHandler(this.llDbUseCurrent_LinkClicked);
            this.lblSyncDb.Anchor = AnchorStyles.Left;
            this.lblSyncDb.AutoSize = true;
            this.lblSyncDb.Cursor = Cursors.Hand;
            this.lblSyncDb.Font = new Font("Wingdings", 10.2f, FontStyle.Regular, GraphicsUnit.Point, 2);
            this.lblSyncDb.Location = new Point(0x13b, 6);
            this.lblSyncDb.Margin = new Padding(0, 2, 0, 0);
            this.lblSyncDb.Name = "lblSyncDb";
            this.lblSyncDb.Size = new Size(0x16, 0x11);
            this.lblSyncDb.TabIndex = 7;
            this.lblSyncDb.Text = "\x00ef";
            this.lblSyncDb.Click += new EventHandler(this.lblSyncDb_Click);
            this.btnText.Checked = true;
            this.btnText.Image = Resources.TextResults;
            this.btnText.Location = new Point(0x4d, 3);
            this.btnText.Margin = new Padding(12, 3, 0, 2);
            this.btnText.Name = "btnText";
            this.btnText.NoImageScale = false;
            this.btnText.Size = new Size(0x17, 0x17);
            this.btnText.TabIndex = 8;
            this.btnText.TabStop = false;
            this.toolTip.SetToolTip(this.btnText, "Results to Rich Text (Ctrl+Shift+T)");
            this.btnText.ToolTipText = "";
            this.btnText.Click += new EventHandler(this.btnText_Click);
            this.btnGrids.Checked = false;
            this.btnGrids.Image = Resources.GridResults;
            this.btnGrids.Location = new Point(100, 3);
            this.btnGrids.Margin = new Padding(0, 3, 2, 2);
            this.btnGrids.Name = "btnGrids";
            this.btnGrids.NoImageScale = false;
            this.btnGrids.Size = new Size(0x17, 0x17);
            this.btnGrids.TabIndex = 9;
            this.btnGrids.TabStop = false;
            this.toolTip.SetToolTip(this.btnGrids, "Results to Data Grids (Ctrl+Shift+G)");
            this.btnGrids.ToolTipText = "";
            this.btnGrids.Click += new EventHandler(this.btnGrids_Click);
            this.panTop.AutoSize = true;
            this.panTop.Controls.Add(this.panTopControls);
            this.panTop.Controls.Add(this.panCloseButton);
            this.panTop.Dock = DockStyle.Top;
            this.panTop.Location = new Point(0, 0);
            this.panTop.Name = "panTop";
            this.panTop.Size = new Size(0x2cf, 0x1d);
            this.panTop.TabIndex = 4;
            this.panCloseButton.Controls.Add(this.btnPin);
            this.panCloseButton.Controls.Add(this.btnClose);
            this.panCloseButton.Dock = DockStyle.Right;
            this.panCloseButton.Location = new Point(0x2a3, 0);
            this.panCloseButton.Margin = new Padding(2);
            this.panCloseButton.Name = "panCloseButton";
            this.panCloseButton.Padding = new Padding(0, 3, 2, 4);
            this.panCloseButton.Size = new Size(0x2c, 0x1d);
            this.panCloseButton.TabIndex = 9;
            this.btnPin.Checked = false;
            this.btnPin.Dock = DockStyle.Left;
            this.btnPin.Glyph = ButtonGlyph.Pin;
            this.btnPin.Location = new Point(0, 3);
            this.btnPin.Margin = new Padding(2);
            this.btnPin.Name = "btnPin";
            this.btnPin.NoImageScale = false;
            this.btnPin.Size = new Size(20, 0x16);
            this.btnPin.TabIndex = 1;
            this.toolTip.SetToolTip(this.btnPin, "Keep query open");
            this.btnPin.ToolTipText = "";
            this.btnPin.Click += new EventHandler(this.btnPin_Click);
            this.btnClose.Checked = false;
            this.btnClose.Dock = DockStyle.Right;
            this.btnClose.Location = new Point(0x16, 3);
            this.btnClose.Margin = new Padding(2);
            this.btnClose.Name = "btnClose";
            this.btnClose.NoImageScale = false;
            this.btnClose.Size = new Size(20, 0x16);
            this.btnClose.TabIndex = 0;
            this.toolTip.SetToolTip(this.btnClose, "Close query (Ctrl+F4)");
            this.btnClose.ToolTipText = "";
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.panMain.Controls.Add(this.splitContainer);
            this.panMain.Dock = DockStyle.Fill;
            this.panMain.Location = new Point(0, 0x1d);
            this.panMain.Name = "panMain";
            this.panMain.Size = new Size(0x2cf, 0x192);
            this.panMain.TabIndex = 3;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.Transparent;
            base.Controls.Add(this.panMain);
            base.Controls.Add(this.panTop);
            base.Name = "QueryControl";
            base.Size = new Size(0x2cf, 0x1af);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.panEditor.ResumeLayout(false);
            this.panError.ResumeLayout(false);
            this.panError.PerformLayout();
            this.panBottom.ResumeLayout(false);
            this.panBottom.PerformLayout();
            this.panOutput.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tsOutput.ResumeLayout(false);
            this.tsOutput.PerformLayout();
            this.panTopControls.ResumeLayout(false);
            this.panTopControls.PerformLayout();
            this.panTop.ResumeLayout(false);
            this.panTop.PerformLayout();
            this.panCloseButton.ResumeLayout(false);
            this.panMain.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        internal void InsertSnippet(bool surroundWith)
        {
            if (this._editor.get_Document().get_Language() is CSharpSyntaxLanguage)
            {
                this._editor.InsertSnippet(surroundWith);
            }
        }

        internal void InsertText(string text, int postCaretAdjustment)
        {
            this._editor.get_Document().InsertText(0x1d, this._editor.get_Caret().get_Offset(), text);
            try
            {
                Caret caret1 = this._editor.get_Caret();
                caret1.set_Offset(caret1.get_Offset() + postCaretAdjustment);
            }
            catch
            {
            }
        }

        public bool IsAutoScrollingResultsFromQuery()
        {
            return (this._autoScrollResultsFromQuery == true);
        }

        internal bool IsEditorFocused()
        {
            return this._editor.ContainsFocus;
        }

        internal bool IsInDataGridMode()
        {
            return (this.btnResults.Text == "&Output");
        }

        internal bool IsMouseInPlugIn()
        {
            if (!this.IsPluginSelected())
            {
                return false;
            }
            return this.GetWinManagerTargetBounds().Contains(Control.MousePosition);
        }

        internal bool IsPluginSelected()
        {
            return this._pluginWinButtons.Any<ToolStripButton>(b => b.Checked);
        }

        private bool IsQueryLanguageTrackable(QueryLanguage language)
        {
            return ((((language == QueryLanguage.Statements) || (language == QueryLanguage.Program)) || (language == QueryLanguage.VBStatements)) || (language == QueryLanguage.VBProgram));
        }

        internal void JumpToExecutingLine()
        {
            Action a = null;
            if (this._query.IsRunning)
            {
                if (!this.IsQueryLanguageTrackable(this._executingQueryLanguage))
                {
                    MessageBox.Show("Query is not in Statements or Program mode.", "Jump to Execution Point", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                if (this._executingQueryOptimized && !_warnedAboutOptimizationTracking)
                {
                    _warnedAboutOptimizationTracking = true;
                    MessageBox.Show("Execution tracking is only approximate while compiler optimizations are enabled. Click the button on the status bar and re-run query to disable optimizations.", "Jump to Execution Point", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                this._editor.set_CurrentLineHighlightingVisible(false);
                ExecutionTrackInfo mainThreadPosition = this._query.GetMainThreadPosition(true);
                if (((mainThreadPosition != null) && (mainThreadPosition.MainThreadStack != null)) && (mainThreadPosition.MainThreadStack.Length != 0))
                {
                    int row = mainThreadPosition.MainThreadStack[0].Row - 1;
                    int column = mainThreadPosition.MainThreadStack[0].Column - 1;
                    try
                    {
                        int num3;
                        this._editor.get_SelectedView().get_Selection().set_StartOffset(num3 = this.GetOffsetFromRowCol(row, column));
                        this._editor.get_SelectedView().get_Selection().set_EndOffset(num3);
                        if (a == null)
                        {
                            a = () => this._editor.set_CurrentLineHighlightingVisible(true);
                        }
                        Program.RunOnWinFormsTimer(a, 50);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void KillIEComExceptionTimer()
        {
            if (this._ieComExceptionTimer != null)
            {
                this._ieComExceptionTimer.Dispose();
                this._ieComExceptionTimer = null;
            }
        }

        private void lblOptimize_Click(object sender, EventArgs e)
        {
            MainForm.Instance.OptimizeQueries = !MainForm.Instance.OptimizeQueries;
            if (this._optimizeTipShown)
            {
                this.ShowOptimizeTip();
            }
        }

        private void lblOptimize_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                using (OptionsForm form = new OptionsForm(1))
                {
                    form.ShowDialog(MainForm.Instance);
                }
            }
        }

        private void lblOptimize_MouseEnter(object sender, EventArgs e)
        {
        }

        private void lblOptimize_MouseHover(object sender, EventArgs e)
        {
            this.ShowOptimizeTip();
        }

        private void lblOptimize_MouseLeave(object sender, EventArgs e)
        {
            if (this._optimizeTipShown)
            {
                this.toolTip.Hide(this.statusStrip);
                this._optimizeTipShown = false;
            }
        }

        private void lblOptimize_Paint(object sender, PaintEventArgs e)
        {
        }

        private void lblSyncDb_Click(object sender, EventArgs e)
        {
            if (this._query.Repository != null)
            {
                this._schemaTree.RegisterRepository(this._query, true, false);
            }
        }

        internal void ListMembers()
        {
            if (this._editor.get_Document().get_Language() is CSharpSyntaxLanguage)
            {
                ((ICustomSyntaxLanguage) this._editor.get_Document().get_Language()).ShowIntelliPromptMemberList(this._editor, false, false);
            }
        }

        internal void ListMembersWithoutExtensions()
        {
            if (this._editor.get_Document().get_Language() is CSharpSyntaxLanguage)
            {
                ((ICustomSyntaxLanguage) this._editor.get_Document().get_Language()).ShowIntelliPromptMemberList(this._editor, false, false, null, true);
            }
        }

        internal void ListTables()
        {
            if (this._editor.get_Document().get_Language() is CSharpSyntaxLanguage)
            {
                ((ICustomSyntaxLanguage) this._editor.get_Document().get_Language()).ShowIntelliPromptMemberList(this._editor, true, false);
            }
        }

        private void llDbUseCurrent_LinkClicked(object sender, EventArgs e)
        {
            this.UseCurrentDb(false);
        }

        internal void ManageNugetRefs()
        {
        }

        private void miArrangeVertical_Click(object sender, EventArgs e)
        {
            MainForm.Instance.ToggleVerticalResults();
        }

        private void miAutoScroll_Click(object sender, EventArgs e)
        {
            MainForm.Instance.ToggleAutoScrollResults(true);
        }

        private void miHideResults_Click(object sender, EventArgs e)
        {
            this.ToggleResultsCollapse();
        }

        private void miKeyboardShortcuts_Click(object sender, EventArgs e)
        {
            MainForm.Instance.ShowKeyboardShortcuts();
        }

        private void miOpenInSSMS_Click(object sender, EventArgs e)
        {
            if (this._query.Repository != null)
            {
                string str2;
                string sSMSLocation = this.GetSSMSLocation();
                if (sSMSLocation == null)
                {
                    str2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\ssms.exe");
                    if (File.Exists(str2))
                    {
                        sSMSLocation = str2;
                    }
                }
                if (sSMSLocation == null)
                {
                    str2 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @" (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\ssms.exe";
                    if (File.Exists(str2))
                    {
                        sSMSLocation = str2;
                    }
                }
                if (sSMSLocation == null)
                {
                    MessageBox.Show("Cannot locate SQL Server Management Studio.");
                }
                else
                {
                    string path = Path.Combine(Path.GetTempPath(), "LINQPad");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    FileInfo[] files = new DirectoryInfo(path).GetFiles();
                    int index = 0;
                    while (true)
                    {
                        if (index >= files.Length)
                        {
                            break;
                        }
                        FileInfo info = files[index];
                        if (info.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-7.0))
                        {
                            try
                            {
                                info.Delete();
                            }
                            catch
                            {
                            }
                        }
                        index++;
                    }
                    string str4 = Path.Combine(path, Path.ChangeExtension(Path.GetRandomFileName(), ".sql"));
                    File.WriteAllText(str4, this.txtSQL.Text);
                    Repository repository = this._query.Repository;
                    string arguments = "-nosplash \"" + str4 + "\"";
                    if (!(!repository.IsSqlServer || repository.AttachFile))
                    {
                        arguments = "-S " + repository.Server + " -d " + repository.Database + (repository.SqlSecurity ? (" -U " + repository.UserName + " -P " + repository.Password) : " -E") + " " + arguments;
                    }
                    Process.Start(sSMSLocation, arguments);
                }
            }
        }

        private void miOpenSQLQueryNewTab_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddSqlQueryPage(this.txtSQL.Text);
        }

        private void miScrollEnd_Click(object sender, EventArgs e)
        {
            this.ScrollResults(VerticalScrollAmount.Document, true);
        }

        private void miScrollStart_Click(object sender, EventArgs e)
        {
            this.ScrollResults(VerticalScrollAmount.Document, false);
        }

        private void miUndock_Click(object sender, EventArgs e)
        {
            MainForm.Instance.ToggleDockResults();
        }

        internal void NavigateResultPanel(bool next)
        {
            int index = this.tsOutput.Items.IndexOf(this.btnIL);
            ToolStripButton button = this.tsOutput.Items.OfType<ToolStripButton>().FirstOrDefault<ToolStripButton>(b => b.Checked);
            if (button != null)
            {
                int num2 = this.tsOutput.Items.IndexOf(button);
                if (num2 <= index)
                {
                    num2 += next ? 1 : -1;
                    if (num2 > index)
                    {
                        num2 = 1;
                    }
                    else if (num2 < 1)
                    {
                        num2 = index;
                    }
                    this.SelectOutputPanel(this.tsOutput.Items[num2] as ToolStripButton, false);
                }
            }
        }

        internal void NotifyWindowRestoreFromMaximize()
        {
            if (this.panOutput.BackColor == Program.TransparencyKey)
            {
                this.panOutput.BackColor = Color.White;
                this.panOutput.Refresh();
                this.UpdatePluginTransparency();
            }
        }

        internal void NotifyWindowRestoreFromMinimize()
        {
            if (this.IsPluginSelected() && this.AreResultsVisible())
            {
                this.UpdateOutputVisibility();
            }
        }

        internal void OnAllPluginsRemoved()
        {
            this.ResetPluginManager(false);
        }

        internal void OnFormActivated()
        {
            if (this.splitContainer.ContainsFocus)
            {
                Control activeControl = GetActiveControl();
                if ((activeControl is SplitContainer) || ((activeControl is DataResultsWebBrowser) && !this.btnResults.Checked))
                {
                    this._editor.Focus();
                }
            }
        }

        internal void OnIdle()
        {
        }

        internal void OnInfoMessageChanged(PluginControl pic, string value)
        {
            if (this.GetSelectedPluginControl() == pic)
            {
                this.lblMiscStatus.Text = value;
            }
        }

        internal void OnNewlySelectedPage()
        {
            if (this.IsPluginSelected() && this.AreResultsVisible())
            {
                this.panOutput.BackColor = Color.White;
                this.panOutput.Update();
                this._pluginWinManager.Show(this.GetSelectedPluginControl(), false);
            }
            this.RequestWinManagerRelocation();
            this.UpdatePluginTransparency();
            this.SetRegion();
            this.FocusQuery();
        }

        internal void OnNoLongerSelectedPage()
        {
            if ((this._editor != null) && (this._editor.get_IntelliPrompt() != null))
            {
                if (this._editor.get_IntelliPrompt().get_MemberList() != null)
                {
                    this._editor.get_IntelliPrompt().get_MemberList().Abort();
                }
                if (this._editor.get_IntelliPrompt().get_ParameterInfo() != null)
                {
                    this._editor.get_IntelliPrompt().get_ParameterInfo().Hide();
                }
                if (this._editor.get_IntelliPrompt().get_QuickInfo() != null)
                {
                    this._editor.get_IntelliPrompt().get_QuickInfo().Hide();
                }
            }
            if (this.IsPluginSelected())
            {
                this.panOutput.BackColor = Color.White;
                this.panOutput.Update();
            }
            this._pluginWinManager.Hide();
            this.ClearRegion();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            this.RefreshOptimizeQuery();
            base.OnParentChanged(e);
        }

        internal void OnPluginAdded(PluginControl c)
        {
            if (this._gotPluginsReadyMessage || this._query.MessageLoopStartedWithoutForm)
            {
                this.CreatePluginWinButton(c, true, true);
                this.UpdateOutputToolStripLayout();
            }
        }

        internal void OnPluginRemoved(PluginControl plugin)
        {
            this.RemovePluginWinButton(plugin);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.UpdateUseCurrentDbVisibility();
            this.UpdateOutputToolStripLayout();
        }

        private void OpenExcel(string target)
        {
            ExcelHelper.OpenInExcel(target, false);
        }

        public bool OpenFileQuery(string fullPath)
        {
            fullPath = Encoding.UTF8.GetString(Convert.FromBase64String(fullPath));
            MainForm.Instance.ActivateMyQueries();
            MainForm.Instance.OpenQuery(fullPath, false);
            return false;
        }

        public bool OpenSample(string query)
        {
            string id = Encoding.UTF8.GetString(Convert.FromBase64String(query));
            MainForm.Instance.OpenSampleQuery(id);
            return false;
        }

        private void OpenWord(string target)
        {
            Type typeFromProgID;
            try
            {
                try
                {
                    typeFromProgID = Type.GetTypeFromProgID("Word.Application", true);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
            }
            catch (COMException exception2)
            {
                Log.Write(exception2, "Export to Word");
                MessageBox.Show("Cannot open Word - is it installed?", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            object obj2 = Activator.CreateInstance(typeFromProgID);
            Type type2 = obj2.GetType();
            object obj3 = type2.InvokeMember("Documents", BindingFlags.GetProperty, null, obj2, null);
            obj3.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, obj3, new object[] { target });
            type2.InvokeMember("Visible", BindingFlags.SetProperty, null, obj2, new object[] { true });
        }

        internal void OverrideAutoScrollFromQuery(bool p)
        {
            this._autoScrollResultsFromQuery = null;
        }

        private void panBottom_Layout(object sender, LayoutEventArgs e)
        {
        }

        private void panError_Layout(object sender, LayoutEventArgs e)
        {
            this.UpdateErrorHeight();
        }

        private void panOutput_Layout(object sender, LayoutEventArgs e)
        {
        }

        internal void PasteIntoEditor(string text)
        {
            this._editor.get_SelectedView().ReplaceSelectedText(20, text);
        }

        internal void PerformIncrementalSearch(bool up)
        {
            this.Editor.get_SelectedView().get_FindReplace().get_IncrementalSearch().PerformSearch(up);
        }

        internal void PropagateOptions()
        {
            this._editor.PropagateOptions();
        }

        internal void PullData(QueryChangedEventArgs changeInfo)
        {
            this.CheckIsMyExtensions();
            if (this._suppressPullCount <= 0)
            {
                bool flag = false;
                bool flag2 = false;
                using (this.SuppressPull())
                {
                    if (changeInfo.SourceChanged && (this._editor.Text != this._query.Source))
                    {
                        this._editor.Text = this._query.Source;
                        this._editor.set_IndicatorMarginVisible(false);
                        this._editor.get_Document().get_LineIndicators().Clear();
                    }
                    this._editor.IsOutliningEnabled = (this._query.QueryKind == QueryLanguage.Program) || (this._query.QueryKind == QueryLanguage.VBProgram);
                    this.btnPin.Checked = this._query.Pinned;
                    this.cboLanguage.SelectedIndex = this.QueryLanguageToIndex(this._query.QueryKind);
                    this.btnGrids.Checked = this._query.ToDataGrids;
                    this.btnText.Checked = !this._query.ToDataGrids;
                    bool flag3 = false;
                    if (changeInfo.ReferencesChanged && this._query.IsMyExtensions)
                    {
                        MyExtensions.UpdateAdditionalRefs(this._query);
                        this._docMan.ResetSharedReferences(MyExtensions.AdditionalRefs);
                    }
                    if (((this._lastQueryKind != this._query.QueryKind) || changeInfo.ReferencesChanged) || changeInfo.NamespacesChanged)
                    {
                        flag2 = (this._lastQueryKind != this._query.QueryKind) && (this._lastQueryKind >= QueryLanguage.FSharpExpression);
                        this._lastQueryKind = this._query.QueryKind;
                        this._docMan.ConfigureLanguage();
                        this._docMan.ConfigureResolver();
                        flag3 = true;
                    }
                    this.lblSyncDb.Visible = this._query.Repository != null;
                    if (changeInfo.DbChanged)
                    {
                        this.UpdateRepositoryItems(false);
                        this.UpdateFocusedRepository();
                    }
                    if ((!flag3 && !this._docMan.CheckForRepositoryChange()) && (flag || flag2))
                    {
                        this._docMan.ConfigureResolver();
                    }
                }
                this.UpdateOutputVisibility();
                if (base.Parent != null)
                {
                    this._schemaTree.UpdateSqlMode(this._query);
                }
            }
        }

        private void QueryCompiled(QueryCompilationEventArgs e)
        {
            Func<CompilerError, string> selector = null;
            Func<CompilerError, bool> predicate = null;
            CompilerError mainError;
            CompilerError[] mainErrors;
            if (!base.IsDisposed)
            {
                this._lastCompilation = e;
                IEnumerable<CompilerError> source = from er in e.Errors.Cast<CompilerError>()
                    where !er.IsWarning
                    select er;
                mainError = source.FirstOrDefault<CompilerError>();
                mainErrors = (from er in source.Take<CompilerError>(5)
                    where er.Line == mainError.Line
                    select er).ToArray<CompilerError>();
                if (mainErrors.Length > 0)
                {
                    if (selector == null)
                    {
                        selector = er => this.GetErrorText(er);
                    }
                    string msg = string.Join("\r\n\r\n", mainErrors.Select<CompilerError, string>(selector).Distinct<string>().ToArray<string>());
                    if (e.PartialSource)
                    {
                        msg = "Cannot execute text selection: " + msg;
                    }
                    this.DisplayError(msg);
                }
                if (!this._modifiedWhenRunning)
                {
                    foreach (CompilerError error in from ce in e.Errors.Cast<CompilerError>()
                        where ce.Line > 0
                        orderby ce.IsWarning
                        select ce)
                    {
                        this.HighlightErrorOrWarning(error.Line - 1, error.Column - 1, error.ErrorText, error.IsWarning, true, error == mainError);
                    }
                    if (predicate == null)
                    {
                        predicate = ce => (ce.Line == 0) && !mainErrors.Contains<CompilerError>(ce);
                    }
                    string message = string.Join("\n", (from ce in e.Errors.Cast<CompilerError>().Where<CompilerError>(predicate) select ce.ErrorText).ToArray<string>());
                    if (message.Length > 0)
                    {
                        this.HighlightErrorOrWarning(0, 0, message, true, true, false);
                    }
                }
                if (mainError != null)
                {
                    this.SetILContent("");
                }
                this._ilDirty = true;
                if (this.btnIL.Checked)
                {
                    this.UpdateILContent();
                }
                if (mainError != null)
                {
                    this.lblStatus.Text = "Error compiling query";
                }
                else if (this._compileOnly)
                {
                    this.lblStatus.Text = "Query compiled " + (e.Errors.HasErrors ? "with warnings" : "successfully");
                }
                else
                {
                    this.lblStatus.Text = "Executing";
                }
                if ((mainError != null) || this._compileOnly)
                {
                    this.lblElapsed.Visible = false;
                    if (!(((mainError == null) || this.AreResultsDetached()) || this.AreResultsCollapsed()))
                    {
                        this.ToggleResultsCollapse();
                    }
                    this.EnableControls();
                }
                else
                {
                    this.ShowResultsUponQueryStart();
                }
                if ((this._compileOnly && this._pendingReflection) && (mainError == null))
                {
                    this.ReflectILNow();
                }
                this._pendingReflection = false;
                if ((mainError == null) && this._query.IsMyExtensions)
                {
                    foreach (QueryControl control in MainForm.Instance.GetQueryControls())
                    {
                        if ((control != this) && (control.Query.QueryKind != QueryLanguage.SQL))
                        {
                            control.CheckAutocompletionCache();
                        }
                    }
                }
            }
        }

        private void QueryCompleted(QueryStatusEventArgs e)
        {
            Exception exception;
            if (e.ExecutionComplete)
            {
                this._gotQueryCompletionMessage = true;
                this._executionTrackingTimer.Stop();
                this.ClearExecutionTrackingIndicators();
                if (this._editor.get_Document().get_LineIndicators().get_Count() == 0)
                {
                    this._editor.set_IndicatorMarginVisible(false);
                }
            }
            this._refreshTimer.Interval = 1;
            try
            {
                this.ShowResultsUponQueryStart();
                if (!string.IsNullOrEmpty(e.StatusMessage))
                {
                    this.lblStatus.Text = e.StatusMessage;
                }
                if (!(string.IsNullOrEmpty(e.ErrorMessage) || this.panError.Visible))
                {
                    if (!e.IsInfo)
                    {
                        this.DisplayError(e.ErrorMessage);
                    }
                }
                else if ((e.ExecutionComplete && (e.ExecTime.TotalMilliseconds > 0.0)) && this.lblElapsed.Visible)
                {
                    TimeSpan execTime = e.ExecTime;
                    string str = execTime.Minutes.ToString("D2") + ":" + execTime.Seconds.ToString("D2") + "." + execTime.Milliseconds.ToString("D3");
                    if (execTime.Hours > 0)
                    {
                        str = ((int) execTime.TotalHours) + ":" + str;
                    }
                    this.lblExecTime.Text = "(" + str + ")";
                    this.lblExecTime.Visible = true;
                }
                if ((e.ErrorLine > 0) && !this._modifiedWhenRunning)
                {
                    this.HighlightErrorOrWarning(e.ErrorLine - 1, e.ErrorColumn - 1, e.ErrorMessage, e.IsWarning || e.IsInfo, false, !e.IsInfo);
                    if ((e.StackTraceLines != null) && (e.StackTraceColumns != null))
                    {
                        for (int i = 0; i < e.StackTraceLines.Length; i++)
                        {
                            this.HighlightStackTraceError(e.StackTraceLines[i] - 1, e.StackTraceColumns[i] - 1);
                        }
                    }
                }
            }
            catch (Exception exception1)
            {
                exception = exception1;
                Program.ProcessException(exception);
            }
            finally
            {
                try
                {
                    if (e.ExecutionComplete)
                    {
                        this.lblElapsed.Visible = false;
                    }
                    this.EnableControls();
                    if (e.ExecutionComplete && ((GC.GetTotalMemory(false) - this._memoryAtStart) > 0x2faf080L))
                    {
                        MainForm.Instance.TriggerGC(2);
                    }
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    Program.ProcessException(exception);
                }
            }
        }

        private int QueryLanguageToIndex(QueryLanguage language)
        {
            if (this._query.IsMyExtensions)
            {
                return 0;
            }
            int num2 = (int) language;
            if (language >= QueryLanguage.SQL)
            {
                num2 -= 2;
            }
            else if ((language == QueryLanguage.FSharpExpression) || (language == QueryLanguage.FSharpProgram))
            {
                num2 += 2;
            }
            return num2;
        }

        internal void QuickInfo()
        {
            if (this._editor.get_Document().get_Language() is CSharpSyntaxLanguage)
            {
                this._editor.get_Document().get_Language().ShowIntelliPromptQuickInfo(this._editor);
            }
        }

        internal void ReflectIL()
        {
            if ((this._query.QueryKind != QueryLanguage.SQL) && (this._query.QueryKind != QueryLanguage.ESQL))
            {
                if (!(((this._query.ExecutionProgress != ExecutionProgress.Executing) && (this._query.ExecutionProgress != ExecutionProgress.Async)) && this._query.RequiresRecompilation))
                {
                    this.ReflectILNow();
                }
                else
                {
                    this._pendingReflection = true;
                    if (!this._query.IsRunning)
                    {
                        this.Run(true);
                    }
                }
            }
        }

        private void ReflectILNow()
        {
            if ((this._lastCompilation != null) && (this._lastCompilation.AssemblyDLL != null))
            {
                string fullPath = this._lastCompilation.AssemblyDLL.FullPath;
                if (File.Exists(fullPath))
                {
                    string str2 = this._query.QueryKind.ToString().Contains("Program") ? "Main" : "RunUserAuthoredQuery";
                    ReflectorAgent.ActivateReflector(new MemberHelpInfo(null, "code://" + Path.GetFileNameWithoutExtension(fullPath) + "/UserQuery/" + str2 + "()", fullPath, false));
                }
            }
        }

        internal void RefreshOptimizeQuery()
        {
            this.lblOptimize.BorderStyle = MainForm.Instance.OptimizeQueries ? Border3DStyle.SunkenInner : Border3DStyle.RaisedOuter;
            this.lblOptimize.Text = MainForm.Instance.OptimizeQueries ? "/o+" : "/o-";
            this.lblOptimize.ForeColor = MainForm.Instance.OptimizeQueries ? Color.Green : Color.Black;
            this.lblOptimize.BackColor = MainForm.Instance.OptimizeQueries ? Color.FromArgb(0xde, 0xec, 0xff) : Color.Empty;
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            bool completionResults;
            int queryCount;
            bool completedBeforeWeStarted;
            if (!this._processingProvisionalData)
            {
                ExecutionProgress? nullable;
                ExecutionProgress executionProgress;
                if (!this.IsPluginSelected() && (this.lblMiscStatus.Text != (this._outputInfoMessage ?? "")))
                {
                    this.lblMiscStatus.Text = this._outputInfoMessage ?? "";
                }
                completionResults = this._refreshTimer.Interval == 1;
                if (completionResults)
                {
                    this._refreshTimer.Interval = 200;
                }
                if (!completionResults)
                {
                    nullable = this._lastExecutionProgress;
                    executionProgress = this._query.ExecutionProgress;
                }
                if (((((ExecutionProgress) nullable.GetValueOrDefault()) != executionProgress) || !nullable.HasValue) && !this._readLinePanelVisible)
                {
                    this._lastExecutionProgress = new ExecutionProgress?(this._query.ExecutionProgress);
                    if (((ExecutionProgress) this._lastExecutionProgress) == ExecutionProgress.AwaitingDataContext)
                    {
                        this.lblStatus.Text = "Fetching schema...";
                    }
                    else if (((ExecutionProgress) this._lastExecutionProgress) == ExecutionProgress.Async)
                    {
                        this.lblStatus.Text = "Query continuing asynchronously...";
                    }
                    else if (((ExecutionProgress) this._lastExecutionProgress) < ExecutionProgress.Finished)
                    {
                        this.lblStatus.Text = this._lastExecutionProgress.ToString();
                    }
                    if ((((((ExecutionProgress) this._lastExecutionProgress) == ExecutionProgress.Executing) && !this._executionTrackingTimer.Enabled) && !this._executingQueryOptimized) && this.IsQueryLanguageTrackable(this._executingQueryLanguage))
                    {
                        this._executionTrackingTimer.Interval = 500;
                        this._executionTrackingTimer.Start();
                    }
                }
                if (!((MainForm.Instance.CurrentQueryControl == this) && this.btnResults.Checked))
                {
                    this._refreshTicksOnResults = 0;
                }
                else
                {
                    this._refreshTicksOnResults++;
                }
                if (completionResults || ((((MainForm.Instance.CurrentQueryControl == this) && (Control.MouseButtons == MouseButtons.None)) && (this.btnResults.Checked || this.btnSql.Checked)) && ((this._pendingResultsShow || (this._query.ExecutionProgress != ExecutionProgress.Finished)) || this.AreResultsVisible())))
                {
                    TimeSpan span;
                    if ((this._query.ExecutionProgress == ExecutionProgress.Executing) || (this._query.ExecutionProgress == ExecutionProgress.Async))
                    {
                        this.ShowResultsUponQueryStart();
                    }
                    if (this._query.ExecutionProgress == ExecutionProgress.Finished)
                    {
                        span = (TimeSpan) (DateTime.Now - this._lastServerAction);
                    }
                    if ((span.Milliseconds > 0x1388) && (this._refreshTimer.Interval < 0x3e8))
                    {
                        this._refreshTimer.Interval = 0x3e8;
                    }
                    if (!completionResults)
                    {
                        int? progress = this._query.Progress;
                        if (progress.HasValue && ((this.queryProgressBar.Style != ProgressBarStyle.Continuous) || (this.queryProgressBar.Value != progress.Value)))
                        {
                            this.queryProgressBar.Value = progress.Value;
                            this.queryProgressBar.Style = ProgressBarStyle.Continuous;
                        }
                        else if (!(progress.HasValue || (this.queryProgressBar.Style != ProgressBarStyle.Continuous)))
                        {
                            this.queryProgressBar.Style = ProgressBarStyle.Marquee;
                        }
                    }
                    if (this._query.HaveResultsChanged())
                    {
                        this._lastServerAction = DateTime.Now;
                        if ((this._query.ExecutionProgress == ExecutionProgress.Finished) && (this._refreshTimer.Interval == 0x3e8))
                        {
                            this._refreshTimer.Interval = 200;
                        }
                        queryCount = this._queryCount;
                        this._processingProvisionalData = true;
                        completedBeforeWeStarted = this._gotQueryCompletionMessage;
                        ThreadPool.QueueUserWorkItem(delegate (object param0) {
                            try
                            {
                                if ((this._queryCount != queryCount) || this.IsDisposed)
                                {
                                    this._processingProvisionalData = false;
                                }
                                else
                                {
                                    ResultData resultData = this._query.GetResultData();
                                    if (((resultData == null) || (this._queryCount != queryCount)) || this.IsDisposed)
                                    {
                                        this._processingProvisionalData = false;
                                    }
                                    else
                                    {
                                        this.BeginInvoke(delegate {
                                            try
                                            {
                                                if ((this._queryCount == queryCount) && !this.IsDisposed)
                                                {
                                                    if (resultData.AutoScrollResults != this._lastAutoScrollResultsFromQuery)
                                                    {
                                                        this._lastAutoScrollResultsFromQuery = this._autoScrollResultsFromQuery = resultData.AutoScrollResults;
                                                    }
                                                    bool flag = this._autoScrollResultsFromQuery.HasValue ? this._autoScrollResultsFromQuery.Value : ((MainForm.Instance != null) && MainForm.Instance.AutoScrollResults);
                                                    bool flag2 = false;
                                                    Stopwatch stopwatch = Stopwatch.StartNew();
                                                    if (!string.IsNullOrEmpty(resultData.Output))
                                                    {
                                                        using (new SaveBrowserScrollPos(this._dataBrowser, (!flag || (Control.ModifierKeys == Keys.Alt)) ? this._readLinePanelVisible : true))
                                                        {
                                                            if (this.SetDataContent(resultData.Output) && ((((!completionResults && completedBeforeWeStarted) && (this._gotQueryCompletionMessage && !this.IsPluginSelected())) && ((!this._uberCancelMessage && !resultData.MessageLoopFailed) && (this._refreshTicksOnResults > 1))) && this.btnResults.Checked))
                                                            {
                                                                this.DisplayUberCancel(true);
                                                            }
                                                        }
                                                        if (resultData.Output.Length > 0x186a0)
                                                        {
                                                            flag2 = true;
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(resultData.Lambda))
                                                    {
                                                        this.SetLambdaContent(resultData.Lambda);
                                                    }
                                                    if (!string.IsNullOrEmpty(resultData.SQL))
                                                    {
                                                        if (((resultData.SQL.Length < 0x1388) || this.btnSql.Checked) || completionResults)
                                                        {
                                                            this._pendingSqlTranslation = null;
                                                            this.SetSqlContent(resultData.SQL);
                                                            if (resultData.SQL.Length > 0x4e20)
                                                            {
                                                                flag2 = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            this._pendingSqlTranslation = resultData.SQL;
                                                        }
                                                    }
                                                    long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                                                    if (elapsedMilliseconds > 200L)
                                                    {
                                                        flag2 = true;
                                                    }
                                                    if (flag2)
                                                    {
                                                        this._refreshTimer.Interval = (elapsedMilliseconds > 600L) ? 0x1388 : 0x3e8;
                                                    }
                                                }
                                            }
                                            catch (Exception exception)
                                            {
                                                Program.ProcessException(exception);
                                            }
                                            finally
                                            {
                                                this._processingProvisionalData = false;
                                            }
                                        });
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                Program.ProcessException(exception);
                            }
                        });
                    }
                }
            }
        }

        private void RemovePluginWinButton(PluginControl pluginControl)
        {
            Action a = delegate {
                ToolStripButton button = this._pluginWinButtons.FirstOrDefault<ToolStripButton>(b => b.Tag == pluginControl);
                if (button != null)
                {
                    bool flag = button.Checked;
                    this.tsOutput.Items.Remove(button);
                    this._pluginWinButtons.Remove(button);
                    if (flag)
                    {
                        this.SelectResultsPanel(false);
                    }
                }
            };
            if (base.InvokeRequired)
            {
                this.BeginInvoke(a);
            }
            else
            {
                a();
            }
        }

        internal void ReportMainThreadPosition()
        {
            bool flag = this._firstExecutionTrack;
            this._firstExecutionTrack = false;
            if ((MainForm.Instance.CurrentQueryControl == this) && (MainForm.Instance.WindowState != FormWindowState.Minimized))
            {
                if (UserOptionsLive.Instance.ExecutionTrackingDisabled)
                {
                    this.ClearExecutionTrackingIndicators();
                    if ((this._editor.get_Document().get_LineIndicators().get_Count() == 0) && this._editor.get_IndicatorMarginVisible())
                    {
                        this._editor.set_IndicatorMarginVisible(false);
                    }
                }
                else if (!(!this._modifiedWhenRunning && this._query.IsRunning))
                {
                    this._executionTrackingTimer.Stop();
                    this.ClearExecutionTrackingIndicators();
                }
                else
                {
                    ExecutionTrackInfo mainThreadPosition = this._query.GetMainThreadPosition(false);
                    int num = this._lastExecutionTrackCost1 + this._lastExecutionTrackCost2;
                    if (((mainThreadPosition != null) && flag) && (mainThreadPosition.Cost > 200))
                    {
                        mainThreadPosition.Cost = 200;
                    }
                    if ((mainThreadPosition != null) && ((mainThreadPosition.Cost > 200) || (((mainThreadPosition.Cost > 40) && (this._lastExecutionTrackCost1 > 40)) && (this._lastExecutionTrackCost2 > 40))))
                    {
                        this._executionTrackingTimer.Stop();
                        this.ClearExecutionTrackingIndicators();
                        Log.Write(string.Concat(new object[] { "Execution tracking timed out [", mainThreadPosition.Cost, ",", this._lastExecutionTrackCost1, ",", this._lastExecutionTrackCost2, "]" }));
                        this.lblMiscStatus.Text = this._outputInfoMessage = "Auto execution tracking timed out";
                    }
                    else if ((mainThreadPosition == null) && (this._lastTrackInfo != null))
                    {
                        this._lastTrackInfo = null;
                    }
                    else
                    {
                        this._lastTrackInfo = mainThreadPosition;
                        if (mainThreadPosition != null)
                        {
                            num += mainThreadPosition.Cost;
                            this._lastExecutionTrackCost2 = this._lastExecutionTrackCost1;
                            this._lastExecutionTrackCost1 = mainThreadPosition.Cost;
                        }
                        RowColumn[] source = (mainThreadPosition == null) ? null : mainThreadPosition.MainThreadStack;
                        if (this._currentExecutionStack != null)
                        {
                            foreach (BitmapBookmarkLineIndicator indicator in this._currentExecutionStack)
                            {
                                this._editor.get_Document().get_LineIndicators().Remove(indicator);
                            }
                        }
                        List<BitmapBookmarkLineIndicator> list = new List<BitmapBookmarkLineIndicator>();
                        if ((source != null) && (source.Length > 0))
                        {
                            Func<BitmapBookmarkLineIndicator, bool> predicate = null;
                            int line = (source[0].Row - 1) + this._querySelectionStartRow;
                            if (line == this._lastExecutionLine)
                            {
                                this._lastExecutionLineCount++;
                            }
                            else
                            {
                                this._lastExecutionLine = line;
                                this._lastExecutionLineCount = 0;
                            }
                            if (!list.Any<BitmapBookmarkLineIndicator>(i => (((int) i.get_Tag()) == line)))
                            {
                                ExecutionPointLineIndicator item = new ExecutionPointLineIndicator();
                                item.set_Tag(line);
                                list.Add(item);
                            }
                            foreach (RowColumn column in source.Skip<RowColumn>(1))
                            {
                                line = (column.Row - 1) + this._querySelectionStartRow;
                                if (predicate == null)
                                {
                                    predicate = i => ((int) i.get_Tag()) == line;
                                }
                                if (!list.Any<BitmapBookmarkLineIndicator>(predicate))
                                {
                                    ExecutionPointLineStackIndicator indicator3 = new ExecutionPointLineStackIndicator();
                                    indicator3.set_Tag(line);
                                    list.Add(indicator3);
                                }
                            }
                        }
                        if (this._currentExecutionStack == null)
                        {
                            this._currentExecutionStack = new List<BitmapBookmarkLineIndicator>();
                        }
                        else
                        {
                            using (List<BitmapBookmarkLineIndicator>.Enumerator enumerator = this._currentExecutionStack.ToList<BitmapBookmarkLineIndicator>().GetEnumerator())
                            {
                                Func<BitmapBookmarkLineIndicator, bool> func2 = null;
                                BitmapBookmarkLineIndicator existingIndicator;
                                while (enumerator.MoveNext())
                                {
                                    existingIndicator = enumerator.Current;
                                    if (func2 == null)
                                    {
                                        func2 = i => ((int) i.get_Tag()) == ((int) existingIndicator.get_Tag());
                                    }
                                    if (list.FirstOrDefault<BitmapBookmarkLineIndicator>(func2) == null)
                                    {
                                        existingIndicator.Opacity *= 0.55f;
                                        if (existingIndicator.Opacity > 0.3f)
                                        {
                                            this._editor.get_Document().get_LineIndicators().Add(existingIndicator, (int) existingIndicator.get_Tag());
                                        }
                                        else
                                        {
                                            this._currentExecutionStack.Remove(existingIndicator);
                                        }
                                    }
                                    else
                                    {
                                        this._currentExecutionStack.Remove(existingIndicator);
                                    }
                                }
                            }
                        }
                        foreach (BitmapBookmarkLineIndicator indicator5 in list)
                        {
                            this._editor.get_Document().get_LineIndicators().Add(indicator5, (int) indicator5.get_Tag());
                            this._currentExecutionStack.Add(indicator5);
                        }
                        if (this._currentExecutionStack.Count > 0)
                        {
                            this._editor.set_IndicatorMarginVisible(true);
                        }
                        this._executionTrackingTimer.Interval = ((170 + this._random.Next(50)) + (num * 5)) + ((this._lastExecutionLineCount > 100) ? 0x3e8 : ((this._lastExecutionLineCount > 20) ? 500 : 0));
                    }
                }
            }
        }

        internal void RequestPlugInShow(PluginControl c)
        {
            Action a = delegate {
                if ((!this.IsDisposed && (MainForm.Instance != null)) && (MainForm.Instance.CurrentQueryControl == this))
                {
                    ToolStripButton selectedButton = this._pluginWinButtons.FirstOrDefault<ToolStripButton>(b => b.Tag == c);
                    if (selectedButton != null)
                    {
                        this.SelectOutputPanel(selectedButton, false);
                    }
                }
            };
            if (base.InvokeRequired)
            {
                this.BeginInvoke(a);
            }
            else
            {
                a();
            }
        }

        internal void RequestWinManagerRelocation()
        {
            Action a = delegate {
                if ((!base.IsDisposed && (MainForm.Instance != null)) && (MainForm.Instance.CurrentQueryControl == this))
                {
                    this.SetRegion();
                    this._pluginWinManager.Relocate(this.GetWinManagerTargetBounds());
                }
            };
            if (base.InvokeRequired)
            {
                this.BeginInvoke(a);
            }
            else
            {
                a();
            }
        }

        private void ResetPluginManager(bool delayReset)
        {
            this._outputInfoMessage = null;
            this._pluginWinManager.Reset(delayReset);
            bool flag = this.IsPluginSelected();
            foreach (ToolStripButton button in this._pluginWinButtons)
            {
                button.Dispose();
            }
            this._pluginWinButtons.Clear();
            if (flag)
            {
                this.SelectResultsPanel(false);
            }
            else
            {
                this.UpdateOutputToolStripLayout();
            }
        }

        internal void Run()
        {
            this.Run(false);
        }

        internal void Run(bool compileOnly)
        {
            if (!base.IsDisposed && !this.QueryRunning)
            {
                this._gotQueryCompletionMessage = false;
                this._compileOnly = compileOnly;
                this._memoryAtStart = GC.GetTotalMemory(false);
                this._lastExecutionProgress = 5;
                bool? nullable = null;
                this._lastAutoScrollResultsFromQuery = nullable;
                this._autoScrollResultsFromQuery = this._lastAutoScrollResultsFromQuery = nullable;
                this._pendingResultsShow = true;
                this._modifiedWhenRunning = false;
                this._uberCancelMessage = false;
                this._pendingSqlTranslation = null;
                this.lblMiscStatus.Text = "";
                this._queryCount++;
                this._outputInfoMessage = null;
                this.ClearExecutionTrackingIndicators();
                this._executionTrackingTimer.Stop();
                this.ClearExecutionTrackingIndicators();
                this._firstExecutionTrack = true;
                this._lastExecutionTrackCost2 = 0;
                this._lastExecutionTrackCost1 = 0;
                this._lastExecutionLine = -1;
                this._lastTrackInfo = null;
                this._executingQueryOptimized = UserOptionsLive.Instance.OptimizeQueries;
                this._executingQueryLanguage = this._query.QueryKind;
                this._gotPluginsReadyMessage = false;
                this.KillIEComExceptionTimer();
                this.ClearQueryHighlight();
                this.panOutput.BackColor = Color.White;
                this.panOutput.Update();
                this._docMan.MainErrorLayer.Clear();
                this._docMan.WarningsLayer.Clear();
                this._docMan.StackTraceLayer.Clear();
                this._docMan.ExecutedSelectionLayer.Clear();
                this._editor.set_IndicatorMarginVisible(false);
                this._editor.get_Document().get_LineIndicators().Clear();
                string str = this._editor.get_SelectedView().get_SelectedText();
                if (this.CheckAndPromptQueryDriver())
                {
                    if (this._query != null)
                    {
                        this._query.SuppressErrorsOnExistingClient();
                    }
                    this.ResetPluginManager(false);
                    string querySelection = ((str.Trim().Length <= 1) || compileOnly) ? null : this._editor.get_SelectedView().get_SelectedText();
                    QueryCompilationEventArgs lastCompiler = (this._query.RequiresRecompilation || (this._lastRanSourceSelection != querySelection)) ? null : this._lastCompilation;
                    this._lastRanSourceSelection = querySelection;
                    if (querySelection != null)
                    {
                        this._docMan.ExecutedSelectionLayer.Add(new GrayCodeSpanIndicator(), new TextRange(0, this._editor.get_SelectedView().get_Selection().get_FirstOffset()), false);
                        this._docMan.ExecutedSelectionLayer.Add(new GrayCodeSpanIndicator(), new TextRange(this._editor.get_SelectedView().get_Selection().get_LastOffset(), this._editor.get_Document().get_Length()), false);
                        this._querySelectionStartRow = this._editor.get_SelectedView().get_Selection().get_FirstDocumentPosition().get_Line();
                        this._querySelectionStartCol = this._editor.get_SelectedView().get_Selection().get_FirstDocumentPosition().get_Character();
                        this._query.Run(querySelection, compileOnly, lastCompiler, this._pluginWinManager);
                    }
                    else
                    {
                        this._querySelectionStartCol = 0;
                        this._querySelectionStartRow = 0;
                        this._query.Run(null, compileOnly, lastCompiler, this._pluginWinManager);
                    }
                    this.queryProgressBar.Style = ProgressBarStyle.Marquee;
                    this.lblUberCancel.Visible = false;
                    this.panError.Hide();
                    this.btnResults.Text = this._query.ToDataGrids ? "&Output" : "&Results";
                    this.btnSql.Text = ((this._query.Repository == null) || (!(this._query.Repository.DriverLoader.InternalID == "AstoriaAuto") && !(this._query.Repository.DriverLoader.InternalID == "DallasAuto"))) ? "&SQL" : "Reque&st Log";
                    if (!compileOnly)
                    {
                        this.SetDataContent("");
                        this.SetLambdaContent("");
                        this.SetSqlContent("");
                        this._browserHidden = true;
                    }
                    this.EnableControls();
                    this.lblExecTime.Visible = false;
                    this.lblElapsed.Text = "";
                    this.lblElapsed.Visible = true;
                    this._clockTimer.Start();
                    this._refreshTimer.Interval = 200;
                    this._refreshTimer.Start();
                    this.UpdateAutocompletionMsg();
                    this._editor.Focus();
                }
            }
        }

        internal bool Save()
        {
            if (((this._query.FilePath.Length > 0) && !this._query.IsReadOnly) && ".linq .sql".Split(new char[0]).Contains<string>(Path.GetExtension(this._query.FilePath).ToLowerInvariant()))
            {
                this._query.Save();
            }
            else if (!this.SaveAs())
            {
                return false;
            }
            return true;
        }

        internal bool SaveAs()
        {
            this._query.LastMoved = DateTime.MinValue;
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                string defaultQueryFolder = Options.GetDefaultQueryFolder(true);
                if (_lastDefaultQueryFolder != defaultQueryFolder)
                {
                    _lastDefaultQueryFolder = defaultQueryFolder;
                    _lastSaveFolder = null;
                }
                if (_lastSaveFolder == null)
                {
                    _lastSaveFolder = defaultQueryFolder;
                }
                if (_lastSaveFolder != null)
                {
                    try
                    {
                        dialog.InitialDirectory = _lastSaveFolder;
                    }
                    catch
                    {
                    }
                }
                dialog.Title = "Save Query As";
                dialog.DefaultExt = "linq";
                dialog.Filter = "LINQ query files (*.linq)|*.linq";
                string str2 = (this._query.FilePath.Length > 0) ? Path.GetExtension(this._query.FilePath).ToLowerInvariant() : "";
                if (this._query.QueryKind == QueryLanguage.SQL)
                {
                    string str3 = "SSMS file (*.sql)|*.sql";
                    if (str2 == ".sql")
                    {
                        dialog.Filter = str3 + "|" + dialog.Filter;
                        dialog.DefaultExt = "sql";
                    }
                    else
                    {
                        dialog.Filter = dialog.Filter + "|" + str3;
                    }
                }
                if (this._query.FilePath.Length > 0)
                {
                    dialog.FileName = this._query.FilePath;
                    if (!".linq .sql".Split(new char[0]).Contains<string>(str2))
                    {
                        dialog.FileName = Path.ChangeExtension(dialog.FileName, ".linq");
                    }
                }
                else if ((((this._query.Name != null) && !this._query.Name.ToLowerInvariant().StartsWith("query ")) && (this._query.Name.Length > 0)) && !char.IsSymbol(this._query.Name, 1))
                {
                    dialog.FileName = this._query.Name;
                    if (!(!dialog.FileName.Contains<char>('.') || dialog.FileName.ToLowerInvariant().EndsWith(".linq")))
                    {
                        dialog.FileName = dialog.FileName + ".linq";
                    }
                }
                try
                {
                    if (!(string.IsNullOrEmpty(dialog.FileName) || !(Path.GetDirectoryName(dialog.FileName) == dialog.InitialDirectory)))
                    {
                        dialog.FileName = Path.GetFileName(dialog.FileName);
                    }
                }
                catch
                {
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    QueryControl control = MainForm.Instance.FindOpenQueryControl(dialog.FileName);
                    if ((control != null) && (control != this))
                    {
                        control.Close();
                        MainForm.Instance.CurrentQueryControl = this;
                    }
                    this._query.SaveAs(dialog.FileName);
                    try
                    {
                        _lastSaveFolder = Path.GetDirectoryName(dialog.FileName);
                    }
                    catch
                    {
                    }
                    return true;
                }
                return false;
            }
        }

        private void ScrollActiProEditor(ActiproSoftware.SyntaxEditor.SyntaxEditor editor, VerticalScrollAmount amount, bool down)
        {
            if ((amount == VerticalScrollAmount.Line) && down)
            {
                editor.get_SelectedView().ScrollDown();
            }
            else if (amount == VerticalScrollAmount.Line)
            {
                editor.get_SelectedView().ScrollUp();
            }
            else if ((amount == VerticalScrollAmount.Page) && down)
            {
                editor.get_SelectedView().ScrollPageDown();
            }
            else if (amount == VerticalScrollAmount.Page)
            {
                editor.get_SelectedView().ScrollPageUp();
            }
            else if ((amount == VerticalScrollAmount.Document) && down)
            {
                editor.get_SelectedView().ScrollToDocumentEnd();
            }
            else if (amount == VerticalScrollAmount.Document)
            {
                editor.get_SelectedView().ScrollToDocumentStart();
            }
        }

        private void ScrollBrowser(WebBrowser browser, VerticalScrollAmount amount, bool down)
        {
            int height;
            Rectangle scrollRectangle = browser.Document.Body.ScrollRectangle;
            object domDocument = browser.Document.DomDocument;
            object target = domDocument.GetType().InvokeMember("documentElement", BindingFlags.GetProperty, null, domDocument, null);
            if ((amount == VerticalScrollAmount.Document) && down)
            {
                height = scrollRectangle.Height;
            }
            else if (!((amount != VerticalScrollAmount.Document) || down))
            {
                height = 0;
            }
            else
            {
                int num2 = this.Font.Height;
                height = (int) target.GetType().InvokeMember("scrollTop", BindingFlags.GetProperty, null, target, null);
                if ((amount == VerticalScrollAmount.Page) && down)
                {
                    height += browser.Height - num2;
                }
                else if (!((amount != VerticalScrollAmount.Page) || down))
                {
                    height -= browser.Height - num2;
                }
                else if (down)
                {
                    height += num2;
                }
                else
                {
                    height -= num2;
                }
                if (height < 0)
                {
                    height = 0;
                }
                else if (height > scrollRectangle.Height)
                {
                    height = scrollRectangle.Height;
                }
            }
            target.GetType().InvokeMember("scrollTop", BindingFlags.SetProperty, null, target, new object[] { height });
        }

        internal void ScrollResults(VerticalScrollAmount amount, bool down)
        {
            this.ScrollResults(amount, down, false, false);
        }

        internal void ScrollResults(VerticalScrollAmount amount, bool down, bool applyToFirstResultsPanel, bool selectFirstResultsPanel)
        {
            if (this.AreResultsVisible())
            {
                bool flag = this._editor.Focused || this._editor.ContainsFocus;
                if (!(!selectFirstResultsPanel || this.btnResults.Checked))
                {
                    this.SelectResultsPanel(false);
                }
                try
                {
                    if (this.btnResults.Checked || applyToFirstResultsPanel)
                    {
                        this.ScrollBrowser(this._dataBrowser, amount, down);
                    }
                    else if (this.btnSql.Checked)
                    {
                        this.ScrollActiProEditor(this.txtSQL, amount, down);
                    }
                    else if (this.btnLambda.Checked)
                    {
                        this.ScrollBrowser(this._lambdaBrowser, amount, down);
                    }
                    else if (this.btnIL.Checked)
                    {
                        this.ScrollBrowser(this._ilBrowser, amount, down);
                    }
                    else if (this.IsPluginSelected())
                    {
                        this._pluginWinManager.InvokeScroll(amount, down);
                    }
                }
                catch
                {
                    if (applyToFirstResultsPanel && selectFirstResultsPanel)
                    {
                        this._dataBrowser.Focus();
                        SendKeys.Flush();
                        SendKeys.SendWait("^{END}");
                        this._dataBrowser.Focus();
                        SendKeys.SendWait("^{END}");
                        if (flag)
                        {
                            this._editor.Focus();
                        }
                    }
                }
            }
        }

        internal void SelectILPanel(bool focusIfAlreadySelected)
        {
            this.SelectOutputPanel(this.btnIL, focusIfAlreadySelected);
        }

        internal void SelectLambdaPanel(bool focusIfAlreadySelected)
        {
            this.SelectOutputPanel(this.btnLambda, focusIfAlreadySelected);
        }

        private void SelectOutputPanel(ToolStripButton selectedButton, bool focusIfAlreadySelected)
        {
            if (selectedButton != null)
            {
                bool flag2;
                ToolStripButton selectedPanelButton = this.GetSelectedPanelButton();
                if (selectedButton == this.btnResults)
                {
                    this._refreshTicksOnResults = 0;
                }
                foreach (ToolStripButton button2 in this.GetPanelSelectorButtons(true))
                {
                    button2.Checked = button2 == selectedButton;
                }
                bool focused = this._editor.Focused;
                bool flag3 = (flag2 = this._readLinePanelVisible && (this._readLinePanel != null)) && this._readLinePanel.ContainsFocus;
                this.UpdateOutputVisibility();
                if ((selectedButton == this.btnSql) && (this._pendingSqlTranslation != null))
                {
                    this.SetSqlContent(this._pendingSqlTranslation);
                    this._pendingSqlTranslation = null;
                }
                if (MainForm.Instance.CurrentQueryControl == this)
                {
                    if (flag2 && ((flag3 || !focused) || focusIfAlreadySelected))
                    {
                        this._readLinePanel.FocusTextBox();
                    }
                    else if (!focused || (focusIfAlreadySelected && selectedPanelButton.Checked))
                    {
                        Control outputControl = this.GetOutputControl(selectedButton);
                        if (outputControl != null)
                        {
                            outputControl.Focus();
                        }
                        else if ((selectedButton.Tag is PluginControl) && this.AreResultsVisible())
                        {
                            this.LastPluginFocus = DateTime.UtcNow;
                            this._pluginWinManager.Show(this.GetSelectedPluginControl(), true);
                        }
                    }
                }
            }
        }

        internal void SelectResultsPanel(bool focusIfAlreadySelected)
        {
            this.SelectOutputPanel(this.btnResults, focusIfAlreadySelected);
        }

        internal void SelectSqlPanel(bool focusIfAlreadySelected)
        {
            this.SelectOutputPanel(this.btnSql, focusIfAlreadySelected);
        }

        private bool SetDataContent(string content)
        {
            try
            {
                bool flag;
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                if (!(flag = (this._resultsContent != null) && this._resultsContent.SequenceEqual<byte>(bytes)))
                {
                    this._dataBrowser.DocumentStream = this._msData = new MemoryStream(this._resultsContent = bytes);
                }
                if (content.Length > 0)
                {
                    this._browserHidden = false;
                }
                this.UpdateOutputVisibility();
                return !flag;
            }
            catch (COMException)
            {
                if (!this._query.IsRunning)
                {
                    throw;
                }
                return true;
            }
        }

        internal void SetHorizontalLayout()
        {
            if (this.splitContainer.Orientation != Orientation.Horizontal)
            {
                this.splitContainer.SplitterWidth--;
                this._oldVerticalSplitFraction = (this.splitContainer.SplitterDistance * 1.0) / ((double) this.splitContainer.ClientSize.Width);
                this.splitContainer.Orientation = Orientation.Horizontal;
                if ((this._oldHorizontalSplitFraction <= 0.0) || (this._oldHorizontalSplitFraction >= 0.95))
                {
                    this._oldHorizontalSplitFraction = 0.5;
                }
                this.splitContainer.SplitterDistance = Convert.ToInt32((double) (this._oldHorizontalSplitFraction * this.splitContainer.ClientSize.Height));
            }
        }

        private void SetILContent(string content)
        {
            this._ilBrowser.DocumentStream = this._ilData = new MemoryStream(Encoding.UTF8.GetBytes(content));
            this._ilBrowser.Visible = !string.IsNullOrEmpty(content);
            this.UpdateOutputVisibility();
        }

        private void SetLambdaContent(string content)
        {
            try
            {
                this._lambdaBrowser.DocumentStream = this._lambdaData = new MemoryStream(Encoding.UTF8.GetBytes(content));
                this._lambdaBrowser.Visible = !string.IsNullOrEmpty(content);
                this.UpdateOutputVisibility();
            }
            catch (COMException)
            {
            }
        }

        internal void SetLanguage(int index)
        {
            if (this.cboLanguage.Enabled)
            {
                if (this._query.IsMyExtensions)
                {
                    if (index != 2)
                    {
                        return;
                    }
                    index = 0;
                }
                this.cboLanguage.SelectedIndex = index;
                this.CheckToFromProgramLanguage();
                this.cboType_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        private void SetRegion()
        {
        }

        internal void SetSplitterHeight(float fraction)
        {
            if (this.splitContainer.Orientation != Orientation.Vertical)
            {
                this.splitContainer.SplitterDistance = Convert.ToInt32((float) (this.splitContainer.ClientSize.Height * fraction));
            }
        }

        private void SetSqlContent(string content)
        {
            this.txtSQL.Text = content ?? "";
            this.txtSQL.Enabled = !string.IsNullOrEmpty(content);
            this.txtSQL.set_ScrollBarType(6);
            this.txtSQL.set_ScrollBarType(0);
        }

        internal void SetVerticalLayout()
        {
            if (this.splitContainer.Orientation != Orientation.Vertical)
            {
                this.splitContainer.SplitterWidth++;
                if (this.Dock == DockStyle.Fill)
                {
                    this._oldHorizontalSplitFraction = (this.splitContainer.SplitterDistance * 1.0) / ((double) this.splitContainer.ClientSize.Height);
                }
                this.splitContainer.Orientation = Orientation.Vertical;
                if ((this._oldVerticalSplitFraction <= 0.0) || (this._oldVerticalSplitFraction >= 0.95))
                {
                    this._oldVerticalSplitFraction = 0.5;
                }
                this.splitContainer.SplitterDistance = Convert.ToInt32((double) (this._oldVerticalSplitFraction * this.splitContainer.ClientSize.Width));
            }
        }

        private void ShowOptimizeTip()
        {
            string text = MainForm.Instance.OptimizeQueries ? "Compiler optimizations ON: click to toggle (Shift+Alt+O) or right-click for more info" : "Compiler optimizations off: click to toggle (Shift+Alt+O) or right-click for more info";
            int width = TextRenderer.MeasureText(text, this.Font).Width;
            this.toolTip.Show(text, this.statusStrip, new Point((base.Width - width) - 10, -this.Font.Height - 5), 0xbb8);
            this._optimizeTipShown = true;
        }

        private void ShowReadLinePanel(Client client, string prompt, string defaultValue, string[] options)
        {
            this._readLinePanelVisible = true;
            if (this._readLinePanel == null)
            {
                ReadLinePanel panel = new ReadLinePanel {
                    Dock = DockStyle.Bottom
                };
                this._readLinePanel = panel;
            }
            this._readLinePanel.EntryMade = delegate (string text) {
                if (this.QueryRunning)
                {
                    this.lblStatus.Text = "Executing";
                }
                this.HideReadLinePanel();
                this._query.ReadLineCompleted(client, text);
            };
            if (this._readLinePanel.Parent != this)
            {
                base.SuspendLayout();
                base.Controls.Add(this._readLinePanel);
                this._readLinePanel.SendToBack();
                this.statusStrip.SendToBack();
                base.ResumeLayout();
            }
            this.lblStatus.Text = "Awaiting user input";
            this._readLinePanel.Go(prompt, defaultValue, options);
            this.ScrollResults(VerticalScrollAmount.Document, true, true, false);
        }

        private void ShowResultsUponQueryStart()
        {
            if (!(!this._pendingResultsShow || this.AreResultsVisible()))
            {
                this.ToggleResultsCollapse();
                this._pendingResultsShow = false;
            }
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this._editor != null)
            {
                this.FocusQuery();
            }
            MainForm.Instance.IsSplitting = false;
            if (((this.panOutput.BackColor == Program.LightTransparencyKey) && (MainForm.Instance.TransparencyKey == Program.LightTransparencyKey)) && (Program.TransparencyKey != MainForm.Instance.TransparencyKey))
            {
                this.panOutput.BackColor = MainForm.Instance.TransparencyKey = Program.TransparencyKey;
            }
        }

        private SuppressPullData SuppressPull()
        {
            return new SuppressPullData(this);
        }

        internal void ToggleAllOutlining()
        {
            bool flag = true;
            if (this._editor.get_Document().get_Outlining().get_RootNode().get_Count() > 0)
            {
                flag = this._editor.get_Document().get_Outlining().get_RootNode().get_Item(0).get_Expanded();
            }
            OutliningNode node = this._editor.get_Document().get_Outlining().get_RootNode().FindNodeRecursive(this._editor.get_Caret().get_Offset());
            if (node != null)
            {
                flag = node.get_Expanded();
            }
            else if (this._editor.get_Caret().get_Offset() > 0)
            {
                node = this._editor.get_Document().get_Outlining().get_RootNode().FindNodeRecursive(this._editor.get_Caret().get_Offset() - 1);
                if (node != null)
                {
                    flag = node.get_Expanded();
                }
            }
            if (flag)
            {
                this._editor.get_Document().get_Outlining().get_RootNode().CollapseDescendants();
            }
            else
            {
                this._editor.get_Document().get_Outlining().get_RootNode().ExpandDescendants();
            }
        }

        public bool ToggleGraphColumn(string tableID, int colIndex)
        {
            this._dataBrowser.ToggleGraphColumn(tableID, colIndex);
            return false;
        }

        internal bool ToggleOutliningExpansion()
        {
            // This item is obfuscated and can not be translated.
            bool flag2;
            bool flag3;
            CompilationUnit unit;
            OutliningNode node3;
            if ((this._query.QueryKind != QueryLanguage.Program) && (this._query.QueryKind != QueryLanguage.VBProgram))
            {
                return false;
            }
            int num = this._editor.get_Caret().get_Offset();
            int num2 = num;
            if (!(flag2 = num == 0))
            {
                while (true)
                {
                    if ((num2 > 0) && !char.IsWhiteSpace(this._editor.get_Document().get_Characters(num2)))
                    {
                    }
                    if (0 == 0)
                    {
                        break;
                    }
                    num2--;
                }
            }
            if (!flag2)
            {
                flag2 = (num2 == 0) || (this._editor.get_Document().get_Characters(num2) == '\n');
            }
            num2 = num;
            if (!(flag3 = (num2 >= this._editor.get_Document().get_Length()) || (this._editor.get_Document().get_Characters(num2) == '\n')))
            {
                while (true)
                {
                    if ((num2 < this._editor.get_Document().get_Length()) && !char.IsWhiteSpace(this._editor.get_Document().get_Characters(num2)))
                    {
                    }
                    if (0 == 0)
                    {
                        break;
                    }
                    num2++;
                }
            }
            if (!flag3)
            {
                flag3 = (num2 == this._editor.get_Document().get_Length()) || (this._editor.get_Document().get_Characters(num2) == '\n');
            }
            this._editor.get_Document().get_Characters(num);
            if (flag2 && !flag3)
            {
                while (char.IsWhiteSpace(this._editor.get_Document().get_Characters(num)))
                {
                    num++;
                }
            }
            else if (flag3 && !flag2)
            {
                while (this._editor.get_Document().get_Characters(num) == '\0')
                {
                Label_01A3:
                    if (1 == 0)
                    {
                        goto Label_01D8;
                    }
                    num--;
                }
                goto Label_01A3;
            }
        Label_01D8:
            unit = this._editor.get_Document().get_SemanticParseData() as CompilationUnit;
            if (unit != null)
            {
                for (IAstNode node = unit.FindNodeRecursive(num); node == null; node = node.get_ParentNode())
                {
                Label_0203:
                    if (0 == 0)
                    {
                        IBlockAstNode node2 = node as IBlockAstNode;
                        if (((node2 != null) && (node2.get_BlockStartOffset() > 0)) && ((node2 is TypeDeclaration) || (node2 is TypeMemberDeclaration)))
                        {
                            num = node2.get_BlockStartOffset();
                        }
                        goto Label_0262;
                    }
                }
                goto Label_0203;
            }
        Label_0262:
            node3 = this._editor.get_Document().get_Outlining().get_RootNode().FindNodeRecursive(num);
            if (node3 != null)
            {
                node3.ToggleExpansion();
                return true;
            }
            if (num > 0)
            {
                node3 = this._editor.get_Document().get_Outlining().get_RootNode().FindNodeRecursive(num - 1);
                if (node3 != null)
                {
                    node3.ToggleExpansion();
                    return true;
                }
            }
            return false;
        }

        internal void ToggleResultsCollapse()
        {
            Func<EditorView, int> selector = null;
            base.SuspendLayout();
            this.panMain.SuspendLayout();
            try
            {
                if (this.panEditor.Parent == this.panMain)
                {
                    if (this.AreResultsDetached())
                    {
                        return;
                    }
                    this.panEditor.Parent = this.splitContainer.Panel1;
                    this.splitContainer.Show();
                    if (this.GetSelectedPanelButton().Tag is PluginControl)
                    {
                        this._pluginWinManager.Show(this.GetSelectedPluginControl(), false);
                    }
                }
                else
                {
                    this.splitContainer.Hide();
                    this.panEditor.Parent = this.panMain;
                    this._pluginWinManager.Hide();
                }
            }
            finally
            {
                base.ResumeLayout();
                this.panMain.ResumeLayout();
            }
            if ((this.panEditor.Parent == this.splitContainer.Panel1) && this._firstResultsShow)
            {
                this._firstResultsShow = false;
                if (!MainForm.Instance.VerticalResultsLayout && (this._editor.get_Document().get_Length() > 0))
                {
                    try
                    {
                        if (selector == null)
                        {
                            selector = v => v.GetCharacterBounds(this._editor.get_Document().get_Length() - 1).Bottom;
                        }
                        int num = this._editor.get_Views().OfType<EditorView>().Max<EditorView>(selector) + (this.Font.Height * 5);
                        num = Math.Min(Math.Max(num, Convert.ToInt32((float) (this.splitContainer.ClientSize.Height * 0.15f))), Convert.ToInt32((float) (this.splitContainer.ClientSize.Height * 0.5f)));
                        this.splitContainer.SplitterDistance = num;
                    }
                    catch
                    {
                    }
                }
            }
            this.FocusQuery();
        }

        public override string ToString()
        {
            return this._query.Name;
        }

        internal bool TryClose()
        {
            if (this._query.IsModified && (this._query.Source.Trim().Length > 0))
            {
                DialogResult result = MessageBox.Show("Save " + this._query.Name + "?", "LINQPad", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel)
                {
                    return false;
                }
                if (!((result != DialogResult.Yes) || this.Save()))
                {
                    return false;
                }
            }
            this.Close();
            return true;
        }

        private void tsOutput_MouseEnter(object sender, EventArgs e)
        {
            if (this.AreResultsDetached())
            {
                Form form = this.tsOutput.FindForm();
                if (form != null)
                {
                    form.Activate();
                }
            }
        }

        internal void UpdateAutocompletionMsg()
        {
            if (base.InvokeRequired)
            {
                this.BeginInvoke(new Action(this.UpdateAutocompletionMsgCore));
            }
            else
            {
                this.UpdateAutocompletionMsgCore();
            }
        }

        private void UpdateAutocompletionMsgCore()
        {
            try
            {
                this.btnActivateAutocompletion.Visible = (MainForm.Instance != null) && !MainForm.Instance.ShowLicensee;
            }
            catch
            {
            }
        }

        internal void UpdateAutocompletionService()
        {
            this._docMan.ConfigureLanguage();
            this._docMan.ConfigureResolver();
        }

        internal void UpdateEditorZoom()
        {
            this._editor.UpdateZoom();
        }

        private void UpdateElapsed()
        {
            TimeSpan totalRunningTime = this._query.TotalRunningTime;
            string str = totalRunningTime.Hours.ToString("D2") + ":" + totalRunningTime.Minutes.ToString("D2") + ":" + totalRunningTime.Seconds.ToString("D2");
            if (str != this.lblElapsed.Text)
            {
                this.lblElapsed.Text = str;
            }
        }

        private void UpdateErrorHeight()
        {
            int lineCount = Native.GetLineCount(this.txtError);
            if (lineCount > 8)
            {
                lineCount = 8;
                this.txtError.ScrollBars = ScrollBars.Vertical;
            }
            else
            {
                this.txtError.ScrollBars = ScrollBars.None;
            }
            this.panError.Height = ((((lineCount * this.txtError.Font.Height) + this.panError.Padding.Top) + this.panError.Padding.Bottom) + this.panError.Height) - this.panError.ClientSize.Height;
        }

        private void UpdateFocusedRepository()
        {
            Repository currentRepository = this._schemaTree.GetCurrentRepository(true);
            this._enableUseCurrentDb = currentRepository != null;
            this.llDbUseCurrent.Enabled = currentRepository != this._query.Repository;
            if (currentRepository != null)
            {
                string friendlyName = currentRepository.GetFriendlyName(Repository.FriendlyNameMode.Short);
                if (friendlyName.Length > 0x17)
                {
                    friendlyName = friendlyName.Substring(0, 20) + "...";
                }
                this.llDbUseCurrent.Text = "Use " + friendlyName;
            }
            this.UpdateUseCurrentDbVisibility();
        }

        private void UpdateILContent()
        {
            if (this._ilDirty)
            {
                this._ilDirty = false;
                if ((this._lastCompilation != null) && (this._lastCompilation.AssemblyDLL != null))
                {
                    if (((this._query.Repository != null) && this._query.Repository.DriverLoader.IsValid) && this._query.Repository.DriverLoader.Driver.DisallowQueryDisassembly)
                    {
                        this.SetILContent("");
                    }
                    else
                    {
                        string[] additionalReferences = QueryCompiler.Create(this._query, true).References.ToArray<string>();
                        this.SetILContent(Disassembler.DisassembleQuery(this._lastCompilation.AssemblyDLL.FullPath, additionalReferences));
                    }
                }
            }
        }

        private void UpdateOutputToolStripLayout()
        {
            if ((this._pluginWinButtons.Count != 0) && (CS$<>9__CachedAnonymousMethodDelegate32 == null))
            {
                CS$<>9__CachedAnonymousMethodDelegate32 = i => i.Visible || i.IsOnOverflow;
            }
            int num = (CS$<>9__CachedAnonymousMethodDelegate33 != null) ? 0 : this.tsOutput.Items.Cast<ToolStripItem>().Where<ToolStripItem>(CS$<>9__CachedAnonymousMethodDelegate32).Sum<ToolStripItem>(CS$<>9__CachedAnonymousMethodDelegate33);
            ToolStripLayoutStyle style = ((num > 50) && (num > (this.tsOutput.ClientSize.Width - 20))) ? ToolStripLayoutStyle.Flow : ToolStripLayoutStyle.HorizontalStackWithOverflow;
            if (this.tsOutput.LayoutStyle != style)
            {
                this.tsOutput.LayoutStyle = style;
            }
        }

        private void UpdateOutputVisibility()
        {
            ToolStripButton selectedPanelButton = this.GetSelectedPanelButton();
            string infoMessage = (selectedPanelButton == this.btnResults) ? this._outputInfoMessage : null;
            foreach (ToolStripButton button2 in this.GetPanelSelectorButtons(false))
            {
                if (button2 != selectedPanelButton)
                {
                    this.GetOutputPanel(button2).Hide();
                }
            }
            if (this.IsPluginSelected())
            {
                if (this.AreResultsVisible())
                {
                    PluginControl selectedPluginControl = this.GetSelectedPluginControl();
                    infoMessage = selectedPluginControl.InfoMessage;
                    if (!string.IsNullOrEmpty(infoMessage))
                    {
                        this.lblMiscStatus.Font = this.Font;
                        this.lblMiscStatus.BackColor = Color.Transparent;
                    }
                    if (MainForm.Instance.CurrentQueryControl == this)
                    {
                        this._pluginWinManager.Show(selectedPluginControl, false);
                        this.RequestWinManagerRelocation();
                    }
                }
                this.UpdatePluginTransparency();
            }
            else
            {
                this.UpdatePluginTransparency();
                this._pluginWinManager.Hide();
                if (this.btnSql.Checked)
                {
                    this.txtSQL.Show();
                }
                else if (this.btnLambda.Checked)
                {
                    this._lambdaPanel.Visible = true;
                }
                else if (this.btnIL.Checked)
                {
                    if (this._ilDirty)
                    {
                        this.UpdateILContent();
                    }
                    this._ilPanel.Visible = true;
                }
                else
                {
                    this._dataPanel.Visible = this._dataBrowser.Visible = !this._browserHidden;
                }
            }
            this.lblMiscStatus.Text = infoMessage ?? "";
            this.SetRegion();
            this.btnAnalyze.Visible = this.btnSql.Checked;
            this.btnExport.Visible = this.btnFormat.Visible = this.btnResults.Checked;
            this.UpdateOutputToolStripLayout();
        }

        private void UpdatePluginTransparency()
        {
            Action a = null;
            if (this.IsPluginSelected())
            {
                MainForm.Instance.RequestTransparency();
                if (a == null)
                {
                    a = delegate {
                        if (!(base.IsDisposed || !this.IsPluginSelected()))
                        {
                            this.panOutput.BackColor = Program.TransparencyKey;
                        }
                    };
                }
                Program.RunOnWinFormsTimer(a, 100);
            }
            else if (this.panOutput.BackColor != Color.White)
            {
                this.panOutput.BackColor = Color.White;
                this.panOutput.Refresh();
            }
        }

        private void UpdateRepositoryItems(bool populate)
        {
            this.CheckQueryRepositoryWithSchemaTree();
            this.cboDb.Items.Clear();
            this.cboDb.Items.Add("<None>");
            if (this._query.Repository != null)
            {
                this.cboDb.Items.Add(this._query.Repository);
            }
            IEnumerable<Repository> allRepositories = this._schemaTree.GetAllRepositories(true);
            if (populate)
            {
                this.cboDb.Items.AddRange((from r in allRepositories
                    where r != this._query.Repository
                    select r).ToArray<Repository>());
            }
            this.cboDb.SelectedIndex = (this._query.Repository == null) ? 0 : 1;
        }

        private void UpdateUseCurrentDbVisibility()
        {
            this.llDbUseCurrent.Visible = this._enableUseCurrentDb && (base.ClientSize.Width > (this.Font.Height * 0x2d));
        }

        internal void UseCurrentDb(bool toggle)
        {
            if (!this._query.IsMyExtensions)
            {
                Repository currentRepository = this._schemaTree.GetCurrentRepository(true);
                Repository repository = this._query.Repository;
                if (currentRepository == null)
                {
                    if (toggle)
                    {
                        this._schemaTree.ReselectRepository();
                        repository = this._schemaTree.GetCurrentRepository(true);
                    }
                }
                else if (this._query.Repository == currentRepository)
                {
                    if (toggle)
                    {
                        repository = null;
                        this._schemaTree.UnselectRepository();
                    }
                }
                else
                {
                    repository = currentRepository;
                }
                if (repository != this._query.Repository)
                {
                    this.CheckToFromProgramLanguage(this._query.QueryKind, this._query.QueryKind, this._query.Repository != null, repository != null);
                    this._query.Repository = repository;
                }
                this._editor.Focus();
            }
        }

        internal int CaretOffset
        {
            get
            {
                return this._editor.get_Caret().get_Offset();
            }
            set
            {
                this._editor.get_Caret().set_Offset(value);
            }
        }

        private Document Doc
        {
            get
            {
                return this._docMan.Document;
            }
        }

        internal QueryEditor Editor
        {
            get
            {
                return this._editor;
            }
        }

        internal bool HasPluginControls
        {
            get
            {
                return ((this._pluginWinManager != null) && this._pluginWinManager.HasControls);
            }
        }

        internal DateTime LastPluginFocus { get; private set; }

        internal RunnableQuery Query
        {
            get
            {
                return this._query;
            }
        }

        private bool QueryRunning
        {
            get
            {
                return this._query.IsRunning;
            }
        }

        internal string SelectedQueryText
        {
            get
            {
                return this._editor.get_SelectedView().get_SelectedText();
            }
        }

        internal bool WasPluginRecentlyFocused
        {
            get
            {
                return (((this._pluginWinManager != null) && this.IsPluginSelected()) && (this._pluginWinManager.ActiveFormPulse > DateTime.UtcNow.AddMilliseconds(-250.0)));
            }
        }

        private class BrowserBorder : Panel
        {
            protected override void OnLayout(LayoutEventArgs levent)
            {
                if (base.Controls.Count != 0)
                {
                    Rectangle clientRectangle = base.ClientRectangle;
                    clientRectangle.Inflate(2, 2);
                    if (base.Controls[0].Bounds != clientRectangle)
                    {
                        base.Controls[0].Bounds = clientRectangle;
                    }
                }
            }
        }

        private class LINQPadFindReplaceForm : FindReplaceForm
        {
            public LINQPadFindReplaceForm(ActiproSoftware.SyntaxEditor.SyntaxEditor editor, FindReplaceOptions options) : base(editor, options)
            {
            }

            protected override void OnClosing(CancelEventArgs e)
            {
            }
        }

        private class SuppressPullData : IDisposable
        {
            private QueryControl _qc;

            internal SuppressPullData(QueryControl qc)
            {
                this._qc = qc;
                this._qc._suppressPullCount++;
            }

            public void Dispose()
            {
                this._qc._suppressPullCount--;
            }
        }
    }
}

