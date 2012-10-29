namespace LINQPad.UI
{
    using System;
    using System.Windows.Forms;

    internal class ToolStripEx : ToolStrip
    {
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

