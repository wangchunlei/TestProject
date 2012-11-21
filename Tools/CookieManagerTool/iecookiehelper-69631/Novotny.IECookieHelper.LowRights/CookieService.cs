using System;

namespace Novotny.IECookieHelper.LowRights
{
    // NOTE: If you change the class name "CookieService" here, you must also update the reference to "CookieService" in App.config.
    internal class CookieService : ICookieService
    {
        #region ICookieService Members

        public void ExitProcess()
        {
            Program._exit.Set();
        }

        public string GetCookiesForUrl(Uri address)
        {
            Program._lastUpdated = DateTime.Now;
            return HttpCookieManager.RetrieveIECookiesForUrl(address.AbsoluteUri);
        }

        public void KeepAlive()
        {
            Program._lastUpdated = DateTime.Now;
        }
        #endregion
    }
}
