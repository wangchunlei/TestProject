namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using System;
    using System.CodeDom.Compiler;

    internal class QueryCompilationEventArgs : EventArgs
    {
        public readonly QueryCompiler Compiler;
        public readonly TempFileRef DataContextDLL;
        public readonly bool PartialSource;

        public QueryCompilationEventArgs(QueryCompiler c, TempFileRef dataContextDLL, bool partialSource)
        {
            this.Compiler = c;
            this.DataContextDLL = dataContextDLL;
            this.PartialSource = partialSource;
        }

        public TempFileRef AssemblyDLL
        {
            get
            {
                return this.Compiler.OutputFile;
            }
        }

        public TempFileRef AssemblyPDB
        {
            get
            {
                return this.Compiler.PDBFile;
            }
        }

        public CompilerErrorCollection Errors
        {
            get
            {
                return this.Compiler.Errors;
            }
        }
    }
}

