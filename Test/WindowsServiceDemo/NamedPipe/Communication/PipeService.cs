using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace NamedPipe.Communication
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PipeService : IPipeService
    {
        public static string URI = "net.pipe://localhost/Pipe";

        // This is when we used the HTTP bindings.
        // = "http://localhost:8000/Pipe";

        #region IPipeService Members

        public void PipeIn(List<string> data)
        {
            if (DataReady != null)
                DataReady(data);
        }

        public delegate void DataIsReady(List<string> hotData);
        public DataIsReady DataReady = null;

        #endregion
    }
}
