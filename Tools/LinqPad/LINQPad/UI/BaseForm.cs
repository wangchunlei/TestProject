namespace LINQPad.UI
{
    using System;
    using System.Windows.Forms;

    internal class BaseForm : Form
    {
        private IntPtr _handle;

        public BaseForm()
        {
            try
            {
                this.Font = FontManager.GetDefaultFont();
            }
            catch
            {
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            this._handle = base.Handle;
            base.OnHandleCreated(e);
        }

        public IntPtr HandleThreadsafe
        {
            get
            {
                return (base.InvokeRequired ? this._handle : base.Handle);
            }
        }
    }
}

