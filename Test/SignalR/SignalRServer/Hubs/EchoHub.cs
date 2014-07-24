using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Diagnostics;

namespace SignalRServer.Hubs
{
    [HubName("echo")]
    public class EchoHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
        public void Say(string message)
        {
            Trace.WriteLine(message);
        }
    }
}