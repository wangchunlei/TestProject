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
                Process.Start(@"InterceptKeys.exe");
                //Win32DLL.StartProcessAndBypassUAC(@"F:\Github\TestProject\Test\WindowsServiceDemo\WindowsServiceDemo\bin\Debug\InterceptKeys.exe");
            });
        }

        protected override void OnStop()
        {
            //m_Desktop.EndInteraction();
        }
    }
}
