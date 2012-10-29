namespace LINQPad
{
    using System;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    internal class ProxyOptions : SerializableOptions
    {
        private static ProxyOptions _instance;
        private static object _locker = new object();
        [Serialize]
        public string Address;
        [Serialize]
        public bool DisableExpect100Continue;
        [Serialize]
        public string Domain;
        public static readonly string FileName = Path.Combine(Program.LocalUserDataFolder, "Proxy.xml");
        [Serialize]
        public bool IsManual;
        [Serialize]
        public string PasswordData;
        [Serialize]
        public ushort Port;
        [Serialize]
        public string Username;

        public WebClient GetWebClient()
        {
            WebClient client = new WebClient();
            WebProxy webProxy = this.GetWebProxy();
            if (webProxy != null)
            {
                client.Proxy = webProxy;
            }
            return client;
        }

        public WebProxy GetWebProxy()
        {
            if (!this.IsManual)
            {
                return null;
            }
            if ((this.Address ?? "").Trim().Length == 0)
            {
                return null;
            }
            ServicePointManager.Expect100Continue = !this.DisableExpect100Continue;
            string uriString = this.Address.Trim();
            if (this.Port > 0)
            {
                uriString = uriString + ":" + this.Port;
            }
            WebProxy proxy3 = new WebProxy {
                Address = new Uri(uriString)
            };
            if ((this.Username ?? "").Trim().Length > 0)
            {
                if ((this.Domain ?? "").Trim().Length > 0)
                {
                    proxy3.Credentials = new NetworkCredential(this.Username.Trim(), this.Password, this.Domain.Trim());
                }
                else
                {
                    proxy3.Credentials = new NetworkCredential(this.Username.Trim(), this.Password);
                }
            }
            return proxy3;
        }

        public bool IsValidProxy()
        {
            return (this.IsManual && ((this.Address ?? "").Trim().Length > 0));
        }

        public override string FullPath
        {
            get
            {
                return FileName;
            }
        }

        public static ProxyOptions Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        try
                        {
                            _instance = new ProxyOptions();
                            _instance.Deserialize();
                        }
                        catch
                        {
                        }
                    }
                    return _instance;
                }
            }
        }

        internal string Password
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(this.PasswordData))
                    {
                        return "";
                    }
                    byte[] bytes = ProtectedData.Unprotect(Convert.FromBase64String(this.PasswordData), base.GetType().Assembly.GetName().GetPublicKey(), DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.PasswordData = "";
                }
                else
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(value);
                    this.PasswordData = Convert.ToBase64String(ProtectedData.Protect(bytes, base.GetType().Assembly.GetName().GetPublicKey(), DataProtectionScope.CurrentUser));
                }
            }
        }
    }
}

