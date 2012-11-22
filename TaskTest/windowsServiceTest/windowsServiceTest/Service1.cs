using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace ServiceStart
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer timer = null;
        public Service1()
        {
            InitializeComponent();
        }
        public void Start(string[] args)
        {
            OnStart(args);
        }
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool WTSQueryUserToken(int sessionId, out IntPtr tokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint WTSGetActiveConsoleSessionId();
        protected override void OnStart(string[] args)
        {
            var log = Domas.DAP.ADF.LogManager.LogManager.GetLogger("cookie");


            //var proxy = new Domas.Service.Print.PrintTask.POProxy();
            //proxy.GetPrintSetting("P2012111400029");
            //proxy.FindByBusinessKey("P2012111400029");
            //timer = new Timer(1000 * 10);
            //timer.Enabled = true;
            //timer.Elapsed += (sender, e) =>
            //    {
            //        try
            //        {
            //            Domas.DAP.ADF.Cookie.CookieManger.Impersonate(() =>
            //                {
            //                    var cookie = Domas.DAP.ADF.Cookie.CookieManger.GetUriCookieContainer(new Uri("http://127.0.0.1"));
            //                    var c = cookie.GetCookies(new Uri("http://127.0.0.1"))[0];
            //                    var u = Domas.DAP.ADF.Cookie.CookieManger.DecryCookie(c.Value);

            //                    log.Debug(u.UserID + "," + u.UserCode + "," + u.UserName);

            //                });




            //            Domas.DAP.ADF.NotifierClient.NotifierClient.Start("http://127.0.0.1", (ex) =>
            //                {

            //                });


            //        }
            //        catch (Exception ex)
            //        {
            //            log.Error(ex.GetBaseException());
            //        }
            //    };
            try
            {
                Domas.DAP.ADF.Cookie.CookieManger.Impersonate(() =>
                    {
                        Process.Start("notepad.exe");
                    });

            }
            catch (Exception ex)
            {
                log.Error(ex.GetBaseException());
            }

        }

        protected override void OnStop()
        {

        }
    }
}
