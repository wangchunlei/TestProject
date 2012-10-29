namespace LINQPad.UI
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    internal class ResultsWebBrowser : WebBrowser
    {
        public ResultsWebBrowser()
        {
            if (!Debugger.IsAttached)
            {
                try
                {
                    base.AllowWebBrowserDrop = false;
                }
                catch
                {
                }
            }
        }
    }
}

