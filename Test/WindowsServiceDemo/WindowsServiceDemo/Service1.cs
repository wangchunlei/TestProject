using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Domas.DAP.ADF.LogManager;
using NamedPipe;

namespace WindowsServiceDemo
{
    public partial class Service1 : ServiceBase
    {
        private ILogger logger = LogManager.GetLogger("KeyDemo");
        //private KeyboardHookListener m_KeyboardHookManager;
        //private Desktop m_Desktop = new Desktop();

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
                ClosePreProcess();
                //Process.Start(@"InterceptKeys.exe");
                //Win32DLL.StartProcessAndBypassUAC(@"F:\Github\TestProject\Test\WindowsServiceDemo\WindowsServiceDemo\bin\Debug\InterceptKeys.exe");
            });

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
            });
        }

        protected override void OnStop()
        {
            //m_Desktop.EndInteraction();
            ClosePreProcess();
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
