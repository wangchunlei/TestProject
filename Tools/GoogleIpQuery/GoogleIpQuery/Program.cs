using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GoogleIpQuery
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://gist.github.com/fqrouter/10024379
            //# ASIA
            //_ = '173.194.36.%s' # del 印度 新德里
            //SG3 = '173.194.38.%s' # 新加坡
            //TW3 = '173.194.72.%s' 
            //SG4 = '173.194.117.%s'
            //JP2 = '173.194.120.%s'
            //_ = '173.194.123.%s'
            //JP1 = '173.194.126.%s'
            //HK1 = '173.194.127.%s'
            //TW1 = '74.125.23.%s'
            //TW2 = '74.125.31.%s'
            //_ = '74.125.37.%s'
            //_ = '74.125.135.%s' # ni
            //SG1 = '74.125.200.%s'
            //_ = '74.125.203.%s' # unknown
            //_ = '74.125.204.%s' # unknown
            //SG2 = '74.125.235.%s'
            //_ = '74.125.236.%s' # maa 印度 陈奈
            //_ = '74.125.237.%s' # syd 澳大利亚 悉尼
            //118.98.36.39 # 马来西亚
            //218.189.25.167 # 香港

            var dic = new Dictionary<string, int>();
            for (int i = 1; i < 255; i++)
            {
                var timeout = false;
                for (int j = 0; j < 5; j++)
                {
                    var ping = new Ping();
                    var ipaddress = IPAddress.Parse(string.Format("203.233.63.{0}", i));
                    var reply = ping.SendPingAsync(ipaddress, 5000);
                    reply.ContinueWith(t =>
                    {
                        if (t.Status != TaskStatus.Faulted)
                        {
                            var result = t.Result;
                            if (result.Status == IPStatus.Success && result.RoundtripTime < 300)
                            {
                                if (j == 4)
                                {
                                    dic.Add(result.Address.ToString(), (int)result.RoundtripTime);
                                }

                                Console.WriteLine(result.RoundtripTime);
                            }
                            else
                            {
                                timeout = true;
                                Console.WriteLine(result.Status);
                            }
                        }
                        else
                        {
                            Console.WriteLine(t.Exception);
                            timeout = true;
                        }
                    });
                    if (timeout)
                    {
                        break;
                    }
                }

            }

            Console.ReadKey(false);
        }
    }
}
