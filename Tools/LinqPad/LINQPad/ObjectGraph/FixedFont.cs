namespace LINQPad.ObjectGraph
{
    using System;

    [MetaGraphNode]
    internal class FixedFont
    {
        public readonly object Data;

        public FixedFont(object data)
        {
            this.Data = data;
        }
    }
}

