using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace HttpProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            //var port = args[0];
            int port = 8087;

            var proxy = HttpListenerProxy.Create(port);
            proxy.Start();
            Console.ReadKey(false);
            //proxy.Stop();
            //TcpListenerProxy.RunListener(IPAddress.Parse("192.168.70.118"), 8087);
            //HttpServer httpServer = new MyHttpServer(portNo);

            //Thread thread = new Thread(new ThreadStart(httpServer.listen));
            //thread.Start();
        }
    }
}
