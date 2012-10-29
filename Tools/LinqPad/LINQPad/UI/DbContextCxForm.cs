namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class DbContextCxForm : BaseForm
    {
        private IConnectionInfo _cxInfo;
        private bool _parameterlessCtorOK;
        private bool _stringCtrlOK;
        private Button btnCancel;
        private Button btnOK;
        private Button btnTest;
        private CheckBox chkEncryptCustomCxString;
        private CheckBox chkRemember;
        private IContainer components = null;
        private GroupBox groupBox1;
        private GroupBox grpStringConstructor;
        private Label label1;
        private Label label2;
        private Label label5;
        private Label lblAppConfigWarning;
        private Label lblCustomTypeName;
        private Label lblTypeMessage;
        private LinkLabel llBrowseAssembly;
        private LinkLabel llChooseAppConfigPath;
        private LinkLabel llChooseTypeName;
        private FlowLayoutPanel panAppConfigWarning;
        private TableLayoutPanel panCustomDC;
        private Panel panel1;
        private Panel panel2;
        private Panel panel4;
        private Panel panOKCancel;
        private RadioButton rbParameterlessConstructor;
        private RadioButton rbStringConstructor;
        private TableLayoutPanel tableLayoutPanel1;
        private Timer tmrCheckType;
        private TextBox txtAppConfigPath;
        private TextBox txtAssemblyPath;
        private TextBox txtCxName;
        private TextBox txtCxString;
        private TextBox txtTypeName;

        public DbContextCxForm(IConnectionInfo cxInfo)
        {
            this._cxInfo = cxInfo;
            this.InitializeComponent();
            try
            {
                this.lblAppConfigWarning.Font = this.lblTypeMessage.Font = new Font(this.lblAppConfigWarning.Font, FontStyle.Bold);
            }
            catch
            {
            }
            this._stringCtrlOK = true;
            this._parameterlessCtorOK = true;
            this.PopulateFromRepository();
            this.EnableControls();
        }

        private void BrowseAssembly()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose Custom Assembly";
                string str = this.txtAssemblyPath.Text.Trim();
                if (str.Length > 0)
                {
                    dialog.FileName = str;
                }
                dialog.DefaultExt = ".dll";
                dialog.Filter = "Assembly files|*.dll;*.exe";
                if ((dialog.ShowDialog() == DialogResult.OK) && (dialog.FileName != str))
                {
                    this.txtAssemblyPath.Text = dialog.FileName;
                    if (str.Length == 0)
                    {
                        this.ChooseTypeName();
                    }
                }
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            BackgroundWorker worker2 = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };
            string arg = this.rbStringConstructor.Checked ? this.txtCxString.Text.Trim() : null;
            Repository testRepos = ((Repository) this._cxInfo).Clone();
            this.UpdateRepository(testRepos);
            worker2.DoWork += delegate (object sender, DoWorkEventArgs e) {
                using (DomainIsolator isolator = this.GetDomainIsolatorForProber("EF Connection Tester", true))
                {
                    isolator.GetInstance<AssemblyProber>().Test(testRepos, arg);
                }
            };
            using (WorkerForm form = new WorkerForm(worker2, "Testing...", true))
            {
                if (form.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }
            MessageBox.Show("Successful", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void ChooseTypeName()
        {
            if (!File.Exists(this.txtAssemblyPath.Text.Trim()))
            {
                MessageBox.Show("The assembly '" + this.txtAssemblyPath.Text.Trim() + "' does not exist.");
            }
            else
            {
                string[] customTypeNames;
                try
                {
                    customTypeNames = this.GetCustomTypeNames();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + ((exception.InnerException == null) ? "" : ("\r\n" + exception.InnerException.Message)));
                    return;
                }
                if (customTypeNames.Length == 0)
                {
                    MessageBox.Show("There are no types based on System.Data.Entity.DbContext in that assembly.");
                }
                else
                {
                    using (ChooseTypeForm form = new ChooseTypeForm(this.txtAssemblyPath.Text.Trim(), customTypeNames, this.txtTypeName.Text.Trim(), null, null))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            this.txtTypeName.Text = form.SelectedTypeName;
                            this.ProbeType();
                        }
                    }
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
            this.rbParameterlessConstructor.Enabled = this._parameterlessCtorOK;
            this.rbStringConstructor.Enabled = this._stringCtrlOK;
            if (!(this.rbStringConstructor.Enabled || !this.rbParameterlessConstructor.Enabled))
            {
                this.rbParameterlessConstructor.Checked = true;
            }
            else if (!(this.rbParameterlessConstructor.Enabled || !this.rbStringConstructor.Enabled))
            {
                this.rbStringConstructor.Checked = true;
            }
            if (!(this.rbStringConstructor.Enabled || this.rbParameterlessConstructor.Enabled))
            {
                this.rbStringConstructor.Checked = false;
                this.rbParameterlessConstructor.Checked = false;
            }
            this.lblAppConfigWarning.Enabled = this.rbParameterlessConstructor.Checked;
            this.grpStringConstructor.Enabled = this.txtCxString.Enabled = this.chkEncryptCustomCxString.Enabled = this.rbStringConstructor.Checked;
            this.btnOK.Enabled = this.btnTest.Enabled = this.IsDataValid();
        }

        private void EnableControls(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void GetConstructorInfo(out bool parameterlessCtor, out string stringCtorParamName)
        {
            using (DomainIsolator isolator = this.GetDomainIsolatorForProber("EF Assembly Prober", false))
            {
                isolator.GetInstance<AssemblyProber>().GetConstructorInfo(this.txtAssemblyPath.Text.Trim(), this.txtTypeName.Text.Trim(), out parameterlessCtor, out stringCtorParamName);
            }
        }

        private string[] GetCustomTypeNames()
        {
            using (DomainIsolator isolator = this.GetDomainIsolatorForProber("EF Assembly Prober", false))
            {
                return isolator.GetInstance<AssemblyProber>().GetCustomTypeNames(this.txtAssemblyPath.Text.Trim());
            }
        }

        private DomainIsolator GetDomainIsolatorForProber(string name, bool validateApConfig)
        {
            string path = this.txtAppConfigPath.Text.Trim();
            if ((path.Length > 0) && !File.Exists(path))
            {
                if (validateApConfig)
                {
                    throw new Exception("Application config file does not exist at that location.");
                }
                path = "";
            }
            return new DomainIsolator(name, path, null);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.panOKCancel = new Panel();
            this.btnTest = new Button();
            this.btnOK = new Button();
            this.panel4 = new Panel();
            this.btnCancel = new Button();
            this.panel1 = new Panel();
            this.chkRemember = new CheckBox();
            this.panCustomDC = new TableLayoutPanel();
            this.label2 = new Label();
            this.lblCustomTypeName = new Label();
            this.txtAssemblyPath = new TextBox();
            this.txtTypeName = new TextBox();
            this.llBrowseAssembly = new LinkLabel();
            this.llChooseTypeName = new LinkLabel();
            this.lblTypeMessage = new Label();
            this.lblAppConfigWarning = new Label();
            this.groupBox1 = new GroupBox();
            this.panel2 = new Panel();
            this.grpStringConstructor = new GroupBox();
            this.txtCxString = new TextBox();
            this.chkEncryptCustomCxString = new CheckBox();
            this.rbStringConstructor = new RadioButton();
            this.panAppConfigWarning = new FlowLayoutPanel();
            this.rbParameterlessConstructor = new RadioButton();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.txtCxName = new TextBox();
            this.label1 = new Label();
            this.label5 = new Label();
            this.txtAppConfigPath = new TextBox();
            this.llChooseAppConfigPath = new LinkLabel();
            this.tmrCheckType = new Timer(this.components);
            this.panOKCancel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panCustomDC.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.grpStringConstructor.SuspendLayout();
            this.panAppConfigWarning.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.Controls.Add(this.btnTest);
            this.panOKCancel.Controls.Add(this.btnOK);
            this.panOKCancel.Controls.Add(this.panel4);
            this.panOKCancel.Controls.Add(this.btnCancel);
            this.panOKCancel.Dock = DockStyle.Top;
            this.panOKCancel.Location = new Point(10, 0x24c);
            this.panOKCancel.Margin = new Padding(4, 5, 4, 0);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 6, 0, 0);
            this.panOKCancel.Size = new Size(0x217, 0x23);
            this.panOKCancel.TabIndex = 4;
            this.btnTest.Dock = DockStyle.Left;
            this.btnTest.Location = new Point(0, 6);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new Size(0x55, 0x1d);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "&Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new EventHandler(this.btnTest_Click);
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Dock = DockStyle.Right;
            this.btnOK.Location = new Point(0x167, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x55, 0x1d);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.panel4.Dock = DockStyle.Right;
            this.panel4.Location = new Point(0x1bc, 6);
            this.panel4.Margin = new Padding(4, 5, 4, 5);
            this.panel4.Name = "panel4";
            this.panel4.Size = new Size(6, 0x1d);
            this.panel4.TabIndex = 2;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Dock = DockStyle.Right;
            this.btnCancel.Location = new Point(450, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x55, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.panel1.Controls.Add(this.chkRemember);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(10, 0x21d);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x217, 0x2f);
            this.panel1.TabIndex = 3;
            this.chkRemember.AutoSize = true;
            this.chkRemember.Dock = DockStyle.Right;
            this.chkRemember.Location = new Point(0x158, 0);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Padding = new Padding(0, 0x11, 0, 0);
            this.chkRemember.Size = new Size(0xbf, 0x2f);
            this.chkRemember.TabIndex = 7;
            this.chkRemember.Text = "&Remember this connection";
            this.chkRemember.UseVisualStyleBackColor = true;
            this.panCustomDC.AutoSize = true;
            this.panCustomDC.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panCustomDC.ColumnCount = 2;
            this.panCustomDC.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panCustomDC.ColumnStyles.Add(new ColumnStyle());
            this.panCustomDC.Controls.Add(this.label2, 0, 0);
            this.panCustomDC.Controls.Add(this.lblCustomTypeName, 0, 2);
            this.panCustomDC.Controls.Add(this.txtAssemblyPath, 0, 1);
            this.panCustomDC.Controls.Add(this.txtTypeName, 0, 3);
            this.panCustomDC.Controls.Add(this.llBrowseAssembly, 1, 0);
            this.panCustomDC.Controls.Add(this.llChooseTypeName, 1, 2);
            this.panCustomDC.Controls.Add(this.lblTypeMessage, 0, 4);
            this.panCustomDC.Dock = DockStyle.Top;
            this.panCustomDC.Location = new Point(10, 10);
            this.panCustomDC.Name = "panCustomDC";
            this.panCustomDC.Padding = new Padding(0, 0, 0, 7);
            this.panCustomDC.RowCount = 5;
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.Size = new Size(0x217, 0x89);
            this.panCustomDC.TabIndex = 0;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0, 0);
            this.label2.Margin = new Padding(0, 0, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0xa8, 0x13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Pat&h to Custom Assembly";
            this.lblCustomTypeName.AutoSize = true;
            this.lblCustomTypeName.Location = new Point(0, 0x35);
            this.lblCustomTypeName.Margin = new Padding(0, 0, 3, 0);
            this.lblCustomTypeName.Name = "lblCustomTypeName";
            this.lblCustomTypeName.Size = new Size(230, 0x13);
            this.lblCustomTypeName.TabIndex = 3;
            this.lblCustomTypeName.Text = "&Full Type Name of Typed DbContext";
            this.txtAssemblyPath.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panCustomDC.SetColumnSpan(this.txtAssemblyPath, 2);
            this.txtAssemblyPath.Location = new Point(1, 20);
            this.txtAssemblyPath.Margin = new Padding(1, 1, 0, 8);
            this.txtAssemblyPath.Name = "txtAssemblyPath";
            this.txtAssemblyPath.Size = new Size(0x216, 0x19);
            this.txtAssemblyPath.TabIndex = 1;
            this.txtAssemblyPath.TextChanged += new EventHandler(this.txtAssemblyPath_TextChanged);
            this.txtTypeName.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panCustomDC.SetColumnSpan(this.txtTypeName, 2);
            this.txtTypeName.Location = new Point(1, 0x49);
            this.txtTypeName.Margin = new Padding(1, 1, 0, 8);
            this.txtTypeName.Name = "txtTypeName";
            this.txtTypeName.Size = new Size(0x216, 0x19);
            this.txtTypeName.TabIndex = 4;
            this.txtTypeName.TextChanged += new EventHandler(this.txtTypeName_TextChanged);
            this.llBrowseAssembly.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.llBrowseAssembly.AutoSize = true;
            this.llBrowseAssembly.Location = new Point(0x1e2, 0);
            this.llBrowseAssembly.Margin = new Padding(0);
            this.llBrowseAssembly.Name = "llBrowseAssembly";
            this.llBrowseAssembly.Size = new Size(0x35, 0x13);
            this.llBrowseAssembly.TabIndex = 2;
            this.llBrowseAssembly.TabStop = true;
            this.llBrowseAssembly.Text = "Browse";
            this.llBrowseAssembly.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llBrowseAssembly_LinkClicked);
            this.llChooseTypeName.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.llChooseTypeName.AutoSize = true;
            this.llChooseTypeName.Location = new Point(480, 0x35);
            this.llChooseTypeName.Margin = new Padding(0);
            this.llChooseTypeName.Name = "llChooseTypeName";
            this.llChooseTypeName.Size = new Size(0x37, 0x13);
            this.llChooseTypeName.TabIndex = 5;
            this.llChooseTypeName.TabStop = true;
            this.llChooseTypeName.Text = "Choose";
            this.llChooseTypeName.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llChooseTypeName_LinkClicked);
            this.lblTypeMessage.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.lblTypeMessage.AutoSize = true;
            this.panCustomDC.SetColumnSpan(this.lblTypeMessage, 2);
            this.lblTypeMessage.ForeColor = Color.FromArgb(220, 30, 30);
            this.lblTypeMessage.Location = new Point(10, 0x6a);
            this.lblTypeMessage.Margin = new Padding(10, 0, 10, 5);
            this.lblTypeMessage.Name = "lblTypeMessage";
            this.lblTypeMessage.Size = new Size(0x203, 0x13);
            this.lblTypeMessage.TabIndex = 6;
            this.lblTypeMessage.Text = "      ";
            this.lblTypeMessage.Visible = false;
            this.lblAppConfigWarning.AutoSize = true;
            this.lblAppConfigWarning.Location = new Point(3, 0);
            this.lblAppConfigWarning.Name = "lblAppConfigWarning";
            this.lblAppConfigWarning.Padding = new Padding(20, 2, 0, 0);
            this.lblAppConfigWarning.Size = new Size(0x1f3, 40);
            this.lblAppConfigWarning.TabIndex = 0;
            this.lblAppConfigWarning.Text = "If your connection string is stored in an application config file, make sure you specify a path to it in the textbox below.";
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.rbStringConstructor);
            this.groupBox1.Controls.Add(this.panAppConfigWarning);
            this.groupBox1.Controls.Add(this.rbParameterlessConstructor);
            this.groupBox1.Dock = DockStyle.Top;
            this.groupBox1.Location = new Point(10, 0x93);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new Padding(20, 5, 10, 10);
            this.groupBox1.Size = new Size(0x217, 0x117);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "How should LINQPad instantiate your DbContext?";
            this.panel2.Controls.Add(this.grpStringConstructor);
            this.panel2.Dock = DockStyle.Top;
            this.panel2.Location = new Point(20, 0x74);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new Padding(20, 0, 0, 0);
            this.panel2.Size = new Size(0x1f9, 0x99);
            this.panel2.TabIndex = 2;
            this.grpStringConstructor.Controls.Add(this.txtCxString);
            this.grpStringConstructor.Controls.Add(this.chkEncryptCustomCxString);
            this.grpStringConstructor.Dock = DockStyle.Fill;
            this.grpStringConstructor.Location = new Point(20, 0);
            this.grpStringConstructor.Name = "grpStringConstructor";
            this.grpStringConstructor.Padding = new Padding(8, 4, 8, 3);
            this.grpStringConstructor.Size = new Size(0x1e5, 0x99);
            this.grpStringConstructor.TabIndex = 0;
            this.grpStringConstructor.TabStop = false;
            this.grpStringConstructor.Text = "Please provide a value:";
            this.txtCxString.Dock = DockStyle.Fill;
            this.txtCxString.Location = new Point(8, 0x11);
            this.txtCxString.Multiline = true;
            this.txtCxString.Name = "txtCxString";
            this.txtCxString.ScrollBars = ScrollBars.Vertical;
            this.txtCxString.Size = new Size(0x1d5, 0x70);
            this.txtCxString.TabIndex = 0;
            this.txtCxString.TextChanged += new EventHandler(this.EnableControls);
            this.chkEncryptCustomCxString.AutoSize = true;
            this.chkEncryptCustomCxString.Dock = DockStyle.Bottom;
            this.chkEncryptCustomCxString.Location = new Point(8, 0x81);
            this.chkEncryptCustomCxString.Name = "chkEncryptCustomCxString";
            this.chkEncryptCustomCxString.Padding = new Padding(2, 2, 0, 0);
            this.chkEncryptCustomCxString.Size = new Size(0x1d5, 0x15);
            this.chkEncryptCustomCxString.TabIndex = 1;
            this.chkEncryptCustomCxString.Text = "Encrypt this string when saving connection details";
            this.chkEncryptCustomCxString.UseVisualStyleBackColor = true;
            this.rbStringConstructor.AutoSize = true;
            this.rbStringConstructor.Dock = DockStyle.Top;
            this.rbStringConstructor.Location = new Point(20, 0x56);
            this.rbStringConstructor.Name = "rbStringConstructor";
            this.rbStringConstructor.Padding = new Padding(0, 7, 0, 0);
            this.rbStringConstructor.Size = new Size(0x1f9, 30);
            this.rbStringConstructor.TabIndex = 1;
            this.rbStringConstructor.TabStop = true;
            this.rbStringConstructor.Text = "Via a constructor that accepts a string";
            this.rbStringConstructor.UseVisualStyleBackColor = true;
            this.rbStringConstructor.Click += new EventHandler(this.EnableControls);
            this.panAppConfigWarning.AutoSize = true;
            this.panAppConfigWarning.Controls.Add(this.lblAppConfigWarning);
            this.panAppConfigWarning.Dock = DockStyle.Top;
            this.panAppConfigWarning.Location = new Point(20, 0x2e);
            this.panAppConfigWarning.Name = "panAppConfigWarning";
            this.panAppConfigWarning.Size = new Size(0x1f9, 40);
            this.panAppConfigWarning.TabIndex = 0x13;
            this.rbParameterlessConstructor.AutoSize = true;
            this.rbParameterlessConstructor.Dock = DockStyle.Top;
            this.rbParameterlessConstructor.Location = new Point(20, 0x17);
            this.rbParameterlessConstructor.Name = "rbParameterlessConstructor";
            this.rbParameterlessConstructor.Size = new Size(0x1f9, 0x17);
            this.rbParameterlessConstructor.TabIndex = 0;
            this.rbParameterlessConstructor.TabStop = true;
            this.rbParameterlessConstructor.Text = "Via the parameterless constructor";
            this.rbParameterlessConstructor.UseVisualStyleBackColor = true;
            this.rbParameterlessConstructor.Click += new EventHandler(this.EnableControls);
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.txtCxName, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtAppConfigPath, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.llChooseAppConfigPath, 1, 0);
            this.tableLayoutPanel1.Dock = DockStyle.Top;
            this.tableLayoutPanel1.Location = new Point(10, 0x1aa);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new Padding(0, 10, 0, 5);
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.Size = new Size(0x217, 0x73);
            this.tableLayoutPanel1.TabIndex = 2;
            this.txtCxName.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.tableLayoutPanel1.SetColumnSpan(this.txtCxName, 2);
            this.txtCxName.Location = new Point(1, 80);
            this.txtCxName.Margin = new Padding(1, 1, 0, 5);
            this.txtCxName.Name = "txtCxName";
            this.txtCxName.Size = new Size(0x216, 0x19);
            this.txtCxName.TabIndex = 4;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0, 60);
            this.label1.Margin = new Padding(0, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0xe1, 0x13);
            this.label1.TabIndex = 3;
            this.label1.Text = "&Name for this connection (optional)";
            this.label5.AutoSize = true;
            this.label5.Location = new Point(0, 10);
            this.label5.Margin = new Padding(0, 0, 3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x148, 0x13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Path to &application config file (if required for connection string)";
            this.txtAppConfigPath.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.tableLayoutPanel1.SetColumnSpan(this.txtAppConfigPath, 2);
            this.txtAppConfigPath.Location = new Point(1, 30);
            this.txtAppConfigPath.Margin = new Padding(1, 1, 0, 5);
            this.txtAppConfigPath.Name = "txtAppConfigPath";
            this.txtAppConfigPath.Size = new Size(0x216, 0x19);
            this.txtAppConfigPath.TabIndex = 1;
            this.llChooseAppConfigPath.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.llChooseAppConfigPath.AutoSize = true;
            this.llChooseAppConfigPath.Location = new Point(480, 10);
            this.llChooseAppConfigPath.Margin = new Padding(0);
            this.llChooseAppConfigPath.Name = "llChooseAppConfigPath";
            this.llChooseAppConfigPath.Size = new Size(0x37, 0x13);
            this.llChooseAppConfigPath.TabIndex = 2;
            this.llChooseAppConfigPath.TabStop = true;
            this.llChooseAppConfigPath.Text = "Choose";
            this.llChooseAppConfigPath.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llChooseAppConfigPath_LinkClicked);
            this.tmrCheckType.Interval = 500;
            this.tmrCheckType.Tick += new EventHandler(this.tmrCheckType_Tick);
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x22b, 0x291);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.panel1);
            base.Controls.Add(this.tableLayoutPanel1);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.panCustomDC);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Margin = new Padding(3, 4, 3, 4);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "DbContextCxForm";
            base.Padding = new Padding(10);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Entity Framework POCO Connection";
            this.panOKCancel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panCustomDC.ResumeLayout(false);
            this.panCustomDC.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.grpStringConstructor.ResumeLayout(false);
            this.grpStringConstructor.PerformLayout();
            this.panAppConfigWarning.ResumeLayout(false);
            this.panAppConfigWarning.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private bool IsDataValid()
        {
            return (((this.rbParameterlessConstructor.Checked || this.rbStringConstructor.Checked) && ((this.txtAssemblyPath.Text.Trim().Length > 2) && (this.txtTypeName.Text.Trim().Length > 0))) && (!this.rbStringConstructor.Checked || (this.txtCxString.Text.Trim().Length > 0)));
        }

        private void llBrowseAssembly_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.BrowseAssembly();
        }

        private void llChooseAppConfigPath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose application config file";
                string str = this.txtAppConfigPath.Text.Trim();
                if (str.Length > 0)
                {
                    dialog.FileName = str;
                }
                else
                {
                    try
                    {
                        string directoryName = Path.GetDirectoryName(this.txtAssemblyPath.Text.Trim());
                        if (Directory.Exists(directoryName))
                        {
                            dialog.InitialDirectory = directoryName;
                        }
                    }
                    catch
                    {
                    }
                }
                dialog.DefaultExt = ".config";
                dialog.Filter = "Application configuration files|*.config";
                if ((dialog.ShowDialog() == DialogResult.OK) && (dialog.FileName != str))
                {
                    this.txtAppConfigPath.Text = dialog.FileName;
                }
            }
        }

        private void llChooseTypeName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ChooseTypeName();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!((base.DialogResult != DialogResult.OK) || this.IsDataValid()))
            {
                base.DialogResult = DialogResult.Cancel;
            }
            if (base.DialogResult == DialogResult.OK)
            {
                this.UpdateRepository(this._cxInfo);
            }
            base.OnClosing(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if ((this.btnOK != null) && (this.btnOK.Parent != null))
            {
                int height = (this.btnOK.Bottom + this.btnOK.Parent.Top) + base.Padding.Bottom;
                if (base.ClientSize.Height != height)
                {
                    base.ClientSize = new Size(base.ClientSize.Width, height);
                }
            }
        }

        private void PopulateFromRepository()
        {
            this.txtAssemblyPath.Text = this._cxInfo.CustomTypeInfo.CustomAssemblyPath;
            this.txtTypeName.Text = this._cxInfo.CustomTypeInfo.CustomTypeName;
            this.txtCxString.Text = this._cxInfo.DatabaseInfo.CustomCxString;
            this.rbStringConstructor.Checked = this.txtCxString.Text.Length > 0;
            this.rbParameterlessConstructor.Checked = !this.rbStringConstructor.Checked;
            this.chkEncryptCustomCxString.Checked = this._cxInfo.DatabaseInfo.EncryptCustomCxString;
            this.txtAppConfigPath.Text = this._cxInfo.AppConfigPath;
            this.txtCxName.Text = this._cxInfo.DisplayName;
            this.chkRemember.Checked = this._cxInfo.Persist;
        }

        private void ProbeType()
        {
            if ((this.txtAssemblyPath.Text.Trim().Length < 3) || (this.txtTypeName.Text.Trim().Length < 1))
            {
                this._stringCtrlOK = false;
                this._parameterlessCtorOK = false;
                this.lblTypeMessage.Visible = false;
            }
            else
            {
                try
                {
                    string str;
                    this.GetConstructorInfo(out this._parameterlessCtorOK, out str);
                    this._stringCtrlOK = str != null;
                    this.grpStringConstructor.Text = "Please provide a value" + (!this._stringCtrlOK ? "" : (" for " + str));
                    if (!(this._parameterlessCtorOK || this._stringCtrlOK))
                    {
                        this.txtTypeName.Text.Trim().Split(new char[] { '.' }).Last<string>();
                        this.lblTypeMessage.Text = "Error: the type must define a public parameterless constructor and/or a constructor accepting a single string parameter.";
                        this.lblTypeMessage.Visible = true;
                    }
                    else
                    {
                        this.lblTypeMessage.Visible = false;
                    }
                }
                catch (Exception exception)
                {
                    this.lblTypeMessage.Text = "Error: " + exception.Message;
                    if (exception.InnerException != null)
                    {
                        this.lblTypeMessage.Text = this.lblTypeMessage.Text + "\r\n" + exception.InnerException.Message;
                    }
                    this.lblTypeMessage.Visible = true;
                    this._stringCtrlOK = true;
                    this._parameterlessCtorOK = true;
                }
            }
            this.EnableControls();
        }

        private void tmrCheckType_Tick(object sender, EventArgs e)
        {
            this.tmrCheckType.Stop();
            this.ProbeType();
        }

        private void txtAssemblyPath_TextChanged(object sender, EventArgs e)
        {
            this.tmrCheckType.Stop();
            this.tmrCheckType.Start();
            this.EnableControls();
        }

        private void txtTypeName_TextChanged(object sender, EventArgs e)
        {
            this.tmrCheckType.Stop();
            this.tmrCheckType.Start();
            this.EnableControls();
        }

        private void UpdateRepository(IConnectionInfo cxInfo)
        {
            cxInfo.CustomTypeInfo.CustomAssemblyPath = this.txtAssemblyPath.Text.Trim();
            cxInfo.CustomTypeInfo.CustomTypeName = this.txtTypeName.Text.Trim();
            cxInfo.DatabaseInfo.CustomCxString = this.rbParameterlessConstructor.Checked ? "" : this.txtCxString.Text.Trim();
            cxInfo.DatabaseInfo.EncryptCustomCxString = this.chkEncryptCustomCxString.Checked;
            cxInfo.AppConfigPath = this.txtAppConfigPath.Text.Trim();
            cxInfo.DisplayName = this.txtCxName.Text.Trim();
            cxInfo.Persist = this.chkRemember.Checked;
        }

        private class AssemblyProber : MarshalByRefObject
        {
            private ConstructorInfo GetConstructor(Type t, params Type[] paramTypes)
            {
                return t.GetConstructor(paramTypes);
            }

            public void GetConstructorInfo(string assemPath, string typeName, out bool parameterlessCtor, out string stringCtorParamName)
            {
                Type customType = this.GetCustomType(assemPath, typeName);
                ConstructorInfo constructor = this.GetConstructor(customType, new Type[0]);
                parameterlessCtor = (constructor != null) && (constructor.IsFamily || constructor.IsPublic);
                ConstructorInfo info2 = this.GetConstructor(customType, new Type[] { typeof(string) });
                stringCtorParamName = ((info2 == null) || (!info2.IsFamily && !info2.IsPublic)) ? null : info2.GetParameters()[0].Name;
            }

            private Type GetCustomType(string assemPath, string typeName)
            {
                Type type = Assembly.LoadFrom(assemPath).GetType(typeName, true);
                if (type == null)
                {
                    throw new Exception("Type '" + typeName + "' does not exist in that assembly");
                }
                return type;
            }

            public string[] GetCustomTypeNames(string assemPath)
            {
                Type[] types;
                new EntityFrameworkDbContextDriver();
                Assembly assembly = Assembly.LoadFrom(assemPath);
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    if ((exception.LoaderExceptions != null) && (exception.LoaderExceptions.Length > 0))
                    {
                        throw exception.LoaderExceptions[0];
                    }
                    throw;
                }
                return (from t in types
                    where this.IsDbContextType(t)
                    select t.FullName into t
                    orderby t
                    select t).ToArray<string>();
            }

            private bool IsDbContextType(Type t)
            {
                t = t.BaseType;
                while (t != null)
                {
                    if (t.FullName == "System.Data.Entity.DbContext")
                    {
                        return true;
                    }
                    t = t.BaseType;
                }
                return false;
            }

            public void Test(Repository repos, string arg)
            {
                Type customType = this.GetCustomType(repos.CustomAssemblyPath, repos.CustomTypeName);
                Type[] paramTypes = string.IsNullOrEmpty(arg) ? new Type[0] : new Type[] { typeof(string) };
                if (this.GetConstructor(customType, paramTypes) == null)
                {
                    throw new Exception("Could not find constructor on type '" + repos.CustomTypeName + "'.");
                }
                object[] args = string.IsNullOrEmpty(arg) ? null : new object[] { arg };
                dynamic obj2 = Activator.CreateInstance(customType, args);
                obj2.Database.Connection.Open();
                obj2.Database.Connection.Close();
                ((EntityFrameworkDbContextDriver) repos.DriverLoader.Driver).Test(repos);
            }
        }
    }
}

