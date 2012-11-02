using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace DownloadBingImage
{
    public partial class DownloadBingImageService : ServiceBase
    {
        private Timer timer;
        public DownloadBingImageService()
        {
            InitializeComponent();

            log = Domas.DAP.ADF.LogManager.LogManager.GetLogger("Service");
        }

        private Domas.DAP.ADF.LogManager.ILogger log = null;
        public void Start(string[] args)
        {
            OnStart(args);
        }
        protected override void OnStart(string[] args)
        {
            var interval = System.Configuration.ConfigurationSettings.AppSettings["interval"];
            int intervalMin = int.MaxValue;
            int.TryParse(interval, out intervalMin);
            log.Debug("服务启动，下次下载" + intervalMin + "分钟后");
            NetworkChange.NetworkAvailabilityChanged += (o, eventArgs) => log.Debug(string.Format("网络发生变化,当前网络{0}可用", eventArgs.IsAvailable ? "" : "不"));
            NetworkChange.NetworkAddressChanged += (o, eventArgs) => log.Debug("IP地址发生变化");
            timer = new Timer(state =>
                {
                    if (IsNetworkAvailable())
                    {
                        log.Debug(string.Format("开始下载"));
                        BingImages.DownLoadImages();
                    }
                    else
                    {
                        log.Debug("当前网络不可用，等待网络连接");
                    }
                }, null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMinutes(intervalMin));
        }

        private bool IsNetworkAvailable()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }
        protected override void OnStop()
        {
            timer.Dispose();
        }
    }
}
