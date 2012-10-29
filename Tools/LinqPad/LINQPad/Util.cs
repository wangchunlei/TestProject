namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using LINQPad.ObjectGraph;
    using LINQPad.ObjectGraph.Formatters;
    using LINQPad.ObjectModel;
    using LINQPad.Properties;
    using LINQPad.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Linq;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public static class Util
    {
        private static int? _progressPercent;
        internal static volatile Process CurrentExternalProcess;

        public static T Cache<T>(Func<T> dataFetcher)
        {
            return UserCache.CacheValue<T>(dataFetcher, null);
        }

        public static T Cache<T>(Func<T> dataFetcher, string key)
        {
            return UserCache.CacheValue<T>(dataFetcher, key);
        }

        public static void ClearResults()
        {
            Server currentServer = Server.CurrentServer;
            if (currentServer != null)
            {
                currentServer.ResultsWriter.Clear();
            }
        }

        public static string[] Cmd(string commandText)
        {
            return Cmd(commandText, false);
        }

        public static string[] Cmd(string commandText, bool quiet)
        {
            return Cmd(commandText, null, quiet);
        }

        public static string[] Cmd(string commandText, string args)
        {
            return Cmd(commandText, args, false);
        }

        public static string[] Cmd(string commandText, string args, bool quiet)
        {
            Process process;
            List<string> list = new List<string>();
            try
            {
                process = GetCmdProcess(commandText, args, false);
            }
            catch (Win32Exception)
            {
                process = GetCmdProcess(commandText, args, true);
            }
            CurrentExternalProcess = process;
            using (process)
            {
                try
                {
                    string str2;
                    goto Label_003C;
                Label_002A:
                    if (!quiet)
                    {
                        Console.WriteLine(str2);
                    }
                    list.Add(str2);
                Label_003C:
                    str2 = process.StandardOutput.ReadLine();
                    if (str2 != null)
                    {
                        goto Label_002A;
                    }
                    string str = process.StandardError.ReadToEnd();
                    int exitCode = process.ExitCode;
                    if (!string.IsNullOrEmpty(str))
                    {
                        throw new CommandExecutionException(str, exitCode);
                    }
                    if (exitCode > 0)
                    {
                        throw new CommandExecutionException(null, exitCode);
                    }
                }
                finally
                {
                    CurrentExternalProcess = null;
                    if (!process.HasExited)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return list.ToArray();
        }

        public static AppDomain CreateAppDomain(string friendlyName)
        {
            return CreateAppDomain(friendlyName, AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
        }

        public static AppDomain CreateAppDomain(string friendlyName, Evidence securityInfo)
        {
            return CreateAppDomain(friendlyName, securityInfo, AppDomain.CurrentDomain.SetupInformation);
        }

        public static AppDomain CreateAppDomain(string friendlyName, Evidence securityInfo, AppDomainSetup setup)
        {
            AppDomain domain = AppDomain.CreateDomain(friendlyName, securityInfo, setup);
            domain.SetData("CurrentQueryAssemblyPath", Server.CurrentQueryAssemblyPath);
            domain.SetData("CurrentQueryAdditionalRefs", Server.CurrentQueryAdditionalRefs);
            domain.AssemblyResolve += new ResolveEventHandler(Util.domain_AssemblyResolve);
            return domain;
        }

        public static void CreateSynchronizationContext()
        {
            CreateSynchronizationContext(true);
        }

        public static void CreateSynchronizationContext(bool detectDeadlocks)
        {
            CreateSynchronizationContext(detectDeadlocks, false);
        }

        public static void CreateSynchronizationContext(bool detectDeadlocks, bool reportActivity)
        {
            if (!(AsyncOperationManager.SynchronizationContext is LINQPadSynchronizationContext))
            {
                AsyncOperationManager.SynchronizationContext = new LINQPadSynchronizationContext(detectDeadlocks, reportActivity);
                if (reportActivity)
                {
                    Highlight("Synchronization Context created on thread " + Thread.CurrentThread.ManagedThreadId).Dump<object>();
                }
            }
        }

        public static TextWriter CreateXhtmlWriter()
        {
            return new XhtmlWriter(false, false);
        }

        public static TextWriter CreateXhtmlWriter(bool enableExpansions)
        {
            return new XhtmlWriter(enableExpansions, false);
        }

        public static TextWriter CreateXhtmlWriter(int maxDepth)
        {
            return new XhtmlWriter(false, false) { MaxDepth = new int?(maxDepth) };
        }

        public static TextWriter CreateXhtmlWriter(bool enableExpansions, int maxDepth)
        {
            return new XhtmlWriter(enableExpansions, false) { MaxDepth = new int?(maxDepth) };
        }

        public static void DisplayWebPage(string uriOrPath)
        {
            DisplayWebPage(uriOrPath, null);
        }

        public static void DisplayWebPage(Uri uri)
        {
            DisplayWebPage(uri.ToString(), null);
        }

        public static void DisplayWebPage(string uriOrPath, string title)
        {
            WebBrowser browser;
            ToolStrip tools;
            ToolStripItem backButton;
            ToolStripItem forwardButton;
            if (!string.IsNullOrEmpty(uriOrPath))
            {
                if (string.IsNullOrEmpty(title))
                {
                    title = "Web Page";
                    try
                    {
                        Uri uri = new Uri(uriOrPath);
                        if (uri.Segments.Any<string>() && (uri.Segments.Last<string>().Length > 2))
                        {
                            title = uri.Segments.Last<string>();
                        }
                        else if (uri.Host.Length > 2)
                        {
                            title = uri.Host;
                        }
                        else
                        {
                            title = uri.ToString();
                        }
                    }
                    catch
                    {
                    }
                }
                Panel o = new Panel();
                browser = new WebBrowser {
                    Dock = DockStyle.Fill
                };
                tools = new ToolStrip {
                    Dock = DockStyle.Top,
                    GripStyle = ToolStripGripStyle.Hidden,
                    Visible = false,
                    Padding = new Padding(0, 0, 0, 1)
                };
                backButton = tools.Items.Add("Back", Resources.Back, delegate (object sender, EventArgs e) {
                    if (browser.CanGoBack)
                    {
                        browser.GoBack();
                    }
                });
                forwardButton = tools.Items.Add("Forward", Resources.Forward, delegate (object sender, EventArgs e) {
                    if (browser.CanGoForward)
                    {
                        browser.GoForward();
                    }
                });
                backButton.Margin = new Padding(2, 0, 0, 0);
                EventHandler handler = delegate (object sender, EventArgs e) {
                    backButton.Enabled = browser.CanGoBack;
                    forwardButton.Enabled = browser.CanGoForward;
                    tools.Visible = browser.CanGoBack || browser.CanGoForward;
                };
                browser.CanGoBackChanged += handler;
                browser.CanGoForwardChanged += handler;
                o.Controls.Add(browser);
                o.Controls.Add(tools);
                browser.Navigate(uriOrPath);
                o.Dump<Panel>(title);
            }
        }

        public static void DisplayWebPage(Uri uri, string title)
        {
            DisplayWebPage(uri.ToString(), title);
        }

        private static Assembly domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Func<string, bool> predicate = null;
            string simpleName = new AssemblyName(args.Name).Name.ToLowerInvariant();
            string[] data = (string[]) AppDomain.CurrentDomain.GetData("CurrentQueryAdditionalRefs");
            string assemblyFile = (string) AppDomain.CurrentDomain.GetData("CurrentQueryAssemblyPath");
            if (data != null)
            {
                if (predicate == null)
                {
                    predicate = r => Path.GetFileNameWithoutExtension(r).ToLowerInvariant() == simpleName;
                }
                string str2 = data.FirstOrDefault<string>(predicate);
                if (str2 != null)
                {
                    return Assembly.LoadFrom(str2);
                }
            }
            if ((assemblyFile != null) && assemblyFile.ToLowerInvariant().Contains(simpleName))
            {
                return Assembly.LoadFrom(assemblyFile);
            }
            return null;
        }

        internal static IEnumerable<T> Flatten<T>(T element, Func<T, IEnumerable<T>> childSelector)
        {
            return new T[] { element }.Concat<T>((from child in childSelector(element) select Flatten<T>(child, childSelector)));
        }

        internal static IEnumerable<T> Flatten<T>(T element, Func<T, T> childSelector)
        {
            return new T[] { element }.Concat<T>((from child in new T[] { childSelector(element) } select Flatten<T>(child, childSelector)));
        }

        private static Process GetCmdProcess(string cmdText, string args, bool useCmdExec)
        {
            cmdText = cmdText.Trim();
            ProcessStartInfo startInfo = new ProcessStartInfo {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            if (useCmdExec)
            {
                startInfo.FileName = "cmd.exe";
                string source = cmdText;
                if (source.Contains<char>(' '))
                {
                    source = '"' + source + '"';
                }
                startInfo.Arguments = "/c " + source;
                if (!string.IsNullOrEmpty(args))
                {
                    startInfo.Arguments = startInfo.Arguments + " " + args;
                }
            }
            else
            {
                startInfo.FileName = cmdText;
                startInfo.Arguments = args;
                if ((string.IsNullOrEmpty(args) && cmdText.Contains<char>(' ')) && !File.Exists(cmdText))
                {
                    string[] strArray = cmdText.Split(" ".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                    if (File.Exists(strArray[0]))
                    {
                        startInfo.FileName = strArray[0];
                        startInfo.Arguments = strArray[1];
                    }
                }
                try
                {
                    if (Path.IsPathRooted(startInfo.FileName))
                    {
                        string directoryName = Path.GetDirectoryName(startInfo.FileName);
                        if (Directory.Exists(directoryName))
                        {
                            startInfo.WorkingDirectory = directoryName;
                        }
                    }
                }
                catch (ArgumentException)
                {
                }
            }
            return Process.Start(startInfo);
        }

        public static IEnumerable<Query> GetMyQueries()
        {
            return FileQuery.GetMyQueries();
        }

        public static string GetPassword(string name)
        {
            return GetPassword(name, false);
        }

        public static string GetPassword(string name, bool noDefaultSave)
        {
            string password = PasswordManager.GetPassword(name);
            if (password == null)
            {
                using (SavePasswordForm form = new SavePasswordForm("Password for " + name, !noDefaultSave))
                {
                    if (form.ShowDialog() != DialogResult.OK)
                    {
                        return null;
                    }
                    password = form.Password;
                    if (form.SavePassword)
                    {
                        PasswordManager.SetPassword(name, password);
                    }
                }
            }
            return password;
        }

        public static IDisposable GetQueryLifeExtensionToken()
        {
            Server server = Server.CurrentServer;
            if (server != null)
            {
                server.QueryCompletionCountdown.Increment();
            }
            return new LifeExtensionToken(delegate {
                try
                {
                    if (server != null)
                    {
                        server.QueryCompletionCountdown.Decrement();
                    }
                }
                catch
                {
                }
            });
        }

        internal static PermissionSet GetQueryPermissions()
        {
            return GetQueryPermissions(false);
        }

        internal static PermissionSet GetQueryPermissions(bool userQuery)
        {
            return new PermissionSet(PermissionState.Unrestricted);
        }

        public static string GetSampleFilePath(string sampleLibraryName, string fileName)
        {
            if (string.IsNullOrEmpty(sampleLibraryName))
            {
                throw new ArgumentException("You must specify a sample library name", "sampleLibraryName");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("You must specify a filename", "fileName");
            }
            return Path.Combine(GetSamplesFolder(sampleLibraryName), fileName);
        }

        public static IEnumerable<Query> GetSamples()
        {
            return Query.GetSamples();
        }

        public static string GetSamplesFolder()
        {
            return GetSamplesFolder(null);
        }

        public static string GetSamplesFolder(string sampleLibraryName)
        {
            if (string.IsNullOrEmpty(sampleLibraryName))
            {
                return Path.Combine(Program.UserDataFolder, "Samples");
            }
            return Path.Combine(Program.UserDataFolder, @"Samples\" + sampleLibraryName);
        }

        public static WebProxy GetWebProxy()
        {
            return ProxyOptions.Instance.GetWebProxy();
        }

        public static object Highlight(object data)
        {
            return new LINQPad.ObjectGraph.Highlight(data);
        }

        public static object HighlightIf<T>(T data, Func<T, bool> predicate)
        {
            if (predicate(data))
            {
                return new LINQPad.ObjectGraph.Highlight(data);
            }
            return data;
        }

        public static object HorizontalRun(bool withGaps, params object[] elements)
        {
            return new LINQPad.ObjectGraph.HorizontalRun(withGaps, elements);
        }

        public static object HorizontalRun(bool withGaps, IEnumerable elements)
        {
            return new LINQPad.ObjectGraph.HorizontalRun(withGaps, elements);
        }

        public static object Image(Binary imageData)
        {
            if (imageData == null)
            {
                return null;
            }
            byte[] data = imageData.ToArray();
            if ((data == null) || (data.Length == 0))
            {
                return null;
            }
            return new ImageBlob(data);
        }

        public static object Image(byte[] imageData)
        {
            return new ImageBlob(imageData);
        }

        public static object Image(string pathOrUri)
        {
            if (string.IsNullOrEmpty(pathOrUri))
            {
                return null;
            }
            return Image(new Uri(pathOrUri));
        }

        public static object Image(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }
            return new ImageRef(uri);
        }

        internal static bool IsMetaGraphNode(object x)
        {
            if (x == null)
            {
                return false;
            }
            return (x.GetType().GetCustomAttributes(typeof(MetaGraphNodeAttribute), true).Length > 0);
        }

        public static object Metatext(string text)
        {
            return new LINQPad.ObjectGraph.Metatext(text);
        }

        public static object RawHtml(string xhtml)
        {
            object obj2;
            try
            {
                obj2 = RawHtml(XElement.Parse(xhtml));
            }
            catch
            {
                try
                {
                    obj2 = RawHtml(XElement.Parse("<div>" + xhtml + "</div>"));
                }
                catch (Exception exception)
                {
                    obj2 = RawHtml(new XElement("i", "Cannot parse custom HTML: '" + xhtml + "' - '" + exception.Message));
                }
            }
            return obj2;
        }

        public static object RawHtml(XElement xhtmlNode)
        {
            return new LINQPad.ObjectGraph.RawHtml(xhtmlNode);
        }

        public static TResult ReadLine<TResult>()
        {
            return ReadLine<TResult>(null);
        }

        public static string ReadLine()
        {
            return ReadLine(null, null);
        }

        public static TResult ReadLine<TResult>(string prompt)
        {
            return TryReadLine<TResult>(prompt, false, default(TResult), null);
        }

        public static string ReadLine(string prompt)
        {
            return ReadLine(prompt, null);
        }

        public static TResult ReadLine<TResult>(string prompt, TResult defaultValue)
        {
            return TryReadLine<TResult>(prompt, true, defaultValue, null);
        }

        public static string ReadLine(string prompt, string defaultValue)
        {
            return ReadLine(prompt, defaultValue, null);
        }

        public static TResult ReadLine<TResult>(string prompt, TResult defaultValue, IEnumerable<TResult> suggestions)
        {
            return TryReadLine<TResult>(prompt, true, defaultValue, suggestions);
        }

        public static string ReadLine(string prompt, string defaultValue, IEnumerable<string> suggestions)
        {
            if (Server.CurrentServer == null)
            {
                return Console.ReadLine();
            }
            return Server.CurrentServer.ReadLine(prompt, defaultValue, suggestions);
        }

        public static string ReadSampleTextFile(string sampleLibraryName, string fileName)
        {
            return File.ReadAllText(GetSampleFilePath(sampleLibraryName, fileName));
        }

        public static void SetPassword(string name, string password)
        {
            PasswordManager.SetPassword(name, password);
        }

        private static TResult TryReadLine<TResult>(string prompt, bool hasDefault, TResult defaultValue, IEnumerable<TResult> suggestions)
        {
            // This item is obfuscated and can not be translated.
            Func<string, TResult> func2;
            string str2;
            Func<string, TResult> func = null;
            Func<TResult, string> toString;
            bool flag = false;
            Type t = typeof(TResult);
            if (flag = t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                t = t.GetGenericArguments()[0];
            }
            TypeConverter typeConverter = null;
            TypeCode typeCode = Type.GetTypeCode(t);
            if (t.IsEnum)
            {
                func2 = s => (TResult) Enum.Parse(t, s, true);
            }
            else if ((typeCode != TypeCode.Empty) && (typeCode != TypeCode.Object))
            {
                func2 = s => (TResult) Convert.ChangeType(s, t);
            }
            else if (t == typeof(TimeSpan))
            {
                func2 = s => (TResult) TimeSpan.Parse(s);
            }
            else if (t == typeof(DateTimeOffset))
            {
                func2 = s => (TResult) DateTimeOffset.Parse(s);
            }
            else
            {
                typeConverter = TypeDescriptor.GetConverter(t);
                if (!((typeConverter != null) && typeConverter.CanConvertFrom(typeof(string))))
                {
                    throw new InvalidOperationException("No type converter available to convert from string to " + t.FormatTypeName() + ".");
                }
                if (func == null)
                {
                    func = s => (TResult) typeConverter.ConvertFromString(s);
                }
                func2 = func;
            }
            if (typeConverter != null)
            {
                toString = x => typeConverter.ConvertToString(x);
            }
            else
            {
                toString = x => (x == null) ? null : x.ToString();
            }
            string str = null;
            if (hasDefault)
            {
                str = toString(defaultValue);
            }
            IEnumerable<string> names = null;
            if (suggestions != null)
            {
                names = (from c in suggestions
                    select toString(c) into s
                    where s != null
                    select s).Distinct<string>();
            }
            else if (t.IsEnum)
            {
                names = Enum.GetNames(t);
            }
            else if (t == typeof(bool))
            {
                names = "false true".Split(new char[0]);
            }
            goto Label_02A0;
        Label_0241:
            if (1 == 0)
            {
                return default(TResult);
            }
            try
            {
                return func2(str2);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Cannot convert from string to " + t.FormatTypeName() + ".\r\n" + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                goto Label_02A0;
            }
        Label_0283:;
            goto Label_0241;
        Label_02A0:
            str2 = str = ReadLine(prompt, str, names);
            if (!flag)
            {
                goto Label_0241;
            }
            goto Label_0283;
        }

        internal static void UnloadAppDomain(AppDomain d)
        {
            UnloadAppDomain(d, 3);
        }

        internal static void UnloadAppDomain(AppDomain d, int attempts)
        {
            for (int i = 0; i < attempts; i++)
            {
                if (i > 0)
                {
                    Thread.Sleep((int) (100 * i));
                }
                try
                {
                    AppDomain.Unload(d);
                    return;
                }
                catch (AppDomainUnloadedException)
                {
                    return;
                }
                catch (CannotUnloadAppDomainException)
                {
                }
            }
            Log.Write("Cannot unload app domain - terminal");
        }

        public static object VerticalRun(IEnumerable elements)
        {
            return new LINQPad.ObjectGraph.VerticalRun(elements);
        }

        public static object VerticalRun(params object[] elements)
        {
            return new LINQPad.ObjectGraph.VerticalRun(elements);
        }

        public static object WordRun(bool withGaps, params object[] elements)
        {
            return new LINQPad.ObjectGraph.WordRun(withGaps, elements);
        }

        public static object WordRun(bool withGaps, IEnumerable elements)
        {
            return new LINQPad.ObjectGraph.WordRun(withGaps, elements);
        }

        public static bool? AutoScrollResults
        {
            [CompilerGenerated]
            get
            {
                return <AutoScrollResults>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <AutoScrollResults>k__BackingField = value;
            }
        }

        internal static string CurrentQueryName
        {
            [CompilerGenerated]
            get
            {
                return <CurrentQueryName>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <CurrentQueryName>k__BackingField = value;
            }
        }

        public static string CurrentQueryPath
        {
            [CompilerGenerated]
            get
            {
                return <CurrentQueryPath>k__BackingField;
            }
            [CompilerGenerated]
            internal set
            {
                <CurrentQueryPath>k__BackingField = value;
            }
        }

        public static int? Progress
        {
            get
            {
                return _progressPercent;
            }
            set
            {
                if (_progressPercent != value)
                {
                    int? nullable3 = value;
                    if (nullable3.HasValue)
                    {
                        if (nullable3 < 0)
                        {
                            nullable3 = 0;
                        }
                        if (nullable3 > 100)
                        {
                            nullable3 = 100;
                        }
                    }
                    _progressPercent = nullable3;
                }
            }
        }

        public static TextWriter SqlOutputWriter
        {
            get
            {
                return LINQPadDbController.SqlLog;
            }
        }

        private class LifeExtensionToken : IDisposable
        {
            private Action _onDispose;

            internal LifeExtensionToken(Action onDispose)
            {
                this._onDispose = onDispose;
            }

            public void Dispose()
            {
                Action action = this._onDispose;
                this._onDispose = null;
                if (action != null)
                {
                    action();
                }
            }
        }
    }
}

