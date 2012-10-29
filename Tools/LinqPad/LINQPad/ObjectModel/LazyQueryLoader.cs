namespace LINQPad.ObjectModel
{
    using System;

    internal abstract class LazyQueryLoader
    {
        public readonly LINQPad.ObjectModel.Query Query;

        public LazyQueryLoader(LINQPad.ObjectModel.Query q)
        {
            this.Query = q;
        }

        public abstract string GetData();
        public abstract void Open();

        public abstract object OpenLink { get; }
    }
}

