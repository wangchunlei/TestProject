using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadFile
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0) throw new Exception("输入文件名");
            var fileName = args[0];
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs,Encoding.Default))
                {
                    while (true)
                    {
                        while (!sr.EndOfStream)
                            Console.WriteLine(sr.ReadLine());
                        while (sr.EndOfStream)
                            Thread.Sleep(100);
                        //ProcessLinr(sr.ReadLine());
                    }
                }
            }

            // Wait for the user to quit the program.
            Console.WriteLine("Press \'q\' to quit the sample.");
            while (Console.Read() != 'q') ;
        }
    }
}
