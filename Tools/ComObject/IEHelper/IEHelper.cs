using System;
using System.Runtime.InteropServices;
using IEHelper;

namespace IEHelper
{
    [Guid("F11235E5-CAB1-4E74-8F75-EA22FD8E397E")]
    public interface IEHelper_Interface
    {
        [DispId(1)]
        string GetPCInfo();
    }

    [Guid("00BBB8D4-0B76-4DEB-A665-4D43C22624FC"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IEHelper_Events
    {

    }

    [Guid("35464329-6813-4E02-AFEC-17599EF6124B"), ClassInterface(ClassInterfaceType.None),
    ComSourceInterfaces(typeof(IEHelper_Events))]
    public class IEHelper_Class : IEHelper_Interface
    {
        public string GetPCInfo()
        {
            return "abc";
        }
    }


}
