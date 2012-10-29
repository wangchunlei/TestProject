namespace LINQPad
{
    using System;
    using System.Collections.Generic;

    internal class AnonymousEqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> _equals;
        private Func<T, int> _getHashCode;

        public AnonymousEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            this._equals = equals;
            this._getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            return this._equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return this._getHashCode(obj);
        }
    }
}

