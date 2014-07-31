using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.Transports;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace SignalRConsoleClient
{
    internal class Program
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string librayName);

        private static void Main(string[] args)
        {
            var obj = new
            {
                name = "wangcl",
                code = "wcl"
            };
            string data = string.Empty;
            using (var mem = new MemoryStream())
            {
                using (var writer = new BsonWriter(mem))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, obj);
                    data = Convert.ToBase64String(mem.ToArray());
                    Console.WriteLine(data);
                }
            }
            using (
                var ms =
                    new MemoryStream(
                        Convert.FromBase64String("MQAAAAMwACkAAAACTmFtZQAHAAAARWFzdGVyAAlTdGFydERhdGUAgDf0uj0BAAAAAA=="))
                )
            {
                using (var reader = new BsonReader(ms))
                {
                    reader.ReadRootValueAsArray = true;
                    var serializer = new JsonSerializer();

                    var e = serializer.Deserialize<IList<dynamic>>(reader);
                }
            }
            Do().ContinueWith(_ =>
            {
                IHubProxy hub = (_ as Task<IHubProxy>).Result;
                for (int i = 0; i < 10000; i++)
                {
                    var @var = i;
                    hub.Invoke("Say", @var.ToString())
                        .ContinueWith(t => { Console.WriteLine(@var); });
                }


                Console.WriteLine("Init completed!");
            });
            Console.ReadLine();
        }

        private static Task Do()
        {
            string url = "http://localhost:1980";
            var connection = new HubConnection(url);
            IHubProxy hub = connection.CreateHubProxy("echo");
            var httpClient = new DefaultHttpClient();
            var transport = new AutoTransport(
                httpClient,
                new IClientTransport[]
                {
                    new ServerSentEventsTransport(httpClient),
                    new LongPollingTransport(httpClient)
                }
                );
            //connection.Error +=
            //    error => ConsoleColor.Red.AsColorFor(() => Console.WriteLine("Error from connection: {0}", error));
            connection.Closed += () =>
            {
                Console.WriteLine("Closed");
                //if (!connection.EnsureReconnecting())
                //{
                //    Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(t => connection.Start().Wait());
                //}
                if (!connection.EnsureReconnecting())
                {
                    Task.Factory.StartNew(() => Thread.Sleep(TimeSpan.FromSeconds(30)))
                        .ContinueWith(t => connection.Start().Wait());
                }
            };
            connection.ConnectionSlow += () => Console.WriteLine("ConnectionSolw!");
            connection.Received += data => Console.WriteLine(string.Format("Received:{0}", data));
            connection.Reconnected += () => Console.WriteLine("Reconnected!");
            connection.StateChanged +=
                state =>
                    Console.WriteLine("StateChanged:From {0} to {1}", state.OldState, state.NewState);
            return connection.Start(transport).ContinueWith(_ =>
            {
                Console.WriteLine("Connected, transport is :{0}", connection.Transport.Name);
                return hub;
            });
        }
    }

    public static class Extensions
    {
        public static void AsColorFor(this ConsoleColor color, Action action)
        {
            Console.ForegroundColor = color;
            action();
            Console.ResetColor();
        }
    }

    public class Event
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
    }
}