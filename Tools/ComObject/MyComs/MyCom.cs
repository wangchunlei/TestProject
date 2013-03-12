using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MyComs
{
    [ComVisible(true)]
    [Guid("F11235E5-CAB1-4E74-8F75-EA22FD8E397E")]
    public interface IEHelper_Interface
    {
        [DispId(0x00000001)]
        string GetPCInfo();
    }

    [Guid("00BBB8D4-0B76-4DEB-A665-4D43C22624FC"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IEHelper_Events
    {

    }

    [Guid("35464329-6813-4E02-AFEC-17599EF6124B")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IEHelper_Events))]
    public class IEHelper_Class : IEHelper_Interface, IObjectSafety
    {
        private const int INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001;
        private const int INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002;
        private const int S_OK = 0;
        public string GetPCInfo()
        {
            return "abcddddddddddddddddddd";
        }

        public int GetInterfaceSafetyOptions(ref Guid riid, out int pdwSupportedOptions, out int pdwEnabledOptions)
        {
            pdwSupportedOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER | INTERFACESAFE_FOR_UNTRUSTED_DATA;
            pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER | INTERFACESAFE_FOR_UNTRUSTED_DATA;
            return S_OK;   // return S_OK
        }

        public int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions)
        {
            return S_OK;   // return S_OK
        }
    }

    [ComImport]
    [Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064")] // This is the only Guid that cannot be modifed in this file
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IObjectSafety
    {
        [PreserveSig]
        int GetInterfaceSafetyOptions(ref Guid riid, out int pdwSupportedOptions, out int pdwEnabledOptions);

        [PreserveSig]
        int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions);
    }
}
