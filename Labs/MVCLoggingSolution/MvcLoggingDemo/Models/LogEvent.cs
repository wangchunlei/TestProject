using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcLoggingDemo.Models
{

    /// <summary>
    /// This represents a generic log message that can store log information about
    /// any logger implemented. Eg: Log4Net, NLog, Health Monitoring, Elmah
    /// </summary>
    public class LogEvent
    {
        private string _Id = string.Empty;

        /// <summary>
        /// String representation of the event log id
        /// </summary>
        public string Id 
        {
            get
            {
                switch (IdType)
                {
                    case "number":
                        return IdAsInteger.ToString();                        

                    case "guid":
                        return IdAsGuid.ToString();                        

                    default:
                        return _Id;
                }
            }

            set
            {
                _Id = value;
            }
        }

        /// <summary>
        /// Stores the Id of the log event as a GUID 
        /// </summary>
        internal Guid IdAsGuid { get; set; }

        /// <summary>
        /// Stores the Id of the log event as an integer
        /// </summary>
        internal int IdAsInteger { get; set; }

        /// <summary>
        /// Stores the base type of the id 
        /// Valid values are : number, guid, string
        /// </summary>
        internal string IdType { get; set; }

        /// <summary>
        /// The date of the log event
        /// </summary>
        public DateTime LogDate { get; set; }

        /// <summary>
        /// The name of the log provider
        /// Example values are NLog, Log4Net, Elmah, Health Monitoring
        /// </summary>
        public string LoggerProviderName { get; set; }

        /// <summary>
        /// Information about where the error occurred
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The machine where the error occured
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// The Type name of the class that logged the error
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The level of the message logged
        /// Valid values are : Debug, Info, Warning, Error, Fatal
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// The message that was logged
        /// </summary>
        public string Message { get; set; }                

        /// <summary>
        /// If the message was from an error this value will contain details of the stack trace. 
        /// Otherwise it will be empty
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// If the message was from an error this value will contain details of the HTTP Server variables and Cookies. 
        /// Otherwise it will be empty
        /// </summary>
        public string AllXml { get; set; }        
    }
}
