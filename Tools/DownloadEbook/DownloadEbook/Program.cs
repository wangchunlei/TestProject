using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DownloadEbook
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            var strart = "<div class=\"bookcontent clearfix\" id=\"BookText\">";
            var end = "<script language=\"javascript\" type=\"text/javascript\" src=\"/bdad.js\">";
            Regex regex = new Regex(string.Concat(strart, ".*", end));

            var titleReg = new Regex("<title>.*</title>");

            using (var client = new WebClient())
            {
                for (int i = 244; i <= 963; i++)
                {
                    var cur = i;
                    try
                    {
                        var content = client.DownloadString(string.Format("http://www.513gp.org/mayishenxiang/{0}.html", i));
                        var title =
                            titleReg.Match(content)
                                .Value.Replace("<title>", string.Empty)
                                .Replace("</title>", string.Empty)
                                .Trim();
                        var match = regex.Match(content).Value;
                        var result = match.Replace(strart, string.Empty).Trim().Replace(end, string.Empty).Trim().
                            Replace("<br/><br/>", Environment.NewLine).Replace("<br /><br />", Environment.NewLine).Replace("<br />", Environment.NewLine);
                        sb.Append(string.Concat(title, Environment.NewLine, result));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Console.WriteLine(i - 244);
                }
            }
            File.WriteAllText("1.txt", sb.ToString());
        }
    }
}
