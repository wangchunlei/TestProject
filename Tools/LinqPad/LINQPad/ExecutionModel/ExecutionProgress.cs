namespace LINQPad.ExecutionModel
{
    using System;

    internal enum ExecutionProgress
    {
        Starting,
        AwaitingDataContext,
        Compiling,
        Executing,
        Async,
        Finished
    }
}

