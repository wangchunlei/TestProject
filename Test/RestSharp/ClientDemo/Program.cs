using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace ClientDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RestClient("http://localhost/api/AddMonitorLogAPI/AddMonitorLog");
            client.Timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
            var request = new RestRequest(Method.POST);
            request.Timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
            var response = client.Execute(request);
        }
    }
}
