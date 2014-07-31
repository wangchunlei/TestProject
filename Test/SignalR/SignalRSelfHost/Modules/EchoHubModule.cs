using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalRSelfHost.Modules
{
    public class EchoHubModule : HubPipelineModule
    {
        //public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
        //{
        //    Trace.WriteLine("BuildIncoming");
        //    //return base.BuildIncoming(invoke);
        //    return base.BuildIncoming(ctx =>
        //    {
        //        Trace.WriteLine(string.Format("I might call {0}...", ctx.MethodDescriptor.Name));

        //        if (ctx.MethodDescriptor.Name == "SayHello")
        //        {
        //            Trace.WriteLine(" But I won`t :(");
        //            return null;
        //        }
        //        Trace.WriteLine(" and I will!");
        //        return invoke(ctx);
        //    });
        //}

        //public override Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send)
        //{
        //    Trace.WriteLine("BuildOutgoing");
        //    //return base.BuildOutgoing(send);
        //    return base.BuildOutgoing(ctx =>
        //    {
        //        Trace.Write(string.Format("I might callback {0}...",ctx.Invocation.Method));
        //        if (ctx.Invocation.Method == "goodbye")
        //        {
        //            Trace.WriteLine(" but I won't :(");
        //            return null;
        //        }
        //        Trace.WriteLine(" and I will!");
        //        return send(ctx);
        //    });
        //}
    }
}