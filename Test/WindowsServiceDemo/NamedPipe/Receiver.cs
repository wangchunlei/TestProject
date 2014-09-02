using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using NamedPipe.Communication;

namespace NamedPipe
{
    public class Receiver : IDisposable
    {

        #region Constructors
        // Default constructor, use this if
        // the default name of the pipe can
        // be used instead of one provided.
        public Receiver() : this(DefaultPipeName) { }

        // Use this constructor to create
        // a pipe with a unique name.
        public Receiver(string PipeName)
        {
            _PipeName = PipeName;
        }
        #endregion

        #region Public Operations
        // If the service host can be started,
        // true will be returned. If false is returned
        // another process owns the pipe.
        public bool ServiceOn()
        {
            return (_operational = HostThisService());
        }

        public void ServiceOff()
        {
            if (_host != null)
                if (_host.State != CommunicationState.Closed)
                    _host.Close();

            _operational = false;

        }
        #endregion

        #region WCF Operations
        // The thread which will listen for communications
        // from the other side of the pipe.
        private bool HostThisService()
        {

            try
            {

                _host = new ServiceHost(_ps, new Uri(PipeService.URI));

                // Usage BasicHttpBinding can be used if this is
                // not going to be on the local machine.
                _host.AddServiceEndpoint(typeof(IPipeService),
                     new NetNamedPipeBinding(),
                     _PipeName);
                _host.Open();

                _operational = true;
            }
            catch (Exception ex)
            {
                error = ex;
                _operational = false;
            }

            return _operational;

        }
        #endregion

        #region Public Properties
        // The consumer of this class will subscribe to this delegate to
        // receive data in a callback fashion.
        public PipeService.DataIsReady Data
        {
            get
            {
                return _ps.DataReady;
            }

            set
            {
                _ps.DataReady += value;
            }
        }

        public string CurrentPipeName
        {
            get { return _PipeName; }
        }

        // Any error text will be placed here.
        public Exception error = null;

        // See the actual name of the pipe for
        // any default operations.
        public const string DefaultPipeName = "Pipe1";
        #endregion

        #region Private Variables
        private PipeService _ps = new PipeService();
        private bool _operational = false;
        private ServiceHost _host = null;
        private string _PipeName = string.Empty;
        #endregion

        #region IDisposable Members

        // A basic dispose.
        public void Dispose()
        {
            this.ServiceOff();

            if (_ps != null)
                _ps = null;
        }

        #endregion
    }
}
