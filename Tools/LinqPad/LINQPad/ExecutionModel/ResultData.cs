namespace LINQPad.ExecutionModel
{
    using System;
    using System.Diagnostics;

    [Serializable]
    internal class ResultData
    {
        private static Stopwatch _stopWatch = Stopwatch.StartNew();
        public bool? AutoScrollResults;
        public string Lambda = "";
        public bool MessageLoopFailed;
        public string Output = "";
        public string SQL = "";
    }
}

