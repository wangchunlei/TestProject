using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SignalRSelfHost.Hubs
{
    [HubName("echo")]
    public class EchoHub : Hub
    {
        //static EchoHub()
        //{
        //    var aTimer = new System.Timers.Timer(TimeSpan.FromMinutes(2).TotalMilliseconds);
        //    aTimer.Elapsed += ATimerElapsed;
        //    aTimer.Enabled = true;
        //    aTimer.Start();
        //}

        private static void ATimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EchoHub>();
            context.Clients.All.ServerLoop(DateTime.Now.ToString());
        }
        public void Say(string message)
        {
            Console.WriteLine(message);
        }

        public override Task OnDisconnected()
        {
            return base.OnDisconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }
    }
}
