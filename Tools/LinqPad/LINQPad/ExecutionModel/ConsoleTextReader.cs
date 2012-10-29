namespace LINQPad.ExecutionModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class ConsoleTextReader : TextReader
    {
        private Queue<char> _chars;
        private Func<string> _readLineFunc;

        public ConsoleTextReader(Func<string> readLineFunc)
        {
            this._readLineFunc = readLineFunc;
        }

        public override int Peek()
        {
            return -1;
        }

        public override int Read()
        {
            if (this._chars == null)
            {
                this._chars = new Queue<char>(this._readLineFunc() ?? "");
            }
            if (this._chars.Count == 0)
            {
                this._chars = null;
                return 13;
            }
            return this._chars.Dequeue();
        }
    }
}

