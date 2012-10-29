namespace LINQPad.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    internal class FileRetryForm : BaseForm
    {
        private FileActionInfo[] _actions;
        private Button btnCancel;
        private Button btnRetry;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private IContainer components = null;
        private Label label1;
        private ListView lvFiles;
        private TableLayoutPanel panOKCancel;

        private FileRetryForm(IEnumerable<FileActionInfo> actions)
        {
            this._actions = actions.ToArray<FileActionInfo>();
            this.InitializeComponent();
            this.PopulateList();
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            if (Try(from a in this._actions
                where a.Error != null
                select a))
            {
                base.DialogResult = DialogResult.OK;
            }
            this.PopulateList();
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
            this.btnRetry = new Button();
            this.btnCancel = new Button();
            this.label1 = new Label();
            this.lvFiles = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.columnHeader2 = new ColumnHeader();
            this.panOKCancel.SuspendLayout();
            base.SuspendLayout();
            this.panOKCancel.AutoSize = true;
            this.panOKCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.panOKCancel.ColumnCount = 4;
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.ColumnStyles.Add(new ColumnStyle());
            this.panOKCancel.Controls.Add(this.btnRetry, 2, 0);
            this.panOKCancel.Controls.Add(this.btnCancel, 3, 0);
            this.panOKCancel.Dock = DockStyle.Bottom;
            this.panOKCancel.Location = new Point(7, 0x163);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 5, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x304, 0x25);
            this.panOKCancel.TabIndex = 7;
            this.btnRetry.Location = new Point(0x254, 8);
            this.btnRetry.Margin = new Padding(3, 3, 3, 0);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Size = new Size(0x55, 0x1d);
            this.btnRetry.TabIndex = 1;
            this.btnRetry.Text = "&Retry";
            this.btnRetry.UseVisualStyleBackColor = true;
            this.btnRetry.Click += new EventHandler(this.btnRetry_Click);
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x2af, 8);
            this.btnCancel.Margin = new Padding(3, 3, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x55, 0x1d);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Ignore";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(7, 7);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 3);
            this.label1.Size = new Size(0xf8, 0x16);
            this.label1.TabIndex = 8;
            this.label1.Text = "The following files could not be written:";
            this.lvFiles.Columns.AddRange(new ColumnHeader[] { this.columnHeader1, this.columnHeader2 });
            this.lvFiles.Dock = DockStyle.Fill;
            this.lvFiles.Location = new Point(7, 0x1d);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new Size(0x304, 0x146);
            this.lvFiles.TabIndex = 9;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = View.Details;
            this.columnHeader1.Text = "File";
            this.columnHeader1.Width = 170;
            this.columnHeader2.Text = "Error";
            this.columnHeader2.Width = 0x291;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x312, 0x18f);
            base.ControlBox = false;
            base.Controls.Add(this.lvFiles);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.panOKCancel);
            base.Margin = new Padding(3, 4, 3, 4);
            base.Name = "FileRetryForm";
            base.Padding = new Padding(7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Error Writing Files";
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void PopulateList()
        {
            this.lvFiles.Items.Clear();
            foreach (FileActionInfo info in this._actions)
            {
                if (info.Error != null)
                {
                    this.lvFiles.Items.Add(new ListViewItem(new string[] { info.FileName, info.Error.GetType().Name + " - " + info.Error.Message }));
                }
            }
        }

        private static bool Try(IEnumerable<FileActionInfo> actions)
        {
            bool flag = false;
            foreach (FileActionInfo info in actions)
            {
                try
                {
                    info.Action();
                    info.Error = null;
                }
                catch (Exception exception)
                {
                    flag = true;
                    info.Error = exception;
                }
            }
            return !flag;
        }

        public static bool TryProcessFiles(IEnumerable<FileActionInfo> actions, Action beforeShowForm)
        {
            if (Try(actions))
            {
                return true;
            }
            beforeShowForm();
            using (FileRetryForm form = new FileRetryForm(actions))
            {
                return (form.ShowDialog() == DialogResult.OK);
            }
        }

        internal class FileActionInfo
        {
            public readonly System.Action Action;
            public Exception Error;
            public readonly string FileName;

            public FileActionInfo(string fileName, System.Action action)
            {
                this.FileName = fileName;
                this.Action = action;
            }
        }
    }
}

