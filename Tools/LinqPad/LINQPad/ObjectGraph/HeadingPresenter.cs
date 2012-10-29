namespace LINQPad.ObjectGraph
{
    using System;

    [MetaGraphNode]
    internal class HeadingPresenter
    {
        public object Content;
        public object Heading;
        internal bool HidePresenter;

        public HeadingPresenter(string heading, object content)
        {
            this.Heading = heading;
            this.Content = content;
        }
    }
}

