using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("abc");
            Get();
            Console.ReadKey(false);
        }
        private static string Get()
        {
            string a = "aaa";
            int.Parse(a);
            return a;
        }
    }
}
