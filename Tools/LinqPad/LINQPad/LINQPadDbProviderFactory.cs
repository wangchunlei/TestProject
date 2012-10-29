namespace LINQPad
{
    using System;
    using System.Data.Common;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    internal abstract class LINQPadDbProviderFactory : DbProviderFactory, IServiceProvider
    {
        private readonly DbProviderFactory _proxy;

        public LINQPadDbProviderFactory(DbProviderFactory proxy)
        {
            this._proxy = proxy;
        }

        public override DbCommand CreateCommand()
        {
            return new LINQPadDbCommand(this._proxy.CreateCommand());
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return this._proxy.CreateCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            return new LINQPadDbConnection(this._proxy.CreateConnection());
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return this._proxy.CreateConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return this._proxy.CreateDataAdapter();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return this._proxy.CreateDataSourceEnumerator();
        }

        public override DbParameter CreateParameter()
        {
            return this._proxy.CreateParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return this._proxy.CreatePermission(state);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if ((serviceType != null) && (serviceType.FullName == "System.Data.Common.DbProviderServices"))
            {
                return (LINQPadSqlOrCEProviderServices) Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType("LINQPad.LINQPadSqlOrCEProviderServices"), new object[] { this._proxy.CreateConnection() });
            }
            IServiceProvider provider = this._proxy as IServiceProvider;
            if (provider != null)
            {
                return provider.GetService(serviceType);
            }
            return null;
        }

        public override bool CanCreateDataSourceEnumerator
        {
            get
            {
                return this._proxy.CanCreateDataSourceEnumerator;
            }
        }
    }
}

