using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.ServiceModel;
using System.Threading;

using Novotny.IECookieHelper.LowRights;

namespace Novotny.IECookieHelper
{
    internal class RemoteServiceManager : CriticalFinalizerObject, IDisposable
    {
        public RemoteServiceManager()
        {
            TimeOutSeconds = 60 * 10; // 10 min
        }

        ~RemoteServiceManager()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                Dispose(false);
            }
        }

        internal double TimeOutSeconds
        {
            get;
            set;
        }

        private ICookieService _service;

        internal ICookieService Service
        {
            get
            {
                StartProcess();
                return _service;
            }
        }

        private Process _process;
        private bool _exiting;

        private void StartProcess()
        {
            // If we already have a reference to the child
            if (_process != null && !_process.HasExited)
                return;

            // Unhook old process
            UnhookProcessEvent();

            // Get exe
            string loc = typeof (LowRights.Program).Assembly.Location;

            string id = Assembly.GetEntryAssembly().EscapedCodeBase;


            //string args = string.Format("{0} {1} {2}", id, timeOutSec, "DEBUG");
            string args = string.Format(CultureInfo.InvariantCulture, "{0} {1}", id, TimeOutSeconds);



            // To use the regular way of starting a process use this insead of StartLowIntegrityProcess 

            //ProcessStartInfo info = new ProcessStartInfo(loc, id + " DEBUG");
            //ProcessStartInfo info = new ProcessStartInfo(loc, id);
            //info.UseShellExecute = false;
            //_process = Process.Start(info);

            _process = LowIntegrityProcess.Start(loc, args);
            if (!_process.HasExited)
            {
                _process.EnableRaisingEvents = true;
                _process.Exited += ChildExited;
            }
            else
            {
                _process = null;
            }

            string uri = string.Format(Program.Address, id) + "/CookieService";

            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            ChannelFactory<ICookieService> factory = new ChannelFactory<ICookieService>(binding, uri);

            _service = factory.CreateChannel();

            // Allow the other side to setup the pipe -- ideally this should be synchronized instead of
            // a blanket sleep
            Thread.Sleep(1000);
        }

        private void ChildExited(object sender, EventArgs e)
        {
            // might have been killed, restart -- if it's a duplicate or timed out, let it die
            if (!_exiting && (_process.ExitCode != -2 && _process.ExitCode != -3))
                StartProcess();    
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            _exiting = true;
            StopProcess(true);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        private void StopProcess(bool exiting)
        {

            if (exiting && _process != null && !_process.HasExited)
            {
                _service.ExitProcess();
                _service = null;
            }

            UnhookProcessEvent();
        }

        private void UnhookProcessEvent()
        {
            if (_process != null)
            {
                _process.Exited -= ChildExited;
                _process.Dispose();
                _process = null;
            }
        }

        
    }
}
