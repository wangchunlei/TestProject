using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcLoggingDemo.Models.Google.Visualization
{
    public class ChartRow
    {
        private ArrayList _cellItems = new ArrayList();

        public ChartCellItem[] c
        {
            get
            {
                ChartCellItem[] myCellItems = (ChartCellItem[])_cellItems.ToArray(typeof(ChartCellItem));
                return myCellItems;
            }
        }

        public void AddCellItem(ChartCellItem cellItem)
        {
            _cellItems.Add(cellItem);
        }

    }
}