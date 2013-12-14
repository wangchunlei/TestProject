using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace DNSTest
{
    class Program
    {
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        static void Main()
        {
            // Do a few lookups by host name and address  
            var ips = DNSLookup(new string[] { "g.cn", "google.com.hk" });
            Console.WriteLine(ips);
            Copy(ips.ToString());
            // Keep the console window open in debug mode  
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
        private static void Copy(string content)
        {
            OpenClipboard(IntPtr.Zero);

            var ptr = Marshal.StringToHGlobalUni(content);
            SetClipboardData(13, ptr);
            CloseClipboard();
            Marshal.FreeHGlobal(ptr);
        }
        protected static StringBuilder DNSLookup(string[] hostNameOrAddresses)
        {
            var stringBuilder = new StringBuilder();
            foreach (var hostNameOrAddress in hostNameOrAddresses)
            {
                Console.WriteLine("Lookup: {0}\n", hostNameOrAddress);

                IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);
                Console.WriteLine("  Host Name: {0}", hostEntry.HostName);

                IPAddress[] ips = hostEntry.AddressList;
                if (hostNameOrAddress.Equals("g.cn"))
                {
                    var ipcn = "google_cn = " + string.Join("|", ips.Select(i => i.ToString()));
                    stringBuilder.AppendLine(ipcn);
                }
                else if (hostNameOrAddress.Equals("google.com.hk"))
                {
                    var iphk = "google_hk = " + string.Join("|", ips.Select(i => i.ToString()));
                    stringBuilder.AppendLine(iphk);
                }

                foreach (IPAddress ip in ips)
                {
                    Console.WriteLine("  Address: {0}", ip);
                }

                Console.WriteLine();
            }
            return stringBuilder;
        }
    }
}
