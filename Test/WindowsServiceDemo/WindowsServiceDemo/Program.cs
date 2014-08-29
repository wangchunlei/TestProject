using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var ser = new Service1();
            if (Environment.UserInteractive)
            {
                ser.Start(null);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    ser
                };
                ServiceBase.Run(ServicesToRun);
            }
            Console.ReadKey(false);
        }
    }
}
