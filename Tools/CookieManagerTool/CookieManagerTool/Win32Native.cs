using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace CookieManagerTool
{
    /// <summary>
    /// Provides ability to create low integrity processes
    /// </summary>
    internal class LowIntegrityProcess
    {
        private static readonly string LowIntegritySid = "S-1-16-4096";

        /// <summary>
        /// Starts a process with a low integrity level
        /// </summary>
        /// <param name="executableFileName">Path and filename of executable.</param>
        /// <param name="args">Command line arguments</param>
        /// <returns>The new running process</returns>
        /// <exception cref="T:System.ComponentModel.Win32Exception">Any errors creating the process</exception>
        public static Process Start(string executableFileName, string args)
        {
            SafeTokenHandle hToken = SafeTokenHandle.InvalidHandle;
            SafeTokenHandle newToken = SafeTokenHandle.InvalidHandle;

            // Get the current token
            if (Win32Native.OpenProcessToken(Process.GetCurrentProcess().Handle, TokenAccessLevels.MaximumAllowed,
                                             ref hToken))
            {
                using (hToken)
                {
                    // Make a copy
                    if (Win32Native.DuplicateTokenEx(hToken, TokenAccessLevels.MaximumAllowed, IntPtr.Zero,
                                                     Win32Native.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                                                     Win32Native.TokenType.TokenPrimary,
                                                     ref newToken))
                    {
                        using (newToken)
                        {
                            // Get the Low Integrity SID as a PSID
                            SafeLocalMemHandle sid;
                            if (Win32Native.ConvertStringSidToSid(LowIntegritySid, out sid))
                            {
                                using (sid)
                                {
                                    // Create the low integrity label
                                    Win32Native.TOKEN_MANDATORY_LABEL til = new Win32Native.TOKEN_MANDATORY_LABEL();
                                    til.Label.Attributes = Win32Native.SE_GROUP_INTEGRITY;
                                    til.Label.Sid = sid;

                                    int sidLen = Win32Native.GetLengthSid(sid);
                                    int size = Marshal.SizeOf(til);

                                    // Set low integrity label on the token
                                    if (Win32Native.SetTokenInformation(newToken,
                                                                        Win32Native.TOKEN_INFORMATION_CLASS.
                                                                            TokenIntegrityLevel,
                                                                        til, size + sidLen))
                                    {
                                        Win32Native.STARTUPINFO startup = new Win32Native.STARTUPINFO();
                                        Win32Native.PROCESS_INFORMATION info = new Win32Native.PROCESS_INFORMATION();


                                        string cmdLine = BuildCommandLine(executableFileName, args);

                                        // Spawn the process
                                        if (Win32Native.CreateProcessAsUser(newToken, null, cmdLine, null, null, false,
                                                                            0,
                                                                            IntPtr.Zero,
                                                                            null, startup, info))
                                        {
                                            return Process.GetProcessById(info.dwProcessId);
                                        }
                                    }
                                } // free sid
                            }
                        } // using newToken
                    }
                } // using hToken
            }

            // If we get here, then one of the Win32 API's failed -- get the error and throw
            int error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }

        private static string BuildCommandLine(string executableFileName, string arguments)
        {
            StringBuilder builder = new StringBuilder();
            string str = executableFileName.Trim();
            bool flag = str.StartsWith("\"", StringComparison.Ordinal) && str.EndsWith("\"", StringComparison.Ordinal);
            if (!flag)
            {
                builder.Append("\"");
            }
            builder.Append(str);
            if (!flag)
            {
                builder.Append("\"");
            }
            if (!string.IsNullOrEmpty(arguments))
            {
                builder.Append(" ");
                builder.Append(arguments);
            }
            return builder.ToString();
        }
    }

    #region Win32 interop

    #region SafeHandles

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    internal sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeTokenHandle()
            : base(true)
        {
        }

        internal SafeTokenHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        internal static SafeTokenHandle InvalidHandle
        {
            get
            {
                return new SafeTokenHandle(IntPtr.Zero);
            }
        }

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success),
         DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
    }

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    internal sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Methods
        internal SafeLocalMemHandle()
            : base(true)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(existingHandle);
        }


        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        protected override bool ReleaseHandle()
        {
            return (LocalFree(handle) == IntPtr.Zero);
        }
    }

    #endregion

    [SuppressUnmanagedCodeSecurity]
    internal static class Win32Native
    {
        /*
         * A note about the structures:
         * 
         * Some of the structures are implemented as classes to provide a default constructor.  
         * The structures implemented this way are expected by reference, so it works.
         * */

        #region Methods

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail),
         DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DuplicateTokenEx(SafeTokenHandle ExistingTokenHandle,
                                                     TokenAccessLevels DesiredAccess, IntPtr TokenAttributes,
                                                     SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
                                                     TokenType TokenType, ref SafeTokenHandle DuplicateTokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr ProcessToken, TokenAccessLevels DesiredAccess,
                                                     ref SafeTokenHandle TokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool CreateProcessAsUser(SafeTokenHandle hToken, string lpApplicationName,
                                                      string lpCommandLine, SECURITY_ATTRIBUTES lpProcessAttributes,
                                                      SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles,
                                                      int dwCreationFlags, IntPtr lpEnvironment,
                                                      string lpCurrentDirectory, STARTUPINFO lpStartupInfo,
                                                      PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetTokenInformation(SafeTokenHandle hToken, TOKEN_INFORMATION_CLASS informationClass,
                                                        TOKEN_MANDATORY_LABEL tokenInformation,
                                                        int tokenInformationLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetLengthSid(SafeLocalMemHandle sid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ConvertStringSidToSid(string stringSid, out SafeLocalMemHandle ByteArray);

        [DllImport("ieframe.dll", CharSet = CharSet.Auto)]
        internal static extern int IEIsProtectedModeURL(string url);

        #endregion

        #region Enums

        internal const int SE_GROUP_INTEGRITY = 0x00000020;

        internal enum TokenType
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        internal enum TOKEN_INFORMATION_CLASS
        {
            /// <summary>
            /// The buffer receives a TOKEN_USER structure that contains the user account of the token.
            /// </summary>
            TokenUser = 1,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS structure that contains the group accounts associated with the token.
            /// </summary>
            TokenGroups,

            /// <summary>
            /// The buffer receives a TOKEN_PRIVILEGES structure that contains the privileges of the token.
            /// </summary>
            TokenPrivileges,

            /// <summary>
            /// The buffer receives a TOKEN_OWNER structure that contains the default owner security identifier (SID) for newly created objects.
            /// </summary>
            TokenOwner,

            /// <summary>
            /// The buffer receives a TOKEN_PRIMARY_GROUP structure that contains the default primary group SID for newly created objects.
            /// </summary>
            TokenPrimaryGroup,

            /// <summary>
            /// The buffer receives a TOKEN_DEFAULT_DACL structure that contains the default DACL for newly created objects.
            /// </summary>
            TokenDefaultDacl,

            /// <summary>
            /// The buffer receives a TOKEN_SOURCE structure that contains the source of the token. TOKEN_QUERY_SOURCE access is needed to retrieve this information.
            /// </summary>
            TokenSource,

            /// <summary>
            /// The buffer receives a TOKEN_TYPE value that indicates whether the token is a primary or impersonation token.
            /// </summary>
            TokenType,

            /// <summary>
            /// The buffer receives a SECURITY_IMPERSONATION_LEVEL value that indicates the impersonation level of the token. If the access token is not an impersonation token, the function fails.
            /// </summary>
            TokenImpersonationLevel,

            /// <summary>
            /// The buffer receives a TOKEN_STATISTICS structure that contains various token statistics.
            /// </summary>
            TokenStatistics,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS structure that contains the list of restricting SIDs in a restricted token.
            /// </summary>
            TokenRestrictedSids,

            /// <summary>
            /// The buffer receives a DWORD value that indicates the Terminal Services session identifier that is associated with the token. 
            /// </summary>
            TokenSessionId,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS_AND_PRIVILEGES structure that contains the user SID, the group accounts, the restricted SIDs, and the authentication ID associated with the token.
            /// </summary>
            TokenGroupsAndPrivileges,

            /// <summary>
            /// Reserved.
            /// </summary>
            TokenSessionReference,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token includes the SANDBOX_INERT flag.
            /// </summary>
            TokenSandBoxInert,

            /// <summary>
            /// Reserved.
            /// </summary>
            TokenAuditPolicy,

            /// <summary>
            /// The buffer receives a TOKEN_ORIGIN value. 
            /// </summary>
            TokenOrigin,

            /// <summary>
            /// The buffer receives a TOKEN_ELEVATION_TYPE value that specifies the elevation level of the token.
            /// </summary>
            TokenElevationType,

            /// <summary>
            /// The buffer receives a TOKEN_LINKED_TOKEN structure that contains a handle to another token that is linked to this token.
            /// </summary>
            TokenLinkedToken,

            /// <summary>
            /// The buffer receives a TOKEN_ELEVATION structure that specifies whether the token is elevated.
            /// </summary>
            TokenElevation,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token has ever been filtered.
            /// </summary>
            TokenHasRestrictions,

            /// <summary>
            /// The buffer receives a TOKEN_ACCESS_INFORMATION structure that specifies security information contained in the token.
            /// </summary>
            TokenAccessInformation,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if virtualization is allowed for the token.
            /// </summary>
            TokenVirtualizationAllowed,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if virtualization is enabled for the token.
            /// </summary>
            TokenVirtualizationEnabled,

            /// <summary>
            /// The buffer receives a TOKEN_MANDATORY_LABEL structure that specifies the token's integrity level. 
            /// </summary>
            TokenIntegrityLevel,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token has the UIAccess flag set.
            /// </summary>
            TokenUIAccess,

            /// <summary>
            /// The buffer receives a TOKEN_MANDATORY_POLICY structure that specifies the token's mandatory integrity policy.
            /// </summary>
            TokenMandatoryPolicy,

            /// <summary>
            /// The buffer receives the token's logon security identifier (SID).
            /// </summary>
            TokenLogonSid,

            /// <summary>
            /// The maximum value for this enumeration
            /// </summary>
            MaxTokenInfoClass
        }

        internal enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        internal class PROCESS_INFORMATION
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SID_AND_ATTRIBUTES
        {
            public SafeLocalMemHandle Sid;
            public int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            public int nLength;
            public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
            public bool bInheritHandle;

            public SECURITY_ATTRIBUTES()
            {
                nLength = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class TOKEN_MANDATORY_LABEL
        {
            public SID_AND_ATTRIBUTES Label;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class STARTUPINFO
        {
            public int cb;
            public IntPtr lpReserved = IntPtr.Zero;
            public IntPtr lpDesktop = IntPtr.Zero;
            public IntPtr lpTitle = IntPtr.Zero;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2 = IntPtr.Zero;
            public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);

            public STARTUPINFO()
            {
                cb = Marshal.SizeOf(this);
            }
        }

        #endregion
    }

    #endregion
}
