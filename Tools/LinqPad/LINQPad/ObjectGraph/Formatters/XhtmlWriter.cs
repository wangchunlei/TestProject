namespace LINQPad.ObjectGraph.Formatters
{
    using LINQPad.Extensibility.DataContext;
    using LINQPad.ObjectGraph;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class XhtmlWriter : TextWriter
    {
        private StringBuilder _buffer = new StringBuilder();
        private string _bufferSnapshot = "";
        private StringBuilder _data;
        private readonly XhtmlFormatter _formatter;
        private TimeSpan _formattingTime;
        private int _headerLength;
        private bool _limit;
        private object _locker = new object();
        private DateTimeOffset _nextBufferUpdate;
        private int _writes;
        internal DataContextDriver DCDriver;
        internal bool HasTypeReferences;
        public int? MaxDepth;

        public XhtmlWriter(bool enableExpansions, bool enableGraphs)
        {
            this._formatter = new XhtmlFormatter(enableExpansions, enableGraphs);
            this._data = this._formatter.GetHeader();
            this._headerLength = this._data.Length;
        }

        public void Clear()
        {
            lock (this._locker)
            {
                this._data = this._formatter.GetHeader();
                this._buffer.Length = 0;
                this._bufferSnapshot = "";
                this._writes = 0;
            }
        }

        private string Format(ObjectNode node)
        {
            return this._formatter.Format(node);
        }

        public int GetLength()
        {
            lock (this._locker)
            {
                return (this._data.Length + this._buffer.Length);
            }
        }

        public override string ToString()
        {
            int num;
            return this.ToString(out num);
        }

        public string ToString(out int logicalLength)
        {
            string str;
            string str2;
            lock (this._locker)
            {
                str = this._data.ToString();
                str2 = this._buffer.ToString();
            }
            logicalLength = str.Length + str2.Length;
            if ((str.Length <= this._headerLength) && (str2.Length == 0))
            {
                return "";
            }
            if (str2.Length > 0)
            {
                str = str + this.Format(ObjectNode.Create(str2.ToString(), null, null)) + "\r\n";
            }
            return (str + this._formatter.GetFooter());
        }

        private void UpdateBufferSnapshot()
        {
            if ((this._bufferSnapshot.Length < 0x3e8) || (DateTimeOffset.Now > this._nextBufferUpdate))
            {
                this._bufferSnapshot = this._buffer.ToString();
                this._nextBufferUpdate = DateTimeOffset.Now.AddMilliseconds(200.0);
            }
        }

        public override void Write(char value)
        {
            lock (this._locker)
            {
                this._buffer.Append(value);
                this.UpdateBufferSnapshot();
            }
        }

        public override void Write(object value)
        {
            this.WriteDepth(value, this.MaxDepth);
        }

        public override void Write(string value)
        {
            if (value == null)
            {
                this.Write(null);
            }
            else
            {
                lock (this._locker)
                {
                    this._buffer.Append(value);
                    this.UpdateBufferSnapshot();
                }
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            this.Write(new string(buffer, index, count));
        }

        public void WriteDepth(object value, int? maxDepth)
        {
            this.WriteDepth(value, maxDepth, null);
        }

        internal void WriteDepth(object value, int? maxDepth, Action<ClickContext> onClick)
        {
            if (!(((this._data.Length <= 0xf4240) || (this._writes <= 100)) ? (this._data.Length <= 0x4c4b40) : false))
            {
                if (!this._limit)
                {
                    this.Write("<limit of graph>");
                }
                this._limit = true;
            }
            else if (value is string)
            {
                this.Write((string) value);
            }
            else
            {
                ObjectNode node = ObjectNode.Create(value, maxDepth, this.DCDriver);
                node.OnClick = onClick;
                if (node.HasTypeReferences)
                {
                    this.HasTypeReferences = true;
                }
                Stopwatch stopwatch = Stopwatch.StartNew();
                string str = this.Format(node);
                lock (this._locker)
                {
                    this._formattingTime += stopwatch.Elapsed;
                    if (this._buffer.Length > 0)
                    {
                        this._data.AppendLine(this.Format(ObjectNode.Create(this._buffer.ToString(), null, null)));
                        this._buffer = new StringBuilder();
                    }
                    this._data.AppendLine(str);
                    this._bufferSnapshot = "";
                    this._writes++;
                }
            }
        }

        public override void WriteLine(bool value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine(char value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine(decimal value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine(double value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine(int value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine(long value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine(object value)
        {
            this.WriteLineDepth(value, this.MaxDepth);
        }

        public override void WriteLine(float value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine(string value)
        {
            if (value == null)
            {
                this.Write(null);
            }
            else
            {
                lock (this._locker)
                {
                    this._buffer.Append(value);
                    this._buffer.Append(this.NewLine);
                    this.UpdateBufferSnapshot();
                }
            }
        }

        public override void WriteLine(uint value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine(ulong value)
        {
            lock (this._locker)
            {
                base.WriteLine(value);
            }
        }

        public void WriteLineDepth(object value, int? maxDepth)
        {
            this.WriteLineDepth(value, maxDepth, null);
        }

        internal void WriteLineDepth(object value, int? maxDepth, Action<ClickContext> onClick)
        {
            if (value is string)
            {
                this.WriteLine((string) value);
            }
            else
            {
                this.WriteDepth(value, maxDepth, onClick);
            }
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return System.Text.Encoding.Unicode;
            }
        }

        public List<object> Explorables
        {
            get
            {
                return this._formatter.Explorables;
            }
        }

        public TimeSpan FormattingTime
        {
            get
            {
                return this._formattingTime;
            }
        }
    }
}

