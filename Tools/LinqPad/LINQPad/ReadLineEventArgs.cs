namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using System;

    internal class ReadLineEventArgs : EventArgs
    {
        public LINQPad.ExecutionModel.Client Client;
        public string DefaultValue;
        public string[] Options;
        public string Prompt;
    }
}

