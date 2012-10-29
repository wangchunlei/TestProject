namespace LINQPad
{
    using ActiproBridge;
    using LINQPad.UI;
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    internal class ReflectorAgent
    {
        private static string _reflectorPath;

        public static void ActivateReflector(MemberHelpInfo hi)
        {
            string assemblyPath = hi.get_AssemblyLocation();
            if (ReflectorPath == null)
            {
                MessageBox.Show("Unable to invoke .NET Reflector (is it installed?)", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                if (!IsReflectorRunning())
                {
                    WorkingForm.FlashForm("Working...", 0x5dc);
                }
                new Thread(delegate {
                    try
                    {
                        if (((!string.IsNullOrEmpty(assemblyPath) && File.Exists(assemblyPath)) && !assemblyPath.ToLowerInvariant().Contains(@"\microsoft\framework\")) && !assemblyPath.ToLowerInvariant().Contains(@"\gac_"))
                        {
                            bool flag;
                            string arguments = "/share \"" + assemblyPath + "\"";
                            ProcessStartInfo startInfo = new ProcessStartInfo(ReflectorPath, arguments) {
                                WorkingDirectory = Path.GetDirectoryName(ReflectorPath)
                            };
                            if (flag = IsReflectorRunning())
                            {
                                ShowReflectorWindow();
                            }
                            Process process = Process.Start(startInfo);
                            try
                            {
                                if (!flag)
                                {
                                    if (!process.WaitForInputIdle(0x3a98))
                                    {
                                        return;
                                    }
                                    Thread.Sleep(200);
                                }
                                else if (!process.WaitForExit(0x3a98))
                                {
                                    return;
                                }
                            }
                            catch
                            {
                            }
                            finally
                            {
                                if (process != null)
                                {
                                    process.Dispose();
                                }
                            }
                        }
                        NavigateReflector(hi.get_ReflectorCodeUri());
                        Thread.Sleep(100);
                        ShowReflectorWindow();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Unable to invoke .NET Reflector: " + exception.Message);
                    }
                }) { Name = "Reflector Invoker", IsBackground = true }.Start();
            }
        }

        private static bool IsReflectorRunning()
        {
            try
            {
                Process[] processesByName = Process.GetProcessesByName("Reflector");
                if (processesByName.Count<Process>() == 0)
                {
                    return false;
                }
                Process[] processArray2 = processesByName;
                int index = 0;
                while (true)
                {
                    if (index >= processArray2.Length)
                    {
                        break;
                    }
                    Process process = processArray2[index];
                    try
                    {
                        process.Dispose();
                    }
                    catch
                    {
                    }
                    index++;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void NavigateReflector(string codeUri)
        {
            if (!string.IsNullOrEmpty(codeUri))
            {
                ShowReflectorWindow();
                ProcessStartInfo startInfo = new ProcessStartInfo(ReflectorPath, "/share /select:" + codeUri) {
                    WorkingDirectory = Path.GetDirectoryName(ReflectorPath)
                };
                using (Process.Start(startInfo))
                {
                }
            }
        }

        private static void ShowReflectorWindow()
        {
            try
            {
                Process[] processesByName = Process.GetProcessesByName("Reflector");
                if (processesByName.Count<Process>() == 1)
                {
                    IntPtr mainWindowHandle = processesByName[0].MainWindowHandle;
                    if (Native.IsIconic(mainWindowHandle))
                    {
                        Native.ShowWindow(mainWindowHandle, 9);
                    }
                    Native.SetForegroundWindow(mainWindowHandle);
                }
                foreach (Process process in processesByName)
                {
                    try
                    {
                        process.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public static string ReflectorPath
        {
            get
            {
                if (_reflectorPath == null)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reflector.exe");
                    if (File.Exists(path))
                    {
                        return (_reflectorPath = path);
                    }
                    RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"code\shell\open\command");
                    if (key == null)
                    {
                        key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\code\shell\open\command");
                    }
                    if (key == null)
                    {
                        key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\Reflector.exe\shell\open\command");
                    }
                    if (key == null)
                    {
                        return null;
                    }
                    string str3 = key.GetValue(null) as string;
                    if (!(((str3 != null) && (str3.Length >= 5)) && str3.Substring(1).Contains("\"")))
                    {
                        return null;
                    }
                    str3 = str3.Substring(1, str3.IndexOf("\"", 1) - 1);
                    if (File.Exists(str3))
                    {
                        _reflectorPath = str3;
                    }
                }
                return _reflectorPath;
            }
        }
    }
}

