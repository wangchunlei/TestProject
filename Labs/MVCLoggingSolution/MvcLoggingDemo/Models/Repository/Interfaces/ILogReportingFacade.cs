using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MvcLoggingDemo.Services.Paging;

namespace MvcLoggingDemo.Models.Repository
{    
    /// <summary>
    /// This interface provides a facade over all of our LogReport repositories
    /// </summary>
    public interface ILogReportingFacade
    {
        /// <summary>
        /// Gets a filtered list of log events
        /// </summary>
        /// <param name="pageIndex">0 based page index</param>
        /// <param name="pageSize">max number of records to return</param>
        /// <param name="start">start date</param>
        /// <param name="end">end date</param>
        /// <param name="logProviderName">If empty all log providers used, otherwise it will be filtered by the specified log provider</param>
        /// <param name="logLevel">The level of the log messages</param>
        /// <returns>A filtered list of log events</returns>
        IPagedList<LogEvent> GetByDateRangeAndType(int pageIndex, int pageSize, DateTime start, DateTime end, string logProviderName, string logLevel);        

        /// <summary>
        /// Returns a single Log event
        /// </summary>
        /// <param name="logProviderName">Name of the log provider</param>
        /// <param name="id">Id of the log event as a string</param>
        /// <returns>A single Log event</returns>
        LogEvent GetById(string logProviderName, string id);

        /// <summary>
        /// Clears log messages for a given date range and log level
        /// </summary>
        /// <param name="logProviderName">Name of the log provider</param>
        /// <param name="start">start date</param>
        /// <param name="end">end date</param>
        /// <param name="logLevels">The level of the log messages</param>
        void ClearLog(string logProviderName, DateTime start, DateTime end, string[] logLevels);
        
        /// <summary>
        /// Get's a list of all log providers registered in the web.config file
        /// </summary>
        /// <returns>A list of all log providers registered</returns>
        Dictionary<string, string> GetLogProviders();
    }
}