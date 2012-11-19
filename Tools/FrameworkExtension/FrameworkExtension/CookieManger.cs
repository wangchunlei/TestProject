using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using Domas.DAP.ADF.Security;

namespace Domas.DAP.ADF.Cookie
{
    public class CookieManger
    {
        [DllImport("wininet.dll", SetLastError = true)]
        protected static extern bool InternetGetCookie(string url, string cookieName, StringBuilder cookieData, ref int size);
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool WTSQueryUserToken(int sessionId, out IntPtr tokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint WTSGetActiveConsoleSessionId();
        public static CookieContainer GetUriCookieContainer(Uri uri)
        {
            var logger = Domas.DAP.ADF.LogManager.LogManager.GetLogger("GetCookie");
            CookieContainer cookies = null;
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && windowsIdentity.IsSystem)
            {
                logger.Debug(string.Format("当前用户{0}", windowsIdentity.Name));
                int sessionID = (int)WTSGetActiveConsoleSessionId();
                if (sessionID != -1)
                {
                    System.IntPtr currentToken = IntPtr.Zero;
                    bool bRet = WTSQueryUserToken(sessionID, out currentToken);
                    using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(currentToken))
                    {
                        cookies = GetUriCookieContainer(uri);
                        if (cookies == null)
                        {
                            logger.Debug("没有取到");
                            string cookieValue = string.Empty;
                            GetCookie_InternetExplorer(uri.Host, "userinfo", ref cookieValue);
                            logger.Debug(cookieValue);
                            if (!string.IsNullOrEmpty(cookieValue))
                            {
                                cookies = new CookieContainer();
                                cookies.Add(uri, new System.Net.Cookie("userinfo", cookieValue));
                            }
                        }

                    }
                }
                return cookies;
            }

            //定义Cookie数据的大小。
            int datasize = 256;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookie(uri.ToString(), null, cookieData, ref datasize))
            {
                if (datasize < 0) return null;
                // 确信有足够大的空间来容纳Cookie数据。
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookie(uri.ToString(), null, cookieData, ref datasize)) return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }
        private static bool GetCookie_InternetExplorer(string strHost, string strField, ref string Value)
        {
            Value = string.Empty;
            bool fRtn = false;
            string strPath, strCookie;
            string[] fp;
            StreamReader r;
            int idx;

            try
            {
                strField = strField + "\n";
                strPath = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
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
