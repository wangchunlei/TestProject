using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace await4.net4
{
    class Program
    {
        static void Main(string[] args)
        {
            var pro = new Program();
            pro.AsyncResponsiveCPU();
            OpenReadWriteFileAsync("MyFileAsync.txt");
            Console.WriteLine(123);
            HttpClientTest();
            Console.WriteLine("1");
            Console.ReadKey();
        }
        static async void HttpClientTest()
        {
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://www.contoso.com/");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine(responseBody);
        }
        static async void OpenReadWriteFileAsync(string fileName)
        {
            byte[] buffer = null;
            try
            {
                using (Stream streamRead = new FileStream(@fileName, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[streamRead.Length];

                    Console.WriteLine("In Read Operation Async");
                    Task readData = streamRead.ReadAsync(buffer, 0, (int)streamRead.Length);
                    await readData;
                }

                using (Stream streamWrite = new FileStream(@"MyFileAsync(bak).txt", FileMode.Create, FileAccess.Write))
                {
                    Console.WriteLine("In Write Operation Async ");
                    Task writeData = streamWrite.WriteAsync(buffer, 0, buffer.Length);
                    await writeData;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task AsyncResponsiveCPU()
        {
            Console.WriteLine("Processing data...  Drag the window around or scroll the tree!");
            Console.WriteLine();
            int[] data = await ProcessDataAsync(16, 16);
            Console.WriteLine();
            Console.WriteLine("Processing complete.");
        }

        public Task<int[]> ProcessDataAsync(int width, int height)
        {
            return TaskEx.Run(() =>
            {
                var result = new int[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Thread.Sleep(10);   // simulate processing cell [x,y]
                    }
                    Console.WriteLine("Processed row {0}", y);
                }

                return result;
            });
        }
    }
}
