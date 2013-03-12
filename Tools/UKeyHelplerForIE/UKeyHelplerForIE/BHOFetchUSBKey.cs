using System;
using System.Runtime.InteropServices;
using Domas.DAP.ADF.UkeyManager;
using Microsoft.Win32;
using MyBHOTool;
using SHDocVw;
using mshtml;

namespace UKeyHelplerForIE
{
    [ComVisible(true), ClassInterface(ClassInterfaceType.None), Guid("14385712-A335-4313-8F14-2F508ADBD0F4")]
    public class BHOFetchUSBKey : IObjectWithSite
    {
        private InternetExplorer ieInstance;
        private const string BHORegistryKey =
            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects";

        #region Com Register/UnRegister Methods
        [ComRegisterFunction]
        public static void RegisterBHO(Type t)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(BHORegistryKey, true);
            if (key == null)
            {
                key = Registry.LocalMachine.CreateSubKey(BHORegistryKey);
            }
            string bhoKeyStr = t.GUID.ToString("B");

            RegistryKey bhoKey = key.OpenSubKey(bhoKeyStr, true);

            if (bhoKey == null)
            {
                bhoKey = key.CreateSubKey(bhoKeyStr);
            }

            // NoExplorer:dword = 1 prevents the BHO to be loaded by Explorer
            string name = "NoExplorer";
            object value = (object)1;
            bhoKey.SetValue(name, value);
            key.Close();
            bhoKey.Close();
        }

        [ComUnregisterFunction]
        public static void UnregisterBHO(Type t)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(BHORegistryKey, true);
            string guidString = t.GUID.ToString("B");
            if (key != null)
            {
                key.DeleteSubKey(guidString, false);
            }
        }
        #endregion

        #region IObjectWithSite Members
        public void SetSite(object pUnkSite)
        {
            if (pUnkSite != null)
            {
                ieInstance = pUnkSite as InternetExplorer;
                ieInstance.DocumentComplete += new DWebBrowserEvents2_DocumentCompleteEventHandler(ieInstance_DocumentComplete);
                ieInstance.DownloadComplete += new DWebBrowserEvents2_DownloadCompleteEventHandler(ieInstance_DownloadComplete);
            }
        }

        public void GetSite(ref Guid riid, out object ppvSite)
        {
            IntPtr punk = Marshal.GetIUnknownForObject(ieInstance);
            ppvSite = new object();
            IntPtr ppvSiteIntPtr = Marshal.GetIUnknownForObject(ppvSite);
            int hr = Marshal.QueryInterface(punk, ref riid, out ppvSiteIntPtr);
            Marshal.ThrowExceptionForHR(hr);
            Marshal.Release(punk);
            Marshal.Release(ppvSiteIntPtr);
        }
        #endregion

        #region event handler

        private void ieInstance_DownloadComplete()
        {
            HTMLDocument doc = this.ieInstance.Document as HTMLDocument;
            if (doc != null)
            {
                IHTMLWindow2 tmpWindow = doc.parentWindow;
                if (tmpWindow != null)
                {
                    HTMLWindowEvents2_Event events = (tmpWindow as HTMLWindowEvents2_Event);
                    try
                    {
                        //if (events != null)
                        {
                            events.onload -= new HTMLWindowEvents2_onloadEventHandler(RefreshHandler);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    events.onload += new HTMLWindowEvents2_onloadEventHandler(RefreshHandler);
                }
            }
        }

        void ieInstance_DocumentComplete(object pDisp, ref object URL)
        {
            var url = URL as string;
            if (string.IsNullOrEmpty(url) || url.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
        public void RefreshHandler(IHTMLEventObj pDisp)
        {
            var explorer = this.ieInstance;

            if (explorer != null)
            {
                try
                {
                    var document = explorer.Document as HTMLDocument;
                    var loginId = document.getElementById("LoginID");
                    if (loginId != null)
                    {
                        var loginInput = loginId as HTMLInputElement;
                        var ukey = Domas.DAP.ADF.UkeyManager.UkeyManager.GetUkey(UkeyTpye.A05);
                        if (ukey != null)
                        {
                            //loginInput.value = ukey.GetUkeyCode();
                            loginInput.readOnly = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
                //SetHandler(explorer);
            }
        }
        void SetHandler(InternetExplorer explorer)
        {
            try
            {
                HTMLDocumentEventHelper helper =
                    new HTMLDocumentEventHelper(explorer.Document as HTMLDocument);
                helper.oncontextmenu += new HtmlEvent(oncontextmenuHandler);
            }
            catch (Exception)
            {

                throw;
            }
        }

        void oncontextmenuHandler(IHTMLEventObj e)
        {
            e.returnValue = false;
            System.Windows.Forms.MessageBox.Show("禁用右键");
        }

        #endregion
    }
}
