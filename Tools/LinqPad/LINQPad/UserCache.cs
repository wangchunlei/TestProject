namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;

    internal class UserCache
    {
        private static int _cacheVersion;
        private static Hashtable _objectCache = new Hashtable();
        private static Dictionary<string, StackTrace> _sessionAutoTraces = new Dictionary<string, StackTrace>();
        private static Dictionary<string, string> _sessionAutoTraceStrings = new Dictionary<string, string>();
        private static Hashtable _sessionMap = new Hashtable(new ReferenceComparer());

        private static T Cache<T>(Func<T> dataFetcher, object key)
        {
            Hashtable hashtable;
            object obj2;
            lock ((hashtable = _objectCache))
            {
                obj2 = _objectCache[key];
            }
            if (obj2 != null)
            {
                try
                {
                    CacheConverter converter = new CacheConverter();
                    if (converter.CanConvert(obj2.GetType(), typeof(T)))
                    {
                        return (T) converter.Convert(obj2, typeof(T), false);
                    }
                }
                catch (TypeChangeException)
                {
                }
            }
            T local2 = dataFetcher();
            lock ((hashtable = _objectCache))
            {
                _objectCache[key] = local2;
            }
            Interlocked.Increment(ref _cacheVersion);
            return local2;
        }

        public static T[] CacheSequence<T>(IEnumerable<T> o, object key)
        {
            if (o == null)
            {
                return null;
            }
            if (key == null)
            {
                key = "<Sequence>" + InferKeyFromType(o.GetType());
                ValidateAutomaticKey((string) key);
            }
            return Cache<T[]>(() => o.ToArray<T>(), key);
        }

        public static T CacheValue<T>(Func<T> dataFetcher, object key)
        {
            if (dataFetcher == null)
            {
                return default(T);
            }
            if (key == null)
            {
                key = InferKeyFromType(typeof(T));
                ValidateAutomaticKey((string) key);
            }
            return Cache<T>(dataFetcher, key);
        }

        public static void ClearSession()
        {
            _sessionMap.Clear();
            _sessionAutoTraceStrings.Clear();
            _sessionAutoTraces.Clear();
        }

        private static string InferKeyFromType(Type t)
        {
            return InferKeyFromType(t, new HashSet<Type>());
        }

        private static string InferKeyFromType(Type t, HashSet<Type> visitedTypes)
        {
            if (t == null)
            {
                return "";
            }
            if (t.IsByRef)
            {
                t = t.GetElementType();
            }
            if (!visitedTypes.Contains(t))
            {
                visitedTypes.Add(t);
                if (t.IsGenericType)
                {
                    return ((t.IsAnonymous() ? "<>Anon" : t.GetGenericTypeDefinition().FullName) + "{" + string.Join(",", (from p in t.GetGenericArguments() select InferKeyFromType(p, visitedTypes)).ToArray<string>()) + "}");
                }
                if (t.IsArray)
                {
                    return (InferKeyFromType(t.GetElementType(), visitedTypes) + "[".PadRight(t.GetArrayRank() - 1, ',') + "]");
                }
            }
            return t.FullName;
        }

        private static void ValidateAutomaticKey(string key)
        {
            StackTrace trace = new StackTrace(true);
            string str = null;
            StackTrace trace2 = null;
            lock (_sessionAutoTraceStrings)
            {
                if (!_sessionAutoTraceStrings.TryGetValue(key, out str))
                {
                    _sessionAutoTraces.Add(key, trace);
                    _sessionAutoTraceStrings.Add(key, trace.ToString());
                    return;
                }
                if (_sessionAutoTraces.TryGetValue(key, out trace2))
                {
                    _sessionAutoTraces.Remove(key);
                }
            }
            if (str != trace.ToString())
            {
                Server currentServer = Server.CurrentServer;
                if (currentServer != null)
                {
                    string message = "The same type was cached elsewhere in the query. A cache slot will be shared. Specify a key when calling Cache() for a separate slot.";
                    if (trace2 != null)
                    {
                        currentServer.ReportSpecialMessage(trace2, null, message, false, true);
                    }
                    currentServer.ReportSpecialMessage(trace, null, message, false, true);
                }
            }
        }

        public static int CacheVersion
        {
            get
            {
                return _cacheVersion;
            }
        }

        private class CacheConverter
        {
            private static MethodInfo _makeLazyEnumerable = typeof(UserCache.CacheConverter).GetMethod("MakeLazyEnumerable");
            private Dictionary<TypeType, Func<object, object>> _shredders;
            private Func<Type, Type> GetFirstGenericArg;
            private Func<Type, Type> GetGenericIEnumerableFromType;
            private Func<Type, Type> GetGenericTypeDefinition;
            private Func<Type, Type> MakeGenericTypeForList;
            private Func<Type, Func<object, Type, IEnumerable>> MakeLazyEnumerableConverter;

            public CacheConverter()
            {
                Func<Type, Type> valueGenerator = null;
                Func<Type, Func<object, Type, IEnumerable>> func2 = null;
                this.GetGenericTypeDefinition = MakeTypeGymnastCache<Type>(t => t.GetGenericTypeDefinition());
                this.MakeGenericTypeForList = MakeTypeGymnastCache<Type>(t => typeof(List<>).MakeGenericType(new Type[] { t }));
                this.GetFirstGenericArg = MakeTypeGymnastCache<Type>(t => t.GetGenericArguments()[0]);
                this._shredders = new Dictionary<TypeType, Func<object, object>>();
                if (valueGenerator == null)
                {
                    valueGenerator = t => (this.GetGenericTypeDefinition(t) == typeof(IEnumerable<>)) ? t : t.GetInterface("System.Collections.Generic.IEnumerable`1");
                }
                this.GetGenericIEnumerableFromType = MakeTypeGymnastCache<Type>(valueGenerator);
                if (func2 == null)
                {
                    func2 = delegate (Type t) {
                        Func<object, Type, IEnumerable> func = null;
                        MethodInfo genericMethod = _makeLazyEnumerable.MakeGenericMethod(new Type[] { t });
                        if (Environment.Version.Major == 2)
                        {
                            if (func == null)
                            {
                                func = (source, sourceType) => (IEnumerable) genericMethod.Invoke(this, new object[] { source, sourceType });
                            }
                            return func;
                        }
                        Type type = typeof(IEnumerable<>).MakeGenericType(new Type[] { t });
                        return (Func<object, Type, IEnumerable>) Delegate.CreateDelegate(typeof(Func<,,>).MakeGenericType(new Type[] { typeof(object), typeof(Type), type }), this, genericMethod);
                    };
                }
                this.MakeLazyEnumerableConverter = MakeTypeGymnastCache<Func<object, Type, IEnumerable>>(func2);
            }

            public bool CanConvert(Type sourceType, Type targetType)
            {
                return this.CanConvert(sourceType, targetType, new HashSet<TypeType>());
            }

            private bool CanConvert(Type sourceType, Type targetType, HashSet<TypeType> visitedTypes)
            {
                if ((targetType != sourceType) && !targetType.IsAssignableFrom(sourceType))
                {
                    TypeType item = new TypeType {
                        SourceType = sourceType,
                        TargetType = targetType
                    };
                    if (visitedTypes.Contains(item))
                    {
                        return true;
                    }
                    visitedTypes.Add(item);
                    if (!targetType.IsAnonymous())
                    {
                        if (targetType.IsArray)
                        {
                            if (!sourceType.IsArray)
                            {
                                return false;
                            }
                            return this.CanConvert(sourceType.GetElementType(), targetType.GetElementType(), visitedTypes);
                        }
                        Type type3 = null;
                        if (targetType.IsGenericType)
                        {
                            type3 = this.GetGenericTypeDefinition(targetType);
                        }
                        if (((type3 == typeof(IEnumerable<>)) || (type3 == typeof(IList<>))) || (type3 == typeof(List<>)))
                        {
                            Type arg = this.GetGenericIEnumerableFromType(sourceType);
                            if (arg == null)
                            {
                                return false;
                            }
                            Type type5 = this.GetFirstGenericArg(arg);
                            Type type6 = this.GetFirstGenericArg(targetType);
                            return this.CanConvert(type5, type6, visitedTypes);
                        }
                        if (!targetType.IsSerializable || !targetType.Assembly.FullName.StartsWith("query_"))
                        {
                            throw this.GetUncacheableException(targetType);
                        }
                        if (!(sourceType.IsSerializable && sourceType.Assembly.FullName.StartsWith("query_")))
                        {
                            return false;
                        }
                        if (UserCache.InferKeyFromType(sourceType) != UserCache.InferKeyFromType(targetType))
                        {
                            return false;
                        }
                        IEnumerable<string> enumerable = from m in sourceType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) select m.ToString();
                        IEnumerable<string> enumerable2 = from m in targetType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) select m.ToString();
                        return (from m in enumerable
                            orderby m
                            select m).SequenceEqual<string>((from m in enumerable2
                            orderby m
                            select m));
                    }
                    if (!sourceType.IsAnonymous())
                    {
                        return false;
                    }
                    PropertyInfo[] infoArray = (from p in sourceType.GetProperties()
                        orderby p.Name
                        select p).ToArray<PropertyInfo>();
                    PropertyInfo[] infoArray2 = (from p in targetType.GetProperties()
                        orderby p.Name
                        select p).ToArray<PropertyInfo>();
                    if (!(from p in infoArray select p.Name).SequenceEqual<string>((from p in infoArray2 select p.Name)))
                    {
                        return false;
                    }
                    for (int i = 0; i < infoArray.Length; i++)
                    {
                        if (!this.CanConvert(infoArray[i].PropertyType, infoArray2[i].PropertyType, visitedTypes))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            public object Convert(object source, Type targetType, bool knownToBeAnonymous)
            {
                Hashtable hashtable;
                object obj3;
                if (source == null)
                {
                    return null;
                }
                Type c = source.GetType();
                if ((targetType == c) || targetType.IsAssignableFrom(c))
                {
                    return source;
                }
                lock ((hashtable = UserCache._sessionMap))
                {
                    obj3 = UserCache._sessionMap[source];
                }
                if (obj3 != null)
                {
                    return obj3;
                }
                object obj4 = null;
                bool flag2 = knownToBeAnonymous || targetType.IsAnonymous();
                Type type2 = null;
                if (!(flag2 || !targetType.IsGenericType))
                {
                    type2 = this.GetGenericTypeDefinition(targetType);
                }
                if (flag2)
                {
                    obj4 = this.ConvertAnonymousType(source, c, targetType);
                }
                else if (targetType.IsArray)
                {
                    obj4 = this.ConvertArray(source, c, targetType);
                }
                else if ((type2 == typeof(IList<>)) || (type2 == typeof(List<>)))
                {
                    obj4 = this.ConvertList(source, c, targetType);
                }
                else if (type2 == typeof(IEnumerable<>))
                {
                    obj4 = this.ConvertEnumerable(source, c, targetType);
                }
                else if (targetType.IsSerializable && targetType.Assembly.FullName.StartsWith("query_"))
                {
                    obj4 = this.ConvertViaSerialization(source, c, targetType);
                }
                if (obj4 == null)
                {
                    throw this.GetUncacheableException(targetType);
                }
                lock ((hashtable = UserCache._sessionMap))
                {
                    UserCache._sessionMap[source] = obj4;
                }
                return obj4;
            }

            private object ConvertAnonymousType(object source, Type sourceType, Type targetType)
            {
                Func<object, object> func;
                TypeType key = new TypeType {
                    SourceType = sourceType,
                    TargetType = targetType
                };
                if (!this._shredders.TryGetValue(key, out func))
                {
                    this._shredders[key] = func = this.CreateAnonymousShredder(sourceType, targetType);
                }
                return func(source);
            }

            private Array ConvertArray(object source, Type sourceType, Type targetType)
            {
                if (!(source is Array))
                {
                    throw new UserCache.TypeChangeException();
                }
                sourceType.GetElementType();
                Type elementType = targetType.GetElementType();
                bool knownToBeAnonymous = elementType.IsAnonymous();
                Array array = (Array) source;
                Array array2 = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    array2.SetValue(this.Convert(array.GetValue(i), elementType, knownToBeAnonymous), i);
                }
                return array2;
            }

            private IEnumerable ConvertEnumerable(object source, Type sourceType, Type targetType)
            {
                if (this.GetGenericIEnumerableFromType(sourceType) == null)
                {
                    throw new UserCache.TypeChangeException();
                }
                Type arg = this.GetFirstGenericArg(targetType);
                return this.MakeLazyEnumerableConverter(arg)(source, sourceType);
            }

            private IList ConvertList(object source, Type sourceType, Type targetType)
            {
                Type arg = this.GetGenericIEnumerableFromType(sourceType);
                if (arg == null)
                {
                    throw new UserCache.TypeChangeException();
                }
                this.GetFirstGenericArg(arg);
                Type type2 = this.GetFirstGenericArg(targetType);
                IList list = source as IList;
                int count = 0x80;
                if (list != null)
                {
                    try
                    {
                        count = ((IList) source).Count;
                    }
                    catch
                    {
                        list = null;
                    }
                }
                IList list2 = (IList) Activator.CreateInstance(this.MakeGenericTypeForList(type2), new object[] { count });
                bool knownToBeAnonymous = type2.IsAnonymous();
                if (list == null)
                {
                    foreach (object obj2 in (IEnumerable) source)
                    {
                        list2.Add(this.Convert(obj2, type2, knownToBeAnonymous));
                    }
                    return list2;
                }
                for (int i = 0; i < count; i++)
                {
                    list2.Add(this.Convert(list[i], type2, knownToBeAnonymous));
                }
                return list2;
            }

            private object ConvertViaSerialization(object source, Type sourceType, Type targetType)
            {
                object obj2;
                if ((!sourceType.IsSerializable || !sourceType.Assembly.FullName.StartsWith("query_")) || (UserCache.InferKeyFromType(sourceType) != UserCache.InferKeyFromType(targetType)))
                {
                    throw new UserCache.TypeChangeException();
                }
                byte[] bytes = Encoding.ASCII.GetBytes(sourceType.Assembly.FullName);
                byte[] buffer2 = Encoding.ASCII.GetBytes(targetType.Assembly.FullName);
                if (bytes.Length != buffer2.Length)
                {
                    throw new UserCache.TypeChangeException();
                }
                IFormatter formatter = new BinaryFormatter();
                MemoryStream serializationStream = new MemoryStream();
                formatter.Serialize(serializationStream, source);
                byte[] buffer = serializationStream.ToArray();
                for (int i = 0; i < (buffer.Length - bytes.Length); i++)
                {
                    bool flag = false;
                    int index = 0;
                    while (index < bytes.Length)
                    {
                        if (buffer[i + index] != bytes[index])
                        {
                            goto Label_00C4;
                        }
                        index++;
                    }
                    goto Label_00C7;
                Label_00C4:
                    flag = true;
                Label_00C7:
                    if (!flag)
                    {
                        for (index = 0; index < bytes.Length; index++)
                        {
                            buffer[i + index] = buffer2[index];
                        }
                    }
                }
                serializationStream = new MemoryStream(buffer);
                try
                {
                    obj2 = formatter.Deserialize(serializationStream);
                }
                catch (ArgumentException)
                {
                    throw new UserCache.TypeChangeException();
                }
                return obj2;
            }

            private Func<object, object> CreateAnonymousShredder(Type sourceType, Type targetType)
            {
                ConstructorInfo info;
                if (sourceType.IsAnonymous())
                {
                    if (CS$<>9__CachedAnonymousMethodDelegate35 == null)
                    {
                        CS$<>9__CachedAnonymousMethodDelegate35 = p => p.Name;
                    }
                    if (CS$<>9__CachedAnonymousMethodDelegate36 == null)
                    {
                        CS$<>9__CachedAnonymousMethodDelegate36 = p => p;
                    }
                    if (CS$<>9__CachedAnonymousMethodDelegate37 == null)
                    {
                        CS$<>9__CachedAnonymousMethodDelegate37 = p => p.Name;
                    }
                }
                if (!((CS$<>9__CachedAnonymousMethodDelegate38 == null) && sourceType.GetProperties().Select<PropertyInfo, string>(CS$<>9__CachedAnonymousMethodDelegate35).OrderBy<string, string>(CS$<>9__CachedAnonymousMethodDelegate36).SequenceEqual<string>(targetType.GetProperties().Select<PropertyInfo, string>(CS$<>9__CachedAnonymousMethodDelegate37).OrderBy<string, string>(CS$<>9__CachedAnonymousMethodDelegate38))))
                {
                    throw new UserCache.TypeChangeException();
                }
                try
                {
                    info = targetType.GetConstructors().Single<ConstructorInfo>();
                }
                catch (Exception exception)
                {
                    throw new UserCache.UncacheableObjectException("Cannot cache type " + targetType.Name + " - expecting single constructor", exception);
                }
                var props = (from p in info.GetParameters() select new { SourceProp = sourceType.GetProperty(p.Name), TargetType = p.ParameterType }).ToArray();
                if (props.Any(p => p.SourceProp == null))
                {
                    throw new UserCache.TypeChangeException();
                }
                DynamicMethod method = new DynamicMethod("", typeof(object[]), new Type[] { typeof(object) }, sourceType);
                ILGenerator iLGenerator = method.GetILGenerator();
                iLGenerator.DeclareLocal(typeof(object[]));
                iLGenerator.DeclareLocal(sourceType);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Castclass, sourceType);
                iLGenerator.Emit(OpCodes.Stloc_1);
                iLGenerator.Emit(OpCodes.Ldc_I4, props.Length);
                iLGenerator.Emit(OpCodes.Newarr, typeof(object));
                iLGenerator.Emit(OpCodes.Stloc_0);
                int num = 0;
                foreach (var type in props)
                {
                    iLGenerator.Emit(OpCodes.Ldloc_0);
                    iLGenerator.Emit(OpCodes.Ldc_I4, num++);
                    iLGenerator.Emit(OpCodes.Ldloc_1);
                    iLGenerator.Emit(OpCodes.Callvirt, type.SourceProp.GetGetMethod());
                    if (type.SourceProp.PropertyType.IsValueType)
                    {
                        iLGenerator.Emit(OpCodes.Box, type.SourceProp.PropertyType);
                    }
                    iLGenerator.Emit(OpCodes.Stelem_Ref);
                }
                iLGenerator.Emit(OpCodes.Ldloc_0);
                iLGenerator.Emit(OpCodes.Ret);
                Func<object, object[]> toArrayFunc = (Func<object, object[]>) method.CreateDelegate(typeof(Func<object, object[]>));
                DynamicMethod method2 = new DynamicMethod("", typeof(object), new Type[] { typeof(object[]) }, targetType);
                iLGenerator = method2.GetILGenerator();
                num = 0;
                foreach (var type in props)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldc_I4, num);
                    iLGenerator.Emit(OpCodes.Ldelem_Ref);
                    if (props[num].TargetType.IsValueType)
                    {
                        iLGenerator.Emit(OpCodes.Unbox_Any, props[num].TargetType);
                    }
                    else
                    {
                        iLGenerator.Emit(OpCodes.Castclass, props[num].TargetType);
                    }
                    num++;
                }
                iLGenerator.Emit(OpCodes.Newobj, info);
                iLGenerator.Emit(OpCodes.Ret);
                Func<object[], object> toTargetFunc = (Func<object[], object>) method2.CreateDelegate(typeof(Func<object[], object>));
                return delegate (object source) {
                    object[] arg = toArrayFunc(source);
                    for (int j = 0; j < arg.Length; j++)
                    {
                        arg[j] = this.Convert(arg[j], props[j].TargetType, false);
                    }
                    return toTargetFunc(arg);
                };
            }

            private UserCache.UncacheableObjectException GetUncacheableException(Type targetType)
            {
                string msg = "Cannot load type '" + targetType.FormatTypeName(true) + "' from cache.";
                if ((!targetType.IsSimple() && targetType.IsClass) && targetType.Assembly.FullName.StartsWith("query_"))
                {
                    msg = msg + "\r\nAdd the [Serializable] attribute to this type to make it cacheable.";
                }
                return new UserCache.UncacheableObjectException(msg);
            }

            public IEnumerable<TTarget> MakeLazyEnumerable<TTarget>(object source, Type sourceType)
            {
                return new <MakeLazyEnumerable>d__3c<TTarget>(-2) { <>4__this = this, <>3__source = source, <>3__sourceType = sourceType };
            }

            private static Func<TKey, TValue> MakeObjectCache<TKey, TValue>(Func<TKey, TValue> valueGenerator)
            {
                Dictionary<TKey, TValue> store = new Dictionary<TKey, TValue>();
                return delegate (TKey key) {
                    TValue local;
                    if (store.TryGetValue(key, out local))
                    {
                        return local;
                    }
                    return (store[key] = valueGenerator(key));
                };
            }

            private static Func<Type, TValue> MakeTypeGymnastCache<TValue>(Func<Type, TValue> valueGenerator)
            {
                return MakeObjectCache<Type, TValue>(valueGenerator);
            }

            [CompilerGenerated]
            private sealed class <MakeLazyEnumerable>d__3c<TTarget> : IEnumerable<TTarget>, IEnumerable, IEnumerator<TTarget>, IEnumerator, IDisposable
            {
                private bool $__disposing;
                private int <>1__state;
                private TTarget <>2__current;
                public object <>3__source;
                public Type <>3__sourceType;
                public UserCache.CacheConverter <>4__this;
                public IEnumerator <>7__wrap41;
                public IDisposable <>7__wrap42;
                private int <>l__initialThreadId;
                public bool <isAnon>5__3f;
                public object <sourceElement>5__40;
                public Type <sourceElementType>5__3e;
                public Type <sourceNumerable>5__3d;
                public object source;
                public Type sourceType;

                [DebuggerHidden]
                public <MakeLazyEnumerable>d__3c(int <>1__state)
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
                            this.<sourceNumerable>5__3d = this.<>4__this.GetGenericIEnumerableFromType(this.sourceType);
                            this.<sourceElementType>5__3e = this.<>4__this.GetFirstGenericArg(this.<sourceNumerable>5__3d);
                            this.<isAnon>5__3f = typeof(TTarget).IsAnonymous();
                            this.<>7__wrap41 = ((IEnumerable) this.source).GetEnumerator();
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
                            if (this.<>7__wrap41.MoveNext())
                            {
                                this.<sourceElement>5__40 = this.<>7__wrap41.Current;
                                this.<>2__current = (TTarget) this.<>4__this.Convert(this.<sourceElement>5__40, typeof(TTarget), this.<isAnon>5__3f);
                                this.<>1__state = 1;
                                flag = false;
                                return true;
                            }
                        }
                        finally
                        {
                            if (flag)
                            {
                                this.<>7__wrap42 = this.<>7__wrap41 as IDisposable;
                                if (this.<>7__wrap42 != null)
                                {
                                    this.<>7__wrap42.Dispose();
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

                [DebuggerHidden]
                IEnumerator<TTarget> IEnumerable<TTarget>.GetEnumerator()
                {
                    UserCache.CacheConverter.<MakeLazyEnumerable>d__3c<TTarget> d__c;
                    if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                    {
                        this.<>1__state = 0;
                        d__c = (UserCache.CacheConverter.<MakeLazyEnumerable>d__3c<TTarget>) this;
                    }
                    else
                    {
                        d__c = new UserCache.CacheConverter.<MakeLazyEnumerable>d__3c<TTarget>(0) {
                            <>4__this = this.<>4__this
                        };
                    }
                    d__c.source = this.<>3__source;
                    d__c.sourceType = this.<>3__sourceType;
                    return d__c;
                }

                [DebuggerHidden]
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.System.Collections.Generic.IEnumerable<TTarget>.GetEnumerator();
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

                TTarget IEnumerator<TTarget>.Current
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

            [StructLayout(LayoutKind.Sequential)]
            private struct TypeType : IEquatable<UserCache.CacheConverter.TypeType>
            {
                public Type SourceType;
                public Type TargetType;
                public override bool Equals(object obj)
                {
                    return ((obj is UserCache.CacheConverter.TypeType) && this.Equals((UserCache.CacheConverter.TypeType) obj));
                }

                public bool Equals(UserCache.CacheConverter.TypeType other)
                {
                    return ((this.SourceType == other.SourceType) && (this.TargetType == other.TargetType));
                }

                public override int GetHashCode()
                {
                    return ((this.SourceType.GetHashCode() * 0x1f) + this.TargetType.GetHashCode());
                }
            }
        }

        private class ReferenceComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        private class TypeChangeException : Exception
        {
            public TypeChangeException() : base("Types have changed since query was last cached. Press Shift+F5 to clear cache and run again.")
            {
            }
        }

        private class UncacheableObjectException : Exception
        {
            public UncacheableObjectException(string msg) : base(msg)
            {
            }

            public UncacheableObjectException(string msg, Exception inner) : base(msg, inner)
            {
            }
        }
    }
}

