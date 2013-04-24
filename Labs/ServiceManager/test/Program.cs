using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var name = Console.ReadLine();
                var sh = new ServiceManager.ServiceHelper(name);
                sh.ServiceStatusChanged += sh_ServiceStatusChanged;
                sh.Start();
            }
        }

        static void sh_ServiceStatusChanged(ServiceManager.ServiceStatus arg1, object arg2)
        {
            Console.WriteLine("{0}---{1}", arg1.ToString(), arg2);
        }
    }
}
