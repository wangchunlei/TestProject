namespace LINQPad.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    internal class ChooseTypeForm : BaseForm
    {
        private string _assemPath;
        private string[] _metadataNames;
        private bool _separateArtifacts;
        private string[] _typeNames;
        private Button btnCancel;
        private Button btnOK;
        private IContainer components = null;
        private GroupBox grpCustomType;
        private GroupBox grpMetadata;
        private Label lblConceptual;
        private Label lblMapping;
        private Label lblStore;
        private LinkLabel llBrowseMetadata;
        private ListBox lstConceptual;
        private ListBox lstMapping;
        private ListBox lstMetadata;
        private ListBox lstStore;
        private ListBox lstTypes;
        private Panel panAssemblyEDM;
        private TableLayoutPanel panOKCancel;
        private RadioButton rbAssembly;
        private RadioButton rbFile;
        private SplitContainer splitContainer;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox txtFileMetadata;

        public ChooseTypeForm(string assemPath, string[] typeNames, string existingTypeName, string[] metadataNames, string existingMetadataName)
        {
            this._assemPath = assemPath;
            this._typeNames = typeNames;
            this._metadataNames = metadataNames;
            this.InitializeComponent();
            this.lstTypes.Items.AddRange(this._typeNames);
            if (this._metadataNames != null)
            {
                IGrouping<string, string>[] source = (from <>h__TransparentIdentifier2 in from m in this._metadataNames
                    let dotPos = m.LastIndexOf('.')
                    let stem = m.Substring(0, dotPos)
                    select new { <>h__TransparentIdentifier1 = <>h__TransparentIdentifier1, ext = m.Substring(dotPos + 1) }
                    orderby <>h__TransparentIdentifier2.<>h__TransparentIdentifier1.stem
                    select <>h__TransparentIdentifier2 into <>h__TransparentIdentifier2
                    group <>h__TransparentIdentifier2.ext by <>h__TransparentIdentifier2.<>h__TransparentIdentifier1.stem).ToArray<IGrouping<string, string>>();
                this._separateArtifacts = source.Any<IGrouping<string, string>>(g => g.Count<string>() < 3);
                if (this._separateArtifacts)
                {
                    this.lstConceptual.Items.AddRange((from a in source
                        where a.Contains<string>("csdl", StringComparer.OrdinalIgnoreCase)
                        select a.Key).ToArray<string>());
                    this.lstStore.Items.AddRange((from a in source
                        where a.Contains<string>("ssdl", StringComparer.OrdinalIgnoreCase)
                        select a.Key).ToArray<string>());
                    this.lstMapping.Items.AddRange((from a in source
                        where a.Contains<string>("msl", StringComparer.OrdinalIgnoreCase)
                        select a.Key).ToArray<string>());
                }
                else
                {
                    foreach (Control control in this.panAssemblyEDM.Controls.OfType<Control>().ToArray<Control>())
                    {
                        control.Dispose();
                    }
                    this.panAssemblyEDM.BorderStyle = BorderStyle.None;
                    this.panAssemblyEDM.Padding = new Padding(0);
                    this.rbAssembly.Padding = new Padding(1, 0, 0, 0);
                    ListBox box = new ListBox {
                        Dock = DockStyle.Fill
                    };
                    this.lstMetadata = box;
                    this.lstMetadata.Items.AddRange((from a in source select a.Key).ToArray<string>());
                    this.lstMetadata.SelectedIndexChanged += new EventHandler(this.EnableControls);
                    this.panAssemblyEDM.Controls.Add(this.lstMetadata);
                    base.ClientSize = new Size(base.ClientSize.Width, (base.ClientSize.Height * 2) / 3);
                }
            }
            else
            {
                this.grpCustomType.Parent = this;
                this.splitContainer.Parent = null;
                this.splitContainer.Dispose();
                this.grpCustomType.BringToFront();
                base.Width = (base.Width * 2) / 3;
            }
            if (!(string.IsNullOrEmpty(existingTypeName) || !typeNames.Contains<string>(existingTypeName)))
            {
                this.lstTypes.SelectedItem = existingTypeName;
            }
            else
            {
                this.lstTypes.SelectedIndex = 0;
            }
            if (this._metadataNames != null)
            {
                if (string.IsNullOrEmpty(existingMetadataName))
                {
                    this.rbAssembly.Checked = this._metadataNames.Length > 0;
                    this.rbFile.Checked = this._metadataNames.Length == 0;
                    if (this._separateArtifacts)
                    {
                        if (this.lstConceptual.Items.Count > 0)
                        {
                            this.lstConceptual.SelectedIndex = 0;
                        }
                        if (this.lstStore.Items.Count > 0)
                        {
                            this.lstStore.SelectedIndex = 0;
                        }
                        if (this.lstMapping.Items.Count > 0)
                        {
                            this.lstMapping.SelectedIndex = 0;
                        }
                    }
                    else if (this.lstMetadata.Items.Count > 0)
                    {
                        this.lstMetadata.SelectedIndex = 0;
                    }
                }
                else if (this._separateArtifacts)
                {
                    string[] assemblyBasedMetadataStems = this.GetAssemblyBasedMetadataStems(existingMetadataName);
                    if ((((assemblyBasedMetadataStems != null) && this.lstConceptual.Items.Contains(assemblyBasedMetadataStems[0])) && this.lstMapping.Items.Contains(assemblyBasedMetadataStems[1])) && this.lstStore.Items.Contains(assemblyBasedMetadataStems[2]))
                    {
                        this.lstConceptual.SelectedItem = assemblyBasedMetadataStems[0];
                        this.lstMapping.SelectedItem = assemblyBasedMetadataStems[1];
                        this.lstStore.SelectedItem = assemblyBasedMetadataStems[2];
                        this.rbAssembly.Checked = true;
                    }
                    else
                    {
                        this.txtFileMetadata.Text = existingMetadataName;
                        this.rbFile.Checked = true;
                    }
                }
                else
                {
                    string assemblyBasedMetadataStem = this.GetAssemblyBasedMetadataStem(existingMetadataName);
                    if (!(string.IsNullOrEmpty(assemblyBasedMetadataStem) || !this.lstMetadata.Items.Contains(assemblyBasedMetadataStem)))
                    {
                        this.lstMetadata.SelectedItem = assemblyBasedMetadataStem;
                        this.rbAssembly.Checked = true;
                    }
                    else
                    {
                        this.txtFileMetadata.Text = existingMetadataName;
                        this.rbFile.Checked = true;
                    }
                }
            }
            this.EnableControls();
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
            if (this._separateArtifacts)
            {
                this.lstConceptual.Enabled = this.lstStore.Enabled = this.lstMapping.Enabled = this.rbAssembly.Checked;
            }
            else if (this._metadataNames != null)
            {
                this.lstMetadata.Enabled = this.rbAssembly.Checked;
            }
            this.txtFileMetadata.Enabled = this.llBrowseMetadata.Enabled = this.rbFile.Checked;
            if (this._metadataNames == null)
            {
                this.btnOK.Enabled = this.lstTypes.SelectedIndex >= 0;
            }
            else
            {
                this.btnOK.Enabled = (this.lstTypes.SelectedIndex >= 0) && ((!this.rbAssembly.Checked || !this.MetadataListsValid()) ? (this.rbFile.Checked && (this.txtFileMetadata.Text.Trim().Length > 0)) : true);
            }
        }

        private void EnableControls(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private string GetAssemblyBasedMetadataStem(string metadataName)
        {
            string[] stems = this.GetAssemblyBasedMetadataStems(metadataName);
            if (stems == null)
            {
                return null;
            }
            if (!stems.All<string>(s => (s == stems[0])))
            {
                return null;
            }
            return stems[0];
        }

        private string[] GetAssemblyBasedMetadataStems(string metadataName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this._assemPath);
            Match match = Regex.Match(metadataName, "res://" + fileNameWithoutExtension + @"/(.+?)\|\s*res://" + fileNameWithoutExtension + @"/(.+?)\|\s*res://" + fileNameWithoutExtension + "/(.+)");
            if (!(match.Success && (match.Groups.Count == 4)))
            {
                return null;
            }
            string[] strArray3 = (from g in match.Groups.OfType<Group>().Skip<Group>(1)
                select g.Value.Trim() into v
                orderby v.ToLowerInvariant()
                select v).ToArray<string>();
            if (strArray3[2].EndsWith("|", StringComparison.Ordinal))
            {
                strArray3[2] = strArray3[2].Substring(0, strArray3[2].Length - 1);
            }
            if (!strArray3[0].EndsWith(".csdl", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (!strArray3[1].EndsWith(".msl", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (!strArray3[2].EndsWith(".ssdl", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return (from r in strArray3 select Path.GetFileNameWithoutExtension(r)).ToArray<string>();
        }

        private void InitializeComponent()
        {
            this.panOKCancel = new TableLayoutPanel();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.lstTypes = new ListBox();
            this.splitContainer = new SplitContainer();
            this.grpCustomType = new GroupBox();
            this.grpMetadata = new GroupBox();
            this.panAssemblyEDM = new Panel();
            this.lstMapping = new ListBox();
            this.lblMapping = new Label();
            this.lstStore = new ListBox();
            this.lblStore = new Label();
            this.lstConceptual = new ListBox();
            this.lblConceptual = new Label();
            this.rbAssembly = new RadioButton();
            this.rbFile = new RadioButton();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.llBrowseMetadata = new LinkLabel();
            this.txtFileMetadata = new TextBox();
            this.panOKCancel.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.grpCustomType.SuspendLayout();
            this.grpMetadata.SuspendLayout();
            this.panAssemblyEDM.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 4;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnOK, 2, 0);
            this.panOKCancel.Controls.Add(this.btnCancel, 3, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(6, 530);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 5, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x2a1, 0x26);
            this.panOKCancel.TabIndex = 1;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0x1fd, 8);
            this.btnOK.Margin = new Padding(3, 3, 3, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 30);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x252, 8);
            this.btnCancel.Margin = new Padding(3, 3, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.lstTypes.Dock = DockStyle.Fill;
            this.lstTypes.FormattingEnabled = true;
            this.lstTypes.IntegralHeight = false;
            this.lstTypes.ItemHeight = 0x11;
            this.lstTypes.Location = new Point(8, 0x16);
            this.lstTypes.Name = "lstTypes";
            this.lstTypes.Size = new Size(0x134, 0x1ed);
            this.lstTypes.TabIndex = 0;
            this.lstTypes.SelectedIndexChanged += new EventHandler(this.EnableControls);
            this.lstTypes.DoubleClick += new EventHandler(this.lstTypes_DoubleClick);
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.Location = new Point(6, 7);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Panel1.Controls.Add(this.grpCustomType);
            this.splitContainer.Panel2.Controls.Add(this.grpMetadata);
            this.splitContainer.Size = new Size(0x2a1, 0x20b);
            this.splitContainer.SplitterDistance = 0x144;
            this.splitContainer.SplitterWidth = 7;
            this.splitContainer.TabIndex = 0;
            this.grpCustomType.Controls.Add(this.lstTypes);
            this.grpCustomType.Dock = DockStyle.Fill;
            this.grpCustomType.Location = new Point(0, 0);
            this.grpCustomType.Name = "grpCustomType";
            this.grpCustomType.Padding = new Padding(8, 4, 8, 8);
            this.grpCustomType.Size = new Size(0x144, 0x20b);
            this.grpCustomType.TabIndex = 0;
            this.grpCustomType.TabStop = false;
            this.grpCustomType.Text = "Custom Type Name";
            this.grpMetadata.Controls.Add(this.panAssemblyEDM);
            this.grpMetadata.Controls.Add(this.rbAssembly);
            this.grpMetadata.Controls.Add(this.rbFile);
            this.grpMetadata.Controls.Add(this.tableLayoutPanel1);
            this.grpMetadata.Dock = DockStyle.Fill;
            this.grpMetadata.Location = new Point(0, 0);
            this.grpMetadata.Name = "grpMetadata";
            this.grpMetadata.Padding = new Padding(8, 4, 8, 7);
            this.grpMetadata.Size = new Size(0x156, 0x20b);
            this.grpMetadata.TabIndex = 0;
            this.grpMetadata.TabStop = false;
            this.grpMetadata.Text = "Entity Data Model";
            this.panAssemblyEDM.BorderStyle = BorderStyle.FixedSingle;
            this.panAssemblyEDM.Controls.Add(this.lstMapping);
            this.panAssemblyEDM.Controls.Add(this.lblMapping);
            this.panAssemblyEDM.Controls.Add(this.lstStore);
            this.panAssemblyEDM.Controls.Add(this.lblStore);
            this.panAssemblyEDM.Controls.Add(this.lstConceptual);
            this.panAssemblyEDM.Controls.Add(this.lblConceptual);
            this.panAssemblyEDM.Dock = DockStyle.Fill;
            this.panAssemblyEDM.Location = new Point(8, 0x2f);
            this.panAssemblyEDM.Name = "panAssemblyEDM";
            this.panAssemblyEDM.Padding = new Padding(6, 3, 6, 6);
            this.panAssemblyEDM.Size = new Size(0x146, 0x189);
            this.panAssemblyEDM.TabIndex = 4;
            this.panAssemblyEDM.Layout += new LayoutEventHandler(this.panAssemblyEDM_Layout);
            this.lstMapping.Dock = DockStyle.Top;
            this.lstMapping.FormattingEnabled = true;
            this.lstMapping.IntegralHeight = false;
            this.lstMapping.ItemHeight = 0x11;
            this.lstMapping.Location = new Point(6, 0xf2);
            this.lstMapping.Name = "lstMapping";
            this.lstMapping.Size = new Size(0x138, 0x59);
            this.lstMapping.TabIndex = 5;
            this.lstMapping.SelectedIndexChanged += new EventHandler(this.EnableControls);
            this.lblMapping.AutoSize = true;
            this.lblMapping.Dock = DockStyle.Top;
            this.lblMapping.Location = new Point(6, 0xdd);
            this.lblMapping.Name = "lblMapping";
            this.lblMapping.Padding = new Padding(0, 2, 0, 0);
            this.lblMapping.Size = new Size(0x43, 0x15);
            this.lblMapping.TabIndex = 4;
            this.lblMapping.Text = "Mapping:";
            this.lstStore.Dock = DockStyle.Top;
            this.lstStore.FormattingEnabled = true;
            this.lstStore.IntegralHeight = false;
            this.lstStore.ItemHeight = 0x11;
            this.lstStore.Location = new Point(6, 0x84);
            this.lstStore.Name = "lstStore";
            this.lstStore.Size = new Size(0x138, 0x59);
            this.lstStore.TabIndex = 3;
            this.lstStore.SelectedIndexChanged += new EventHandler(this.EnableControls);
            this.lblStore.AutoSize = true;
            this.lblStore.Dock = DockStyle.Top;
            this.lblStore.Location = new Point(6, 0x6f);
            this.lblStore.Name = "lblStore";
            this.lblStore.Padding = new Padding(0, 2, 0, 0);
            this.lblStore.Size = new Size(0x5f, 0x15);
            this.lblStore.TabIndex = 2;
            this.lblStore.Text = "Store Schema:";
            this.lstConceptual.Dock = DockStyle.Top;
            this.lstConceptual.FormattingEnabled = true;
            this.lstConceptual.IntegralHeight = false;
            this.lstConceptual.ItemHeight = 0x11;
            this.lstConceptual.Location = new Point(6, 0x16);
            this.lstConceptual.Name = "lstConceptual";
            this.lstConceptual.Size = new Size(0x138, 0x59);
            this.lstConceptual.TabIndex = 1;
            this.lstConceptual.SelectedIndexChanged += new EventHandler(this.EnableControls);
            this.lblConceptual.AutoSize = true;
            this.lblConceptual.Dock = DockStyle.Top;
            this.lblConceptual.Location = new Point(6, 3);
            this.lblConceptual.Name = "lblConceptual";
            this.lblConceptual.Size = new Size(0x84, 0x13);
            this.lblConceptual.TabIndex = 0;
            this.lblConceptual.Text = "Conceptual Schema:";
            this.rbAssembly.AutoSize = true;
            this.rbAssembly.Dock = DockStyle.Top;
            this.rbAssembly.Location = new Point(8, 0x16);
            this.rbAssembly.Name = "rbAssembly";
            this.rbAssembly.Padding = new Padding(1, 0, 0, 2);
            this.rbAssembly.Size = new Size(0x146, 0x19);
            this.rbAssembly.TabIndex = 0;
            this.rbAssembly.TabStop = true;
            this.rbAssembly.Text = "From Same Assembly";
            this.rbAssembly.UseVisualStyleBackColor = true;
            this.rbAssembly.Click += new EventHandler(this.EnableControls);
            this.rbFile.AutoSize = true;
            this.rbFile.Dock = DockStyle.Bottom;
            this.rbFile.Location = new Point(8, 440);
            this.rbFile.Name = "rbFile";
            this.rbFile.Padding = new Padding(1, 7, 0, 0);
            this.rbFile.Size = new Size(0x146, 30);
            this.rbFile.TabIndex = 2;
            this.rbFile.TabStop = true;
            this.rbFile.Text = "From File or Other Source:";
            this.rbFile.UseVisualStyleBackColor = true;
            this.rbFile.Click += new EventHandler(this.rbFile_Click);
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.Controls.Add(this.llBrowseMetadata, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtFileMetadata, 0, 0);
            this.tableLayoutPanel1.Dock = DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new Point(8, 470);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
            this.tableLayoutPanel1.Size = new Size(0x146, 0x2e);
            this.tableLayoutPanel1.TabIndex = 3;
            this.llBrowseMetadata.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.llBrowseMetadata.AutoSize = true;
            this.llBrowseMetadata.Location = new Point(0xed, 0x1b);
            this.llBrowseMetadata.Margin = new Padding(3, 0, 0, 0);
            this.llBrowseMetadata.Name = "llBrowseMetadata";
            this.llBrowseMetadata.Size = new Size(0x59, 0x13);
            this.llBrowseMetadata.TabIndex = 1;
            this.llBrowseMetadata.TabStop = true;
            this.llBrowseMetadata.Text = "Browse files...";
            this.llBrowseMetadata.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llBrowseMetadata_LinkClicked);
            this.txtFileMetadata.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtFileMetadata.Location = new Point(0, 1);
            this.txtFileMetadata.Margin = new Padding(0, 1, 0, 1);
            this.txtFileMetadata.Name = "txtFileMetadata";
            this.txtFileMetadata.Size = new Size(0x146, 0x19);
            this.txtFileMetadata.TabIndex = 0;
            this.txtFileMetadata.TextChanged += new EventHandler(this.EnableControls);
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x2ad, 0x23f);
            base.ControlBox = false;
            base.Controls.Add(this.splitContainer);
            base.Controls.Add(this.panOKCancel);
            base.Margin = new Padding(3, 4, 3, 4);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ChooseTypeForm";
            base.Padding = new Padding(6, 7, 6, 7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Choose Custom Type";
            this.panOKCancel.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.grpCustomType.ResumeLayout(false);
            this.grpMetadata.ResumeLayout(false);
            this.grpMetadata.PerformLayout();
            this.panAssemblyEDM.ResumeLayout(false);
            this.panAssemblyEDM.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void llBrowseMetadata_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (File.Exists(this._assemPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(this._assemPath);
                }
                dialog.DefaultExt = "csdl";
                dialog.Multiselect = true;
                dialog.Filter = "Entity Framework Metadata files|*.csdl;*.msl;*.ssdl";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string str2;
                    if (dialog.FileNames.Length == 1)
                    {
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dialog.FileName);
                        str2 = fileNameWithoutExtension + ".csdl|" + fileNameWithoutExtension + ".msl|" + fileNameWithoutExtension + ".ssdl";
                    }
                    else
                    {
                        str2 = string.Join("|", dialog.FileNames);
                    }
                    this.txtFileMetadata.Text = str2;
                }
            }
        }

        private void lstTypes_DoubleClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
        }

        private bool MetadataListsValid()
        {
            if (this._separateArtifacts)
            {
                return (((this.lstConceptual.SelectedIndex >= 0) && (this.lstMapping.SelectedIndex >= 0)) && (this.lstStore.SelectedIndex >= 0));
            }
            if (this._metadataNames != null)
            {
                return (this.lstMetadata.SelectedIndex >= 0);
            }
            return true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (base.DialogResult == DialogResult.OK)
            {
                this.SelectedTypeName = (string) this.lstTypes.SelectedItem;
            }
            base.OnClosing(e);
        }

        private void panAssemblyEDM_Layout(object sender, LayoutEventArgs e)
        {
            if (this._separateArtifacts)
            {
                Panel panAssemblyEDM = this.panAssemblyEDM;
                int num = ((((panAssemblyEDM.ClientSize.Height - panAssemblyEDM.Padding.Top) - panAssemblyEDM.Padding.Bottom) - this.lblConceptual.Height) - this.lblStore.Height) - this.lblMapping.Height;
                this.lstConceptual.Height = num / 3;
                num -= this.lstConceptual.Height;
                this.lstStore.Height = num / 2;
                num -= this.lstStore.Height;
                this.lstMapping.Height = num;
            }
        }

        private void rbFile_Click(object sender, EventArgs e)
        {
            this.EnableControls();
            this.txtFileMetadata.Focus();
        }

        public string SelectedMetadataName
        {
            get
            {
                if (this.rbFile.Checked || (this._metadataNames == null))
                {
                    return this.txtFileMetadata.Text.Trim();
                }
                if (!this.MetadataListsValid())
                {
                    return "";
                }
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this._assemPath);
                if (this._separateArtifacts)
                {
                    return ("res://" + fileNameWithoutExtension + "/" + this.lstConceptual.SelectedItem.ToString() + ".csdl|res://" + fileNameWithoutExtension + "/" + this.lstStore.SelectedItem.ToString() + ".ssdl|res://" + fileNameWithoutExtension + "/" + this.lstMapping.SelectedItem.ToString() + ".msl");
                }
                string str3 = this.lstMetadata.SelectedItem.ToString();
                return ("res://" + fileNameWithoutExtension + "/" + str3 + ".csdl|res://" + fileNameWithoutExtension + "/" + str3 + ".ssdl|res://" + fileNameWithoutExtension + "/" + str3 + ".msl");
            }
        }

        public string SelectedTypeName { get; private set; }
    }
}

