namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.ObjectModel;
    using LINQPad.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;

    internal class NavigateToForm : BaseForm
    {
        private Query[] _allQueries;
        private bool _end;
        private bool _firstSearch = true;
        private static bool _lastMyQueries = true;
        private static bool _lastSamples = false;
        private static bool _lastSearchText = false;
        private object _locker = new object();
        private string _searchString;
        private AutoResetEvent _signal = new AutoResetEvent(false);
        private Thread _worker;
        private Button btnCancel;
        private Button btnOK;
        private ComboBox cboSearch;
        private CheckBox chkMyQueries;
        private CheckBox chkQueryText;
        private CheckBox chkSamples;
        private ColumnHeader clLocation;
        private ColumnHeader clName;
        private IContainer components = null;
        private Label lblResult;
        private Label lblSearch;
        private ListView lstResults;
        private TableLayoutPanel panOKCancel;

        public NavigateToForm()
        {
            this.InitializeComponent();
            base.Icon = Resources.LINQPad;
            Thread thread = new Thread(new ThreadStart(this.TryWork)) {
                IsBackground = true,
                Name = "NavigateToWorker"
            };
            this._worker = thread;
            this._worker.Start();
            this.cboSearch.Items.AddRange(MRU.NavigateTo.GetNames());
            this.chkSamples.Checked = _lastSamples;
            this.chkMyQueries.Checked = _lastMyQueries;
            this.chkQueryText.Checked = _lastSearchText;
            base.KeyPreview = true;
            this.cboSearch.TextChanged += new EventHandler(this.cboSearch_TextChanged);
            if (this.cboSearch.Items.Count > 0)
            {
                this.cboSearch.Text = this.cboSearch.Items[0].ToString();
            }
        }

        private void cboSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (!((((e.KeyData != Keys.Down) && (e.KeyCode != Keys.Up)) || (this.lstResults.Items.Count <= 0)) || this.cboSearch.DroppedDown))
            {
                this.lstResults.Focus();
                e.Handled = true;
            }
        }

        private void cboSearch_TextChanged(object sender, EventArgs e)
        {
            if (this._firstSearch)
            {
                this.lstResults.Items.Add("working...");
                this._firstSearch = false;
            }
            lock (this._locker)
            {
                this._searchString = this.cboSearch.Text.Trim();
            }
            if (this.chkQueryText.Checked)
            {
                this.lblResult.Text = "Result: (working...)";
            }
            this._signal.Set();
        }

        private void chkMyQueries_Click(object sender, EventArgs e)
        {
            this.RepeatSearch();
        }

        private void chkQueryText_Click(object sender, EventArgs e)
        {
            this.RepeatSearch();
        }

        private void chkSamples_Click(object sender, EventArgs e)
        {
            this.RepeatSearch();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panOKCancel = new TableLayoutPanel();
            this.chkQueryText = new CheckBox();
            this.chkSamples = new CheckBox();
            this.chkMyQueries = new CheckBox();
            this.btnCancel = new Button();
            this.btnOK = new Button();
            this.lblSearch = new Label();
            this.cboSearch = new ComboBox();
            this.lblResult = new Label();
            this.lstResults = new ListView();
            this.clName = new ColumnHeader();
            this.clLocation = new ColumnHeader();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 5;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.chkQueryText, 0, 0);
            this.panOKCancel.Controls.Add(this.chkSamples, 0, 0);
            this.panOKCancel.Controls.Add(this.chkMyQueries, 0, 0);
            this.panOKCancel.Controls.Add(this.btnCancel, 4, 0);
            this.panOKCancel.Controls.Add(this.btnOK, 3, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(7, 0x215);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 4, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x2e1, 0x24);
            this.panOKCancel.TabIndex = 4;
            this.chkQueryText.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chkQueryText.AutoSize = true;
            this.chkQueryText.Location = new Point(0x11d, 13);
            this.chkQueryText.Margin = new Padding(3, 3, 3, 0);
            this.chkQueryText.Name = "chkQueryText";
            this.chkQueryText.Size = new Size(0x8b, 0x17);
            this.chkQueryText.TabIndex = 4;
            this.chkQueryText.Text = "Search &Query Text";
            this.chkQueryText.UseVisualStyleBackColor = true;
            this.chkQueryText.Click += new EventHandler(this.chkQueryText_Click);
            this.chkSamples.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chkSamples.AutoSize = true;
            this.chkSamples.Location = new Point(0x9d, 13);
            this.chkSamples.Margin = new Padding(3, 3, 3, 0);
            this.chkSamples.Name = "chkSamples";
            this.chkSamples.Size = new Size(0x7a, 0x17);
            this.chkSamples.TabIndex = 1;
            this.chkSamples.Text = "Search &Samples";
            this.chkSamples.UseVisualStyleBackColor = true;
            this.chkSamples.Click += new EventHandler(this.chkSamples_Click);
            this.chkMyQueries.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chkMyQueries.AutoSize = true;
            this.chkMyQueries.Checked = true;
            this.chkMyQueries.CheckState = CheckState.Checked;
            this.chkMyQueries.Location = new Point(3, 13);
            this.chkMyQueries.Margin = new Padding(3, 3, 3, 0);
            this.chkMyQueries.Name = "chkMyQueries";
            this.chkMyQueries.Padding = new Padding(0, 0, 5, 0);
            this.chkMyQueries.Size = new Size(0x94, 0x17);
            this.chkMyQueries.TabIndex = 0;
            this.chkMyQueries.Text = "Search &My Queries";
            this.chkMyQueries.UseVisualStyleBackColor = true;
            this.chkMyQueries.Click += new EventHandler(this.chkMyQueries_Click);
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x292, 7);
            this.btnCancel.Margin = new Padding(3, 3, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Enabled = false;
            this.btnOK.Location = new Point(0x23d, 7);
            this.btnOK.Margin = new Padding(3, 3, 3, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 0x1d);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.lblSearch.AutoSize = true;
            this.lblSearch.Dock = DockStyle.Top;
            this.lblSearch.Location = new Point(7, 7);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Padding = new Padding(0, 0, 0, 3);
            this.lblSearch.Size = new Size(0x5d, 0x16);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "S&earch Terms:";
            this.cboSearch.Dock = DockStyle.Top;
            this.cboSearch.FormattingEnabled = true;
            this.cboSearch.Location = new Point(7, 0x1d);
            this.cboSearch.Name = "cboSearch";
            this.cboSearch.Size = new Size(0x2e1, 0x19);
            this.cboSearch.TabIndex = 1;
            this.cboSearch.KeyDown += new KeyEventHandler(this.cboSearch_KeyDown);
            this.lblResult.AutoSize = true;
            this.lblResult.Dock = DockStyle.Top;
            this.lblResult.Location = new Point(7, 0x36);
            this.lblResult.Name = "lblResult";
            this.lblResult.Padding = new Padding(0, 5, 0, 3);
            this.lblResult.Size = new Size(0x31, 0x1b);
            this.lblResult.TabIndex = 2;
            this.lblResult.Text = "Result:";
            this.lstResults.Columns.AddRange(new ColumnHeader[] { this.clName, this.clLocation });
            this.lstResults.Dock = DockStyle.Fill;
            this.lstResults.FullRowSelect = true;
            this.lstResults.HideSelection = false;
            this.lstResults.Location = new Point(7, 0x51);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new Size(0x2e1, 0x1c4);
            this.lstResults.TabIndex = 3;
            this.lstResults.UseCompatibleStateImageBehavior = false;
            this.lstResults.View = View.Details;
            this.lstResults.DoubleClick += new EventHandler(this.lstResults_DoubleClick);
            this.clName.Text = "Name";
            this.clName.Width = 0xe8;
            this.clLocation.Text = "Location";
            this.clLocation.Width = 0x126;
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x2ef, 0x240);
            base.ControlBox = false;
            base.Controls.Add(this.lstResults);
            base.Controls.Add(this.lblResult);
            base.Controls.Add(this.cboSearch);
            base.Controls.Add(this.lblSearch);
            base.Controls.Add(this.panOKCancel);
            base.Margin = new Padding(3, 4, 3, 4);
            base.Name = "NavigateToForm";
            base.Padding = new Padding(7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Navigate To";
            this.panOKCancel.ResumeLayout(false);
            this.panOKCancel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void lstResults_DoubleClick(object sender, EventArgs e)
        {
            if (this.lstResults.SelectedItems.Count > 0)
            {
                base.DialogResult = DialogResult.OK;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if ((base.DialogResult == DialogResult.OK) && (this.lstResults.SelectedItems.Count > 0))
            {
                foreach (QueryMatch match in this.lstResults.SelectedItems)
                {
                    match.Query.Open();
                }
                QueryControl qc = MainForm.Instance.CurrentQueryControl;
                if (qc != null)
                {
                    Program.RunOnWinFormsTimer(delegate {
                        if (!qc.IsDisposed)
                        {
                            qc.FocusQuery();
                        }
                    });
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            lock (this._locker)
            {
                this._end = true;
            }
            this._signal.Set();
            if (((this.cboSearch.Text.Trim().Length > 0) && (this.lstResults.Items.Count > 0)) && (base.DialogResult == DialogResult.OK))
            {
                MRU.NavigateTo.RegisterUse(this.cboSearch.Text.Trim());
            }
            if (base.DialogResult == DialogResult.OK)
            {
                _lastMyQueries = this.chkMyQueries.Checked;
                _lastSamples = this.chkSamples.Checked;
                _lastSearchText = this.chkQueryText.Checked;
            }
            base.OnClosing(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.lstResults.Columns[0].Width = (this.lstResults.ClientSize.Width / 2) - 1;
            this.lstResults.Columns[1].Width = this.lstResults.Width;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            Action a = null;
            if (keyData == (Keys.Control | Keys.Shift | Keys.F))
            {
                base.DialogResult = DialogResult.Cancel;
                if (a == null)
                {
                    a = delegate {
                        using (SearchQueries queries = new SearchQueries(this.chkSamples.Checked, this.cboSearch.Text.Trim()))
                        {
                            queries.ShowDialog();
                        }
                    };
                }
                Program.RunOnWinFormsTimer(a);
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void RepeatSearch()
        {
            if (this.cboSearch.Text.Trim().Length > 0)
            {
                if (this.chkQueryText.Checked)
                {
                    this.lblResult.Text = "Result: (working...)";
                }
                this._signal.Set();
            }
            this.cboSearch.Focus();
        }

        private void TryWork()
        {
            try
            {
                this.Work();
            }
            catch (Exception exception)
            {
                Program.ProcessException(exception);
            }
        }

        private void Work()
        {
            object obj2;
            string str;
            <>c__DisplayClass15 class2;
            this._allQueries = Util.GetMyQueries().Concat<Query>(Util.GetSamples()).ToArray<Query>();
            if (base.IsDisposed)
            {
                return;
            }
            this._signal.WaitOne();
        Label_0034:
            class2 = new <>c__DisplayClass15();
            class2.<>4__this = this;
            lock ((obj2 = this._locker))
            {
                if (this._end)
                {
                    return;
                }
                str = this._searchString.Trim();
            }
            class2.words = str.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (str.Length == 0)
            {
                class2.matchingQueries = new Query[0];
            }
            else
            {
                <>c__DisplayClass15 CS$<>8__locals16 = class2;
                Regex[] searchExprs = (from s in class2.words select new Regex(Regex.Escape(s), RegexOptions.IgnoreCase)).ToArray<Regex>();
                IEnumerable<Query> source = this._allQueries;
                if (!this.chkMyQueries.Checked)
                {
                    source = from q in source
                        where !(q is FileQuery)
                        select q;
                }
                if (!this.chkSamples.Checked)
                {
                    source = from q in source
                        where !(q is InbuiltSampleQuery) && !(q is ImportedSampleQuery)
                        select q;
                }
                if (this.chkQueryText.Checked && (str.Length > 1))
                {
                    source = source.Where<Query>(delegate (Query q) {
                        <>c__DisplayClass15 class1 = CS$<>8__locals16;
                        return searchExprs.All<Regex>(se => se.IsMatch(q.Name)) || searchExprs.All<Regex>(se => se.IsMatch(q.Text));
                    });
                }
                else
                {
                    source = source.Where<Query>(delegate (Query q) {
                        <>c__DisplayClass15 class1 = CS$<>8__locals16;
                        return searchExprs.All<Regex>(se => se.IsMatch(q.Name));
                    });
                }
                class2.matchingQueries = (from q in source
                    orderby q.Name
                    select q).ToArray<Query>();
            }
            if (this._signal.WaitOne(0, false))
            {
                goto Label_0034;
            }
            lock ((obj2 = this._locker))
            {
                if (this._end)
                {
                    return;
                }
                goto Label_02B2;
            }
        Label_0229:
            Thread.Sleep(100);
            lock ((obj2 = this._locker))
            {
                if (this._end)
                {
                    return;
                }
                goto Label_02B2;
            }
        Label_025F:
            base.Invoke(new Action(class2.<Work>b__f));
            lock ((obj2 = this._locker))
            {
                if (this._end)
                {
                    return;
                }
            }
            this._signal.WaitOne();
            goto Label_0034;
        Label_02B2:
            if (!base.IsHandleCreated)
            {
                goto Label_0229;
            }
            goto Label_025F;
        }

        private class QueryMatch : ListViewItem
        {
            public readonly LINQPad.ObjectModel.Query Query;

            public QueryMatch(LINQPad.ObjectModel.Query query) : base(new string[] { query.Name, query.Location })
            {
                this.Query = query;
            }
        }
    }
}

