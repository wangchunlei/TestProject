namespace LINQPad.Extensibility.DataContext
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ObjectGraphInfo
    {
        private static object _displayNothingToken = new object();

        internal ObjectGraphInfo(string heading, IEnumerable<object> parentHierarchy)
        {
            this.Heading = heading;
            this.ParentHierarchy = parentHierarchy;
        }

        internal static object GetDisplayNothingToken()
        {
            return _displayNothingToken;
        }

        public object DisplayNothingToken
        {
            get
            {
                return _displayNothingToken;
            }
        }

        public string Heading { get; private set; }

        public IEnumerable<object> ParentHierarchy { get; private set; }
    }
}

