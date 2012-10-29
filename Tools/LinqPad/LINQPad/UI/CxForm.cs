namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Linq;

    internal class CxForm : BaseForm
    {
        private bool _activated;
        private bool _allowAllDbs;
        private bool _allowSpecificDb;
        private Thread _createDbWorker;
        private List<string> _databases;
        private string _dbError;
        private object _dbLocker;
        private Thread _fetchDbWorker;
        private bool _initialized;
        private int _lastSchemaSelectedIndex;
        private string _lastSetDb;
        private string _lastSetFile;
        private string _lastSetPW;
        private string _lastSetServer;
        private string _lastSetUserName;
        private LinkedDatabase[] _linkedDatabases;
        private bool? _oldEncryptTraffic;
        private float _panDbFirstColWidth;
        private Padding _panSqlAuthPadding;
        private Repository _repository;
        private volatile Thread _testCxThread;
        private volatile bool _testSuccessful;
        private Button btnCancel;
        private Button btnCreateDb;
        private LinkLabel btnDbFile;
        private Button btnOK;
        private Button btnTest;
        private ComboBox cboDatabase;
        private ComboBox cboProviderName;
        private ComboBox cboSchema;
        private ComboBox cboServer;
        private CheckBox chkCapitalize;
        private CheckBox chkEncryptCustomCxString;
        private CheckBox chkEncryptTraffic;
        private CheckBox chkLinkedDbs;
        private CheckBox chkPluralize;
        private CheckBox chkRemember;
        private CheckBox chkSPFunctions;
        private CheckBox chkSystemObjects;
        private CheckBox chkUserInstance;
        private IContainer components;
        private GroupBox grpDatabase;
        private GroupBox grpLogon;
        private GroupBox grpOptions;
        private GroupBox grpProvider;
        private Label label1;
        private Label label2;
        private Label lblCreateStatus;
        private Label lblCustomTypeName;
        private Label lblCxString;
        private Label lblFetchingDbs;
        private Label lblMaxDbOptional;
        private Label lblMaxDbSize;
        private Label lblMetadataPath;
        private Label lblPassword;
        private Label lblProviderName;
        private Label lblServer;
        private Label lblUserName;
        private Label lblWarning;
        private LinkLabel llBrowseAssembly;
        private LinkLabel llChooseMetadata;
        private LinkLabel llChooseTypeName;
        private LinkLabel llLinkDbDetails;
        private LinkLabel llProxy;
        private Panel panCustomCx;
        private TableLayoutPanel panCustomDC;
        private TableLayoutPanel panDatabase;
        private TableLayoutPanel panDataContext;
        private Panel panel1;
        private Panel panel4;
        private TableLayoutPanel panMaxDbSize;
        private Panel panOKCancel;
        private Panel panSpace1;
        private Panel panSpace2;
        private Panel panSpace3;
        private TableLayoutPanel panSqlAuthentication;
        private FlowLayoutPanel panWarning;
        private RadioButton rbAllDbs;
        private RadioButton rbAstoria;
        private RadioButton rbAttach;
        private RadioButton rbAzure;
        private RadioButton rbCustomCx;
        private RadioButton rbSpecifyDb;
        private RadioButton rbSqlAuth;
        private RadioButton rbSqlCE;
        private RadioButton rbSqlCE40;
        private RadioButton rbSqlServer;
        private RadioButton rbWindowsAuth;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox txtAssemblyPath;
        private TextBox txtCxString;
        private TextBox txtDbFile;
        private TextBox txtMaxDbSize;
        private TextBox txtMetadataPath;
        private TextBox txtPassword;
        private TextBox txtTypeName;
        private TextBox txtUserName;

        public CxForm(Repository repository, bool isNewRepository)
        {
            bool flag;
            this._databases = new List<string>();
            this._dbLocker = new object();
            this._dbError = "";
            this.components = null;
            this._repository = repository;
            string[] items = (!(flag = this._repository.DriverLoader.Driver is AstoriaDynamicDriver) && this._repository.IsAzure) ? MRU.AzureServerNames.GetNames() : (flag ? MRU.AstoriaUriNames.GetNames() : MRU.ServerNames.GetNames());
            if (isNewRepository)
            {
                if (items.Length == 0)
                {
                    this._repository.Server = flag ? "http://" : this.GetDefaultSQLString();
                }
                else
                {
                    this._repository.Server = items[0];
                }
                this._repository.Persist = true;
                this._allowSpecificDb = true;
                this._allowAllDbs = true;
            }
            else
            {
                this._allowAllDbs = ((this._repository.DriverLoader.Driver is LinqToSqlDynamicDriver) && (this._repository.Database.Length == 0)) && !this._repository.AttachFile;
                this._allowSpecificDb = !this._allowAllDbs;
            }
            this.InitializeComponent();
            this.panDataContext.Hide();
            this._panDbFirstColWidth = this.panDatabase.ColumnStyles[0].Width;
            this._panSqlAuthPadding = this.panSqlAuthentication.Padding;
            try
            {
                this.lblWarning.Font = new Font(this.Font, FontStyle.Bold);
            }
            catch
            {
            }
            this.PopulateFromRepository();
            this.EnableControls();
            DataContextDriver driver = this._repository.DriverLoader.Driver;
            this.Text = driver.Name + ((driver is StaticDataContextDriver) ? " Custom Assembly" : "") + " Connection";
            this.cboServer.Items.AddRange(items);
            this._initialized = true;
        }

        private bool AreDbDetailsBlank()
        {
            if ((((this.cboServer.Text.Trim().Length > 0) && (this.cboServer.Text.Trim().ToUpperInvariant() != this.GetDefaultSQLString())) && (this.cboServer.Text != this._lastSetServer)) && (this._lastSetServer != null))
            {
                return false;
            }
            if (((this.txtDbFile.Text.Trim().Length > 0) && (this.txtDbFile.Text != this._lastSetFile)) && (this._lastSetFile != null))
            {
                return false;
            }
            if (((this.cboDatabase.Text.Trim().Length > 0) && (this.cboDatabase.Text != this._lastSetDb)) && (this._lastSetDb != null))
            {
                return false;
            }
            if (((this.txtUserName.Text.Trim().Length > 0) && (this.txtUserName.Text != this._lastSetUserName)) && (this._lastSetUserName != null))
            {
                return false;
            }
            if ((this.txtPassword.Text.Trim().Length > 0) && ((this.txtPassword.Text != this._lastSetPW) & (this._lastSetPW != null)))
            {
                return false;
            }
            return true;
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

        private void btnCreateDb_Click(object sender, EventArgs e)
        {
            if (!((this.rbSqlCE.Checked || this.rbSqlCE40.Checked) ? (Path.GetExtension(this.txtDbFile.Text.Trim()).Length != 0) : true))
            {
                this.txtDbFile.Text = this.txtDbFile.Text + ".sdf";
            }
            Repository r = new Repository();
            this.UpdateRepository(r);
            this.lblCreateStatus.Text = "Working...";
            Thread thread = new Thread(new ParameterizedThreadStart(this.CreateDb)) {
                Name = "Create Db Worker",
                IsBackground = true
            };
            this._createDbWorker = thread;
            this._createDbWorker.Start(r);
        }

        private void btnDbFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.ValidateNames = false;
                dialog.DefaultExt = (this.rbSqlCE.Checked || this.rbSqlCE40.Checked) ? "sdf" : "mdf";
                dialog.Filter = (this.rbSqlCE.Checked || this.rbSqlCE40.Checked) ? "SQL CE database files (*.sdf)|*.sdf|All files (*.*)|*.*" : "SQL database files (*.mdf)|*.mdf|All files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.txtDbFile.Text = dialog.FileName;
                }
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (!((this.rbSqlCE.Checked || this.rbSqlCE40.Checked) ? File.Exists(this.txtDbFile.Text.Trim()) : true))
            {
                MessageBox.Show("File: " + this.txtDbFile.Text.Trim() + " does not exist.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!((this.cboSchema.SelectedIndex <= 0) || File.Exists(this.txtAssemblyPath.Text.Trim())))
            {
                MessageBox.Show("File: " + this.txtAssemblyPath.Text.Trim() + " does not exist.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                if (this._testCxThread != null)
                {
                    this._testCxThread = null;
                }
                else
                {
                    Repository r = new Repository {
                        DoNotSave = true
                    };
                    this.UpdateRepository(r);
                    Thread thread = new Thread(() => this.TestCx(r)) {
                        Name = "Cx Tester",
                        IsBackground = true
                    };
                    this._testCxThread = thread;
                    this._testCxThread.Start();
                }
                this.EnableControls();
            }
        }

        private void cboDatabase_DropDown(object sender, EventArgs e)
        {
            this.FetchDatabases();
        }

        private void cboSchema_Enter(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Alt)
            {
                Native.SendMessage(this.cboSchema.Handle, 0x14f, new IntPtr(1), IntPtr.Zero);
            }
        }

        private void cboSchema_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if ((this.cboSchema.SelectedIndex > 0) && this.rbAstoria.Checked)
            {
                this.rbSqlServer.Checked = true;
            }
            if (((this.cboSchema.SelectedIndex < 2) && this.rbCustomCx.Checked) && (this.txtCxString.Text.Trim().Length > 0))
            {
                this.rbSqlServer.Checked = true;
            }
            this.EnableControls();
            if (this._lastSchemaSelectedIndex != this.cboSchema.SelectedIndex)
            {
                this._lastSchemaSelectedIndex = this.cboSchema.SelectedIndex;
                if (this.txtAssemblyPath.Text.Trim().Length == 0)
                {
                    this.txtAssemblyPath.Focus();
                }
            }
        }

        private void CheckUserInstanceIfApplicable()
        {
            if ((this.rbAttach.Checked && this.rbWindowsAuth.Checked) && this.cboServer.Text.ToUpperInvariant().Contains("SQLEXPRESS"))
            {
                this.chkUserInstance.Checked = true;
            }
        }

        private void chkLinkedDbs_Click(object sender, EventArgs e)
        {
            this.chkLinkedDbs.Checked = true;
            this.ShowLinkedDbDialog();
        }

        private void ChooseTypeName()
        {
            ThreadStart start = null;
            if (!File.Exists(this.txtAssemblyPath.Text.Trim()))
            {
                MessageBox.Show("The assembly '" + this.txtAssemblyPath.Text.Trim() + "' does not exist.");
            }
            else
            {
                string[] data;
                string[] strArray2;
                Repository r = new Repository();
                this.UpdateRepository(r);
                using (DomainIsolator isolator = new DomainIsolator("Inspect Custom Assembly"))
                {
                    isolator.Domain.SetData("assem", this.txtAssemblyPath.Text.Trim());
                    isolator.Domain.SetData("loader", r.DriverLoader);
                    try
                    {
                        isolator.Domain.DoCallBack(new CrossAppDomainDelegate(CxForm.GetCustomTypes));
                        data = (string[]) isolator.Domain.GetData("types");
                        strArray2 = (string[]) isolator.Domain.GetData("metadata");
                    }
                    catch (Exception exception)
                    {
                        Log.Write(exception, "CxForm GetCustomTypes");
                        MessageBox.Show("Error loading custom assembly:\r\n\r\n" + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                }
                if (data.Length == 0)
                {
                    MessageBox.Show("There are no types based on '" + r.DriverLoader.Driver.ContextBaseTypeName + "' in that assembly.");
                }
                else
                {
                    if (this.cboSchema.SelectedIndex < 2)
                    {
                        strArray2 = null;
                    }
                    using (ChooseTypeForm form = new ChooseTypeForm(this.txtAssemblyPath.Text.Trim(), data, this.txtTypeName.Text.Trim(), strArray2, this.txtMetadataPath.Text.Trim()))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            this.txtTypeName.Text = form.SelectedTypeName;
                            this.txtMetadataPath.Text = form.SelectedMetadataName;
                            if (this.AreDbDetailsBlank())
                            {
                                if (start == null)
                                {
                                    start = delegate {
                                        try
                                        {
                                            this.ProbeDefaultCxString();
                                        }
                                        catch (Exception exception)
                                        {
                                            Log.Write(exception, "CxForm ProbeDefaultCxString");
                                        }
                                    };
                                }
                                new Thread(start) { Name = "Default Cx String Probe", IsBackground = true }.Start();
                            }
                        }
                    }
                }
            }
        }

        private void CreateDb(object repositoryObj)
        {
            Repository repository = (Repository) repositoryObj;
            string attachFileName = "database";
            try
            {
                if (repository.IsSqlCE)
                {
                    attachFileName = repository.AttachFileName;
                    DbFactory.GetFactory(repository.Provider).CreateDatabaseFile(repository.GetCxString());
                }
                else
                {
                    attachFileName = repository.Database;
                    repository.Database = "";
                    using (SqlConnection connection = new SqlConnection(repository.GetCxString()))
                    {
                        connection.Open();
                        new SqlCommand("create database " + attachFileName, connection).ExecuteNonQuery();
                    }
                }
                if (!base.IsDisposed && (this._createDbWorker == Thread.CurrentThread))
                {
                    base.Invoke(new Action<string>(this.UpdateCreateMsg), new object[] { "Created " + attachFileName });
                }
            }
            catch (Exception exception)
            {
                if (!base.IsDisposed && (this._createDbWorker == Thread.CurrentThread))
                {
                    base.Invoke(new Action<string>(this.UpdateCreateMsg), new object[] { "Error creating database." });
                    MessageBox.Show("Could not create " + attachFileName + ":\r\n" + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private string DecodeStudioAttachPath(string path, string assemblyPath)
        {
            if (path.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(15);
                if (path.StartsWith(@"\"))
                {
                    path = path.Substring(1);
                }
                return Path.Combine(assemblyPath, path);
            }
            if (path.Contains<char>('|'))
            {
                return "";
            }
            return path;
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
            base.SuspendLayout();
            bool flag = this._testCxThread != null;
            this.Text = flag ? "Testing connection..." : (this._testSuccessful ? "Connection Successful!" : "LINQPad Connection");
            this.rbCustomCx.Visible = this.cboSchema.SelectedIndex == 2;
            this.cboSchema.Enabled = !flag;
            this.panCustomCx.Visible = (this.cboSchema.SelectedIndex == 2) && this.rbCustomCx.Checked;
            this.panSpace3.Visible = this.grpOptions.Visible = (this.cboSchema.SelectedIndex == 0) && !this.rbAstoria.Checked;
            this.panCustomDC.Visible = this.cboSchema.SelectedIndex > 0;
            if (((this.cboSchema.SelectedIndex == 2) && this.rbCustomCx.Checked) && (this.cboProviderName.Items.Count == 0))
            {
                try
                {
                    this.cboProviderName.Items.AddRange((from r in DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>() select r["InvariantName"]).ToArray<object>());
                }
                catch
                {
                }
            }
            this.txtAssemblyPath.Enabled = this.txtTypeName.Enabled = this.txtMetadataPath.Enabled = this.llBrowseAssembly.Enabled = !flag;
            this.lblMetadataPath.Visible = this.txtMetadataPath.Visible = this.llChooseMetadata.Visible = this.cboSchema.SelectedIndex == 2;
            this.llChooseTypeName.Enabled = !flag && (this.txtAssemblyPath.Text.Trim().Length > 0);
            this.llChooseMetadata.Enabled = (!flag && (this.txtAssemblyPath.Text.Trim().Length > 0)) && (this.txtTypeName.Text.Trim().Length > 0);
            this.lblServer.Visible = this.cboServer.Visible = (this.rbSqlServer.Checked || this.rbAstoria.Checked) || this.rbAzure.Checked;
            this.lblServer.Text = this.rbAstoria.Checked ? "URI" : "&Server";
            this.rbSpecifyDb.Visible = this.cboDatabase.Visible = this.rbWindowsAuth.Visible = this.rbSqlAuth.Visible = this.chkSPFunctions.Visible = this.rbSqlServer.Checked || this.rbAzure.Checked;
            this.rbAllDbs.Visible = (this.rbSqlServer.Checked || this.rbAzure.Checked) ? (this.cboSchema.SelectedIndex == 0) : false;
            this.lblCustomTypeName.Text = "Full Name of Typed " + ((this.cboSchema.SelectedIndex == 2) ? "ObjectContext" : "DataContext");
            this.panDatabase.ColumnStyles[0].Width = (this.rbSqlServer.Checked || this.rbAzure.Checked) ? this._panDbFirstColWidth : 1f;
            if (this.rbSqlServer.Checked || this.rbAzure.Checked)
            {
                this.panSqlAuthentication.Padding = this._panSqlAuthPadding;
            }
            else
            {
                this.panSqlAuthentication.Padding = new Padding(1, 0, 0, this._panSqlAuthPadding.Bottom);
            }
            this.lblServer.Enabled = this.cboServer.Enabled = this.rbSqlAuth.Enabled = !flag;
            this.rbWindowsAuth.Enabled = !flag && !this.rbAzure.Checked;
            if (this.rbAzure.Checked && this.rbWindowsAuth.Checked)
            {
                this.rbSqlAuth.Checked = true;
            }
            this.rbAllDbs.Enabled = this._allowAllDbs && !flag;
            this.rbAttach.Enabled = ((this._allowSpecificDb && this.rbSqlServer.Checked) || (this.rbSqlCE.Checked || this.rbSqlCE40.Checked)) ? !flag : false;
            this.rbSpecifyDb.Enabled = this._allowSpecificDb && !flag;
            this.chkUserInstance.Visible = ((this.rbAttach.Checked && this.rbWindowsAuth.Checked) && (this.rbSqlServer.Checked && !flag)) && !this.cboServer.Text.ToLowerInvariant().Contains("(localdb)");
            if (!((this.rbAttach.Checked && this.rbWindowsAuth.Checked) && this.rbSqlServer.Checked))
            {
                this.chkUserInstance.Checked = false;
            }
            if (!((this.rbSqlCE.Checked || this.rbSqlCE40.Checked) ? this.rbAttach.Checked : true))
            {
                this.rbAttach.Checked = true;
            }
            this.txtDbFile.Enabled = this.btnDbFile.Enabled = ((this.rbAttach.Checked || this.rbSqlCE.Checked) || this.rbSqlCE40.Checked) ? !flag : false;
            this.cboDatabase.Enabled = this.rbSpecifyDb.Checked && !flag;
            this.lblUserName.Visible = this.txtUserName.Visible = this.rbSqlAuth.Checked || this.rbAstoria.Checked;
            this.lblPassword.Visible = this.txtPassword.Visible = ((this.rbSqlAuth.Checked || this.rbSqlCE.Checked) || this.rbSqlCE40.Checked) || this.rbAstoria.Checked;
            this.txtUserName.Enabled = this.txtPassword.Enabled = !flag;
            if (this.rbSqlServer.Checked)
            {
                this.btnCreateDb.Enabled = (this.cboDatabase.Enabled && (this.cboDatabase.Text.Trim().Length > 0)) && !flag;
            }
            else if (this.rbSqlCE.Checked || this.rbSqlCE40.Checked)
            {
                this.btnCreateDb.Enabled = (this.txtDbFile.Text.Trim().Length > 0) && !flag;
            }
            else
            {
                this.btnCreateDb.Enabled = false;
            }
            this.chkPluralize.Enabled = this.chkCapitalize.Enabled = this.chkSPFunctions.Enabled = !flag;
            this.panMaxDbSize.Visible = this.rbSqlCE.Checked || this.rbSqlCE40.Checked;
            this.rbSqlCE.Enabled = this.rbSqlCE40.Enabled = this.rbSqlServer.Enabled = this.rbAzure.Enabled = this.rbAstoria.Enabled = !flag;
            if (this.rbSqlCE.Checked || this.rbSqlCE40.Checked)
            {
                this.panDatabase.RowStyles[4].SizeType = SizeType.Absolute;
                this.panDatabase.RowStyles[3].SizeType = SizeType.Absolute;
                this.panDatabase.RowStyles[3].Height = this.panDatabase.RowStyles[4].Height = 0f;
            }
            else
            {
                this.panDatabase.RowStyles[4].SizeType = SizeType.AutoSize;
                this.panDatabase.RowStyles[3].SizeType = SizeType.AutoSize;
            }
            this.grpDatabase.Visible = this.panSpace2.Visible = ((this.rbSqlServer.Checked || this.rbSqlCE.Checked) || this.rbSqlCE40.Checked) || this.rbAzure.Checked;
            this.grpLogon.Visible = this.panSpace1.Visible = !this.rbCustomCx.Checked;
            this.rbCustomCx.Enabled = !flag;
            this.rbAttach.Visible = this.btnDbFile.Visible = this.txtDbFile.Visible = !this.rbAzure.Checked;
            this.chkLinkedDbs.Visible = this.llLinkDbDetails.Visible = (((this.cboSchema.SelectedIndex == 0) && !this.rbSqlCE.Checked) && !this.rbSqlCE40.Checked) && !this.rbAzure.Checked;
            this.chkSystemObjects.Visible = ((this.cboSchema.SelectedIndex == 0) && !this.rbSqlCE.Checked) && !this.rbSqlCE40.Checked;
            this.chkLinkedDbs.Enabled = this.llLinkDbDetails.Enabled = !flag && (this.rbSpecifyDb.Checked || this.rbAttach.Checked);
            this.chkSystemObjects.Enabled = !flag;
            this.chkLinkedDbs.Checked = (this._linkedDatabases != null) && (this._linkedDatabases.Length > 0);
            this.chkEncryptCustomCxString.Visible = this.chkRemember.Checked;
            this.chkEncryptTraffic.Visible = this.rbSqlServer.Checked || this.rbAzure.Checked;
            this.chkEncryptTraffic.Enabled = this.rbSqlServer.Checked || !flag;
            this.llProxy.Visible = this.rbAstoria.Checked;
            this.panWarning.Visible = this.rbAstoria.Checked && (Program.MajorMinorVersion < 400);
            if (this.rbAzure.Checked)
            {
                this.chkSystemObjects.Text = "Include System Views and SPs";
            }
            else
            {
                this.chkSystemObjects.Text = "Include System Views and SPs (requires SQL 2005 or later)";
            }
            this.btnTest.Text = flag ? "S&top" : "&Test";
            this.btnOK.Enabled = this.btnTest.Enabled = this.IsDataValid();
            base.ResumeLayout();
            if (this._activated)
            {
                this.EnsureFormFits();
            }
        }

        private void EnableControls(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void EnsureFormFits()
        {
            int num = base.Top - Screen.GetWorkingArea(this).Top;
            int num2 = (base.Bottom - Screen.GetWorkingArea(this).Bottom) + 10;
            if ((num2 > 0) && (num > 0))
            {
                base.Top -= Math.Min(num2, num);
            }
        }

        private void FetchDatabases()
        {
            object obj2;
            bool lockTaken = false;
            try
            {
                Repository r;
                Monitor.Enter(obj2 = this._dbLocker, ref lockTaken);
                if (((this._fetchDbWorker == null) && (this.cboDatabase.Items.Count <= 0)) && (this.cboServer.Text.Trim().Length != 0))
                {
                    r = new Repository();
                    this.UpdateRepository(r);
                    r.Database = "";
                    this.lblFetchingDbs.Text = "Fetching...";
                    Thread thread = new Thread(() => this.GetDatabases(r)) {
                        Name = "Fetch Db Worker",
                        IsBackground = true
                    };
                    this._fetchDbWorker = thread;
                    this._fetchDbWorker.Start();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(obj2);
                }
            }
        }

        private static void GetCustomTypes()
        {
            Type[] types;
            string data = (string) AppDomain.CurrentDomain.GetData("assem");
            DCDriverLoader loader = (DCDriverLoader) AppDomain.CurrentDomain.GetData("loader");
            DataContextDriver driver = loader.Driver;
            Type baseType = driver.GetContextBaseType();
            if (baseType == null)
            {
                throw new Exception("Unable to resolve type: " + driver.ContextBaseTypeName);
            }
            Assembly assembly = Assembly.LoadFrom(data);
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
            string[] strArray = (from t in types
                where baseType.IsAssignableFrom(t)
                select t.FullName into t
                orderby t
                select t).ToArray<string>();
            AppDomain.CurrentDomain.SetData("types", strArray);
            IEnumerable<string> source = from name in assembly.GetManifestResourceNames()
                let dotPos = name.LastIndexOf('.')
                where (dotPos > 0) && (dotPos < (name.Length - 2))
                let ext = name.Substring(dotPos + 1).ToLowerInvariant()
                where ((ext == "csdl") || (ext == "msl")) || (ext == "ssdl")
                select name;
            AppDomain.CurrentDomain.SetData("metadata", source.ToArray<string>());
        }

        private static Dictionary<string, string> GetCxStringsFromConfig(string path)
        {
            if (!(!string.IsNullOrEmpty(path) && File.Exists(path)))
            {
                return new Dictionary<string, string>();
            }
            try
            {
                return XElement.Load(path).Element("connectionStrings").Elements().ToDictionary<XElement, string, string>(e => ((string) e.Attribute("name")), e => ((string) e.Attribute("connectionString")));
            }
            catch
            {
                return new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            }
        }

        private void GetDatabases(Repository r)
        {
            List<string> databaseList = new List<string>();
            string message = "";
            try
            {
                databaseList = r.GetDatabaseList();
            }
            catch (Exception exception)
            {
                databaseList.Clear();
                message = exception.Message;
            }
            finally
            {
                lock (this._dbLocker)
                {
                    if (!(base.IsDisposed || (this._fetchDbWorker != Thread.CurrentThread)))
                    {
                        this._fetchDbWorker = null;
                        this._databases = databaseList;
                        this._dbError = message;
                        base.Invoke(new MethodInvoker(this.UpdateDbItems));
                    }
                }
            }
        }

        private static void GetDefaultCxString()
        {
            string data = (string) AppDomain.CurrentDomain.GetData("assem");
            string name = (string) AppDomain.CurrentDomain.GetData("type");
            Type type = Assembly.LoadFrom(data).GetType(name);
            if ((type != null) && (type.GetConstructor(Type.EmptyTypes) != null))
            {
                PropertyInfo property = type.GetProperty("Connection");
                if (property != null)
                {
                    object obj2 = Activator.CreateInstance(type);
                    IDbConnection connection = property.GetValue(obj2, null) as IDbConnection;
                    if (connection != null)
                    {
                        AppDomain.CurrentDomain.SetData("nativecxstring", connection.ConnectionString);
                        PropertyInfo info3 = connection.GetType().GetProperty("StoreConnection");
                        if (info3 != null)
                        {
                            connection = info3.GetValue(connection, null) as IDbConnection;
                        }
                        if (connection != null)
                        {
                            AppDomain.CurrentDomain.SetData("cxstring", connection.ConnectionString);
                        }
                    }
                }
            }
        }

        private string GetDefaultSQLString()
        {
            try
            {
                if (Directory.Exists(Path.Combine(PathHelper.ProgramFiles, @"Microsoft SQL Server\110\LocalDB\Binn")) || Directory.Exists(Path.Combine(PathHelper.ProgramFilesX86, @"Microsoft SQL Server\110\LocalDB\Binn")))
                {
                    return @"(localdb)\v11.0";
                }
            }
            catch
            {
            }
            return @".\SQLEXPRESS";
        }

        private void InitializeComponent()
        {
            this.lblServer = new Label();
            this.cboServer = new ComboBox();
            this.txtDbFile = new TextBox();
            this.btnDbFile = new LinkLabel();
            this.chkUserInstance = new CheckBox();
            this.grpLogon = new GroupBox();
            this.panMaxDbSize = new TableLayoutPanel();
            this.lblMaxDbSize = new Label();
            this.txtMaxDbSize = new TextBox();
            this.lblMaxDbOptional = new Label();
            this.panSqlAuthentication = new TableLayoutPanel();
            this.lblPassword = new Label();
            this.lblUserName = new Label();
            this.txtUserName = new TextBox();
            this.txtPassword = new TextBox();
            this.rbSqlAuth = new RadioButton();
            this.rbWindowsAuth = new RadioButton();
            this.panOKCancel = new Panel();
            this.btnTest = new Button();
            this.btnOK = new Button();
            this.panel4 = new Panel();
            this.btnCancel = new Button();
            this.grpDatabase = new GroupBox();
            this.chkSystemObjects = new CheckBox();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.chkLinkedDbs = new CheckBox();
            this.llLinkDbDetails = new LinkLabel();
            this.panDatabase = new TableLayoutPanel();
            this.rbAllDbs = new RadioButton();
            this.rbAttach = new RadioButton();
            this.cboDatabase = new ComboBox();
            this.rbSpecifyDb = new RadioButton();
            this.btnCreateDb = new Button();
            this.lblCreateStatus = new Label();
            this.lblFetchingDbs = new Label();
            this.panSpace1 = new Panel();
            this.panSpace2 = new Panel();
            this.chkRemember = new CheckBox();
            this.grpOptions = new GroupBox();
            this.chkSPFunctions = new CheckBox();
            this.chkCapitalize = new CheckBox();
            this.chkPluralize = new CheckBox();
            this.panSpace3 = new Panel();
            this.panel1 = new Panel();
            this.chkEncryptTraffic = new CheckBox();
            this.grpProvider = new GroupBox();
            this.rbCustomCx = new RadioButton();
            this.rbAstoria = new RadioButton();
            this.rbAzure = new RadioButton();
            this.rbSqlCE40 = new RadioButton();
            this.rbSqlCE = new RadioButton();
            this.rbSqlServer = new RadioButton();
            this.cboSchema = new ComboBox();
            this.label1 = new Label();
            this.panDataContext = new TableLayoutPanel();
            this.panCustomDC = new TableLayoutPanel();
            this.label2 = new Label();
            this.lblCustomTypeName = new Label();
            this.lblMetadataPath = new Label();
            this.txtAssemblyPath = new TextBox();
            this.txtTypeName = new TextBox();
            this.txtMetadataPath = new TextBox();
            this.llBrowseAssembly = new LinkLabel();
            this.llChooseTypeName = new LinkLabel();
            this.llChooseMetadata = new LinkLabel();
            this.panCustomCx = new Panel();
            this.chkEncryptCustomCxString = new CheckBox();
            this.txtCxString = new TextBox();
            this.lblCxString = new Label();
            this.cboProviderName = new ComboBox();
            this.lblProviderName = new Label();
            this.llProxy = new LinkLabel();
            this.panWarning = new FlowLayoutPanel();
            this.lblWarning = new Label();
            this.grpLogon.SuspendLayout();
            this.panMaxDbSize.SuspendLayout();
            this.panSqlAuthentication.SuspendLayout();
            this.panOKCancel.SuspendLayout();
            this.grpDatabase.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panDatabase.SuspendLayout();
            this.grpOptions.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpProvider.SuspendLayout();
            this.panDataContext.SuspendLayout();
            this.panCustomDC.SuspendLayout();
            this.panCustomCx.SuspendLayout();
            this.panWarning.SuspendLayout();
            base.SuspendLayout();
            this.lblServer.AutoSize = true;
            this.lblServer.Dock = DockStyle.Top;
            this.lblServer.Location = new Point(10, 0x1d5);
            this.lblServer.Margin = new Padding(0);
            this.lblServer.Name = "lblServer";
            this.lblServer.Padding = new Padding(0, 8, 0, 1);
            this.lblServer.Size = new Size(0x2f, 0x1c);
            this.lblServer.TabIndex = 3;
            this.lblServer.Text = "&Server";
            this.cboServer.Dock = DockStyle.Top;
            this.cboServer.Location = new Point(10, 0x1f1);
            this.cboServer.Name = "cboServer";
            this.cboServer.Size = new Size(0x1e5, 0x19);
            this.cboServer.TabIndex = 4;
            this.cboServer.Text = @".\SQLEXPRESS";
            this.cboServer.Enter += new EventHandler(this.txtServer_Enter);
            this.cboServer.TextChanged += new EventHandler(this.txtServer_TextChanged);
            this.txtDbFile.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panDatabase.SetColumnSpan(this.txtDbFile, 3);
            this.txtDbFile.Location = new Point(0x18, 0x2f);
            this.txtDbFile.Margin = new Padding(3, 0, 3, 5);
            this.txtDbFile.Name = "txtDbFile";
            this.txtDbFile.Size = new Size(0x1b5, 0x19);
            this.txtDbFile.TabIndex = 4;
            this.txtDbFile.TextChanged += new EventHandler(this.txtDbFile_TextChanged);
            this.btnDbFile.Anchor = AnchorStyles.Right;
            this.btnDbFile.AutoSize = true;
            this.btnDbFile.Location = new Point(0x198, 0x19);
            this.btnDbFile.Name = "btnDbFile";
            this.btnDbFile.Size = new Size(0x35, 0x13);
            this.btnDbFile.TabIndex = 3;
            this.btnDbFile.TabStop = true;
            this.btnDbFile.Text = "Browse";
            this.btnDbFile.TextAlign = ContentAlignment.BottomRight;
            this.btnDbFile.LinkClicked += new LinkLabelLinkClickedEventHandler(this.btnDbFile_LinkClicked);
            this.chkUserInstance.Anchor = AnchorStyles.Left;
            this.chkUserInstance.AutoSize = true;
            this.chkUserInstance.Location = new Point(0x9f, 0x17);
            this.chkUserInstance.Margin = new Padding(6, 0, 3, 0);
            this.chkUserInstance.Name = "chkUserInstance";
            this.chkUserInstance.Padding = new Padding(0, 1, 0, 0);
            this.chkUserInstance.Size = new Size(0x6f, 0x18);
            this.chkUserInstance.TabIndex = 2;
            this.chkUserInstance.Text = "&User Instance";
            this.chkUserInstance.UseVisualStyleBackColor = true;
            this.grpLogon.AutoSize = true;
            this.grpLogon.Controls.Add(this.panMaxDbSize);
            this.grpLogon.Controls.Add(this.panSqlAuthentication);
            this.grpLogon.Controls.Add(this.rbSqlAuth);
            this.grpLogon.Controls.Add(this.rbWindowsAuth);
            this.grpLogon.Dock = DockStyle.Top;
            this.grpLogon.Location = new Point(10, 0x229);
            this.grpLogon.Margin = new Padding(4, 5, 4, 5);
            this.grpLogon.Name = "grpLogon";
            this.grpLogon.Padding = new Padding(11, 3, 10, 3);
            this.grpLogon.Size = new Size(0x1e5, 0xa7);
            this.grpLogon.TabIndex = 6;
            this.grpLogon.TabStop = false;
            this.grpLogon.Text = "Log on details";
            this.panMaxDbSize.AutoSize = true;
            this.panMaxDbSize.ColumnCount = 3;
            this.panMaxDbSize.ColumnStyles.Add(new ColumnStyle());
            this.panMaxDbSize.ColumnStyles.Add(new ColumnStyle());
            this.panMaxDbSize.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 251f));
            this.panMaxDbSize.Controls.Add(this.lblMaxDbSize, 0, 0);
            this.panMaxDbSize.Controls.Add(this.txtMaxDbSize, 1, 0);
            this.panMaxDbSize.Controls.Add(this.lblMaxDbOptional, 2, 0);
            this.panMaxDbSize.Dock = DockStyle.Top;
            this.panMaxDbSize.Location = new Point(11, 0x86);
            this.panMaxDbSize.Margin = new Padding(0);
            this.panMaxDbSize.Name = "panMaxDbSize";
            this.panMaxDbSize.Padding = new Padding(0, 0, 0, 5);
            this.panMaxDbSize.RowCount = 1;
            this.panMaxDbSize.RowStyles.Add(new RowStyle());
            this.panMaxDbSize.Size = new Size(0x1d0, 30);
            this.panMaxDbSize.TabIndex = 4;
            this.lblMaxDbSize.Anchor = AnchorStyles.Left;
            this.lblMaxDbSize.AutoSize = true;
            this.lblMaxDbSize.Location = new Point(0, 3);
            this.lblMaxDbSize.Margin = new Padding(0);
            this.lblMaxDbSize.Name = "lblMaxDbSize";
            this.lblMaxDbSize.Size = new Size(0xa3, 0x13);
            this.lblMaxDbSize.TabIndex = 0;
            this.lblMaxDbSize.Text = "Max Database Size in MB";
            this.txtMaxDbSize.Location = new Point(0xa6, 0);
            this.txtMaxDbSize.Margin = new Padding(3, 0, 3, 0);
            this.txtMaxDbSize.Name = "txtMaxDbSize";
            this.txtMaxDbSize.Size = new Size(0x3a, 0x19);
            this.txtMaxDbSize.TabIndex = 1;
            this.lblMaxDbOptional.Anchor = AnchorStyles.Left;
            this.lblMaxDbOptional.AutoSize = true;
            this.lblMaxDbOptional.Location = new Point(0xe3, 3);
            this.lblMaxDbOptional.Margin = new Padding(0, 0, 3, 0);
            this.lblMaxDbOptional.Name = "lblMaxDbOptional";
            this.lblMaxDbOptional.Size = new Size(0x43, 0x13);
            this.lblMaxDbOptional.TabIndex = 2;
            this.lblMaxDbOptional.Text = "(optional)";
            this.panSqlAuthentication.AutoSize = true;
            this.panSqlAuthentication.ColumnCount = 2;
            this.panSqlAuthentication.ColumnStyles.Add(new ColumnStyle());
            this.panSqlAuthentication.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panSqlAuthentication.Controls.Add(this.lblPassword, 0, 1);
            this.panSqlAuthentication.Controls.Add(this.lblUserName, 0, 0);
            this.panSqlAuthentication.Controls.Add(this.txtUserName, 1, 0);
            this.panSqlAuthentication.Controls.Add(this.txtPassword, 1, 1);
            this.panSqlAuthentication.Dock = DockStyle.Top;
            this.panSqlAuthentication.Location = new Point(11, 0x44);
            this.panSqlAuthentication.Margin = new Padding(4, 5, 4, 5);
            this.panSqlAuthentication.Name = "panSqlAuthentication";
            this.panSqlAuthentication.Padding = new Padding(0x1a, 3, 0, 2);
            this.panSqlAuthentication.RowCount = 3;
            this.panSqlAuthentication.RowStyles.Add(new RowStyle());
            this.panSqlAuthentication.RowStyles.Add(new RowStyle());
            this.panSqlAuthentication.RowStyles.Add(new RowStyle());
            this.panSqlAuthentication.Size = new Size(0x1d0, 0x42);
            this.panSqlAuthentication.TabIndex = 2;
            this.lblPassword.Anchor = AnchorStyles.Left;
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new Point(0x1a, 0x26);
            this.lblPassword.Margin = new Padding(0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Padding = new Padding(0, 0, 0, 2);
            this.lblPassword.Size = new Size(0x43, 0x15);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password";
            this.lblUserName.Anchor = AnchorStyles.Left;
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new Point(0x1a, 8);
            this.lblUserName.Margin = new Padding(0);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Padding = new Padding(0, 0, 0, 2);
            this.lblUserName.Size = new Size(0x4b, 0x15);
            this.lblUserName.TabIndex = 0;
            this.lblUserName.Text = "User name";
            this.txtUserName.Anchor = AnchorStyles.Left;
            this.txtUserName.Location = new Point(0x68, 6);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new Size(0xc1, 0x19);
            this.txtUserName.TabIndex = 1;
            this.txtUserName.TextChanged += new EventHandler(this.EnableControls);
            this.txtPassword.Anchor = AnchorStyles.Left;
            this.txtPassword.Location = new Point(0x68, 0x24);
            this.txtPassword.Margin = new Padding(3, 2, 3, 3);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new Size(0xc1, 0x19);
            this.txtPassword.TabIndex = 3;
            this.rbSqlAuth.AutoSize = true;
            this.rbSqlAuth.Dock = DockStyle.Top;
            this.rbSqlAuth.Location = new Point(11, 0x2c);
            this.rbSqlAuth.Name = "rbSqlAuth";
            this.rbSqlAuth.Padding = new Padding(0, 1, 0, 0);
            this.rbSqlAuth.Size = new Size(0x1d0, 0x18);
            this.rbSqlAuth.TabIndex = 1;
            this.rbSqlAuth.Text = "S&QL Authentication";
            this.rbSqlAuth.UseVisualStyleBackColor = true;
            this.rbSqlAuth.Click += new EventHandler(this.rbSqlAuth_Click);
            this.rbWindowsAuth.AutoSize = true;
            this.rbWindowsAuth.Checked = true;
            this.rbWindowsAuth.Dock = DockStyle.Top;
            this.rbWindowsAuth.Location = new Point(11, 0x15);
            this.rbWindowsAuth.Margin = new Padding(3, 0, 3, 1);
            this.rbWindowsAuth.Name = "rbWindowsAuth";
            this.rbWindowsAuth.Size = new Size(0x1d0, 0x17);
            this.rbWindowsAuth.TabIndex = 0;
            this.rbWindowsAuth.TabStop = true;
            this.rbWindowsAuth.Text = "&Windows Authentication";
            this.rbWindowsAuth.UseVisualStyleBackColor = true;
            this.rbWindowsAuth.Click += new EventHandler(this.rbWindowsAuth_Click);
            this.panOKCancel.Controls.Add(this.btnTest);
            this.panOKCancel.Controls.Add(this.btnOK);
            this.panOKCancel.Controls.Add(this.panel4);
            this.panOKCancel.Controls.Add(this.btnCancel);
            this.panOKCancel.Dock = DockStyle.Top;
            this.panOKCancel.Location = new Point(10, 0x48c);
            this.panOKCancel.Margin = new Padding(4, 5, 4, 0);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 6, 0, 0);
            this.panOKCancel.Size = new Size(0x1e5, 0x23);
            this.panOKCancel.TabIndex = 10;
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
            this.btnOK.Location = new Point(0x135, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x55, 0x1d);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.panel4.Dock = DockStyle.Right;
            this.panel4.Location = new Point(0x18a, 6);
            this.panel4.Margin = new Padding(4, 5, 4, 5);
            this.panel4.Name = "panel4";
            this.panel4.Size = new Size(6, 0x1d);
            this.panel4.TabIndex = 2;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Dock = DockStyle.Right;
            this.btnCancel.Location = new Point(400, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x55, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.grpDatabase.AutoSize = true;
            this.grpDatabase.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.grpDatabase.Controls.Add(this.chkSystemObjects);
            this.grpDatabase.Controls.Add(this.tableLayoutPanel1);
            this.grpDatabase.Controls.Add(this.panDatabase);
            this.grpDatabase.Dock = DockStyle.Top;
            this.grpDatabase.Location = new Point(10, 0x2db);
            this.grpDatabase.Name = "grpDatabase";
            this.grpDatabase.Padding = new Padding(11, 2, 10, 5);
            this.grpDatabase.Size = new Size(0x1e5, 0xef);
            this.grpDatabase.TabIndex = 7;
            this.grpDatabase.TabStop = false;
            this.grpDatabase.Text = "Database";
            this.chkSystemObjects.AutoSize = true;
            this.chkSystemObjects.Dock = DockStyle.Top;
            this.chkSystemObjects.Location = new Point(11, 0xd3);
            this.chkSystemObjects.Name = "chkSystemObjects";
            this.chkSystemObjects.Padding = new Padding(3, 0, 0, 0);
            this.chkSystemObjects.Size = new Size(0x1d0, 0x17);
            this.chkSystemObjects.TabIndex = 2;
            this.chkSystemObjects.Text = "Include System Views and SPs (requires SQL 2005 or later)";
            this.chkSystemObjects.UseVisualStyleBackColor = true;
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.Controls.Add(this.chkLinkedDbs, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.llLinkDbDetails, 1, 0);
            this.tableLayoutPanel1.Dock = DockStyle.Top;
            this.tableLayoutPanel1.Location = new Point(11, 0xb6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.Size = new Size(0x1d0, 0x1d);
            this.tableLayoutPanel1.TabIndex = 1;
            this.chkLinkedDbs.Anchor = AnchorStyles.Left;
            this.chkLinkedDbs.AutoSize = true;
            this.chkLinkedDbs.Location = new Point(3, 5);
            this.chkLinkedDbs.Margin = new Padding(3, 5, 3, 0);
            this.chkLinkedDbs.Name = "chkLinkedDbs";
            this.chkLinkedDbs.Padding = new Padding(0, 1, 0, 0);
            this.chkLinkedDbs.Size = new Size(0xc9, 0x18);
            this.chkLinkedDbs.TabIndex = 0;
            this.chkLinkedDbs.Text = "Include additional databases";
            this.chkLinkedDbs.UseVisualStyleBackColor = true;
            this.chkLinkedDbs.Click += new EventHandler(this.chkLinkedDbs_Click);
            this.llLinkDbDetails.Anchor = AnchorStyles.Right;
            this.llLinkDbDetails.AutoSize = true;
            this.llLinkDbDetails.Location = new Point(0x192, 7);
            this.llLinkDbDetails.Margin = new Padding(3, 5, 3, 0);
            this.llLinkDbDetails.Name = "llLinkDbDetails";
            this.llLinkDbDetails.Size = new Size(0x3b, 0x13);
            this.llLinkDbDetails.TabIndex = 1;
            this.llLinkDbDetails.TabStop = true;
            this.llLinkDbDetails.Text = "Details...";
            this.llLinkDbDetails.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llLinkDbDetails_LinkClicked);
            this.panDatabase.AutoSize = true;
            this.panDatabase.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panDatabase.ColumnCount = 4;
            this.panDatabase.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 21f));
            this.panDatabase.ColumnStyles.Add(new ColumnStyle());
            this.panDatabase.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panDatabase.ColumnStyles.Add(new ColumnStyle());
            this.panDatabase.Controls.Add(this.rbAllDbs, 0, 0);
            this.panDatabase.Controls.Add(this.rbAttach, 0, 1);
            this.panDatabase.Controls.Add(this.cboDatabase, 1, 5);
            this.panDatabase.Controls.Add(this.rbSpecifyDb, 0, 4);
            this.panDatabase.Controls.Add(this.chkUserInstance, 2, 1);
            this.panDatabase.Controls.Add(this.txtDbFile, 1, 2);
            this.panDatabase.Controls.Add(this.btnDbFile, 3, 1);
            this.panDatabase.Controls.Add(this.btnCreateDb, 1, 7);
            this.panDatabase.Controls.Add(this.lblCreateStatus, 2, 7);
            this.panDatabase.Controls.Add(this.lblFetchingDbs, 3, 4);
            this.panDatabase.Dock = DockStyle.Top;
            this.panDatabase.Location = new Point(11, 20);
            this.panDatabase.Name = "panDatabase";
            this.panDatabase.RowCount = 8;
            this.panDatabase.RowStyles.Add(new RowStyle());
            this.panDatabase.RowStyles.Add(new RowStyle());
            this.panDatabase.RowStyles.Add(new RowStyle());
            this.panDatabase.RowStyles.Add(new RowStyle());
            this.panDatabase.RowStyles.Add(new RowStyle());
            this.panDatabase.RowStyles.Add(new RowStyle());
            this.panDatabase.RowStyles.Add(new RowStyle());
            this.panDatabase.RowStyles.Add(new RowStyle());
            this.panDatabase.Size = new Size(0x1d0, 0xa2);
            this.panDatabase.TabIndex = 0;
            this.rbAllDbs.Anchor = AnchorStyles.Left;
            this.rbAllDbs.AutoSize = true;
            this.rbAllDbs.Checked = true;
            this.panDatabase.SetColumnSpan(this.rbAllDbs, 3);
            this.rbAllDbs.Location = new Point(3, 0);
            this.rbAllDbs.Margin = new Padding(3, 0, 3, 0);
            this.rbAllDbs.Name = "rbAllDbs";
            this.rbAllDbs.Size = new Size(0xad, 0x17);
            this.rbAllDbs.TabIndex = 0;
            this.rbAllDbs.TabStop = true;
            this.rbAllDbs.Text = "D&isplay all in a TreeView";
            this.rbAllDbs.UseVisualStyleBackColor = true;
            this.rbAllDbs.Click += new EventHandler(this.EnableControls);
            this.rbAttach.Anchor = AnchorStyles.Left;
            this.rbAttach.AutoSize = true;
            this.panDatabase.SetColumnSpan(this.rbAttach, 2);
            this.rbAttach.Location = new Point(3, 0x17);
            this.rbAttach.Margin = new Padding(3, 0, 3, 0);
            this.rbAttach.Name = "rbAttach";
            this.rbAttach.Size = new Size(0x93, 0x17);
            this.rbAttach.TabIndex = 1;
            this.rbAttach.Text = "&Attach database file";
            this.rbAttach.UseVisualStyleBackColor = true;
            this.rbAttach.Click += new EventHandler(this.rbAttach_Click);
            this.cboDatabase.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panDatabase.SetColumnSpan(this.cboDatabase, 3);
            this.cboDatabase.Enabled = false;
            this.cboDatabase.FormattingEnabled = true;
            this.cboDatabase.Location = new Point(0x18, 0x65);
            this.cboDatabase.Margin = new Padding(3, 0, 3, 3);
            this.cboDatabase.Name = "cboDatabase";
            this.cboDatabase.Size = new Size(0x1b5, 0x19);
            this.cboDatabase.TabIndex = 6;
            this.cboDatabase.DropDown += new EventHandler(this.cboDatabase_DropDown);
            this.cboDatabase.TextChanged += new EventHandler(this.EnableControls);
            this.rbSpecifyDb.Anchor = AnchorStyles.Left;
            this.rbSpecifyDb.AutoSize = true;
            this.panDatabase.SetColumnSpan(this.rbSpecifyDb, 3);
            this.rbSpecifyDb.Location = new Point(3, 0x4e);
            this.rbSpecifyDb.Margin = new Padding(3, 1, 3, 0);
            this.rbSpecifyDb.Name = "rbSpecifyDb";
            this.rbSpecifyDb.Size = new Size(0xe0, 0x17);
            this.rbSpecifyDb.TabIndex = 5;
            this.rbSpecifyDb.Text = "Specif&y new or existing database";
            this.rbSpecifyDb.UseVisualStyleBackColor = true;
            this.rbSpecifyDb.Click += new EventHandler(this.EnableControls);
            this.btnCreateDb.Location = new Point(0x17, 0x83);
            this.btnCreateDb.Margin = new Padding(2, 2, 3, 3);
            this.btnCreateDb.Name = "btnCreateDb";
            this.btnCreateDb.Size = new Size(0x7b, 0x1c);
            this.btnCreateDb.TabIndex = 7;
            this.btnCreateDb.Text = "&Create database";
            this.btnCreateDb.UseVisualStyleBackColor = true;
            this.btnCreateDb.Click += new EventHandler(this.btnCreateDb_Click);
            this.lblCreateStatus.Anchor = AnchorStyles.Left;
            this.lblCreateStatus.AutoSize = true;
            this.panDatabase.SetColumnSpan(this.lblCreateStatus, 2);
            this.lblCreateStatus.ForeColor = Color.Red;
            this.lblCreateStatus.Location = new Point(0x9c, 0x87);
            this.lblCreateStatus.Margin = new Padding(3, 1, 3, 2);
            this.lblCreateStatus.Name = "lblCreateStatus";
            this.lblCreateStatus.Padding = new Padding(9, 0, 0, 0);
            this.lblCreateStatus.Size = new Size(30, 0x13);
            this.lblCreateStatus.TabIndex = 8;
            this.lblCreateStatus.Text = "   ";
            this.lblFetchingDbs.Anchor = AnchorStyles.Right;
            this.lblFetchingDbs.AutoSize = true;
            this.lblFetchingDbs.ForeColor = Color.Red;
            this.lblFetchingDbs.Location = new Point(0x1af, 0x4f);
            this.lblFetchingDbs.Margin = new Padding(3, 1, 3, 2);
            this.lblFetchingDbs.Name = "lblFetchingDbs";
            this.lblFetchingDbs.Padding = new Padding(9, 0, 0, 0);
            this.lblFetchingDbs.Size = new Size(30, 0x13);
            this.lblFetchingDbs.TabIndex = 6;
            this.lblFetchingDbs.Text = "   ";
            this.panSpace1.Dock = DockStyle.Top;
            this.panSpace1.Location = new Point(10, 0x21f);
            this.panSpace1.Name = "panSpace1";
            this.panSpace1.Size = new Size(0x1e5, 10);
            this.panSpace1.TabIndex = 2;
            this.panSpace2.Dock = DockStyle.Top;
            this.panSpace2.Location = new Point(10, 720);
            this.panSpace2.Name = "panSpace2";
            this.panSpace2.Size = new Size(0x1e5, 11);
            this.panSpace2.TabIndex = 4;
            this.chkRemember.AutoSize = true;
            this.chkRemember.Dock = DockStyle.Right;
            this.chkRemember.Location = new Point(0x126, 0);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Padding = new Padding(0, 10, 0, 0);
            this.chkRemember.Size = new Size(0xbf, 40);
            this.chkRemember.TabIndex = 7;
            this.chkRemember.Text = "&Remember this connection";
            this.chkRemember.UseVisualStyleBackColor = true;
            this.chkRemember.Click += new EventHandler(this.EnableControls);
            this.grpOptions.AutoSize = true;
            this.grpOptions.Controls.Add(this.chkSPFunctions);
            this.grpOptions.Controls.Add(this.chkCapitalize);
            this.grpOptions.Controls.Add(this.chkPluralize);
            this.grpOptions.Dock = DockStyle.Top;
            this.grpOptions.Location = new Point(10, 0x3d5);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Size = new Size(0x1e5, 0x5f);
            this.grpOptions.TabIndex = 8;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "Data Context Options";
            this.chkSPFunctions.AutoSize = true;
            this.chkSPFunctions.Checked = true;
            this.chkSPFunctions.CheckState = CheckState.Checked;
            this.chkSPFunctions.Dock = DockStyle.Top;
            this.chkSPFunctions.Location = new Point(3, 0x43);
            this.chkSPFunctions.Name = "chkSPFunctions";
            this.chkSPFunctions.Padding = new Padding(10, 0, 9, 2);
            this.chkSPFunctions.Size = new Size(0x1df, 0x19);
            this.chkSPFunctions.TabIndex = 3;
            this.chkSPFunctions.Text = "Include Stored Procedures and Functions";
            this.chkSPFunctions.UseVisualStyleBackColor = true;
            this.chkCapitalize.AutoSize = true;
            this.chkCapitalize.Dock = DockStyle.Top;
            this.chkCapitalize.Location = new Point(3, 0x2c);
            this.chkCapitalize.Name = "chkCapitalize";
            this.chkCapitalize.Padding = new Padding(10, 0, 9, 0);
            this.chkCapitalize.Size = new Size(0x1df, 0x17);
            this.chkCapitalize.TabIndex = 2;
            this.chkCapitalize.Text = "Capitalize property names";
            this.chkCapitalize.UseVisualStyleBackColor = true;
            this.chkPluralize.AutoSize = true;
            this.chkPluralize.Dock = DockStyle.Top;
            this.chkPluralize.Location = new Point(3, 0x15);
            this.chkPluralize.Name = "chkPluralize";
            this.chkPluralize.Padding = new Padding(10, 0, 9, 0);
            this.chkPluralize.Size = new Size(0x1df, 0x17);
            this.chkPluralize.TabIndex = 1;
            this.chkPluralize.Text = "Pluralize EntitySet and Table properties";
            this.chkPluralize.UseVisualStyleBackColor = true;
            this.panSpace3.Dock = DockStyle.Top;
            this.panSpace3.Location = new Point(10, 970);
            this.panSpace3.Name = "panSpace3";
            this.panSpace3.Size = new Size(0x1e5, 11);
            this.panSpace3.TabIndex = 13;
            this.panel1.Controls.Add(this.chkEncryptTraffic);
            this.panel1.Controls.Add(this.chkRemember);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(10, 0x464);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x1e5, 40);
            this.panel1.TabIndex = 9;
            this.chkEncryptTraffic.AutoSize = true;
            this.chkEncryptTraffic.Dock = DockStyle.Left;
            this.chkEncryptTraffic.Location = new Point(0, 0);
            this.chkEncryptTraffic.Name = "chkEncryptTraffic";
            this.chkEncryptTraffic.Padding = new Padding(0, 10, 0, 0);
            this.chkEncryptTraffic.Size = new Size(0x4c, 40);
            this.chkEncryptTraffic.TabIndex = 0;
            this.chkEncryptTraffic.Text = "Use SS&L";
            this.chkEncryptTraffic.UseVisualStyleBackColor = true;
            this.grpProvider.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.grpProvider.Controls.Add(this.rbCustomCx);
            this.grpProvider.Controls.Add(this.rbAstoria);
            this.grpProvider.Controls.Add(this.rbAzure);
            this.grpProvider.Controls.Add(this.rbSqlCE40);
            this.grpProvider.Controls.Add(this.rbSqlCE);
            this.grpProvider.Controls.Add(this.rbSqlServer);
            this.grpProvider.Dock = DockStyle.Top;
            this.grpProvider.Location = new Point(10, 0xe3);
            this.grpProvider.Name = "grpProvider";
            this.grpProvider.Padding = new Padding(12, 0, 3, 3);
            this.grpProvider.Size = new Size(0x1e5, 0x2e);
            this.grpProvider.TabIndex = 1;
            this.grpProvider.TabStop = false;
            this.grpProvider.Text = "&Provider";
            this.rbCustomCx.AutoSize = true;
            this.rbCustomCx.Dock = DockStyle.Left;
            this.rbCustomCx.Location = new Point(0x1ee, 0x12);
            this.rbCustomCx.Name = "rbCustomCx";
            this.rbCustomCx.Size = new Size(0x3f, 0x19);
            this.rbCustomCx.TabIndex = 5;
            this.rbCustomCx.TabStop = true;
            this.rbCustomCx.Text = "Other";
            this.rbCustomCx.UseVisualStyleBackColor = true;
            this.rbCustomCx.Visible = false;
            this.rbCustomCx.Click += new EventHandler(this.rbCustomCx_Click);
            this.rbAstoria.AutoSize = true;
            this.rbAstoria.Dock = DockStyle.Left;
            this.rbAstoria.Location = new Point(0x1a3, 0x12);
            this.rbAstoria.Name = "rbAstoria";
            this.rbAstoria.Size = new Size(0x4b, 0x19);
            this.rbAstoria.TabIndex = 4;
            this.rbAstoria.TabStop = true;
            this.rbAstoria.Text = "OData  ";
            this.rbAstoria.UseVisualStyleBackColor = true;
            this.rbAstoria.Click += new EventHandler(this.rbAstoria_Click);
            this.rbAzure.AutoSize = true;
            this.rbAzure.Dock = DockStyle.Left;
            this.rbAzure.Location = new Point(320, 0x12);
            this.rbAzure.Name = "rbAzure";
            this.rbAzure.Size = new Size(0x63, 0x19);
            this.rbAzure.TabIndex = 3;
            this.rbAzure.TabStop = true;
            this.rbAzure.Text = "SQL Azure  ";
            this.rbAzure.UseVisualStyleBackColor = true;
            this.rbAzure.Click += new EventHandler(this.rbAzure_Click);
            this.rbSqlCE40.AutoSize = true;
            this.rbSqlCE40.Dock = DockStyle.Left;
            this.rbSqlCE40.Location = new Point(0xd9, 0x12);
            this.rbSqlCE40.Name = "rbSqlCE40";
            this.rbSqlCE40.Size = new Size(0x67, 0x19);
            this.rbSqlCE40.TabIndex = 2;
            this.rbSqlCE40.TabStop = true;
            this.rbSqlCE40.Text = "SQL CE 4.0  ";
            this.rbSqlCE40.UseVisualStyleBackColor = true;
            this.rbSqlCE40.Click += new EventHandler(this.rbSqlCE_Click);
            this.rbSqlCE.AutoSize = true;
            this.rbSqlCE.Dock = DockStyle.Left;
            this.rbSqlCE.Location = new Point(0x72, 0x12);
            this.rbSqlCE.Name = "rbSqlCE";
            this.rbSqlCE.Size = new Size(0x67, 0x19);
            this.rbSqlCE.TabIndex = 1;
            this.rbSqlCE.TabStop = true;
            this.rbSqlCE.Text = "SQL CE 3.5  ";
            this.rbSqlCE.UseVisualStyleBackColor = true;
            this.rbSqlCE.Click += new EventHandler(this.rbSqlCE_Click);
            this.rbSqlServer.AutoSize = true;
            this.rbSqlServer.Checked = true;
            this.rbSqlServer.Dock = DockStyle.Left;
            this.rbSqlServer.Location = new Point(12, 0x12);
            this.rbSqlServer.Name = "rbSqlServer";
            this.rbSqlServer.Size = new Size(0x66, 0x19);
            this.rbSqlServer.TabIndex = 0;
            this.rbSqlServer.TabStop = true;
            this.rbSqlServer.Text = "SQL Server  ";
            this.rbSqlServer.UseVisualStyleBackColor = true;
            this.rbSqlServer.Click += new EventHandler(this.rbSqlServer_Click);
            this.cboSchema.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cboSchema.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboSchema.FormattingEnabled = true;
            this.cboSchema.Items.AddRange(new object[] { "Automatic", "Custom LINQ to SQL DataContext", "Custom Entity Framework ObjectContext" });
            this.cboSchema.Location = new Point(2, 20);
            this.cboSchema.Margin = new Padding(2, 0, 0, 3);
            this.cboSchema.Name = "cboSchema";
            this.cboSchema.Size = new Size(0x1e3, 0x19);
            this.cboSchema.TabIndex = 2;
            this.cboSchema.SelectionChangeCommitted += new EventHandler(this.cboSchema_SelectionChangeCommitted);
            this.cboSchema.Enter += new EventHandler(this.cboSchema_Enter);
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0, 0);
            this.label1.Margin = new Padding(0, 0, 3, 1);
            this.label1.Name = "label1";
            this.label1.Size = new Size(90, 0x13);
            this.label1.TabIndex = 1;
            this.label1.Text = "&Data Context";
            this.panDataContext.AutoSize = true;
            this.panDataContext.ColumnCount = 1;
            this.panDataContext.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panDataContext.Controls.Add(this.cboSchema, 0, 1);
            this.panDataContext.Controls.Add(this.label1, 0, 0);
            this.panDataContext.Dock = DockStyle.Top;
            this.panDataContext.Location = new Point(10, 9);
            this.panDataContext.Name = "panDataContext";
            this.panDataContext.Padding = new Padding(0, 0, 0, 9);
            this.panDataContext.RowCount = 2;
            this.panDataContext.RowStyles.Add(new RowStyle());
            this.panDataContext.RowStyles.Add(new RowStyle());
            this.panDataContext.Size = new Size(0x1e5, 0x39);
            this.panDataContext.TabIndex = 11;
            this.panCustomDC.AutoSize = true;
            this.panCustomDC.ColumnCount = 2;
            this.panCustomDC.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panCustomDC.ColumnStyles.Add(new ColumnStyle());
            this.panCustomDC.Controls.Add(this.label2, 0, 0);
            this.panCustomDC.Controls.Add(this.lblCustomTypeName, 0, 2);
            this.panCustomDC.Controls.Add(this.lblMetadataPath, 0, 4);
            this.panCustomDC.Controls.Add(this.txtAssemblyPath, 0, 1);
            this.panCustomDC.Controls.Add(this.txtTypeName, 0, 3);
            this.panCustomDC.Controls.Add(this.txtMetadataPath, 0, 5);
            this.panCustomDC.Controls.Add(this.llBrowseAssembly, 1, 0);
            this.panCustomDC.Controls.Add(this.llChooseTypeName, 1, 2);
            this.panCustomDC.Controls.Add(this.llChooseMetadata, 1, 4);
            this.panCustomDC.Dock = DockStyle.Top;
            this.panCustomDC.Location = new Point(10, 0x42);
            this.panCustomDC.Name = "panCustomDC";
            this.panCustomDC.Padding = new Padding(0, 0, 0, 5);
            this.panCustomDC.RowCount = 6;
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle());
            this.panCustomDC.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
            this.panCustomDC.Size = new Size(0x1e5, 0xa1);
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
            this.lblCustomTypeName.Size = new Size(0xdf, 0x13);
            this.lblCustomTypeName.TabIndex = 3;
            this.lblCustomTypeName.Text = "&Full Type Name of Custom Context";
            this.lblMetadataPath.AutoSize = true;
            this.lblMetadataPath.Location = new Point(0, 0x6a);
            this.lblMetadataPath.Margin = new Padding(0, 0, 3, 0);
            this.lblMetadataPath.Name = "lblMetadataPath";
            this.lblMetadataPath.Size = new Size(0xa9, 0x13);
            this.lblMetadataPath.TabIndex = 6;
            this.lblMetadataPath.Text = "Path to &Entity Data Model";
            this.txtAssemblyPath.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panCustomDC.SetColumnSpan(this.txtAssemblyPath, 2);
            this.txtAssemblyPath.Location = new Point(1, 20);
            this.txtAssemblyPath.Margin = new Padding(1, 1, 0, 8);
            this.txtAssemblyPath.Name = "txtAssemblyPath";
            this.txtAssemblyPath.Size = new Size(0x1e4, 0x19);
            this.txtAssemblyPath.TabIndex = 2;
            this.txtAssemblyPath.TextChanged += new EventHandler(this.EnableControls);
            this.txtTypeName.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panCustomDC.SetColumnSpan(this.txtTypeName, 2);
            this.txtTypeName.Location = new Point(1, 0x49);
            this.txtTypeName.Margin = new Padding(1, 1, 0, 8);
            this.txtTypeName.Name = "txtTypeName";
            this.txtTypeName.Size = new Size(0x1e4, 0x19);
            this.txtTypeName.TabIndex = 5;
            this.txtTypeName.TextChanged += new EventHandler(this.EnableControls);
            this.txtMetadataPath.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panCustomDC.SetColumnSpan(this.txtMetadataPath, 2);
            this.txtMetadataPath.Location = new Point(1, 0x7e);
            this.txtMetadataPath.Margin = new Padding(1, 1, 0, 5);
            this.txtMetadataPath.Name = "txtMetadataPath";
            this.txtMetadataPath.Size = new Size(0x1e4, 0x19);
            this.txtMetadataPath.TabIndex = 8;
            this.txtMetadataPath.TextChanged += new EventHandler(this.EnableControls);
            this.llBrowseAssembly.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.llBrowseAssembly.AutoSize = true;
            this.llBrowseAssembly.Location = new Point(0x1b0, 0);
            this.llBrowseAssembly.Margin = new Padding(0);
            this.llBrowseAssembly.Name = "llBrowseAssembly";
            this.llBrowseAssembly.Size = new Size(0x35, 0x13);
            this.llBrowseAssembly.TabIndex = 1;
            this.llBrowseAssembly.TabStop = true;
            this.llBrowseAssembly.Text = "Browse";
            this.llBrowseAssembly.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llBrowseAssembly_LinkClicked);
            this.llChooseTypeName.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.llChooseTypeName.AutoSize = true;
            this.llChooseTypeName.Location = new Point(430, 0x35);
            this.llChooseTypeName.Margin = new Padding(0);
            this.llChooseTypeName.Name = "llChooseTypeName";
            this.llChooseTypeName.Size = new Size(0x37, 0x13);
            this.llChooseTypeName.TabIndex = 4;
            this.llChooseTypeName.TabStop = true;
            this.llChooseTypeName.Text = "Choose";
            this.llChooseTypeName.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llChooseTypeName_LinkClicked);
            this.llChooseMetadata.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.llChooseMetadata.AutoSize = true;
            this.llChooseMetadata.Location = new Point(430, 0x6a);
            this.llChooseMetadata.Margin = new Padding(0);
            this.llChooseMetadata.Name = "llChooseMetadata";
            this.llChooseMetadata.Size = new Size(0x37, 0x13);
            this.llChooseMetadata.TabIndex = 7;
            this.llChooseMetadata.TabStop = true;
            this.llChooseMetadata.Text = "Choose";
            this.llChooseMetadata.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llChooseMetadata_LinkClicked);
            this.panCustomCx.AutoSize = true;
            this.panCustomCx.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panCustomCx.Controls.Add(this.chkEncryptCustomCxString);
            this.panCustomCx.Controls.Add(this.txtCxString);
            this.panCustomCx.Controls.Add(this.lblCxString);
            this.panCustomCx.Controls.Add(this.cboProviderName);
            this.panCustomCx.Controls.Add(this.lblProviderName);
            this.panCustomCx.Dock = DockStyle.Top;
            this.panCustomCx.Location = new Point(10, 0x111);
            this.panCustomCx.Name = "panCustomCx";
            this.panCustomCx.Size = new Size(0x1e5, 0xc4);
            this.panCustomCx.TabIndex = 2;
            this.panCustomCx.Visible = false;
            this.chkEncryptCustomCxString.AutoSize = true;
            this.chkEncryptCustomCxString.Dock = DockStyle.Top;
            this.chkEncryptCustomCxString.Location = new Point(0, 0xab);
            this.chkEncryptCustomCxString.Name = "chkEncryptCustomCxString";
            this.chkEncryptCustomCxString.Padding = new Padding(2, 2, 0, 0);
            this.chkEncryptCustomCxString.Size = new Size(0x1e5, 0x19);
            this.chkEncryptCustomCxString.TabIndex = 4;
            this.chkEncryptCustomCxString.Text = "Encrypt connection string when saving";
            this.chkEncryptCustomCxString.UseVisualStyleBackColor = true;
            this.txtCxString.Dock = DockStyle.Top;
            this.txtCxString.Location = new Point(0, 80);
            this.txtCxString.Multiline = true;
            this.txtCxString.Name = "txtCxString";
            this.txtCxString.Size = new Size(0x1e5, 0x5b);
            this.txtCxString.TabIndex = 3;
            this.txtCxString.TextChanged += new EventHandler(this.txtCxString_TextChanged);
            this.lblCxString.AutoSize = true;
            this.lblCxString.Dock = DockStyle.Top;
            this.lblCxString.Location = new Point(0, 0x35);
            this.lblCxString.Margin = new Padding(0);
            this.lblCxString.Name = "lblCxString";
            this.lblCxString.Padding = new Padding(0, 7, 0, 1);
            this.lblCxString.Size = new Size(0x77, 0x1b);
            this.lblCxString.TabIndex = 2;
            this.lblCxString.Text = "&Connection String";
            this.cboProviderName.Dock = DockStyle.Top;
            this.cboProviderName.Location = new Point(0, 0x1c);
            this.cboProviderName.Name = "cboProviderName";
            this.cboProviderName.Size = new Size(0x1e5, 0x19);
            this.cboProviderName.TabIndex = 1;
            this.cboProviderName.TextChanged += new EventHandler(this.EnableControls);
            this.lblProviderName.AutoSize = true;
            this.lblProviderName.Dock = DockStyle.Top;
            this.lblProviderName.Location = new Point(0, 0);
            this.lblProviderName.Margin = new Padding(0);
            this.lblProviderName.Name = "lblProviderName";
            this.lblProviderName.Padding = new Padding(0, 8, 0, 1);
            this.lblProviderName.Size = new Size(100, 0x1c);
            this.lblProviderName.TabIndex = 0;
            this.lblProviderName.Text = "Pr&ovider Name";
            this.llProxy.AutoSize = true;
            this.llProxy.Dock = DockStyle.Top;
            this.llProxy.Location = new Point(10, 0x20a);
            this.llProxy.Name = "llProxy";
            this.llProxy.Padding = new Padding(0, 2, 0, 0);
            this.llProxy.Size = new Size(0x7b, 0x15);
            this.llProxy.TabIndex = 5;
            this.llProxy.TabStop = true;
            this.llProxy.Text = "Web Proxy Setup...";
            this.llProxy.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llProxy_LinkClicked);
            this.panWarning.AutoSize = true;
            this.panWarning.Controls.Add(this.lblWarning);
            this.panWarning.Dock = DockStyle.Top;
            this.panWarning.Location = new Point(10, 0x434);
            this.panWarning.Name = "panWarning";
            this.panWarning.Padding = new Padding(0, 10, 0, 0);
            this.panWarning.Size = new Size(0x1e5, 0x30);
            this.panWarning.TabIndex = 14;
            this.lblWarning.AutoSize = true;
            this.lblWarning.Location = new Point(3, 10);
            this.lblWarning.Margin = new Padding(3, 0, 7, 0);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new Size(360, 0x26);
            this.lblWarning.TabIndex = 9;
            this.lblWarning.Text = "Note: OData works better in LINQPad for Framework 4.0.\r\nYou can download LINQPad 4.x at www.linqpad.net.";
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x221, 0x47a);
            base.ControlBox = false;
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.panel1);
            base.Controls.Add(this.panWarning);
            base.Controls.Add(this.grpOptions);
            base.Controls.Add(this.panSpace3);
            base.Controls.Add(this.grpDatabase);
            base.Controls.Add(this.panSpace2);
            base.Controls.Add(this.grpLogon);
            base.Controls.Add(this.panSpace1);
            base.Controls.Add(this.llProxy);
            base.Controls.Add(this.cboServer);
            base.Controls.Add(this.lblServer);
            base.Controls.Add(this.panCustomCx);
            base.Controls.Add(this.grpProvider);
            base.Controls.Add(this.panCustomDC);
            base.Controls.Add(this.panDataContext);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Margin = new Padding(4, 5, 4, 5);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "CxForm";
            base.Padding = new Padding(10, 9, 10, 9);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "LINQPad Connection";
            this.grpLogon.ResumeLayout(false);
            this.grpLogon.PerformLayout();
            this.panMaxDbSize.ResumeLayout(false);
            this.panMaxDbSize.PerformLayout();
            this.panSqlAuthentication.ResumeLayout(false);
            this.panSqlAuthentication.PerformLayout();
            this.panOKCancel.ResumeLayout(false);
            this.grpDatabase.ResumeLayout(false);
            this.grpDatabase.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panDatabase.ResumeLayout(false);
            this.panDatabase.PerformLayout();
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.grpProvider.ResumeLayout(false);
            this.grpProvider.PerformLayout();
            this.panDataContext.ResumeLayout(false);
            this.panDataContext.PerformLayout();
            this.panCustomDC.ResumeLayout(false);
            this.panCustomDC.PerformLayout();
            this.panCustomCx.ResumeLayout(false);
            this.panCustomCx.PerformLayout();
            this.panWarning.ResumeLayout(false);
            this.panWarning.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private static void InstantiateCustomType()
        {
            Repository data = (Repository) AppDomain.CurrentDomain.GetData("repository");
            string customAssemblyPath = data.CustomAssemblyPath;
            string customTypeName = data.CustomTypeName;
            Type type = Assembly.LoadFrom(customAssemblyPath).GetType(customTypeName);
            if (type == null)
            {
                throw new Exception("Type '" + customTypeName + "' does not exist the specified assembly." + (customTypeName.Contains<char>('.') ? "" : "\r\n(Did you remember to include the type's namespace?)"));
            }
            DataContextDriver driver = data.DriverLoader.Driver;
            string[] paramTypeNames = (from p in driver.GetContextConstructorParameters(data) ?? new ParameterDescriptor[0] select p.FullTypeName).ToArray<string>();
            if (type.GetConstructors().FirstOrDefault<ConstructorInfo>(c => paramTypeNames.SequenceEqual<string>((from p in c.GetParameters() select p.ParameterType.FullName))) == null)
            {
                string message = "Type '" + customTypeName + "' does not define a ";
                ParameterDescriptor[] contextConstructorParameters = driver.GetContextConstructorParameters(data);
                if ((contextConstructorParameters == null) || (contextConstructorParameters.Length == 0))
                {
                    message = message + "parameterless constructor";
                }
                else
                {
                    message = message + "constructor that accepts the following type(s):\r\n  " + string.Join(", ", (from p in contextConstructorParameters select p.FullTypeName).ToArray<string>());
                }
                throw new Exception(message);
            }
            Activator.CreateInstance(type, data.DriverLoader.Driver.GetContextConstructorArguments(data));
        }

        private bool IsDataValid()
        {
            if (this.rbAstoria.Checked)
            {
                return (this.cboServer.Text.Trim().Length > 0);
            }
            if (this.cboSchema.SelectedIndex > 0)
            {
                if (this.txtAssemblyPath.Text.Trim().Length == 0)
                {
                    return false;
                }
                if (this.txtTypeName.Text.Trim().Length == 0)
                {
                    return false;
                }
                if (!(!this.rbAllDbs.Checked || this.rbCustomCx.Checked))
                {
                    return false;
                }
            }
            if ((this.cboSchema.SelectedIndex == 2) && (this.txtMetadataPath.Text.Trim().Length == 0))
            {
                return false;
            }
            if ((this.cboSchema.SelectedIndex == 2) && this.rbCustomCx.Checked)
            {
                return ((this.cboProviderName.Text.Trim().Length > 0) && (this.txtCxString.Text.Trim().Length > 0));
            }
            if (this.rbSqlCE.Checked || this.rbSqlCE40.Checked)
            {
                return (this.txtDbFile.Text.Trim().Length > 0);
            }
            if (this.cboServer.Text.Trim().Length == 0)
            {
                return false;
            }
            if (this.rbAttach.Checked && (this.txtDbFile.Text.Trim().Length == 0))
            {
                return false;
            }
            if (this.rbSpecifyDb.Checked && (this.cboDatabase.Text.Trim().Length == 0))
            {
                return false;
            }
            if (this.rbSqlAuth.Checked && (this.txtUserName.Text.Trim().Length == 0))
            {
                return false;
            }
            return true;
        }

        private void llBrowseAssembly_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.BrowseAssembly();
        }

        private void llChooseMetadata_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ChooseTypeName();
        }

        private void llChooseTypeName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ChooseTypeName();
        }

        private void llLinkDbDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ShowLinkedDbDialog();
        }

        private void llProxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (ProxyForm form = new ProxyForm())
            {
                form.ShowDialog();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!this._activated)
            {
                this._activated = true;
                if (this.cboSchema.SelectedIndex == 0)
                {
                    if (this.rbSqlCE.Checked || this.rbSqlCE40.Checked)
                    {
                        this.txtDbFile.Focus();
                    }
                    else
                    {
                        this.cboServer.Focus();
                    }
                }
                else
                {
                    this.txtAssemblyPath.Focus();
                }
                this.EnsureFormFits();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!((base.DialogResult != DialogResult.OK) || this.IsDataValid()))
            {
                base.DialogResult = DialogResult.Cancel;
            }
            if (base.DialogResult == DialogResult.OK)
            {
                this.UpdateRepository(this._repository);
                if (this._repository.DriverLoader.Driver is AstoriaDynamicDriver)
                {
                    MRU.AstoriaUriNames.RegisterUse(this._repository.Server);
                }
                else if (this.rbAzure.Checked)
                {
                    MRU.AzureServerNames.RegisterUse(this._repository.Server);
                }
                else
                {
                    MRU.ServerNames.RegisterUse(this._repository.Server);
                }
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void PopulateFromRepository()
        {
            if (this._repository.DriverLoader.Driver is LinqToSqlDriver)
            {
                this.cboSchema.SelectedIndex = 1;
            }
            else if (this._repository.DriverLoader.Driver is EntityFrameworkDriver)
            {
                this.cboSchema.SelectedIndex = 2;
                if (this._repository.CustomCxString.Length > 0)
                {
                    this.rbCustomCx.Checked = true;
                    this.cboProviderName.Text = this._repository.Provider;
                    this.txtCxString.Text = this._repository.CustomCxString;
                }
            }
            else
            {
                this.cboSchema.SelectedIndex = 0;
            }
            this._lastSchemaSelectedIndex = this.cboSchema.SelectedIndex;
            this.txtAssemblyPath.Text = this._repository.CustomAssemblyPath;
            this.txtTypeName.Text = this._repository.CustomTypeName;
            this.txtMetadataPath.Text = this._repository.CustomMetadataPath;
            if (this._repository.CustomCxString.Length == 0)
            {
                if (this._repository.DriverLoader.Driver is AstoriaDynamicDriver)
                {
                    this.rbAstoria.Checked = true;
                    this.lblServer.Padding = new Padding(0, 0, 0, 1);
                    this.grpProvider.Hide();
                }
                else
                {
                    this.rbAstoria.Hide();
                    if (this._repository.IsSqlCE40)
                    {
                        this.rbSqlCE40.Checked = true;
                    }
                    else if (this._repository.IsSqlCE)
                    {
                        this.rbSqlCE.Checked = true;
                    }
                    else if (this._repository.IsAzure)
                    {
                        this.rbAzure.Checked = true;
                    }
                    else
                    {
                        this.rbSqlServer.Checked = true;
                    }
                }
            }
            else
            {
                this.rbAstoria.Hide();
                this.chkEncryptCustomCxString.Checked = this._repository.EncryptCustomCxString;
            }
            this.cboServer.Text = this._repository.Server;
            this.rbAttach.Checked = this._repository.AttachFile;
            this.chkUserInstance.Checked = this._repository.UserInstance;
            this.txtDbFile.Text = this._repository.AttachFileName;
            this.rbSqlAuth.Checked = this._repository.SqlSecurity;
            this.cboDatabase.Text = this._repository.Database;
            this.rbSpecifyDb.Checked = this._repository.Database.Length > 0;
            this.txtUserName.Text = this._repository.UserName;
            this.txtPassword.Text = this._repository.Password;
            this.chkPluralize.Checked = !this._repository.NoPluralization;
            this.chkCapitalize.Checked = !this._repository.NoCapitalization;
            this.chkSPFunctions.Checked = !this._repository.ExcludeRoutines;
            this.txtMaxDbSize.Text = (this._repository.MaxDatabaseSize > 0) ? this._repository.MaxDatabaseSize.ToString() : "";
            this.chkSystemObjects.Checked = this._repository.IncludeSystemObjects;
            this._linkedDatabases = this._repository.LinkedDatabases.ToArray<LinkedDatabase>();
            this.chkEncryptTraffic.Checked = this.rbAzure.Checked || this._repository.EncryptTraffic;
            this.chkRemember.Checked = this._repository.Persist;
        }

        private void ProbeDefaultCxString()
        {
            string str = null;
            string str2 = this.txtAssemblyPath.Text.Trim() + ".config";
            if (File.Exists(str2))
            {
                str = str2;
            }
            else
            {
                string directoryName = Path.GetDirectoryName(this.txtAssemblyPath.Text.Trim());
                string[] strArray = (from f in Directory.GetFiles(directoryName, "*.exe.config")
                    where !f.Contains(".vshost.")
                    select f).ToArray<string>();
                if (strArray.Length == 0)
                {
                    strArray = Directory.GetFiles(directoryName, "app.config").ToArray<string>();
                }
                if (strArray.Length == 0)
                {
                    try
                    {
                        strArray = (from fi in new DirectoryInfo(directoryName).Parent.GetFiles("web.config") select fi.FullName).ToArray<string>();
                    }
                    catch
                    {
                    }
                }
                if (strArray.Length == 1)
                {
                    str = strArray[0];
                }
            }
            Dictionary<string, string> cxStringsFromConfig = GetCxStringsFromConfig(str);
            string thirdPartyProvider = null;
            string thirdPartyProviderCxString = null;
            using (DomainIsolator isolator = new DomainIsolator("Default cx string probe", str, null))
            {
                string assemblyPath;
                AppDomain domain = isolator.Domain;
                domain.SetData("assem", this.txtAssemblyPath.Text.Trim());
                domain.SetData("type", this.txtTypeName.Text.Trim());
                domain.DoCallBack(new CrossAppDomainDelegate(CxForm.GetDefaultCxString));
                string cxString = domain.GetData("cxstring") as string;
                string data = domain.GetData("nativecxstring") as string;
                if (!string.IsNullOrEmpty(cxString))
                {
                    if (data != null)
                    {
                        DbConnectionStringBuilder builder = new DbConnectionStringBuilder {
                            ConnectionString = data
                        };
                        if (((builder.Count == 1) && builder.ContainsKey("name")) && cxStringsFromConfig.ContainsKey((string) builder["name"]))
                        {
                            string str5 = cxStringsFromConfig[(string) builder["name"]];
                            builder = new DbConnectionStringBuilder {
                                ConnectionString = str5
                            };
                            if (builder.ContainsKey("provider") || builder.ContainsKey("provider connection string"))
                            {
                                string a = (string) builder["provider"];
                                if (!(string.Equals(a, "System.Data.SqlClient", StringComparison.InvariantCultureIgnoreCase) || string.Equals(a, "System.Data.SqlServerCe.3.5", StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    thirdPartyProvider = a;
                                    thirdPartyProviderCxString = (string) builder["provider connection string"];
                                    if (((thirdPartyProviderCxString != null) && thirdPartyProviderCxString.Contains("|DataDirectory|")) && File.Exists(this.txtAssemblyPath.Text.Trim()))
                                    {
                                        thirdPartyProviderCxString = thirdPartyProviderCxString.Replace("|DataDirectory|", Path.GetDirectoryName(this.txtAssemblyPath.Text.Trim()));
                                    }
                                }
                            }
                        }
                    }
                    assemblyPath = Path.GetDirectoryName(this.txtAssemblyPath.Text.Trim());
                    base.BeginInvoke(delegate {
                        if (!this.IsDisposed && this.AreDbDetailsBlank())
                        {
                            if (((thirdPartyProvider != null) && (thirdPartyProviderCxString != null)) && this.rbCustomCx.Visible)
                            {
                                if (((this.txtCxString.Text.Length <= 0) || (this.cboProviderName.Text.Length <= 0)) || !this.rbCustomCx.Checked)
                                {
                                    this.rbCustomCx.Checked = true;
                                    this.cboProviderName.Text = thirdPartyProvider;
                                    this.txtCxString.Text = thirdPartyProviderCxString;
                                }
                            }
                            else
                            {
                                DbConnectionStringBuilder builder = new DbConnectionStringBuilder {
                                    ConnectionString = cxString
                                };
                                string path = "";
                                if (builder.ContainsKey("Data Source"))
                                {
                                    path = (string) builder["Data Source"];
                                }
                                string str2 = "";
                                if (builder.ContainsKey("Password"))
                                {
                                    str2 = (string) builder["Password"];
                                }
                                if (path.Length != 0)
                                {
                                    if (path.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
                                    {
                                        this.rbSqlCE.Checked = true;
                                        this.txtDbFile.Text = this.DecodeStudioAttachPath(path, assemblyPath);
                                        this.txtPassword.Text = str2;
                                    }
                                    else
                                    {
                                        this.rbSqlServer.Checked = true;
                                        this.cboServer.Text = path;
                                        if (builder.ContainsKey("AttachDbFilename"))
                                        {
                                            this.rbAttach.Checked = true;
                                            this.txtDbFile.Text = this.DecodeStudioAttachPath((string) builder["AttachDbFilename"], assemblyPath);
                                            this.chkUserInstance.Checked = builder.ContainsKey("User Instance") && (builder["User Instance"].ToString().ToLowerInvariant() == "true");
                                        }
                                        else if (builder.ContainsKey("Database"))
                                        {
                                            this.rbSpecifyDb.Checked = true;
                                            this.cboDatabase.Text = (string) builder["Database"];
                                        }
                                        else if (builder.ContainsKey("Initial Catalog"))
                                        {
                                            this.rbSpecifyDb.Checked = true;
                                            this.cboDatabase.Text = (string) builder["Initial Catalog"];
                                        }
                                        if ((builder.ContainsKey("Username") || builder.ContainsKey("uid")) || builder.ContainsKey("user id"))
                                        {
                                            this.rbSqlAuth.Checked = true;
                                            if (builder.ContainsKey("uid"))
                                            {
                                                this.txtUserName.Text = (string) builder["uid"];
                                            }
                                            else if (builder.ContainsKey("user id"))
                                            {
                                                this.txtUserName.Text = (string) builder["user id"];
                                            }
                                            else
                                            {
                                                this.txtUserName.Text = (string) builder["Username"];
                                            }
                                        }
                                        else
                                        {
                                            this.rbWindowsAuth.Checked = true;
                                        }
                                        this.txtPassword.Text = str2;
                                    }
                                    this._lastSetServer = this.cboServer.Text;
                                    this._lastSetDb = this.cboDatabase.Text;
                                    this._lastSetFile = this.txtDbFile.Text;
                                    this._lastSetUserName = this.txtUserName.Text;
                                    this._lastSetPW = this.txtPassword.Text;
                                    this.EnableControls();
                                }
                            }
                        }
                    });
                }
            }
        }

        private void rbAstoria_Click(object sender, EventArgs e)
        {
            if (this._activated)
            {
                this.EnableControls();
                if (!this.cboServer.Text.StartsWith("http"))
                {
                    this.cboServer.Text = "http://";
                }
                this.cboServer.Items.Clear();
                this.cboServer.Items.AddRange(MRU.AstoriaUriNames.GetNames());
                this.cboServer.Focus();
                this.cboServer.SelectionStart = this.cboServer.Text.Length;
            }
        }

        private void rbAttach_Click(object sender, EventArgs e)
        {
            if (this._activated)
            {
                this._testSuccessful = false;
                this.EnableControls();
                this.CheckUserInstanceIfApplicable();
                if (this.rbAttach.Checked)
                {
                    this.txtDbFile.Focus();
                }
            }
        }

        private void rbAzure_Click(object sender, EventArgs e)
        {
            if (this._activated)
            {
                if (this.cboServer.Text.StartsWith("http"))
                {
                    this.cboServer.Text = "";
                }
                this.cboServer.Items.Clear();
                this.cboServer.Items.AddRange(MRU.AzureServerNames.GetNames());
                if (this.cboDatabase.Text.Trim().Length == 0)
                {
                    this.rbAllDbs.Checked = true;
                }
                else
                {
                    this.rbSpecifyDb.Checked = true;
                }
                this._oldEncryptTraffic = new bool?(this.chkEncryptTraffic.Checked);
                this.chkEncryptTraffic.Checked = true;
                this.EnableControls();
            }
        }

        private void rbCustomCx_Click(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void rbSqlAuth_Click(object sender, EventArgs e)
        {
            if (this._activated)
            {
                this._testSuccessful = false;
                this.EnableControls();
                this.txtUserName.Focus();
            }
        }

        private void rbSqlCE_Click(object sender, EventArgs e)
        {
            if (this._activated)
            {
                this.EnableControls();
                if (this.cboServer.Text.StartsWith("http"))
                {
                    this.cboServer.Text = "";
                }
                this.cboServer.Items.Clear();
                this.txtDbFile.Focus();
            }
        }

        private void rbSqlServer_Click(object sender, EventArgs e)
        {
            if (this._activated)
            {
                if (this.txtDbFile.Text.Trim().Length == 0)
                {
                    if (this._allowAllDbs)
                    {
                        this.rbAllDbs.Checked = true;
                    }
                    else
                    {
                        this.rbSpecifyDb.Checked = true;
                    }
                }
                if (this.cboServer.Text.StartsWith("http"))
                {
                    this.cboServer.Text = "";
                }
                this.cboServer.Items.Clear();
                this.cboServer.Items.AddRange(MRU.ServerNames.GetNames());
                if (this._oldEncryptTraffic.HasValue)
                {
                    this.chkEncryptTraffic.Checked = this._oldEncryptTraffic.Value;
                    this._oldEncryptTraffic = null;
                }
                if ((this.rbSqlAuth.Checked && (this.txtUserName.Text.Length == 0)) && (this.txtPassword.Text.Length == 0))
                {
                    this.rbWindowsAuth.Checked = true;
                }
                this.EnableControls();
            }
        }

        private void rbWindowsAuth_Click(object sender, EventArgs e)
        {
            if (this._activated)
            {
                this._testSuccessful = false;
                this.EnableControls();
            }
        }

        private void ShowLinkedDbDialog()
        {
            if (!MainForm.Instance.IsPremium)
            {
                MessageBox.Show("This feature requires a LINQPad Premium Edition license.", "LINQPad");
            }
            else
            {
                Repository r = new Repository();
                this.UpdateRepository(r);
                r.Database = "";
                using (LinkedDbForm form = new LinkedDbForm(new Func<IEnumerable<string>>(r.GetDatabaseList), new Func<IEnumerable<LinkedDatabase>>(r.GetLinkedDatabaseList)))
                {
                    form.LinkedDatabases = this._linkedDatabases;
                    form.ShowDialog();
                    this._linkedDatabases = form.LinkedDatabases.ToArray<LinkedDatabase>();
                }
                this.EnableControls();
            }
        }

        private static string TestAstoria(Repository r)
        {
            try
            {
                return AstoriaHelper.TestConnection(r);
            }
            catch (Exception exception)
            {
                Log.Write(exception);
                return exception.Message;
            }
        }

        private static string TestCustomType(Repository r)
        {
            try
            {
                using (DomainIsolator isolator = new DomainIsolator(r.CreateSchemaAndRunnerDomain("Custom Type Test Domain", false, false)))
                {
                    isolator.Domain.SetData("repository", r);
                    isolator.Domain.DoCallBack(new CrossAppDomainDelegate(CxForm.InstantiateCustomType));
                }
                return null;
            }
            catch (Exception exception)
            {
                string str2 = "The database connection is valid, but the following exception\r\nwas thrown when trying to instantiate the custom " + r.DriverLoader.Driver.ContextBaseTypeName.Split(new char[] { '.' }).Last<string>() + ":\r\n\r\n" + exception.Message;
                if (exception.InnerException != null)
                {
                    str2 = str2 + "\r\n\r\nInner Exception: " + exception.InnerException.Message;
                }
                return str2;
            }
        }

        private void TestCx(Repository r)
        {
            MethodInvoker method = null;
            try
            {
                this._testSuccessful = false;
                Stopwatch stopwatch = Stopwatch.StartNew();
                string text = null;
                string password = r.Password;
                if (r.DriverLoader.Driver is AstoriaDynamicDriver)
                {
                    text = TestAstoria(r);
                }
                else
                {
                    if (r.DriverLoader.Driver.UsesDatabaseConnection)
                    {
                        text = TestDbConnection(r);
                    }
                    if ((text == null) && (r.DriverLoader.Driver is StaticDataContextDriver))
                    {
                        text = TestCustomType(r);
                    }
                }
                if (((this._testCxThread == Thread.CurrentThread) && !base.IsDisposed) && base.Visible)
                {
                    this._testCxThread = null;
                    this._testSuccessful = text == null;
                    if (r.Password != password)
                    {
                        if (method == null)
                        {
                            method = () => this.txtPassword.Text = r.Password;
                        }
                        base.Invoke(method);
                    }
                    base.Invoke(new MethodInvoker(this.EnableControls));
                    if (text != null)
                    {
                        MessageBox.Show(text, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        if (stopwatch.ElapsedMilliseconds < 300L)
                        {
                            Thread.Sleep(300);
                        }
                        MessageBox.Show("Connection Successful.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
            catch (Exception exception)
            {
                Program.ProcessException(exception);
            }
        }

        private static string TestDbConnection(Repository r)
        {
            try
            {
                using (r.Open(true))
                {
                }
                return null;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        private void txtCxString_TextChanged(object sender, EventArgs e)
        {
            if (this._initialized && !((this.txtCxString.Text.Trim().Length != 0) && this.btnOK.Enabled))
            {
                this.EnableControls();
            }
        }

        private void txtDbFile_TextChanged(object sender, EventArgs e)
        {
            if (this._initialized)
            {
                this._testSuccessful = false;
                this.EnableControls();
            }
        }

        private void txtServer_Enter(object sender, EventArgs e)
        {
            this.cboServer.SelectAll();
        }

        private void txtServer_TextChanged(object sender, EventArgs e)
        {
            if (this._initialized)
            {
                this._testSuccessful = false;
                this.EnableControls();
                lock (this._dbLocker)
                {
                    this._databases.Clear();
                    this._dbError = "";
                    this._fetchDbWorker = null;
                    this.lblFetchingDbs.Text = "";
                    this.cboDatabase.Items.Clear();
                }
            }
        }

        private void UpdateCreateMsg(string msg)
        {
            this.lblCreateStatus.Text = msg;
        }

        private void UpdateDbItems()
        {
            this.cboDatabase.Items.Clear();
            this.cboDatabase.Items.AddRange(this._databases.ToArray());
            if (this._dbError.Length == 0)
            {
                this.lblFetchingDbs.Text = "";
            }
            else
            {
                this.lblFetchingDbs.Text = "";
                MessageBox.Show(this._dbError, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void UpdateRepository(Repository r)
        {
            r.DriverData = null;
            if (this.rbAstoria.Checked)
            {
                r.DriverLoader = new AstoriaDynamicDriver().Loader;
            }
            else if (this.cboSchema.SelectedIndex == 1)
            {
                r.DriverLoader = new LinqToSqlDriver().Loader;
            }
            else if (this.cboSchema.SelectedIndex == 2)
            {
                r.DriverLoader = new EntityFrameworkDriver().Loader;
            }
            else
            {
                r.DriverLoader = new LinqToSqlDynamicDriver().Loader;
            }
            if (this.rbAstoria.Checked)
            {
                r.CustomAssemblyPath = r.CustomTypeName = (string) (r.CustomMetadataPath = null);
                r.Provider = "";
                r.AttachFile = false;
                r.UserInstance = false;
                r.AttachFileName = "";
                r.SqlSecurity = false;
                r.Database = "";
                r.ExcludeRoutines = false;
                r.NoCapitalization = false;
                r.NoPluralization = false;
                r.UserName = this.txtUserName.Text;
                r.Password = this.txtPassword.Text;
                r.EncryptCustomCxString = false;
                r.EncryptTraffic = false;
            }
            else
            {
                r.CustomAssemblyPath = this.txtAssemblyPath.Text.Trim();
                r.CustomTypeName = this.txtTypeName.Text.Trim();
                r.CustomMetadataPath = this.txtMetadataPath.Text.Trim();
                r.Provider = this.rbCustomCx.Checked ? this.cboProviderName.Text : (this.rbSqlCE.Checked ? "System.Data.SqlServerCe.3.5" : (this.rbSqlCE40.Checked ? "System.Data.SqlServerCe.4.0" : "System.Data.SqlClient"));
                r.CustomCxString = this.rbCustomCx.Checked ? this.txtCxString.Text : "";
                r.EncryptCustomCxString = this.rbCustomCx.Checked && this.chkEncryptCustomCxString.Checked;
                r.EncryptTraffic = this.chkEncryptTraffic.Checked && this.rbSqlServer.Checked;
                if (this.rbCustomCx.Checked)
                {
                    r.SqlSecurity = false;
                    r.UserInstance = false;
                    r.AttachFile = false;
                    r.AttachFileName = r.Database = r.UserName = r.Password = "";
                    r.LinkedDatabases = null;
                }
                else
                {
                    r.AttachFile = ((!this.rbSqlCE.Checked && !this.rbSqlCE40.Checked) && !this.rbAzure.Checked) && this.rbAttach.Checked;
                    r.UserInstance = (this.rbAttach.Checked && this.rbSqlServer.Checked) && this.chkUserInstance.Checked;
                    r.AttachFileName = ((this.rbAttach.Checked && this.rbSqlServer.Checked) || (this.rbSqlCE.Checked || this.rbSqlCE40.Checked)) ? this.txtDbFile.Text : "";
                    r.SqlSecurity = (!this.rbSqlCE.Checked && !this.rbSqlCE40.Checked) && this.rbSqlAuth.Checked;
                    r.Database = (this.rbSqlCE.Checked || this.rbSqlCE40.Checked) ? "" : (this.rbSpecifyDb.Checked ? this.cboDatabase.Text : "");
                    r.NoPluralization = (this.cboSchema.SelectedIndex <= 0) && !this.chkPluralize.Checked;
                    r.NoCapitalization = (this.cboSchema.SelectedIndex <= 0) && !this.chkCapitalize.Checked;
                    r.ExcludeRoutines = (this.cboSchema.SelectedIndex <= 0) && !this.chkSPFunctions.Checked;
                    r.UserName = ((this.rbWindowsAuth.Checked || this.rbSqlCE.Checked) || this.rbSqlCE40.Checked) ? "" : this.txtUserName.Text;
                    r.Password = (!this.rbWindowsAuth.Checked || (!this.rbSqlServer.Checked && !this.rbAzure.Checked)) ? this.txtPassword.Text : "";
                    r.DbVersion = this.rbAzure.Checked ? "Azure" : "";
                    r.LinkedDatabases = ((this.rbSqlCE.Checked || this.rbSqlCE40.Checked) || !this.chkLinkedDbs.Checked) ? null : ((IEnumerable<LinkedDatabase>) this._linkedDatabases);
                    r.IncludeSystemObjects = (!this.rbSqlCE.Checked && !this.rbSqlCE40.Checked) && this.chkSystemObjects.Checked;
                }
            }
            r.Server = ((this.rbSqlCE.Checked || this.rbSqlCE40.Checked) || this.rbCustomCx.Checked) ? "" : this.cboServer.Text;
            r.Persist = this.chkRemember.Checked;
            r.MaxDatabaseSize = 0;
            if (!((this.rbSqlCE.Checked || this.rbSqlCE40.Checked) ? (this.txtMaxDbSize.Text.Trim().Length <= 0) : true))
            {
                uint num;
                uint.TryParse(this.txtMaxDbSize.Text, out num);
                if (num < 0xf4240)
                {
                    r.MaxDatabaseSize = (int) num;
                }
            }
        }
    }
}

