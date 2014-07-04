using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();

            using (var client = new HttpClient(clientHandler))
            {
                client.BaseAddress = new Uri("http://www.baidu.com");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.GetStringAsync("").ContinueWith((content) =>
                {
                    Console.WriteLine(2);
                });
                Console.WriteLine(1);
                Console.ReadKey(false);
            }
        }
    }
}
