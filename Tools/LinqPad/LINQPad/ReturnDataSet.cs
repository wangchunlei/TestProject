namespace LINQPad
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class ReturnDataSet : DataSet
    {
        [return: Dynamic(new bool[] { false, true })]
        public IEnumerable<object> AsDynamic()
        {
            return this.AsDynamic(0);
        }

        [return: Dynamic(new bool[] { false, true })]
        public IEnumerable<object> AsDynamic(int resultSetIndex)
        {
            return new <AsDynamic>d__0(-2) { <>4__this = this, <>3__resultSetIndex = resultSetIndex };
        }

        public Dictionary<string, object> OutputParameters { get; set; }

        public int ReturnValue { get; set; }

        [CompilerGenerated]
        private sealed class <AsDynamic>d__0 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private object <>2__current;
            public int <>3__resultSetIndex;
            public ReturnDataSet <>4__this;
            public IEnumerator <>7__wrap2;
            public IDisposable <>7__wrap3;
            private int <>l__initialThreadId;
            public DataRow <row>5__1;
            public int resultSetIndex;

            [DebuggerHidden]
            public <AsDynamic>d__0(int <>1__state)
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
                        this.<>7__wrap2 = this.<>4__this.Tables[this.resultSetIndex].Rows.GetEnumerator();
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
                        }
                        if (this.<>7__wrap2.MoveNext())
                        {
                            this.<row>5__1 = (DataRow) this.<>7__wrap2.Current;
                            this.<>2__current = new ReturnDataSet.DynamicDataRow(this.<row>5__1);
                            this.<>1__state = 1;
                            flag = false;
                            return true;
                        }
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.<>7__wrap3 = this.<>7__wrap2 as IDisposable;
                            if (this.<>7__wrap3 != null)
                            {
                                this.<>7__wrap3.Dispose();
                            }
                        }
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

            [return: Dynamic(new bool[] { false, true })]
            [DebuggerHidden]
            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                ReturnDataSet.<AsDynamic>d__0 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new ReturnDataSet.<AsDynamic>d__0(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.resultSetIndex = this.<>3__resultSetIndex;
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
                [return: Dynamic]
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

        private class DynamicDataRow : DynamicObject
        {
            private DataRow _row;

            public DynamicDataRow(DataRow row)
            {
                this._row = row;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return (from dc in this._row.Table.Columns.Cast<DataColumn>() select dc.ColumnName);
            }

            public override bool TryConvert(ConvertBinder binder, out object result)
            {
                if (binder.Type == typeof(DataRow))
                {
                    result = this._row;
                    return true;
                }
                return base.TryConvert(binder, out result);
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                if (indexes.Length == 1)
                {
                    result = this._row[int.Parse(indexes[0].ToString())];
                    return true;
                }
                return base.TryGetIndex(binder, indexes, out result);
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = this._row[binder.Name];
                if (result is DBNull)
                {
                    result = null;
                }
                return true;
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                if (!(binder.Name == "Dump"))
                {
                    return base.TryInvokeMember(binder, args, out result);
                }
                if (args.Length == 0)
                {
                    this._row.Dump<DataRow>();
                }
                else if ((args.Length == 1) && (args[0] is int))
                {
                    this._row.Dump<DataRow>((int) args[0]);
                }
                else if ((args.Length == 1) && (args[0] is string))
                {
                    this._row.Dump<DataRow>((string) args[0]);
                }
                else if (args.Length == 2)
                {
                    this._row.Dump<DataRow>(args[0] as string, args[1] as int?);
                }
                else
                {
                    this._row.Dump<DataRow>();
                }
                result = this._row;
                return true;
            }

            public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                if (indexes.Length == 1)
                {
                    this._row[int.Parse(indexes[0].ToString())] = value;
                    return true;
                }
                return base.TrySetIndex(binder, indexes, value);
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                this._row[binder.Name] = value;
                return true;
            }
        }
    }
}

