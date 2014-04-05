using ImpersonForIIS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ImpersonForIIS
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {
            //var security = new Models.SecurityManager()
            //{
            //    UserName = "administrator",
            //    Password = "123456"
            //};
            //var currentUser = WindowsIdentity.GetCurrent().Name;
            //security.Impersonation(() =>
            //{
            //    currentUser += "," + WindowsIdentity.GetCurrent().Name;
            //    System.Threading.Tasks.Task.Factory.StartNew(() =>
            //    {
            //        currentUser += "," + System.Threading.Thread.CurrentPrincipal.Identity.Name;
            //        var app = new AppManager();
            //        var path = Server.MapPath("~/app_data");
            //        app.RunApp(Path.Combine(path, "1.bat"));
            //    }).Wait();
            //});
            //this.test.Text = currentUser;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            var ap = TextBox1.Text;
            var result = new Models.SecurityManager().CreateProcessAsUser(ap, true);
            Label2.Text = result;
        }
    }
}