using System;
using UnityInterface;

namespace UnityImp
{
    public class Logger : ILogger
    {
        public Logger()
        {
            
        }
        public void Log(string msg)
        {
            Console.WriteLine("Log:" + msg);
        }
    }

    public class Logger1 : ILogger
    {
        public void Log(string msg)
        {
            Console.WriteLine("Log1:" + msg);
        }
    }
}
