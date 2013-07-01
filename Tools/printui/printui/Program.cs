using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace printui
{
    class Program
    {
        [DllImport("printui.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern void PrintUIEntry(IntPtr hwnd, IntPtr hinst, string lpszCmdLine, int nCmdShow);
        static void Main(string[] args)
        {
            PrintUIEntry(IntPtr.Zero, IntPtr.Zero, @"/?", 0);
            //rundll32 printui.dll,PrintUIEntry /ia /m "Lanxum EMF Printer" /f "C:\Documents and Settings\Administrator\桌面\Deploy\PrintProcess\PrintProcess\CommonPrintDriver\x86\Driver\EmfDriver.inf"
            PrintUIEntry(IntPtr.Zero, IntPtr.Zero, "/ia /m \"Lanxum EMF Printer\" /f " + "\"C" + @":\Documents and Settings\Administrator\桌面\Deploy\PrintProcess\PrintProcess\CommonPrintDriver\x86\Driver\EmfDriver.inf" + "\"", 0);
            Console.ReadKey(false);
        }

    }
}
