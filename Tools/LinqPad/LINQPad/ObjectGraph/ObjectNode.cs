namespace LINQPad.ObjectGraph
{
    using LINQPad;
    using LINQPad.ExecutionModel;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Linq;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    internal abstract class ObjectNode
    {
        private bool _containsProcess;
        public readonly ObjectNode CyclicReference;
        public readonly DataContextDriver DCDriver;
        public static bool ExpandTypes = false;
        internal bool HasTypeReferences;
        internal readonly int MaxDepth = 5;
        public readonly int NestingDepth;
        public readonly object ObjectValue;
        public Action<ClickContext> OnClick;

        protected ObjectNode(ObjectNode parent, object value, int maxDepth, DataContextDriver dcDriver)
        {
            this.Parent = parent;
            this.ObjectValue = value;
            this.MaxDepth = maxDepth;
            this.DCDriver = dcDriver;
            if (Util.IsMetaGraphNode(value))
            {
                this.NestingDepth--;
            }
            while (parent != null)
            {
                if (!Util.IsMetaGraphNode(parent.ObjectValue))
                {
                    this.NestingDepth++;
                }
                if (IsSame(value, parent.ObjectValue))
                {
                    this.CyclicReference = parent;
                }
                if (parent.ObjectValue is Process)
                {
                    this._containsProcess = true;
                }
                parent = parent.Parent;
            }
        }

        public abstract object Accept(IObjectGraphVisitor visitor);
        public static ObjectNode Create(object item, int? maxDepth, DataContextDriver dcDriver)
        {
            int? nullable = maxDepth;
            return Create(null, item, nullable.HasValue ? nullable.GetValueOrDefault() : 5, dcDriver);
        }

        internal static ObjectNode Create(ObjectNode parent, object item, int maxDepth, DataContextDriver dcDriver)
        {
            if (item is ObjectNode)
            {
                ObjectNode node = (ObjectNode) item;
                node.Parent = parent;
                return node;
            }
            if (item == null)
            {
                return new SimpleNode(parent, null);
            }
            if (((!(item is string) && !(item is int)) && !(item is decimal)) && !(item is DateTime))
            {
                Type type = item.GetType();
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    type = type.GetGenericArguments()[0];
                }
                if (Type.GetTypeCode(type) == TypeCode.Object)
                {
                    HandleObjectProxies(ref item, parent, dcDriver);
                }
                if (item == ObjectGraphInfo.GetDisplayNothingToken())
                {
                    return new EmptyNode();
                }
                if (item == null)
                {
                    return new SimpleNode(parent, null);
                }
            }
            Type type2 = item.GetType();
            if ((((item is byte[]) || (item is Binary)) && (parent != null)) && !(parent.ObjectValue is HeadingPresenter))
            {
                string str;
                byte[] buffer = item as byte[];
                if (buffer == null)
                {
                    buffer = ((Binary) item).ToArray();
                }
                if (buffer.Length <= 20)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (byte num2 in buffer)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(" ");
                        }
                        builder.AppendFormat("{0:X2}", num2);
                    }
                    str = builder.ToString();
                }
                else
                {
                    str = "byte[]";
                }
                return new SimpleNode(parent, str, "(binary data)", SimpleNodeKind.Metadata);
            }
            if (((((((item is string) || (item is bool)) || ((item is char) || (item is TimeSpan))) || (((item is IFormattable) || (item is XObject)) || ((item is XName) || (item is XNamespace)))) || ((((item is SqlBoolean) || (item is SqlByte)) || ((item is SqlDateTime) || (item is SqlDecimal))) || (((item is SqlDouble) || (item is SqlGuid)) || ((item is SqlInt16) || (item is SqlInt32))))) || (((item is SqlInt64) || (item is SqlMoney)) || (item is SqlSingle))) || (item is SqlString))
            {
                return new SimpleNode(parent, item.ToString());
            }
            if (item is XmlNode)
            {
                return new SimpleNode(parent, XmlHelper.ToFormattedString((XmlNode) item));
            }
            Type type3 = type2.IsGenericType ? type2.GetGenericTypeDefinition() : null;
            if (!(((!(type3 == typeof(EntitySet<>)) || (parent == null)) || (parent.IsAnonType || (parent is ListNode))) || (parent.ObjectValue is HeadingPresenter)))
            {
                return new SimpleNode(parent, "EntitySet", type2.FormatTypeName(), SimpleNodeKind.Metadata);
            }
            for (ObjectNode node3 = parent; node3 == null; node3 = node3.Parent)
            {
            Label_0305:
                if (0 == 0)
                {
                    if ((type3 == typeof(Table<>)) && (node3 != null))
                    {
                        return new SimpleNode(parent, "Table", type2.FormatTypeName(), SimpleNodeKind.Metadata);
                    }
                    if (item is DBNull)
                    {
                        return new SimpleNode(parent, "null", "DbNull", SimpleNodeKind.Metadata);
                    }
                    string text = null;
                    ObjectNode payload = null;
                    ReturnDataSet set = item as ReturnDataSet;
                    if (set != null)
                    {
                        text = set.ReturnValue.ToString();
                        if (set.OutputParameters != null)
                        {
                            foreach (KeyValuePair<string, object> pair in set.OutputParameters)
                            {
                                if (pair.Value != null)
                                {
                                    string str3 = text;
                                    text = str3 + ", " + pair.Key + "=" + pair.Value.ToString();
                                }
                            }
                        }
                        payload = new SimpleNode(null, text);
                    }
                    if ((item is DataSet) && (((DataSet) item).Tables.Count == 1))
                    {
                        item = ((DataSet) item).Tables[0];
                    }
                    if (item is DataSet)
                    {
                        return new ListPayloadNode(parent, ((DataSet) item).Tables, maxDepth, dcDriver, "Result Sets", payload, "ReturnValue");
                    }
                    if (item is DataTable)
                    {
                        return new ListPayloadNode(parent, ((DataTable) item).Rows, maxDepth, dcDriver, "Result Set", payload, "ReturnValue");
                    }
                    if (item is DataRow)
                    {
                        return new DataRowNode(parent, (DataRow) item, maxDepth, dcDriver);
                    }
                    if (item is IDataReader)
                    {
                        DataReaderNode node5 = new DataReaderNode(parent, (IDataReader) item, maxDepth, dcDriver);
                        if (node5.Items.Count == 1)
                        {
                            return node5.Items[0];
                        }
                        return node5;
                    }
                    if (item is IDataRecord)
                    {
                        return new DataRecordMemberNode(parent, null, (IDataRecord) item, maxDepth, dcDriver);
                    }
                    if (type2.GetInterfaces().Any<Type>(t => t.FullName == typeof(ICustomMemberProvider).FullName))
                    {
                        try
                        {
                            return new CustomMemberProviderNode(parent, item, maxDepth, dcDriver, false);
                        }
                        catch (Exception exception)
                        {
                            Log.Write(exception, "CustomMemberProvider");
                        }
                    }
                    if ((dcDriver != null) && (dcDriver.GetCustomDisplayMemberProvider(item) != null))
                    {
                        return new CustomMemberProviderNode(parent, item, maxDepth, dcDriver, true);
                    }
                    if (item is Type)
                    {
                        for (ObjectNode node6 = parent; node6 != null; node6 = node6.Parent)
                        {
                            node6.HasTypeReferences = true;
                        }
                    }
                    if (!(ExpandTypes || !(item is Type)))
                    {
                        return new SimpleNode(parent, "typeof (" + ((Type) item).FormatTypeName() + ")", ((Type) item).FormatTypeName(true), SimpleNodeKind.Metadata) { HasTypeReferences = true };
                    }
                    if (item is Image)
                    {
                        Image image = (Image) item;
                        MemoryStream stream = new MemoryStream();
                        image.Save(stream, ImageFormat.Png);
                        item = Util.Image(stream.ToArray());
                    }
                    if (((!type2.IsArray && (item is IList)) && (((IList) item).Count == 1)) && (((IList) item)[0] == item))
                    {
                        return new ClrMemberNode(parent, item, maxDepth, dcDriver);
                    }
                    if (type2.IsArray && (type2.GetArrayRank() == 2))
                    {
                        Type elementType = type2.GetElementType();
                        if (elementType.IsGenericType && (elementType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            elementType = elementType.GetGenericArguments()[0];
                        }
                        if (((typeof(IFormattable).IsAssignableFrom(elementType) || (elementType == typeof(bool))) || ((elementType == typeof(char)) || (elementType == typeof(TimeSpan)))) || (elementType == typeof(string)))
                        {
                            return new MultiDimArrayNode(parent, (Array) item);
                        }
                    }
                    if (item is IEnumerable)
                    {
                        return new ListNode(parent, (IEnumerable) item, maxDepth, dcDriver);
                    }
                    if (item is Exception)
                    {
                        return new ExceptionNode(parent, (Exception) item, maxDepth);
                    }
                    if (item is DynamicObject)
                    {
                        return new DynamicObjectMemberNode(parent, (DynamicObject) item, maxDepth, dcDriver);
                    }
                    return new ClrMemberNode(parent, item, maxDepth, dcDriver);
                }
            }
            goto Label_0305;
        }

        internal static ObjectNode Create(ObjectNode parent, object item, bool foldExceptions, int maxDepth, DataContextDriver dcDriver)
        {
            if (!foldExceptions)
            {
                return Create(parent, item, maxDepth, dcDriver);
            }
            try
            {
                return Create(parent, item, maxDepth, dcDriver);
            }
            catch (Exception exception)
            {
                return new SimpleNode(parent, "(" + exception.GetType().Name + ": " + exception.Message + ")", "An exception was thrown when querying this value", SimpleNodeKind.Warning);
            }
        }

        private static IEnumerable<object> GetParentHierarchy(ObjectNode parent)
        {
            return new <GetParentHierarchy>d__2(-2) { <>3__parent = parent };
        }

        private static bool HandleAsyncEnumerable(ref object item, ObjectGraphInfo info)
        {
            Server currentServer = Server.CurrentServer;
            if (currentServer == null)
            {
                return false;
            }
            Type type = item.GetType().GetInterface("System.Collections.Generic.IAsyncEnumerable`1");
            if (type == null)
            {
                return false;
            }
            MethodInfo method = type.GetMethod("GetEnumerator", new Type[0]);
            if (method == null)
            {
                return false;
            }
            object rator = method.Invoke(item, new object[0]);
            if (rator == null)
            {
                return false;
            }
            List<IDisposable> disposables = currentServer.Disposables;
            if (rator is IDisposable)
            {
                disposables.Add((IDisposable) rator);
            }
            MethodInfo moveNextMethod = rator.GetType().GetMethod("MoveNext", new Type[0]);
            if (moveNextMethod == null)
            {
                moveNextMethod = rator.GetType().GetMethod("MoveNext", new Type[] { typeof(CancellationToken) });
            }
            if (moveNextMethod == null)
            {
                return false;
            }
            if (moveNextMethod.ReturnType != typeof(Task<bool>))
            {
                return false;
            }
            PropertyInfo property = rator.GetType().GetProperty("Current");
            if (property == null)
            {
                return false;
            }
            Server.CurrentServer.QueryCompletionCountdown.Increment();
            HandleAsyncEnumerator(rator, moveNextMethod, property, info.Heading);
            item = info.DisplayNothingToken;
            return true;
        }

        private static void HandleAsyncEnumerator(object rator, MethodInfo moveNextMethod, PropertyInfo currentProp, string heading)
        {
            object[] objArray2;
            if (moveNextMethod.GetParameters().Length == 1)
            {
                objArray2 = new object[] { CancellationToken.None };
            }
            else
            {
                objArray2 = new object[0];
            }
            Task<bool> task = (Task<bool>) moveNextMethod.Invoke(rator, objArray2);
            if (task != null)
            {
                task.ContinueWith(delegate (Task<bool> ant) {
                    if (ant.Exception != null)
                    {
                        if (string.IsNullOrEmpty(heading))
                        {
                            ant.Exception.Dump<AggregateException>();
                        }
                        else
                        {
                            Util.HorizontalRun(true, new object[] { Util.Metatext(heading + " →"), ant.Exception }).Dump<object>();
                        }
                    }
                    if (!((ant.Exception != null) ? false : ant.Result))
                    {
                        Server.CurrentServer.QueryCompletionCountdown.Decrement();
                    }
                    else
                    {
                        object o = currentProp.GetValue(rator, new object[0]);
                        if (string.IsNullOrEmpty(heading))
                        {
                            o.Dump<object>();
                        }
                        else
                        {
                            Util.HorizontalRun(true, new object[] { Util.Metatext(heading + " →"), o }).Dump<object>();
                        }
                        HandleAsyncEnumerator(rator, moveNextMethod, currentProp, heading);
                    }
                });
            }
        }

        private static void HandleObjectProxies(ref object item, ObjectNode parent, DataContextDriver dcDriver)
        {
            HeadingPresenter objectValue = null;
            if (parent != null)
            {
                objectValue = parent.ObjectValue as HeadingPresenter;
            }
            string heading = (objectValue == null) ? null : (objectValue.Heading as string);
            ObjectGraphInfo info = new ObjectGraphInfo(heading, GetParentHierarchy(parent));
            if (dcDriver != null)
            {
                dcDriver.PreprocessObjectToWrite(ref item, info);
            }
            HandleObservable(ref item, info);
            HandleAsyncEnumerable(ref item, info);
            if ((item == info.DisplayNothingToken) && (objectValue != null))
            {
                objectValue.HidePresenter = true;
            }
        }

        private static bool HandleObservable(ref object item, ObjectGraphInfo info)
        {
            Server currentServer = Server.CurrentServer;
            if (currentServer == null)
            {
                return false;
            }
            Type type = item.GetType().GetInterface("System.IObservable`1");
            if (type == null)
            {
                return false;
            }
            MethodInfo method = typeof(ObservableHelper).GetMethod("Subscribe", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                return false;
            }
            string heading = info.Heading;
            Type type2 = type.GetGenericArguments()[0];
            MethodInfo info3 = typeof(NextActionClosure).GetMethod("NextAction").MakeGenericMethod(new Type[] { type2 });
            Delegate delegate2 = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(new Type[] { type2 }), new NextActionClosure(heading), info3);
            Countdown countdown = currentServer.QueryCompletionCountdown;
            List<IDisposable> disposables = currentServer.Disposables;
            Action<Exception> action = delegate (Exception ex) {
                if (!string.IsNullOrEmpty(heading))
                {
                    Util.HorizontalRun(true, new object[] { Util.Metatext(heading + " →"), ex }).Dump<object>();
                }
                else
                {
                    ex.Dump<Exception>();
                }
                try
                {
                    countdown.Decrement();
                }
                catch
                {
                }
            };
            Action action2 = new Action(countdown.Decrement);
            object obj2 = method.MakeGenericMethod(new Type[] { type2 }).Invoke(null, new object[] { item, delegate2, action, action2 });
            countdown.Increment();
            if (obj2 is IDisposable)
            {
                lock (disposables)
                {
                    disposables.Add((IDisposable) obj2);
                }
            }
            item = info.DisplayNothingToken;
            return true;
        }

        protected bool IsAtNestingLimit()
        {
            int num = this.MaxDepth + ((this.ObjectValue is Exception) ? 1 : 0);
            if ((this.Parent != null) && (this.Parent.ObjectValue is Exception))
            {
                num++;
            }
            if (this._containsProcess)
            {
                num -= 2;
            }
            return ((this.NestingDepth >= num) || (((this.Parent != null) && (this.Parent.Parent != null)) && (this.Parent.Parent.ObjectValue is Type)));
        }

        internal static bool IsKey(string name, Type type)
        {
            if (((type == typeof(decimal)) || (type == typeof(double))) || (type == typeof(float)))
            {
                return false;
            }
            if (name.ToLowerInvariant() == "id")
            {
                return true;
            }
            if (name.Length < 3)
            {
                return false;
            }
            return (name.EndsWith("Id", StringComparison.Ordinal) || (name.EndsWith("_ID", StringComparison.OrdinalIgnoreCase) || ((name.EndsWith("ID", StringComparison.Ordinal) && char.IsLower(name[name.Length - 3])) || (name.EndsWith("Key", StringComparison.Ordinal) || name.EndsWith("_KEY", StringComparison.OrdinalIgnoreCase)))));
        }

        private static bool IsSame(object o1, object o2)
        {
            return (object.ReferenceEquals(o1, o2) || (((o1 is FileSystemInfo) && (o2 is FileSystemInfo)) && (((FileSystemInfo) o1).FullName == ((FileSystemInfo) o2).FullName)));
        }

        public bool GraphTruncated { get; protected set; }

        public bool InitiallyHidden { get; protected set; }

        public bool IsAnonType
        {
            get
            {
                return ((this.ObjectValue != null) && this.ObjectValue.GetType().IsAnonymous());
            }
        }

        public ObjectNode Parent { get; private set; }

        [CompilerGenerated]
        private sealed class <GetParentHierarchy>d__2 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private object <>2__current;
            public ObjectNode <>3__parent;
            private int <>l__initialThreadId;
            public ObjectNode parent;

            [DebuggerHidden]
            public <GetParentHierarchy>d__2(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    if (this.<>1__state == 1)
                    {
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>1__state = 0;
                        goto Label_0067;
                    }
                    if (this.<>1__state == -1)
                    {
                        return false;
                    }
                    if (this.$__disposing)
                    {
                        return false;
                    }
                Label_0047:
                    if (this.parent == null)
                    {
                        goto Label_00A2;
                    }
                    if (!Util.IsMetaGraphNode(this.parent.ObjectValue))
                    {
                        goto Label_007C;
                    }
                Label_0067:
                    this.parent = this.parent.Parent;
                    goto Label_0047;
                Label_007C:
                    this.<>2__current = this.parent.ObjectValue;
                    this.<>1__state = 1;
                    return true;
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
            Label_00A2:
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                ObjectNode.<GetParentHierarchy>d__2 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new ObjectNode.<GetParentHierarchy>d__2(0);
                }
                d__.parent = this.<>3__parent;
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

        private class NextActionClosure
        {
            public readonly string Heading;

            public NextActionClosure(string heading)
            {
                this.Heading = heading;
            }

            public void NextAction<T>(T data)
            {
                if (string.IsNullOrEmpty(this.Heading))
                {
                    data.Dump<T>();
                }
                else
                {
                    Util.HorizontalRun(true, new object[] { Util.Metatext(this.Heading + " →"), data }).Dump<object>();
                }
            }
        }
    }
}

