using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using NamedPipe.Communication;

namespace NamedPipe
{
    // Create this class to send a message on a
    // named pipe.
    public class Sender
    {
        // Use a default pipe name. Be careful not to reuse
        // this on other applications.
        public static void SendMessage(List<string> messages)
        {
            SendMessage(messages, Receiver.DefaultPipeName);
        }

        // Use this method when we have an actual pipe name.
        public static void SendMessage(List<string> messages, string PipeName)
        {
            EndpointAddress ep
                = new EndpointAddress(
                    string.Format("{0}/{1}",
                       PipeService.URI,
                       PipeName));

            //      IPipeService proxy = ChannelFactory<IPipeService>.CreateChannel( new BasicHttpBinding(), ep );
            IPipeService proxy = ChannelFactory<IPipeService>.CreateChannel(new NetNamedPipeBinding(), ep);
            proxy.PipeIn(messages);
        }
    }
}
