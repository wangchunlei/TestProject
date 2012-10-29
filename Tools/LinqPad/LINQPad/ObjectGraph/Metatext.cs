namespace LINQPad.ObjectGraph
{
    using System;

    [MetaGraphNode]
    internal class Metatext
    {
        public readonly string Text;

        public Metatext(string text)
        {
            this.Text = text;
        }
    }
}

