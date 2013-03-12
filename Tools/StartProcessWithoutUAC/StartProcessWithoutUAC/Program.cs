using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace StartProcessWithoutUAC
{
    class Program
    {
        static void Main(string[] args)
        {
           // Privileges.EnablePrivilege(SecurityEntity.SE_TCB_NAME);
            var token = Domas.DAP.ADF.Cookie.CookieManger.GetCurrentUserToken();
            var error = Marshal.GetLastWin32Error();
            Console.WriteLine(error);
            Console.WriteLine(token);
            Console.ReadKey(false);
        }
        const int SE_PRIVILEGE_ENABLED = 0x00000002;
        const int TOKEN_QUERY = 0x00000008;
        const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        const string SE_TIME_ZONE_NAMETEXT = "SeTcbPrivilege"; 
        private static bool SetPriv()
        {
            try
            {
                bool retVal;
                TokPriv1Luid tp;
                IntPtr hproc = Process.GetCurrentProcess().Handle;
                IntPtr htok = IntPtr.Zero;
                retVal = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
                tp.Count = 1;
                tp.Luid = 0;
                tp.Attr = SE_PRIVILEGE_ENABLED;
                retVal = LookupPrivilegeValue(null, SE_TIME_ZONE_NAMETEXT, ref tp.Luid);
                retVal = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                return retVal;
            }
            catch (Exception ex)
            {
                throw;
                return false;
            }

        }
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr
        phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name,
        ref long pluid);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

    }
}
