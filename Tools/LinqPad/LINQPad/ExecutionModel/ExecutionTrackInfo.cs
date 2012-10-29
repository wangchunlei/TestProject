namespace LINQPad.ExecutionModel
{
    using System;

    [Serializable]
    internal class ExecutionTrackInfo
    {
        public int Cost;
        public RowColumn[] MainThreadStack;
    }
}

