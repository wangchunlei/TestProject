namespace LINQPad
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class AssemblyProber
    {
        private static AssemblyReferenceCache _refCache = new AssemblyReferenceCache();

        public static string[] GetNamespaces(string assemblyPath)
        {
            using (DomainIsolator isolator = new DomainIsolator("GetNamespaces"))
            {
                return isolator.GetInstance<CustomTypeInspector>().GetNamespaces(assemblyPath);
            }
        }

        public static RefInfo GetRefs(string assemPath)
        {
            return GetRefs(new string[] { assemPath }, new string[0])[0];
        }

        private static RefInfo[] GetRefs(string[] assemPaths, string[] gacNames)
        {
            using (DomainIsolator isolator = new DomainIsolator("Inspect Custom Assembly for Refs"))
            {
                return isolator.GetInstance<CustomTypeInspector>().GetRefs(assemPaths, gacNames);
            }
        }

        private static void GetRefsWithDescendants(string assemblyFilePath, HashSet<string> visitedPaths, List<FileWithVersionInfo> results)
        {
            string item = assemblyFilePath.ToUpperInvariant();
            if (!visitedPaths.Contains(item))
            {
                visitedPaths.Add(item);
                AssemblyRefInfo info = _refCache.Get(assemblyFilePath);
                if (info != null)
                {
                    results.Add(new FileWithVersionInfo(assemblyFilePath, info.LastWriteTimeUtc, info.Length));
                    foreach (string str2 in info.ReferencedAssemblyPaths)
                    {
                        GetRefsWithDescendants(str2, visitedPaths, results);
                    }
                }
            }
        }

        public static FileWithVersionInfo[] GetSameFolderAssemblyReferenceChain(IEnumerable<string> assemblyFilePaths)
        {
            string[] strArray = (from path in assemblyFilePaths
                where _refCache.NeedsUpdate(path)
                select path).ToArray<string>();
            if (strArray.Length > 0)
            {
                using (DomainIsolator isolator = new DomainIsolator("GetSameFolderAssemblyReferenceChain"))
                {
                    isolator.GetInstance<CustomTypeInspector>().UpdateSameFolderAssemblyReferenceChain(_refCache, strArray);
                }
            }
            List<FileWithVersionInfo> results = new List<FileWithVersionInfo>();
            HashSet<string> visitedPaths = new HashSet<string>();
            foreach (string str in assemblyFilePaths)
            {
                GetRefsWithDescendants(str, visitedPaths, results);
            }
            return (from g in from r in results group r by Path.GetFileName(r.FilePath).ToUpperInvariant()
                select (from r in g
                    orderby r.LastWriteTimeUtc descending
                    select r).First<FileWithVersionInfo>() into r
                orderby r.FilePath
                select r).ToArray<FileWithVersionInfo>();
        }

        public static string[] GetSameFolderReferences(string assemblyPath)
        {
            using (DomainIsolator isolator = new DomainIsolator("GetSameFolderReferences"))
            {
                return isolator.GetInstance<CustomTypeInspector>().GetSameFolderReferences(assemblyPath, false);
            }
        }

        public static string[] GetTypeNames(string assemblyPath, string baseTypeName)
        {
            assemblyPath = assemblyPath.Trim();
            using (DomainIsolator isolator = new DomainIsolator("Inspect Custom Assembly"))
            {
                return isolator.GetInstance<CustomTypeInspector>().GetTypeNames(assemblyPath, baseTypeName);
            }
        }

        private class AssemblyReferenceCache : MarshalByRefObject
        {
            private Dictionary<string, AssemblyProber.AssemblyRefInfo> _assemRefCache = new Dictionary<string, AssemblyProber.AssemblyRefInfo>(StringComparer.InvariantCultureIgnoreCase);

            public AssemblyProber.AssemblyRefInfo Get(string assemblyFilePath)
            {
                AssemblyProber.AssemblyRefInfo info = null;
                this._assemRefCache.TryGetValue(assemblyFilePath, out info);
                return info;
            }

            public bool NeedsUpdate(string assemblyFilePath)
            {
                FileInfo info;
                try
                {
                    info = new FileInfo(assemblyFilePath);
                }
                catch
                {
                    return false;
                }
                AssemblyProber.AssemblyRefInfo info2 = this.Get(assemblyFilePath);
                return ((info2 == null) || ((info2.LastWriteTimeUtc != info.LastWriteTimeUtc) || (info2.Length != info.Length)));
            }

            public void Update(string path, DateTime lastWriteTimeUtc, long length, string[] refs)
            {
                AssemblyProber.AssemblyRefInfo info = new AssemblyProber.AssemblyRefInfo {
                    LastWriteTimeUtc = lastWriteTimeUtc,
                    Length = length,
                    ReferencedAssemblyPaths = refs
                };
                this._assemRefCache[path] = info;
            }
        }

        private class AssemblyRefInfo
        {
            public DateTime LastWriteTimeUtc;
            public long Length;
            public string[] ReferencedAssemblyPaths;
        }

        private class CustomTypeInspector : MarshalByRefObject
        {
            private static HashSet<string> PopularSystemReferences = new HashSet<string>((from r in QueryCompiler.DefaultReferences select Path.GetFileNameWithoutExtension(r)).Concat<string>("mscorlib System.Windows.Forms System.Deployment System.Data.Services System.Data.Services.Client System.ServiceModel System.Web PresentationCore PresentationFramework WindowsBase".Split(new char[0])), StringComparer.InvariantCultureIgnoreCase);

            [CompilerGenerated]
            private static string <.cctor>b__3b(string r)
            {
                return Path.GetFileNameWithoutExtension(r);
            }

            public string[] GetEFMetaDataNames(string assemblyPath)
            {
                return (from name in Assembly.LoadFrom(assemblyPath).GetManifestResourceNames()
                    let dotPos = name.LastIndexOf('.')
                    where (dotPos > 0) && (dotPos < (name.Length - 2))
                    let ext = name.Substring(dotPos + 1).ToLowerInvariant()
                    where ((ext == "csdl") || (ext == "msl")) || (ext == "ssdl")
                    select name).ToArray<string>();
            }

            public string[] GetNamespaces(string assemblyPath)
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                try
                {
                    return (from n in (from t in assembly.GetExportedTypes() select t.Namespace).Distinct<string>()
                        orderby n
                        select n).ToArray<string>();
                }
                catch (Exception exception)
                {
                    Log.Write(exception, "AssemblyProber.GetNamespaces");
                    return new string[0];
                }
            }

            public AssemblyProber.RefInfo[] GetRefs(string[] assemPaths, string[] gacNames)
            {
                return (from p in assemPaths select new AssemblyProber.RefInfo { FullPath = p, SimpleName = AssemblyName.GetAssemblyName(p).Name, Refs = (from a in Assembly.ReflectionOnlyLoadFrom(p).GetReferencedAssemblies() select a.Name).ToArray<string>() }).Concat<AssemblyProber.RefInfo>((from n in gacNames select new AssemblyProber.RefInfo { FullName = n, SimpleName = new AssemblyName(n).Name, Refs = (from a in Assembly.ReflectionOnlyLoad(n).GetReferencedAssemblies() select a.Name).ToArray<string>() })).ToArray<AssemblyProber.RefInfo>();
            }

            public string[] GetSameFolderReferences(string assemblyPath, bool includeConfigFiles)
            {
                return this.YieldSameFolderReferences(assemblyPath, includeConfigFiles).ToArray<string>();
            }

            public string[] GetTypeNames(string assemblyPath, string baseTypeName)
            {
                Type[] exportedTypes;
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                try
                {
                    exportedTypes = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    if ((exception.LoaderExceptions != null) && (exception.LoaderExceptions.Length > 0))
                    {
                        throw exception.LoaderExceptions[0];
                    }
                    throw;
                }
                return (from t in exportedTypes
                    where string.IsNullOrEmpty(baseTypeName) || IsOfType(t, baseTypeName)
                    where !t.Name.Contains("<>")
                    select t.FullName).ToArray<string>();
            }

            private static bool IsOfType(Type t, string baseTypeName)
            {
                Func<Type, bool> predicate = null;
                while (t != null)
                {
                    if (t.FullName == baseTypeName)
                    {
                        return true;
                    }
                    if (predicate == null)
                    {
                        predicate = i => i.FullName == baseTypeName;
                    }
                    if (t.GetInterfaces().Any<Type>(predicate))
                    {
                        return true;
                    }
                    t = t.BaseType;
                }
                return false;
            }

            public void UpdateSameFolderAssemblyReferenceChain(AssemblyProber.AssemblyReferenceCache cache, string[] assemblyFilePaths)
            {
                HashSet<string> visitedPaths = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (string str in assemblyFilePaths)
                {
                    this.UpdateSameFolderAssemblyReferenceChain(cache, str, visitedPaths);
                }
            }

            public void UpdateSameFolderAssemblyReferenceChain(AssemblyProber.AssemblyReferenceCache cache, string assemblyFilePath, HashSet<string> visitedPaths)
            {
                FileInfo info;
                try
                {
                    info = new FileInfo(assemblyFilePath);
                    if (!info.Exists)
                    {
                        return;
                    }
                }
                catch
                {
                    return;
                }
                string[] sameFolderReferences = this.GetSameFolderReferences(assemblyFilePath, true);
                cache.Update(assemblyFilePath, info.LastWriteTimeUtc, info.Length, sameFolderReferences);
                visitedPaths.Add(assemblyFilePath);
                foreach (string str in sameFolderReferences)
                {
                    if (!(visitedPaths.Contains(str) || !cache.NeedsUpdate(str)))
                    {
                        this.UpdateSameFolderAssemblyReferenceChain(cache, str, visitedPaths);
                    }
                }
            }

            private IEnumerable<string> YieldSameFolderReferences(string assemblyPath, bool includeConfigFiles)
            {
                return new <YieldSameFolderReferences>d__2c(-2) { <>4__this = this, <>3__assemblyPath = assemblyPath, <>3__includeConfigFiles = includeConfigFiles };
            }

            [CompilerGenerated]
            private sealed class <YieldSameFolderReferences>d__2c : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
            {
                private bool $__disposing;
                private int <>1__state;
                private string <>2__current;
                public string <>3__assemblyPath;
                public bool <>3__includeConfigFiles;
                public AssemblyProber.CustomTypeInspector <>4__this;
                public AssemblyName[] <>7__wrap33;
                public int <>7__wrap34;
                private int <>l__initialThreadId;
                public Assembly <a>5__2d;
                public string <configPath>5__32;
                public string <dir>5__2e;
                public string <dllPath>5__30;
                public string <exePath>5__31;
                public AssemblyName <r>5__2f;
                public string assemblyPath;
                public bool includeConfigFiles;

                [DebuggerHidden]
                public <YieldSameFolderReferences>d__2c(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                    this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
                }

                private bool MoveNext()
                {
                    try
                    {
                        bool flag = true;
                        switch (this.<>1__state)
                        {
                            case 1:
                            case 2:
                                break;

                            case 3:
                                if (!this.$__disposing)
                                {
                                    goto Label_0284;
                                }
                                return false;

                            case -1:
                                return false;

                            default:
                                if (this.$__disposing)
                                {
                                    return false;
                                }
                                if ((Path.GetExtension(this.assemblyPath).ToLowerInvariant() == ".config") || !File.Exists(this.assemblyPath))
                                {
                                    goto Label_0297;
                                }
                                try
                                {
                                    this.<a>5__2d = Assembly.LoadFrom(this.assemblyPath);
                                }
                                catch
                                {
                                    goto Label_0297;
                                }
                                this.<dir>5__2e = Path.GetDirectoryName(this.assemblyPath);
                                break;
                        }
                        switch (this.<>1__state)
                        {
                            case 1:
                                if (!this.$__disposing)
                                {
                                    break;
                                }
                                return false;

                            case 2:
                                if (!this.$__disposing)
                                {
                                    goto Label_0105;
                                }
                                return false;

                            default:
                                this.<>7__wrap33 = this.<a>5__2d.GetReferencedAssemblies();
                                this.<>7__wrap34 = 0;
                                goto Label_0111;
                        }
                        this.<>1__state = 0;
                        goto Label_01DD;
                    Label_0105:
                        this.<>1__state = 0;
                        goto Label_01DD;
                    Label_0111:
                        if (this.<>7__wrap34 >= this.<>7__wrap33.Length)
                        {
                            goto Label_01F0;
                        }
                        this.<r>5__2f = this.<>7__wrap33[this.<>7__wrap34];
                        if (!AssemblyProber.CustomTypeInspector.PopularSystemReferences.Contains(this.<r>5__2f.Name))
                        {
                            this.<dllPath>5__30 = Path.Combine(this.<dir>5__2e, this.<r>5__2f.Name + ".dll");
                            if (File.Exists(this.<dllPath>5__30))
                            {
                                goto Label_0243;
                            }
                            if (!this.<r>5__2f.Name.Equals("linqpad.exe", StringComparison.InvariantCultureIgnoreCase))
                            {
                                this.<exePath>5__31 = Path.Combine(this.<dir>5__2e, this.<r>5__2f.Name + ".exe");
                                if (File.Exists(this.<exePath>5__31))
                                {
                                    goto Label_025C;
                                }
                            }
                        }
                    Label_01DD:
                        this.<>7__wrap34++;
                        goto Label_0111;
                    Label_01F0:
                        if (!flag)
                        {
                        }
                        if (!this.includeConfigFiles)
                        {
                            goto Label_0297;
                        }
                        this.<configPath>5__32 = this.assemblyPath + ".config";
                        if (!File.Exists(this.<configPath>5__32))
                        {
                            goto Label_0297;
                        }
                        this.<>2__current = this.<configPath>5__32;
                        this.<>1__state = 3;
                        flag = false;
                        return true;
                    Label_0243:
                        this.<>2__current = this.<dllPath>5__30;
                        this.<>1__state = 1;
                        flag = false;
                        return true;
                    Label_025C:
                        this.<>2__current = this.<exePath>5__31;
                        this.<>1__state = 2;
                        flag = false;
                        return true;
                    Label_0284:
                        this.<>1__state = 0;
                    }
                    catch (Exception)
                    {
                        this.<>1__state = -1;
                        throw;
                    }
                Label_0297:
                    this.<>1__state = -1;
                    return false;
                }

                [DebuggerHidden]
                IEnumerator<string> IEnumerable<string>.GetEnumerator()
                {
                    AssemblyProber.CustomTypeInspector.<YieldSameFolderReferences>d__2c d__c;
                    if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                    {
                        this.<>1__state = 0;
                        d__c = this;
                    }
                    else
                    {
                        d__c = new AssemblyProber.CustomTypeInspector.<YieldSameFolderReferences>d__2c(0) {
                            <>4__this = this.<>4__this
                        };
                    }
                    d__c.assemblyPath = this.<>3__assemblyPath;
                    d__c.includeConfigFiles = this.<>3__includeConfigFiles;
                    return d__c;
                }

                [DebuggerHidden]
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();
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

                string IEnumerator<string>.Current
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

        [Serializable]
        public class RefInfo
        {
            public string FullName;
            public string FullPath;
            public string[] Refs;
            public string SimpleName;
        }
    }
}

