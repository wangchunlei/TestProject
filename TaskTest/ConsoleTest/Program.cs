using System;
using System.Collections.Generic;
using System.Text;
using ConsoleTest.INotify;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var foo = new Foo();
            foo.PropertyChanged += foo_PropertyChanged;
            foo.CustomerName = "12";

            Console.ReadKey(false);
        }

        static void foo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var arg = e as PropertyChangedArgs;
            Console.WriteLine(arg.PropertyName + " 变化,由{0}变成{1}", arg.OldValue, arg.NewValue);
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
