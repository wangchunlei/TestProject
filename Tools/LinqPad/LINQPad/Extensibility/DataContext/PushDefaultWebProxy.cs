namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using System;
    using System.Net;

    internal class PushDefaultWebProxy : IDisposable
    {
        private IWebProxy _oldProxy;
        private bool _proxyChanged;

        public PushDefaultWebProxy()
        {
            WebProxy webProxy = Util.GetWebProxy();
            if (webProxy != null)
            {
                this._oldProxy = WebRequest.DefaultWebProxy;
                this._proxyChanged = true;
                WebRequest.DefaultWebProxy = webProxy;
            }
        }

        public void Dispose()
        {
            if (this._proxyChanged)
            {
                WebRequest.DefaultWebProxy = this._oldProxy;
            }
        }
    }
}

