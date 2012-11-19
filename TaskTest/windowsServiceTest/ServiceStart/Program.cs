using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using ServiceStart;

namespace ServiceStart
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                new Service1().Start(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                        {
                            new Service1()
                        };
                ServiceBase.Run(ServicesToRun);
            }

            Console.ReadKey(false);
        }
    }
}
