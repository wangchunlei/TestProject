using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;

namespace IISTools
{
    public static class IISHelper
    {
        public class AppPoolDTO
        {
            public string PoolID { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public bool EncryptPassword { get; set; }
            public RunTimeVersion RunTimeVersion { get; set; }
            public ManagedPipelineMode ManagedPipelineMode { get; set; }
        }
        public enum RunTimeVersion
        {
            V2, V4
        }
        public static void CreateAppPool(AppPoolDTO poolDto)
        {
            var mgr = new ServerManager();
            var pools = mgr.ApplicationPools;

            CreateAppPool(pools, poolDto);

            mgr.CommitChanges();
        }
        public static void Clear(string poolId)
        {
            var mgr = new ServerManager();
            var pools = mgr.ApplicationPools;
            foreach (var pool in pools)
            {
                if (pool.Name.Equals(poolId, StringComparison.OrdinalIgnoreCase))
                {
                    pool.Delete();
                }
            }
            mgr.CommitChanges();
        }
        private static bool CreateAppPool(ApplicationPoolCollection pools, AppPoolDTO dto)
        {
            try
            {
                var newPool = pools.Add(dto.PoolID);
                newPool.ProcessModel.UserName = dto.UserName;
                if (!dto.EncryptPassword)
                {
                    newPool.ProcessModel.Attributes["password"].SetMetadata("encryptProvider", "");
                }
                newPool.ProcessModel.Password = dto.Password;
                newPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
                newPool.ManagedPipelineMode = dto.ManagedPipelineMode;
                newPool.ManagedRuntimeVersion = dto.RunTimeVersion == RunTimeVersion.V2 ? "v2.0" : dto.RunTimeVersion == RunTimeVersion.V4 ? "v4.0" : "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Adding AppPool {0} failed. Reason: {1}", dto.PoolID, ex.Message);
                return false;
            }
            return true;
        }
        public class SiteDTO
        {
            public string PoolName { get; set; }
            public long SiteId { get; set; }
            public string SiteName { get; set; }
            public string RootDir { get; set; }
            public int Port { get; set; }
        }
        public static void CreateSite(SiteDTO siteDto)
        {
            var mgr = new ServerManager();
            var sites = mgr.Sites;
            CreateSiteInIIS(sites, siteDto);
            mgr.CommitChanges();
        }
        private static bool CreateSiteInIIS(SiteCollection sites, SiteDTO dto)
        {
            try
            {
                var site = sites.CreateElement();
                site.Id = dto.SiteId;
                site.SetAttributeValue("name", dto.SiteName);
                sites.Add(site);

                var app = site.Applications.CreateElement();
                app.SetAttributeValue("path", "/");
                app.SetAttributeValue("applicationPool", dto.PoolName);
                site.Applications.Add(app);

                var vdir = app.VirtualDirectories.CreateElement();
                vdir.SetAttributeValue("path", "/");
                vdir.SetAttributeValue("physicalPath", string.Format(@"{0}\{1}", dto.RootDir, dto.SiteName));
                app.VirtualDirectories.Add(vdir);

                var binding = site.Bindings.CreateElement();
                binding.SetAttributeValue("protocol", "http");
                binding.SetAttributeValue("bindingInformation", string.Format(@":{0}:{1}", dto.Port, dto.SiteName));
                site.Bindings.Add(binding);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create site {0} failed. Reason: {1}", dto.SiteName, ex.Message);
                return false;
            }
            return true;
        }

        public static void SetAnonyousUserToProcessId()
        {
            var mgr = new ServerManager();
            try
            {
                var config = mgr.GetApplicationHostConfiguration();
                var section = config.GetSection("system.webServer/security/authentication/anonymousAuthentication");
                section.SetAttributeValue("userName", "");
                section.RawAttributes.Remove("password");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Removing anonymous user entry failed. Reason: {0}", ex.Message);
            }
            mgr.CommitChanges();
        }
    }
}
