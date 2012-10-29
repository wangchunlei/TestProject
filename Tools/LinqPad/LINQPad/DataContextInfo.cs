namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class DataContextInfo
    {
        public DataContextInfo(LINQPad.Repository repository, bool busy, string status, string error, TempFileRef assemblyPath, IEnumerable<ExplorerItem> schema)
        {
            this.Repository = repository;
            this.Busy = busy;
            this.Status = status;
            this.Error = error;
            this.AssemblyPath = assemblyPath;
            this.Schema = schema;
        }

        public TempFileRef AssemblyPath { get; private set; }

        public bool Busy { get; private set; }

        public string Error { get; private set; }

        public LINQPad.Repository Repository { get; private set; }

        public IEnumerable<ExplorerItem> Schema { get; private set; }

        public string Status { get; private set; }
    }
}

