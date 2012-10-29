namespace LINQPad
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Transactions;

    internal class LINQPadDbConnection : DbConnection
    {
        private readonly DbConnection _proxy;
        internal static DbCommand LastCommand;

        public event StateChangeEventHandler StateChange
        {
            add
            {
                this._proxy.StateChange += value;
            }
            remove
            {
                this._proxy.StateChange -= value;
            }
        }

        internal LINQPadDbConnection(DbConnection proxy)
        {
            LINQPadDbConnection connection = proxy as LINQPadDbConnection;
            if (connection != null)
            {
                proxy = connection.Proxy;
            }
            this._proxy = proxy;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new LINQPadDbTransaction(this, this._proxy.BeginTransaction(isolationLevel));
        }

        public override void ChangeDatabase(string databaseName)
        {
            this._proxy.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            this._proxy.Close();
        }

        protected override DbCommand CreateDbCommand()
        {
            DbCommand proxy = this._proxy.CreateCommand();
            LastCommand = proxy;
            return new LINQPadDbCommand(proxy);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._proxy.Dispose();
            }
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            this._proxy.EnlistTransaction(transaction);
        }

        public override DataTable GetSchema()
        {
            return this._proxy.GetSchema();
        }

        public override DataTable GetSchema(string collectionName)
        {
            return this._proxy.GetSchema(collectionName);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return this._proxy.GetSchema(collectionName, restrictionValues);
        }

        public override void Open()
        {
            this._proxy.Open();
        }

        public override string ConnectionString
        {
            get
            {
                return this._proxy.ConnectionString;
            }
            set
            {
                this._proxy.ConnectionString = value;
            }
        }

        public override int ConnectionTimeout
        {
            get
            {
                return this._proxy.ConnectionTimeout;
            }
        }

        public override string Database
        {
            get
            {
                return this._proxy.Database;
            }
        }

        public override string DataSource
        {
            get
            {
                return this._proxy.DataSource;
            }
        }

        protected override System.Data.Common.DbProviderFactory DbProviderFactory
        {
            get
            {
                if (this._proxy is SqlConnection)
                {
                    return new LINQPadSqlClientProviderFactory();
                }
                if (this._proxy.GetType().Assembly.GetName().Version.Major >= 4)
                {
                    return new LINQPadSqlCE40ProviderFactory();
                }
                return new LINQPadSqlCE35ProviderFactory();
            }
        }

        public DbConnection Proxy
        {
            get
            {
                return this._proxy;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return this._proxy.ServerVersion;
            }
        }

        public override ConnectionState State
        {
            get
            {
                return this._proxy.State;
            }
        }
    }
}

