using CredentialManagement;
using System;
using System.Collections.Generic;
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
            var cm = new VistaPrompt();
            //cm.Domain = "lanxum";
            cm.SaveChecked = true;
            cm.ShowSaveCheckBox = true;
            DialogResult rs = cm.ShowDialog();
            
            //CredentialManagement();
            var httpClient = new HttpClient(new HttpClientHandler()
            {
                //UseDefaultCredentials = true
                Credentials=new System.Net.NetworkCredential(cm.Username,cm.SecurePassword)
            });
            httpClient.GetStringAsync("http://192.168.20.30:8078").ContinueWith(t =>
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
    }
}
