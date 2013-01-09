using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace IISTools
{
    internal static class Managerment
    {
        /// <summary>
        /// 创建用户
        /// </summary>
        public static void CreatingUsers()
        {
            var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            var newUser = ad.Children.Add("PoolID1", "user");
            newUser.Invoke("SetPassword", new object[] { "PoolIDPwd1" });
            newUser.Invoke("Put", new object[] { "Description", "AppPool Account" });
            newUser.CommitChanges();
        }
        /// <summary>
        /// 指定用户对指定文件夹的完全控制
        /// </summary>
        /// <param name="user"></param>
        /// <param name="folder"></param>
        public static void SetDirectoryPermissions(string user, string folder)
        {
            //var user = string.Format(@"wangchunleird");
            //user = Environment.UserName;

            if (System.IO.Directory.Exists(folder))
            {
                var dirsec = Directory.GetAccessControl(folder);
                dirsec.SetAccessRuleProtection(true, false);

                foreach (AuthorizationRule rule in dirsec.GetAccessRules(true, true, typeof(NTAccount)))
                {
                    dirsec.RemoveAccessRuleAll(new FileSystemAccessRule(rule.IdentityReference, FileSystemRights.FullControl, AccessControlType.Allow));
                }

                dirsec.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl, AccessControlType.Allow));
                dirsec.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                dirsec.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));

                Directory.SetAccessControl(folder, dirsec);
            }
            else
            {
                Console.WriteLine("目录{0}不存在", folder);
            }
        }
    }
}
