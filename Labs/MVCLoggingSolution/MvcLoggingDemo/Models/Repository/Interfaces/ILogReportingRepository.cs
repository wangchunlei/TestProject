using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MvcLoggingDemo.Services.Paging;

namespace MvcLoggingDemo.Models.Repository
{    
    /// <summary>
    /// This interface provides the methods that we need to so that we can report on log messages stored by the various
    /// logging tools used in our website
    /// </summary>
    public interface ILogReportingRepository
    {
        /// <summary>
        /// Gets a filtered list of log events
        /// </summary>
        /// <param name="pageIndex">0 based page index</param>
        /// <param name="pageSize">max number of records to return</param>
        /// <param name="start">start date</param>
        /// <param name="end">end date</param>
        /// <param name="logLevel">The level of the log messages</param>
        /// <returns>A filtered list of log events</returns>
        IQueryable<LogEvent> GetByDateRangeAndType(int pageIndex, int pageSize, DateTime start, DateTime end, string logLevel);

        /// <summary>
        /// Returns a single Log event
        /// </summary>
        /// <param name="id">Id of the log event as a string</param>
        /// <returns>A single Log event</returns>
        LogEvent GetById(string id);                

        /// <summary>
        /// Clears log messages between a date range and for specified log levels
        /// </summary>
        /// <param name="start">start date</param>
        /// <param name="end">end date</param>
        /// <param name="logLevels">string array of log levels</param>
        void ClearLog(DateTime start, DateTime end, string[] logLevels);
    }
}