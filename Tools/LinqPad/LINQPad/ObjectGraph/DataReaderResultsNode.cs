namespace LINQPad.ObjectGraph
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class DataReaderResultsNode : ListNode
    {
        public DataReaderResultsNode(ObjectNode parent, IDataReader r, int maxDepth, DataContextDriver dcDriver) : base(parent, GetRows(parent, r, maxDepth, dcDriver), maxDepth, dcDriver, "Result Set")
        {
        }

        private static IEnumerable GetRows(ObjectNode parent, IDataReader reader, int maxDepth, DataContextDriver dcDriver)
        {
            return new <GetRows>d__8(-2) { <>3__parent = parent, <>3__reader = reader, <>3__maxDepth = maxDepth, <>3__dcDriver = dcDriver };
        }

        [CompilerGenerated]
        private sealed class <GetRows>d__8 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private object <>2__current;
            public DataContextDriver <>3__dcDriver;
            public int <>3__maxDepth;
            public ObjectNode <>3__parent;
            public IDataReader <>3__reader;
            private int <>l__initialThreadId;
            public Type[] <types>5__9;
            public DataContextDriver dcDriver;
            public int maxDepth;
            public ObjectNode parent;
            public IDataReader reader;

            [DebuggerHidden]
            public <GetRows>d__8(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    if (this.<>1__state != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<types>5__9 = null;
                        try
                        {
                            if (DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate5 == null)
                            {
                                DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate5 = new Func<DataRow, <>f__AnonymousType38<DataRow, Type>>(DataReaderResultsNode.<GetRows>b__2);
                            }
                            if (DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate6 == null)
                            {
                                DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate6 = new Func<<>f__AnonymousType38<DataRow, Type>, <>f__AnonymousType39<<>f__AnonymousType38<DataRow, Type>, bool>>(DataReaderResultsNode.<GetRows>b__3);
                            }
                            if (DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate7 == null)
                            {
                                DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate7 = new Func<<>f__AnonymousType39<<>f__AnonymousType38<DataRow, Type>, bool>, Type>(DataReaderResultsNode.<GetRows>b__4);
                            }
                            this.<types>5__9 = this.reader.GetSchemaTable().Rows.OfType<DataRow>().Select(DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate5).Select(DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate6).Select(DataReaderResultsNode.CS$<>9__CachedAnonymousMethodDelegate7).ToArray<Type>();
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                    else
                    {
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>1__state = 0;
                    }
                    if (this.reader.Read())
                    {
                        this.<>2__current = new DataRecordMemberNode(this.parent, this.<types>5__9, this.reader, this.maxDepth, this.dcDriver);
                        this.<>1__state = 1;
                        return true;
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                DataReaderResultsNode.<GetRows>d__8 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new DataReaderResultsNode.<GetRows>d__8(0);
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

