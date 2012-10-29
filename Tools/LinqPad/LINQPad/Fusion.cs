namespace LINQPad
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class Fusion
    {
        [DllImport("fusion.dll", PreserveSig=false)]
        internal static extern void CreateAssemblyEnum(out IAssemblyEnum ppEnum, IntPtr pUnkReserved, IAssemblyName pName, ASM_CACHE_FLAGS dwFlags, IntPtr pvReserved);

        [Flags]
        internal enum ASM_CACHE_FLAGS
        {
            ASM_CACHE_DOWNLOAD = 4,
            ASM_CACHE_GAC = 2,
            ASM_CACHE_ZAP = 1
        }

        [Flags]
        internal enum ASM_CMP_FLAGS
        {
            ALL = 0xff,
            BUILD_NUMBER = 8,
            CULTURE = 0x40,
            CUSTOM = 0x80,
            DEFAULT = 0x100,
            MAJOR_VERSION = 2,
            MINOR_VERSION = 4,
            NAME = 1,
            PUBLIC_KEY_TOKEN = 0x20,
            REVISION_NUMBER = 0x10
        }

        [Flags]
        internal enum ASM_DISPLAY_FLAGS
        {
            CULTURE = 2,
            CUSTOM = 0x10,
            LANGUAGEID = 0x40,
            PROCESSORARCHITECTURE = 0x20,
            PUBLIC_KEY = 8,
            PUBLIC_KEY_TOKEN = 4,
            VERSION = 1
        }

        internal enum ASM_NAME
        {
            PUBLIC_KEY,
            PUBLIC_KEY_TOKEN,
            HASH_VALUE,
            NAME,
            MAJOR_VERSION,
            MINOR_VERSION,
            BUILD_NUMBER,
            REVISION_NUMBER,
            CULTURE,
            PROCESSOR_ID_ARRAY,
            OSINFO_ARRAY,
            HASH_ALGID,
            ALIAS,
            CODEBASE_URL,
            CODEBASE_LASTMOD,
            NULL_PUBLIC_KEY,
            NULL_PUBLIC_KEY_TOKEN,
            CUSTOM,
            NULL_CUSTOM,
            MVID
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("21B8916C-F28E-11D2-A473-00C04F8EF448")]
        internal interface IAssemblyEnum
        {
            [PreserveSig]
            int GetNextAssembly(IntPtr pvReserved, out Fusion.IAssemblyName ppName, uint dwFlags);
            [PreserveSig]
            int Reset();
            [PreserveSig]
            int Clone(out Fusion.IAssemblyEnum ppEnum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11D2-9833-00C04FC31D2E")]
        internal interface IAssemblyName
        {
            [PreserveSig]
            int SetProperty(Fusion.ASM_NAME PropertyId, IntPtr pvProperty, uint cbProperty);
            [PreserveSig]
            int GetProperty(Fusion.ASM_NAME PropertyId, StringBuilder pvProperty, ref uint pcbProperty);
            [PreserveSig]
            int Finalize();
            [PreserveSig]
            int GetDisplayName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szDisplayName, ref uint pccDisplayName, Fusion.ASM_DISPLAY_FLAGS dwDisplayFlags);
            [PreserveSig]
            int BindToObject(ref Guid refIID, [MarshalAs(UnmanagedType.IUnknown)] object pUnkSink, [MarshalAs(UnmanagedType.IUnknown)] object pUnkContext, [MarshalAs(UnmanagedType.LPWStr)] string szCodeBase, long llFlags, IntPtr pvReserved, uint cbReserved, out IntPtr ppv);
            [PreserveSig]
            int GetName(ref uint lpcwBuffer, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzName);
            [PreserveSig]
            int GetVersion(out uint pdwVersionHi, out uint pdwVersionLow);
            [PreserveSig]
            int IsEqual(Fusion.IAssemblyName pName, Fusion.ASM_CMP_FLAGS dwCmpFlags);
            [PreserveSig]
            int Clone(out Fusion.IAssemblyName pName);
        }
    }
}

