using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;

namespace ImpersonForIIS.Models
{
    public class SecurityManager
    {
        #region DLL Imports
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);
        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
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
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CreateProcessAsUser(IntPtr hToken, String lpApplicationName, String lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandle, int dwCreationFlags, IntPtr lpEnvironment,
            String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        public extern static bool DuplicateTokenEx(IntPtr ExistingTokenHandle, uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, int TokenType,
            int ImpersonationLevel, ref IntPtr DuplicateTokenHandle);
        #endregion

        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        //private WindowsImpersonationContext m_CurrentImpersonationContext;
        public string CreateProcessAsUser(string appPath, bool currentUser = false)
        {
            IntPtr Token = new IntPtr(0);
            IntPtr DupedToken = new IntPtr(0);
            bool ret;
            string text = WindowsIdentity.GetCurrent().Name.ToString();

            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.bInheritHandle = false;
            sa.Length = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = (IntPtr)0;

            //Token = WindowsIdentity.GetCurrent().Token;

            Token = Domas.DAP.ADF.Cookie.CookieManger.GetCurrentUserToken();
            using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(Token))
            {
                text += "<br/>" + WindowsIdentity.GetCurrent().Name + "<br/>";
            }
            const uint GENERIC_ALL = 0x10000000;

            const int SecurityImpersonation = 2;
            const int TokenType = 1;

            ret = DuplicateTokenEx(Token, GENERIC_ALL, ref sa, SecurityImpersonation, TokenType, ref DupedToken);

            if (ret == false)
                text += "DuplicateTokenEx failed with " + Marshal.GetLastWin32Error();

            else
                text += "DuplicateTokenEx SUCCESS";

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = "";

            string commandLinePath;
            commandLinePath = appPath;

            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            ret = CreateProcessAsUser(DupedToken, null, commandLinePath, ref sa, ref sa, false, 0, (IntPtr)0, "c:\\", ref si, out pi);

            if (ret == false)
                text += "CreateProcessAsUser failed with " + Marshal.GetLastWin32Error();
            else
            {
                text += "CreateProcessAsUser SUCCESS.  The child PID is" + pi.dwProcessId;

                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);
            }

            ret = CloseHandle(DupedToken);
            if (ret == false)
                text += Marshal.GetLastWin32Error();
            else
                text += "CloseHandle SUCCESS";
            return text;
        }
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void Impersonation(Action func)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_INTERACTIVE = 2;

            IntPtr tokenHandle = IntPtr.Zero;
            IntPtr dupeTokenHandle = IntPtr.Zero;

            // obtain a handle to an access token
            bool wasLogonSuccessful = LogonUser(UserName, Domain, Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle);

            if (!wasLogonSuccessful)
                throw new Exception(String.Format("Logon failed with error number {0}", Marshal.GetLastWin32Error()));

            // use the token handle to impersonate the user
            WindowsIdentity newId = new WindowsIdentity(tokenHandle);
            using (var m_CurrentImpersonationContext = newId.Impersonate())
            {
                func();
            }

            // free the tokens
            if (tokenHandle != IntPtr.Zero)
                CloseHandle(tokenHandle);
        }
        public IntPtr GetUserToken()
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_INTERACTIVE = 2;
            IntPtr tokenHandle = IntPtr.Zero;
            bool wasLogonSuccessful = LogonUser(UserName, Domain, Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle);
            if (!wasLogonSuccessful)
                throw new Exception(String.Format("Logon failed with error number {0}", Marshal.GetLastWin32Error()));
            return tokenHandle;
        }
        //public void EndImpersonation()
        //{
        //    m_CurrentImpersonationContext.Undo();
        //}
    }
}