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
using Domas.DAP.ADF.AutoUpdate;

namespace WindowsServiceDemo
{
    public partial class Service1 : ServiceBase
    {
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
                var uiExeFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InterceptKeys.exe");
                Interop.CreateProcess(uiExeFileName);
            });
        }

        protected override void OnStop()
        {
            var uiExeFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InterceptKeys.exe");
           var process= Process.GetProcessesByName("InterceptKeys").FirstOrDefault();
            if (process!=null)
            {
                process.Kill();
                process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
            }
        }
    }
}
