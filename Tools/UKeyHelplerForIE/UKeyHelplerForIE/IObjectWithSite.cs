using System;
using System.Runtime.InteropServices;

namespace UKeyHelplerForIE
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")]
    public interface IObjectWithSite
    {
        [PreserveSig]
        void SetSite([In, MarshalAs(UnmanagedType.IUnknown)]object pUnkSite);
        [PreserveSig]
        void GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)]out object ppvSite);
    }
}
