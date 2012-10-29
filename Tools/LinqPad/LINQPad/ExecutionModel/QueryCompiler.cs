namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal abstract class QueryCompiler : CodeCompiler
    {
        internal static string[] DefaultReferences = new string[] { "System.dll", "Microsoft.CSharp.dll", "System.Core.dll", "System.Data.dll", "System.Transactions.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.Linq.dll", "System.Drawing.dll", "System.Data.DataSetExtensions.dll", typeof(DataContextBase).Assembly.Location };
        public UniqueStringCollection ImportedNamespaces = new UniqueStringCollection { 
            "System", "System.IO", "System.Text", "System.Text.RegularExpressions", "System.Diagnostics", "System.Threading", "System.Reflection", "System.Collections", "System.Collections.Generic", "System.Linq", "System.Linq.Expressions", "System.Data", "System.Data.SqlClient", "System.Data.Linq", "System.Data.Linq.SqlClient", "System.Transactions", 
            "System.Xml", "System.Xml.Linq", "System.Xml.XPath", "LINQPad"
         };
        protected static readonly string PredicateBuilderMessage = "\r\n\r\nNote that to use PredicateBuilder in LINQPad you must check 'Include PredicateBuilder' in Query Options (press F4)";
        public UniqueStringCollection References = new UniqueStringCollection(DefaultReferences, new FileNameComparer());

        protected QueryCompiler(QueryCore query, bool addReferences)
        {
            this.IsMyExtensions = query.IsMyExtensions;
            if (this.IsMyExtensions)
            {
                MyExtensions.UpdateAdditionalRefs(query);
            }
            if (GacResolver.IsEntityFrameworkAvailable)
            {
                this.References.Add("System.Data.Entity.dll");
            }
            DataContextDriver driver = query.GetDriver(true);
            if ((query.Repository != null) && (driver != null))
            {
                this.ImportedNamespaces.RemoveRange(driver.GetNamespacesToRemoveInternal(query.Repository));
                this.ImportedNamespaces.AddRange(driver.GetNamespacesToAddInternal(query.Repository));
                if (addReferences)
                {
                    this.References.AddRange(query.Repository.GetDriverAssemblies());
                }
            }
            if (addReferences)
            {
                this.References.AddRange(PluginAssembly.GetCompatibleAssemblies(query.IsMyExtensions));
            }
            this.ImportedNamespaces.AddRange(query.AdditionalNamespaces);
            if (!string.IsNullOrEmpty(query.QueryBaseClassNamespace))
            {
                this.ImportedNamespaces.Add(query.QueryBaseClassNamespace);
            }
            if (addReferences)
            {
                this.References.AddRange(query.AllFileReferences);
                foreach (string str in query.AdditionalGACReferences)
                {
                    string item = GacResolver.FindPath(str);
                    if (item != null)
                    {
                        this.References.Add(item);
                    }
                }
                this.References.AddRange(query.GetStaticSchemaSameFolderReferences());
                if (!this.IsMyExtensions)
                {
                    this.References.AddRange(MyExtensions.AdditionalRefs);
                }
            }
        }

        public abstract bool Compile(string queryText, QueryCore query, TempFileRef dataContextAssembly);
        public bool Compile(CodeDomProvider codeProvider, string source, TempFileRef dataContextAssembly, QueryLanguage queryKind, string compilerOptions)
        {
            if (this.IsMyExtensions)
            {
                string directoryName = Path.GetDirectoryName(MyExtensions.QueryFilePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                TempFileRef ref2 = new TempFileRef(Path.ChangeExtension(MyExtensions.QueryFilePath, ".dll")) {
                    AutoDelete = false
                };
                this.OutputFile = ref2;
                TempFileRef ref3 = new TempFileRef(Path.ChangeExtension(MyExtensions.QueryFilePath, ".pdb")) {
                    AutoDelete = false
                };
                this.PDBFile = ref3;
            }
            else
            {
                this.OutputFile = TempFileRef.GetRandom("query", ".dll");
                this.PDBFile = new TempFileRef(Path.ChangeExtension(this.OutputFile.FullPath, ".pdb"));
            }
            List<string> references = new List<string>(this.References);
            if (!((dataContextAssembly == null) || string.IsNullOrEmpty(dataContextAssembly.FullPath)))
            {
                references.Add(dataContextAssembly.FullPath);
            }
            return base.Compile(codeProvider, references, source.ToString(), this.OutputFile.FullPath, compilerOptions);
        }

        public static QueryCompiler Create(QueryCore query, bool addReferences)
        {
            if (query.QueryKind.ToString().StartsWith("VB", StringComparison.Ordinal))
            {
                return new VBQueryCompiler(query, addReferences);
            }
            if (query.QueryKind.ToString().StartsWith("FSharp", StringComparison.Ordinal))
            {
                return new FSharpQueryCompiler(query, addReferences);
            }
            return new CSharpQueryCompiler(query, addReferences);
        }

        protected ParameterDescriptor[] GetContextConstructorParams(DataContextDriver driver, Repository r)
        {
            ParameterDescriptor[] descriptorArray2;
            try
            {
                descriptorArray2 = driver.GetContextConstructorParameters(r) ?? new ParameterDescriptor[0];
            }
            catch (Exception exception)
            {
                if (driver.IsBuiltIn)
                {
                    throw;
                }
                Log.Write(exception, "GetContextConstructorParameters");
                throw new DisplayToUserException(exception.Message);
            }
            return descriptorArray2;
        }

        public abstract string GetFooter(QueryCore query);
        public abstract string GetHeader(QueryCore query);

        public bool IsMyExtensions { get; private set; }

        public TempFileRef OutputFile { get; protected set; }

        public TempFileRef PDBFile { get; protected set; }
    }
}

