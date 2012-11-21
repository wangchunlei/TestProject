using System;
using System.Net;

namespace Novotny.IECookieHelper
{
    public class CookieManager : IDisposable
    {
        private bool _disposed;
        private readonly RemoteServiceManager _remoteService = new RemoteServiceManager();
        private static readonly OperatingSystem _os = Environment.OSVersion;
        private bool _serviceUsed;

        /// <summary>
        /// Length of idle time before the helper process times out.  This must be set before first use.
        /// </summary>
        public double IdleTimeOutSeconds
        {
            get
            {
                return _remoteService.TimeOutSeconds;
            }
            set
            {
                if (_serviceUsed)
                    throw new InvalidOperationException("The timeout can only be set before the helper is first used.");

                _remoteService.TimeOutSeconds = value;
            }
        }

        public CookieContainer GetCookieContainerUri(Uri uri)
        {
            CheckDisposed();

            if(IsAtLeastVista && Win32Native.IEIsProtectedModeURL(uri.AbsoluteUri) == 0)
            {
                Console.WriteLine("00000000000000");
                return GetCookieContainerViaHelper(uri);
            }
            else
            {
                Console.WriteLine("11111111111111");
                return GetCookieContainerNormal(uri);
            }
        }

        private CookieContainer GetCookieContainerViaHelper(Uri uri)
        {
            string headers = string.Empty;
            try
            {
                _serviceUsed = true;

                headers = _remoteService.Service.GetCookiesForUrl(uri);
            }
            catch
            {
                try
                {
                    // Try once more
                    headers = _remoteService.Service.GetCookiesForUrl(uri);
                }
                catch
                {
                }
            }

            return HttpCookieManager.GetCookieContainerFromString(uri, headers);
        }

        private static CookieContainer GetCookieContainerNormal(Uri uri)
        {
            return HttpCookieManager.GetCookieContainerUri(uri);
        }

        private static bool IsAtLeastVista
        {
            get
            {
                return _os.Platform == PlatformID.Win32NT && _os.Version.Major >= 6;
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());
        }

        #region IDisposable Members

        public void Dispose()
        {
            if(!_disposed)
            {
                _disposed = true;
                _remoteService.Dispose();
            }
        }

        #endregion
    }
}
