namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    internal class SearchQueries : BaseForm
    {
        private static bool _lastCSharp = !UserOptions.Instance.IsVBDefault;
        private static bool _lastMatchCase;
        private static bool _lastMyQueries = true;
        private static bool _lastRegEx;
        private static bool _lastSamples = true;
        private static bool _lastSQL = true;
        private static bool _lastVB = UserOptions.Instance.IsVBDefault;
        private static bool _lastWholeWords;
        private Button btnCancel;
        private Button btnOK;
        private ComboBox cboText;
        private CheckBox chkCSharp;
        private CheckBox chkMatchCase;
        private CheckBox chkMyQueries;
        private CheckBox chkRegEx;
        private CheckBox chkSamples;
        private CheckBox chkSQL;
        private CheckBox chkVB;
        private CheckBox chkWholeWord;
        private IContainer components;
        private GroupBox groupBox1;
        private GroupBox grpInclude;
        private Label label1;
        private Panel panel1;
        private TableLayoutPanel panOKCancel;
        private TableLayoutPanel tableLayoutPanel1;

        public SearchQueries(bool samples) : this(samples, null)
        {
        }

        public SearchQueries(bool samples, string initialSearch)
        {
            this.components = null;
            this.InitializeComponent();
            string[] names = MRU.FindAll.GetNames();
            if (!string.IsNullOrEmpty(initialSearch))
            {
                this.cboText.Text = initialSearch;
            }
            else if (names.Length > 0)
            {
                this.cboText.Text = names[0];
            }
            this.cboText.Items.AddRange(names);
            this.chkMatchCase.Checked = _lastMatchCase;
            this.chkWholeWord.Checked = _lastWholeWords;
            this.chkRegEx.Checked = _lastRegEx;
            this.chkMyQueries.Checked = _lastMyQueries;
            this.chkSamples.Checked = _lastSamples || samples;
            this.chkCSharp.Checked = _lastCSharp;
            this.chkVB.Checked = _lastVB;
            this.chkSQL.Checked = _lastSQL;
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
            this.btnOK.Enabled = ((this.cboText.Text.Trim().Length > 0) && (this.chkMyQueries.Checked || this.chkSamples.Checked)) && ((this.chkCSharp.Checked || this.chkVB.Checked) || this.chkSQL.Checked);
        }

        private void EnableControls(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void FindAll()
        {
            StringBuilder builder = new StringBuilder("var search = new Regex (");
            if (this.chkWholeWord.Checked)
            {
                builder.Append("@\"\\b\" + ");
            }
            string str = this.cboText.Text.Trim().Replace("\"", "\"\"");
            if (!this.chkRegEx.Checked)
            {
                str = Regex.Escape(str);
            }
            builder.Append("@\"" + str + "\"");
            if (this.chkWholeWord.Checked)
            {
                builder.Append(" + @\"\\b\"");
            }
            if (!this.chkMatchCase.Checked)
            {
                builder.Append(", RegexOptions.IgnoreCase");
            }
            builder.AppendLine(");");
            builder.Append("\r\nvar queries =\r\n\tfrom query in ");
            if (!(!this.chkMyQueries.Checked || this.chkSamples.Checked))
            {
                builder.Append("Util.GetMyQueries()");
            }
            else if (!(this.chkMyQueries.Checked || !this.chkSamples.Checked))
            {
                builder.Append("Util.GetSamples()");
            }
            else
            {
                builder.Append("Util.GetMyQueries().Concat (Util.GetSamples())");
            }
            builder.AppendLine((Environment.Version.Major >= 4) ? ".AsParallel().AsOrdered()" : "");
            if ((!this.chkCSharp.Checked || !this.chkVB.Checked) || !this.chkSQL.Checked)
            {
                List<string> list = new List<string>();
                if (this.chkCSharp.Checked)
                {
                    list.Add("query.IsCSharp");
                }
                if (this.chkVB.Checked)
                {
                    list.Add("query.IsVB");
                }
                if (this.chkSQL.Checked)
                {
                    list.Add("query.IsSQL");
                }
                builder.AppendLine("\twhere " + string.Join(" || ", list.ToArray()));
            }
            builder.AppendLine("\tlet matches = search.Matches (query.Text)\r\n\twhere matches.Count > 0 || search.IsMatch (query.Name)\r\n\tgroup new { Query = query.OpenLink, Matches = query.FormatMatches (matches) } by query.Location;\r\n\r\nforeach (var item in queries)\r\n\titem.ToArray().Dump (item.Key);");
            string name = "Find: " + this.cboText.Text.Trim();
            if (name.Length > 30)
            {
                name = name.Substring(0, 0x19) + "...";
            }
            QueryControl qc = MainForm.Instance.AddQueryPage(false, false, builder.ToString(), 1, name, false, false, false);
            qc.Query.Repository = null;
            qc.Query.ToDataGrids = false;
            qc.Query.IsModified = false;
            qc.Query.OnQueryChanged();
            qc.SetSplitterHeight(0.2f);
            qc.Run();
            Program.RunOnWinFormsTimer(delegate {
                if (!qc.IsDisposed)
                {
                    qc.FocusQuery();
                }
            });
        }

        private void InitializeComponent()
        {
            this.panOKCancel = new TableLayoutPanel();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.label1 = new Label();
            this.cboText = new ComboBox();
            this.chkMatchCase = new CheckBox();
            this.chkWholeWord = new CheckBox();
            this.groupBox1 = new GroupBox();
            this.chkSamples = new CheckBox();
            this.chkMyQueries = new CheckBox();
            this.grpInclude = new GroupBox();
            this.chkSQL = new CheckBox();
            this.chkVB = new CheckBox();
            this.chkCSharp = new CheckBox();
            this.panel1 = new Panel();
            this.chkRegEx = new CheckBox();
            this.panOKCancel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpInclude.SuspendLayout();
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
            this.panOKCancel.Dock = DockStyle.Top;
            this.panOKCancel.Location = new Point(7, 0xf8);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 15, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x1c3, 0x2f);
            this.panOKCancel.TabIndex = 6;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0x10f, 0x12);
            this.btnOK.Margin = new Padding(3, 3, 3, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x5f, 0x1d);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "&Find All";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x174, 0x12);
            this.btnCancel.Margin = new Padding(3, 3, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboText, 1, 0);
            this.tableLayoutPanel1.Dock = DockStyle.Top;
            this.tableLayoutPanel1.Location = new Point(7, 7);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 31f));
            this.tableLayoutPanel1.Size = new Size(0x1c3, 0x1f);
            this.tableLayoutPanel1.TabIndex = 0;
            this.label1.Anchor = AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0, 6);
            this.label1.Margin = new Padding(0, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x48, 0x13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Fi&nd what:";
            this.label1.TextAlign = ContentAlignment.MiddleLeft;
            this.cboText.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cboText.FormattingEnabled = true;
            this.cboText.Location = new Point(0x4e, 3);
            this.cboText.Name = "cboText";
            this.cboText.Size = new Size(370, 0x19);
            this.cboText.TabIndex = 1;
            this.cboText.TextChanged += new EventHandler(this.EnableControls);
            this.chkMatchCase.AutoSize = true;
            this.chkMatchCase.Dock = DockStyle.Top;
            this.chkMatchCase.Location = new Point(7, 0x26);
            this.chkMatchCase.Name = "chkMatchCase";
            this.chkMatchCase.Padding = new Padding(2, 10, 0, 0);
            this.chkMatchCase.Size = new Size(0x1c3, 0x21);
            this.chkMatchCase.TabIndex = 1;
            this.chkMatchCase.Text = "Match &case";
            this.chkMatchCase.UseVisualStyleBackColor = true;
            this.chkWholeWord.AutoSize = true;
            this.chkWholeWord.Dock = DockStyle.Top;
            this.chkWholeWord.Location = new Point(7, 0x47);
            this.chkWholeWord.Name = "chkWholeWord";
            this.chkWholeWord.Padding = new Padding(2, 3, 0, 0);
            this.chkWholeWord.Size = new Size(0x1c3, 0x1a);
            this.chkWholeWord.TabIndex = 2;
            this.chkWholeWord.Text = "Match whole &word";
            this.chkWholeWord.UseVisualStyleBackColor = true;
            this.groupBox1.Controls.Add(this.chkSamples);
            this.groupBox1.Controls.Add(this.chkMyQueries);
            this.groupBox1.Dock = DockStyle.Top;
            this.groupBox1.Location = new Point(7, 0x80);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0x1c3, 0x37);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Look in";
            this.chkSamples.AutoSize = true;
            this.chkSamples.Checked = true;
            this.chkSamples.CheckState = CheckState.Checked;
            this.chkSamples.Dock = DockStyle.Left;
            this.chkSamples.Location = new Point(0x6f, 0x15);
            this.chkSamples.Name = "chkSamples";
            this.chkSamples.Padding = new Padding(12, 0, 0, 0);
            this.chkSamples.Size = new Size(90, 0x1f);
            this.chkSamples.TabIndex = 1;
            this.chkSamples.Text = "&Samples";
            this.chkSamples.UseVisualStyleBackColor = true;
            this.chkSamples.Click += new EventHandler(this.EnableControls);
            this.chkMyQueries.AutoSize = true;
            this.chkMyQueries.Checked = true;
            this.chkMyQueries.CheckState = CheckState.Checked;
            this.chkMyQueries.Dock = DockStyle.Left;
            this.chkMyQueries.Location = new Point(3, 0x15);
            this.chkMyQueries.Name = "chkMyQueries";
            this.chkMyQueries.Padding = new Padding(12, 0, 0, 0);
            this.chkMyQueries.Size = new Size(0x6c, 0x1f);
            this.chkMyQueries.TabIndex = 0;
            this.chkMyQueries.Text = "&My queries";
            this.chkMyQueries.UseVisualStyleBackColor = true;
            this.chkMyQueries.Click += new EventHandler(this.EnableControls);
            this.grpInclude.Controls.Add(this.chkSQL);
            this.grpInclude.Controls.Add(this.chkVB);
            this.grpInclude.Controls.Add(this.chkCSharp);
            this.grpInclude.Dock = DockStyle.Top;
            this.grpInclude.Location = new Point(7, 0xc1);
            this.grpInclude.Name = "grpInclude";
            this.grpInclude.Size = new Size(0x1c3, 0x37);
            this.grpInclude.TabIndex = 5;
            this.grpInclude.TabStop = false;
            this.grpInclude.Text = "Include";
            this.chkSQL.AutoSize = true;
            this.chkSQL.Checked = true;
            this.chkSQL.CheckState = CheckState.Checked;
            this.chkSQL.Dock = DockStyle.Left;
            this.chkSQL.Location = new Point(0xb7, 0x15);
            this.chkSQL.Name = "chkSQL";
            this.chkSQL.Padding = new Padding(12, 0, 0, 0);
            this.chkSQL.Size = new Size(0x66, 0x1f);
            this.chkSQL.TabIndex = 2;
            this.chkSQL.Text = "S&QL/ESQL";
            this.chkSQL.UseVisualStyleBackColor = true;
            this.chkSQL.Click += new EventHandler(this.EnableControls);
            this.chkVB.AutoSize = true;
            this.chkVB.Checked = true;
            this.chkVB.CheckState = CheckState.Checked;
            this.chkVB.Dock = DockStyle.Left;
            this.chkVB.Location = new Point(0x5d, 0x15);
            this.chkVB.Name = "chkVB";
            this.chkVB.Padding = new Padding(12, 0, 0, 0);
            this.chkVB.Size = new Size(90, 0x1f);
            this.chkVB.TabIndex = 1;
            this.chkVB.Text = "VB co&de";
            this.chkVB.UseVisualStyleBackColor = true;
            this.chkVB.Click += new EventHandler(this.EnableControls);
            this.chkCSharp.AutoSize = true;
            this.chkCSharp.Checked = true;
            this.chkCSharp.CheckState = CheckState.Checked;
            this.chkCSharp.Dock = DockStyle.Left;
            this.chkCSharp.Location = new Point(3, 0x15);
            this.chkCSharp.Name = "chkCSharp";
            this.chkCSharp.Padding = new Padding(12, 0, 0, 0);
            this.chkCSharp.Size = new Size(90, 0x1f);
            this.chkCSharp.TabIndex = 0;
            this.chkCSharp.Text = "C# c&ode";
            this.chkCSharp.UseVisualStyleBackColor = true;
            this.chkCSharp.Click += new EventHandler(this.EnableControls);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(7, 0xb7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x1c3, 10);
            this.panel1.TabIndex = 6;
            this.chkRegEx.AutoSize = true;
            this.chkRegEx.Dock = DockStyle.Top;
            this.chkRegEx.Location = new Point(7, 0x61);
            this.chkRegEx.Name = "chkRegEx";
            this.chkRegEx.Padding = new Padding(2, 3, 0, 5);
            this.chkRegEx.Size = new Size(0x1c3, 0x1f);
            this.chkRegEx.TabIndex = 3;
            this.chkRegEx.Text = "Use &regular expressions";
            this.chkRegEx.UseVisualStyleBackColor = true;
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x1d1, 0x134);
            base.ControlBox = false;
            base.Controls.Add(this.panOKCancel);
            base.Controls.Add(this.grpInclude);
            base.Controls.Add(this.panel1);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.chkRegEx);
            base.Controls.Add(this.chkWholeWord);
            base.Controls.Add(this.chkMatchCase);
            base.Controls.Add(this.tableLayoutPanel1);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "SearchQueries";
            base.Padding = new Padding(7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Search All Queries & Samples";
            this.panOKCancel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpInclude.ResumeLayout(false);
            this.grpInclude.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if ((base.DialogResult == DialogResult.OK) && (this.cboText.Text.Trim().Length > 0))
            {
                MRU.FindAll.RegisterUse(this.cboText.Text.Trim());
                _lastMatchCase = this.chkMatchCase.Checked;
                _lastWholeWords = this.chkWholeWord.Checked;
                _lastRegEx = this.chkRegEx.Checked;
                _lastMyQueries = this.chkMyQueries.Checked;
                _lastSamples = this.chkSamples.Checked;
                _lastCSharp = this.chkCSharp.Checked;
                _lastVB = this.chkVB.Checked;
                _lastSQL = this.chkSQL.Checked;
                this.FindAll();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            base.ClientSize = new Size(base.ClientSize.Width, this.panOKCancel.Bottom + base.Padding.Bottom);
        }
    }
}

