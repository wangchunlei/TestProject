namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;

    internal class DataResultsWebBrowser : ResultsWebBrowser
    {
        private HashSet<string> _expandedColumns = new HashSet<string>();
        private static Regex _tableIdFormat = new Regex(@"^t\d+$");

        public event EventHandler<LinqClickEventArgs> LinqClicked;

        [CompilerGenerated]
        private static void <.ctor>b__f(object sender, LinqClickEventArgs e)
        {
        }

        public void ClearExpandedColumns()
        {
            this._expandedColumns.Clear();
        }

        public void CollapseTo(int? depth)
        {
            if (base.Document != null)
            {
                foreach (HtmlElement element in from x in base.Document.GetElementsByTagName("table").OfType<HtmlElement>()
                    where (x.Id != null) && _tableIdFormat.IsMatch(x.Id)
                    select x)
                {
                    this.SetTableVisibility(element, !depth.HasValue || (this.GetTableDepth(element) < depth.Value));
                }
            }
        }

        private int GetTableDepth(HtmlElement table)
        {
            int num = 1;
        Label_003F:
            table = table.Parent;
            if (table != null)
            {
                if (((table.TagName.ToUpperInvariant() == "TABLE") && (table.Id != null)) && _tableIdFormat.IsMatch(table.Id))
                {
                    num++;
                }
                goto Label_003F;
            }
            return num;
        }

        protected override void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
        {
            base.OnDocumentCompleted(e);
            try
            {
                this.RestoreExpandedColumns();
            }
            catch (Exception exception)
            {
                Log.Write(exception);
            }
        }

        protected override void OnNavigating(WebBrowserNavigatingEventArgs e)
        {
            string uriString = e.Url.ToString();
            if (!(uriString.StartsWith("about") || !Uri.IsWellFormedUriString(uriString, UriKind.Absolute)))
            {
                e.Cancel = true;
                WebHelper.LaunchBrowser(uriString);
            }
            base.OnNavigating(e);
        }

        private void RestoreExpandedColumns()
        {
            foreach (string str in this._expandedColumns.ToArray<string>())
            {
                string[] strArray2 = str.Split(".".ToCharArray(), 3);
                this.ToggleGraphColumn(strArray2[0], int.Parse(strArray2[1]), str);
            }
        }

        private void SetTableVisibility(HtmlElement table, bool makeVisible)
        {
            HtmlElement elementById = base.Document.GetElementById(table.Id + "ud");
            if (elementById != null)
            {
                bool flag = elementById.InnerText == "6";
                if (makeVisible == flag)
                {
                    elementById.InnerText = flag ? "5" : "6";
                    table.Style = flag ? "border-bottom: 2px solid" : "border-bottom: dashed 2px";
                    HtmlElementCollection children = table.Children;
                    if ((children.Count == 1) && (children[0].TagName.ToUpperInvariant() == "TBODY"))
                    {
                        children = children[0].Children;
                    }
                    IEnumerable<HtmlElement> source = children.OfType<HtmlElement>();
                    if (source.Count<HtmlElement>() >= 2)
                    {
                        foreach (HtmlElement element2 in source.Skip<HtmlElement>(1))
                        {
                            if ((element2.TagName.ToUpperInvariant() == "TR") && !((element2.Id != null) && element2.Id.StartsWith("sum")))
                            {
                                element2.Style = flag ? "" : "display:none";
                            }
                        }
                    }
                }
            }
        }

        public void ToggleGraphColumn(string tableID, int colIndex)
        {
            this.ToggleGraphColumn(tableID, colIndex, null);
        }

        private void ToggleGraphColumn(string tableID, int colIndex, string oldKey)
        {
            HtmlElement elementById = base.Document.GetElementById(tableID);
            if (elementById != null)
            {
                HtmlElement element2 = (from r in elementById.Children[0].Children.OfType<HtmlElement>()
                    where r.TagName == "TR"
                    select r).Skip<HtmlElement>(1).FirstOrDefault<HtmlElement>();
                if (element2 != null)
                {
                    HtmlElement element3 = (from r in element2.Children.OfType<HtmlElement>()
                        where r.TagName == "TH"
                        select r).Skip<HtmlElement>(colIndex).FirstOrDefault<HtmlElement>();
                    if (element3 != null)
                    {
                        string item = string.Concat(new object[] { tableID, ".", colIndex, ".", element3.InnerText });
                        if ((oldKey == null) || !(item != oldKey))
                        {
                            decimal num3;
                            HtmlElement[] source = (from row in (from r in elementById.Children[0].Children.OfType<HtmlElement>()
                                where r.TagName == "TR"
                                select r).Skip<HtmlElement>(2)
                                select (from r in row.Children.OfType<HtmlElement>()
                                    where r.TagName == "TD"
                                    select r).Skip<HtmlElement>(colIndex).First<HtmlElement>() into row
                                where row != null
                                select row).ToArray<HtmlElement>();
                            IEnumerable<HtmlElement> enumerable = source.AsEnumerable<HtmlElement>();
                            if (source.Length > 0)
                            {
                                enumerable = enumerable.Take<HtmlElement>(source.Length - 1);
                            }
                            decimal num = 0M;
                            bool flag = false;
                            foreach (HtmlElement element4 in enumerable)
                            {
                                if (element4.InnerHtml.StartsWith("<span", StringComparison.OrdinalIgnoreCase))
                                {
                                    flag = true;
                                    int index = element4.InnerHtml.IndexOf("</span> ", StringComparison.OrdinalIgnoreCase);
                                    if (index > 5)
                                    {
                                        element4.InnerHtml = element4.InnerHtml.Substring(index + 8);
                                    }
                                    element4.Style = null;
                                }
                                if (decimal.TryParse(element4.InnerHtml, out num3) && (num3 > num))
                                {
                                    num = num3;
                                }
                            }
                            if (flag)
                            {
                                this._expandedColumns.Remove(item);
                            }
                            else
                            {
                                this._expandedColumns.Add(item);
                                if (num == 0M)
                                {
                                    num = 1M;
                                }
                                foreach (HtmlElement element4 in enumerable)
                                {
                                    if (decimal.TryParse(element4.InnerHtml, out num3))
                                    {
                                        decimal num4 = num3 / num;
                                        element4.InnerHtml = "<span class='graphbar' style='padding-right:" + Math.Round((decimal) (num4 * 15), 2).ToString(CultureInfo.InvariantCulture) + "em'>:</span> " + num3.ToString(CultureInfo.InvariantCulture);
                                        element4.Style = "text-align:left";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

