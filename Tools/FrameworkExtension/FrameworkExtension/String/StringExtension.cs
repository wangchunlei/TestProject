using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkExtension.String
{
    public static class StringExtension
    {
        public static string CreateRepeating(this char repeatChar, int count)
        {
            return new string(repeatChar, count);
        }

        public static int IndexOfAnyExtension(this string left, string right)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            string username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
            "The quick brown  fox".Split(' ').Select(x =>
                {
                    return new { value = x, length = x.Length };
                });
            return left.IndexOfAny(right.ToCharArray());
        }
    }
}
