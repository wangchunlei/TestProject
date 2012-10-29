namespace LINQPad.ObjectGraph
{
    using System;

    [MetaGraphNode]
    internal class SampleCommandLink
    {
        public readonly string ID;
        public readonly string Text;

        public SampleCommandLink(string id, string text)
        {
            this.ID = id;
            this.Text = text;
        }
    }
}

