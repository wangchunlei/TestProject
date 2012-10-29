namespace LINQPad
{
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;

    public static class PowerPoint
    {
        internal static string Foo()
        {
            return "?";
        }

        public static string ShowSlide(string filename)
        {
            return ShowSlide(filename, 1);
        }

        public static string ShowSlide(string filename, ushort slideNumber)
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Applications\pptview.exe\shell\Show\command");
            if (key == null)
            {
                key = Registry.ClassesRoot.OpenSubKey(@"PowerPointViewer.Show.12\shell\Show\command");
            }
            if (key == null)
            {
                key = Registry.ClassesRoot.OpenSubKey(@"PowerPointViewer.Show.11\shell\Show\command");
            }
            if (key == null)
            {
                throw new Exception("Cannot find PowerPoint viewer entry in registry.");
            }
            string str = key.GetValue(null) as string;
            if (str == null)
            {
                throw new Exception("Cannot find PowerPoint viewer command in registry");
            }
            str = str.Replace("\"", "");
            int length = str.ToLowerInvariant().LastIndexOf(".exe", StringComparison.Ordinal);
            if (length < 10)
            {
                throw new Exception("Cannot parse PowerPoint viewer command in registry");
            }
            Process.Start(str.Substring(0, length), string.Concat(new object[] { "\"", filename, "\" /s /n", slideNumber }));
            return "";
        }
    }
}

