namespace LINQPad
{
    using System;
    using System.Collections.Generic;

    internal class SubTypeComparer : Comparer<Type>
    {
        public override int Compare(Type x, Type y)
        {
            if (!object.Equals(x, y))
            {
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                int num2 = y.IsAssignableFrom(x).CompareTo(x.IsAssignableFrom(y));
                if (num2 != 0)
                {
                    return num2;
                }
                if (!(!x.IsGenericType || y.IsGenericType))
                {
                    return 1;
                }
                if (!(!y.IsGenericType || x.IsGenericType))
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}

