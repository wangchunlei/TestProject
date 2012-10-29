namespace LINQPad
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;

    internal class LINQPadDbCommand : DbCommand, ICloneable
    {
        private LINQPadDbConnection _cx;
        private readonly DbCommand _proxy;
        private LINQPadDbTransaction _tx;

        internal LINQPadDbCommand(DbCommand proxy)
        {
            this._proxy = proxy;
        }

        public override void Cancel()
        {
            this._proxy.Cancel();
        }

        public object Clone()
        {
            return new LINQPadDbCommand((DbCommand) ((ICloneable) this._proxy).Clone());
        }

        protected override DbParameter CreateDbParameter()
        {
            return this._proxy.CreateParameter();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._proxy.Dispose();
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            LINQPadDbController.DbCommandExecuting(this);
            Stopwatch stopwatch = Stopwatch.StartNew();
            DbDataReader reader = this._proxy.ExecuteReader(behavior);
            stopwatch.Stop();
            LINQPadDbController.DbCommandFinished(stopwatch.Elapsed);
            return reader;
        }

        public override int ExecuteNonQuery()
        {
            LINQPadDbController.DbCommandExecuting(this);
            Stopwatch stopwatch = Stopwatch.StartNew();
            int num = this._proxy.ExecuteNonQuery();
            stopwatch.Stop();
            LINQPadDbController.DbCommandFinished(stopwatch.Elapsed);
            return num;
        }

        public override object ExecuteScalar()
        {
            LINQPadDbController.DbCommandExecuting(this);
            Stopwatch stopwatch = Stopwatch.StartNew();
            object obj2 = this._proxy.ExecuteScalar();
            stopwatch.Stop();
            LINQPadDbController.DbCommandFinished(stopwatch.Elapsed);
            return obj2;
        }

        public override void Prepare()
        {
            this._proxy.Prepare();
        }

        public override string CommandText
        {
            get
            {
                return this._proxy.CommandText;
            }
            set
            {
                this._proxy.CommandText = value;
            }
        }

        public override int CommandTimeout
        {
            get
            {
                return this._proxy.CommandTimeout;
            }
            set
            {
                this._proxy.CommandTimeout = value;
            }
        }

        public override System.Data.CommandType CommandType
        {
            get
            {
                return this._proxy.CommandType;
            }
            set
            {
                this._proxy.CommandType = value;
            }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                if (this._cx == null)
                {
                    System.Data.Common.DbConnection connection = this._proxy.Connection;
                    if (connection is LINQPadDbConnection)
                    {
                        this._cx = (LINQPadDbConnection) connection;
                    }
                    else
                    {
                        this._cx = new LINQPadDbConnection(connection);
                    }
                }
                return this._cx;
            }
            set
            {
                if (value is LINQPadDbConnection)
                {
                    this._cx = (LINQPadDbConnection) value;
                    this._proxy.Connection = this._cx.Proxy;
                }
                else
                {
                    this._proxy.Connection = value;
                    this._cx = new LINQPadDbConnection(this._proxy.Connection);
                }
            }
        }

        protected override System.Data.Common.DbParameterCollection DbParameterCollection
        {
            get
            {
                return this._proxy.Parameters;
            }
        }

        protected override System.Data.Common.DbTransaction DbTransaction
        {
            get
            {
                return this._tx;
            }
            set
            {
                if (value is LINQPadDbTransaction)
                {
                    this._tx = (LINQPadDbTransaction) value;
                    this._proxy.Transaction = this._tx.Proxy;
                }
                else
                {
                    this._proxy.Transaction = value;
                    this._tx = new LINQPadDbTransaction(this._cx, value);
                }
            }
        }

        public override bool DesignTimeVisible
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        internal DbCommand Proxy
        {
            get
            {
                return this._proxy;
            }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return this._proxy.UpdatedRowSource;
            }
            set
            {
                this._proxy.UpdatedRowSource = value;
            }
        }
    }
}

