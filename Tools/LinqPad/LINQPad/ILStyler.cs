namespace LINQPad
{
    using System;

    internal class ILStyler
    {
        public virtual string StyleIdentifier(string s)
        {
            return s;
        }

        public virtual string StyleOpCode(string s)
        {
            return s;
        }

        public virtual string StyleString(string s)
        {
            return s;
        }
    }
}

