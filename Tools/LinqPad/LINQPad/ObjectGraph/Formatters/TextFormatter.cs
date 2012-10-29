namespace LINQPad.ObjectGraph.Formatters
{
    using LINQPad.ObjectGraph;
    using System;
    using System.Linq;
    using System.Text;

    internal class TextFormatter : IObjectGraphVisitor
    {
        private StringBuilder _builder = new StringBuilder();
        private int _indents;
        private bool _needNewLine = true;

        public string FormatGraph(ObjectNode g)
        {
            g.Accept(this);
            return this._builder.ToString();
        }

        public object Visit(EmptyNode g)
        {
            return "";
        }

        public object Visit(ListNode g)
        {
            if (g.GroupKey == null)
            {
                this.Write("<List>", false, true);
            }
            else
            {
                this.Write("<Key=", false, false);
                g.GroupKey.Accept(this);
                this.Write(">", false, true);
            }
            this._indents++;
            foreach (ObjectNode node in g.Items)
            {
                node.Accept(this);
                this._needNewLine = true;
            }
            this._indents--;
            return this._builder;
        }

        public object Visit(MemberNode g)
        {
            bool flag;
            if (g.Name != null)
            {
                this.Write("[" + g.Name + "] ", false, false);
            }
            MemberData[] dataArray = (from m in g.Members
                where m.Value is SimpleNode
                select m).ToArray<MemberData>();
            MemberData[] dataArray2 = (from m in g.Members
                where !(m.Value is SimpleNode)
                select m).ToArray<MemberData>();
            if (flag = ((dataArray.Length <= 0) || (dataArray2.Length <= 0)) ? (dataArray2.Length > 1) : true)
            {
                this.Write("{", false, true);
                this._indents++;
            }
            else
            {
                this.Write("{ ", false, false);
            }
            bool flag2 = true;
            foreach (MemberData data in dataArray)
            {
                if (!flag2)
                {
                    this.Write(" ", false, false);
                }
                this.Write(data.Name + "=", false, false);
                data.Value.Accept(this);
                this.Write(" ", false, false);
                flag2 = false;
            }
            foreach (MemberData data in dataArray2)
            {
                this.Write(data.Name + "=", true, false);
                data.Value.Accept(this);
            }
            if (flag)
            {
                this._indents--;
            }
            this.Write("}", false, true);
            return this._builder;
        }

        public object Visit(MultiDimArrayNode g)
        {
            return "";
        }

        public object Visit(SimpleNode g)
        {
            this.Write(g.Text ?? "<null>", false, false);
            return this._builder;
        }

        private void Write(string s, bool firstThingOnLine, bool lastThingOnLine)
        {
            if (this._needNewLine || firstThingOnLine)
            {
                if (this._builder.Length > 0)
                {
                    this._builder.AppendLine();
                }
                this._builder.Append(this.Indent);
            }
            this._builder.Append(s);
            this._needNewLine = lastThingOnLine;
        }

        private string Indent
        {
            get
            {
                return "".PadRight(this._indents, '\t');
            }
        }
    }
}

