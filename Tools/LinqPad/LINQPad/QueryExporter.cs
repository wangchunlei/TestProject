namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal static class QueryExporter
    {
        public static void Go(IEnumerable<QueryCore> queries, string baseFolder)
        {
            foreach (QueryCore core in queries)
            {
                if ((((core.QueryKind == QueryLanguage.Expression) || (core.QueryKind == QueryLanguage.Statements)) || (core.QueryKind == QueryLanguage.Program)) && (core.Repository == null))
                {
                    CSharpQueryCompiler compiler = new CSharpQueryCompiler(core, false);
                    string contents = compiler.GetHeader(core) + core.Source + "\r\n" + compiler.GetFooter(core);
                    File.WriteAllText(Path.Combine(baseFolder, core.Name + ".cs"), contents);
                }
            }
        }
    }
}

