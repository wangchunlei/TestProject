using System;
using System.ServiceModel;

namespace Novotny.IECookieHelper.LowRights
{
    // NOTE: If you change the interface name "ICookieService" here, you must also update the reference to "ICookieService" in App.config.
    [ServiceContract]
    internal interface ICookieService
    {
        [OperationContract(IsOneWay=true)]
        void ExitProcess();

        [OperationContract]
        string GetCookiesForUrl(Uri address);

        [OperationContract(IsOneWay = true)]
        void KeepAlive();

    }
}
