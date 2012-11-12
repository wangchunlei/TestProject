using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CheckProcessVisualMemory
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    int pid = 0;
                    while (true)
                    {
                        Console.WriteLine("请输入PID(输入a,显示所有进程)");
                        var input = Console.ReadLine();
                        if (input == "a")
                        {
                            double total = 0;
                            foreach (var pro in Process.GetProcesses())
                            {
                                total += ConsoleMemory(pro.Id);
                            }
                            Console.WriteLine("Total:{0}GB", total.ToString("0.00"));
                            break;
                        }
                        if (int.TryParse(input, out pid))
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine("输入有误");
                        }
                    }
                    ConsoleMemory(pid);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static double ConsoleMemory(int pid)
        {
            var process = Process.GetProcessById(pid);
            double memoryUsage = process.VirtualMemorySize64 / (double)(1024 * 1024 * 1024);
            double physicalMemory = process.WorkingSet64 / (double)(1024 * 1024 * 1024);
            Console.WriteLine(string.Format("进程：{0},PID:{1} 虚拟内存{2},物理内存{3}", process.ProcessName, pid, memoryUsage.ToString("0.00") + " GB", physicalMemory.ToString("0.00") + " GB"));
            return memoryUsage;
        }

        private void AllocateMemory(int maxtCount)
        {
            //try
            //{
            //    Console.WriteLine("按任意键分配{0}GB内存", maxtCount / 1024);
            //    Console.ReadKey(true);
            //    FileReadArray fileReadArray = new FileReadArray(maxtCount);
            //    iFileReadArrays.Add(fileReadArray);

            //    double memoryUsage = Process.GetCurrentProcess().PeakVirtualMemorySize64 / (double)(1024 * 1024 * 1024);
            //    Console.WriteLine(string.Format("当前虚拟内存使用情况{0}", memoryUsage.ToString("0.00") + " GB"));
            //    Console.WriteLine();

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.GetBaseException());
            //    throw;
            //}
        }
    }
}
