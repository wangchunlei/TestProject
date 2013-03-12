using System;
using System.Net;
using System.ServiceProcess;

namespace ServiceMonitor
{
    internal class Program
    {
        //private static void Main(string[] args)
        //{
        //    var myService = new ServiceController();
        //    myService.ServiceName = "BingImages";

        //    Console.WriteLine(myService.Status.ToString());
        //    var tasks = new System.Threading.Tasks.Task[2];
        //    tasks[0] = System.Threading.Tasks.Task.Factory.StartNew(() =>
        //        {
        //            while (true)
        //            {
        //                myService.WaitForStatus(ServiceControllerStatus.Stopped);
        //                Console.WriteLine("服务停止，现在开始启动...");
        //                myService.Start();
        //                Console.WriteLine("服务启动完成.");
        //            }
        //        });
        //    tasks[1] = System.Threading.Tasks.Task.Factory.StartNew(() =>
        //   {
        //       while (true)
        //       {
        //           myService.WaitForStatus(ServiceControllerStatus.Paused);
        //           Console.WriteLine("服务暂停，现在开始启动...");
        //           myService.Start();
        //           Console.WriteLine("服务启动完成.");
        //       }
        //   });

        //    System.Threading.Tasks.Task.WaitAll(tasks);
        //}
        public static void Main(string[] args)
        {
            args = new string[1] { "http://192.168.20.206:8099/notifier/list/admin/" };
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("Specify the URL to receive the request.");
                Environment.Exit(1);
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(args[0]);
            request.CookieContainer = new CookieContainer();

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // Print the properties of each cookie. 
                foreach (Cookie cook in response.Cookies)
                {
                    Console.WriteLine("Cookie:");
                    Console.WriteLine("{0} = {1}", cook.Name, cook.Value);
                    Console.WriteLine("Domain: {0}", cook.Domain);
                    Console.WriteLine("Path: {0}", cook.Path);
                    Console.WriteLine("Port: {0}", cook.Port);
                    Console.WriteLine("Secure: {0}", cook.Secure);

                    Console.WriteLine("When issued: {0}", cook.TimeStamp);
                    Console.WriteLine("Expires: {0} (expired? {1})",
                        cook.Expires, cook.Expired);
                    Console.WriteLine("Don't save: {0}", cook.Discard);
                    Console.WriteLine("Comment: {0}", cook.Comment);
                    Console.WriteLine("Uri for comments: {0}", cook.CommentUri);
                    Console.WriteLine("Version: RFC {0}", cook.Version == 1 ? "2109" : "2965");

                    // Show the string representation of the cookie.
                    Console.WriteLine("String: {0}", cook.ToString());
                }
            }
            
        }
    }
}
