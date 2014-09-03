using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsyncClientServerLib.Message;
using AsyncClientServerLib.Server;
using Domas.DAP.ADF.LogManager;
using NamedPipe;
using SocketServerLib.SocketHandler;

namespace WindowsServiceDemo
{
    public partial class Service1 : ServiceBase
    {
        private ILogger logger = LogManager.GetLogger("KeyDemo");
        //private KeyboardHookListener m_KeyboardHookManager;
        //private Desktop m_Desktop = new Desktop();
        private BasicSocketServer server = null;
        private Guid serverGuid = Guid.Empty;

        public Service1()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            this.OnStart(args);
        }
        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(() =>
            {
                var pipe = new Receiver();
                pipe.Data += (data) =>
                {
                    LogManager.Logger.Debug(string.Format("收到数据：{0}", string.Join(",", data)));
                };
                if (pipe.ServiceOn() == false)
                {
                    LogManager.Logger.Error(pipe.error);
                }

                //start socket server
                this.serverGuid = Guid.NewGuid();
                this.server = new BasicSocketServer();
                this.server.ReceiveMessageEvent += (handler, message) =>
                {
                    BasicMessage receivedMessage = (BasicMessage)message;
                    byte[] buffer = receivedMessage.GetBuffer();
                    if (buffer.Length > 1000)
                    {
                        LogManager.Logger.Debug(string.Format("Received a long message of {0} bytes", receivedMessage.MessageLength), "Socket Server");
                        return;
                    }
                    string s = System.Text.ASCIIEncoding.Unicode.GetString(buffer);
                    LogManager.Logger.Debug(string.Format("Received Message:{0} from {1}", s, receivedMessage.ClientUID));
                    var client = server.GetClientList().FirstOrDefault(c => c.ClientUID == handler.ToString());
                    client.TcpSocketClientHandler.SendAsync(new BasicMessage(this.serverGuid, System.Text.ASCIIEncoding.Unicode.GetBytes("收到")));
                };
                this.server.ConnectionEvent += (handler) =>
                {
                    LogManager.Logger.Debug(string.Format("A client is connected to the server"), "Socket Server");
                };
                this.server.CloseConnectionEvent += (handler) =>
                {
                    LogManager.Logger.Debug(string.Format("A client is disconnected from the server"), "Socket Server");
                };
                this.server.Init(new IPEndPoint(IPAddress.Loopback, 8100));
                this.server.StartUp();

                ClosePreProcess();
                Process.Start(@"InterceptKeys.exe");
            });
        }

        protected override void OnStop()
        {
            //m_Desktop.EndInteraction();
            ClosePreProcess();
            if (this.server != null)
            {
                this.server.Dispose();
            }
        }

        private static void ClosePreProcess()
        {
            var processes = Process.GetProcessesByName("InterceptKeys");
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit((int)TimeSpan.FromSeconds(20).TotalMilliseconds);
            }
        }
    }
}
