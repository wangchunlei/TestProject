namespace LINQPad
{
    using System;

    internal class RepositoryDragData
    {
        public readonly string DragText;
        public readonly LINQPad.Repository Repository;

        public RepositoryDragData(LINQPad.Repository r)
        {
            this.Repository = r;
        }

        public RepositoryDragData(LINQPad.Repository r, string dragText) : this(r)
        {
            this.DragText = dragText;
        }
    }
}

