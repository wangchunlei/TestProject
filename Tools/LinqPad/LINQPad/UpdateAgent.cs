namespace LINQPad
{
    using LINQPad.UI;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal static class UpdateAgent
    {
        private static bool _useSlowWebClient;
        public static readonly string BetaFolder = Path.Combine(Program.AppDataFolder, "BetaVersion.V4");
        public const string ConfigTextV4 = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<configuration>\r\n  <startup useLegacyV2RuntimeActivationPolicy=\"true\">\r\n    <supportedRuntime version=\"v4.0\"/>\r\n  </startup>\r\n  <runtime>\r\n    <legacyUnhandledExceptionPolicy enabled=\"1\" />\r\n  </runtime>\r\n</configuration>";
        public const string ExeName = "LINQPad.exe";
        public static readonly string RetractionsFile = Path.Combine(UpdatesFolder, "retractions.txt");
        internal static volatile bool UpdateReady;
        public static readonly string UpdatesFolder = Path.Combine(Program.AppDataFolder, "Updates40");
        private const string UpdatesUriBase = "http://www.linqpad.net/updates40/";

        internal static void CheckForMessage()
        {
            try
            {
                string fileName = Path.Combine(UpdatesFolder, "message.exe");
                FileInfo info = new FileInfo(fileName);
                if ((!info.Exists || (info.Length == 0L)) || (info.LastWriteTime < DateTime.Now.AddDays(-1.0)))
                {
                    if (info.Exists)
                    {
                        info.Delete();
                    }
                    else if (!Directory.Exists(UpdatesFolder))
                    {
                        Directory.CreateDirectory(UpdatesFolder);
                    }
                    using (WebClient client = _useSlowWebClient ? WebHelper.GetBackupWebClient() : WebHelper.GetFastWebClient())
                    {
                        client.DownloadFile("http://www.linqpad.net/updates40/message2", fileName);
                    }
                }
                if (File.Exists(fileName) && IsAssemblyValid(fileName))
                {
                    AppDomain.CurrentDomain.ExecuteAssembly(fileName);
                }
            }
            catch
            {
            }
        }

        internal static void CheckForUpdates(object caller)
        {
            try
            {
                int? latestVersion = GetLatestVersion(true, caller);
                if (latestVersion.HasValue)
                {
                    int? nullable2 = latestVersion;
                    int majorMinorVersion = Program.MajorMinorVersion;
                    if ((nullable2.GetValueOrDefault() > majorMinorVersion) || !nullable2.HasValue)
                    {
                        string str = Path.Combine(UpdatesFolder, latestVersion.ToString());
                        string path = Path.Combine(str, "LINQPad.exe");
                        if ((!Directory.Exists(str) || !File.Exists(path)) || !IsAssemblyValid(path))
                        {
                            if (!Directory.Exists(str))
                            {
                                Directory.CreateDirectory(str);
                            }
                            using (WebClient client = _useSlowWebClient ? WebHelper.GetBackupWebClient() : WebHelper.GetFastWebClient())
                            {
                                if (WaitForMainForm())
                                {
                                    MainForm.Instance.Invoke(new MethodInvoker(MainForm.Instance.InformUpdateInProgress));
                                }
                                string str3 = Path.ChangeExtension(path, ".tmp");
                                try
                                {
                                    DeleteFile(str3);
                                }
                                catch
                                {
                                    str3 = Path.ChangeExtension(path, ".tm" + new Random().Next(10));
                                    DeleteFile(str3);
                                }
                                string str4 = null;
                                bool flag = false;
                                string str5 = ".txt";
                                try
                                {
                                    str4 = client.DownloadString("http://www.linqpad.net/updates40/redirect" + str5);
                                    if ((str4 != null) && (str4.Trim().Length > 5))
                                    {
                                        Log.Write("Downloading update from: " + str4 + latestVersion.ToString());
                                        client.DownloadFile(str4 + latestVersion.ToString(), str3);
                                        flag = true;
                                    }
                                }
                                catch
                                {
                                }
                                if (!flag)
                                {
                                    Log.Write("Downloading update from: http://www.linqpad.net/updates40/" + latestVersion.ToString());
                                    client.DownloadFile("http://www.linqpad.net/updates40/" + latestVersion.ToString(), str3);
                                }
                                DeleteFile(path);
                                File.Move(str3, path);
                            }
                            try
                            {
                                File.WriteAllText(path + ".config", "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<configuration>\r\n  <startup useLegacyV2RuntimeActivationPolicy=\"true\">\r\n    <supportedRuntime version=\"v4.0\"/>\r\n  </startup>\r\n  <runtime>\r\n    <legacyUnhandledExceptionPolicy enabled=\"1\" />\r\n  </runtime>\r\n</configuration>");
                            }
                            catch
                            {
                            }
                            UpdateReady = true;
                            if (WaitForMainForm())
                            {
                                MainForm.Instance.Invoke(new MethodInvoker(MainForm.Instance.InformAboutUpdate));
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception, "CheckForUpdates");
            }
        }

        private static void DeleteFile(string path)
        {
            FileInfo info = new FileInfo(path);
            if (info.Exists)
            {
                if (info.IsReadOnly)
                {
                    info.IsReadOnly = false;
                }
                info.Delete();
            }
        }

        internal static void DeleteOldUpdates()
        {
            if (Directory.Exists(UpdatesFolder))
            {
                foreach (string str in (from d in new DirectoryInfo(UpdatesFolder).GetDirectories()
                    where d.Name.All<char>(c => char.IsDigit(c))
                    let version = int.Parse(d.Name)
                    where (version < 0xe2) || ((version > 400) && (version < 0x1aa))
                    let exePath = Path.Combine(d.FullName, "LINQPad.exe")
                    where File.Exists(exePath)
                    select exePath).ToArray<string>())
                {
                    try
                    {
                        Thread.Sleep(100);
                        Directory.Delete(Path.GetDirectoryName(str), true);
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal static string GetLaterExe()
        {
            if (!Directory.Exists(UpdatesFolder))
            {
                return null;
            }
            string[] retractions = new string[0];
            if (File.Exists(RetractionsFile))
            {
                try
                {
                    retractions = File.ReadAllText(RetractionsFile).Trim().Split(new char[] { ',' });
                }
                catch
                {
                    Thread.Sleep(500);
                    try
                    {
                        retractions = File.ReadAllText(RetractionsFile).Trim().Split(new char[] { ',' });
                    }
                    catch (Exception exception)
                    {
                        Log.Write(exception, "GetLaterExe");
                    }
                }
            }
            return (from d in new DirectoryInfo(UpdatesFolder).GetDirectories()
                where d.Name.All<char>(c => char.IsDigit(c))
                let version = int.Parse(d.Name)
                where (version > Program.MajorMinorVersion) && !retractions.Contains<string>(version.ToString())
                orderby version descending
                let exePath = Path.Combine(d.FullName, "LINQPad.exe")
                where File.Exists(exePath) && IsAssemblyValid(exePath)
                select exePath).FirstOrDefault<string>();
        }

        internal static Version GetLatestBetaVersion()
        {
            string str = ".txt";
            string str2 = null;
            try
            {
                using (WebClient client = _useSlowWebClient ? WebHelper.GetBackupWebClient() : WebHelper.GetFastWebClient())
                {
                    str2 = client.DownloadString("http://www.linqpad.net/updates40/betaversionx" + str);
                }
            }
            catch
            {
            }
            if ((str2 == null) || (str2.Length < 3))
            {
                return null;
            }
            try
            {
                return new Version(str2.Substring(0, 1) + "." + str2.Substring(1) + ".0");
            }
            catch
            {
                return null;
            }
        }

        internal static int? GetLatestVersion(bool startupCheck, object caller)
        {
            WebClient client;
            int num;
            string str = ".txt";
            string str2 = null;
            try
            {
                using (client = GetWebClient(true, startupCheck, caller))
                {
                    str2 = client.DownloadString("http://www.linqpad.net/updates40/version" + str);
                }
            }
            catch
            {
            }
            if (string.IsNullOrEmpty(str2))
            {
                try
                {
                    using (client = GetWebClient(false, startupCheck, caller))
                    {
                        str2 = client.DownloadString("http://www.linqpad.net/updates40/version" + str);
                    }
                    _useSlowWebClient = true;
                }
                catch
                {
                }
            }
            if (string.IsNullOrEmpty(str2))
            {
                return null;
            }
            if (!int.TryParse(str2, out num))
            {
                return null;
            }
            return new int?(num);
        }

        private static WebClient GetWebClient(bool fast, bool startupCheck, object caller)
        {
            WebClient client = fast ? WebHelper.GetFastWebClient() : WebHelper.GetBackupWebClient();
            try
            {
                if (startupCheck)
                {
                    bool flag;
                    string location;
                    if (flag = (caller is string) && File.Exists((string) caller))
                    {
                        location = (string) caller;
                    }
                    else
                    {
                        location = Assembly.GetExecutingAssembly().Location;
                    }
                    if (!File.Exists(location))
                    {
                        return client;
                    }
                    DateTime creationTimeUtc = File.GetCreationTimeUtc(location);
                    string str2 = "linqpad.net";
                    client.Headers["referer"] = "http://" + str2 + "/lp/old?" + (flag ? "f" : "c") + "=" + creationTimeUtc.ToString("s") + "&v=" + Program.VersionString;
                }
            }
            catch
            {
            }
            return client;
        }

        public static bool IsAssemblyValid(string path)
        {
            bool flag;
            using (DomainIsolator isolator = new DomainIsolator("GuineaPig"))
            {
                isolator.Domain.SetData("AssemblyToTest", path);
                try
                {
                    isolator.Domain.DoCallBack(new CrossAppDomainDelegate(UpdateAgent.TestAssembly));
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;
        }

        internal static void RunServerComms(object caller)
        {
            CheckForUpdates(caller);
            CheckForMessage();
            DeleteOldUpdates();
        }

        private static void TestAssembly()
        {
            byte[] publicKey = Assembly.LoadFile((string) AppDomain.CurrentDomain.GetData("AssemblyToTest")).GetName().GetPublicKey();
            byte[] second = typeof(Program).Assembly.GetName().GetPublicKey();
            if (!publicKey.SequenceEqual<byte>(second))
            {
                second = Convert.FromBase64String("ACQAAASAAACUAAAABgIAAAAkAABSU0ExAAQAAAEAAQABjnpxf7nD0OR4svA3sGzyVwFahX6Y8OK1t6fJblAXq3chgz6bUPspknCaKSJ1EciFh+8idj6Si+zm2IbRdP+KvYxDopk+JjTo1XyeVjssZcX5x5+Bexd3eCY0FnOYF+YXd2+aDYgJ7Sh5WeXUcbprMzlpz4P5JhcIaVhgTYtLrg==");
                if (!publicKey.SequenceEqual<byte>(second))
                {
                    throw new InvalidOperationException("Assembly has invalid public key");
                }
            }
        }

        internal static void UpdateRetractions()
        {
            string str = ".txt";
            try
            {
                using (WebClient client = WebHelper.GetWebClient())
                {
                    string contents = client.DownloadString("http://www.linqpad.net/updates40/retractions" + str);
                    if (!File.Exists(RetractionsFile) || !(File.ReadAllText(RetractionsFile).Trim() == contents.Trim()))
                    {
                        try
                        {
                            File.WriteAllText(RetractionsFile, contents);
                        }
                        catch
                        {
                            Thread.Sleep(300);
                            File.WriteAllText(RetractionsFile, contents);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static bool WaitForMainForm()
        {
            for (int i = 0; i < 50; i++)
            {
                if ((MainForm.Instance != null) && MainForm.Instance.IsHandleCreated)
                {
                    if (MainForm.Instance.IsDisposed)
                    {
                        return false;
                    }
                    return true;
                }
                Thread.Sleep(500);
            }
            return false;
        }
    }
}

