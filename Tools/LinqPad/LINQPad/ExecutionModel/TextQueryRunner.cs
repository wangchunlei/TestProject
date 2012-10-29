namespace LINQPad.ExecutionModel
{
    using System;

    internal abstract class TextQueryRunner : QueryRunner
    {
        public readonly string query;

        public TextQueryRunner(Server server, string query) : base(server)
        {
            this.query = query;
        }
    }
}

