using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                if (DateTime.Now.Hour < 21 || DateTime.Now.Hour >= 23)
                {
                    Button_Click(s, e);
                }
                else
                {
                    this.Close();
                }
            };
            this.webBrowser.Navigated += webBrowser_Navigated;
        }

        void webBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            Help.SetSilent(this.webBrowser, true); // make it silent
        }

        private void Login()
        {
            IHTMLDocument2 doc = (IHTMLDocument2)webBrowser.Document;
            doc.all.item("username").setAttribute("value", "wangcl2");
            doc.all.item("password").setAttribute("value", "314114");

            IHTMLWindow2 window = doc.parentWindow;
            window.execScript("login();");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    IHTMLDocument2 doc = (IHTMLDocument2)webBrowser.Document;

            //    IHTMLWindow2 window = doc.parentWindow;
            //    window.execScript("alert($('iframe').contentWindow)");
            //}
            //catch (Exception)
            //{

            //}


            this.webBrowser.LoadCompleted += (s, arg) =>
            {
                try
                {
                    Login();
                }
                catch (Exception)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(20));
                        this.Dispatcher.Invoke(() =>
                        {
                            this.Close();
                        });
                    });
                }
            };
            var timer = new System.Timers.Timer(TimeSpan.FromMinutes(Convert.ToInt32(this.check.Text)).TotalMilliseconds);
            timer.Elapsed += (s, er) =>
            {
                this.webBrowser.Dispatcher.Invoke(() =>
                {
                    this.webBrowser.Navigate(new Uri(@"http://dxz.37wan.com/ingame.php?server=S510&time=1381806598&tuitan=&source="));
                });
            };

            timer.Start();
            this.webBrowser.Navigate(new Uri(@"http://dxz.37wan.com/ingame.php?server=S510&time=1381806598&tuitan=&source="));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.webBrowser.Visibility = Visibility.Hidden;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.webBrowser.Visibility = Visibility.Visible;
        }
    }

    public class Help
    {
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }
}
