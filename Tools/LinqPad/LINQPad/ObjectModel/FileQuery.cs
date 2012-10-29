namespace LINQPad.ObjectModel
{
    using LINQPad;
    using LINQPad.ObjectGraph;
    using LINQPad.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class FileQuery : Query
    {
        private DateTime _lastWriteTime;
        private object _openLink;

        private FileQuery(string fullName, string filePath, DateTime lastWriteTime)
        {
            base.SetDisplayPath(fullName);
            this.FilePath = filePath;
            this._lastWriteTime = lastWriteTime;
            this._openLink = new FileCommandLink(this.FilePath, base.Name);
        }

        internal static Query FromPath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            string defaultQueryFolder = Options.GetDefaultQueryFolder(false);
            string fullName = filePath;
            if (!(string.IsNullOrEmpty(defaultQueryFolder) || !filePath.ToLowerInvariant().StartsWith(defaultQueryFolder.ToLowerInvariant())))
            {
                fullName = fullName.Substring(defaultQueryFolder.Length);
            }
            return new FileQuery(fullName, filePath, new FileInfo(filePath).LastWriteTime);
        }

        protected override string GetData()
        {
            try
            {
                return File.ReadAllText(this.FilePath);
            }
            catch
            {
                return "";
            }
        }

        private static IEnumerable<Query> GetFromFolder(DirectoryInfo dir, string root)
        {
            return new <GetFromFolder>d__8(-2) { <>3__dir = dir, <>3__root = root };
        }

        internal static IEnumerable<Query> GetMyQueries()
        {
            string defaultQueryFolder = Options.GetDefaultQueryFolder(true);
            if (!Directory.Exists(defaultQueryFolder))
            {
                return new Query[0];
            }
            return GetFromFolder(new DirectoryInfo(defaultQueryFolder), defaultQueryFolder);
        }

        internal override void Open()
        {
            MainForm.Instance.OpenQuery(this.FilePath, true);
        }

        internal override QueryCore ToQueryCore()
        {
            QueryCore core = new QueryCore();
            core.Open(this.FilePath);
            return core;
        }

        public override DateTime? LastModified
        {
            get
            {
                return new DateTime?(this._lastWriteTime);
            }
        }

        public override object OpenLink
        {
            get
            {
                return this._openLink;
            }
        }

        [CompilerGenerated]
        private sealed class <GetFromFolder>d__8 : IEnumerable<Query>, IEnumerable, IEnumerator<Query>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private Query <>2__current;
            public DirectoryInfo <>3__dir;
            public string <>3__root;
            public IEnumerator<FileInfo> <>7__wrap10;
            public IEnumerator<DirectoryInfo> <>7__wrape;
            public IEnumerator<Query> <>7__wrapf;
            private int <>l__initialThreadId;
            public IEnumerable<DirectoryInfo> <dirs>5__9;
            public FileInfo <file>5__d;
            public IEnumerable<FileInfo> <files>5__c;
            public Query <q>5__b;
            public DirectoryInfo <subDir>5__a;
            public DirectoryInfo dir;
            public string root;

            [DebuggerHidden]
            public <GetFromFolder>d__8(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    int num;
                    bool flag = true;
                    switch (this.<>1__state)
                    {
                        case 1:
                            break;

                        case 2:
                            goto Label_0233;

                        case -1:
                            return false;

                        default:
                            if (this.$__disposing)
                            {
                                return false;
                            }
                            this.<dirs>5__9 = new DirectoryInfo[0];
                            try
                            {
                                this.<dirs>5__9 = this.dir.GetDirectories();
                            }
                            catch
                            {
                            }
                            if (FileQuery.CS$<>9__CachedAnonymousMethodDelegate4 == null)
                            {
                                FileQuery.CS$<>9__CachedAnonymousMethodDelegate4 = new Func<DirectoryInfo, bool>(FileQuery.<GetFromFolder>b__0);
                            }
                            if (FileQuery.CS$<>9__CachedAnonymousMethodDelegate5 == null)
                            {
                                FileQuery.CS$<>9__CachedAnonymousMethodDelegate5 = new Func<DirectoryInfo, string>(FileQuery.<GetFromFolder>b__1);
                            }
                            this.<dirs>5__9 = this.<dirs>5__9.Where<DirectoryInfo>(FileQuery.CS$<>9__CachedAnonymousMethodDelegate4).OrderBy<DirectoryInfo, string>(FileQuery.CS$<>9__CachedAnonymousMethodDelegate5);
                            this.<>7__wrape = this.<dirs>5__9.GetEnumerator();
                            break;
                    }
                    try
                    {
                        num = this.<>1__state;
                        if (num != 1)
                        {
                            goto Label_0188;
                        }
                    Label_00D7:
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
                            if (!this.<>7__wrapf.MoveNext())
                            {
                                goto Label_0188;
                            }
                            this.<q>5__b = this.<>7__wrapf.Current;
                            this.<>2__current = this.<q>5__b;
                            this.<>1__state = 1;
                            flag = false;
                            return true;
                        }
                        finally
                        {
                            if (flag && (this.<>7__wrapf != null))
                            {
                                this.<>7__wrapf.Dispose();
                            }
                        }
                    Label_0156:
                        this.<subDir>5__a = this.<>7__wrape.Current;
                        this.<>7__wrapf = FileQuery.GetFromFolder(this.<subDir>5__a, this.root).GetEnumerator();
                        goto Label_00D7;
                    Label_0188:
                        if (this.<>7__wrape.MoveNext())
                        {
                            goto Label_0156;
                        }
                    }
                    finally
                    {
                        if (flag && (this.<>7__wrape != null))
                        {
                            this.<>7__wrape.Dispose();
                        }
                    }
                    this.<files>5__c = new FileInfo[0];
                    try
                    {
                        this.<files>5__c = this.dir.GetFiles("*.linq");
                    }
                    catch
                    {
                    }
                    if (FileQuery.CS$<>9__CachedAnonymousMethodDelegate6 == null)
                    {
                        FileQuery.CS$<>9__CachedAnonymousMethodDelegate6 = new Func<FileInfo, bool>(FileQuery.<GetFromFolder>b__2);
                    }
                    if (FileQuery.CS$<>9__CachedAnonymousMethodDelegate7 == null)
                    {
                        FileQuery.CS$<>9__CachedAnonymousMethodDelegate7 = new Func<FileInfo, string>(FileQuery.<GetFromFolder>b__3);
                    }
                    this.<>7__wrap10 = this.<files>5__c.Where<FileInfo>(FileQuery.CS$<>9__CachedAnonymousMethodDelegate6).OrderBy<FileInfo, string>(FileQuery.CS$<>9__CachedAnonymousMethodDelegate7).GetEnumerator();
                Label_0233:
                    try
                    {
                        num = this.<>1__state;
                        if (num == 2)
                        {
                            if (this.$__disposing)
                            {
                                return false;
                            }
                            this.<>1__state = 0;
                        }
                        if (this.<>7__wrap10.MoveNext())
                        {
                            this.<file>5__d = this.<>7__wrap10.Current;
                            this.<>2__current = new FileQuery(this.<file>5__d.FullName.Substring(this.root.Length), this.<file>5__d.FullName, this.<file>5__d.LastWriteTime);
                            this.<>1__state = 2;
                            flag = false;
                            return true;
                        }
                    }
                    finally
                    {
                        if (flag && (this.<>7__wrap10 != null))
                        {
                            this.<>7__wrap10.Dispose();
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
            IEnumerator<Query> IEnumerable<Query>.GetEnumerator()
            {
                FileQuery.<GetFromFolder>d__8 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new FileQuery.<GetFromFolder>d__8(0);
                }
                d__.dir = this.<>3__dir;
                d__.root = this.<>3__root;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<LINQPad.ObjectModel.Query>.GetEnumerator();
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

            Query IEnumerator<Query>.Current
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

