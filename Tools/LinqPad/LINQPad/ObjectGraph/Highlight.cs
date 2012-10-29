namespace LINQPad.ObjectGraph
{
    using System;

    [MetaGraphNode]
    internal class Highlight
    {
        public readonly object Data;

        public Highlight(object data)
        {
            this.Data = data;
        }
    }
}

