namespace LINQPad
{
    using ActiproBridge;
    using ActiproSoftware.SyntaxEditor;
    using LINQPad.ExecutionModel;
    using LINQPad.Properties;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using System.IO.Pipes;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Timers;
    using System.Windows.Forms;

    internal static class Program
    {
        private static string _appInstanceID;
        private static bool _enableMars;
        private static bool _freshAppDomains;
        private static DateTimeOffset _lastError = DateTimeOffset.MinValue;
        private static Dictionary<string, Assembly> _libs = new Dictionary<string, Assembly>();
        private static bool _linqPadAssemblyResolverAdded;
        private static int _majorMinorVersion;
        private static bool _mtaMode;
        private static bool _presentationMode;
        private static bool _preserveAppDomains;
        private static int _sessionID = new Random().Next(0xf423f);
        private static string _tempFolder;
        private static Thread _updateThread;
        private static System.Version _version;
        public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LINQPad");
        public static readonly Color LightTransparencyKey = Color.FromArgb(0xf7, 0xef, 250);
        public static string LocalUserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LINQPad");
        internal static readonly string OneToOneAckFile = Path.Combine(AppDataFolder, "OneToOneEnabled.txt");
        internal static readonly string OptimizeQueriesFile = Path.Combine(UserDataFolder, "OptimizeQueries.txt");
        public const string ProgramSubPath = "LINQPad";
        internal static string QueryConfigFile;
        internal static LINQPad.UI.Splash Splash;
        public static readonly string TempFolderBase = Path.Combine(Path.GetTempPath(), "LINQPad");
        public static Color TransparencyKey = Color.FromArgb(10, 10, 10);
        public static string UserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LINQPad");

