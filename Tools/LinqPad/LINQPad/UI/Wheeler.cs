namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using LINQPad;
    using System;
    using System.Windows.Forms;

    internal class Wheeler : IMessageFilter
    {
        private static Wheeler _instance;
        private const int SysKeyDownContextMask = 0x20000000;

        private Wheeler()
        {
        }

        public static void Register()
        {
            if (_instance == null)
            {
                _instance = new Wheeler();
                Application.AddMessageFilter(_instance);
            }
        }

        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            try
            {
                if (((((m.Msg == 260) && ((m.LParam.ToInt32() & 0x20000000) == 0x20000000)) && ((m.WParam.ToInt32() == 0x25) || (m.WParam.ToInt32() == 0x27))) && ((MainForm.Instance != null) && (MainForm.Instance.CurrentQueryControl != null))) && MainForm.Instance.CurrentQueryControl.DoesBrowserHaveFocus())
                {
                    MainForm.Instance.CurrentQueryControl.NavigateResultPanel(m.WParam.ToInt32() == 0x27);
                    return true;
                }
                if (((((m.Msg == 0x201) || (m.Msg == 0x204)) || ((m.Msg == 0x20a) || (m.Msg == 0x100))) || (m.Msg == 260)) && (MainForm.Instance != null))
                {
                    MainForm.Instance.RegisterUIActivity();
                }
                if (m.Msg == 0x20a)
                {
                    IntPtr ptr2;
                    try
                    {
                        ptr2 = Native.WindowFromPoint(Control.MousePosition);
                    }
                    catch
                    {
                        return false;
                    }
                    if (ptr2 == IntPtr.Zero)
                    {
                        return false;
                    }
                    Control control = Control.FromChildHandle(ptr2);
                    if (control is WebBrowser)
                    {
                        if (!control.Focused && !control.InvokeRequired)
                        {
                            control.Focus();
                            m.HWnd = control.Handle;
                            try
                            {
                                Native.SendMessage(control.Handle, (uint) m.Msg, m.WParam, m.LParam);
                            }
                            catch
                            {
                            }
                        }
                        return false;
                    }
                    if (((((control is TreeView) || (control is RichTextBox)) || ((control is TextBox) && ((TextBox) control).Multiline)) || ((control is ActiproSoftware.SyntaxEditor.SyntaxEditor) || (control is ListView))) || (control is ListBox))
                    {
                        if (control.InvokeRequired)
                        {
                            return false;
                        }
                        m.HWnd = control.Handle;
                        try
                        {
                            Native.SendMessage(control.Handle, (uint) m.Msg, m.WParam, m.LParam);
                        }
                        catch
                        {
                        }
                        return true;
                    }
                }
                return false;
            }
            catch (Exception exception)
            {
                Log.Write(exception);
                return false;
            }
        }
    }
}

