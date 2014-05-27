using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity.InterceptionExtension;
using System.IO;
using log4net;

namespace AOP.Aspects
{
    public class LoggerBehavior : IInterceptionBehavior
    {
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var stopwatch = new Stopwatch();

            var logger = LogManager.GetLogger(input.MethodBase.ReflectedType);

            var declaringType = input.MethodBase.DeclaringType;
            var className = declaringType != null ? declaringType.Name : string.Empty;
            var methodName = input.MethodBase.Name;
            var generic = declaringType != null && declaringType.IsGenericType
                              ? string.Format("<{0}>", string.Join<Type>(", ", declaringType.GetGenericArguments()))
                              : string.Empty;

            var argumentWriter = new List<string>();
            for (var i = 0; i < input.Arguments.Count; i++)
            {
                var argument = input.Arguments[i];
                var argumentInfo = input.Arguments.GetParameterInfo(i);
                argumentWriter.Add(argumentInfo.Name);
            }
            var methodCall = string.Format("{0}{1}.{2}\n{3}", className, generic, methodName, string.Join(",", argumentWriter));

            logger.InfoFormat(@"Entering {0}", methodCall);

            stopwatch.Start();
            var returnMessage = getNext()(input, getNext);
            stopwatch.Stop();

            logger.InfoFormat(@"Exited {0} after {1}ms", methodName, stopwatch.ElapsedMilliseconds);

            return returnMessage;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute
        {
            get { return true; }
        }
    }

    public class LoggingInterceptionBehavior : IInterceptionBehavior
    {
        ILog _logger;
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            _logger = log4net.LogManager.GetLogger(input.MethodBase.Name);
            // Before invoking the method on the original target.
            WriteLog(String.Format(
            "Invoking method {0} at {1}",
            input.MethodBase, DateTime.Now.ToLongTimeString()));
            // Invoke the next behavior in the chain.
            var result = getNext()(input, getNext);
            // After invoking the method on the original target.
            if (result.Exception != null)
            {
                WriteLog(String.Format(
                "Method {0} threw exception {1} at {2}",
                input.MethodBase, result.Exception.Message,
                DateTime.Now.ToLongTimeString()));
            }
            else
            {
                WriteLog(String.Format(
                "Method {0} returned {1} at {2}",
                input.MethodBase, result.ReturnValue,
                DateTime.Now.ToLongTimeString()));
            }
            return result;
        }
        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }
        public bool WillExecute
        {
            get { return true; }
        }
        private void WriteLog(string message)
        {
            _logger.Debug(message);
        }
    }
   
    public class LoggingCallHandlerAttribute : HandlerAttribute
    {
        private readonly int order;
        public LoggingCallHandlerAttribute(int order)
        {
            this.order = order;
        }

        public override ICallHandler CreateHandler(Microsoft.Practices.Unity.IUnityContainer container)
        {
            return new LoggingCallHandler() { Order = order };
        }
    }
}
