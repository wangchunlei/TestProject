namespace LINQPad
{
    using System;
    using System.IO;

    internal class StringWriterEx : StringWriter
    {
        private readonly int _maxLength;

        public StringWriterEx()
        {
        }

        public StringWriterEx(int maxLength)
        {
            this._maxLength = maxLength;
        }

        public override void Write(char value)
        {
            if (this.GetStringBuilder().Length <= this._maxLength)
            {
                base.Write(value);
                this.WriteEllipsesIfOverLength();
            }
        }

        public override void Write(string value)
        {
            if (this.GetStringBuilder().Length <= this._maxLength)
            {
                base.Write(value);
                this.WriteEllipsesIfOverLength();
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (this.GetStringBuilder().Length <= this._maxLength)
            {
                base.Write(buffer, index, count);
                this.WriteEllipsesIfOverLength();
            }
        }

        private void WriteEllipsesIfOverLength()
        {
            if (this.GetStringBuilder().Length > this._maxLength)
            {
                base.Write("\r\n...");
            }
        }
    }
}

