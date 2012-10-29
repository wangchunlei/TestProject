namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using LINQPad.UI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;

    public class OutputPanel
    {
        private string _infoMessage;
        private PluginControl _pic;

        public event EventHandler PanelClosed;

        public event EventHandler QueryEnded;

        internal OutputPanel(PluginControl c)
        {
            this._pic = c;
        }

        internal void Activate()
        {
            Server currentServer = Server.CurrentServer;
            if ((currentServer != null) && (currentServer.PluginWindowManager != null))
            {
                currentServer.PluginWindowManager.RequestShow(this._pic);
            }
        }

        public void Close()
        {
            Control control = this.GetControl();
            if (control != null)
            {
                control.Dispose();
            }
        }

        public Control GetControl()
        {
            return this._pic.Control;
        }

        public UIElement GetWpfElement()
        {
            ElementHost control = this.GetControl() as ElementHost;
            if (control == null)
            {
                return null;
            }
            return control.Child;
        }

        internal void OnPanelClosed()
        {
            EventHandler panelClosed = this.PanelClosed;
            this.PanelClosed = null;
            if (panelClosed != null)
            {
                foreach (EventHandler handler2 in panelClosed.GetInvocationList())
                {
                    try
                    {
                        handler2(this, EventArgs.Empty);
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal void OnQueryEnded(bool closing, bool canceled)
        {
            this.IsPanelClosing = closing;
            this.IsQueryCanceled = canceled;
            EventHandler queryEnded = this.QueryEnded;
            this.QueryEnded = null;
            if (queryEnded != null)
            {
                foreach (EventHandler handler2 in queryEnded.GetInvocationList())
                {
                    try
                    {
                        handler2(this, EventArgs.Empty);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public string Heading
        {
            get
            {
                return this._pic.Heading;
            }
        }

        public string InfoMessage
        {
            get
            {
                return this._infoMessage;
            }
            set
            {
                if (!(this._infoMessage == value))
                {
                    this._infoMessage = value;
                    this._pic.OnInfoMessageChanged(value);
                }
            }
        }

        private bool IsAlive
        {
            get
            {
                return ((this._pic.Control != null) && !this._pic.Control.IsDisposed);
            }
        }

        public bool IsPanelClosing { get; private set; }

        public bool IsQueryCanceled { get; private set; }

        public bool IsVisible
        {
            get
            {
                return (this.IsAlive && this._pic.Control.Visible);
            }
        }

        public object Tag { get; set; }

        public string ToolTip
        {
            get
            {
                return this._pic.ToolTipText;
            }
            set
            {
                this._pic.ToolTipText = value;
            }
        }
    }
}

