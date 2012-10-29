namespace LINQPad.UI
{
    using Ionic.Zip;
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Linq;

    internal class BrowseDriversForm : BaseForm
    {
        private static Random _random = new Random();
        private bool _suggestRestart;
        private string _tempDir = Path.Combine(Path.GetTempPath(), "LINQPad");
        private Button btnBrowse;
        private Button btnClose;
        private IContainer components = null;
        private GroupBox groupBox1;
        private Label label1;
        private Label lblSpacer;
        private Panel panBrowser;
        private TableLayoutPanel panOKCancel;
        private WebBrowser webBrowser;

        public BrowseDriversForm(bool suggestRestart)
        {
            this._suggestRestart = suggestRestart;
            this.InitializeComponent();
            this.webBrowser.DocumentStream = new MemoryStream(Encoding.UTF8.GetBytes("Connecting..."));
            this.webBrowser.Navigating += new WebBrowserNavigatingEventHandler(this.webBrowser_Navigating);
            base.Icon = Resources.LINQPad;
            if (!Directory.Exists(this._tempDir))
            {
                Directory.CreateDirectory(this._tempDir);
            }
            Timer tmr = new Timer();
            tmr.Tick += delegate (object sender, EventArgs e) {
                if (!this.IsDisposed)
                {
                    this.webBrowser.Navigate("http://www.linqpad.net/RichClient/DataContextDrivers.aspx");
                }
                tmr.Dispose();
            };
            tmr.Start();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string path = null;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Browse LINQPad Data Context Driver";
                dialog.DefaultExt = ".lpx";
                dialog.Filter = "LINQPad Drivers|*.lpx";
                if (!((dialog.ShowDialog(this) == DialogResult.OK) && File.Exists(dialog.FileName)))
                {
                    return;
                }
                path = dialog.FileName;
            }
            this.TryImportFile(path);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ImportFile(string path)
        {
            using (ZipFile file = new ZipFile(path))
            {
                string targetFolderName;
                ZipEntry entry = file.get_Item("header.xml");
                if (entry == null)
                {
                    MessageBox.Show("Driver is missing header.xml metadata file.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    MemoryStream stream = new MemoryStream();
                    entry.Extract(stream);
                    stream.Position = 0L;
                    string str = (string) XElement.Load(new StreamReader(stream)).Element("MainAssembly");
                    if (string.IsNullOrEmpty(str))
                    {
                        MessageBox.Show("Driver metadata file header.xml is missing the MainAssembly element.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        str = str.Trim();
                        if (Path.GetExtension(str) == "")
                        {
                            str = str + ".dll";
                        }
                        if (file.get_Item(str) == null)
                        {
                            MessageBox.Show("Invalid driver: file '" + str + "' does not exist.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        else
                        {
                            AssemblyName assemblyName;
                            Exception exception;
                            bool flag;
                            file.get_Item(str).Extract(this._tempDir, true);
                            try
                            {
                                assemblyName = AssemblyName.GetAssemblyName(Path.Combine(this._tempDir, str));
                            }
                            catch (Exception exception1)
                            {
                                exception = exception1;
                                MessageBox.Show("Driver contains invalid assembly '" + str + "' - " + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                return;
                            }
                            targetFolderName = assemblyName.Name + " (" + DCDriverLoader.GetPublicKeyToken(assemblyName) + ")";
                            string str2 = Path.Combine(DCDriverLoader.ThirdPartyDriverFolder, targetFolderName);
                            DCDriverLoader.UnloadDomains(true);
                            Thread.Sleep(200);
                            if (!Directory.Exists(DCDriverLoader.ThirdPartyDriverFolder))
                            {
                                Directory.CreateDirectory(DCDriverLoader.ThirdPartyDriverFolder);
                            }
                            FileUtil.AssignUserPermissionsToFolder(DCDriverLoader.ThirdPartyDriverFolder);
                            if (!(flag = Directory.Exists(str2)))
                            {
                                try
                                {
                                    Directory.CreateDirectory(str2);
                                }
                                catch (Exception exception3)
                                {
                                    exception = exception3;
                                    MessageBox.Show("Unable to create directory '" + str2 + "' - " + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                    return;
                                }
                            }
                            FileUtil.AssignUserPermissionsToFolder(str2);
                            new Thread(delegate {
                                try
                                {
                                    WebHelper.GetWebClient().DownloadString("http://www.linqpad.net/RichClient/GetDataContextDriver.aspx?lib=" + targetFolderName);
                                }
                                catch
                                {
                                }
                            }).Start();
                            Exception exception2 = null;
                            ZipEntry entry2 = null;
                            List<ZipEntry> list = (from e in file.get_Entries()
                                where !e.get_IsDirectory()
                                select e).ToList<ZipEntry>();
                            ZipEntry[] entryArray = list.ToArray();
                            int index = 0;
                            while (true)
                            {
                                if (index >= entryArray.Length)
                                {
                                    break;
                                }
                                ZipEntry item = entryArray[index];
                                try
                                {
                                    string str3 = Path.Combine(str2, item.get_FileName());
                                    if (File.Exists(str3))
                                    {
                                        MemoryStream stream2 = new MemoryStream();
                                        item.Extract(stream2);
                                        if (File.ReadAllBytes(str3).SequenceEqual<byte>(stream2.ToArray()))
                                        {
                                            list.Remove(item);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                                index++;
                            }
                            foreach (ZipEntry entry3 in list)
                            {
                                if (!entry3.get_IsDirectory())
                                {
                                    try
                                    {
                                        entry3.Extract(str2, true);
                                    }
                                    catch (Exception exception4)
                                    {
                                        exception = exception4;
                                        if (exception2 == null)
                                        {
                                            exception2 = exception;
                                            entry2 = entry3;
                                        }
                                    }
                                }
                            }
                            if (exception2 != null)
                            {
                                MessageBox.Show("Unable to write file '" + entry2.get_FileName() + "' - " + exception2.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                            else
                            {
                                string text = "Driver successfully " + (flag ? "updated." : "loaded.");
                                MainForm.Instance.RepopulateSchemaTree();
                                if (this._suggestRestart)
                                {
                                    text = text + "\r\n\r\nRestart LINQPad?";
                                    this.DoRestart = MessageBox.Show(text, "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                                }
                                else
                                {
                                    MessageBox.Show(text, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                }
                                base.Close();
                            }
                        }
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.panBrowser = new Panel();
            this.webBrowser = new WebBrowser();
            this.lblSpacer = new Label();
            this.groupBox1 = new GroupBox();
            this.btnBrowse = new Button();
            this.btnClose = new Button();
            this.panOKCancel = new TableLayoutPanel();
            this.label1 = new Label();
            this.panBrowser.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.panBrowser.BorderStyle = BorderStyle.FixedSingle;
            this.panBrowser.Controls.Add(this.webBrowser);
            this.panBrowser.Dock = DockStyle.Fill;
            this.panBrowser.Location = new Point(8, 0x1b);
            this.panBrowser.Name = "panBrowser";
            this.panBrowser.Size = new Size(0x319, 0x21d);
            this.panBrowser.TabIndex = 10;
            this.webBrowser.Dock = DockStyle.Fill;
            this.webBrowser.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser.Location = new Point(0, 0);
            this.webBrowser.MinimumSize = new Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScriptErrorsSuppressed = true;
            this.webBrowser.Size = new Size(0x317, 0x21b);
            this.webBrowser.TabIndex = 0;
            this.webBrowser.WebBrowserShortcutsEnabled = false;
            this.lblSpacer.Dock = DockStyle.Bottom;
            this.lblSpacer.Location = new Point(8, 0x238);
            this.lblSpacer.Name = "lblSpacer";
            this.lblSpacer.Size = new Size(0x319, 8);
            this.lblSpacer.TabIndex = 9;
            this.groupBox1.Controls.Add(this.btnBrowse);
            this.groupBox1.Dock = DockStyle.Bottom;
            this.groupBox1.Location = new Point(8, 0x240);
            this.groupBox1.Margin = new Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new Padding(7, 3, 0, 5);
            this.groupBox1.Size = new Size(0x319, 0x38);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Or, browse to a .LPX file:";
            this.btnBrowse.Dock = DockStyle.Left;
            this.btnBrowse.Location = new Point(7, 0x15);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new Size(90, 30);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
            this.btnClose.DialogResult = DialogResult.OK;
            this.btnClose.Location = new Point(0x2c9, 7);
            this.btnClose.Margin = new Padding(3, 3, 1, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(0x4f, 0x1d);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 3;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnClose, 2, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(8, 0x278);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 4, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x319, 0x27);
            this.panOKCancel.TabIndex = 8;
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(8, 6);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 2);
            this.label1.Size = new Size(0xd7, 0x15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Choose from the featured drivers:";
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnClose;
            base.ClientSize = new Size(0x329, 680);
            base.Controls.Add(this.panBrowser);
            base.Controls.Add(this.lblSpacer);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.label1);
            base.Margin = new Padding(3, 4, 3, 4);
            base.Name = "BrowseDriversForm";
            base.Padding = new Padding(8, 6, 8, 9);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Choose a Driver";
            this.panBrowser.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private bool TryImportFile(string path)
        {
            try
            {
                this.ImportFile(path);
                return true;
            }
            catch (ZipException exception)
            {
                Log.Write(exception, "Import lpx file");
                MessageBox.Show("Invalid driver file", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
        }

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                string path;
                BackgroundWorker worker;
                string uri = e.Url.ToString();
                if (uri.ToLowerInvariant().EndsWith(".lpx"))
                {
                    e.Cancel = true;
                    DCDriverLoader.UnloadDomains(true);
                    path = Path.Combine(this._tempDir, "TempDriver" + _random.Next(0xf4240) + ".lpx");
                    worker = new BackgroundWorker {
                        WorkerSupportsCancellation = true,
                        WorkerReportsProgress = true
                    };
                    worker.DoWork += delegate (object sender, DoWorkEventArgs e) {
                        WebClient webClient = WebHelper.GetWebClient();
                        webClient.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e) {
                            if (worker.IsBusy)
                            {
                                worker.ReportProgress(e.ProgressPercentage);
                            }
                        };
                        webClient.DownloadFileAsync(new Uri(uri), path);
                        while (webClient.IsBusy)
                        {
                            if (worker.CancellationPending)
                            {
                                webClient.CancelAsync();
                                break;
                            }
                            Thread.Sleep(100);
                        }
                    };
                    using (WorkerForm form = new WorkerForm(worker, "Downloading...", true))
                    {
                        if (form.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }
                    }
                    if (!this.TryImportFile(path))
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception exception)
            {
                e.Cancel = true;
                Program.ProcessException(exception);
            }
        }

        public bool DoRestart { get; private set; }
    }
}

