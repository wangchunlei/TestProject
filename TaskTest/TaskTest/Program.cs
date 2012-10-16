using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (send, arg) => Console.WriteLine("出错了");
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            try
            {
                var thread = new Thread(Go);
                thread.Start();
                //thread.Join();
                //var blocked = (thread.ThreadState & ThreadState.WaitSleepJoin) != 0;
                //Console.WriteLine("thread run over");
            }
            catch (Exception exception)
            {
                //永远也不会走到这里
                Console.WriteLine(exception.Message);
            }


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

            var text = "t1";
            var t1 = new Thread(() => Console.WriteLine(text));
            text = "t2";
            var t2 = new Thread(() => Console.WriteLine(text));
            t1.Start();
            t2.Start();


            var worker = new Thread(() => Console.ReadLine());
            worker.IsBackground = true;
            worker.Start();

            var signal = new ManualResetEvent(false);
            new Thread(() =>
                {
                    Console.WriteLine("Waiting for signal......");
                    signal.WaitOne();
                    signal.Dispose();
                    Console.WriteLine("Got signal!");
                }).Start();

            Thread.Sleep(2000);
            signal.Set();

            new Thread(Work).Start();

            Task.Run(() => Work());

            ThreadPool.QueueUserWorkItem(notUsed => Work());

            Task<int> primeNumberTask = Task.Run(() =>
                    Enumerable.Range(2, 3000000).Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
                );
            Console.WriteLine("Task is runing");
            var awaiter = primeNumberTask.GetAwaiter();
            awaiter.OnCompleted(() =>
                {
                    int result = awaiter.GetResult();
                    Console.WriteLine(result);
                });
            Console.WriteLine("the answer is :");

            Task.Factory.StartNew(() => Console.WriteLine("123"), TaskCreationOptions.LongRunning);

            var awaiters = GetAnswerToLife().GetAwaiter();
            awaiters.OnCompleted(() => Console.WriteLine(awaiters.GetResult()));

            DisplayPrimesCount();
            Console.WriteLine("2012年9月20日 17:29:36");
            Console.ReadKey();
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine("又出错了");
        }
        static async void DisplayPrimesCount()
        {
             Console.WriteLine("2012年9月20日 17:26:07");
            int res = await GetPrimesCountAsync(2, 1000000);
            Console.WriteLine(res);
        }
        static Task<int> GetPrimesCountAsync(int start, int count)
        {
            return Task.Run(() =>

                            ParallelEnumerable.Range(start, count).Count(n =>

                                                                         Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All
                                                                             (i => n % i > 0)));
        }
        static Task<int> GetAnswerToLife()
        {
            var tcs = new TaskCompletionSource<int>();
            var timer = new System.Timers.Timer(5000) { AutoReset = false };
            timer.Elapsed += delegate
                {
                    timer.Dispose();
                    tcs.SetResult(42);
                };
            timer.Start();
            return tcs.Task;
        }
        private static SynchronizationContext context = new SynchronizationContext();

        static void Go()
        {
            //throw null;
            for (int i = 0; i < 10; i++)
            {
                Thread.Yield();
                Console.Write("y");
            }
        }
        static void Work()
        {
            Thread.Sleep(5000);
            UpdateConsole("The answer");
        }
        static void UpdateConsole(string msg)
        {
            context.Post(_ => Console.WriteLine(msg), null);
        }
    }
}
