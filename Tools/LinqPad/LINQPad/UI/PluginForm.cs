namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.ExecutionModel;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;
    using System.Threading;
    using System.Windows.Forms;

    internal class PluginForm : BaseForm
    {
        private bool _clickTransparencySupported;
        private PluginControlCollection _controls = new PluginControlCollection();
        private IntPtr _lastParentHandle;
        private PluginWindowManager _manager;
        private bool _shouldBeVisible;
        private bool _shown;
        private bool _shutdownMode;
        private volatile bool _threadSafeVisible;
        private ToolTip _toolTip;
        public readonly ManualResetEvent Ready;
        public const int WS_EX_TOOLWINDOW = 0x80;

        internal PluginForm(PluginWindowManager manager)
        {
            ToolTip tip = new ToolTip {
                ShowAlways = true
            };
            this._toolTip = tip;
            this.Ready = new ManualResetEvent(false);
            this._manager = manager;
            base.ControlBox = false;
            base.FormBorderStyle = FormBorderStyle.None;
            base.ShowInTaskbar = false;
            base.KeyPreview = true;
            base.StartPosition = FormStartPosition.Manual;
            this.DoubleBuffered = true;
            this.EditManager = new LINQPad.UI.EditManager(this);
        }

        public PluginControl AddControl(Control c, string heading)
        {
            PluginControl item = new PluginControl(c, heading);
            this._controls.Add(item);
            c.Dock = DockStyle.Fill;
            c.Visible = false;
            if (base.InvokeRequired)
            {
                this.BeginInvoke(delegate {
                    this.Controls.Add(c);
                });
                return item;
            }
            base.Controls.Add(c);
            return item;
        }

        private void BeginInvoke(Action a)
        {
            if (base.IsHandleCreated)
            {
                base.BeginInvoke(a);
            }
        }

        internal void CloseWithShutdown()
        {
            this._shouldBeVisible = false;
            if (Server.IsWpfLoaded())
            {
                WpfBridge.ShutdownCurrentDispatcher();
            }
            base.Close();
        }

        private void DisconnectControl(Form.ControlCollection cc)
        {
            foreach (Control control in cc)
            {
                try
                {
                    RemotingServices.Disconnect(control);
                }
                catch
                {
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            this._controls.Clear();
            this._toolTip.Dispose();
            if (!this._shutdownMode)
            {
                this._manager.OnPluginFormClosed(this);
            }
            base.Dispose(disposing);
        }

        public PluginControl[] GetControls()
        {
            lock (this._controls)
            {
                return this._controls.ToArray<PluginControl>();
            }
        }

        private IntPtr GetTargetToPutBehind(IntPtr parentHandle, IntPtr mainFormHandle)
        {
            uint num;
            IntPtr ptr3;
            Native.GetWindowThreadProcessId(base.Handle, out num);
            IntPtr hWnd = parentHandle;
        Label_004D:
            ptr3 = Native.GetWindow(hWnd, 2);
            if (!(ptr3 == IntPtr.Zero))
            {
                uint num2;
                if (ptr3 == base.Handle)
                {
                    return IntPtr.Zero;
                }
                if ((ptr3 == mainFormHandle) && (mainFormHandle != parentHandle))
                {
                    return hWnd;
                }
                Native.GetWindowThreadProcessId(ptr3, out num2);
                if (num2 != num)
                {
                    return hWnd;
                }
                hWnd = ptr3;
                goto Label_004D;
            }
            return IntPtr.Zero;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal void InvokeCloseWithShutdown()
        {
            this._shutdownMode = true;
            this.BeginInvoke(new Action(this.CloseWithShutdown));
        }

        internal void InvokeDisposeControl(PluginControl c)
        {
            this.BeginInvoke(() => c.Control.Dispose());
        }

        internal void InvokeHide()
        {
            this.BeginInvoke(delegate {
                this._shouldBeVisible = false;
                IntPtr activeWindow = Native.GetForegroundWindow();
                bool visible = base.Visible;
                base.Hide();
                if (visible)
                {
                    this._manager.CheckMainForm(activeWindow);
                }
            });
        }

        internal void InvokeHideIfVisible()
        {
            this.BeginInvoke(delegate {
                this._shouldBeVisible = false;
                if (base.Visible)
                {
                    base.Hide();
                }
            });
        }

        internal void InvokeRelocate(Rectangle rectangle)
        {
            Action a = delegate {
                if (this.Bounds != rectangle)
                {
                    this.Bounds = rectangle;
                }
            };
            if (this._shown)
            {
                this.BeginInvoke(a);
            }
            else
            {
                Server.CurrentServer.RunOnMessageLoopThread(a);
            }
        }

        internal void InvokeScroll(VerticalScrollAmount amount, bool down)
        {
            this.BeginInvoke(() => this.Scroll(amount, down));
        }

        internal void InvokeSetZOrder(IntPtr parentHandle, IntPtr mainFormHandle)
        {
            this._lastParentHandle = parentHandle;
            this.BeginInvoke(() => this.SetZOrder(parentHandle, mainFormHandle));
        }

        internal void InvokeShow(PluginControl plugin, IntPtr parentHandle, IntPtr mainFormHandle, bool focus, bool clickTransparencySupported)
        {
            this._clickTransparencySupported = clickTransparencySupported && !LocalUserOptions.Instance.PluginsOnTop;
            Action a = delegate {
                if (!this.Visible)
                {
                    this.ShowNoActivate();
                }
                this._shouldBeVisible = true;
                this.SetZOrder(parentHandle, mainFormHandle);
                Control[] controlArray = (from c in this.Controls.Cast<Control>()
                    where (c != plugin.Control) && c.Visible
                    select c).ToArray<Control>();
                plugin.Control.Visible = true;
                foreach (Control control in controlArray)
                {
                    control.Hide();
                }
                if (focus)
                {
                    this.Activate();
                    plugin.Control.Focus();
                }
            };
            if (this._shown)
            {
                this.BeginInvoke(a);
            }
            else
            {
                Server.CurrentServer.RunOnMessageLoopThread(a);
            }
        }

        private bool IsPluginInFront(IntPtr parentHandle)
        {
            IntPtr handle = base.Handle;
        Label_0015:
            handle = Native.GetWindow(handle, 3);
            if (!(handle == IntPtr.Zero))
            {
                if (handle == parentHandle)
                {
                    return false;
                }
                goto Label_0015;
            }
            return true;
        }

        protected override void OnActivated(EventArgs e)
        {
            this.IsActive = true;
            if (!(Native.GetWindow(this._lastParentHandle, 3) == base.Handle))
            {
                Native.SetWindowPos(this._lastParentHandle, base.Handle, 0, 0, 0, 0, 0x13);
                this._manager.RequestRelocation(this);
                base.OnActivated(e);
                if (!this._shown)
                {
                    this._shown = true;
                    this.Ready.Set();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                RemotingServices.Disconnect(this);
            }
            catch
            {
            }
            foreach (PluginControl control in this._controls)
            {
                try
                {
                    RemotingServices.Disconnect(control);
                }
                catch
                {
                }
                if (control.Control != null)
                {
                    try
                    {
                        RemotingServices.Disconnect(control.Control);
                    }
                    catch
                    {
                    }
                }
            }
            base.OnClosed(e);
            this._controls.Clear();
            if (!this._shutdownMode)
            {
                this._manager.OnPluginFormClosed(this);
            }
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            if (this._controls.Contains(e.Control))
            {
                PluginControl item = this._controls[e.Control];
                this._controls.Remove(item);
                this._manager.OnPluginRemoved(item);
            }
            base.OnControlRemoved(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            this.IsActive = false;
            base.OnDeactivate(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        internal void OnInfoMessageChanged(PluginControl pic, string value)
        {
            this._manager.OnInfoMessageChanged(pic, value);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            this._threadSafeVisible = base.Visible;
            if (base.Visible)
            {
                this._shown = true;
            }
            base.OnVisibleChanged(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return (this._manager.ProcessKey(msg.Msg, msg.WParam, msg.LParam, keyData) || base.ProcessCmdKey(ref msg, keyData));
        }

        private void PutJustBehind(IntPtr parentHandle, IntPtr mainFormHandle)
        {
            IntPtr targetToPutBehind = this.GetTargetToPutBehind(parentHandle, mainFormHandle);
            if (!(targetToPutBehind == IntPtr.Zero))
            {
                this.PutOurWindowsBehind(targetToPutBehind);
            }
        }

        private void PutOnTop(IntPtr parentHandle)
        {
            if (!this.IsPluginInFront(parentHandle))
            {
                IntPtr window = Native.GetWindow(parentHandle, 3);
                if (window != IntPtr.Zero)
                {
                    this.PutOurWindowsBehind(window);
                }
            }
        }

        private void PutOurWindowsBehind(IntPtr target)
        {
            uint num;
            uint windowThreadProcessId = Native.GetWindowThreadProcessId(base.Handle, out num);
            List<IntPtr> windows = new List<IntPtr>();
            Native.EnumThreadWindows(windowThreadProcessId, delegate (IntPtr hwnd, IntPtr lParam) {
                windows.Add(hwnd);
                return true;
            }, IntPtr.Zero);
            foreach (IntPtr ptr in windows)
            {
                try
                {
                    Native.SetWindowPos(ptr, target, 0, 0, 0, 0, 0x13);
                    target = ptr;
                }
                catch
                {
                }
            }
        }

        private void Scroll(VerticalScrollAmount amount, bool down)
        {
            uint num = 0;
            Control control = base.Controls.Cast<Control>().FirstOrDefault<Control>(c => c.Visible);
            if (control != null)
            {
                if ((amount == VerticalScrollAmount.Line) && down)
                {
                    num = 1;
                }
                else if (amount == VerticalScrollAmount.Line)
                {
                    num = 0;
                }
                else if ((amount == VerticalScrollAmount.Page) && down)
                {
                    num = 3;
                }
                else if (amount == VerticalScrollAmount.Page)
                {
                    num = 2;
                }
                else if ((amount == VerticalScrollAmount.Document) && down)
                {
                    num = 7;
                }
                else
                {
                    if (amount != VerticalScrollAmount.Document)
                    {
                        return;
                    }
                    num = 6;
                }
                VScrollBar bar = control.Controls.OfType<VScrollBar>().FirstOrDefault<VScrollBar>();
                IntPtr lParam = (bar == null) ? IntPtr.Zero : bar.Handle;
                Native.SendMessage(control.Handle, 0x115, new IntPtr((int) num), lParam);
                Native.SendMessage(control.Handle, 8, new IntPtr((int) num), lParam);
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!(!value || this._shouldBeVisible))
            {
                this.Ready.Set();
            }
            else
            {
                base.SetVisibleCore(value);
            }
        }

        private void SetZOrder(IntPtr parentHandle, IntPtr mainFormhandle)
        {
            this._lastParentHandle = parentHandle;
            if (!base.Visible)
            {
                this.ShowNoActivate();
            }
            if (this._clickTransparencySupported)
            {
                this.PutJustBehind(parentHandle, mainFormhandle);
            }
            else
            {
                this.PutOnTop(parentHandle);
            }
        }

        public void ShowNoActivate()
        {
            this._shouldBeVisible = true;
            Native.ShowWindow(base.Handle, Native.ShowWindowCommands.ShowNoActivate);
        }

        public void ShowToolTip(string text)
        {
            this.BeginInvoke(delegate {
                try
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        this._toolTip.Hide(this);
                    }
                    else
                    {
                        Point point = this.PointToClient(Control.MousePosition);
                        point.Y += 20;
                        this._toolTip.Show(text, this, point, 0xbb8);
                    }
                }
                catch
                {
                }
            });
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x80;
                return createParams;
            }
        }

        public LINQPad.UI.EditManager EditManager { get; private set; }

        public bool HasControls
        {
            get
            {
                return (this._controls.Count > 0);
            }
        }

        public bool IsActive { get; private set; }

        public bool ThreadSafeVisible
        {
            get
            {
                return this._threadSafeVisible;
            }
        }
    }
}

