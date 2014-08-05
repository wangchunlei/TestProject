using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://192.168.70.118:1980";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server running at :" + url);
                Console.ReadLine();
            }
        }
    }
}
