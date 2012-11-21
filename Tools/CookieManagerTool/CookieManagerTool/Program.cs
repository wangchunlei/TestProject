using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CookieManagerTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://192.168.20.222:8088");

            System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var cookies = CookieManger.GetUriCookieContainer(uri);

                        Console.WriteLine(cookies.GetCookies(uri)[0].Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.GetBaseException());
                    }
                });
            Console.WriteLine("123");
            Console.ReadKey(false);
        }
    }
}
