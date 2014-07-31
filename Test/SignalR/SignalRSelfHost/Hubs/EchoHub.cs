using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRSelfHost.Hubs
{
    [HubName("echo")]
    public class EchoHub : Hub
    {
        public void Say(string message)
        {
            Console.WriteLine(message);
        }
    }
}
