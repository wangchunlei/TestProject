using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace Novotny.IECookieHelper.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            using (CookieManager cm = new CookieManager())
            {
                Uri req = new Uri("http://192.168.20.222:8088");
                CookieContainer container = cm.GetCookieContainerUri(req);

                PrintCookies(container, req);

                Console.ReadKey();
            }
        }

        private static void PrintCookies(CookieContainer container, Uri req)
        {
            var cookies = container.GetCookies(req);

            if (cookies.Count == 0)
            {
                Console.WriteLine("No Cookies found");
            }

            foreach (Cookie cookie in container.GetCookies(req))
                Console.WriteLine("Cookie: {0}, Val: {1}", cookie.Name, cookie.Value);
        }



        
    }
}
