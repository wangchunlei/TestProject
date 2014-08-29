using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ComponentModel;
using Domas.DAP.ADF.LogManager;
using Domas.DAP.ADF.Cookie;

namespace Domas.DAP.ADF.AutoUpdate
{
    public class Interop
    {
        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        public static void ShowMessageBox(string message, string title)
        {
            int resp = 0;
            WTSSendMessage(
               WTS_CURRENT_SERVER_HANDLE,
                WTSGetActiveConsoleSessionId(),
                title, title.Length,
                message, message.Length,
               0, 0, out resp, false);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSSendMessage(
            IntPtr hServer,
            int SessionId,
            String pTitle,
            int TitleLength,
            String pMessage,
            int MessageLength,
            int Style,
            int Timeout,
            out int pResponse,
            bool bWait);
        private static ILogger logger = null;
        public static void CreateProcess(string app, string parm, string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = System.IO.Path.GetDirectoryName(app);
            }
            logger = LogManager.LogManager.GetLogger("CreateProcess");
            bool result;
            IntPtr hToken = WindowsIdentity.GetCurrent().Token;
            IntPtr hDupedToken = IntPtr.Zero;

            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.Length = Marshal.SizeOf(sa);

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);

            hToken = CookieManger.GetCurrentUserToken();
            //int dwSessionID = WTSGetActiveConsoleSessionId();
            //result = WTSQueryUserToken(dwSessionID, out hToken);

            //if (!result)
            //{
            //    ShowMessageBox("WTSQueryUserToken failed", "AlertService Message");
            //}
            logger.Debug("hToken:" + hToken.ToString());

            result = DuplicateTokenEx(
                  hToken,
                  GENERIC_ALL_ACCESS,
                  ref sa,
                 (int)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                  (int)TOKEN_TYPE.TokenPrimary,
                  ref hDupedToken
              );

            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                logger.Error("DuplicateTokenEx:" + errorMessage);
                //ShowMessageBox("DuplicateTokenEx failed", "AlertService Message");
            }

            IntPtr lpEnvironment = IntPtr.Zero;
            result = CreateEnvironmentBlock(out lpEnvironment, hDupedToken, false);

            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                logger.Error("CreateEnvironmentBlock:" + errorMessage);
                //ShowMessageBox("CreateEnvironmentBlock failed", "AlertService Message");
            }
            string lpCommandline = string.Empty;
            if (!string.IsNullOrEmpty(parm))
                lpCommandline = parm;
            result = CreateProcessAsUser(
                                 hDupedToken,
                                 app,//file to execute
                                 lpCommandline,//command line
                                 ref sa, ref sa,
                                 false, 0, IntPtr.Zero,
                                 path, ref si, ref pi);

            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                logger.Error("CreateProcessAsUser:" + errorMessage);
                string message = String.Format("CreateProcessAsUser Error: {0}", error);
                //ShowMessageBox(message, "AlertService Message");
            }

            if (pi.hProcess != IntPtr.Zero)
                CloseHandle(pi.hProcess);
            if (pi.hThread != IntPtr.Zero)
                CloseHandle(pi.hThread);
            if (hDupedToken != IntPtr.Zero)
                CloseHandle(hDupedToken);
        }

        public static void CreateProcess(string app, string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = System.IO.Path.GetDirectoryName(app);
            }
            logger = LogManager.LogManager.GetLogger("CreateProcess");
            bool result;
            IntPtr hToken = WindowsIdentity.GetCurrent().Token;
            IntPtr hDupedToken = IntPtr.Zero;

            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.Length = Marshal.SizeOf(sa);
            
            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            
            int dwSessionID = WTSGetActiveConsoleSessionId();
            result = WTSQueryUserToken(dwSessionID, out hToken);

            if (!result)
            {
                ShowMessageBox("WTSQueryUserToken failed", "AlertService Message");
            }

            result = DuplicateTokenEx(
                  hToken,
                  GENERIC_ALL_ACCESS,
                  ref sa,
                 (int)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                  (int)TOKEN_TYPE.TokenPrimary,
                  ref hDupedToken
              );

            if (!result)
            {
                ShowMessageBox("DuplicateTokenEx failed", "AlertService Message");
            }

            IntPtr lpEnvironment = IntPtr.Zero;
            result = CreateEnvironmentBlock(out lpEnvironment, hDupedToken, false);

            if (!result)
            {
                ShowMessageBox("CreateEnvironmentBlock failed", "AlertService Message");
            }
            string lpCommandline = string.Empty;

            result = CreateProcessAsUser(
                       hDupedToken,
                        app,
                        String.Empty,
                        ref sa, ref sa,
                        false, 0, IntPtr.Zero,
                        path, ref si, ref pi);


            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                logger.Error("CreateProcessAsUser:" + errorMessage);
                string message = String.Format("CreateProcessAsUser Error: {0}", error);
                ShowMessageBox(message, "AlertService Message");
            }

            if (pi.hProcess != IntPtr.Zero)
                CloseHandle(pi.hProcess);
            if (pi.hThread != IntPtr.Zero)
                CloseHandle(pi.hThread);
            if (hDupedToken != IntPtr.Zero)
                CloseHandle(hDupedToken);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public Int32 dwProcessID;
            public Int32 dwThreadID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public Int32 Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        public const int GENERIC_ALL_ACCESS = 0x10000000;

        [DllImport("kernel32.dll", SetLastError = true,
            CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", SetLastError = true,
            CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandle,
            Int32 dwCreationFlags,
            IntPtr lpEnvrionment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            Int32 dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            Int32 ImpersonationLevel,
            Int32 dwTokenType,
            ref IntPtr phNewToken);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSQueryUserToken(
            Int32 sessionId,
           out IntPtr Token);

        [DllImport("userenv.dll", SetLastError = true)]
        static extern bool CreateEnvironmentBlock(
            out IntPtr lpEnvironment,
            IntPtr hToken,
            bool bInherit);

    }
}
