using CredentialManagement;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoLoginTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new Credential()
            {
                Target = "192.168.20.195",
                Type = CredentialType.DomainPassword,//windows 凭证 Generic 普通凭证
                PersistanceType = PersistanceType.Enterprise,//永久
                
            };
            if (c.Exists())
            {
                c.Load();
                Console.WriteLine(c.Username);
            }

            //GetCredential("192.168.20.30");
            //using (PrincipalContext context = new PrincipalContext(ContextType.Machine, "192.168.20.30",null,ContextOptions.Negotiate))
            //{
            //    //if (!currentUser.ToLower().StartsWith("lanxum\\"))
            //    //{
            //    //    Validate(domain, context, new Exception("当前用户未登录域"));
            //    //}
            //    //else
            //    {
            //        try
            //        {
            //            var upUser = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, "wcl");
            //        }
            //        catch (Exception ex)
            //        {
            //            //Validate(domain, context, ex);
            //        }
            //    }
            //}

            //var cm = new VistaPrompt();
            ////cm.Domain = "lanxum";
            //cm.SaveChecked = true;
            //cm.ShowSaveCheckBox = true;
            //cm.Title = @"指定已授权的 域(计算机)\用户";
            //cm.Message = "123213";
            //DialogResult rs = cm.ShowDialog();

            ////CredentialManagement();
            var us = c.Username.Split('\\');
            var httpClient = new HttpClient(new HttpClientHandler()
            {
                //UseDefaultCredentials = true
                Credentials = new System.Net.NetworkCredential(@"transfer218", "Lanxum1234", "WIN-6U432IIN")
            });
            httpClient.GetStringAsync("http://192.168.20.195:8090").ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine(t.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine(t.Result);
                }
            });

            //var c = new Credential()
            //{
            //    Target = "192.168.20.30",
            //    Type = CredentialType.DomainPassword,//windows 凭证 Generic 普通凭证
            //    PersistanceType = PersistanceType.Enterprise,//永久
            //    Username = "zhangzhongsheng@lanxum.com",
            //    SecurePassword = cm.SecurePassword,
            //};
            //c.Save();

            Console.ReadKey();
        }

        private static void CredentialManagement()
        {
            //var cm = new CredentialManagement.CredentialSet();
            ////{
            ////    Target = "192.168.20.30",
            ////    //PersistanceType = PersistanceType.Enterprise,
            ////    //Username = "wangchunleird@lanxum.com",
            ////    //Password = "lx314114",
            ////    ////Type = CredentialType.None
            ////};
            ////cm.Save();
            //cm.Load();
            //var item = cm.FirstOrDefault(cc => cc.Target == "192.168.20.30");
            //{
            //    Console.WriteLine(item.Target + "   " + item.PersistanceType + "    " + item.Type + "   " + item.Username + ":" + item.Password);
            //}

            var c = new Credential()
            {
                Target = "192.168.20.30",
                Type = CredentialType.DomainPassword,//windows 凭证 Generic 普通凭证
                PersistanceType = PersistanceType.Enterprise,//永久
                Username = "zhangzhongsheng@lanxum.com",
                Password = "zhangzs"
            };
            c.Save();
        }

        public static System.Net.NetworkCredential GetCredential(string serverip)
        {
            System.Net.NetworkCredential credentials = null;
            BaseCredentialsPrompt prompt = null;
            if (IsWinVistaOrHigher())
            {
                prompt = new VistaPrompt();
                Console.WriteLine("win7");
            }
            else
            {
                prompt = new XPPrompt() { Target = serverip };

                Console.WriteLine("xp");
            }
            prompt.SaveChecked = true;
            prompt.ShowSaveCheckBox = true;
            prompt.Title = @"指定已授权的 域(计算机)\用户";
            try
            {
                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    credentials = new System.Net.NetworkCredential(prompt.Username, prompt.SecurePassword);
                    if (prompt.SaveChecked)
                    {
                        var cm = new Credential()
                        {
                            Target = serverip,
                            Type = CredentialType.DomainPassword,//windows 凭证 Generic 普通凭证
                            PersistanceType = PersistanceType.Enterprise,//永久
                            Username = prompt.Username,
                            SecurePassword = prompt.SecurePassword
                        };
                        cm.Save();
                    }
                    return credentials;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException());
            }

            return null;
        }
        private static bool IsWinVistaOrHigher()
        {
            OperatingSystem OS = Environment.OSVersion;
            return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
        }
    }
}
