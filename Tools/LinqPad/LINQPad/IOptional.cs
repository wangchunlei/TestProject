namespace LINQPad
{
    using System;

    internal interface IOptional
    {
        bool HasValue { get; }

        object Value { get; }
    }
}

