namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using LINQPad.Expressions;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.ObjectGraph;
    using LINQPad.ObjectGraph.Formatters;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Linq;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;

    public static class Extensions
    {
        public static T[] Cache<T>(this IEnumerable<T> query)
        {
            return UserCache.CacheSequence<T>(query, null);
        }

        public static T[] Cache<T>(this IEnumerable<T> query, string key)
        {
            return UserCache.CacheSequence<T>(query, key);
        }

        public static string Disassemble(this MethodBase method)
        {
            return Disassembler.Disassemble(method);
        }

        public static T Dump<T>(this T o)
        {
            return o.Dump<T>(null, ((int?) null));
        }

        public static T Dump<T>(this T o, bool toDataGrid)
        {
            return o.Dump<T>(null, null, toDataGrid, null);
        }

        public static T Dump<T>(this T o, int depth)
        {
            return o.Dump<T>(null, new int?(depth));
        }

        public static T Dump<T>(this T o, string description)
        {
            return o.Dump<T>(description, ((int?) null));
        }

        public static T Dump<T>(this T o, string description, int? depth)
        {
            return o.Dump<T>(description, depth, false, null);
        }

        public static T Dump<T>(this T o, string description, bool toDataGrid)
        {
            return o.Dump<T>(description, null, toDataGrid, null);
        }

        private static T Dump<T>(this T o, string description, int? depth, bool toDataGrid, Action<ClickContext> onClick)
        {
            if (o != null)
            {
                Type type = o.GetType();
                if ((((!type.IsValueType && !type.IsPrimitive) && (type.Namespace != null)) && (type.Namespace.StartsWith("System.Threading.Tasks.Dataflow") && (type.GetInterface("System.Threading.Tasks.Dataflow.IDataflowBlock") != null))) && DataflowBridge.Dump(o, description))
                {
                    return o;
                }
            }
            if (depth < 0)
            {
                depth = null;
            }
            if (depth > 20)
            {
                depth = 20;
            }
            if (((depth.HasValue || toDataGrid) || !AppDomain.CurrentDomain.GetAssemblies().Any<Assembly>(a => a.FullName.StartsWith("PresentationCore,", StringComparison.InvariantCultureIgnoreCase))) || !WpfBridge.DumpWpfElement(o, description))
            {
                bool flag2;
                if ((!depth.HasValue && !toDataGrid) && (o is Control))
                {
                    if (o is Form)
                    {
                        ((Form) o).Show();
                        return o;
                    }
                    PanelManager.DisplayControl((Control) o, description ?? "WinForms");
                    return o;
                }
                if ((o is XContainer) || (o is XmlDocument))
                {
                    PanelManager.DisplaySyntaxColoredText(o.ToString(), SyntaxLanguageStyle.XML, description ?? "XML");
                    return o;
                }
                Server currentServer = Server.CurrentServer;
                TextWriter writer = (currentServer == null) ? null : currentServer.LambdaFormatter;
                Expression expr = null;
                if (writer != null)
                {
                    if (o is IQueryable)
                    {
                        expr = ((IQueryable) o).Expression;
                    }
                    else if (o is Expression)
                    {
                        expr = (Expression) o;
                    }
                }
                if (expr != null)
                {
                    string content = "";
                    try
                    {
                        ExpressionToken token = ExpressionToken.Visit(expr);
                        if (token != null)
                        {
                            content = token.ToString();
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Write(exception, "Dump ExpressionToken Visit");
                    }
                    if (content.Length > 0)
                    {
                        lock (writer)
                        {
                            if (!string.IsNullOrEmpty(description))
                            {
                                writer.WriteLine(new HeadingPresenter(description, content));
                            }
                            else
                            {
                                writer.WriteLine(content + "\r\n");
                            }
                        }
                    }
                }
                if ((currentServer != null) && currentServer.WriteResultsToGrids)
                {
                    toDataGrid = true;
                }
                if (toDataGrid && (o != null))
                {
                    Type t = o.GetType();
                    if (((!ExplorerGrid.IsAtomic(t) && (!t.IsArray || (t.GetArrayRank() == 1))) && (t.GetInterface("System.IObservable`1") == null)) && (t.GetCustomAttributes(typeof(MetaGraphNodeAttribute), false).Length == 0))
                    {
                        return o.Explore<T>(description);
                    }
                }
                XhtmlWriter writer3 = (currentServer == null) ? null : currentServer.ResultsWriter;
                if (flag2 = o is Type)
                {
                    ObjectNode.ExpandTypes = true;
                }
                try
                {
                    if (!string.IsNullOrEmpty(description))
                    {
                        HeadingPresenter presenter = new HeadingPresenter(description, o);
                        if (writer3 != null)
                        {
                            writer3.WriteDepth(presenter, depth, onClick);
                        }
                        else
                        {
                            Console.Write(presenter);
                        }
                        return o;
                    }
                    if (writer3 != null)
                    {
                        writer3.WriteLineDepth(o, depth, onClick);
                    }
                    else
                    {
                        Console.WriteLine(o);
                    }
                }
                finally
                {
                    if (flag2)
                    {
                        ObjectNode.ExpandTypes = false;
                    }
                }
            }
            return o;
        }

        public static XDocument DumpFormatted(this XDocument xml)
        {
            return xml.DumpFormatted(null);
        }

        public static XElement DumpFormatted(this XElement xml)
        {
            return xml.DumpFormatted(null);
        }

        public static XmlDocument DumpFormatted(this XmlDocument xml)
        {
            return xml.DumpFormatted(null);
        }

        public static XmlElement DumpFormatted(this XmlElement xml)
        {
            return xml.DumpFormatted(null);
        }

        public static XDocument DumpFormatted(this XDocument xml, string heading)
        {
            if (string.IsNullOrEmpty(heading))
            {
                heading = "XML";
            }
            PanelManager.DisplaySyntaxColoredText(xml.ToString(), SyntaxLanguageStyle.XML, heading);
            return xml;
        }

        public static XElement DumpFormatted(this XElement xml, string heading)
        {
            if (string.IsNullOrEmpty(heading))
            {
                heading = "XML";
            }
            PanelManager.DisplaySyntaxColoredText(xml.ToString(), SyntaxLanguageStyle.XML, heading);
            return xml;
        }

        public static XmlDocument DumpFormatted(this XmlDocument xml, string heading)
        {
            if (string.IsNullOrEmpty(heading))
            {
                heading = "XML";
            }
            PanelManager.DisplaySyntaxColoredText(XmlHelper.ToFormattedString(xml), SyntaxLanguageStyle.XML, heading);
            return xml;
        }

        public static XmlElement DumpFormatted(this XmlElement xml, string heading)
        {
            if (string.IsNullOrEmpty(heading))
            {
                heading = "XML";
            }
            PanelManager.DisplaySyntaxColoredText(XmlHelper.ToFormattedString(xml), SyntaxLanguageStyle.XML, heading);
            return xml;
        }

        public static IObservable<T> DumpLive<T>(this IObservable<T> obs)
        {
            return obs.DumpLive<T>(null);
        }

        public static IObservable<T> DumpLive<T>(this IObservable<T> obs, string heading)
        {
            LiveDumper.DumpLive<T>(obs, heading);
            return obs;
        }

        internal static DataSet ExecuteDataSet(this DbCommand cmd)
        {
            if ((cmd == null) || (cmd.Connection == null))
            {
                return null;
            }
            DbConnection connection = cmd.Connection;
            using (DbDataAdapter adapter = cmd.Connection.GetFactory().CreateDataAdapter())
            {
                adapter.SelectCommand = cmd;
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                return dataSet;
            }
        }

        internal static T Explore<T>(this T o, string panelTitle)
        {
            if (o != null)
            {
                GridOptions options2 = new GridOptions {
                    MembersToExclude = null,
                    PanelTitle = panelTitle
                };
                Server currentServer = Server.CurrentServer;
                if ((currentServer != null) && (currentServer.DataContextDriver != null))
                {
                    currentServer.DataContextDriver.DisplayObjectInGrid(o, options2);
                }
                else
                {
                    ExplorerGrid.Display(o, options2);
                }
            }
            return o;
        }

        internal static DbProviderFactory GetFactory(this DbConnection cx)
        {
            if (cx == null)
            {
                return null;
            }
            PropertyInfo property = cx.GetType().GetProperty("DbProviderFactory", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                return null;
            }
            return (property.GetValue(cx, null) as DbProviderFactory);
        }

        public static object ToImage(this Binary imageData)
        {
            return Util.Image(imageData);
        }
    }
}

