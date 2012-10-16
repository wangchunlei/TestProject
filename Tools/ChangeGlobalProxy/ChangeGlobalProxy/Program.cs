using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ChangeGlobalProxy
{
    class Program
    {
        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        bool settingsReturn, refreshReturn;
        RegistryKey RegKey = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
        void SetProxy(string proxyUri, int enable = 1)
        {
            RegKey.SetValue("ProxyServer", proxyUri);
            RegKey.SetValue("ProxyEnable", enable);

            // These lines implement the Interface in the beginning of program 
            // They cause the OS to refresh the settings, causing IP to realy update
            settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
        static void Main(string[] args)
        {
            var proxyUri = "http://127.0.0.1:8087";


            var pro = new Program();
            pro.SetProxy(proxyUri);

            Console.WriteLine(string.Format("当前代理：{0}", proxyUri));

            Console.WriteLine("任意键结束");
            Console.ReadKey();

            pro.SetProxy(proxyUri, 0);
        }

    }
}
