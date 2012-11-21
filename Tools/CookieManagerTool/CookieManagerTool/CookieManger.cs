using System;
using System.ComponentModel;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using Domas.DAP.ADF.LogManager;
using Domas.DAP.ADF.Security;

namespace CookieManagerTool
{
    public class CookieManger
    {
        [DllImport("wininet.dll", SetLastError = true)]
        protected static extern bool InternetGetCookie(string url, string cookieName, StringBuilder cookieData, ref int size);
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool WTSQueryUserToken(uint sessionId, out IntPtr tokenHandle);
        [DllImport("ieframe.dll", CharSet = CharSet.Auto)]
        internal static extern int IEIsProtectedModeURL(string url);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint WTSGetActiveConsoleSessionId();
        [DllImport("ieframe.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int IEGetProtectedModeCookie(String url, String cookieName, StringBuilder cookieData, ref int size, uint flag);

        static ILogger logger = Domas.DAP.ADF.LogManager.LogManager.GetLogger("GetCookie");
        public static void Impersonate(Action method)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && windowsIdentity.IsSystem)
            {
                uint sessionID = WTSGetActiveConsoleSessionId();
                if (sessionID == 3)
                {
                    sessionID = 0xFFFFFFFF;
                }
                System.IntPtr currentToken = IntPtr.Zero;
                bool bRet = WTSQueryUserToken(sessionID, out currentToken);

                using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(currentToken))
                {
                    method();
                }
            }
            else
            {
                method();
            }
        }
        public static T Impersonate<T>(Func<T> method)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && windowsIdentity.IsSystem)
            {
                uint sessionID = WTSGetActiveConsoleSessionId();
                if (sessionID == 3)
                {
                    sessionID = 0xFFFFFFFF;
                }
                System.IntPtr currentToken = IntPtr.Zero;
                bool bRet = WTSQueryUserToken(sessionID, out currentToken);
                using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(currentToken))
                {
                    return method();
                }
            }

            return method();
        }
        public static CookieContainer GetUriCookieContainer(Uri uri, string cookieName = "userinfo")
        {
            CookieContainer cookies = null;

            //定义Cookie数据的大小。
            int datasize = 256;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookie(uri.ToString(), null, cookieData, ref datasize))
            {
                if (datasize < 0) return null;
                // 确信有足够大的空间来容纳Cookie数据。
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookie(uri.ToString(), null, cookieData, ref datasize))
                {
                    var isPMUrl = IEIsProtectedModeURL(uri.AbsoluteUri);
                    if (isPMUrl == 0)
                    {
                        var hResult = IEGetProtectedModeCookie(uri.AbsoluteUri, null, cookieData, ref datasize, 0);
                        if (0 != hResult)
                        {
                            cookieData.EnsureCapacity(datasize);
                            IEGetProtectedModeCookie(uri.AbsoluteUri, null, cookieData, ref datasize, 0);
                        }
                    }
                }
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            if (cookies == null)
            {
                Impersonate(() =>
                {
                    string cookieValue = string.Empty;
                    var strPath = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
                    if (IEIsProtectedModeURL(uri.AbsoluteUri) == 0)
                    {
                        logger.Debug("look up in low");
                        Path.Combine(strPath, "low");
                    }

                    GetCookie_InternetExplorer(uri.Host, cookieName, ref cookieValue, strPath);
                    if (!string.IsNullOrEmpty(cookieValue))
                    {
                        cookies = new CookieContainer();
                        cookies.Add(uri, new System.Net.Cookie(cookieName, cookieValue));
                    }
                });
            }
            return cookies;
        }
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

        public static bool SetInternetCookie(string baseUrl, string cookieName, string cookieValue)
        {
            var success = InternetSetCookie(baseUrl, cookieName, cookieValue);
            return success;
        }

        /// <summary>
        /// 创建"UserInfo"Cookie,并将cookie各值写入Context
        /// </summary>
        /// <param name="encryptionStr">cookie明码串</param>
        /// <returns></returns>
        public static HttpCookie CreateUserInfoCookie(string encryptionStr)
        {
            string cookieValue = Encryption.StringEncryptionAES(encryptionStr, "lanxum");
            HttpCookie userInfoCookie = new HttpCookie("UserInfo", cookieValue);
            userInfoCookie.HttpOnly = false;
            userInfoCookie.Expires = System.DateTime.Now.AddDays(360);

            string[] userInfo = encryptionStr.Split(',');
            if (userInfo.Length == 6 && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session["UserID"] = userInfo[0].Split('=').Length == 2 ? userInfo[0].Split('=')[1] : "";
                HttpContext.Current.Session["UserName"] = userInfo[1].Split('=').Length == 2 ? userInfo[1].Split('=')[1] : "";
                HttpContext.Current.Session["CardNo"] = userInfo[2].Split('=').Length == 2 ? userInfo[2].Split('=')[1] : "";
                HttpContext.Current.Session["LoginID"] = userInfo[3].Split('=').Length == 2 ? userInfo[3].Split('=')[1] : "";
                HttpContext.Current.Session["UserCode"] = userInfo[5].Split('=').Length == 2 ? userInfo[5].Split('=')[1] : "";
            }

            return userInfoCookie;
        }
        /// <summary>
        /// 解密cookie字符串，返回用户信息
        /// </summary>
        /// <param name="cookieStr">cookie加密串</param>
        /// <returns></returns>
        public static UserInfoDTO DecryCookie(string cookieStr)
        {
            UserInfoDTO userInfoDTO = new UserInfoDTO();
            string decryptionStr = Decryption.StringDecryptionAES(cookieStr, "lanxum");//cookie明码串
            string[] userInfo = decryptionStr.Split(',');
            foreach (var s in userInfo)
            {
                string[] detail = s.Split('=');
                if (detail.Length == 2)
                {
                    switch (detail[0])
                    {
                        case "UserID":
                            userInfoDTO.UserID = detail[1];
                            break;
                        case "UserName":
                            userInfoDTO.UserName = detail[1];
                            break;
                        case "CardNo":
                            userInfoDTO.CardNo = detail[1];
                            break;
                        case "LoginID":
                            userInfoDTO.LoginID = detail[1];
                            break;
                        case "RememberUserID":
                            userInfoDTO.RememberUserID = bool.Parse(detail[1].ToString());
                            break;
                        case "UserCode":
                            userInfoDTO.UserCode = detail[1];
                            break;
                        default:
                            break;
                    }
                }

            }
            return userInfoDTO;
        }
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
    }
}
