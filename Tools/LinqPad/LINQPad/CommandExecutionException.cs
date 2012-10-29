namespace LINQPad
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class CommandExecutionException : Exception
    {
        public CommandExecutionException(string msg) : base(msg)
        {
            this.ErrorText = msg;
        }

        public CommandExecutionException(string msg, int exitCode) : base(string.IsNullOrEmpty(msg) ? ("The process returned an exit code of " + exitCode) : msg)
        {
            this.ErrorText = msg;
            this.ExitCode = exitCode;
        }

        public string ErrorText { get; private set; }

        public int ExitCode { get; private set; }
    }
}

