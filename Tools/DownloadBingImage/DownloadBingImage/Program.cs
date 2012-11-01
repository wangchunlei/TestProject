using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DownloadBingImage
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
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
    }
}
