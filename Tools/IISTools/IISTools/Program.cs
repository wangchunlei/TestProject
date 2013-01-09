using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Microsoft.Web.Administration;

namespace IISTools
{
    class Program
    {
        static void Main(string[] args)
        {
            //Managerment.CreatingUsers();
            //var user =
            //    new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null).Translate(typeof(NTAccount)).Value;
            //Managerment.SetDirectoryPermissions(user, @"E:\123");

            //IISHelper.Clear();
            IISHelper.CreateAppPool(new IISHelper.AppPoolDTO()
                {
                    EncryptPassword = true,
                    ManagedPipelineMode = ManagedPipelineMode.Integrated,
                    Password = "123",
                    PoolID = "Domas5.0WebSitePool",
                    RunTimeVersion = IISHelper.RunTimeVersion.V4,
                    UserName = "wangcl"
                });
            IISHelper.CreateSite(new IISHelper.SiteDTO()
                {
                    PoolName = "Domas5.0WebSitePool",
                    Port = 8090,
                    RootDir = @"D:\View\Trunk\Domas.Web\Domas.Web.Portal\Domas.Web.Portal",
                    SiteId = 100,
                    SiteName = "Domas5.0WebSite"
                });
        }
    }
}
