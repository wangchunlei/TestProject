namespace LINQPad.ObjectModel
{
    using Ionic.Zip;
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
    using System.Text.RegularExpressions;
    using System.Threading;

    internal class ImportedSampleQuery : Query
    {
        private string _id;
        private object _openLink;
        private string _pathWithinZip;
        private static Regex _sortPrefixExpander = new Regex(@"(?<=/\[?)[0-9\.]+");
        private static Regex _sortPrefixWithBrackets = new Regex(@"^\[[0-9\.]+\]\s*");
        private string _zipFilePath;

        private ImportedSampleQuery(string fullName, string id, string zipFilePath, string pathWithinZip)
        {
            base.SetDisplayPath(fullName);
            this._id = id;
            this._zipFilePath = zipFilePath;
            this._pathWithinZip = pathWithinZip;
            this._openLink = new SampleCommandLink(this._id, base.Name);
        }

        internal static string Get3rdPartyFileName(string nameWithinZip)
        {
            if (nameWithinZip.Contains<char>('/'))
            {
                nameWithinZip = nameWithinZip.Substring(nameWithinZip.LastIndexOf('/') + 1);
            }
            string str = _sortPrefixWithBrackets.Replace(nameWithinZip, "");
            if (str.EndsWith(".linq", StringComparison.OrdinalIgnoreCase))
            {
                str = str.Substring(0, str.Length - 5);
            }
            return str;
        }

        private static IEnumerable<Query> Get3rdPartySamples(string target)
        {
            return new <Get3rdPartySamples>d__e(-2) { <>3__target = target };
        }

        internal static string Get3rdPartySort(string nameWithinZip)
        {
            return _sortPrefixExpander.Replace("/" + nameWithinZip, (MatchEvaluator) (m => PadDigits(m.Value)));
        }

        internal static IEnumerable<Query> GetAll()
        {
            return new <GetAll>d__0(-2);
        }

        protected override string GetData()
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                using (ZipFile file = new ZipFile(this._zipFilePath))
                {
                    file.get_Item(this._pathWithinZip).Extract(stream);
                }
                stream.Position = 0L;
                return new StreamReader(stream).ReadToEnd();
            }
            catch
            {
                return "";
            }
        }

        private static string GetFullName(string providerName, string zipFileName)
        {
            string[] strArray = (from p in (providerName + "/" + zipFileName).Split(new char[] { '/' }) select _sortPrefixWithBrackets.Replace(p, "")).ToArray<string>();
            return string.Join(@"\", strArray);
        }

        internal override void Open()
        {
            MainForm.Instance.OpenSampleQuery(this._id);
        }

        private static string PadDigits(string s)
        {
            return string.Join(".", (from digits in s.Split(new char[] { '.' }) select digits.PadLeft(10, '0')).ToArray<string>());
        }

        public override object OpenLink
        {
            get
            {
                return this._openLink;
            }
        }

        [CompilerGenerated]
        private sealed class <Get3rdPartySamples>d__e : IEnumerable<Query>, IEnumerable, IEnumerator<Query>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private Query <>2__current;
            public string <>3__target;
            public ZipFile <>7__wrap14;
            public IEnumerator<ZipEntry> <>7__wrap15;
            private int <>l__initialThreadId;
            public string <fullName>5__13;
            public ZipEntry <item>5__11;
            public string <name>5__12;
            public string <providerName>5__f;
            public ZipFile <z>5__10;
            public string target;

            [DebuggerHidden]
            public <Get3rdPartySamples>d__e(int <>1__state)
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
                        if (!File.Exists(this.target))
                        {
                            goto Label_01F9;
                        }
                        this.<providerName>5__f = new DirectoryInfo(Path.GetDirectoryName(this.target)).Name;
                        try
                        {
                            this.<z>5__10 = new ZipFile(this.target);
                        }
                        catch
                        {
                            goto Label_01F9;
                        }
                        this.<>7__wrap14 = this.<z>5__10;
                    }
                    try
                    {
                        num = this.<>1__state;
                        if (num != 1)
                        {
                            if (ImportedSampleQuery.CS$<>9__CachedAnonymousMethodDelegatec == null)
                            {
                                ImportedSampleQuery.CS$<>9__CachedAnonymousMethodDelegatec = new Func<ZipEntry, bool>(ImportedSampleQuery.<Get3rdPartySamples>b__a);
                            }
                            if (ImportedSampleQuery.CS$<>9__CachedAnonymousMethodDelegated == null)
                            {
                                ImportedSampleQuery.CS$<>9__CachedAnonymousMethodDelegated = new Func<ZipEntry, string>(ImportedSampleQuery.<Get3rdPartySamples>b__b);
                            }
                            this.<>7__wrap15 = this.<z>5__10.Where<ZipEntry>(ImportedSampleQuery.CS$<>9__CachedAnonymousMethodDelegatec).OrderBy<ZipEntry, string>(ImportedSampleQuery.CS$<>9__CachedAnonymousMethodDelegated).GetEnumerator();
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
                            if (this.<>7__wrap15.MoveNext())
                            {
                                this.<item>5__11 = this.<>7__wrap15.Current;
                                this.<name>5__12 = ImportedSampleQuery.Get3rdPartyFileName(this.<item>5__11.get_FileName());
                                this.<fullName>5__13 = ImportedSampleQuery.GetFullName(this.<providerName>5__f, this.<item>5__11.get_FileName());
                                this.<>2__current = new ImportedSampleQuery(this.<fullName>5__13, this.target + "/" + this.<item>5__11.get_FileName(), this.target, this.<item>5__11.get_FileName());
                                this.<>1__state = 1;
                                flag = false;
                                return true;
                            }
                        }
                        finally
                        {
                            if (flag && (this.<>7__wrap15 != null))
                            {
                                this.<>7__wrap15.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        if (flag && (this.<>7__wrap14 != null))
                        {
                            this.<>7__wrap14.Dispose();
                        }
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
            Label_01F9:
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Query> IEnumerable<Query>.GetEnumerator()
            {
                ImportedSampleQuery.<Get3rdPartySamples>d__e _e;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _e = this;
                }
                else
                {
                    _e = new ImportedSampleQuery.<Get3rdPartySamples>d__e(0);
                }
                _e.target = this.<>3__target;
                return _e;
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

        [CompilerGenerated]
        private sealed class <GetAll>d__0 : IEnumerable<Query>, IEnumerable, IEnumerator<Query>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private Query <>2__current;
            public string[] <>7__wrap5;
            public int <>7__wrap6;
            public IEnumerator<Query> <>7__wrap7;
            private int <>l__initialThreadId;
            public string <baseFolder>5__1;
            public string <providerPath>5__2;
            public Query <query>5__4;
            public string <target>5__3;

            [DebuggerHidden]
            public <GetAll>d__0(int <>1__state)
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
                        this.<baseFolder>5__1 = Path.Combine(Program.UserDataFolder, "Samples");
                        if (!Directory.Exists(this.<baseFolder>5__1))
                        {
                            goto Label_018E;
                        }
                    }
                    num = this.<>1__state;
                    if (num != 1)
                    {
                        this.<>7__wrap5 = Directory.GetDirectories(this.<baseFolder>5__1);
                        this.<>7__wrap6 = 0;
                        goto Label_016A;
                    }
                Label_0084:
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
                        if (!this.<>7__wrap7.MoveNext())
                        {
                            goto Label_013F;
                        }
                        this.<query>5__4 = this.<>7__wrap7.Current;
                        this.<>2__current = this.<query>5__4;
                        this.<>1__state = 1;
                        flag = false;
                        return true;
                    }
                    finally
                    {
                        if (flag && (this.<>7__wrap7 != null))
                        {
                            this.<>7__wrap7.Dispose();
                        }
                    }
                Label_0106:
                    this.<providerPath>5__2 = this.<>7__wrap5[this.<>7__wrap6];
                    this.<target>5__3 = Path.Combine(this.<providerPath>5__2, "queries.zip");
                    if (File.Exists(this.<target>5__3))
                    {
                        goto Label_014F;
                    }
                Label_013F:
                    this.<>7__wrap6++;
                    goto Label_016A;
                Label_014F:
                    this.<>7__wrap7 = ImportedSampleQuery.Get3rdPartySamples(this.<target>5__3).GetEnumerator();
                    goto Label_0084;
                Label_016A:
                    if (this.<>7__wrap6 < this.<>7__wrap5.Length)
                    {
                        goto Label_0106;
                    }
                    if (!flag)
                    {
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
            Label_018E:
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Query> IEnumerable<Query>.GetEnumerator()
            {
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    return this;
                }
                return new ImportedSampleQuery.<GetAll>d__0(0);
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

