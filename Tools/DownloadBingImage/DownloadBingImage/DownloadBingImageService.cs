using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
            
        }

        protected override void OnStart(string[] args)
        {
            var interval = System.Configuration.ConfigurationSettings.AppSettings["interval"];
            int intervalMin = 0;
            int.TryParse(interval, out intervalMin);
            Timer timer = new Timer(delegate
            {
                BingImages.DownLoadImages();
            }, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMinutes(intervalMin));
        }

        protected override void OnStop()
        {
            timer.Dispose();
        }
    }
}
