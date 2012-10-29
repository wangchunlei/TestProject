namespace LINQPad
{
    using System;
    using System.Data;
    using System.Data.Common;

    internal class DbFactory
    {
        private string _provider;

        public DbFactory(string provider)
        {
            this._provider = provider;
        }

        public virtual DbDataAdapter CreateDataAdapter(string cmdText, IDbConnection cx)
        {
            DbCommand command = (DbCommand) cx.CreateCommand();
            command.CommandText = cmdText;
            DbDataAdapter adapter = this.GetProviderFactory().CreateDataAdapter();
            adapter.SelectCommand = command;
            return adapter;
        }

        public virtual void CreateDatabaseFile(string cxString)
        {
            throw new NotSupportedException();
        }

        public virtual IDbConnection GetCx(string cxString)
        {
            DbConnection connection = this.GetProviderFactory().CreateConnection();
            connection.ConnectionString = cxString;
            return connection;
        }

        public static IDbConnection GetCx(string provider, string cxString)
        {
            return GetFactory(provider).GetCx(cxString);
        }

        public static DbFactory GetFactory(string provider)
        {
            if (provider == "System.Data.SqlServerCe.3.5")
            {
                return new SqlCE35Factory();
            }
            if (provider == "System.Data.SqlServerCe.4.0")
            {
                return new SqlCE40Factory();
            }
            if (string.IsNullOrEmpty(provider) || (provider == "System.Data.SqlClient"))
            {
                return new SqlServerFactory();
            }
            return new DbFactory(provider);
        }

        public virtual DbProviderFactory GetProviderFactory()
        {
            return DbProviderFactories.GetFactory(this._provider);
        }
    }
}

