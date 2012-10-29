namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;
    using System.Threading;
    using System.Windows.Forms;

    internal class PluginWindowManager : MarshalByRefObject, IDisposable
    {
        private PluginControl _activeControl;
        private bool _mainFormEventsHandled;
        private QueryControl _queryControl;
        private Timer _tmr;

        public PluginWindowManager(QueryControl qc)
        {
            Timer timer = new Timer {
                Interval = 100
            };
            this._tmr = timer;
            this._queryControl = qc;
            this._tmr.Tick += new EventHandler(this._tmr_Tick);
        }

        private void _tmr_Tick(object sender, EventArgs e)
        {
            this.ActiveFormPulse = this.Form.IsActive ? DateTime.UtcNow : DateTime.MinValue;
            this.UpdateVisibility();
        }

        public PluginControl AddControl(Control c, string heading)
        {
            PluginControl control = this.Form.AddControl(c, heading);
            this._queryControl.OnPluginAdded(control);
            return control;
        }

        internal void CheckMainForm(IntPtr activeWindow)
        {
            try
            {
                if ((MainForm.Instance != null) && (activeWindow == MainForm.Instance.HandleThreadsafe))
                {
                    MainForm.Instance.BeginInvoke(delegate {
                        MainForm.Instance.TopMost = true;
                        MainForm.Instance.TopMost = false;
                    });
                }
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            this.Reset(false);
            try
            {
                RemotingServices.Disconnect(this);
            }
            catch
            {
            }
        }

        public void DisposeControl(PluginControl c)
        {
            PluginForm form = this.Form;
            if (form != null)
            {
                form.InvokeDisposeControl(c);
            }
        }

        public PluginControl[] GetControls()
        {
            return ((this.Form == null) ? new PluginControl[0] : this.Form.GetControls());
        }

        internal void Hide()
        {
            PluginForm form = this.Form;
            if (form != null)
            {
                this.Stop();
                this._activeControl = null;
                form.InvokeHide();
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal void InvokeScroll(VerticalScrollAmount amount, bool down)
        {
            if ((this.Form != null) && (this._activeControl != null))
            {
                this.Form.InvokeScroll(amount, down);
            }
        }

        internal void OnInfoMessageChanged(PluginControl pic, string value)
        {
            try
            {
                if ((this._queryControl != null) && !this._queryControl.IsDisposed)
                {
                    this._queryControl.OnInfoMessageChanged(pic, value);
                }
            }
            catch
            {
            }
        }

        internal void OnPluginFormClosed(PluginForm pluginForm)
        {
            if ((this._queryControl != null) && !this._queryControl.IsDisposed)
            {
                Action method = delegate {
                    if (pluginForm == this.Form)
                    {
                        this.Stop();
                        this.Form = null;
                        this._activeControl = null;
                        this._queryControl.OnAllPluginsRemoved();
                    }
                };
                if (this._queryControl.InvokeRequired)
                {
                    this._queryControl.BeginInvoke(method);
                }
                else
                {
                    method();
                }
            }
        }

        internal void OnPluginRemoved(PluginControl pic)
        {
            this._queryControl.OnPluginRemoved(pic);
        }

        internal void PluginJustAdded(PluginControl plugin)
        {
            this._queryControl.OnPluginAdded(plugin);
        }

        internal bool ProcessKey(int msg, IntPtr wParam, IntPtr lParam, Keys keyData)
        {
            Func<object> method = null;
            try
            {
                switch (keyData)
                {
                    case (Keys.Control | Keys.A):
                    case (Keys.Control | Keys.C):
                    case (Keys.Control | Keys.V):
                    case (Keys.Control | Keys.X):
                    case (Keys.Control | Keys.Y):
                    case (Keys.Control | Keys.Z):
                        break;

                    default:
                    {
                        bool flag2 = true;
                        if (method == null)
                        {
                            method = delegate {
                                bool flag;
                                if (!(flag = HotKeyManager.HandleKey(MainForm.Instance, this._queryControl, keyData)))
                                {
                                    Message m = new Message {
                                        HWnd = MainForm.Instance.Handle,
                                        Msg = msg,
                                        WParam = wParam,
                                        LParam = lParam
                                    };
                                    flag = MainForm.Instance.ProcessCmdKeyForMenu(ref m, keyData);
                                }
                                if (flag)
                                {
                                    MainForm.Instance.Activate();
                                }
                                return flag;
                            };
                        }
                        return flag2.Equals(this._queryControl.Invoke(method));
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        internal void Relocate(Rectangle rectangle)
        {
            PluginForm form = this.Form;
            if (form != null)
            {
                form.InvokeRelocate(rectangle);
            }
        }

        internal void RequestRelocation(PluginForm pluginForm)
        {
            this._queryControl.RequestWinManagerRelocation();
        }

        internal void RequestShow(PluginControl control)
        {
            this._queryControl.RequestPlugInShow(control);
        }

        public void Reset(bool delayClose)
        {
            Action close;
            this.Stop();
            PluginForm form = this.Form;
            if (form != null)
            {
                this.Form = null;
                this._activeControl = null;
                close = delegate {
                    try
                    {
                        form.InvokeCloseWithShutdown();
                    }
                    catch
                    {
                    }
                };
                if (delayClose)
                {
                    ThreadPool.QueueUserWorkItem(delegate (object _) {
                        Thread.Sleep(0x3e8);
                        close();
                    });
                }
                else
                {
                    close();
                }
            }
        }

        internal void Show(PluginControl control, bool focus)
        {
            bool clickTransparencySupported = (Program.TransparencyKey.R == Program.TransparencyKey.G) && (Program.TransparencyKey.R == Program.TransparencyKey.B);
            if ((Environment.OSVersion.Version.Major == 5) && (Screen.PrimaryScreen.BitsPerPixel == 0x20))
            {
                clickTransparencySupported = false;
            }
            this.RequestRelocation(this.Form);
            PluginForm form = this.Form;
            if (form != null)
            {
                this._activeControl = control;
                this.Start();
                form.InvokeShow(this._activeControl, this.ApplicationFormHandle, MainForm.Instance.HandleThreadsafe, focus, clickTransparencySupported);
            }
        }

        public void ShowToolTip(string text)
        {
            if (this.Form != null)
            {
                this.Form.ShowToolTip(text);
            }
        }

        private void Start()
        {
            this._tmr.Start();
            if (!this._mainFormEventsHandled)
            {
                this._mainFormEventsHandled = true;
                MainForm.Instance.Activated += new EventHandler(this.UpdateVisibility);
                MainForm.Instance.Deactivate += new EventHandler(this.UpdateVisibility);
                MainForm.Instance.ResultsDockForm.Activated += new EventHandler(this.UpdateVisibility);
                MainForm.Instance.ResultsDockForm.Deactivate += new EventHandler(this.UpdateVisibility);
            }
        }

        private void Stop()
        {
            this.ActiveFormPulse = DateTime.MinValue;
            this._tmr.Stop();
            if (this._mainFormEventsHandled)
            {
                this._mainFormEventsHandled = false;
                MainForm.Instance.Activated -= new EventHandler(this.UpdateVisibility);
                MainForm.Instance.Deactivate -= new EventHandler(this.UpdateVisibility);
                MainForm.Instance.ResultsDockForm.Activated -= new EventHandler(this.UpdateVisibility);
                MainForm.Instance.ResultsDockForm.Deactivate -= new EventHandler(this.UpdateVisibility);
            }
        }

        private void UpdateVisibility()
        {
            if (MainForm.Instance != null)
            {
                PluginForm form = this.Form;
                if (MainForm.Instance.WindowState == FormWindowState.Minimized)
                {
                    if ((form != null) && form.ThreadSafeVisible)
                    {
                        this.Form.InvokeHideIfVisible();
                    }
                }
                else if ((form != null) && ((MainForm.Instance.IsAppActive || MainForm.Instance.IsActive) || MainForm.Instance.ResultsDockForm.IsActive))
                {
                    if (MainForm.Instance.IsSplitting)
                    {
                        MainForm.Instance.BeginInvoke(delegate {
                            MainForm.Instance.TopMost = true;
                            MainForm.Instance.TopMost = false;
                        });
                    }
                    else
                    {
                        try
                        {
                            if ((MainForm.Instance.CurrentQueryControl != this._queryControl) || (this._activeControl == null))
                            {
                                form.InvokeHideIfVisible();
                            }
                            else
                            {
                                form.InvokeSetZOrder(this.ApplicationFormHandle, MainForm.Instance.HandleThreadsafe);
                            }
                        }
                        catch (AppDomainUnloadedException)
                        {
                            this.Dispose();
                        }
                    }
                }
            }
        }

        private void UpdateVisibility(object sender, EventArgs e)
        {
            this.UpdateVisibility();
        }

        public DateTime ActiveFormPulse { get; private set; }

        private IntPtr ApplicationFormHandle
        {
            get
            {
                return (MainForm.Instance.ResultsDockForm.AreResultsTorn ? MainForm.Instance.ResultsDockForm.HandleThreadsafe : MainForm.Instance.HandleThreadsafe);
            }
        }

        public bool ContainsFocus
        {
            get
            {
                return ((this.Form != null) && this.Form.ContainsFocus);
            }
        }

        public LINQPad.UI.EditManager EditManager
        {
            get
            {
                return ((this.Form == null) ? null : this.Form.EditManager);
            }
        }

        internal PluginForm Form { get; set; }

        public bool HasControls
        {
            get
            {
                return ((this.Form != null) && this.Form.HasControls);
            }
        }
    }
}

