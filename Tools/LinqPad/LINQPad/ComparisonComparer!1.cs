namespace LINQPad
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class ComparisonComparer<T> : IComparer<T>, IComparer
    {
        private Comparison<T> _comparison;

        public ComparisonComparer(Comparison<T> comparison)
        {
            this._comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return this._comparison(x, y);
        }

        public int Compare(object x, object y)
        {
            return this._comparison((T) x, (T) y);
        }
    }
}

