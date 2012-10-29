namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using LINQPad;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class EditManager : Component
    {
        private Form _activeForm;
        private Container components;
        private ToolStripMenuItem miCopy;
        private ToolStripMenuItem miCopyMarkdown;
        private ToolStripMenuItem miCopyPlain;
        private ToolStripMenuItem miCut;
        private ToolStripMenuItem miEdit;
        private ToolStripMenuItem miPaste;
        private ToolStripMenuItem miRedo;
        private ToolStripMenuItem miSelectAll;
        private ToolStripMenuItem miUndo;
        public const int WM_COPY = 0x301;
        public const int WM_CUT = 0x300;
        public const int WM_PASTE = 770;
        public const int WM_UNDO = 0x304;

        public EditManager()
        {
            this.components = null;
            this.InitializeComponent();
        }

        public EditManager(IContainer container) : this()
        {
            container.Add(this);
        }

        public EditManager(Form activeForm) : this()
        {
            this._activeForm = activeForm;
        }

        public void Copy()
        {
            if (this.ForwardToPlugin)
            {
                EditManager pluginEditManager = MainForm.Instance.CurrentQueryControl.GetPluginEditManager();
                if (pluginEditManager != null)
                {
                    pluginEditManager.RunOnUIThread(new Action(pluginEditManager.Copy));
                }
            }
            else
            {
                Control activeControl = this.GetActiveControl();
                if (activeControl is TextBoxBase)
                {
                    ((TextBoxBase) activeControl).Copy();
                }
                else if (activeControl is ActiproSoftware.SyntaxEditor.SyntaxEditor)
                {
                    ((ActiproSoftware.SyntaxEditor.SyntaxEditor) activeControl).get_SelectedView().CopyToClipboard();
                }
                else if (activeControl is DataGridView)
                {
                    DataObject clipboardContent = ((DataGridView) activeControl).GetClipboardContent();
                    if (clipboardContent != null)
                    {
                        Clipboard.SetDataObject(clipboardContent);
                    }
                }
                else
                {
                    try
                    {
                        Native.SendMessage(activeControl.Handle, 0x301);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void CopyMarkdown()
        {
            int spacesToStrip;
            string selectedText = this.GetSelectedText();
            if (!string.IsNullOrEmpty(selectedText))
            {
                string[] strArray2 = (from l in StringUtil.ConvertTabsToSpaces(selectedText).TrimEnd(new char[0]).Split(new string[] { "\r\n" }, StringSplitOptions.None).SkipWhile<string>((Func<string, bool>) (l => (l.Trim().Length == 0))) select l.TrimEnd(new char[0])).ToArray<string>();
                spacesToStrip = (from l in strArray2
                    where l.Trim().Length > 0
                    select l).Min<string>((Func<string, int>) (l => (l.Length - l.TrimStart(new char[0]).Length)));
                Clipboard.SetText(string.Join("\r\n", (from l in strArray2 select "    " + ((l.Trim().Length == 0) ? "" : l.Substring(spacesToStrip))).ToArray<string>()));
            }
        }

        public void CopyPlain()
        {
            string selectedText = this.GetSelectedText();
            if (!string.IsNullOrEmpty(selectedText))
            {
                Clipboard.SetText(StringUtil.ConvertTabsToSpaces(selectedText));
            }
        }

        public void Cut()
        {
            if (this.ForwardToPlugin)
            {
                EditManager pluginEditManager = MainForm.Instance.CurrentQueryControl.GetPluginEditManager();
                if (pluginEditManager != null)
                {
                    pluginEditManager.RunOnUIThread(new Action(pluginEditManager.Cut));
                }
            }
            else
            {
                Control activeControl = this.GetActiveControl();
                if (activeControl is TextBoxBase)
                {
                    ((TextBoxBase) activeControl).Cut();
                }
                else if (activeControl is ActiproSoftware.SyntaxEditor.SyntaxEditor)
                {
                    ((ActiproSoftware.SyntaxEditor.SyntaxEditor) activeControl).get_SelectedView().CutToClipboard();
                }
                else
                {
                    try
                    {
                        Native.SendMessage(activeControl.Handle, 0x300);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private Control GetActiveControl()
        {
            Form activeMdiChild = (this._activeForm == null) ? Form.ActiveForm : this._activeForm;
            if (activeMdiChild != null)
            {
                if (activeMdiChild.IsMdiContainer)
                {
                    activeMdiChild = activeMdiChild.ActiveMdiChild;
                }
                if (activeMdiChild == null)
                {
                    return null;
                }
                Control activeControl = activeMdiChild.ActiveControl;
                if (activeControl != null)
                {
                    while (activeControl is ContainerControl)
                    {
                        Control control3 = ((ContainerControl) activeControl).ActiveControl;
                        if (control3 == null)
                        {
                            return activeControl;
                        }
                        activeControl = control3;
                    }
                    return activeControl;
                }
            }
            return null;
        }

        private string GetSelectedText()
        {
            Control activeControl = this.GetActiveControl();
            if (activeControl is ActiproSoftware.SyntaxEditor.SyntaxEditor)
            {
                return ((ActiproSoftware.SyntaxEditor.SyntaxEditor) activeControl).get_SelectedView().get_SelectedText();
            }
            if (activeControl is TextBoxBase)
            {
                return ((TextBoxBase) activeControl).SelectedText;
            }
            return null;
        }

        private void InitializeComponent()
        {
            this.components = new Container();
        }

        protected void miCopy_Click(object sender, EventArgs e)
        {
            this.Copy();
        }

        protected void miCopyMarkdown_Click(object sender, EventArgs e)
        {
            this.CopyMarkdown();
        }

        protected void miCopyPlain_Click(object sender, EventArgs e)
        {
            this.CopyPlain();
        }

        protected void miCut_Click(object sender, EventArgs e)
        {
            this.Cut();
        }

        protected void miEdit_Closed(object sender, EventArgs e)
        {
            if (this.Site == null)
            {
                if (this.miUndo != null)
                {
                    this.miUndo.Enabled = true;
                }
                if (this.miRedo != null)
                {
                    this.miRedo.Enabled = true;
                }
                if (this.miCopy != null)
                {
                    this.miCopy.Enabled = true;
                }
                if (this.miCopyPlain != null)
                {
                    this.miCopyPlain.Enabled = true;
                }
                if (this.miCopyMarkdown != null)
                {
                    this.miCopyMarkdown.Enabled = true;
                }
                if (this.miCut != null)
                {
                    this.miCut.Enabled = true;
                }
                if (this.miPaste != null)
                {
                    this.miPaste.Enabled = true;
                }
            }
        }

        protected void miEdit_Popup(object sender, EventArgs e)
        {
            if ((this.Site == null) || !this.Site.DesignMode)
            {
                bool flag;
                bool flag2;
                bool flag3;
                bool flag4;
                bool flag5;
                bool flag6;
                Control c = null;
                if (flag = (this.AllowForwarding && (MainForm.Instance != null)) && MainForm.Instance.ApplyEditToPlugin)
                {
                    flag6 = flag5 = flag4 = flag3 = flag2 = true;
                }
                else
                {
                    c = this.GetActiveControl();
                    this.SetCanEditFlags(c, out flag6, out flag5, out flag4, out flag3, out flag2);
                }
                if (this.miUndo != null)
                {
                    this.miUndo.Enabled = flag6;
                }
                if (this.miRedo != null)
                {
                    this.miRedo.Enabled = flag5;
                }
                if (this.miCopy != null)
                {
                    this.miCopy.Enabled = flag4;
                }
                if (this.miCopyPlain != null)
                {
                    this.miCopyPlain.Enabled = flag4 && !flag;
                }
                if (this.miCopyMarkdown != null)
                {
                    this.miCopyMarkdown.Enabled = flag4 && !flag;
                }
                if (this.miCut != null)
                {
                    this.miCut.Enabled = flag3;
                }
                if (this.miPaste != null)
                {
                    this.miPaste.Enabled = flag2;
                }
                this.miUndo.Text = "&Undo";
                this.miRedo.Text = "&Redo";
                if (!flag)
                {
                    RichTextBox box = c as RichTextBox;
                    if (box != null)
                    {
                        if (this.miUndo != null)
                        {
                            this.miUndo.Text = this.miUndo.Text + " " + ((box.UndoActionName != "Unknown") ? box.UndoActionName : "");
                        }
                        if (this.miRedo != null)
                        {
                            this.miRedo.Text = this.miRedo.Text + " " + ((box.RedoActionName != "Unknown") ? box.RedoActionName : "");
                        }
                    }
                    ActiproSoftware.SyntaxEditor.SyntaxEditor editor = c as ActiproSoftware.SyntaxEditor.SyntaxEditor;
                    if (editor != null)
                    {
                        try
                        {
                            if ((this.miUndo != null) && this.miUndo.Enabled)
                            {
                                this.miUndo.Text = this.miUndo.Text + " " + editor.get_Document().get_UndoRedo().get_UndoStack().GetName(editor.get_Document().get_UndoRedo().get_UndoStack().get_Count() - 1);
                            }
                            if ((this.miRedo != null) && this.miRedo.Enabled)
                            {
                                this.miRedo.Text = this.miRedo.Text + " " + editor.get_Document().get_UndoRedo().get_RedoStack().GetName(editor.get_Document().get_UndoRedo().get_RedoStack().get_Count() - 1);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        protected void miPaste_Click(object sender, EventArgs e)
        {
            this.Paste();
        }

        protected void miRedo_Click(object sender, EventArgs e)
        {
            this.Redo();
        }

        protected void miSelectAll_Click(object sender, EventArgs e)
        {
            this.SelectAll();
        }

        protected void miUndo_Click(object sender, EventArgs e)
        {
            this.Undo();
        }

        public void Paste()
        {
            if (this.ForwardToPlugin)
            {
                EditManager pluginEditManager = MainForm.Instance.CurrentQueryControl.GetPluginEditManager();
                if (pluginEditManager != null)
                {
                    pluginEditManager.RunOnUIThread(new Action(pluginEditManager.Paste));
                }
            }
            else
            {
                Control activeControl = this.GetActiveControl();
                if (activeControl is TextBoxBase)
                {
                    ((TextBoxBase) activeControl).Paste();
                }
                else if (activeControl is ActiproSoftware.SyntaxEditor.SyntaxEditor)
                {
                    ((ActiproSoftware.SyntaxEditor.SyntaxEditor) activeControl).get_SelectedView().PasteFromClipboard();
                }
                else
                {
                    try
                    {
                        Native.SendMessage(activeControl.Handle, 770);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void Redo()
        {
            if (this.ForwardToPlugin)
            {
                EditManager pluginEditManager = MainForm.Instance.CurrentQueryControl.GetPluginEditManager();
                if (pluginEditManager != null)
                {
                    pluginEditManager.RunOnUIThread(new Action(pluginEditManager.Redo));
                }
            }
            else
            {
                Control activeControl = this.GetActiveControl();
                if (activeControl is RichTextBox)
                {
                    ((RichTextBox) activeControl).Redo();
                }
                else if (activeControl is ActiproSoftware.SyntaxEditor.SyntaxEditor)
                {
                    ((ActiproSoftware.SyntaxEditor.SyntaxEditor) activeControl).get_Document().get_UndoRedo().Redo();
                }
            }
        }

        private void RunOnUIThread(Action a)
        {
            if (this._activeForm == null)
            {
                a();
            }
            else
            {
                this._activeForm.BeginInvoke(a);
            }
        }

        public void SelectAll()
        {
            if (this.ForwardToPlugin)
            {
                EditManager pluginEditManager = MainForm.Instance.CurrentQueryControl.GetPluginEditManager();
                if (pluginEditManager != null)
                {
                    pluginEditManager.RunOnUIThread(new Action(pluginEditManager.SelectAll));
                }
            }
            else
            {
                Control activeControl = this.GetActiveControl();
                if (activeControl is TextBoxBase)
                {
                    ((TextBoxBase) activeControl).SelectAll();
                }
                else if (activeControl is DataGridView)
                {
                    ((DataGridView) activeControl).SelectAll();
                }
                else if (activeControl is ActiproSoftware.SyntaxEditor.SyntaxEditor)
                {
                    ((ActiproSoftware.SyntaxEditor.SyntaxEditor) activeControl).get_SelectedView().get_Selection().SelectAll();
                }
                else if (activeControl is DataGridView)
                {
                    ((DataGridView) activeControl).SelectAll();
                }
            }
        }

        protected void SetCanEditFlags(Control c, out bool canUndo, out bool canRedo, out bool canCopy, out bool canCut, out bool canPaste)
        {
            canPaste = false;
            canCut = false;
            canCopy = false;
            canRedo = false;
            canUndo = false;
            if (c is TextBoxBase)
            {
                TextBoxBase base2 = (TextBoxBase) c;
                canUndo = base2.CanUndo;
                canCopy = base2.SelectionLength > 0;
                canCut = (base2.SelectionLength > 0) && !base2.ReadOnly;
                bool flag = false;
                try
                {
                    flag = Clipboard.ContainsData(DataFormats.Text);
                }
                catch
                {
                }
                canPaste = !base2.ReadOnly && flag;
            }
            if (c is RichTextBox)
            {
                canRedo = ((RichTextBox) c).CanRedo;
            }
            if (c is DataGridView)
            {
                canCopy = ((DataGridView) c).SelectedCells.Count > 0;
            }
            if (c is ActiproSoftware.SyntaxEditor.SyntaxEditor)
            {
                ActiproSoftware.SyntaxEditor.SyntaxEditor editor = (ActiproSoftware.SyntaxEditor.SyntaxEditor) c;
                canUndo = editor.get_Document().get_UndoRedo().get_CanUndo();
                canRedo = editor.get_Document().get_UndoRedo().get_CanRedo();
                canCopy = editor.get_SelectedView().get_SelectedText().Length > 0;
                canCut = editor.get_SelectedView().get_CanDelete();
                canPaste = editor.get_SelectedView().get_CanPaste();
            }
        }

        public void Undo()
        {
            if (this.ForwardToPlugin)
            {
                EditManager pluginEditManager = MainForm.Instance.CurrentQueryControl.GetPluginEditManager();
                if (pluginEditManager != null)
                {
                    pluginEditManager.RunOnUIThread(new Action(pluginEditManager.Undo));
                }
            }
            else
            {
                Control activeControl = this.GetActiveControl();
                if (activeControl is TextBoxBase)
                {
                    ((TextBoxBase) activeControl).Undo();
                }
                else if (activeControl is ActiproSoftware.SyntaxEditor.SyntaxEditor)
                {
                    ((ActiproSoftware.SyntaxEditor.SyntaxEditor) activeControl).get_Document().get_UndoRedo().Undo();
                }
                else
                {
                    try
                    {
                        Native.SendMessage(activeControl.Handle, 0x304);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public bool AllowForwarding { get; set; }

        private bool ForwardToPlugin
        {
            get
            {
                return ((this.AllowForwarding && (MainForm.Instance != null)) && MainForm.Instance.ApplyEditToPlugin);
            }
        }

        public ToolStripMenuItem MenuItemCopy
        {
            get
            {
                return this.miCopy;
            }
            set
            {
                if (this.miCopy != null)
                {
                    this.miCopy.Click -= new EventHandler(this.miCopy_Click);
                }
                this.miCopy = value;
                if (this.miCopy != null)
                {
                    this.miCopy.Click += new EventHandler(this.miCopy_Click);
                }
            }
        }

        public ToolStripMenuItem MenuItemCopyMarkdown
        {
            get
            {
                return this.miCopyMarkdown;
            }
            set
            {
                if (this.miCopyMarkdown != null)
                {
                    this.miCopyMarkdown.Click -= new EventHandler(this.miCopyMarkdown_Click);
                }
                this.miCopyMarkdown = value;
                if (this.miCopyMarkdown != null)
                {
                    this.miCopyMarkdown.Click += new EventHandler(this.miCopyMarkdown_Click);
                }
            }
        }

        public ToolStripMenuItem MenuItemCopyPlain
        {
            get
            {
                return this.miCopyPlain;
            }
            set
            {
                if (this.miCopyPlain != null)
                {
                    this.miCopyPlain.Click -= new EventHandler(this.miCopyPlain_Click);
                }
                this.miCopyPlain = value;
                if (this.miCopyPlain != null)
                {
                    this.miCopyPlain.Click += new EventHandler(this.miCopyPlain_Click);
                }
            }
        }

        public ToolStripMenuItem MenuItemCut
        {
            get
            {
                return this.miCut;
            }
            set
            {
                if (this.miCut != null)
                {
                    this.miCut.Click -= new EventHandler(this.miCut_Click);
                }
                this.miCut = value;
                if (this.miCut != null)
                {
                    this.miCut.Click += new EventHandler(this.miCut_Click);
                }
            }
        }

        public ToolStripMenuItem MenuItemEdit
        {
            get
            {
                return this.miEdit;
            }
            set
            {
                if (this.miEdit != null)
                {
                    this.miEdit.DropDownOpened -= new EventHandler(this.miEdit_Popup);
                }
                if (this.miEdit != null)
                {
                    this.miEdit.DropDownClosed -= new EventHandler(this.miEdit_Closed);
                }
                this.miEdit = value;
                if (this.miEdit != null)
                {
                    this.miEdit.DropDownOpened += new EventHandler(this.miEdit_Popup);
                }
                if (this.miEdit != null)
                {
                    this.miEdit.DropDownClosed += new EventHandler(this.miEdit_Closed);
                }
            }
        }

        public ToolStripMenuItem MenuItemPaste
        {
            get
            {
                return this.miPaste;
            }
            set
            {
                if (this.miPaste != null)
                {
                    this.miPaste.Click -= new EventHandler(this.miPaste_Click);
                }
                this.miPaste = value;
                if (this.miPaste != null)
                {
                    this.miPaste.Click += new EventHandler(this.miPaste_Click);
                }
            }
        }

        public ToolStripMenuItem MenuItemRedo
        {
            get
            {
                return this.miRedo;
            }
            set
            {
                if (this.miRedo != null)
                {
                    this.miRedo.Click -= new EventHandler(this.miRedo_Click);
                }
                this.miRedo = value;
                if (this.miRedo != null)
                {
                    this.miRedo.Click += new EventHandler(this.miRedo_Click);
                }
            }
        }

        public ToolStripMenuItem MenuItemSelectAll
        {
            get
            {
                return this.miSelectAll;
            }
            set
            {
                if (this.miSelectAll != null)
                {
                    this.miSelectAll.Click -= new EventHandler(this.miSelectAll_Click);
                }
                this.miSelectAll = value;
                if (this.miSelectAll != null)
                {
                    this.miSelectAll.Click += new EventHandler(this.miSelectAll_Click);
                }
            }
        }

        public ToolStripMenuItem MenuItemUndo
        {
            get
            {
                return this.miUndo;
            }
            set
            {
                if (this.miUndo != null)
                {
                    this.miUndo.Click -= new EventHandler(this.miUndo_Click);
                }
                this.miUndo = value;
                if (this.miUndo != null)
                {
                    this.miUndo.Click += new EventHandler(this.miUndo_Click);
                }
            }
        }
    }
}

