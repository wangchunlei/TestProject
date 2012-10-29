namespace LINQPad.UI
{
    using System;
    using System.Windows.Forms;

    internal class MenuStripEx : MenuStrip
    {
        public bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            return base.ProcessCmdKey(ref m, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if ((m.Msg == 0x21L) && (m.Result == ((IntPtr) 2L)))
            {
                m.Result = (IntPtr) 1L;
            }
        }
    }
}

