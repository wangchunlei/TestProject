namespace LINQPad.Extensibility.DataContext
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class QueryExecutionManager
    {
        internal QueryExecutionManager(TextWriter sqlLog)
        {
            this.SqlTranslationWriter = sqlLog;
        }

        public TextWriter SqlTranslationWriter { get; private set; }
    }
}

