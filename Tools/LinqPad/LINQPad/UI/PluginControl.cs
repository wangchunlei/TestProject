namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class PluginControl : MarshalByRefObject
    {
        public readonly string Heading;
        public readonly LINQPad.OutputPanel OutputPanel;

        public PluginControl(System.Windows.Forms.Control control, string heading)
        {
            this.Control = control;
            this.Heading = heading;
            this.OutputPanel = new LINQPad.OutputPanel(this);
            control.Disposed += new EventHandler(this.control_Disposed);
        }

        private void control_Disposed(object sender, EventArgs e)
        {
            try
            {
                this.Control.Disposed -= new EventHandler(this.control_Disposed);
                this.OutputPanel.OnPanelClosed();
            }
            catch
            {
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal void OnInfoMessageChanged(string value)
        {
            this.InfoMessage = value;
            if ((this.Control != null) && !this.Control.IsDisposed)
            {
                PluginForm form = this.Control.FindForm() as PluginForm;
                if (form != null)
                {
                    form.OnInfoMessageChanged(this, value);
                }
            }
        }

        public System.Windows.Forms.Control Control { get; set; }

        public string InfoMessage { get; private set; }

        public string ToolTipText { get; set; }
    }
}

