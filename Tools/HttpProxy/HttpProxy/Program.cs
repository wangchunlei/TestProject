using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            //var port = args[0];
            int portNo = 8099;
            //if (!string.IsNullOrEmpty(port))
            //{
            //    portNo = int.Parse(port);
            //}
            var proxy = HttpListenerProxy.Create(portNo);
            proxy.Start();
            Console.ReadKey(false);
            proxy.Stop();
        }
    }
}
