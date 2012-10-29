namespace LINQPad
{
    using System;

    internal enum SchemaChangeTestMode
    {
        None,
        TestAndFailNegative,
        TestAndFailPositive,
        ForceRefresh
    }
}

