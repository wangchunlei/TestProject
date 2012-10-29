namespace LINQPad.ObjectGraph
{
    using System;
    using System.Collections;

    [MetaGraphNode]
    internal class WordRun
    {
        public readonly IEnumerable Elements;
        public bool WithGaps;

        public WordRun(bool withGaps, IEnumerable elements)
        {
            this.WithGaps = withGaps;
            this.Elements = elements;
        }
    }
}

