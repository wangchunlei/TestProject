using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var thread = new Thread(Go);
            thread.Start();
            thread.Join();
            var blocked = (thread.ThreadState & ThreadState.WaitSleepJoin) != 0;
            Console.WriteLine("thread run over");


            bool done = false;
            ThreadStart action = () =>
            {
                if (!done)
                {
                    done = true;
                    Console.WriteLine("Done");
                }
            };
            new Thread(action).Start();
            action();
            Console.ReadKey();
        }

        static void Go()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Yield();
                Console.Write("y");
            }
        }
    }
}
