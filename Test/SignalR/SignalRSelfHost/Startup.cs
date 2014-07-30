using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using SignalRSelfHost.Modules;

[assembly: OwinStartup(typeof(SignalRSelfHost.Startup))]

namespace SignalRSelfHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                map.RunSignalR(new HubConfiguration()
                {
                    EnableDetailedErrors = true,
                });
            });
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            GlobalHost.HubPipeline.AddModule(new EchoHubModule());
        }
    }
}
