namespace LINQPad.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class QueryTabControl : TabControl
    {
        private static ContextMenuStrip _cm = new ContextMenuStrip();
        private static QueryTabControl _contextItem;
        private TabPage _dragPage;
        private Rectangle _dragStartZone;
        private TabPage _dropPage;
        private bool _isDragging;
        private static ToolStripMenuItem _miAdvancedProps = new ToolStripMenuItem("Query Properties...", Resources.AdvancedProperties, new EventHandler(QueryTabControl.AdvancedProps));
        private static ToolStripMenuItem _miCloneQuery = new ToolStripMenuItem("Clone Query", Resources.CloneQuery, new EventHandler(QueryTabControl.CloneQuery));
        private static ToolStripMenuItem _miClose = new ToolStripMenuItem("Close Query", Resources.CloseQuery, new EventHandler(QueryTabControl.Close));
        private static ToolStripMenuItem _miCloseAll = new ToolStripMenuItem("Close All", Resources.CloseQuery, new EventHandler(QueryTabControl.CloseAll));
        private static ToolStripMenuItem _miCloseAllButThis = new ToolStripMenuItem("Close All But This", Resources.CloseAllButThis, new EventHandler(QueryTabControl.CloseAllButThis));
        private static ToolStripMenuItem _miNewQuerySameProps = new ToolStripMenuItem("New Query, Same Properties", Resources.NewQuerySameProps, new EventHandler(QueryTabControl.NewQuerySameProps));
        private static ToolStripMenuItem _miSave = new ToolStripMenuItem("Save", Resources.Save, new EventHandler(QueryTabControl.Save));
        private static ToolStripMenuItem _miSaveAs = new ToolStripMenuItem("Save As...", null, new EventHandler(QueryTabControl.SaveAs));

        static QueryTabControl()
        {
            _miSave.ShortcutKeyDisplayString = "Ctrl+S";
            _miClose.ShortcutKeyDisplayString = "Ctrl+F4";
            _miCloseAll.ShortcutKeyDisplayString = "Ctrl+Shift+F4";
            _miNewQuerySameProps.ShortcutKeyDisplayString = "Ctrl+Shift+N";
            _miCloneQuery.ShortcutKeyDisplayString = "Ctrl+Shift+C";
            _cm.Items.Add(_miSave);
            _cm.Items.Add(_miSaveAs);
            _cm.Items.Add("-");
            _cm.Items.Add(_miClose);
            _cm.Items.Add(_miCloseAll);
            _cm.Items.Add(_miCloseAllButThis);
            _cm.Items.Add("-");
            _cm.Items.Add(_miNewQuerySameProps);
            _cm.Items.Add(_miCloneQuery);
            _cm.Items.Add("-");
            _cm.Items.Add(_miAdvancedProps);
        }

        private static void AdvancedProps(object sender, EventArgs e)
        {
            _contextItem.MainForm.AdvancedQueryProps();
        }

        private static void CloneQuery(object sender, EventArgs e)
        {
            _contextItem.MainForm.CloneQuery();
        }

        private static void Close(object sender, EventArgs e)
        {
            _contextItem.QueryControl.TryClose();
        }

        private static void CloseAll(object sender, EventArgs e)
        {
            LINQPad.UI.MainForm.Instance.CloseAll(null, false);
        }

        private static void CloseAllButThis(object sender, EventArgs e)
        {
            LINQPad.UI.MainForm.Instance.CloseAll(_contextItem.QueryControl, false);
        }

        private TabPage GetPageAtPoint(Point p)
        {
            for (int i = 0; i < base.TabPages.Count; i++)
            {
                if (base.GetTabRect(i).Contains(p))
                {
                    return base.TabPages[i];
                }
            }
            return null;
        }

        private static void NewQuerySameProps(object sender, EventArgs e)
        {
            _contextItem.MainForm.NewQuerySameProps("", false);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this._isDragging = false;
            this._dragStartZone = Rectangle.Empty;
            if (e.Button == MouseButtons.Left)
            {
                this._dragStartZone = new Rectangle(e.Location, Size.Empty);
                this._dragStartZone.Inflate(SystemInformation.DragSize);
                this._dragPage = this.GetPageAtPoint(e.Location);
            }
            else
            {
                TabPage pageAtPoint;
                if (e.Button == MouseButtons.Right)
                {
                    pageAtPoint = this.GetPageAtPoint(e.Location);
                    if (pageAtPoint != null)
                    {
                        base.SelectedTab = pageAtPoint;
                        _contextItem = this;
                        _cm.Show(this, e.Location);
                    }
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    pageAtPoint = this.GetPageAtPoint(e.Location);
                    if (pageAtPoint != null)
                    {
                        LINQPad.UI.QueryControl queryControl = this.MainForm.GetQueryControl(pageAtPoint);
                        if (queryControl.Query.IsModified)
                        {
                            base.SelectedTab = pageAtPoint;
                        }
                        queryControl.TryClose();
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                if (!(this._isDragging || this._dragStartZone.Contains(e.Location)))
                {
                    this._dragPage = this.GetPageAtPoint(e.Location);
                    if (this._dragPage != null)
                    {
                        this._isDragging = true;
                        this._dropPage = null;
                    }
                }
                if (this._isDragging && (this._dragPage != null))
                {
                    TabPage pageAtPoint = this.GetPageAtPoint(e.Location);
                    if (pageAtPoint != this._dropPage)
                    {
                        this._dropPage = null;
                    }
                    if (((pageAtPoint != null) && (pageAtPoint != this._dragPage)) && (pageAtPoint != this._dropPage))
                    {
                        base.Parent.SuspendLayout();
                        base.Hide();
                        int index = base.TabPages.IndexOf(pageAtPoint);
                        int num2 = base.TabPages.IndexOf(this._dragPage);
                        base.TabPages.Remove(this._dragPage);
                        if (index < num2)
                        {
                            base.TabPages.Insert(index, this._dragPage);
                        }
                        else if (index == base.TabPages.Count)
                        {
                            base.TabPages.Add(this._dragPage);
                        }
                        else
                        {
                            base.TabPages.Insert(index, this._dragPage);
                        }
                        base.SelectedTab = this._dragPage;
                        base.Show();
                        base.Parent.ResumeLayout();
                        this._dropPage = this.GetPageAtPoint(e.Location);
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this._isDragging = false;
            base.OnMouseUp(e);
        }

        private static void Save(object sender, EventArgs e)
        {
            _contextItem.MainForm.Save();
        }

        private static void SaveAs(object sender, EventArgs e)
        {
            _contextItem.MainForm.SaveAs();
        }

        private LINQPad.UI.MainForm MainForm
        {
            get
            {
                return ControlUtil.FindAncestorOfType<LINQPad.UI.MainForm>(this);
            }
        }

        private LINQPad.UI.QueryControl QueryControl
        {
            get
            {
                return this.MainForm.CurrentQueryControl;
            }
        }
    }
}

