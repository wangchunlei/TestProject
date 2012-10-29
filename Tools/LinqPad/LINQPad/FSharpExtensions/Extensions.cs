namespace LINQPad.FSharpExtensions
{
    using LINQPad;
    using System;
    using System.Runtime.CompilerServices;

    public static class Extensions
    {
        public static void Dump(this object o)
        {
            o.Dump(null, null);
        }

        public static void Dump(this object o, int maximumDepth)
        {
            o.Dump(null, new int?(maximumDepth));
        }

        public static void Dump(this object o, string description)
        {
            o.Dump(description, null);
        }

        public static void Dump(this object o, string description, int? maximumDepth)
        {
            o.Dump<object>(description, maximumDepth);
        }
    }
}

