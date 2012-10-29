namespace LINQPad.UI
{
    using ActiproBridge;
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class OptionsForm : BaseForm
    {
        private AdvancedOptions _advancedOptions;
        private string[] _fontNames;
        private string _fontPath;
        private Font[] _fonts;
        private Color _oldEditorBackColor;
        private bool _populatedFonts;
        private string _queriesPath;
        private UserOptions _userOptions;
        private Button btnCancel;
        private Button btnChooseColor;
        private Button btnEditStyleSheet;
        private Button btnOK;
        private Button btnProxy;
        private ComboBox cboFont;
        private ComboBox cboLanguage;
        private CheckBox chkDisable1To1;
        private CheckBox chkLineNumbers;
        private IContainer components;
        private FlowLayoutPanel flowLayoutPanel1;
        private FontDialog fontDialog;
        private BackgroundWorker fontWorker;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private GroupBox groupBox6;
        private GroupBox groupBox7;
        private GroupBox grpEditorColor;
        private GroupBox grpEditorFont;
        private GroupBox grpMyQueries;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label lblAdvancedRestart;
        private LinkLabel llCreatePluginsFolder;
        private FolderLocationControl myPlugins;
        private FolderLocationControl myQueries;
        private FolderLocationControl mySnippets;
        private Panel panColorSample;
        private Panel panel1;
        private Panel panel2;
        private TableLayoutPanel panOKCancel;
        private FlowLayoutPanel panPluginHelp;
        private Panel panSpacer1;
        private Panel panSpacer2;
        private PropertyGrid propertyGrid;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private RadioButton rbCustomColor;
        private RadioButton rbCustomFont;
        private RadioButton rbCustomStyleSheet;
        private RadioButton rbDefaultColor;
        private RadioButton rbDefaultFont;
        private RadioButton rbDefaultStyleSheet;
        private RadioButton rbGrids;
        private RadioButton rbNoOptimize;
        private RadioButton rbOptimize;
        private RadioButton rbText;
        private TabControl tabControl;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel5;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private TabPage tabPage6;
        private NumericUpDown udMaxQueryResults;

        public OptionsForm() : this(0)
        {
        }

        public OptionsForm(int startTabPage)
        {
            this._fontPath = Path.Combine(Program.UserDataFolder, "queryfont.txt");
            this._queriesPath = Path.Combine(Program.UserDataFolder, "querypath.txt");
            this._userOptions = UserOptions.Instance;
            this._advancedOptions = new AdvancedOptions();
            this._oldEditorBackColor = UserOptions.Instance.ActualEditorBackColor;
            this.components = null;
            this.InitializeComponent();
            this.cboFont.DropDownHeight = this.Font.Height * 20;
            this._fonts = new Font[] { new Font(Control.DefaultFont, FontStyle.Regular) };
            this.cboFont.Items.Add("Populating...");
            this.cboFont.SelectedIndex = 0;
            this.fontWorker.RunWorkerAsync();
            this.rbCustomColor.Checked = !string.IsNullOrEmpty(UserOptions.Instance.EditorBackColor);
            this.panColorSample.BackColor = UserOptions.Instance.ActualEditorBackColor;
            if (File.Exists(this._queriesPath))
            {
                try
                {
                    this.myQueries.FolderText = File.ReadAllText(this._queriesPath);
                }
                catch
                {
                }
            }
            if (!string.IsNullOrEmpty(this._userOptions.CustomSnippetsFolder))
            {
                this.mySnippets.FolderText = this._userOptions.CustomSnippetsFolder;
            }
            if (!string.IsNullOrEmpty(this._userOptions.PluginsFolder))
            {
                this.myPlugins.FolderText = this._userOptions.PluginsFolder;
            }
            if (File.Exists(Options.CustomStyleSheetLocation))
            {
                this.rbCustomStyleSheet.Checked = true;
            }
            try
            {
                int? maxQueryRows = this._userOptions.MaxQueryRows;
                this.udMaxQueryResults.Value = maxQueryRows.HasValue ? ((decimal) maxQueryRows.GetValueOrDefault()) : 0x3e8;
            }
            catch
            {
            }
            try
            {
                QueryLanguage? defaultQueryLanguage = this._userOptions.DefaultQueryLanguage;
                this.cboLanguage.SelectedIndex = defaultQueryLanguage.HasValue ? ((int) defaultQueryLanguage.GetValueOrDefault()) : 0;
            }
            catch
            {
            }
            this.chkDisable1To1.Checked = File.Exists(Program.OneToOneAckFile) && string.Equals(File.ReadAllText(Program.OneToOneAckFile).Trim(), "False", StringComparison.OrdinalIgnoreCase);
            this.chkLineNumbers.Checked = this._userOptions.ShowLineNumbersInEditor;
            if (this._userOptions.ResultsInGrids)
            {
                this.rbGrids.Checked = true;
            }
            this.rbOptimize.Checked = MainForm.Instance.OptimizeQueries;
            this.rbNoOptimize.Checked = !MainForm.Instance.OptimizeQueries;
            this._advancedOptions.Read();
            this.EnableControls();
            this.lblAdvancedRestart.Font = new Font(this.lblAdvancedRestart.Font, FontStyle.Bold);
            this.tabControl.SelectedIndex = startTabPage;
            this.tabControl_SelectedIndexChanged(this, EventArgs.Empty);
            this.myQueries.MRU = MRU.QueryLocations;
            this.mySnippets.MRU = MRU.SnippetLocations;
            this.myPlugins.MRU = MRU.PluginLocations;
        }

        private void btnChooseColor_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog {
                Color = this.panColorSample.BackColor
            };
            using (ColorDialog dialog2 = dialog)
            {
                if (this.panColorSample.BackColor != SystemColors.Window)
                {
                    dialog2.CustomColors = new int[] { ColorTranslator.ToWin32(this.panColorSample.BackColor) };
                }
                if (dialog2.ShowDialog(this) == DialogResult.OK)
                {
                    this.panColorSample.BackColor = dialog2.Color;
                }
            }
        }

        private void btnEditStyleSheet_Click(object sender, EventArgs e)
        {
            ResultStylesForm.Launch();
        }

        private void btnProxy_Click(object sender, EventArgs e)
        {
            using (ProxyForm form = new ProxyForm())
            {
                form.ShowDialog(this);
            }
        }

        private void cboFont_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (this.cboFont.Enabled)
            {
                e.DrawBackground();
            }
            else
            {
                e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
            }
            if (e.Index > -1)
            {
                using (Brush brush = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(this.cboFont.Items[e.Index].ToString(), this._fonts[e.Index], brush, e.Bounds);
                }
            }
            e.DrawFocusRectangle();
        }

        private void DeleteStyleSheet()
        {
            if (File.Exists(Options.CustomStyleSheetLocation))
            {
                try
                {
                    File.Delete(Options.CustomStyleSheetLocation);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Error writing to file " + Options.CustomStyleSheetLocation + "\r\n" + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
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
            this.cboFont.Enabled = this.rbCustomFont.Checked && this._populatedFonts;
            this.rbCustomFont.Enabled = this.rbDefaultFont.Enabled = this._populatedFonts;
            this.btnEditStyleSheet.Enabled = this.rbCustomStyleSheet.Checked;
            this.btnChooseColor.Enabled = this.rbCustomColor.Checked;
        }

        private void fontWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IEnumerable<FontFamily> enumerable = from ff in FontFamily.Families
                where ff.IsStyleAvailable(FontStyle.Regular)
                select ff;
            this._fontNames = (from ff in enumerable select ff.Name).ToArray<string>();
            this._fonts = (from ff in enumerable select new Font(ff, 10f)).ToArray<Font>();
        }

        private void fontWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.cboFont.Items.Clear();
                this.cboFont.Items.AddRange(this._fontNames);
                if (this.cboFont.Items.Contains("Consolas"))
                {
                    this.cboFont.SelectedItem = "Consolas";
                }
                else if (this.cboFont.Items.Contains("Courier New"))
                {
                    this.cboFont.SelectedItem = "Courier New";
                }
                if (File.Exists(this._fontPath))
                {
                    try
                    {
                        this.cboFont.SelectedItem = File.ReadAllText(this._fontPath);
                        this.rbCustomFont.Checked = true;
                    }
                    catch
                    {
                    }
                }
                this._populatedFonts = true;
                this.EnableControls();
            }
        }

        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.rbDefaultFont = new RadioButton();
            this.rbCustomFont = new RadioButton();
            this.fontDialog = new FontDialog();
            this.cboFont = new ComboBox();
            this.grpEditorFont = new GroupBox();
            this.label1 = new Label();
            this.tabControl = new TabControl();
            this.tabPage1 = new TabPage();
            this.chkLineNumbers = new CheckBox();
            this.grpEditorColor = new GroupBox();
            this.btnChooseColor = new Button();
            this.panColorSample = new Panel();
            this.rbDefaultColor = new RadioButton();
            this.rbCustomColor = new RadioButton();
            this.tableLayoutPanel3 = new TableLayoutPanel();
            this.tabPage5 = new TabPage();
            this.groupBox3 = new GroupBox();
            this.label4 = new Label();
            this.chkDisable1To1 = new CheckBox();
            this.panel1 = new Panel();
            this.groupBox6 = new GroupBox();
            this.label6 = new Label();
            this.rbOptimize = new RadioButton();
            this.rbNoOptimize = new RadioButton();
            this.panel2 = new Panel();
            this.groupBox2 = new GroupBox();
            this.cboLanguage = new ComboBox();
            this.tabPage4 = new TabPage();
            this.groupBox7 = new GroupBox();
            this.rbText = new RadioButton();
            this.rbGrids = new RadioButton();
            this.tableLayoutPanel5 = new TableLayoutPanel();
            this.udMaxQueryResults = new NumericUpDown();
            this.label3 = new Label();
            this.groupBox1 = new GroupBox();
            this.btnEditStyleSheet = new Button();
            this.rbDefaultStyleSheet = new RadioButton();
            this.rbCustomStyleSheet = new RadioButton();
            this.tableLayoutPanel4 = new TableLayoutPanel();
            this.tabPage2 = new TabPage();
            this.groupBox5 = new GroupBox();
            this.panPluginHelp = new FlowLayoutPanel();
            this.llCreatePluginsFolder = new LinkLabel();
            this.label5 = new Label();
            this.myPlugins = new FolderLocationControl();
            this.panSpacer2 = new Panel();
            this.groupBox4 = new GroupBox();
            this.mySnippets = new FolderLocationControl();
            this.panSpacer1 = new Panel();
            this.grpMyQueries = new GroupBox();
            this.myQueries = new FolderLocationControl();
            this.tabPage3 = new TabPage();
            this.btnProxy = new Button();
            this.flowLayoutPanel1 = new FlowLayoutPanel();
            this.label2 = new Label();
            this.tabPage6 = new TabPage();
            this.propertyGrid = new PropertyGrid();
            this.radioButton1 = new RadioButton();
            this.radioButton2 = new RadioButton();
            this.tableLayoutPanel2 = new TableLayoutPanel();
            this.panOKCancel = new TableLayoutPanel();
            this.btnCancel = new Button();
            this.btnOK = new Button();
            this.lblAdvancedRestart = new Label();
            this.fontWorker = new BackgroundWorker();
            this.grpEditorFont.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.grpEditorColor.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.udMaxQueryResults.BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.panPluginHelp.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.grpMyQueries.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 21f));
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.Location = new Point(13, 0x1b);
            this.tableLayoutPanel1.Margin = new Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.Size = new Size(0x15, 0);
            this.tableLayoutPanel1.TabIndex = 0;
            this.rbDefaultFont.AutoSize = true;
            this.rbDefaultFont.Checked = true;
            this.rbDefaultFont.Location = new Point(0x12, 0x19);
            this.rbDefaultFont.Margin = new Padding(3, 2, 3, 2);
            this.rbDefaultFont.Name = "rbDefaultFont";
            this.rbDefaultFont.Size = new Size(0x47, 0x17);
            this.rbDefaultFont.TabIndex = 0;
            this.rbDefaultFont.TabStop = true;
            this.rbDefaultFont.Text = "Default";
            this.rbDefaultFont.UseVisualStyleBackColor = true;
            this.rbDefaultFont.CheckedChanged += new EventHandler(this.rbDefaultFont_CheckedChanged);
            this.rbCustomFont.AutoSize = true;
            this.rbCustomFont.Location = new Point(0x12, 0x31);
            this.rbCustomFont.Margin = new Padding(3, 2, 3, 2);
            this.rbCustomFont.Name = "rbCustomFont";
            this.rbCustomFont.Size = new Size(0x4b, 0x17);
            this.rbCustomFont.TabIndex = 0;
            this.rbCustomFont.Text = "Custom";
            this.rbCustomFont.UseVisualStyleBackColor = true;
            this.rbCustomFont.CheckedChanged += new EventHandler(this.rbCustomFont_CheckedChanged);
            this.cboFont.DrawMode = DrawMode.OwnerDrawFixed;
            this.cboFont.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboFont.FormattingEnabled = true;
            this.cboFont.Location = new Point(0x26, 0x4e);
            this.cboFont.Margin = new Padding(3, 2, 3, 2);
            this.cboFont.Name = "cboFont";
            this.cboFont.Size = new Size(0x146, 0x1a);
            this.cboFont.TabIndex = 1;
            this.cboFont.DrawItem += new DrawItemEventHandler(this.cboFont_DrawItem);
            this.grpEditorFont.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.grpEditorFont.Controls.Add(this.rbDefaultFont);
            this.grpEditorFont.Controls.Add(this.cboFont);
            this.grpEditorFont.Controls.Add(this.rbCustomFont);
            this.grpEditorFont.Controls.Add(this.tableLayoutPanel1);
            this.grpEditorFont.Location = new Point(10, 9);
            this.grpEditorFont.Margin = new Padding(3, 2, 3, 2);
            this.grpEditorFont.Name = "grpEditorFont";
            this.grpEditorFont.Padding = new Padding(3, 2, 10, 2);
            this.grpEditorFont.Size = new Size(0x1bc, 0x7c);
            this.grpEditorFont.TabIndex = 1;
            this.grpEditorFont.TabStop = false;
            this.grpEditorFont.Text = "Editor Font";
            this.label1.AutoSize = true;
            this.label1.Location = new Point(8, 0x8e);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x1be, 0x13);
            this.label1.TabIndex = 3;
            this.label1.Text = "To change the font size, use Ctrl+ScrollWheel or Ctrl+Plus / Ctrl+Minus\r\n";
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage5);
            this.tabControl.Controls.Add(this.tabPage4);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage6);
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.Location = new Point(6, 6);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new Size(0x24d, 470);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new EventHandler(this.tabControl_SelectedIndexChanged);
            this.tabPage1.Controls.Add(this.chkLineNumbers);
            this.tabPage1.Controls.Add(this.grpEditorColor);
            this.tabPage1.Controls.Add(this.grpEditorFont);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new Point(4, 0x1a);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new Padding(3);
            this.tabPage1.Size = new Size(0x245, 440);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Editor";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.chkLineNumbers.AutoSize = true;
            this.chkLineNumbers.Location = new Point(10, 0x131);
            this.chkLineNumbers.Name = "chkLineNumbers";
            this.chkLineNumbers.Size = new Size(0xc7, 0x17);
            this.chkLineNumbers.TabIndex = 5;
            this.chkLineNumbers.Text = "Show line numbers in editor";
            this.chkLineNumbers.UseVisualStyleBackColor = true;
            this.grpEditorColor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.grpEditorColor.Controls.Add(this.btnChooseColor);
            this.grpEditorColor.Controls.Add(this.panColorSample);
            this.grpEditorColor.Controls.Add(this.rbDefaultColor);
            this.grpEditorColor.Controls.Add(this.rbCustomColor);
            this.grpEditorColor.Controls.Add(this.tableLayoutPanel3);
            this.grpEditorColor.Location = new Point(10, 0xae);
            this.grpEditorColor.Margin = new Padding(3, 2, 3, 2);
            this.grpEditorColor.Name = "grpEditorColor";
            this.grpEditorColor.Padding = new Padding(3, 2, 10, 2);
            this.grpEditorColor.Size = new Size(0x1bc, 0x75);
            this.grpEditorColor.TabIndex = 4;
            this.grpEditorColor.TabStop = false;
            this.grpEditorColor.Text = "Editor Background";
            this.btnChooseColor.Location = new Point(0x77, 0x4a);
            this.btnChooseColor.Name = "btnChooseColor";
            this.btnChooseColor.Size = new Size(0x5f, 0x1c);
            this.btnChooseColor.TabIndex = 2;
            this.btnChooseColor.Text = "Choose...";
            this.btnChooseColor.UseVisualStyleBackColor = true;
            this.btnChooseColor.Click += new EventHandler(this.btnChooseColor_Click);
            this.panColorSample.BorderStyle = BorderStyle.Fixed3D;
            this.panColorSample.Location = new Point(0x29, 0x4c);
            this.panColorSample.Name = "panColorSample";
            this.panColorSample.Size = new Size(0x41, 0x1a);
            this.panColorSample.TabIndex = 1;
            this.rbDefaultColor.AutoSize = true;
            this.rbDefaultColor.Checked = true;
            this.rbDefaultColor.Location = new Point(0x12, 0x19);
            this.rbDefaultColor.Margin = new Padding(3, 2, 3, 2);
            this.rbDefaultColor.Name = "rbDefaultColor";
            this.rbDefaultColor.Size = new Size(0xdb, 0x17);
            this.rbDefaultColor.TabIndex = 0;
            this.rbDefaultColor.TabStop = true;
            this.rbDefaultColor.Text = "Default (SystemColors.Window)";
            this.rbDefaultColor.UseVisualStyleBackColor = true;
            this.rbDefaultColor.CheckedChanged += new EventHandler(this.rbDefaultColor_CheckedChanged);
            this.rbDefaultColor.Click += new EventHandler(this.rbDefaultColor_Click);
            this.rbCustomColor.AutoSize = true;
            this.rbCustomColor.Location = new Point(0x12, 0x31);
            this.rbCustomColor.Margin = new Padding(3, 2, 3, 2);
            this.rbCustomColor.Name = "rbCustomColor";
            this.rbCustomColor.Size = new Size(0x4b, 0x17);
            this.rbCustomColor.TabIndex = 0;
            this.rbCustomColor.Text = "Custom";
            this.rbCustomColor.UseVisualStyleBackColor = true;
            this.rbCustomColor.CheckedChanged += new EventHandler(this.rbCustomColor_CheckedChanged);
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 21f));
            this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel3.Location = new Point(13, 0x1b);
            this.tableLayoutPanel3.Margin = new Padding(3, 2, 3, 2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel3.Size = new Size(0x15, 0);
            this.tableLayoutPanel3.TabIndex = 0;
            this.tabPage5.Controls.Add(this.groupBox3);
            this.tabPage5.Controls.Add(this.panel1);
            this.tabPage5.Controls.Add(this.groupBox6);
            this.tabPage5.Controls.Add(this.panel2);
            this.tabPage5.Controls.Add(this.groupBox2);
            this.tabPage5.Location = new Point(4, 0x1a);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new Padding(9, 8, 9, 8);
            this.tabPage5.Size = new Size(0x245, 440);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Query";
            this.tabPage5.UseVisualStyleBackColor = true;
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.chkDisable1To1);
            this.groupBox3.Dock = DockStyle.Top;
            this.groupBox3.Location = new Point(9, 0x112);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new Padding(20, 10, 10, 10);
            this.groupBox3.Size = new Size(0x233, 0x69);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Compatibility";
            this.label4.AutoSize = true;
            this.label4.Dock = DockStyle.Top;
            this.label4.Location = new Point(20, 0x33);
            this.label4.Name = "label4";
            this.label4.Padding = new Padding(12, 0, 0, 0);
            this.label4.Size = new Size(0x135, 0x13);
            this.label4.TabIndex = 1;
            this.label4.Text = " and single-character columns (requires restart)";
            this.chkDisable1To1.AutoSize = true;
            this.chkDisable1To1.Dock = DockStyle.Top;
            this.chkDisable1To1.Location = new Point(20, 0x1c);
            this.chkDisable1To1.Name = "chkDisable1To1";
            this.chkDisable1To1.Size = new Size(0x215, 0x17);
            this.chkDisable1To1.TabIndex = 0;
            this.chkDisable1To1.Text = "Disable enhanced support for 1:1 relationships";
            this.chkDisable1To1.UseVisualStyleBackColor = true;
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(9, 0x10a);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x233, 8);
            this.panel1.TabIndex = 7;
            this.groupBox6.Controls.Add(this.label6);
            this.groupBox6.Controls.Add(this.rbOptimize);
            this.groupBox6.Controls.Add(this.rbNoOptimize);
            this.groupBox6.Dock = DockStyle.Top;
            this.groupBox6.Location = new Point(9, 0x75);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new Padding(20, 10, 10, 10);
            this.groupBox6.Size = new Size(0x233, 0x95);
            this.groupBox6.TabIndex = 8;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Query Optimization";
            this.label6.AutoSize = true;
            this.label6.Location = new Point(0x10, 0x5c);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0x1da, 0x26);
            this.label6.TabIndex = 1;
            this.label6.Text = "Enabling /optimize+ is equivalent to choosing Visual Studio's 'Release' mode\r\nand disables live execution tracking";
            this.rbOptimize.AutoSize = true;
            this.rbOptimize.Location = new Point(20, 0x3b);
            this.rbOptimize.Name = "rbOptimize";
            this.rbOptimize.Size = new Size(0x1af, 0x17);
            this.rbOptimize.TabIndex = 0;
            this.rbOptimize.TabStop = true;
            this.rbOptimize.Text = "Compile with /optimize+ (slightly faster for CPU-intensive queries)";
            this.rbOptimize.UseVisualStyleBackColor = true;
            this.rbOptimize.CheckedChanged += new EventHandler(this.rbOptimize_CheckedChanged);
            this.rbNoOptimize.AutoSize = true;
            this.rbNoOptimize.Location = new Point(20, 0x1f);
            this.rbNoOptimize.Name = "rbNoOptimize";
            this.rbNoOptimize.Size = new Size(0x189, 0x17);
            this.rbNoOptimize.TabIndex = 0;
            this.rbNoOptimize.TabStop = true;
            this.rbNoOptimize.Text = "Compile without /optimize+ (more accurate error reporting)";
            this.rbNoOptimize.UseVisualStyleBackColor = true;
            this.rbNoOptimize.Click += new EventHandler(this.rbNoOptimize_Click);
            this.panel2.Dock = DockStyle.Top;
            this.panel2.Location = new Point(9, 0x6d);
            this.panel2.Name = "panel2";
            this.panel2.Size = new Size(0x233, 8);
            this.panel2.TabIndex = 9;
            this.groupBox2.Controls.Add(this.cboLanguage);
            this.groupBox2.Dock = DockStyle.Top;
            this.groupBox2.Location = new Point(9, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new Size(0x233, 0x65);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Default Query Language";
            this.cboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Items.AddRange(new object[] { "C# Expression", "C# Statement(s)", "C# Program", "VB Expression", "VB Statement(s)", "VB Program", "F# Expression", "F# Program", "SQL", "ESQL" });
            this.cboLanguage.Location = new Point(0x16, 0x25);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new Size(0xfb, 0x19);
            this.cboLanguage.TabIndex = 0;
            this.tabPage4.Controls.Add(this.groupBox7);
            this.tabPage4.Controls.Add(this.udMaxQueryResults);
            this.tabPage4.Controls.Add(this.label3);
            this.tabPage4.Controls.Add(this.groupBox1);
            this.tabPage4.Location = new Point(4, 0x1a);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new Padding(10, 8, 0x13, 8);
            this.tabPage4.Size = new Size(0x245, 440);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Results";
            this.tabPage4.UseVisualStyleBackColor = true;
            this.groupBox7.Controls.Add(this.rbText);
            this.groupBox7.Controls.Add(this.rbGrids);
            this.groupBox7.Controls.Add(this.tableLayoutPanel5);
            this.groupBox7.Location = new Point(12, 10);
            this.groupBox7.Margin = new Padding(3, 2, 3, 2);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new Padding(3, 2, 10, 2);
            this.groupBox7.Size = new Size(0x1bc, 0x55);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Default results destination for new queries:";
            this.rbText.AutoSize = true;
            this.rbText.Checked = true;
            this.rbText.Location = new Point(0x12, 0x18);
            this.rbText.Margin = new Padding(3, 2, 3, 2);
            this.rbText.Name = "rbText";
            this.rbText.Size = new Size(130, 0x17);
            this.rbText.TabIndex = 0;
            this.rbText.TabStop = true;
            this.rbText.Text = "Rich Text (HTML)";
            this.rbText.UseVisualStyleBackColor = true;
            this.rbGrids.AutoSize = true;
            this.rbGrids.Location = new Point(0x12, 0x2f);
            this.rbGrids.Margin = new Padding(3, 2, 3, 2);
            this.rbGrids.Name = "rbGrids";
            this.rbGrids.Size = new Size(0x5c, 0x17);
            this.rbGrids.TabIndex = 1;
            this.rbGrids.Text = "Data Grids";
            this.rbGrids.UseVisualStyleBackColor = true;
            this.tableLayoutPanel5.AutoSize = true;
            this.tableLayoutPanel5.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 21f));
            this.tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel5.Location = new Point(13, 0x1b);
            this.tableLayoutPanel5.Margin = new Padding(3, 2, 3, 2);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel5.Size = new Size(0x15, 0);
            this.tableLayoutPanel5.TabIndex = 0;
            int[] bits = new int[4];
            bits[0] = 0x3e8;
            this.udMaxQueryResults.Increment = new decimal(bits);
            this.udMaxQueryResults.Location = new Point(11, 0x115);
            bits = new int[4];
            bits[0] = 0x2710;
            this.udMaxQueryResults.Maximum = new decimal(bits);
            bits = new int[4];
            bits[0] = 100;
            this.udMaxQueryResults.Minimum = new decimal(bits);
            this.udMaxQueryResults.Name = "udMaxQueryResults";
            this.udMaxQueryResults.Size = new Size(0x6a, 0x19);
            this.udMaxQueryResults.TabIndex = 3;
            bits = new int[4];
            bits[0] = 0x3e8;
            this.udMaxQueryResults.Value = new decimal(bits);
            this.label3.AutoSize = true;
            this.label3.Location = new Point(8, 0xfe);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x158, 0x13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Maximum rows to return in single query (in text mode)";
            this.groupBox1.Controls.Add(this.btnEditStyleSheet);
            this.groupBox1.Controls.Add(this.rbDefaultStyleSheet);
            this.groupBox1.Controls.Add(this.rbCustomStyleSheet);
            this.groupBox1.Controls.Add(this.tableLayoutPanel4);
            this.groupBox1.Location = new Point(10, 0x6f);
            this.groupBox1.Margin = new Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new Padding(3, 2, 10, 2);
            this.groupBox1.Size = new Size(0x1bc, 0x80);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Style sheet for text (HTML) results";
            this.btnEditStyleSheet.Location = new Point(0x25, 0x51);
            this.btnEditStyleSheet.Name = "btnEditStyleSheet";
            this.btnEditStyleSheet.Size = new Size(0x6c, 0x1d);
            this.btnEditStyleSheet.TabIndex = 2;
            this.btnEditStyleSheet.Text = "Launch Editor";
            this.btnEditStyleSheet.UseVisualStyleBackColor = true;
            this.btnEditStyleSheet.Click += new EventHandler(this.btnEditStyleSheet_Click);
            this.rbDefaultStyleSheet.AutoSize = true;
            this.rbDefaultStyleSheet.Checked = true;
            this.rbDefaultStyleSheet.Location = new Point(0x12, 0x1b);
            this.rbDefaultStyleSheet.Margin = new Padding(3, 2, 3, 2);
            this.rbDefaultStyleSheet.Name = "rbDefaultStyleSheet";
            this.rbDefaultStyleSheet.Size = new Size(0x47, 0x17);
            this.rbDefaultStyleSheet.TabIndex = 0;
            this.rbDefaultStyleSheet.TabStop = true;
            this.rbDefaultStyleSheet.Text = "Default";
            this.rbDefaultStyleSheet.UseVisualStyleBackColor = true;
            this.rbDefaultStyleSheet.CheckedChanged += new EventHandler(this.rbStyleSheet_CheckedChanged);
            this.rbCustomStyleSheet.AutoSize = true;
            this.rbCustomStyleSheet.Location = new Point(0x12, 50);
            this.rbCustomStyleSheet.Margin = new Padding(3, 2, 3, 2);
            this.rbCustomStyleSheet.Name = "rbCustomStyleSheet";
            this.rbCustomStyleSheet.Size = new Size(0x4b, 0x17);
            this.rbCustomStyleSheet.TabIndex = 1;
            this.rbCustomStyleSheet.Text = "Custom";
            this.rbCustomStyleSheet.UseVisualStyleBackColor = true;
            this.rbCustomStyleSheet.CheckedChanged += new EventHandler(this.rbStyleSheet_CheckedChanged);
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 21f));
            this.tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel4.Location = new Point(13, 0x1b);
            this.tableLayoutPanel4.Margin = new Padding(3, 2, 3, 2);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel4.Size = new Size(0x15, 0);
            this.tableLayoutPanel4.TabIndex = 0;
            this.tabPage2.Controls.Add(this.groupBox5);
            this.tabPage2.Controls.Add(this.panSpacer2);
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Controls.Add(this.panSpacer1);
            this.tabPage2.Controls.Add(this.grpMyQueries);
            this.tabPage2.Location = new Point(4, 0x1a);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new Padding(9, 8, 9, 8);
            this.tabPage2.Size = new Size(0x245, 440);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Folders";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.groupBox5.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.groupBox5.Controls.Add(this.panPluginHelp);
            this.groupBox5.Controls.Add(this.myPlugins);
            this.groupBox5.Dock = DockStyle.Fill;
            this.groupBox5.Location = new Point(9, 0xec);
            this.groupBox5.Margin = new Padding(5, 2, 3, 2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new Padding(8, 5, 8, 8);
            this.groupBox5.Size = new Size(0x233, 0xc4);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Plugins and Extensions";
            this.panPluginHelp.AutoSize = true;
            this.panPluginHelp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panPluginHelp.Controls.Add(this.llCreatePluginsFolder);
            this.panPluginHelp.Controls.Add(this.label5);
            this.panPluginHelp.Dock = DockStyle.Top;
            this.panPluginHelp.Location = new Point(8, 0x62);
            this.panPluginHelp.Margin = new Padding(0, 3, 0, 3);
            this.panPluginHelp.Name = "panPluginHelp";
            this.panPluginHelp.Padding = new Padding(0, 4, 0, 0);
            this.panPluginHelp.Size = new Size(0x223, 0x2d);
            this.panPluginHelp.TabIndex = 6;
            this.llCreatePluginsFolder.AutoSize = true;
            this.llCreatePluginsFolder.Location = new Point(0, 4);
            this.llCreatePluginsFolder.Margin = new Padding(0);
            this.llCreatePluginsFolder.Name = "llCreatePluginsFolder";
            this.llCreatePluginsFolder.Size = new Size(0x8b, 0x13);
            this.llCreatePluginsFolder.TabIndex = 1;
            this.llCreatePluginsFolder.TabStop = true;
            this.llCreatePluginsFolder.Text = "Create Plugins Folder";
            this.llCreatePluginsFolder.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llCreatePluginsFolder_LinkClicked);
            this.label5.AutoSize = true;
            this.label5.Location = new Point(0, 0x17);
            this.label5.Margin = new Padding(0);
            this.label5.Name = "label5";
            this.label5.Padding = new Padding(0, 3, 0, 0);
            this.label5.Size = new Size(0x1c3, 0x16);
            this.label5.TabIndex = 0;
            this.label5.Text = "Assemblies that you put here are referenced automatically by all queries.";
            this.myPlugins.AutoSize = true;
            this.myPlugins.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.myPlugins.DefaultLocationText = @"My Documents\LINQPad Plugins";
            this.myPlugins.Dock = DockStyle.Top;
            this.myPlugins.FolderText = null;
            this.myPlugins.Location = new Point(8, 0x17);
            this.myPlugins.Margin = new Padding(2, 2, 2, 2);
            this.myPlugins.MRU = null;
            this.myPlugins.Name = "myPlugins";
            this.myPlugins.Padding = new Padding(2, 0, 0, 0);
            this.myPlugins.Size = new Size(0x223, 0x4b);
            this.myPlugins.TabIndex = 5;
            this.panSpacer2.Dock = DockStyle.Top;
            this.panSpacer2.Location = new Point(9, 0xe4);
            this.panSpacer2.Name = "panSpacer2";
            this.panSpacer2.Size = new Size(0x233, 8);
            this.panSpacer2.TabIndex = 7;
            this.groupBox4.AutoSize = true;
            this.groupBox4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.groupBox4.Controls.Add(this.mySnippets);
            this.groupBox4.Dock = DockStyle.Top;
            this.groupBox4.Location = new Point(9, 0x7a);
            this.groupBox4.Margin = new Padding(5, 2, 3, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new Padding(8, 5, 8, 8);
            this.groupBox4.Size = new Size(0x233, 0x6a);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "My Snippets";
            this.mySnippets.AutoSize = true;
            this.mySnippets.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.mySnippets.DefaultLocationText = @"My Documents\LINQPad Snippets";
            this.mySnippets.Dock = DockStyle.Top;
            this.mySnippets.FolderText = null;
            this.mySnippets.Location = new Point(8, 0x17);
            this.mySnippets.Margin = new Padding(2, 2, 2, 2);
            this.mySnippets.MRU = null;
            this.mySnippets.Name = "mySnippets";
            this.mySnippets.Padding = new Padding(2, 0, 0, 0);
            this.mySnippets.Size = new Size(0x223, 0x4b);
            this.mySnippets.TabIndex = 5;
            this.panSpacer1.Dock = DockStyle.Top;
            this.panSpacer1.Location = new Point(9, 0x72);
            this.panSpacer1.Name = "panSpacer1";
            this.panSpacer1.Size = new Size(0x233, 8);
            this.panSpacer1.TabIndex = 6;
            this.grpMyQueries.AutoSize = true;
            this.grpMyQueries.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.grpMyQueries.Controls.Add(this.myQueries);
            this.grpMyQueries.Dock = DockStyle.Top;
            this.grpMyQueries.Location = new Point(9, 8);
            this.grpMyQueries.Margin = new Padding(5, 2, 3, 2);
            this.grpMyQueries.Name = "grpMyQueries";
            this.grpMyQueries.Padding = new Padding(8, 5, 8, 8);
            this.grpMyQueries.Size = new Size(0x233, 0x6a);
            this.grpMyQueries.TabIndex = 0;
            this.grpMyQueries.TabStop = false;
            this.grpMyQueries.Text = "My Queries";
            this.myQueries.AutoSize = true;
            this.myQueries.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.myQueries.DefaultLocationText = @"My Documents\LINQPad Queries";
            this.myQueries.Dock = DockStyle.Top;
            this.myQueries.FolderText = null;
            this.myQueries.Location = new Point(8, 0x17);
            this.myQueries.Margin = new Padding(2, 2, 2, 2);
            this.myQueries.MRU = null;
            this.myQueries.Name = "myQueries";
            this.myQueries.Padding = new Padding(2, 0, 0, 0);
            this.myQueries.Size = new Size(0x223, 0x4b);
            this.myQueries.TabIndex = 5;
            this.tabPage3.Controls.Add(this.btnProxy);
            this.tabPage3.Controls.Add(this.flowLayoutPanel1);
            this.tabPage3.Location = new Point(4, 0x1a);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new Padding(9, 11, 9, 11);
            this.tabPage3.Size = new Size(0x245, 440);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Updates";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.btnProxy.Dock = DockStyle.Top;
            this.btnProxy.Location = new Point(9, 0x3b);
            this.btnProxy.Name = "btnProxy";
            this.btnProxy.Size = new Size(0x233, 0x1d);
            this.btnProxy.TabIndex = 2;
            this.btnProxy.Text = "Specify Web Proxy Details...";
            this.btnProxy.UseVisualStyleBackColor = true;
            this.btnProxy.Click += new EventHandler(this.btnProxy_Click);
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Dock = DockStyle.Top;
            this.flowLayoutPanel1.Location = new Point(9, 11);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new Size(0x233, 0x30);
            this.flowLayoutPanel1.TabIndex = 3;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(3, 0);
            this.label2.Margin = new Padding(3, 0, 3, 10);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x1ec, 0x26);
            this.label2.TabIndex = 1;
            this.label2.Text = "LINQPad updates itself automatically when a newer version becomes available. \r\nYou can specify a web proxy if your intranet requires it.";
            this.tabPage6.Controls.Add(this.propertyGrid);
            this.tabPage6.Location = new Point(4, 0x1a);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new Padding(3);
            this.tabPage6.Size = new Size(0x245, 440);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Advanced";
            this.tabPage6.UseVisualStyleBackColor = true;
            this.propertyGrid.Dock = DockStyle.Fill;
            this.propertyGrid.Location = new Point(3, 3);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new Size(0x23f, 0x1b2);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new Point(0x1b, 0x1a);
            this.radioButton1.Margin = new Padding(3, 2, 3, 2);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new Size(0x4a, 0x15);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Default";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new Point(0x1b, 0x34);
            this.radioButton2.Margin = new Padding(3, 2, 3, 2);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new Size(0x4c, 0x15);
            this.radioButton2.TabIndex = 0;
            this.radioButton2.Text = "Custom";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24f));
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel2.Location = new Point(15, 0x19);
            this.tableLayoutPanel2.Margin = new Padding(3, 2, 3, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel2.Size = new Size(0x18, 0);
            this.tableLayoutPanel2.TabIndex = 0;
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnCancel, 2, 0);
            this.panOKCancel.Controls.Add(this.btnOK, 1, 0);
            this.panOKCancel.Controls.Add(this.lblAdvancedRestart, 0, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(6, 0x1dc);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 4, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x24d, 0x27);
            this.panOKCancel.TabIndex = 2;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x1fd, 7);
            this.btnCancel.Margin = new Padding(3, 3, 1, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0x1a8, 7);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 0x1d);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.lblAdvancedRestart.AutoSize = true;
            this.lblAdvancedRestart.Dock = DockStyle.Fill;
            this.lblAdvancedRestart.ForeColor = Color.FromArgb(0xc0, 0, 0);
            this.lblAdvancedRestart.Location = new Point(3, 4);
            this.lblAdvancedRestart.Name = "lblAdvancedRestart";
            this.lblAdvancedRestart.Size = new Size(0x19f, 0x23);
            this.lblAdvancedRestart.TabIndex = 3;
            this.lblAdvancedRestart.Text = "Changing advanced options may require a restart";
            this.lblAdvancedRestart.TextAlign = ContentAlignment.MiddleLeft;
            this.lblAdvancedRestart.Visible = false;
            this.fontWorker.DoWork += new DoWorkEventHandler(this.fontWorker_DoWork);
            this.fontWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.fontWorker_RunWorkerCompleted);
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(600, 520);
            base.ControlBox = false;
            base.Controls.Add(this.tabControl);
            base.Controls.Add(this.panOKCancel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Location = new Point(0, 0);
            base.Margin = new Padding(3, 2, 3, 2);
            base.Name = "OptionsForm";
            base.Padding = new Padding(6, 6, 5, 5);
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Preferences";
            this.grpEditorFont.ResumeLayout(false);
            this.grpEditorFont.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.grpEditorColor.ResumeLayout(false);
            this.grpEditorColor.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.udMaxQueryResults.EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.panPluginHelp.ResumeLayout(false);
            this.panPluginHelp.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.grpMyQueries.ResumeLayout(false);
            this.grpMyQueries.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.panOKCancel.ResumeLayout(false);
            this.panOKCancel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void llCreatePluginsFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = this.myPlugins.IsDefaultLocation ? UserOptions.Instance.GetDefaultPluginsFolder() : this.myPlugins.FolderText;
            if (Directory.Exists(path))
            {
                MessageBox.Show("Folder '" + path + "' already exists.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(path);
                    MessageBox.Show("Done", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Error creating folder '" + path + "':\r\n" + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (base.DialogResult == DialogResult.OK)
            {
                if (this._populatedFonts)
                {
                    if (this.rbDefaultFont.Checked)
                    {
                        if (File.Exists(this._fontPath))
                        {
                            File.Delete(this._fontPath);
                        }
                    }
                    else if ((this.cboFont.SelectedItem != null) && (this.cboFont.SelectedItem.ToString().Length > 0))
                    {
                        if (!Directory.Exists(Program.UserDataFolder))
                        {
                            Directory.CreateDirectory(Program.UserDataFolder);
                        }
                        File.WriteAllText(this._fontPath, this.cboFont.SelectedItem.ToString());
                    }
                }
                if (this.myQueries.IsDefaultLocation || string.IsNullOrEmpty(this.myQueries.FolderText))
                {
                    if (File.Exists(this._queriesPath))
                    {
                        File.Delete(this._queriesPath);
                    }
                }
                else
                {
                    if (!Directory.Exists(Program.UserDataFolder))
                    {
                        Directory.CreateDirectory(Program.UserDataFolder);
                    }
                    File.WriteAllText(this._queriesPath, this.myQueries.FolderText);
                    this.myQueries.SaveMRU();
                }
                if (this.rbDefaultStyleSheet.Checked)
                {
                    this.DeleteStyleSheet();
                }
                UserOptions.Instance.ShowLineNumbersInEditor = this.chkLineNumbers.Checked;
                this._advancedOptions.Write();
                this._userOptions.MaxQueryRows = new int?((int) this.udMaxQueryResults.Value);
                this._userOptions.DefaultQueryLanguage = (this.cboLanguage.SelectedIndex < 0) ? null : new QueryLanguage?((QueryLanguage) this.cboLanguage.SelectedIndex);
                this._userOptions.CustomSnippetsFolder = this.mySnippets.IsDefaultLocation ? null : this.mySnippets.FolderText;
                if (!this.mySnippets.IsDefaultLocation)
                {
                    this.mySnippets.SaveMRU();
                }
                this._userOptions.PluginsFolder = this.myPlugins.IsDefaultLocation ? null : this.myPlugins.FolderText;
                if (!this.myPlugins.IsDefaultLocation)
                {
                    this.myPlugins.SaveMRU();
                }
                this._userOptions.ResultsInGrids = this.rbGrids.Checked;
                this._userOptions.EditorBackColor = this.rbCustomColor.Checked ? ColorTranslator.ToHtml(this.panColorSample.BackColor) : null;
                this._userOptions.Save();
                MainForm.Instance.OptimizeQueries = this.rbOptimize.Checked;
                MainForm.Instance.CheckForQueryFolderChange();
                MainForm.Instance.CheckForMaxQueryCountChange();
                SnippetManager.set_LINQPadSnippetsFolder(UserOptions.Instance.GetCustomSnippetsFolder(false));
                AutocompletionManager.PassiveAutocompletion = UserOptions.Instance.PassiveAutocompletion;
                AutocompletionManager.DisableLambdaSnippets = UserOptions.Instance.DisableLambdaSnippets;
                Program.CreateAppDataFolder();
                try
                {
                    File.WriteAllText(Program.OneToOneAckFile, !this.chkDisable1To1.Checked.ToString());
                }
                catch
                {
                }
                if (this._oldEditorBackColor != UserOptions.Instance.ActualEditorBackColor)
                {
                    DocumentManager.ResetWindowBrightness();
                    MainForm.Instance.EditorBackColorChanged();
                }
                MainForm.Instance.ForceQueryRecompilations();
            }
            base.Dispose();
            foreach (Font font in this._fonts)
            {
                font.Dispose();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void rbCustomColor_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbCustomFont_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbCustomSnippetLocation_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbDefaultColor_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbDefaultColor_Click(object sender, EventArgs e)
        {
            this.panColorSample.BackColor = SystemColors.Window;
        }

        private void rbDefaultFont_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbDefaultSnippetLocation_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbNoOptimize_Click(object sender, EventArgs e)
        {
        }

        private void rbOptimize_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void rbQueryLocation_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbStyleSheet_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lblAdvancedRestart.Visible = this.tabControl.SelectedIndex == 5;
            if ((this.tabControl.SelectedIndex != 1) && (this.tabControl.SelectedIndex == 5))
            {
                this.propertyGrid.SelectedObject = null;
                this.propertyGrid.SelectedObject = this._advancedOptions;
            }
        }

        private class AdvancedOptions
        {
            private uint? _maxColumnWidthInLists;
            private byte? _tabSize;

            public void Read()
            {
                UserOptions instance = UserOptions.Instance;
                this.PresentationMode = instance.PresentationMode;
                this.OpenMyQueriesInNewTab = instance.OpenMyQueriesInNewTab;
                this.MTAThreadingMode = instance.MTAThreadingMode;
                this.PreserveAppDomains = instance.PreserveAppDomains;
                this.FreshAppDomains = instance.FreshAppDomains;
                this.LockReferenceAssemblies = instance.LockReferenceAssemblies;
                this.MARS = instance.MARS;
                this.MaxColumnWidthInLists = instance.MaxColumnWidthInLists;
                this.TabSize = instance.TabSize;
                this.ConvertTabsToSpaces = instance.ConvertTabsToSpaces;
                this.UseVisualStudioHotKeys = !instance.NativeHotKeys;
                this.CompileVBInStrictMode = instance.CompileVBInStrictMode;
                this.SqlPasswordExpiryPrompts = !instance.NoSqlPasswordExpiryPrompts;
                this.OpenMyQueriesOnSingleClick = !instance.DoubleClickToOpenMyQueries;
                this.ShowCompletionListOnCharPress = !instance.PassiveAutocompletion;
                this.EnableLambdaSnippets = !instance.DisableLambdaSnippets;
            }

            public void Write()
            {
                UserOptions instance = UserOptions.Instance;
                instance.PresentationMode = this.PresentationMode;
                instance.OpenMyQueriesInNewTab = this.OpenMyQueriesInNewTab;
                instance.MTAThreadingMode = this.MTAThreadingMode;
                instance.PreserveAppDomains = this.PreserveAppDomains;
                instance.FreshAppDomains = this.FreshAppDomains;
                instance.LockReferenceAssemblies = this.LockReferenceAssemblies;
                instance.MARS = this.MARS;
                instance.MaxColumnWidthInLists = this.MaxColumnWidthInLists;
                instance.TabSize = this.TabSize;
                instance.ConvertTabsToSpaces = this.ConvertTabsToSpaces;
                instance.CompileVBInStrictMode = this.CompileVBInStrictMode;
                instance.DoubleClickToOpenMyQueries = !this.OpenMyQueriesOnSingleClick;
                instance.NoSqlPasswordExpiryPrompts = !this.SqlPasswordExpiryPrompts;
                if (instance.NativeHotKeys == this.UseVisualStudioHotKeys)
                {
                    instance.NativeHotKeys = !this.UseVisualStudioHotKeys;
                    MainForm.Instance.UpdateHotkeys();
                }
                instance.PassiveAutocompletion = !this.ShowCompletionListOnCharPress;
                instance.DisableLambdaSnippets = !this.EnableLambdaSnippets;
            }

            [Category("Execution"), Description("Apply /optionstrict when compiling VB queries"), DefaultValue(false), DisplayName("Compile VB queries in strict mode")]
            public bool CompileVBInStrictMode { get; set; }

            [Category("Editor"), DefaultValue(false), Description("If true, spaces will be used for indentation instead of tabs"), DisplayName("Convert tabs to spaces")]
            public bool ConvertTabsToSpaces { get; set; }

            [Description("If true, pressing tab from a query operator member listing inserts a lambda expression"), Category("Autocompletion"), DefaultValue(true), DisplayName("Enable lambda snippets on query operators")]
            public bool EnableLambdaSnippets { get; set; }

            [DisplayName("Always use Fresh Application Domains"), DefaultValue(false), Description("Forces LINQPad to create a fresh application domain every time you re-run the query (slightly slower)"), Category("Execution")]
            public bool FreshAppDomains { get; set; }

            [Category("Execution"), DefaultValue(false), Description("Tells LINQPad to load custom assembly references directly with Assembly.LoadFrom(string) rather than copying them first to a temporary folder"), DisplayName("Do not shadow assembly references")]
            public bool LockReferenceAssemblies { get; set; }

            [DefaultValue(false), DisplayName("Use MARS with database connections"), Description("Enables MARS (Multiple Active Result Sets)"), Category("Execution")]
            public bool MARS { get; set; }

            [Category("Output"), Description("Maximum characters to display in a single column when displaying lists (blank is unlimited)"), DisplayName("Maximum Column Width in Lists"), DefaultValue((string) null)]
            public uint? MaxColumnWidthInLists
            {
                get
                {
                    return this._maxColumnWidthInLists;
                }
                set
                {
                    this._maxColumnWidthInLists = value;
                    if (this._maxColumnWidthInLists.HasValue)
                    {
                        if (this._maxColumnWidthInLists == 0)
                        {
                            this._maxColumnWidthInLists = null;
                        }
                        else if (this._maxColumnWidthInLists < 20)
                        {
                            this._maxColumnWidthInLists = 20;
                        }
                    }
                }
            }

            [DisplayName("Run Queries in MTA Threads"), Description("Runs query threads in multithreaded apartment mode"), DefaultValue(false), Category("Execution")]
            public bool MTAThreadingMode { get; set; }

            [Description("If true, clicking a query in 'My Queries' always opens it in a new tab"), DefaultValue(false), Category("General"), DisplayName("Always open 'My Queries' in new tab")]
            public bool OpenMyQueriesInNewTab { get; set; }

            [DefaultValue(true), Description("If false, you must double-click to open queries in the 'My Queries' TreeView"), Category("General"), DisplayName("Open 'My Queries' on single click")]
            public bool OpenMyQueriesOnSingleClick { get; set; }

            [Category("General"), Description("For use with Logitech Cordless Presenter - see www.linqpad.net/FAQ.aspx"), DisplayName("Enable Presentation Mode"), DefaultValue(false)]
            public bool PresentationMode { get; set; }

            [Category("Execution"), Description("Prevents a query's application domain recycling when it errors or exceeds a 20MB footprint"), DefaultValue(false), DisplayName("Always Preserve Application Domains")]
            public bool PreserveAppDomains { get; set; }

            [DisplayName("Show completion list after typing a letter"), DefaultValue(true), Description("If true, brings up the autocompletion list as soon as a letter is typed"), Category("Autocompletion")]
            public bool ShowCompletionListOnCharPress { get; set; }

            [DefaultValue(true), DisplayName("Prompt for new SQL passwords on expiry"), Description("Whether to prompt for a password change when a SQL password expires"), Category("General")]
            public bool SqlPasswordExpiryPrompts { get; set; }

            [DisplayName("Tab size"), DefaultValue((string) null), Category("Editor"), Description("Number of tab characters (2-15) or blank for default")]
            public byte? TabSize
            {
                get
                {
                    return this._tabSize;
                }
                set
                {
                    this._tabSize = value;
                    if (this._tabSize.HasValue)
                    {
                        if (this._tabSize < 2)
                        {
                            this._tabSize = 2;
                        }
                        if (this._tabSize > 15)
                        {
                            this._tabSize = 15;
                        }
                    }
                }
            }

            [DisplayName("Use Visual Studio shortcut keys"), Description("Whether to use Visual Studio's chord-style hotkeys for commenting/autocompletion"), Category("Editor"), DefaultValue(true)]
            public bool UseVisualStudioHotKeys { get; set; }
        }
    }
}

