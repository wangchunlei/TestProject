using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcLoggingDemo.Models.Google.Visualization
{
    public class ChartData
    {
        private ArrayList _cols = new ArrayList();
        private ArrayList _rows = new ArrayList();

        /// <summary>
        /// An array of columns
        /// </summary>
        public ChartColumn[] cols
        {
            get
            {
                ChartColumn[] myCols = (ChartColumn[])_cols.ToArray(typeof(ChartColumn));
                return myCols;
            }
        }

        /// <summary>
        /// An array of rows
        /// </summary>
        public ChartRow[] rows
        {
            get
            {
                ChartRow[] myRows = (ChartRow[])_rows.ToArray(typeof(ChartRow));
                return myRows;
            }
        }

        public void AddColumn(ChartColumn column)
        {
            _cols.Add(column);
        }

        public void AddRow(ChartRow row)
        {
            _rows.Add(row);
        }

    }
}