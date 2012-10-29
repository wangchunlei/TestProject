namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class FileNameComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if ((x == null) || (y == null))
            {
                return ((x == null) && (y == null));
            }
            if (x.Contains<char>('\\'))
            {
                x = x.Substring(x.LastIndexOf('\\') + 1);
            }
            if (y.Contains<char>('\\'))
            {
                y = y.Substring(y.LastIndexOf('\\') + 1);
            }
            return x.Equals(y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string x)
        {
            if (x == null)
            {
                return 0;
            }
            if (x.Contains<char>('\\'))
            {
                x = x.Substring(x.LastIndexOf('\\') + 1);
            }
            return x.ToLowerInvariant().GetHashCode();
        }
    }
}

