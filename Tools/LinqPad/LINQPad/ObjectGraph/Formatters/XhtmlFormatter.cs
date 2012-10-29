namespace LINQPad.ObjectGraph.Formatters
{
    using LINQPad;
    using LINQPad.ExecutionModel;
    using LINQPad.ObjectGraph;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml.Linq;

    internal class XhtmlFormatter : IObjectGraphVisitor
    {
        private List<object> _explorables = new List<object>();
        private static Regex _illegalCharReplacer = new Regex(@"[\x00-\x08\x0B\x0C\x0E-\x1F]");
        private int _listNodeDepth = 0;
        private static int _runningID;
        private static Regex _whiteSpaceWithin = new Regex(@"([\t\r\n]|  )");
        private bool _wordRun;
        public const string DefaultStyles = "body\r\n{\r\n\tmargin: 0.3em 0.3em 0.4em 0.5em;\r\n\tfont-family: Verdana;\r\n\tfont-size: 80%;\r\n\tbackground: white;\r\n}\r\n\r\np, pre\r\n{\r\n\tmargin:0;\r\n\tpadding:0;\r\n\tfont-family: Verdana;\r\n}\r\n\r\ntable\r\n{\r\n\tborder-collapse: collapse;\r\n\tborder: 2px solid #17b;\r\n\tborder-top: 1px;\r\n\tmargin: 0.3em 0.2em;\r\n}\r\n\r\ntable.limit\r\n{\r\n\tborder-collapse: collapse;\r\n\tborder-bottom: 2px solid #c31;\r\n}\r\n\r\ntd, th\r\n{\r\n\tvertical-align: top;\r\n\tborder: 1px solid #aaa;\r\n\tpadding: 0.1em 0.2em;\r\n\tmargin: 0;\r\n}\r\n\r\nth\r\n{\r\n\ttext-align: left;\r\n\tbackground-color: #ddd;\r\n\tborder: 1px solid #777;\r\n\tfont-family: tahoma;\r\n\tfont-size:90%;\r\n\tfont-weight: bold;\r\n}\r\n\r\nth.member\r\n{\r\n\tpadding: 0.1em 0.2em 0.1em 0.2em;\r\n}\r\n\r\ntd.typeheader\r\n{\r\n\tfont-family: tahoma;\r\n\tfont-size: 90%;\r\n\tfont-weight: bold;\r\n\tbackground-color: #17b;\r\n\tcolor: white;\r\n\tpadding: 0 0.2em 0.1em 0.1em;\r\n}\r\n\r\ntd.n { text-align: right }\r\n\r\na:link.typeheader, a:visited.typeheader\r\n{\r\n\tfont-family: tahoma;\r\n\tfont-size: 90%;\r\n\tfont-weight: bold;\r\n\ttext-decoration: none;\r\n\tbackground-color: #17b;\r\n\tcolor: white;\r\n\tfloat:left;\r\n}\r\n\r\nspan.typeglyph\r\n{\r\n\tfont-family: webdings;\r\n\tpadding: 0 0.2em 0 0;\r\n\tmargin: 0;\r\n}\r\n\r\ntable.group\r\n{\r\n\tborder: none;\r\n\tmargin: 0;\r\n}\r\n\r\ntd.group\r\n{\r\n\tborder: none;\r\n\tpadding: 0 0.1em;\r\n}\r\n\r\ndiv.spacer\r\n{\r\n\tmargin: 0.6em 0;\r\n}\r\n\r\ntable.headingpresenter\r\n{\r\n\tborder: none;\r\n\tborder-left: 3px dotted #1a5;\r\n\tmargin: 1em 0em 1.2em 0.15em;\r\n}\r\n\r\nth.headingpresenter\r\n{\r\n\tfont-family: Arial;\r\n\tborder: none;\r\n\tpadding: 0 0 0.2em 0.5em;\r\n\tbackground-color: white;\r\n\tcolor: green;\r\n\tfont-size: 110%;        \r\n}\r\n\r\ntd.headingpresenter\r\n{\r\n\tborder: none;\r\n\tpadding: 0 0 0 0.6em;\r\n}\r\n\r\ntd.summary\r\n{ \r\n\tbackground-color: #def;\r\n\tcolor: #024;\r\n\tfont-family: Tahoma;\r\n\tpadding: 0 0.1em 0.1em 0.1em;\r\n}\r\n\r\ntd.columntotal\r\n{\r\n\tfont-family: Tahoma;\r\n\tbackground-color: #eee;\r\n\tfont-weight: bold;\r\n\tcolor: #17b;\r\n\tfont-size:90%;\r\n\ttext-align:right;\r\n}\r\n\r\nspan.graphbar\r\n{\r\n\tbackground: #17b;\r\n\tcolor: #17b;\r\n\tmargin-left: -2px;\r\n\tmargin-right: -2px;\r\n}\r\n\r\na:link.graphcolumn, a:visited.graphcolumn\r\n{\r\n\tcolor: #17b;\r\n\ttext-decoration: none;\r\n\tfont-weight: bold;\r\n\tfont-family: Arial;\r\n\tfont-size: 110%;\r\n\tletter-spacing: -0.4em;\t\r\n\tmargin-left: 0.3em;\r\n}\r\n\r\ni { color: green; }\r\n\r\nem { color: red; }\r\n\r\nspan.highlight { background: #ff8; }";
        public readonly bool EnableExpansions;
        public readonly bool EnableGraphs;
        public const string Head = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n<html>\r\n  <head>\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" />\r\n    <meta name=\"Generator\" content=\"LINQ to XML, baby!\" />";
        private const string JavaScript = "\r\n      function toggle(id)\r\n      {\r\n        table = document.getElementById(id);\r\n        if (table == null) return false;\r\n        updown = document.getElementById(id + 'ud');\r\n        if (updown == null) return false;\r\n        expand = updown.innerText == '6';\r\n        updown.innerText = expand ? '5' : '6';\r\n        table.style.borderBottom = expand ? '2px solid' : 'dashed 2px';\r\n        elements = table.rows;\r\n        if (elements.length == 0 || elements.length == 1) return false;\r\n        for (i = 1; i != elements.length; i++)\r\n          if (elements[i].id.substring(0,3) != 'sum')\r\n            elements[i].style.display = expand ? 'block' : 'none';\r\n        return false;\r\n      }\r\n    ";

        public XhtmlFormatter(bool enableExpansions, bool enableGraphs)
        {
            this.EnableExpansions = enableExpansions;
            this.EnableGraphs = enableGraphs;
        }

        public string Format(ObjectNode node)
        {
            node.Accept(this);
            return Regex.Replace(new XElement("div", node.Accept(this)).ToString(), @"[\u0080-\uFFFF]", (MatchEvaluator) (m => ("&#" + ((int) m.Value[0]).ToString() + ";")));
        }

        private static string FormatTypeName(Type t)
        {
            return FormatTypeName(t, false);
        }

        private static string FormatTypeName(Type t, bool fullname)
        {
            string str = t.FormatTypeName(fullname);
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return StripIllegal(str);
        }

        private IEnumerable<object> Get2DArrayCols(Array a, int row)
        {
            return new <Get2DArrayCols>d__2a(-2) { <>4__this = this, <>3__a = a, <>3__row = row };
        }

        private IEnumerable<IEnumerable<object>> Get2DArrayRows(Array a)
        {
            return new <Get2DArrayRows>d__23(-2) { <>4__this = this, <>3__a = a };
        }

        public string GetFooter()
        {
            return "</body>\r\n</html>";
        }

        private object GetGroupData(ListNode g)
        {
            return ((g.GroupKey == null) ? null : new XElement("table", new object[] { new XAttribute("class", "group"), new XElement("tr", new object[] { new XElement("td", new object[] { new XAttribute("class", "group"), new XElement("i", g.GroupKeyText + "=") }), new XElement("td", new object[] { new XAttribute("class", "group"), g.GroupKey.Accept(this) }) }) }));
        }

        private IEnumerable<MemberDescriptor> GetHappyFamily(ListNode g)
        {
            foreach (ObjectNode node in g.Items)
            {
                if (Util.IsMetaGraphNode(node.ObjectValue))
                {
                    return new MemberDescriptor[0];
                }
            }
            return (from <>h__TransparentIdentifier2f in from mg in g.Items.OfType<MemberNode>()
                from m in mg.Members
                select new { mg = mg, m = m }
                group <>h__TransparentIdentifier2f.m.Type by <>h__TransparentIdentifier2f.m.Name into grouped
                select new MemberDescriptor(grouped.Key, grouped.OrderBy<Type, Type>(t => t, new SubTypeComparer()).First<Type>()));
        }

        public StringBuilder GetHeader()
        {
            string str = "body\r\n{\r\n\tmargin: 0.3em 0.3em 0.4em 0.5em;\r\n\tfont-family: Verdana;\r\n\tfont-size: 80%;\r\n\tbackground: white;\r\n}\r\n\r\np, pre\r\n{\r\n\tmargin:0;\r\n\tpadding:0;\r\n\tfont-family: Verdana;\r\n}\r\n\r\ntable\r\n{\r\n\tborder-collapse: collapse;\r\n\tborder: 2px solid #17b;\r\n\tborder-top: 1px;\r\n\tmargin: 0.3em 0.2em;\r\n}\r\n\r\ntable.limit\r\n{\r\n\tborder-collapse: collapse;\r\n\tborder-bottom: 2px solid #c31;\r\n}\r\n\r\ntd, th\r\n{\r\n\tvertical-align: top;\r\n\tborder: 1px solid #aaa;\r\n\tpadding: 0.1em 0.2em;\r\n\tmargin: 0;\r\n}\r\n\r\nth\r\n{\r\n\ttext-align: left;\r\n\tbackground-color: #ddd;\r\n\tborder: 1px solid #777;\r\n\tfont-family: tahoma;\r\n\tfont-size:90%;\r\n\tfont-weight: bold;\r\n}\r\n\r\nth.member\r\n{\r\n\tpadding: 0.1em 0.2em 0.1em 0.2em;\r\n}\r\n\r\ntd.typeheader\r\n{\r\n\tfont-family: tahoma;\r\n\tfont-size: 90%;\r\n\tfont-weight: bold;\r\n\tbackground-color: #17b;\r\n\tcolor: white;\r\n\tpadding: 0 0.2em 0.1em 0.1em;\r\n}\r\n\r\ntd.n { text-align: right }\r\n\r\na:link.typeheader, a:visited.typeheader\r\n{\r\n\tfont-family: tahoma;\r\n\tfont-size: 90%;\r\n\tfont-weight: bold;\r\n\ttext-decoration: none;\r\n\tbackground-color: #17b;\r\n\tcolor: white;\r\n\tfloat:left;\r\n}\r\n\r\nspan.typeglyph\r\n{\r\n\tfont-family: webdings;\r\n\tpadding: 0 0.2em 0 0;\r\n\tmargin: 0;\r\n}\r\n\r\ntable.group\r\n{\r\n\tborder: none;\r\n\tmargin: 0;\r\n}\r\n\r\ntd.group\r\n{\r\n\tborder: none;\r\n\tpadding: 0 0.1em;\r\n}\r\n\r\ndiv.spacer\r\n{\r\n\tmargin: 0.6em 0;\r\n}\r\n\r\ntable.headingpresenter\r\n{\r\n\tborder: none;\r\n\tborder-left: 3px dotted #1a5;\r\n\tmargin: 1em 0em 1.2em 0.15em;\r\n}\r\n\r\nth.headingpresenter\r\n{\r\n\tfont-family: Arial;\r\n\tborder: none;\r\n\tpadding: 0 0 0.2em 0.5em;\r\n\tbackground-color: white;\r\n\tcolor: green;\r\n\tfont-size: 110%;        \r\n}\r\n\r\ntd.headingpresenter\r\n{\r\n\tborder: none;\r\n\tpadding: 0 0 0 0.6em;\r\n}\r\n\r\ntd.summary\r\n{ \r\n\tbackground-color: #def;\r\n\tcolor: #024;\r\n\tfont-family: Tahoma;\r\n\tpadding: 0 0.1em 0.1em 0.1em;\r\n}\r\n\r\ntd.columntotal\r\n{\r\n\tfont-family: Tahoma;\r\n\tbackground-color: #eee;\r\n\tfont-weight: bold;\r\n\tcolor: #17b;\r\n\tfont-size:90%;\r\n\ttext-align:right;\r\n}\r\n\r\nspan.graphbar\r\n{\r\n\tbackground: #17b;\r\n\tcolor: #17b;\r\n\tmargin-left: -2px;\r\n\tmargin-right: -2px;\r\n}\r\n\r\na:link.graphcolumn, a:visited.graphcolumn\r\n{\r\n\tcolor: #17b;\r\n\ttext-decoration: none;\r\n\tfont-weight: bold;\r\n\tfont-family: Arial;\r\n\tfont-size: 110%;\r\n\tletter-spacing: -0.4em;\t\r\n\tmargin-left: 0.3em;\r\n}\r\n\r\ni { color: green; }\r\n\r\nem { color: red; }\r\n\r\nspan.highlight { background: #ff8; }";
            if (File.Exists(Options.CustomStyleSheetLocation))
            {
                string str2 = File.ReadAllText(Options.CustomStyleSheetLocation);
                str = str + "\r\n" + str2 + "\r\n";
            }
            StringBuilder builder = new StringBuilder("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n  <head>\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" />\r\n    <meta name=\"Generator\" content=\"LINQ to XML, baby!\" />");
            builder.AppendLine("\r\n  <style type='text/css'>");
            builder.AppendLine(str);
            builder.AppendLine("  </style>\r\n");
            if (this.EnableExpansions)
            {
                builder.AppendLine("  <script language='JavaScript' type='text/javascript'>");
                builder.AppendLine("\r\n      function toggle(id)\r\n      {\r\n        table = document.getElementById(id);\r\n        if (table == null) return false;\r\n        updown = document.getElementById(id + 'ud');\r\n        if (updown == null) return false;\r\n        expand = updown.innerText == '6';\r\n        updown.innerText = expand ? '5' : '6';\r\n        table.style.borderBottom = expand ? '2px solid' : 'dashed 2px';\r\n        elements = table.rows;\r\n        if (elements.length == 0 || elements.length == 1) return false;\r\n        for (i = 1; i != elements.length; i++)\r\n          if (elements[i].id.substring(0,3) != 'sum')\r\n            elements[i].style.display = expand ? 'block' : 'none';\r\n        return false;\r\n      }\r\n    ");
                builder.AppendLine("  </script>");
            }
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            return builder;
        }

        private XElement GetTableDataRow(IEnumerable<MemberData> members, MemberDescriptor[] columnHeaders)
        {
            return new XElement("tr", from header in columnHeaders
                join member in members on header.Name.ToLowerInvariant() equals member.Name.ToLowerInvariant()
                select new XElement("td", new object[] { header.Type.IsNumeric() ? new XAttribute("class", "n") : null, ((IEnumerable<MemberData>) member).Any<MemberData>() ? (from r in (IEnumerable<MemberData>) member
                    orderby r.Name != header.Name
                    select r).First<MemberData>().Value.Accept(this) : "" }));
        }

        private XElement GetTableFooter(IEnumerable<MemberDescriptor> headers, ListNode ln)
        {
            if (((ln.Items.Count < 2) || (ln.Totals.Count == 0)) || ln.ItemsTruncated)
            {
                return null;
            }
            return new XElement("tr", from h in headers
                let total = ln.Totals.ContainsKey(h.Name) ? new decimal?(ln.Totals[h.Name]) : null
                select new XElement("td", new object[] { new XAttribute("title", !total.HasValue ? "Totals" : string.Concat(new object[] { "Total=", total.Value, "\r\nAverage=", Math.Round((decimal) (total.Value / ln.Counts[h.Name]), 4) })), new XAttribute("class", "columntotal"), total.HasValue ? total.ToString() : "" }));
        }

        private XElement GetTableHeader(IEnumerable<MemberDescriptor> headers, string tableID, IList<ObjectNode> data)
        {
            int itemCount = data.Count;
            return new XElement("tr", headers.Select<MemberDescriptor, XElement>((Func<MemberDescriptor, int, XElement>) ((h, i) => new XElement("th", new object[] { new XAttribute("title", FormatTypeName(h.Type, true)), h.Name, ((!this.EnableGraphs || (itemCount < 2)) || (!h.Type.IsNumeric() || ObjectNode.IsKey(h.Name, h.Type))) ? null : new XElement("a", new object[] { new XAttribute("href", ""), new XAttribute("class", "graphcolumn"), new XAttribute("title", "Show Graph"), new XAttribute("onclick", string.Concat(new object[] { "return window.external.ToggleGraphColumn('", tableID, "',", i, ")" })), "ΞΞ" }) }))));
        }

        private object GetTypeHeader(ObjectNode node, string text, int colSpan, bool expandable, string id, bool showExtenser)
        {
            object obj3;
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            bool graphTruncated = node.GraphTruncated;
            bool flag2 = node.CyclicReference != null;
            bool flag3 = (node is ListNode) && ((ListNode) node).ItemsTruncated;
            if (!((!graphTruncated && !flag2) && this.EnableExpansions))
            {
                expandable = false;
            }
            XElement element = null;
            if (!((Server.CurrentServer != null) && Server.CurrentServer.IsClrQuery))
            {
                showExtenser = false;
            }
            if (showExtenser)
            {
                element = new XElement("a", new object[] { new XAttribute("href", ""), new XAttribute("class", "typeheader"), new XAttribute("style", "float:right; padding-left:2pt; margin-left:4pt"), new XAttribute("onclick", string.Concat(new object[] { "return window.external.CustomClick('", this.Explorables.Count, "',", flag3.ToString().ToLowerInvariant(), ");" })), new XElement("span", new object[] { new XAttribute("style", "font-family: webdings; margin-top:1.2pt"), new XAttribute("id", id + "ud"), "4" }) });
                this.Explorables.Add(node.ObjectValue);
            }
            if (!(!showExtenser || expandable))
            {
                obj3 = new XElement("a", new object[] { new XAttribute("href", ""), new XAttribute("class", "typeheader"), new XAttribute("onclick", "return window.external.CustomClick('" + (this.Explorables.Count - 1) + "',false);"), new XElement("span", new object[] { new XAttribute("class", "typeglyph"), flag2 ? "q" : "4" }), text });
            }
            else if (flag2)
            {
                obj3 = new object[] { new XElement("span", new object[] { new XAttribute("class", "typeglyph"), "q" }), text };
            }
            else if (!expandable)
            {
                obj3 = text;
            }
            else
            {
                obj3 = new XElement("a", new object[] { new XAttribute("href", ""), new XAttribute("class", "typeheader"), expandable ? new XAttribute("onclick", "return toggle('" + id + "');") : null, new XElement("span", new object[] { new XAttribute("class", "typeglyph"), new XAttribute("id", id + "ud"), (!node.InitiallyHidden || !this.EnableExpansions) ? "5" : "6" }), text });
            }
            return new object[] { new XAttribute("id", id), ((!node.InitiallyHidden || !this.EnableExpansions) ? null : new XAttribute("style", "border-bottom: 2px dashed")), ((graphTruncated || flag2) ? new XAttribute("class", "limit") : null), (flag2 ? new XAttribute("title", "Cyclic reference") : null), ((!graphTruncated || flag2) ? null : new XAttribute("title", showExtenser ? "Click to explore further" : "Limit of graph")), new XElement("tr", new XElement("td", new object[] { new XAttribute("class", "typeheader"), new XAttribute("colspan", colSpan), obj3, showExtenser ? element : null })) };
        }

        public static void ResetTableID()
        {
            _runningID = 0;
        }

        private static string StripIllegal(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            return _illegalCharReplacer.Replace(s, "□");
        }

        public object Visit(EmptyNode n)
        {
            return null;
        }

        public object Visit(ListNode g)
        {
            Func<ObjectNode, XElement> selector = null;
            this._listNodeDepth++;
            object obj2 = null;
            if (g.Items.Count == 0)
            {
                string name = g.Name;
                if (!g.GraphTruncated)
                {
                    name = name + " (0 items)";
                }
                obj2 = new XElement("table", this.GetTypeHeader(g, name, 1, false, "t" + ++_runningID, g.GraphTruncated));
            }
            else
            {
                object[] objArray;
                MemberDescriptor[] happyFamily = this.GetHappyFamily(g).ToArray<MemberDescriptor>();
                if (happyFamily.Length == 0)
                {
                    objArray = new object[2];
                    objArray[0] = this.GetTypeHeader(g, string.Concat(new object[] { g.Name, " (", g.Items.Count, " item", (g.Items.Count == 1) ? ")" : "s)" }), 1, true, "t" + ++_runningID, true);
                    if (selector == null)
                    {
                        selector = subItem => new XElement("tr", new XElement("td", subItem.Accept(this)));
                    }
                    objArray[1] = g.Items.Select<ObjectNode, XElement>(selector);
                    obj2 = new XElement("table", objArray);
                }
                else
                {
                    int colCount = happyFamily.Length;
                    string id = "t" + ++_runningID;
                    objArray = new object[] { this.GetTypeHeader(g, string.Concat(new object[] { g.Name, " (", g.ItemsTruncated ? "First " : "", g.Items.Count, " item", (g.Items.Count == 1) ? ")" : "s)" }), colCount, true, id, true), this.GetTableHeader(happyFamily, id, g.Items), from subItem in g.Items
                        let memberSubItem = subItem as MemberNode
                        select (memberSubItem != null) ? this.GetTableDataRow(memberSubItem.Members, happyFamily) : new XElement("tr", new XElement("td", new object[] { new XAttribute("colspan", colCount), subItem.Accept(this) })), this.GetTableFooter(happyFamily, g) };
                    obj2 = new XElement("table", objArray);
                }
            }
            object obj3 = new object[] { this.GetGroupData(g), obj2 };
            if (g.Parent == null)
            {
                obj3 = new XElement("div", new object[] { new XAttribute("class", "spacer"), obj3 });
            }
            this._listNodeDepth--;
            return obj3;
        }

        public object Visit(MemberNode g)
        {
            Func<ObjectNode, XElement> selector = null;
            Func<MemberData, int, XElement> func2 = null;
            ListNode node;
            byte[] bytes;
            string str;
            string str2;
            if (g.ObjectValue is Highlight)
            {
                return new XElement("span", new object[] { new XAttribute("class", "highlight"), g.Members[0].Value.Accept(this) });
            }
            if (g.ObjectValue is Metatext)
            {
                return new XElement("i", g.Members[0].Value.Accept(this));
            }
            if (g.ObjectValue is FixedFont)
            {
                return new XElement("div", new object[] { new XAttribute("class", "fixedfont"), g.Members[0].Value.Accept(this) });
            }
            if ((g.ObjectValue is WordRun) && (g.Members[0].Value is ListNode))
            {
                node = (ListNode) g.Members[0].Value;
                bool withGaps = ((WordRun) g.ObjectValue).WithGaps;
                XhtmlFormatter formatter = new XhtmlFormatter(this.EnableExpansions, this.EnableGraphs) {
                    _wordRun = true
                };
                if (!withGaps)
                {
                    return (from i in node.Items select i.Accept(formatter));
                }
                return (from i in node.Items select new object[] { i.Accept(formatter), " " });
            }
            if ((g.ObjectValue is HorizontalRun) && (g.Members[0].Value is ListNode))
            {
                node = (ListNode) g.Members[0].Value;
                string style = "float:left";
                if (((HorizontalRun) g.ObjectValue).WithGaps)
                {
                    style = style + ";margin-right:0.4em";
                }
                return new XElement("div", new object[] { from i in node.Items select new XElement("div", new object[] { new XAttribute("style", style), i.Accept(this) }), new XElement("div", new object[] { new XAttribute("style", "clear:left"), " " }) });
            }
            if ((g.ObjectValue is VerticalRun) && (g.Members[0].Value is ListNode))
            {
                node = (ListNode) g.Members[0].Value;
                if (selector == null)
                {
                    selector = i => new XElement("div", i.Accept(this));
                }
                return new XElement("div", node.Items.Select<ObjectNode, XElement>(selector));
            }
            if (g.ObjectValue is Hyperlinq)
            {
                Hyperlinq objectValue = (Hyperlinq) g.ObjectValue;
                if (objectValue.IsValid)
                {
                    return new XElement("a", new object[] { new XAttribute("href", objectValue.Uri), objectValue.EditorRow.HasValue ? new XAttribute("onclick", string.Concat(new object[] { "return window.external.GotoRowColumn(", objectValue.EditorRow, ",", objectValue.EditorColumn, ")" })) : ((objectValue.Query == null) ? null : new XAttribute("onclick", "return window.external.ExecuteQuery('" + Convert.ToBase64String(Encoding.UTF8.GetBytes(objectValue.Query)) + "'," + ((int) objectValue.QueryLanguage).ToString() + ");")), objectValue.Text });
                }
                return new XElement("p", objectValue.Uri);
            }
            if (g.ObjectValue is SampleCommandLink)
            {
                SampleCommandLink link = (SampleCommandLink) g.ObjectValue;
                bytes = Encoding.UTF8.GetBytes(link.ID);
                str = Convert.ToBase64String(bytes);
                str2 = Convert.ToBase64String(SHA1.Create().ComputeHash(bytes).Take<byte>(8).ToArray<byte>());
                return new XElement("a", new object[] { new XAttribute("href", "http://" + str2), new XAttribute("onclick", "return window.external.OpenSample('" + str + "');"), link.Text });
            }
            if (g.ObjectValue is FileCommandLink)
            {
                FileCommandLink link2 = (FileCommandLink) g.ObjectValue;
                bytes = Encoding.UTF8.GetBytes(link2.FilePath);
                str = Convert.ToBase64String(bytes);
                str2 = Convert.ToBase64String(SHA1.Create().ComputeHash(bytes).Take<byte>(8).ToArray<byte>());
                return new XElement("a", new object[] { new XAttribute("href", "http://" + str2), new XAttribute("onclick", "return window.external.OpenFileQuery('" + str + "');"), link2.Text });
            }
            if (g.ObjectValue is HeadingPresenter)
            {
                if (((HeadingPresenter) g.ObjectValue).HidePresenter)
                {
                    return g.Members[1].Value.Accept(this);
                }
                object[] content = new object[2];
                content[0] = new XAttribute("class", "headingpresenter");
                if (func2 == null)
                {
                    func2 = (m, i) => new XElement("tr", new XElement((i == 0) ? "th" : "td", new object[] { new XAttribute("class", "headingpresenter"), m.Value.Accept(this) }));
                }
                content[1] = g.Members.Select<MemberData, XElement>(func2);
                return new XElement("table", content);
            }
            if (g.ObjectValue is ImageBlob)
            {
                byte[] data = ((ImageBlob) g.ObjectValue).Data;
                string randomName = TempFileRef.GetRandomName(6);
                string path = Path.Combine(Program.TempFolder, randomName);
                if ((data != null) && (data.Length > 0))
                {
                    File.WriteAllBytes(path, data);
                }
                return new XElement("img", new XAttribute("src", path));
            }
            if (g.ObjectValue is ImageRef)
            {
                return new XElement("img", new XAttribute("src", ((ImageRef) g.ObjectValue).Uri));
            }
            if (g.ObjectValue is RawHtml)
            {
                return ((RawHtml) g.ObjectValue).Html;
            }
            if ((g.Members.Count == 1) && (g.Members[0].Name == ""))
            {
                return ObjectNode.Create(g.Parent, g.Members[0].Value, g.MaxDepth, g.DCDriver).Accept(this);
            }
            object rowDisplay = (!g.InitiallyHidden || !this.EnableExpansions) ? null : new XAttribute("style", "display:none");
            object obj3 = this.GetTypeHeader(g, g.Name, 2, g.Members.Count > 0, "t" + ++_runningID, ((g.Members.Count > 0) || g.GraphTruncated) || (g.CyclicReference != null));
            object obj4 = new XElement("table", new object[] { obj3, (g.Summary.Length == 0) ? null : new XElement("tr", new object[] { new XAttribute("id", "sum" + ++_runningID), new XElement("td", new object[] { new XAttribute("colspan", 2), new XAttribute("class", "summary"), StripIllegal(g.Summary) }) }), from m in g.Members select new XElement("tr", new object[] { rowDisplay, new XElement("th", new object[] { new XAttribute("class", "member"), new XAttribute("title", FormatTypeName(m.Type, true)), StripIllegal(m.Name) }), new XElement("td", m.Value.Accept(this)) }) });
            if (g.Parent == null)
            {
                obj4 = new XElement("div", new object[] { new XAttribute("class", "spacer"), obj4 });
            }
            return obj4;
        }

        public object Visit(MultiDimArrayNode g)
        {
            string id = "t" + ++_runningID;
            int length = g.Data.GetLength(0);
            int num2 = g.Data.GetLength(1);
            bool flag = g.Data.Length > 0x9c40;
            return new XElement("table", new object[] { this.GetTypeHeader(g, string.Concat(new object[] { FormatTypeName(g.ElementType), " [", length, ",", num2, "]", flag ? " (truncated)" : "" }), length + 1, true, id, false), new XElement("tr", from i in Enumerable.Range(-1, num2 + 1) select new XElement("th", (i < 0) ? "" : i.ToString())), this.Get2DArrayRows(g.Data).Select<IEnumerable<object>, XElement>((Func<IEnumerable<object>, int, XElement>) ((row, i) => new XElement("tr", new object[] { new XElement("th", i), from col in row select new XElement("td", (col == null) ? ((object) new XElement("i", "(null)")) : ((object) col.ToString())) }))) });
        }

        public object Visit(SimpleNode g)
        {
            XElement element;
            string content = StripIllegal(g.Text);
            uint? maxColumnWidthInLists = UserOptions.Instance.MaxColumnWidthInLists;
            if ((maxColumnWidthInLists.HasValue && (this._listNodeDepth > 0)) && (content.Length > ((ulong) maxColumnWidthInLists.Value)))
            {
                content = content.Substring(0, (int) (maxColumnWidthInLists.Value - 3)) + "...";
            }
            if (g.NodeKind == SimpleNodeKind.Metadata)
            {
                element = new XElement("i", content);
            }
            else if (g.NodeKind == SimpleNodeKind.Warning)
            {
                element = new XElement("em", content);
            }
            else if (content == "\r\n")
            {
                element = new XElement("br");
            }
            else if (!(this._wordRun || ((!content.StartsWith(" ", StringComparison.Ordinal) && !content.EndsWith(" ", StringComparison.Ordinal)) && !_whiteSpaceWithin.IsMatch(content))))
            {
                element = XElement.Parse(new XElement("span", content).ToString().Replace("\r\n", "<br />").Replace("\t", "  ").Replace("  ", "&#160; ").Replace("  ", "&#160; ").Replace("> ", ">&#160;").Replace(" <", "&#160;<"));
                if (string.IsNullOrEmpty(g.ToolTip))
                {
                    return element.Nodes();
                }
            }
            else
            {
                if (this._wordRun)
                {
                    if (!content.Contains("\r\n"))
                    {
                        return content;
                    }
                    List<object> list = (from line in content.Split(new string[] { "\r\n" }, StringSplitOptions.None) select new object[] { line, new XElement("br") }).ToList<object>();
                    list.RemoveAt(list.Count - 1);
                    return list;
                }
                if (content.Trim().Length == 0)
                {
                    element = XElement.Parse("<span>&#160;</span>");
                    if (string.IsNullOrEmpty(g.ToolTip))
                    {
                        return element.FirstNode;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(g.ToolTip))
                    {
                        return content;
                    }
                    element = new XElement("span", content);
                }
            }
            if (!string.IsNullOrEmpty(g.ToolTip))
            {
                element.Add(new XAttribute("title", StripIllegal(g.ToolTip)));
            }
            return element;
        }

        public List<object> Explorables
        {
            get
            {
                return this._explorables;
            }
        }

        [CompilerGenerated]
        private sealed class <Get2DArrayCols>d__2a : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private object <>2__current;
            public Array <>3__a;
            public int <>3__row;
            public XhtmlFormatter <>4__this;
            private int <>l__initialThreadId;
            public int <i>5__2c;
            public int <len>5__2b;
            public Array a;
            public int row;

            [DebuggerHidden]
            public <Get2DArrayCols>d__2a(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    if (this.<>1__state != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<len>5__2b = this.a.GetLength(1);
                        this.<i>5__2c = 0;
                    }
                    else
                    {
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>1__state = 0;
                        this.<i>5__2c++;
                    }
                    if ((this.<i>5__2c < this.<len>5__2b) && ((this.a.Length <= 0x9c40) || (this.<i>5__2c <= 200)))
                    {
                        this.<>2__current = this.a.GetValue(this.row, this.<i>5__2c);
                        this.<>1__state = 1;
                        return true;
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                XhtmlFormatter.<Get2DArrayCols>d__2a d__a;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__a = this;
                }
                else
                {
                    d__a = new XhtmlFormatter.<Get2DArrayCols>d__2a(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__a.a = this.<>3__a;
                d__a.row = this.<>3__row;
                return d__a;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Object>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <Get2DArrayRows>d__23 : IEnumerable<IEnumerable<object>>, IEnumerable, IEnumerator<IEnumerable<object>>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private IEnumerable<object> <>2__current;
            public Array <>3__a;
            public XhtmlFormatter <>4__this;
            private int <>l__initialThreadId;
            public int <cols>5__25;
            public int <i>5__27;
            public int <maxRowsToDisplay>5__26;
            public int <rows>5__24;
            public Array a;

            [DebuggerHidden]
            public <Get2DArrayRows>d__23(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    if (this.<>1__state != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<rows>5__24 = this.a.GetLength(0);
                        this.<cols>5__25 = this.a.GetLength(1);
                        this.<maxRowsToDisplay>5__26 = this.a.Length / Math.Max(1, this.a.GetLength(1));
                        this.<i>5__27 = 0;
                    }
                    else
                    {
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>1__state = 0;
                        this.<i>5__27++;
                    }
                    if ((this.<i>5__27 < this.<rows>5__24) && ((this.a.Length <= 0x9c40) || (this.<i>5__27 <= this.<maxRowsToDisplay>5__26)))
                    {
                        this.<>2__current = this.<>4__this.Get2DArrayCols(this.a, this.<i>5__27);
                        this.<>1__state = 1;
                        return true;
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<IEnumerable<object>> IEnumerable<IEnumerable<object>>.GetEnumerator()
            {
                XhtmlFormatter.<Get2DArrayRows>d__23 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new XhtmlFormatter.<Get2DArrayRows>d__23(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.a = this.<>3__a;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Object>>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            IEnumerable<object> IEnumerator<IEnumerable<object>>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        private class MemberDescriptor
        {
            public readonly string Name;
            public readonly System.Type Type;

            public MemberDescriptor(string name, System.Type type)
            {
                this.Name = name;
                this.Type = type;
            }
        }
    }
}

