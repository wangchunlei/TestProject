using System;
using System.Collections.Generic;

namespace LogServiceWebApi.Models
{
    public class LogModel
    {
        public Guid ID { get; set; }
        public string LogName { get; set; }
        public DateTime ReportDate { get; set; }
        public string EventID { get; set; }
        public LogType LogType { get; set; }
        public LogLevel LogLevel { get; set; }
        public IList<string> KeyWordList { get; set; }
        public string User { get; set; }
        public string Computer { get; set; }
        public string OperatorCode { get; set; }
        public string Detail { get; set; }
    }
}