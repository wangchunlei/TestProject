namespace LINQPad.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class ListPicker : BaseForm
    {
        private object[] _items;
        private Button btnCancel;
        private Button btnOK;
        private IContainer components;
        private ListBox listBox;
        private TableLayoutPanel panOKCancel;

        public ListPicker(object[] items) : this(items, false)
        {
        }

        public ListPicker(object[] items, bool allowMulti)
        {
            this.components = null;
            this.InitializeComponent();
            this.AllItems = items;
            this.listBox.SelectionMode = allowMulti ? SelectionMode.MultiExtended : SelectionMode.One;
            if (allowMulti)
            {
                this.Text = this.Text + "(s)";
            }
            this.AutoSelectFirstItem = true;
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
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.listBox = new ListBox();
            this.panOKCancel.SuspendLayout();
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
            this.panOKCancel.Location = new Point(7, 0x180);
            this.panOKCancel.Name = "panOKCancel";
            this.panOKCancel.Padding = new Padding(0, 5, 0, 0);
            this.panOKCancel.RowCount = 1;
            this.panOKCancel.RowStyles.Add(new RowStyle());
            this.panOKCancel.Size = new Size(0x1e3, 0x26);
            this.panOKCancel.TabIndex = 1;
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(0x13f, 8);
            this.btnOK.Margin = new Padding(3, 3, 3, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4f, 30);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(0x194, 8);
            this.btnCancel.Margin = new Padding(3, 3, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4f, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.listBox.Dock = DockStyle.Fill;
            this.listBox.FormattingEnabled = true;
            this.listBox.IntegralHeight = false;
            this.listBox.ItemHeight = 0x11;
            this.listBox.Location = new Point(7, 7);
            this.listBox.Name = "listBox";
            this.listBox.Size = new Size(0x1e3, 0x179);
            this.listBox.TabIndex = 0;
            this.listBox.DoubleClick += new EventHandler(this.listBox_DoubleClick);
            base.AcceptButton = this.btnOK;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x1f1, 0x1ad);
            base.ControlBox = false;
            base.Controls.Add(this.listBox);
            base.Controls.Add(this.panOKCancel);
            base.Margin = new Padding(3, 4, 3, 4);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ListPicker";
            base.Padding = new Padding(7);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Choose Item";
            this.panOKCancel.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if ((this.listBox.Items.Count > 0) && this.AutoSelectFirstItem)
            {
                this.listBox.SelectedIndex = 0;
            }
            this.listBox.Focus();
        }

        public object[] AllItems
        {
            get
            {
                return this._items;
            }
            set
            {
                this._items = value;
                this.listBox.Items.Clear();
                if (value != null)
                {
                    this.listBox.Items.AddRange(value);
                }
            }
        }

        public bool AutoSelectFirstItem { get; set; }

        public object SelectedItem
        {
            get
            {
                return this.listBox.SelectedItem;
            }
            set
            {
                this.listBox.SelectedItem = value;
            }
        }

        public object[] SelectedItems
        {
            get
            {
                return this.listBox.SelectedItems.Cast<object>().ToArray<object>();
            }
        }
    }
}

