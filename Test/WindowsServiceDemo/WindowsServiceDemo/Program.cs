using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Domas.DAP.ADF.LogManager;

namespace WindowsServiceDemo
{
    static class Program
    {
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                LogManager.Logger.Debug("Console window closing, death imminent");
            }
            return false;
        }
        static ConsoleEventDelegate _handler;   // Keeps it from getting garbage collected
        // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        static ILogger logger = LogManager.GetLogger("KeyDemo");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            var ser = new Service1();

            if (Environment.UserInteractive)
            {
                _handler = (eventType) =>
                {
                    //only 5 seconds so run important code first
                    if (eventType == 2)
                    {
                        ser.Stop();
                        LogManager.Logger.Debug("Exiting");
                    }
                    return false;
                };

                SetConsoleCtrlHandler(_handler, true);

                ser.Start(null);
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    ser
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }
        static void HandleException(Exception e)
        {
            logger.Error(e);
            //Handle your Exception here
        }
    }
}
