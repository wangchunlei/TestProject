namespace LINQPad.ObjectGraph
{
    using System;
    using System.Xml.Linq;

    [MetaGraphNode]
    internal class RawHtml
    {
        public readonly XElement Html;

        public RawHtml(XElement xhtmlNode)
        {
            this.Html = xhtmlNode;
        }
    }
}

