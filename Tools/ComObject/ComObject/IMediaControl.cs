using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ComObject
{
    [Guid("56A868B1-0AD4-11CE-B03A-0020AF0BA770"),
         InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaControl
    {
        void Run();
        void Pause();

        void Stop();

        void GetState([In] int msTimeout, [Out] out int pfs);

        void RenderFile(
              [In, MarshalAs(UnmanagedType.BStr)] string strFilename);

        void AddSourceFilter(
              [In, MarshalAs(UnmanagedType.BStr)] string strFilename,
              [Out, MarshalAs(UnmanagedType.Interface)]
                  out object ppUnk);

        [return: MarshalAs(UnmanagedType.Interface)]
        object FilterCollection();

        [return: MarshalAs(UnmanagedType.Interface)]
        object RegFilterCollection();

        void StopWhenReady();
    }
}
