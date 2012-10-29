namespace LINQPad.ObjectGraph
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class DataReaderNode : ListNode
    {
        public DataReaderNode(ObjectNode parent, IDataReader r, int maxDepth, DataContextDriver dcDriver) : base(parent, GetResults(parent, r, maxDepth, dcDriver), maxDepth, dcDriver, "Result Sets")
        {
        }

        private static IEnumerable GetResults(ObjectNode parent, IDataReader reader, int maxDepth, DataContextDriver dcDriver)
        {
            return new <GetResults>d__8(-2) { <>3__parent = parent, <>3__reader = reader, <>3__maxDepth = maxDepth, <>3__dcDriver = dcDriver };
        }

        [CompilerGenerated]
        private sealed class <GetResults>d__8 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private object <>2__current;
            public DataContextDriver <>3__dcDriver;
            public int <>3__maxDepth;
            public ObjectNode <>3__parent;
            public IDataReader <>3__reader;
            public IDataReader <>7__wrap9;
            private int <>l__initialThreadId;
            public DataContextDriver dcDriver;
            public int maxDepth;
            public ObjectNode parent;
            public IDataReader reader;

            [DebuggerHidden]
            public <GetResults>d__8(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    bool flag = true;
                    int num = this.<>1__state;
                    if (num != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>7__wrap9 = this.reader;
                    }
                    try
                    {
                        num = this.<>1__state;
                        if (num == 1)
                        {
                            if (this.$__disposing)
                            {
                                return false;
                            }
                            this.<>1__state = 0;
                            if (!this.reader.NextResult())
                            {
                                goto Label_00CE;
                            }
                        }
                        this.<>2__current = new DataReaderResultsNode(this.parent, this.reader, this.maxDepth, this.dcDriver);
                        this.<>1__state = 1;
                        flag = false;
                        return true;
                    }
                    finally
                    {
                        if (flag && (this.<>7__wrap9 != null))
                        {
                            this.<>7__wrap9.Dispose();
                        }
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
            Label_00CE:
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                DataReaderNode.<GetResults>d__8 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new DataReaderNode.<GetResults>d__8(0);
                }
                d__.parent = this.<>3__parent;
                d__.reader = this.<>3__reader;
                d__.maxDepth = this.<>3__maxDepth;
                d__.dcDriver = this.<>3__dcDriver;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Object>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

