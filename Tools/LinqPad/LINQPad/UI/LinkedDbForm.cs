namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    internal class LinkedDbForm : BaseForm
    {
        private string[] _allDatabases;
        private LinkedDatabase[] _allLinkedDatabases;
        private Func<IEnumerable<string>> _dbFetcher;
        private Func<IEnumerable<LinkedDatabase>> _linkedDbFetcher;
        private Button btnCancel;
        private Button btnDeleteOther;
        private Button btnDeleteSame;
        private Button btnPickOther;
        private Button btnPickSame;
        private DataGridViewTextBoxColumn colDb;
        private DataGridViewTextBoxColumn coLinkedlServer;
        private DataGridViewTextBoxColumn colLinkedDb;
        private IContainer components = null;
        private DataGridView grdOther;
        private DataGridView grdSame;
        private GroupBox grpOtherServers;
        private GroupBox grpSameSever;
        private TableLayoutPanel panOKCancel;
        private Panel panSpacer;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;

        public LinkedDbForm(Func<IEnumerable<string>> dbFetcher, Func<IEnumerable<LinkedDatabase>> linkedDbFetcher)
        {
            this._dbFetcher = dbFetcher;
            this._linkedDbFetcher = linkedDbFetcher;
            this.InitializeComponent();
        }

        private void btnDeleteOther_Click(object sender, EventArgs e)
        {
            try
            {
                this.grdOther.Rows.RemoveAt(this.grdOther.CurrentRow.Index);
            }
            catch
            {
            }
        }

        private void btnDeleteSame_Click(object sender, EventArgs e)
        {
            try
            {
                this.grdSame.Rows.RemoveAt(this.grdSame.CurrentRow.Index);
            }
            catch
            {
            }
        }

        private void btnPickOther_Click(object sender, EventArgs e)
        {
            LinkedDatabase[] items = new LinkedDatabase[] { new LinkedDatabase("Populating list...") };
            if (this._allLinkedDatabases != null)
            {
                items = this._allLinkedDatabases;
            }
            ThreadStart start = null;
            using (ListPicker picker = new ListPicker(items, true))
            {
                if (this._allLinkedDatabases == null)
                {
                    if (start == null)
                    {
                        start = delegate {
                            Action method = null;
                            Action action2 = null;
                            try
                            {
                                this._allLinkedDatabases = this._linkedDbFetcher().ToArray<LinkedDatabase>();
                                if (((this._allLinkedDatabases != null) && this._allLinkedDatabases.Any<LinkedDatabase>()) && !picker.IsDisposed)
                                {
                                    if (method == null)
                                    {
                                        method = () => picker.AllItems = this._allLinkedDatabases;
                                    }
                                    picker.Invoke(method);
                                }
                            }
                            catch (Exception exception)
                            {
                                if (!picker.IsDisposed)
                                {
                                    if (!picker.IsDisposed)
                                    {
                                        if (action2 == null)
                                        {
                                            action2 = () => picker.AllItems = null;
                                        }
                                        picker.Invoke(action2);
                                    }
                                    MessageBox.Show("Error: " + exception.Message, "LINQPad");
                                }
                            }
                        };
                    }
                    new Thread(start).Start();
                }
                if ((picker.ShowDialog() == DialogResult.OK) && (picker.SelectedItems != null))
                {
                    foreach (LinkedDatabase database in picker.SelectedItems)
                    {
                        this.grdOther.Rows.Add(new object[] { database.Server, database.Database });
                    }
                }
            }
        }

        private void btnPickSame_Click(object sender, EventArgs e)
        {
            string[] items = new string[] { "Populating list..." };
            if (this._allDatabases != null)
            {
                items = this._allDatabases;
            }
            ThreadStart start = null;
            using (ListPicker picker = new ListPicker(items, true))
            {
                if (this._allDatabases == null)
                {
                    if (start == null)
                    {
                        start = delegate {
                            Action method = null;
                            Action action2 = null;
                            try
                            {
                                this._allDatabases = this._dbFetcher().ToArray<string>();
                                if (((this._allDatabases != null) && this._allDatabases.Any<string>()) && !picker.IsDisposed)
                                {
                                    if (method == null)
                                    {
                                        method = () => picker.AllItems = this._allDatabases;
                                    }
                                    picker.Invoke(method);
                                }
                            }
                            catch (Exception exception)
                            {
                                if (!picker.IsDisposed)
                                {
                                    if (!picker.IsDisposed)
                                    {
                                        if (action2 == null)
                                        {
                                            action2 = () => picker.AllItems = null;
                                        }
                                        picker.Invoke(action2);
                                    }
                                    MessageBox.Show("Error: " + exception.Message, "LINQPad");
                                }
                            }
                        };
                    }
                    new Thread(start).Start();
                }
                if ((picker.ShowDialog() == DialogResult.OK) && (picker.SelectedItems != null))
                {
                    foreach (string str in picker.SelectedItems)
                    {
                        this.grdSame.Rows.Add(new object[] { str });
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

        private void grdOther_Enter(object sender, EventArgs e)
        {
        }

        private void grdSame_Enter(object sender, EventArgs e)
        {
        }

        private void grpOtherServers_Leave(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in (IEnumerable) this.grdOther.Rows)
            {
                row.Selected = false;
            }
        }

        private void grpSameSever_Leave(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in (IEnumerable) this.grdSame.Rows)
            {
                row.Selected = false;
            }
        }

        private void InitializeComponent()
        {
            this.grdSame = new DataGridView();
            this.colDb = new DataGridViewTextBoxColumn();
            this.grdOther = new DataGridView();
            this.coLinkedlServer = new DataGridViewTextBoxColumn();
            this.colLinkedDb = new DataGridViewTextBoxColumn();
            this.grpSameSever = new GroupBox();
            this.panOKCancel = new TableLayoutPanel();
            this.btnDeleteSame = new Button();
            this.btnPickSame = new Button();
            this.grpOtherServers = new GroupBox();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.btnDeleteOther = new Button();
            this.btnPickOther = new Button();
            this.panSpacer = new Panel();
            this.tableLayoutPanel2 = new TableLayoutPanel();
            this.btnCancel = new Button();
            ((ISupportInitialize) this.grdSame).BeginInit();
            ((ISupportInitialize) this.grdOther).BeginInit();
            this.grpSameSever.SuspendLayout();
            this.panOKCancel.SuspendLayout();
            this.grpOtherServers.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            base.SuspendLayout();
            this.grdSame.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdSame.Columns.AddRange(new DataGridViewColumn[] { this.colDb });
            this.grdSame.Dock = DockStyle.Fill;
            this.grdSame.Location = new Point(10, 0x18);
            this.grdSame.Name = "grdSame";
            this.grdSame.Size = new Size(570, 0xb5);
            this.grdSame.TabIndex = 0;
            this.grdSame.Enter += new EventHandler(this.grdSame_Enter);
            this.colDb.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colDb.HeaderText = "Database";
            this.colDb.Name = "colDb";
            this.grdOther.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdOther.Columns.AddRange(new DataGridViewColumn[] { this.coLinkedlServer, this.colLinkedDb });
            this.grdOther.Dock = DockStyle.Fill;
            this.grdOther.Location = new Point(10, 0x18);
            this.grdOther.Name = "grdOther";
            this.grdOther.Size = new Size(570, 0xc1);
            this.grdOther.TabIndex = 0;
            this.grdOther.Enter += new EventHandler(this.grdOther_Enter);
            this.coLinkedlServer.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.coLinkedlServer.HeaderText = "Server";
            this.coLinkedlServer.Name = "coLinkedlServer";
            this.colLinkedDb.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colLinkedDb.HeaderText = "Database";
            this.colLinkedDb.Name = "colLinkedDb";
            this.grpSameSever.Controls.Add(this.grdSame);
            this.grpSameSever.Controls.Add(this.panOKCancel);
            this.grpSameSever.Dock = DockStyle.Top;
            this.grpSameSever.Location = new Point(7, 7);
            this.grpSameSever.Name = "grpSameSever";
            this.grpSameSever.Padding = new Padding(10, 6, 10, 8);
            this.grpSameSever.Size = new Size(590, 0xfc);
            this.grpSameSever.TabIndex = 0;
            this.grpSameSever.TabStop = false;
            this.grpSameSever.Text = "Additional databases to include - from the same server";
            this.grpSameSever.Leave += new EventHandler(this.grpSameSever_Leave);
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 4;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnDeleteSame, 0, 0);
            this.panOKCancel.Controls.Add(this.btnPickSame, 0, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(10, 0xcd);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 4, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(570, 0x27);
            this.panOKCancel.TabIndex = 1;
            this.btnDeleteSame.Location = new Point(0x89, 7);
            this.btnDeleteSame.Name = "btnDeleteSame";
            this.btnDeleteSame.Size = new Size(0x71, 0x1d);
            this.btnDeleteSame.TabIndex = 1;
            this.btnDeleteSame.Text = "&Delete row";
            this.btnDeleteSame.UseVisualStyleBackColor = true;
            this.btnDeleteSame.Click += new EventHandler(this.btnDeleteSame_Click);
            this.btnPickSame.Location = new Point(0, 7);
            this.btnPickSame.Margin = new Padding(0, 3, 3, 3);
            this.btnPickSame.Name = "btnPickSame";
            this.btnPickSame.Size = new Size(0x83, 0x1d);
            this.btnPickSame.TabIndex = 0;
            this.btnPickSame.Text = "Pick from list...";
            this.btnPickSame.UseVisualStyleBackColor = true;
            this.btnPickSame.Click += new EventHandler(this.btnPickSame_Click);
            this.grpOtherServers.Controls.Add(this.grdOther);
            this.grpOtherServers.Controls.Add(this.tableLayoutPanel1);
            this.grpOtherServers.Dock = DockStyle.Fill;
            this.grpOtherServers.Location = new Point(7, 0x10d);
            this.grpOtherServers.Name = "grpOtherServers";
            this.grpOtherServers.Padding = new Padding(10, 6, 10, 8);
            this.grpOtherServers.Size = new Size(590, 0x108);
            this.grpOtherServers.TabIndex = 1;
            this.grpOtherServers.TabStop = false;
            this.grpOtherServers.Text = "Databases to include from other (linked) servers";
            this.grpOtherServers.Leave += new EventHandler(this.grpOtherServers_Leave);
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.btnDeleteOther, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnPickOther, 0, 0);
            this.tableLayoutPanel1.Dock = DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new Point(10, 0xd9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new Padding(0, 4, 0, 0);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.Size = new Size(570, 0x27);
            this.tableLayoutPanel1.TabIndex = 1;
            this.btnDeleteOther.Location = new Point(0x89, 7);
            this.btnDeleteOther.Name = "btnDeleteOther";
            this.btnDeleteOther.Size = new Size(0x71, 0x1d);
            this.btnDeleteOther.TabIndex = 1;
            this.btnDeleteOther.Text = "&Delete row";
            this.btnDeleteOther.UseVisualStyleBackColor = true;
            this.btnDeleteOther.Click += new EventHandler(this.btnDeleteOther_Click);
            this.btnPickOther.Location = new Point(0, 7);
            this.btnPickOther.Margin = new Padding(0, 3, 3, 3);
            this.btnPickOther.Name = "btnPickOther";
            this.btnPickOther.Size = new Size(0x83, 0x1d);
            this.btnPickOther.TabIndex = 0;
            this.btnPickOther.Text = "Pick from list...";
            this.btnPickOther.UseVisualStyleBackColor = true;
            this.btnPickOther.Click += new EventHandler(this.btnPickOther_Click);
            this.panSpacer.Dock = DockStyle.Top;
            this.panSpacer.Location = new Point(7, 0x103);
            this.panSpacer.Name = "panSpacer";
            this.panSpacer.Size = new Size(590, 10);
            this.panSpacer.TabIndex = 5;
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.btnCancel, 3, 0);
            this.tableLayoutPanel2.Dock = DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new Point(7, 0x215);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.Padding = new Padding(0, 4, 0, 0);
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel2.Size = new Size(590, 0x27);
            this.tableLayoutPanel2.TabIndex = 2;
            this.btnCancel.DialogResult = DialogResult.OK;
            this.btnCancel.Location = new Point(510, 7);
            this.btnCancel.Margin = new Padding(3, 3, 1, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 0x1d);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x25c, 0x242);
            base.Controls.Add(this.grpOtherServers);
            base.Controls.Add(this.panSpacer);
            base.Controls.Add(this.grpSameSever);
            base.Controls.Add(this.tableLayoutPanel2);
            base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "LinkedDbForm";
            base.Padding = new Padding(7, 7, 7, 6);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Additional Databases";
            ((ISupportInitialize) this.grdSame).EndInit();
            ((ISupportInitialize) this.grdOther).EndInit();
            this.grpSameSever.ResumeLayout(false);
            this.grpSameSever.PerformLayout();
            this.panOKCancel.ResumeLayout(false);
            this.grpOtherServers.ResumeLayout(false);
            this.grpOtherServers.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if ((this.panOKCancel != null) && (this.grpSameSever != null))
            {
                int num = ((((base.ClientSize.Height - this.panOKCancel.Height) - base.Padding.Top) - base.Padding.Bottom) - this.panSpacer.Height) / 2;
                if (this.grpSameSever.Height != num)
                {
                    this.grpSameSever.Height = num;
                }
            }
            base.OnLayout(levent);
        }

        public IEnumerable<LinkedDatabase> LinkedDatabases
        {
            get
            {
                IEnumerable<LinkedDatabase> first = from row in this.grdSame.Rows.Cast<DataGridViewRow>()
                    where row.Cells[0].Value != null
                    select row.Cells[0].Value.ToString().Trim() into s
                    where s.Length > 0
                    select new LinkedDatabase(s);
                IEnumerable<LinkedDatabase> second = from row in this.grdOther.Rows.Cast<DataGridViewRow>()
                    where (row.Cells[0].Value != null) && (row.Cells[1].Value != null)
                    let db = row.Cells[1].Value.ToString().Trim()
                    where db.Length > 0
                    select new LinkedDatabase(row.Cells[0].Value.ToString().Trim(), db);
                return (from d in first.Concat<LinkedDatabase>(second).Distinct<LinkedDatabase>()
                    orderby d
                    select d).ToArray<LinkedDatabase>();
            }
            set
            {
                this.grdSame.Rows.Clear();
                this.grdOther.Rows.Clear();
                foreach (LinkedDatabase database in value)
                {
                    if (string.IsNullOrEmpty(database.Server))
                    {
                        this.grdSame.Rows.Add(new object[] { database.Database });
                    }
                    else
                    {
                        this.grdOther.Rows.Add(new object[] { database.Server, database.Database });
                    }
                }
            }
        }
    }
}

