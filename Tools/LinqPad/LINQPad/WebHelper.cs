namespace LINQPad
{
    using LINQPad.Properties;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Windows.Forms;

    internal static class WebHelper
    {
        public static string EscapeJavaScript(string s)
        {
            return s.Replace("'", @"\'").Replace("\r", "\\\r").Replace(@"\", @"\\");
        }

        public static WebClient GetBackupWebClient()
        {
            return new WebClient();
        }

        public static WebClient GetFastWebClient()
        {
            try
            {
                if (ProxyOptions.Instance.IsValidProxy())
                {
                    return GetProxiedWebClient();
                }
            }
            catch
            {
            }
            return new WebClient { Proxy = null };
        }

        public static WebClient GetProxiedWebClient()
        {
            try
            {
                return ProxyOptions.Instance.GetWebClient();
            }
            catch
            {
                return new WebClient();
            }
        }

        public static WebClient GetWebClient()
        {
            try
            {
                if (ProxyOptions.Instance.IsValidProxy())
                {
                    return GetProxiedWebClient();
                }
            }
            catch
            {
            }
            return new WebClient();
        }

        public static string HtmlEncode(string value)
        {
            return WebUtility.HtmlEncode(value);
        }

        public static void LaunchBrowser(string uri)
        {
            try
            {
                Process.Start(uri);
            }
            catch
            {
            }
            return;
            try
            {
                new Process { StartInfo = { FileName = "iexplore.exe", Arguments = uri } }.Start();
            }
            catch
            {
            }
            return;
            WebBrowser browser = new WebBrowser {
                Dock = DockStyle.Fill
            };
            Form form = new Form();
            if (Screen.PrimaryScreen.WorkingArea.Width < 0x3e8)
            {
                form.WindowState = FormWindowState.Maximized;
            }
            else
            {
                form.Width = 0x3e8;
                form.Height = 720;
                form.StartPosition = FormStartPosition.CenterScreen;
            }
            form.Text = uri;
            form.Controls.Add(browser);
            form.Icon = Resources.LINQPad;
            browser.Navigate(uri);
            form.Show();
        }
    }
}

