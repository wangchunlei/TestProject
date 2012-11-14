using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadBingImage
{
    static class Program
    {
        private static Domas.DAP.ADF.LogManager.ILogger log = Domas.DAP.ADF.LogManager.LogManager.GetLogger("Error Log");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) => log.Error((Exception)e.ExceptionObject);
            if (Environment.UserInteractive)
            {
                var dbis = new DownloadBingImageService();
                dbis.Start(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                        {
                            new DownloadBingImageService()
                        };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static Task LoopTask()
        {
            var t = Task.Factory.StartNew(delegate
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                        {
                            new DownloadBingImageService()
                        };
                    ServiceBase.Run(ServicesToRun);
                });
            return t;
        }

        private static Thread LoopThread()
        {
            var t = new Thread(() =>
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                        {
                            new DownloadBingImageService()
                        };
                    ServiceBase.Run(ServicesToRun);
                });
            return t;
        }
    }
}
