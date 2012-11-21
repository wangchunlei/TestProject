using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceModel;
using System.Threading;

namespace Novotny.IECookieHelper.LowRights
{
    internal static class Program
    {
        private static Mutex _mutex;
        private static ServiceHost _host;

        internal static AutoResetEvent _exit = new AutoResetEvent(false);
        internal static DateTime _lastUpdated = DateTime.Now;
        internal static TimeSpan _timeoutMilliseconds = new TimeSpan(0, 10, 0); // default to 10 min
        private static readonly System.Timers.Timer _exitTimer = new System.Timers.Timer(1000 * 60); // Run the timer every min

        internal readonly static string Address = "net.pipe://localhost/CookieService/{0}";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            // Look for a DEBUG parameter -- the CLR can't auto-attach to child processes,
            // so this lets us attach the debugger on startup if we want
            if (args.Length > 2 && args[2].Contains("DEBUG"))
                Debugger.Launch();

            string id;
            if (args.Length > 0)
                id = args[0];
            else
            {
                // ID not found, error
                return -1;
            }
            
            if(args.Length > 1)
            {
                // get the requested timeout
                double val;
                if(double.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                    _timeoutMilliseconds = TimeSpan.FromSeconds(val);
                else
                    return -1; // invalid second parameter
            }

            bool created;
            _mutex = new Mutex(true, "IECookieHelper" + id, out created);
            if (!created)
            {
                // indicate that another instance with this id is already running
                return -2;
            }

            int retCode = 0;

            _exitTimer.AutoReset = true;
            _exitTimer.Elapsed += delegate
                                      {
                                          // if more than 10 min since last usage, consider abandoned and shutdown
                                          // if the host needs us, then it can call keep alive

                                          if(_lastUpdated < DateTime.Now.Subtract(_timeoutMilliseconds))
                                          {
                                              retCode = -3; // timed out
                                              _exit.Set();
                                          }
                                      };

            string uri = string.Format(Address, id);

            // Start the pipe
            StartHost(uri);
            
            // Start the timer
            _exitTimer.Start();
            
            // Wait for the signal telling us it's time to quit
            _exit.WaitOne();

            // Clean up timer, event and host
            _exitTimer.Dispose();
            _exit.Close();
            StopHost();
            _mutex.Close();

            // indicate success
            return retCode;
        }

        private static void StartHost(string uri)
        {
            Uri u = new Uri(uri);
            _host = new ServiceHost(typeof(CookieService), u);
            _host.Open();
        }

        private static void StopHost()
        {
            if (_host.State != CommunicationState.Closed)
                _host.Close();
        }


    }
}
