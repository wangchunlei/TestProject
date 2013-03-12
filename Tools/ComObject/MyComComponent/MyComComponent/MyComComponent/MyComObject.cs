using System;
using System.Runtime.InteropServices;

namespace MyComComponent
{
    [Guid("4794D615-BE51-4a1e-B1BA-453F6E9337C4")] // Use this GUID to create an instance of MyComObject
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IComEvents))] // This exposes the events in IComEvents from the COM component. 
                                              // Note that IComEvents should not appear in the list of implemented
                                              // interfaces (i.e. IComObject and IObjectSafety).
    public class MyComObject : 
        IComOjbect, // Implement the COM interface that defines the operations that can
                    // be performed on MyComObject.
        IObjectSafety // implement IObjectSafety to supress the unsafe for scripting 
                      // warning message
    {
        #region Constants
        // Constants for implementation of the IObjectSafety interface.
        private const int INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001;
        private const int INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002;
        private const int S_OK = 0;
        #endregion

        #region IComEvents
        /// <summary>
        /// Raise the MyFirstEvent event. This method is not exposed through COM.
        /// </summary>
        /// <param name="args"></param>
        [ComVisible(false)]
        private void OnMyFirstEvent(string args)
        {
            if (MyFirstEvent != null)
            {
                MyFirstEvent(args);
            }
        }

        /// <summary>
        /// Delegate for the MyFirstEvent 
        /// </summary>
        /// <param name="args"></param>
        [ComVisible(false)]
        public delegate void MyFirstEventHandler(string args);

        /// <summary>
        /// Implements the event in IComEvents interface
        /// </summary>
        public event MyFirstEventHandler MyFirstEvent;
        #endregion

        #region IComObject operations
        /// <summary>
        /// Implements the MyFirstCommand method that's exposed in the IComObject interface.
        /// MyFirstEvent will be raised, with the given argument as a parameter.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public int MyFirstComCommand(string arg)
        {
            // Trigger the event MyFirstEvent
            OnMyFirstEvent(arg);

            return (int)DateTime.Now.Ticks;
        }

        /// <summary>
        /// Implements the Dispose method that's exposed in the IComObject interface.
        /// Since we're in the world of COM, we provide our own Dispose method, as 
        /// opposed to just implementing the IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            System.Windows.Forms.MessageBox.Show("MyComComponent is now disposed");
        }
        #endregion

        #region IObjectSafety Methods
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
        #endregion
    }

    #region Interface definitions
    /// <summary>
    /// Defines the methods than can be called on an object deriving from this COM interface.
    /// Any class that wants to expose these methods should implement this interface.
    /// </summary>
    [Guid("4B3AE7D8-FB6A-4558-8A96-BF82B54F329C")]
    [ComVisible(true)]
    public interface IComOjbect
    {
        [DispId(0x10000001)]
        int MyFirstComCommand(string arg);

        [DispId(0x10000002)]
        void Dispose();
    }

    /// <summary>
    /// Defines events that will be raised from the associated COM object.
    /// Don't derive from this interface. Instead, ,ark any class that uses 
    /// this interface with the attribute [ComSourceInterfaces(typeof(IComEvents))].
    /// Any class that uses this interface should implement a public event called 
    /// MyFirstEvent using a delegate that returns void and accepts a single string 
    /// parameter called args. e.g. 
    ///     
    ///     public delegate void MyFirstEventHandler(string args);
    /// 
    /// </summary>
    [Guid("ECA5DD1D-096E-440c-BA6A-0118D351650B")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IComEvents
    {
        [DispId(0x00000001)]
        void MyFirstEvent(string args);
    }

    /// <summary>
    /// Import the IObjectSaftety COM Interface. 
    /// See http://msdn.microsoft.com/en-us/library/aa768224(VS.85).aspx
    /// </summary>
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
    #endregion
}
