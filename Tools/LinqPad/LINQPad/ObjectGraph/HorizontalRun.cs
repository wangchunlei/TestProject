namespace LINQPad.ObjectGraph
{
    using System;
    using System.Collections;

    [MetaGraphNode]
    internal class HorizontalRun
    {
        public readonly IEnumerable Elements;
        public bool WithGaps;

        public HorizontalRun(bool withGaps, IEnumerable elements)
        {
            this.WithGaps = withGaps;
            this.Elements = elements;
        }
    }
}

