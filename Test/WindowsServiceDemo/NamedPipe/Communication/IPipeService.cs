using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace NamedPipe.Communication
{
    [ServiceContract(Namespace ="http://localhost/NamedPipe1")]
    public interface IPipeService
    {
        [OperationContract]
        void PipeIn(List<string> data);
    }
}
