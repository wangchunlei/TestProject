using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;

namespace Domas.DAP.ADF.Cookie
{
    public class CookieManger
    {
        #region Import Section
        private static uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private static uint STANDARD_RIGHTS_READ = 0x00020000;
        private static uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        private static uint TOKEN_DUPLICATE = 0x0002;
        private static uint TOKEN_IMPERSONATE = 0x0004;
        private static uint TOKEN_QUERY = 0x0008;
        private static uint TOKEN_QUERY_SOURCE = 0x0010;
        private static uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private static uint TOKEN_ADJUST_GROUPS = 0x0040;
        private static uint TOKEN_ADJUST_DEFAULT = 0x0080;
        private static uint TOKEN_ADJUST_SESSIONID = 0x0100;
        private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        private static uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID);

        private const uint NORMAL_PRIORITY_CLASS = 0x0020;

        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;


        private const uint MAX_PATH = 260;

        private const uint CREATE_NO_WINDOW = 0x08000000;

        private const uint INFINITE = 0xFFFFFFFF;

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
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

        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
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
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public Int32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool WTSQueryUserToken(int sessionId, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateTokenEx(IntPtr existingToken, uint desiredAccess, IntPtr tokenAttributes, SECURITY_IMPERSONATION_LEVEL impersonationLevel, TOKEN_TYPE tokenType, out IntPtr newToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CreateProcessAsUser(IntPtr token, string applicationName, string commandLine, ref SECURITY_ATTRIBUTES processAttributes, ref SECURITY_ATTRIBUTES threadAttributes, bool inheritHandles, uint creationFlags, IntPtr environment, string currentDirectory, ref STARTUPINFO startupInfo, out PROCESS_INFORMATION processInformation);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetLastError();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int WaitForSingleObject(IntPtr token, uint timeInterval);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int WTSEnumerateSessions(System.IntPtr hServer, int Reserved, int Version, ref System.IntPtr ppSessionInfo, ref int pCount);

        [DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [DllImport("wtsapi32.dll", ExactSpelling = true, SetLastError = false)]
        public static extern void WTSFreeMemory(IntPtr memory);

        [DllImport("userenv.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);
        [DllImport("wininet.dll", SetLastError = true)]
        protected static extern bool InternetGetCookie(string url, string cookieName, StringBuilder cookieData, ref int size);
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool WTSQueryUserToken(uint sessionId, out IntPtr tokenHandle);
        [DllImport("ieframe.dll", CharSet = CharSet.Auto)]
        internal static extern int IEIsProtectedModeURL(string url);
        [DllImport("ieframe.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int IEGetProtectedModeCookie(String url, String cookieName, StringBuilder cookieData, ref int size, uint flag);
        #endregion

        public static IntPtr GetCurrentUserToken()
        {
            IntPtr currentToken = IntPtr.Zero;
            IntPtr primaryToken = IntPtr.Zero;
            IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

            int dwSessionId = 0;
            IntPtr hUserToken = IntPtr.Zero;
            IntPtr hTokenDup = IntPtr.Zero;

            IntPtr pSessionInfo = IntPtr.Zero;
            int dwCount = 0;

            WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo, ref dwCount);

            Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));

            Int64 current = (Int64)pSessionInfo;
            for (int i = 0; i < dwCount; i++)
            {
                WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)current, typeof(WTS_SESSION_INFO));
                if (WTS_CONNECTSTATE_CLASS.WTSActive == si.State)
                {
                    dwSessionId = si.SessionID;
                    break;
                }

                current += dataSize;
            }

            WTSFreeMemory(pSessionInfo);
            bool bRet = WTSQueryUserToken(dwSessionId, out currentToken);
            if (bRet == false)
            {
                currentToken = WindowsIdentity.GetCurrent().Token;
            }

            bRet = DuplicateTokenEx(currentToken, TOKEN_ASSIGN_PRIMARY | TOKEN_ALL_ACCESS, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out primaryToken);
            if (bRet == false)
            {
                return IntPtr.Zero;
            }

            return primaryToken;
        }
        public static void Impersonate(Action method)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && windowsIdentity.IsSystem)
            {
                System.IntPtr currentToken = GetCurrentUserToken();
                if (currentToken != windowsIdentity.Token)
                {
                    using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(currentToken))
                    {
                        //logger.Debug(Environment.UserName);
                        method();
                        return;
                    }
                }
            }
            method();
        }
        public static T Impersonate<T>(Func<T> method)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && windowsIdentity.IsSystem)
            {
                System.IntPtr currentToken = GetCurrentUserToken();
                if (currentToken != windowsIdentity.Token)
                {
                    using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(currentToken))
                    {
                        //logger.Debug(Environment.UserName);
                        return method();
                    }
                }

            }

            return method();
        }
        //public static CookieContainer GetUriCookieContainer(Uri uri, string cookieName = "userinfo")
        //{
        //    var cookies = new CookieContainer();
        //    var context = Domas.DAP.ADF.Context.ContextFactory.GetCurrentContext();
        //    if (context != null)
        //    {
        //        var pcToken = context.GetObjectFromApplication("PCToken");
        //        if (pcToken != null)
        //        {
        //            cookies.Add(uri, new System.Net.Cookie("PCToken", pcToken.ToString()));
        //        }
        //        var userToken = context.GetObjectFromApplication("UserToken");
        //        if (userToken != null)
        //        {
        //            cookies.Add(uri, new System.Net.Cookie("UserToken", userToken.ToString()));
        //        }
        //        var clientType = context.GetObjectFromApplication("ClientType");
        //        if (clientType != null)
        //        {
        //            cookies.Add(uri, new System.Net.Cookie("ClientType", clientType.ToString()));
        //        }
        //        var thirdKey = context.GetObjectFromApplication("ThirdKey");
        //        if (thirdKey != null)
        //        {
        //            const string publicKey = @"<RSAKeyValue><Modulus>qZVoofKGP6O/RI4tBxNuhkN3CjQ/koT4Qp7EEijtJP9VjfU6ADcfEQG7hdP0sHySFdHz0OMQtV7L/mgRrFyjkppZtDQ9oIB3MzR3Qws83ye6YeCYYD0YtHVVp7SO1CMPNCPVdrPxuG6d6+UCxNaKZ0NeXMxqk/BJcOuu4t47Krs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        //            cookies.Add(uri, new System.Net.Cookie("ThirdKey", Domas.DAP.ADF.Security.Encryption.StringEncryptionRSA(thirdKey.ToString(), publicKey)));
        //        }
        //    }

        //    return cookies;
        //    //定义Cookie数据的大小。
        //    int datasize = 256;
        //    StringBuilder cookieData = new StringBuilder(datasize);
        //    if (!InternetGetCookie(uri.ToString(), cookieName, cookieData, ref datasize))
        //    {
        //        if (datasize < 0) return null;
        //        // 确信有足够大的空间来容纳Cookie数据。
        //        cookieData = new StringBuilder(datasize);
        //        if (!InternetGetCookie(uri.ToString(), cookieName, cookieData, ref datasize))
        //        {
        //            if (Environment.OSVersion.Version.Major >= 6)
        //            {
        //                var isPMUrl = IEIsProtectedModeURL(uri.AbsoluteUri);
        //                if (isPMUrl == 0)
        //                {
        //                    var hResult = IEGetProtectedModeCookie(uri.AbsoluteUri, cookieName, cookieData, ref datasize, 0);
        //                    if (0 != hResult)
        //                    {
        //                        cookieData.EnsureCapacity(datasize);
        //                        IEGetProtectedModeCookie(uri.AbsoluteUri, cookieName, cookieData, ref datasize, 0);
        //                    }
        //                }
        //            }

        //        }
        //    }
        //    if (cookieData.Length > 0)
        //    {
        //        cookies = new CookieContainer();
        //        cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
        //    }
        //    if (cookies == null)
        //    {
        //        Impersonate(() =>
        //        {
        //            string cookieValue = string.Empty;
        //            var strPath = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
        //            //if (IEIsProtectedModeURL(uri.AbsoluteUri) == 0)
        //            //{
        //            //    logger.Debug("look up in low");
        //            //    strPath = Path.Combine(strPath, "low");
        //            //}

        //            GetCookie_InternetExplorer(uri.Host, cookieName, ref cookieValue, strPath);
        //            if (!string.IsNullOrEmpty(cookieValue))
        //            {
        //                cookies = new CookieContainer();
        //                cookies.Add(uri, new System.Net.Cookie(cookieName, cookieValue));
        //            }

        //            if (cookies == null)
        //            {
        //                if (Environment.OSVersion.Version.Major >= 6)
        //                {
        //                    if (IEIsProtectedModeURL(uri.AbsoluteUri) == 0)
        //                    {
        //                        //logger.Debug("look up in low");
        //                        strPath = Path.Combine(strPath, "low");
        //                    }
        //                }


        //                GetCookie_InternetExplorer(uri.Host, cookieName, ref cookieValue, strPath);
        //                if (!string.IsNullOrEmpty(cookieValue))
        //                {
        //                    cookies = new CookieContainer();
        //                    cookies.Add(uri, new System.Net.Cookie(cookieName, cookieValue));
        //                }
        //            }
        //        });
        //    }
        //    return cookies;
        //}
        private static bool GetCookie_InternetExplorer(string strHost, string strField, ref string Value, string strPath)
        {
            Value = string.Empty;
            bool fRtn = false;
            string strCookie;
            string[] fp;
            StreamReader r;
            int idx;

            try
            {
                strField = strField + "\n";
                ;
                //Version v = Environment.OSVersion.Version;

                fp = Directory.GetFiles(strPath, "*.txt");

                foreach (string path in fp)
                {
                    idx = -1;
                    r = File.OpenText(path);
                    strCookie = r.ReadToEnd();
                    r.Close();

                    if (System.Text.RegularExpressions.Regex.IsMatch(strCookie, strHost))
                    {
                        idx = strCookie.ToUpper().IndexOf(strField.ToUpper());
                    }

                    if (-1 < idx)
                    {
                        idx += strField.Length;
                        Value = strCookie.Substring(idx, strCookie.IndexOf('\n', idx) - idx);
                        if (!Value.Equals(string.Empty))
                        {
                            fRtn = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception) //File not found, etc...
            {
                Value = string.Empty;
                fRtn = false;
            }

            return fRtn;
        }
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern bool InternetSetCookie(string url, string name, string cookieData);

        public static bool SetInternetCookie(string baseUrl, System.Net.Cookie cookie)
        {
            var cookieName = cookie.Name;
            var cookieValue = string.Format("{0}; expires = {1}", cookie.Value, cookie.Expires.ToString("R"));
            var success = InternetSetCookie(baseUrl, cookieName, cookieValue);
            return success;
        }
        public static bool DelInternetCookie(string baseUrl, string cookieName)
        {
            var cookieValue = string.Format("{0}; expires = {1}", string.Empty, DateTime.Now.AddDays(-1).ToString("R"));
            var success = InternetSetCookie(baseUrl, cookieName, cookieValue);
            return success;
        }
        //#if !CLIENT_PROFILE //迁移至ProxyBase
        //        /// <summary>
        //        /// 创建"UserInfo"Cookie,并将cookie各值写入Context
        //        /// </summary>
        //        /// <param name="encryptionStr">cookie明码串</param>
        //        /// <returns></returns>
        //        public static HttpCookie CreateUserInfoCookie(string encryptionStr)
        //        {
        //            string cookieValue = Encryption.StringEncryptionAES(encryptionStr, "lanxum");
        //            HttpCookie userInfoCookie = new HttpCookie("UserInfo", cookieValue);
        //            userInfoCookie.HttpOnly = false;
        //            userInfoCookie.Expires = System.DateTime.Now.AddDays(360);

        //            string[] userInfo = encryptionStr.Split(',');
        //            if (userInfo.Length == 6 && HttpContext.Current.Session != null)
        //            {
        //                HttpContext.Current.Session["UserID"] = userInfo[0].Split('=').Length == 2 ? userInfo[0].Split('=')[1] : "";
        //                HttpContext.Current.Session["UserName"] = userInfo[1].Split('=').Length == 2 ? userInfo[1].Split('=')[1] : "";
        //                HttpContext.Current.Session["CardNo"] = userInfo[2].Split('=').Length == 2 ? userInfo[2].Split('=')[1] : "";
        //                HttpContext.Current.Session["LoginID"] = userInfo[3].Split('=').Length == 2 ? userInfo[3].Split('=')[1] : "";
        //                HttpContext.Current.Session["UserCode"] = userInfo[5].Split('=').Length == 2 ? userInfo[5].Split('=')[1] : "";
        //            }

        //            return userInfoCookie;
        //        }
        //#endif
        /// <summary>
        /// 解密cookie字符串，返回用户信息
        /// </summary>
        /// <param name="cookieStr">cookie加密串</param>
        /// <returns></returns>
        //public static UserInfoDTO DecryCookie(string cookieStr)
        //{
        //    UserInfoDTO userInfoDTO = new UserInfoDTO();
        //    string decryptionStr = Decryption.StringDecryptionAES(cookieStr, "lanxum");//cookie明码串
        //    string[] userInfo = decryptionStr.Split(',');
        //    foreach (var s in userInfo)
        //    {
        //        string[] detail = s.Split('=');
        //        if (detail.Length == 2)
        //        {
        //            switch (detail[0])
        //            {
        //                case "UserID":
        //                    userInfoDTO.UserID = detail[1];
        //                    break;
        //                case "UserName":
        //                    userInfoDTO.UserName = detail[1];
        //                    break;
        //                case "CardNo":
        //                    userInfoDTO.CardNo = detail[1];
        //                    break;
        //                case "LoginID":
        //                    userInfoDTO.LoginID = detail[1];
        //                    break;
        //                case "RememberUserID":
        //                    userInfoDTO.RememberUserID = bool.Parse(detail[1].ToString());
        //                    break;
        //                case "UserCode":
        //                    userInfoDTO.UserCode = detail[1];
        //                    break;
        //                case "IpAddress":
        //                    userInfoDTO.IpAddress = detail[1];
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //    }
        //    return userInfoDTO;
        //}
        //#if !CLIENT_PROFILE //迁移至ProxyBase
        //        public static HttpCookie CreateAuthInfoCookie(string encryptionStr)
        //        {
        //            string cookieValue = Encryption.StringEncryptionAES(encryptionStr, "lanxum");
        //            HttpCookie authInfoCookie = new HttpCookie("AuthInfo", cookieValue);
        //            authInfoCookie.HttpOnly = false;
        //            authInfoCookie.Expires = System.DateTime.Now.AddDays(360);
        //            return authInfoCookie;
        //        }
        //#endif
        //public static AuthInfoDTO DecryAuthCookie(string cookieStr)
        //{
        //    AuthInfoDTO authInfoDTO = new AuthInfoDTO();
        //    string decryptionStr = Decryption.StringDecryptionAES(cookieStr, "lanxum");//cookie明码串
        //    string[] authInfo = decryptionStr.Split(',');
        //    foreach (var s in authInfo)
        //    {
        //        string[] detail = s.Split('=');
        //        if (detail.Length == 2)
        //        {
        //            switch (detail[0])
        //            {
        //                case "HostName":
        //                    authInfoDTO.HostName = detail[1];
        //                    break;
        //                case "Sn":
        //                    authInfoDTO.Sn = detail[1];
        //                    break;
        //                case "Type":
        //                    authInfoDTO.Type = int.Parse(detail[1]);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //    }
        //    return authInfoDTO;
        //}
    }
    public class UserInfoDTO
    {
        /// <summary>
        /// 
        /// </summary>
        //[System.Runtime.Serialization.DataMember]
        public string UserID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //[System.Runtime.Serialization.DataMember]
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //[System.Runtime.Serialization.DataMember]
        public string CardNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //[System.Runtime.Serialization.DataMember]
        public bool RememberUserID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //[System.Runtime.Serialization.DataMember]
        public string UserCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //[System.Runtime.Serialization.DataMember]
        public string LoginID { get; set; }
        public string IpAddress { get; set; }
        public string DepartmentID
        {
            get;
            set;
        }
        public string DepartmentCode
        {
            get;
            set;
        }
        public string DepartmentName
        {
            get;
            set;
        }
    }
    public class AuthInfoDTO
    {
        /// <summary>
        /// 机器名
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// 关键Key
        /// </summary>
        public string Sn { get; set; }
        /// <summary>
        /// 终端类型
        /// </summary>
        public int Type { get; set; }
    }

}
