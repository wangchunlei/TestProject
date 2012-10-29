namespace LINQPad
{
    using System;
    using System.Diagnostics;
    using System.IO;

    internal static class Log
    {
        private static string LogFile = Path.Combine(LogsFolder, "log.txt");
        public static readonly string LogsFolder = Path.Combine(Program.LocalUserDataFolder, "Logs");

        public static void OpenLogFile()
        {
            if (File.Exists(LogFile))
            {
                Process.Start(LogFile);
            }
        }

        public static void Write(Exception ex)
        {
            Write(ex, null);
        }

        public static void Write(string msg)
        {
            try
            {
                if (!Directory.Exists(LogsFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(LogsFolder);
                    }
                    catch
                    {
                        return;
                    }
                }
                File.AppendAllText(LogFile, Program.VersionString + " " + DateTime.Now.ToString("o") + " " + msg.Trim() + "\r\n\r\n");
            }
            catch
            {
            }
        }

        public static void Write(Exception ex, string context)
        {
            try
            {
                string msg = ex.GetType().Name + ": " + ex.Message + "\r\n" + ex.StackTrace;
                if (ex.InnerException != null)
                {
                    string str2 = msg;
                    msg = str2 + "\r\nINNER: " + ex.InnerException.GetType().Name + ex.InnerException.Message + ex.InnerException.StackTrace.Replace("\n", "\n  ");
                }
                if (!string.IsNullOrEmpty(context))
                {
                    msg = context + " - \r\n" + msg;
                }
                Write(msg);
            }
            catch
            {
            }
        }
    }
}

