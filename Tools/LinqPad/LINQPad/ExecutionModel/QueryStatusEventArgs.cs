namespace LINQPad.ExecutionModel
{
    using System;

    [Serializable]
    internal class QueryStatusEventArgs : EventArgs
    {
        public bool AppDomainRecycleSuggested;
        public bool Async;
        public bool DataContextRefreshRequired;
        public int ErrorColumn;
        public string ErrorFileName;
        public int ErrorLine;
        public string ErrorMessage;
        public TimeSpan ExecTime;
        public bool ExecutionComplete;
        public bool IsInfo;
        public bool IsWarning;
        public int[] StackTraceColumns;
        public int[] StackTraceLines;
        public string StatusMessage;

        public QueryStatusEventArgs()
        {
            this.ExecutionComplete = true;
            this.StatusMessage = "";
            this.ErrorMessage = "";
            this.ErrorFileName = "";
        }

        public QueryStatusEventArgs(string statusMessage)
        {
            this.ExecutionComplete = true;
            this.StatusMessage = "";
            this.ErrorMessage = "";
            this.ErrorFileName = "";
            this.StatusMessage = statusMessage;
        }

        public QueryStatusEventArgs(string statusMessage, string errorMessage)
        {
            this.ExecutionComplete = true;
            this.StatusMessage = "";
            this.ErrorMessage = "";
            this.ErrorFileName = "";
            this.StatusMessage = statusMessage;
            this.ErrorMessage = errorMessage;
        }
    }
}

