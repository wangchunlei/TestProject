namespace LINQPad.UI
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;

    internal class SaveBrowserScrollPos : IDisposable
    {
        private WebBrowser _browser;
        private int _top;

        public SaveBrowserScrollPos(WebBrowser browser, bool alwaysScrollToBottom)
        {
            this._browser = browser;
            if (alwaysScrollToBottom)
            {
                this._top = -1;
            }
            else if (browser.Document != null)
            {
                try
                {
                    object domDocument = browser.Document.DomDocument;
                    object target = domDocument.GetType().InvokeMember("documentElement", BindingFlags.GetProperty, null, domDocument, null);
                    this._top = (int) target.GetType().InvokeMember("scrollTop", BindingFlags.GetProperty, null, target, null);
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            if (this._top != 0)
            {
                int retries = 0;
                Timer tmr = new Timer {
                    Interval = 1,
                    Enabled = true
                };
                tmr.Tick += delegate (object sender, EventArgs e) {
                    if (++retries > 2)
                    {
                        tmr.Dispose();
                    }
                    try
                    {
                        object domDocument = this._browser.Document.DomDocument;
                        object target = domDocument.GetType().InvokeMember("documentElement", BindingFlags.GetProperty, null, domDocument, null);
                        int height = this._top;
                        if (height == -1)
                        {
                            HtmlElement body = this._browser.Document.Body;
                            if (body == null)
                            {
                                return;
                            }
                            height = body.ScrollRectangle.Height;
                        }
                        target.GetType().InvokeMember("scrollTop", BindingFlags.SetProperty, null, target, new object[] { height });
                        tmr.Dispose();
                    }
                    catch
                    {
                    }
                };
            }
        }
    }
}

