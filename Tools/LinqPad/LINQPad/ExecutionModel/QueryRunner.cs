namespace LINQPad.ExecutionModel
{
    using System;

    internal abstract class QueryRunner
    {
        public readonly LINQPad.ExecutionModel.Server Server;

        public QueryRunner(LINQPad.ExecutionModel.Server server)
        {
            this.Server = server;
        }

        public virtual void Prepare()
        {
        }

        public abstract object Run();
    }
}

