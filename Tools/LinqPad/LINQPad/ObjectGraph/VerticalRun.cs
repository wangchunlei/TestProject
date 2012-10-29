namespace LINQPad.ObjectGraph
{
    using System;
    using System.Collections;

    [MetaGraphNode]
    internal class VerticalRun
    {
        public readonly IEnumerable Elements;

        public VerticalRun(IEnumerable elements)
        {
            this.Elements = elements;
        }
    }
}

