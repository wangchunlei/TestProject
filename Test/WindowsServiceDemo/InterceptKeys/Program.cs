using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Domas.DAP.ADF.LogManager;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

namespace InterceptKeys
{
    static class Program
    {
        static void Main()
        {
            ILogger logger = LogManager.GetLogger("KeyDemo");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            try
            {
                var m_KeyboardHookManager = new KeyboardHookListener(new GlobalHooker());
                m_KeyboardHookManager.Enabled = true;
                m_KeyboardHookManager.KeyPress += (sender, e) =>
                {
                    NamedPipe.Sender.SendMessage(new List<string>(){e.KeyChar.ToString()});
                    logger.Debug(string.Format("KeyDown \t\t {0}\n", e.KeyChar));
                };
                Application.Run();
            }
            catch (Exception)
            {
                throw;
            }

        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
