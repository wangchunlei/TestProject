namespace LINQPad.UI
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class DCDriverList : UserControl
    {
        private DCDriverInfo[] _drivers;
        private bool _initialized;
        private ColumnHeader colAuthor;
        private ColumnHeader colDriver;
        private ColumnHeader colVersion;
        private System.Windows.Forms.ListView listView;
        private LinkLabel llDelete;

        public event EventHandler DriverDeleted;

        public event EventHandler ItemActivated;

        public DCDriverList()
        {
            this.InitializeComponent();
            this._initialized = true;
            this.EnableControls();
        }

        private void AdjustColumnWidths()
        {
            int num = this.listView.ClientSize.Width - 1;
            num -= this.colAuthor.Width = (num * 0x26) / 100;
            num -= this.colVersion.Width = (num * 0x17) / 100;
            this.colDriver.Width = num;
        }

        private void EnableControls()
        {
            this.listView.Enabled = base.Enabled;
            this.llDelete.Enabled = (base.Enabled && (this.SelectedDriver != null)) && (this.SelectedDriver.Loader.InternalID == null);
        }

        public void FocusList()
        {
            if (base.Enabled && (this.listView.Items.Count != 0))
            {
                if (this.SelectedDriver == null)
                {
                    this.listView.Items[0].Selected = true;
                }
                this.listView.Focus();
            }
        }

        private void InitializeComponent()
        {
            this.listView = new System.Windows.Forms.ListView();
            this.colDriver = new ColumnHeader();
            this.colVersion = new ColumnHeader();
            this.colAuthor = new ColumnHeader();
            this.llDelete = new LinkLabel();
            base.SuspendLayout();
            this.listView.Columns.AddRange(new ColumnHeader[] { this.colDriver, this.colVersion, this.colAuthor });
            this.listView.Dock = DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.HideSelection = false;
            this.listView.Location = new Point(0, 0);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.Size = new Size(0x263, 0x10b);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = View.Details;
            this.listView.ItemActivate += new EventHandler(this.listView_ItemActivate);
            this.listView.SelectedIndexChanged += new EventHandler(this.listView_SelectedIndexChanged);
            this.colDriver.Text = "LINQPad Driver";
            this.colDriver.Width = 0x9e;
            this.colVersion.Text = "Version";
            this.colVersion.Width = 0x63;
            this.colAuthor.Text = "Author";
            this.colAuthor.Width = 0x9e;
            this.llDelete.Dock = DockStyle.Bottom;
            this.llDelete.Location = new Point(0, 0x10b);
            this.llDelete.Name = "llDelete";
            this.llDelete.Size = new Size(0x263, 20);
            this.llDelete.TabIndex = 3;
            this.llDelete.TabStop = true;
            this.llDelete.Text = "Delete Driver";
            this.llDelete.TextAlign = ContentAlignment.BottomRight;
            this.llDelete.LinkClicked += new LinkLabelLinkClickedEventHandler(this.llDelete_LinkClicked);
            base.Controls.Add(this.listView);
            base.Controls.Add(this.llDelete);
            base.Name = "DCDriverList";
            base.Size = new Size(0x263, 0x11f);
            base.ResumeLayout(false);
        }

        private void listView_ItemActivate(object sender, EventArgs e)
        {
            this.ItemActivated(this, EventArgs.Empty);
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void llDelete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if ((this.SelectedDriver != null) && (MessageBox.Show("Delete driver - are you sure?", "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
            {
                this.DriverDeleted(this, EventArgs.Empty);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (!base.Enabled)
            {
                this.listView.SelectedIndices.Clear();
            }
            base.OnEnabledChanged(e);
            this.EnableControls();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (this._initialized)
            {
                this.llDelete.Height = this.llDelete.Font.Height + 1;
            }
            base.OnLayout(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.AdjustColumnWidths();
        }

        public void SetListCapture()
        {
            Point point = this.listView.PointToClient(Control.MousePosition);
            ListViewItem itemAt = this.listView.GetItemAt(point.X, point.Y);
            if (itemAt != null)
            {
                itemAt.Selected = true;
            }
            this.listView.Focus();
            if (itemAt != null)
            {
                itemAt.Focused = true;
            }
        }

        private void UpdateUI()
        {
            this.listView.Items.Clear();
            if (this._drivers != null)
            {
                foreach (DCDriverInfo info in this._drivers)
                {
                    this.listView.Items.Add(new ListViewItem(new string[] { info.Name, (info.Loader.InternalID != null) ? "(built-in)" : info.Version.ToString(), info.Author }));
                }
            }
        }

        public DCDriverInfo[] Drivers
        {
            get
            {
                return this._drivers;
            }
            set
            {
                this._drivers = value;
                this.UpdateUI();
            }
        }

        public System.Windows.Forms.ListView ListView
        {
            get
            {
                return this.listView;
            }
        }

        public DCDriverInfo SelectedDriver
        {
            get
            {
                return (((this.listView.SelectedIndices.Count == 0) || (this.Drivers == null)) ? null : this.Drivers[this.listView.SelectedIndices[0]]);
            }
            set
            {
                if (this.Drivers != null)
                {
                    int index = Array.IndexOf<DCDriverInfo>(this.Drivers, value);
                    this.listView.SelectedIndices.Clear();
                    if (index > -1)
                    {
                        this.listView.Items[index].Selected = true;
                    }
                }
            }
        }
    }
}

