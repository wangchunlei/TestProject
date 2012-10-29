using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            String s = new String();

            Console.WriteLine(s.s);
            s.Console(a: "123", b: 0x1);
            Console.ReadKey(true);
        }
    }

    class @String
    {
        public string s = "hello world";

        public void Console(string a, int b)
        {
            System.Console.WriteLine(a + b);
        }
    }
}
