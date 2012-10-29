namespace LINQPad
{
    using System;
    using System.Data;
    using System.Data.Common;

    internal class LINQPadDbTransaction : DbTransaction
    {
        private LINQPadDbConnection _cx;
        private DbTransaction _proxy;

        internal LINQPadDbTransaction(LINQPadDbConnection cx, DbTransaction proxy)
        {
            this._cx = cx;
            this._proxy = proxy;
        }

        public override void Commit()
        {
            this._proxy.Commit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._proxy.Dispose();
            }
        }

        public override void Rollback()
        {
            this._proxy.Rollback();
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return this._cx;
            }
        }

        public override System.Data.IsolationLevel IsolationLevel
        {
            get
            {
                return this._proxy.IsolationLevel;
            }
        }

        internal DbTransaction Proxy
        {
            get
            {
                return this._proxy;
            }
        }
    }
}

