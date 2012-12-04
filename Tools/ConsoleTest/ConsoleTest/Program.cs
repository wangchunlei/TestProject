using System;
using System.ServiceProcess;

namespace ServiceMonitor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var myService = new ServiceController();
            myService.ServiceName = "BingImages";

            Console.WriteLine(myService.Status.ToString());
            var tasks = new System.Threading.Tasks.Task[2];
            tasks[0] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        myService.WaitForStatus(ServiceControllerStatus.Stopped);
                        Console.WriteLine("服务停止，现在开始启动...");
                        myService.Start();
                        Console.WriteLine("服务启动完成.");
                    }
                });
            tasks[1] = System.Threading.Tasks.Task.Factory.StartNew(() =>
           {
               while (true)
               {
                   myService.WaitForStatus(ServiceControllerStatus.Paused);
                   Console.WriteLine("服务暂停，现在开始启动...");
                   myService.Start();
                   Console.WriteLine("服务启动完成.");
               }
           });

            System.Threading.Tasks.Task.WaitAll(tasks);
        }

    }
}