        internal static void AddLINQPadAssemblyResolver()
        {
            if (!_linqPadAssemblyResolverAdded)
            {
                _linqPadAssemblyResolverAdded = true;
                AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args) {
                    if (args.Name.ToLowerInvariant().StartsWith("linqpad,"))
                    {
                        string data = AppDomain.CurrentDomain.GetData("LINQPad.Path") as string;
                        if (!string.IsNullOrEmpty(data))
                        {
                            return Assembly.LoadFrom(data);
                        }
                    }
                    if (((((args.Name == "Ionic.Zip.Reduced, Version=1.7.2.12, Culture=neutral, PublicKeyToken=791165b13cf84eca") || (args.Name == "ActiproSoftware.SyntaxEditor.Net20, Version=4.0.277.0, Culture=neutral, PublicKeyToken=21a821480e210563")) || ((args.Name == "ActiproSoftware.WinUICore.Net20, Version=1.0.96.0, Culture=neutral, PublicKeyToken=1eba893a2bc55de5") || (args.Name == "ActiproSoftware.Shared.Net20, Version=1.0.96.0, Culture=neutral, PublicKeyToken=36ff2196ab5654b9"))) || ((args.Name == "ActiproSoftware.SyntaxEditor.Addons.DotNet.Net20, Version=4.0.277.0, Culture=neutral, PublicKeyToken=21a821480e210563") || (args.Name == "DotNetLanguage, Version=4.9.0.0, Culture=neutral, PublicKeyToken=53cd1b2bfe1e886e"))) || (args.Name == "System.Data.Services.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))
                    {
                        return FindAssem(sender, args);
                    }
                    return null;
                };
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ProcessException(e.Exception);
        }

        private static bool CheckForExistingProcess(string queryToLoad)
        {
            try
            {
                DateTime startTime;
                int ourID;
                using (Process process = Process.GetCurrentProcess())
                {
                    ourID = process.Id;
                    startTime = process.StartTime;
                }
                Process process2 = (from p in Process.GetProcesses()
                    where (p.ProcessName == "LINQPad") && (p.Id != ourID)
                    select p).OrderBy<Process, DateTime>(delegate (Process p) {
                    try
                    {
                        return p.StartTime;
                    }
                    catch
                    {
                        return DateTime.MaxValue;
                    }
                }).FirstOrDefault<Process>();
                if (process2 == null)
                {
                    return false;
                }
                using (process2)
                {
                    if (process2.StartTime > startTime)
                    {
                        return false;
                    }
                    if (process2.StartTime > DateTime.Now.AddSeconds(-8.0))
                    {
                        process2.WaitForInputIdle();
                    }
                    using (NamedPipeClientStream stream = new NamedPipeClientStream("LINQPad." + process2.Id))
                    {
                        stream.Connect(300);
                        byte[] bytes = Encoding.UTF8.GetBytes("OpenQuery(\"" + queryToLoad + "\")");
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    try
                    {
                        Native.SetForegroundWindow(process2.MainWindowHandle);
                    }
                    catch
                    {
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.Write(exception, "CheckForExistingProcess");
                return false;
            }
        }

        internal static bool CreateAppDataFolder()
        {
            try
            {
                if (!Directory.Exists(AppDataFolder))
                {
                    Directory.CreateDirectory(AppDataFolder);
                }
                FileUtil.AssignUserPermissionsToFolder(AppDataFolder);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exceptionObject = e.ExceptionObject as Exception;
            if (e.IsTerminating || ((!(exceptionObject is ThreadAbortException) && !(exceptionObject is AppDomainUnloadedException)) && (!(exceptionObject is InvalidComObjectException) || !exceptionObject.StackTrace.Contains("System.Windows.Input.TextServicesContext"))))
            {
                if ((exceptionObject != null) && ((exceptionObject.Data == null) || !exceptionObject.Data.Contains("LINQPadHandled")))
                {
                    Log.Write(exceptionObject, "Thread exception");
                    if (((Server.CurrentServer != null) && Server.CurrentServer.InMessageLoop) && (e.ExceptionObject is COMException))
                    {
                        return;
                    }
                    try
                    {
                        new ThreadExceptionForm(exceptionObject).ShowDialog();
                    }
                    catch
                    {
                    }
                }
                Thread.CurrentThread.IsBackground = true;
                if (e.IsTerminating)
                {
                    Thread.Sleep(-1);
                }
            }
        }

        internal static Assembly FindAssem(object sender, ResolveEventArgs args)
        {
            Assembly assembly;
            string str = args.Name.ToLowerInvariant();
            if (!((((str.Contains("actipro") || str.Contains("dotnetlanguage")) || (str.Contains("mono.cecil") || str.Contains("resources"))) || (str.Contains("icsharpcode") || str.Contains("ionic.zip"))) || str.Contains("data.services.design")))
            {
                return null;
            }
            string name = new AssemblyName(args.Name).Name;
            if (name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (name.Contains("sources"))
            {
                name = name + "4";
            }
            if (_libs.ContainsKey(name))
            {
                return _libs[name];
            }
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LINQPad.assemblies." + name + ".bin");
            if (manifestResourceStream == null)
            {
                return null;
            }
            using (manifestResourceStream)
            {
                using (DeflateStream stream3 = new DeflateStream(manifestResourceStream, CompressionMode.Decompress))
                {
                    Assembly assembly2 = Assembly.Load(new BinaryReader(stream3).ReadBytes(0x1e8480));
                    _libs[name] = assembly2;
                    assembly = assembly2;
                }
            }
            return assembly;
        }

        private static string GetExceptionInfo(Exception ex, int depth)
        {
            string stackTrace = ex.StackTrace;
            try
            {
                stackTrace = stackTrace + "\r\n\r\n" + GetStackInfo(ex);
            }
            catch
            {
            }
            string str2 = ex.GetType().Name + "\r\n\r\n" + ex.Message + "\r\n\r\n" + stackTrace;
            if ((ex.InnerException != null) && (depth < 3))
            {
                str2 = str2 + "\r\nINNER: " + GetExceptionInfo(ex.InnerException, depth + 1);
            }
            string str3 = "".PadRight(depth * 2);
            if (str3.Length > 0)
            {
                str2 = str3 + str2.Replace("\n", "\n" + str3);
            }
            return str2;
        }

        private static string GetStackInfo(Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            int num = 0;
            foreach (StackFrame frame in new StackTrace(ex).GetFrames())
            {
                builder.Append(" -");
                int iLOffset = frame.GetILOffset();
                MethodBase method = frame.GetMethod();
                if ((method != null) && (method.DeclaringType != null))
                {
                    builder.Append(method.DeclaringType.FullName);
                }
                if (method != null)
                {
                    builder.Append(" " + method.ToString());
                }
                builder.Append(" offset: 0x" + iLOffset.ToString("X"));
                builder.AppendLine();
                if ((((iLOffset > 0) && (method != null)) && (method.ReflectedType != null)) && !method.ReflectedType.Namespace.StartsWith("System", StringComparison.Ordinal))
                {
                    try
                    {
                        builder.AppendLine("   " + Disassembler.Disassemble(method, iLOffset - 0x19, iLOffset + 0x19).Replace("\n", "\n   "));
                    }
                    catch (Exception exception)
                    {
                        builder.AppendLine("   <" + exception.Message + ex.StackTrace + ">");
                    }
                }
                builder.AppendLine();
                num++;
            }
            return builder.ToString();
        }

        private static void Go(string[] args)
        {
            if (Path.GetFileName(Assembly.GetExecutingAssembly().Location).ToLowerInvariant() != "linqpad.exe")
            {
                MessageBox.Show("The application must be named LINQPad.exe in order to run.", "LINQPad");
            }
            else
            {
                if ((args.Length > 0) && !args[0].StartsWith("-", StringComparison.Ordinal))
                {
                }
                string queryToLoad = (CS$<>9__CachedAnonymousMethodDelegatef != null) ? null : string.Join(" ", args.TakeWhile<string>(CS$<>9__CachedAnonymousMethodDelegatef).ToArray<string>());
                if (queryToLoad != null)
                {
                    queryToLoad = queryToLoad.Trim();
                    if (queryToLoad.StartsWith("\"") && queryToLoad.EndsWith("\""))
                    {
                        queryToLoad = queryToLoad.Substring(1, queryToLoad.Length - 2);
                    }
                }
                if ((queryToLoad == null) || !CheckForExistingProcess(queryToLoad))
                {
                    UserOptions instance = UserOptions.Instance;
                    bool noForward = args.Any<string>(a => a.ToLowerInvariant() == "-noforward");
                    bool noUpdate = args.Any<string>(a => a.ToLowerInvariant() == "-noupdate");
                    BetaMode = args.Any<string>(a => a.ToLowerInvariant() == "-beta");
                    _mtaMode = args.Any<string>(a => a.ToLowerInvariant() == "-mta");
                    _presentationMode = args.Any<string>(a => a.ToLowerInvariant() == "-presenter");
                    _preserveAppDomains = args.Any<string>(a => a.ToLowerInvariant() == "-preserveappdomains");
                    _freshAppDomains = args.Any<string>(a => a.ToLowerInvariant() == "-freshappdomains");
                    _enableMars = args.Any<string>(a => a.ToLowerInvariant() == "-mars");
                    DiagnosticMode = args.Any<string>(a => a.ToLowerInvariant() == "-diagnosticmode");
                    bool runQuery = args.Any<string>(a => a.ToLowerInvariant() == "-run");
                    string caller = args.FirstOrDefault<string>(a => a.StartsWith("-caller=", StringComparison.OrdinalIgnoreCase));
                    if (caller != null)
                    {
                        Caller = caller = caller.Substring(8);
                    }
                    string activationCode = args.FirstOrDefault<string>(a => a.StartsWith("-activate=", StringComparison.OrdinalIgnoreCase));
                    if (activationCode != null)
                    {
                        activationCode = activationCode.Substring(10);
                        noUpdate = noForward = true;
                    }
                    bool deactivate = args.Any<string>(a => a.ToLowerInvariant() == "-deactivate");
                    if (!noForward)
                    {
                        string laterExe = UpdateAgent.GetLaterExe();
                        if (laterExe != null)
                        {
                            string arguments = string.Join(" ", args) + " -noforward";
                            if (caller == null)
                            {
                                arguments = arguments + " -caller=\"" + Assembly.GetExecutingAssembly().Location + "\"";
                            }
                            try
                            {
                                PropagateConfig(laterExe);
                            }
                            catch
                            {
                            }
                            Process.Start(laterExe, arguments);
                            UpdateAgent.UpdateRetractions();
                            return;
                        }
                    }
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Program.FindAssem);
                    try
                    {
                        CreateAppDataFolder();
                        string str7 = "LINQPad4Path.txt";
                        string path = Path.Combine(AppDataFolder, str7);
                        if (!(File.Exists(path) && !(File.ReadAllText(path) != Assembly.GetExecutingAssembly().Location)))
                        {
                            File.WriteAllText(path, Assembly.GetExecutingAssembly().Location);
                        }
                    }
                    catch
                    {
                    }
                    try
                    {
                        if (Caller != null)
                        {
                            QueryConfigFile = Path.Combine(Path.GetDirectoryName(Caller), "LINQPad.config");
                        }
                        else
                        {
                            QueryConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LINQPad.config");
                        }
                    }
                    catch
                    {
                    }
                    new Thread(new ThreadStart(Program.Listen)) { IsBackground = true, Name = "OpenQuery Monitor" }.Start();
                    Run(queryToLoad, runQuery, activationCode, deactivate, noForward, noUpdate, caller);
                }
            }
        }

        private static void Listen()
        {
            Exception exception;
            try
            {
                int id = Process.GetCurrentProcess().Id;
                using (NamedPipeServerStream stream = new NamedPipeServerStream("LINQPad." + id, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                {
                    while (true)
                    {
                        try
                        {
                            stream.WaitForConnection();
                            string str = Encoding.UTF8.GetString(ReadMessage(stream));
                            if (((str != null) && str.StartsWith("OpenQuery(\"")) && str.EndsWith("\")"))
                            {
                                string filename = str.Substring(11, str.Length - 13);
                                ThreadPool.QueueUserWorkItem(o => OpenFile(filename));
                            }
                            stream.Disconnect();
                        }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                            Log.Write(exception, "Open query from pipe");
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                exception = exception2;
                Log.Write(exception, "Pipe listener for opening queries");
            }
        }

        private static void OpenFile(string filename)
        {
            if (File.Exists(filename))
            {
                MainForm.AppStarted.WaitOne();
                if ((MainForm.Instance != null) && !MainForm.Instance.IsDisposed)
                {
                    MainForm.Instance.BeginInvoke(delegate {
                        MainForm.Instance.OpenQuery(filename, true);
                        MainForm.Instance.Activate();
                    });
                }
            }
        }

        private static void PopulateVersion()
        {
            if (_version == null)
            {
                _version = new System.Version(typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).Cast<AssemblyFileVersionAttribute>().Single<AssemblyFileVersionAttribute>().Version);
                _majorMinorVersion = (_version.Major * 100) + _version.Minor;
            }
        }

        internal static void ProcessException(Exception ex)
        {
            Log.Write(ex, "ProcessException");
            if (_lastError < DateTimeOffset.Now.AddSeconds(-3.0))
            {
                _lastError = DateTimeOffset.Now;
                string error = (ex is DisplayToUserException) ? ex.Message : ex.GetType().FullName;
                string details = (!(ex is DisplayToUserException) || (ex.Message.Length >= 80)) ? ex.Message : "";
                if (ex.Message.StartsWith("Could not load file or assembly 'Microsoft.ComplexEventProcessing", StringComparison.Ordinal))
                {
                    error = "Have you installed the right StreamInsight version?";
                }
                ThreadStart start = null;
                using (ErrorForm form = new ErrorForm(error, details))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (start == null)
                        {
                            start = delegate {
                                try
                                {
                                    SubmitFeedback("LINQPad Bug Report", null, GetExceptionInfo(ex, 0), form.AdditionalInfo);
                                    MessageBox.Show("Thank you - error was successfully reported.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                }
                                catch
                                {
                                }
                            };
                        }
                        new Thread(start) { Name = "Feedback Submission" }.Start();
                    }
                }
            }
            _lastError = DateTimeOffset.Now;
        }

        private static void PropagateConfig(string laterVersion)
        {
            string configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            if ((!string.IsNullOrEmpty(configurationFile) && File.Exists(configurationFile)) && !File.Exists(laterVersion + ".keepConfig"))
            {
                string path = laterVersion + ".config";
                if (!File.Exists(path) || !(File.ReadAllText(path) == File.ReadAllText(configurationFile)))
                {
                    File.Copy(configurationFile, path, true);
                }
            }
        }

        private static void QueryOneToOne()
        {
            AllowOneToOne = true;
            try
            {
                if (File.Exists(OneToOneAckFile))
                {
                    AllowOneToOne = string.Equals(File.ReadAllText(OneToOneAckFile).Trim(), "True", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
            }
        }

        private static void QueryStudioKeys(bool viaUpdate)
        {
            try
            {
                if (!UserOptions.Instance.NoNativeKeysQuestion)
                {
                    UserOptions.Instance.NoNativeKeysQuestion = true;
                    UserOptions.Instance.Save();
                    if (viaUpdate)
                    {
                        bool flag1 = MessageBox.Show("Would you like to enable Visual Studio-compatible shortcut keys?\r\n\r\n(You can change this later in Edit | Preferences.)", "LINQPad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No;
                        UserOptions.Instance.NativeHotKeys = true;
                        UserOptions.Instance.Save();
                    }
                }
            }
            catch
            {
            }
        }

        private static byte[] ReadMessage(PipeStream s)
        {
            MemoryStream stream = new MemoryStream();
            byte[] buffer = new byte[0x1000];
            do
            {
                stream.Write(buffer, 0, s.Read(buffer, 0, buffer.Length));
            }
            while (!s.IsMessageComplete);
            return stream.ToArray();
        }

        private static void Run(string queryToLoad, bool runQuery, string activationCode, bool deactivate, bool noForward, bool noUpdate, string caller)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Environment.OSVersion.Version.Major >= 6)
            {
                try
                {
                    SetProcessDPIAware();
                }
                catch
                {
                }
            }
            AutoSaver.Start();
            Wheeler.Register();
            if (Environment.OSVersion.Version.Major < 6)
            {
                ToolStripManager.RenderMode = ToolStripManagerRenderMode.System;
            }
            if (!((activationCode != null) || deactivate))
            {
                Splash = new LINQPad.UI.Splash();
                Splash.Show();
                Splash.Update();
            }
            Log.add_StringWriter(new Action<string>(Log.Write));
            Log.add_ExceptionWriter(new Action<Exception>(Log.Write));
            TypeResolver.ManyToOne = Resources.ManyToOne;
            TypeResolver.OneToMany = Resources.OneToMany;
            TypeResolver.OneToOne = Resources.OneToOne;
            TypeResolver.ManyToMany = Resources.ManyToMany;
            TypeResolver.Results = Resources.Results;
            TypeResolver.Column = Resources.Column;
            TypeResolver.Key = Resources.Key;
            TypeResolver.Database = Resources.Database;
            TypeResolver.GetSameFolderReferences = new Func<string, string[]>(AssemblyProber.GetSameFolderReferences);
            WSAgent.WebClientFactory = new Func<WebClient>(WebHelper.GetWebClient);
            WSAgent.FastWebClientFactory = new Func<WebClient>(WebHelper.GetFastWebClient);
            WSAgent.BackupWebClientFactory = new Func<WebClient>(WebHelper.GetBackupWebClient);
            WSAgent.CurrentVersionString = VersionString;
            AutocompletionManager.PassiveAutocompletion = UserOptions.Instance.PassiveAutocompletion;
            AutocompletionManager.DisableLambdaSnippets = UserOptions.Instance.DisableLambdaSnippets;
            SnippetManager.set_LINQPadSnippetsFolder(UserOptions.Instance.GetCustomSnippetsFolder(false));
            WSAgent.DiagnosticMode = DiagnosticMode;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);
            if (deactivate)
            {
                WSAgent.Remove(true);
            }
            else
            {
                if (activationCode != null)
                {
                    using (RegisterForm form = new RegisterForm(activationCode))
                    {
                        form.ShowDialog();
                        return;
                    }
                }
                if (!noUpdate)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(UpdateAgent.RunServerComms)) {
                        Name = "Update Agent",
                        IsBackground = true
                    };
                    _updateThread = thread;
                    _updateThread.Start(caller);
                }
                if (!string.IsNullOrEmpty(caller))
                {
                    new Thread(delegate {
                        try
                        {
                            Thread.Sleep(0xbb8);
                            if (File.Exists(caller))
                            {
                                FileInfo info = new FileInfo(caller);
                                if (((info.Directory == null) || (info.Directory.Parent == null)) || (info.Directory.Parent.Name.ToLowerInvariant() != "updates"))
                                {
                                    if (info.IsReadOnly)
                                    {
                                        info.IsReadOnly = false;
                                    }
                                    if (!File.Exists(caller + ".config"))
                                    {
                                        try
                                        {
                                            File.WriteAllText(caller + ".config", "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<configuration>\r\n  <startup useLegacyV2RuntimeActivationPolicy=\"true\">\r\n    <supportedRuntime version=\"v4.0\"/>\r\n  </startup>\r\n  <runtime>\r\n    <legacyUnhandledExceptionPolicy enabled=\"1\" />\r\n  </runtime>\r\n</configuration>");
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    string path = Assembly.GetExecutingAssembly().Location + ".config";
                                    if (!File.Exists(path))
                                    {
                                        try
                                        {
                                            File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<configuration>\r\n  <startup useLegacyV2RuntimeActivationPolicy=\"true\">\r\n    <supportedRuntime version=\"v4.0\"/>\r\n  </startup>\r\n  <runtime>\r\n    <legacyUnhandledExceptionPolicy enabled=\"1\" />\r\n  </runtime>\r\n</configuration>");
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    try
                                    {
                                        File.Copy(Assembly.GetExecutingAssembly().Location, caller, true);
                                    }
                                    catch
                                    {
                                        Thread.Sleep(0x1388);
                                        File.Copy(Assembly.GetExecutingAssembly().Location, caller, true);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }) { Name = "Update patcher" }.Start();
                }
                UnpackDb();
                QueryOneToOne();
                QueryStudioKeys(noForward);
                StartSemanticParsingService();
                new Thread(delegate {
                    try
                    {
                        Thread.Sleep(0x3e8);
                        if (GacResolver.IsLINQPadGaced())
                        {
                            MessageBox.Show("Warning: A different revision of LINQPad has been installed to the Global Assembly Cache. This will prevent correct operation.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    catch
                    {
                    }
                }) { IsBackground = true }.Start();
                Application.Run(new MainForm(queryToLoad, runQuery));
                TempFileRef.DeleteAll();
            }
        }

        internal static void RunOnThreadingTimer(Action a, int delay)
        {
            Timer timer = new Timer((double) delay) {
                AutoReset = false
            };
            timer.Elapsed += delegate (object sender, ElapsedEventArgs e) {
                a();
                timer.Dispose();
            };
            timer.Start();
        }

        internal static void RunOnWinFormsTimer(Action a)
        {
            RunOnWinFormsTimer(a, 1);
        }

        internal static void RunOnWinFormsTimer(Action a, int delay)
        {
            Timer tmr = new Timer {
                Interval = delay,
                Enabled = true
            };
            tmr.Tick += delegate (object sender, EventArgs e) {
                tmr.Dispose();
                a();
            };
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError=true)]
        internal static extern bool SetProcessDPIAware();
        public static void Start(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                Application.ThreadException += new ThreadExceptionEventHandler(Program.Application_ThreadException);
            }
            try
            {
                Go(args);
            }
            catch (Exception exception)
            {
                ProcessException(exception);
            }
        }

        private static void StartSemanticParsingService()
        {
            SemanticParserService.Start();
        }

        internal static void SubmitFeedback(string title, string sender, string msg, string additionalInfo)
        {
            try
            {
                SubmitFeedback(title, sender, msg, additionalInfo, false);
            }
            catch
            {
                SubmitFeedback(title, sender, msg, additionalInfo, true);
            }
        }

        private static void SubmitFeedback(string title, string sender, string msg, string additionalInfo, bool slow)
        {
            string address = null;
            WebClient client;
            using (client = slow ? WebHelper.GetBackupWebClient() : WebHelper.GetFastWebClient())
            {
                address = client.DownloadString("http://www.linqpad.net/feedbackuri.txt").Trim();
            }
            using (client = slow ? WebHelper.GetBackupWebClient() : WebHelper.GetFastWebClient())
            {
                NameValueCollection data = new NameValueCollection();
                string str2 = msg;
                string informationalVersion = "?";
                try
                {
                    informationalVersion = ((AssemblyInformationalVersionAttribute) typeof(Queryable).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true)[0]).InformationalVersion;
                }
                catch
                {
                }
                data.Add("subject", title + " - " + Environment.MachineName);
                if (!string.IsNullOrEmpty(sender))
                {
                    data.Add("email", sender);
                }
                data.Add("feedback", string.Concat(new object[] { DateTimeOffset.Now.ToString("o"), "\r\nSession=", _sessionID.ToString(), "\r\nVersion=", VersionString, "\r\nOS=", Environment.OSVersion, "\r\nFW=", informationalVersion, "\r\nPC=", Environment.ProcessorCount.ToString(), string.IsNullOrEmpty(additionalInfo) ? "" : ("\r\nNotes=" + additionalInfo), "\r\n\r\n", str2 }));
                client.UploadValues(address, "POST", data);
            }
        }

        private static void Uncompress(string resourceName, string outPath)
        {
            try
            {
                if (!File.Exists(outPath))
                {
                    string directoryName = Path.GetDirectoryName(outPath);
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    byte[] buffer = new byte[0x1000];
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        using (DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Decompress))
                        {
                            using (Stream stream3 = File.Create(outPath))
                            {
                                int num;
                                goto Label_005A;
                            Label_004F:
                                stream3.Write(buffer, 0, num);
                            Label_005A:
                                num = stream2.Read(buffer, 0, buffer.Length);
                                if (num != 0)
                                {
                                    goto Label_004F;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception, "Uncompress");
            }
        }

        private static void UnpackDb()
        {
            Uncompress("LINQPad.Db.Nutshell", Path.Combine(UserDataFolder, "Nutshell.mdf"));
            Uncompress("LINQPad.Db.Nutshell_log", Path.Combine(UserDataFolder, "Nutshell_log.ldf"));
        }

        public static bool AllowOneToOne
        {
            [CompilerGenerated]
            get
            {
                return <AllowOneToOne>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <AllowOneToOne>k__BackingField = value;
            }
        }

        public static string AppInstanceID
        {
            get
            {
                if (string.IsNullOrEmpty(_appInstanceID))
                {
                    _appInstanceID = AppDomain.CurrentDomain.GetData("LINQPad.InstanceID") as string;
                    if (string.IsNullOrEmpty(_appInstanceID))
                    {
                        _appInstanceID = TempFileRef.GetRandomName(8);
                    }
                }
                return _appInstanceID;
            }
        }

        public static bool BetaMode
        {
            [CompilerGenerated]
            get
            {
                return <BetaMode>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                <BetaMode>k__BackingField = value;
            }
        }

        public static string Caller
        {
            [CompilerGenerated]
            get
            {
                return <Caller>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                <Caller>k__BackingField = value;
            }
        }

        public static bool DiagnosticMode
        {
            [CompilerGenerated]
            get
            {
                return <DiagnosticMode>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                <DiagnosticMode>k__BackingField = value;
            }
        }

        public static bool EnableMARS
        {
            get
            {
                return (_enableMars || UserOptions.Instance.MARS);
            }
        }

        public static bool FreshAppDomains
        {
            get
            {
                return (_freshAppDomains || UserOptions.Instance.FreshAppDomains);
            }
        }

        public static int MajorMinorVersion
        {
            get
            {
                PopulateVersion();
                return _majorMinorVersion;
            }
        }

        public static bool MTAMode
        {
            get
            {
                return (_mtaMode || UserOptions.Instance.MTAThreadingMode);
            }
        }

        public static bool PresentationMode
        {
            get
            {
                return (_presentationMode || UserOptions.Instance.PresentationMode);
            }
        }

        public static bool PreserveAppDomains
        {
            get
            {
                return (_preserveAppDomains || UserOptions.Instance.PreserveAppDomains);
            }
        }

        public static string TempFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_tempFolder))
                {
                    _tempFolder = Path.Combine(TempFolderBase, AppInstanceID);
                    if (!Directory.Exists(_tempFolder))
                    {
                        try
                        {
                            Directory.CreateDirectory(_tempFolder);
                        }
                        catch
                        {
                            Thread.Sleep(100);
                            if (!Directory.Exists(_tempFolder))
                            {
                                Directory.CreateDirectory(_tempFolder);
                            }
                        }
                    }
                }
                return _tempFolder;
            }
        }

        public static System.Version Version
        {
            get
            {
                PopulateVersion();
                return _version;
            }
        }

        public static string VersionString
        {
            get
            {
                return string.Concat(new object[] { Version.Major, ".", Version.Minor.ToString("D2"), ".", Version.Build.ToString("D2") });
            }
        }
    }
}

