using AOP.Aspects;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityInterface;

namespace AOP
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer().LoadConfiguration();
            if (container.IsRegistered<ILogger>())
            {
                var ilogger = container.Resolve<ILogger>();
                ilogger.Log("TEst");
            }

            //container.AddNewExtension<Interception>();
            //container.RegisterType<IApplication, Application>(
            //    new InterceptionBehavior<PolicyInjectionBehavior>(),
            //    new Interceptor<InterfaceInterceptor>()
            //   // new InterceptionBehavior<LoggingInterceptionBehavior>()
            //    );

            //var app = container.Resolve<IApplication>();
            //app.Run();

            //new Test().LongRunningCalc();
            Console.ReadKey(false);
        }
    }
    class Test
    {
        [LoggingCallHandlerAttribute(1)]
        public void LongRunningCalc()
        {
            Thread.Sleep(1000);
        }
    }
    public interface IApplication
    {
        string Run();
    }

    public class Application : IApplication
    {
        [LoggingCallHandler(1)]
        public virtual string Run()
        {
            Console.WriteLine("Hello world");
            return "This is my return value";
        }
    }
}
