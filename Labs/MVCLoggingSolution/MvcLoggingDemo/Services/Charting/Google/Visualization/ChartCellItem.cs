using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcLoggingDemo.Models.Google.Visualization
{
    public class ChartCellItem
    {
        /// <summary>
        /// Value
        /// </summary>
        public object v { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        public string f { get; set; }

        public ChartCellItem(object v, string f)
        {
            this.v = v;
            this.f = f;
        }
    }
}