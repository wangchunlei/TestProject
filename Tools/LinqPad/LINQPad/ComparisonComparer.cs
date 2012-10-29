namespace LINQPad
{
    using System;
    using System.Collections;

    internal class ComparisonComparer : IComparer
    {
        private Func<object, object, int> _comparison;

        public ComparisonComparer(Func<object, object, int> comparison)
        {
            this._comparison = comparison;
        }

        public int Compare(object x, object y)
        {
            return this._comparison(x, y);
        }
    }
}

