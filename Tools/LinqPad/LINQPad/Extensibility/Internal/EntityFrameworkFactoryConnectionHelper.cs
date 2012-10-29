namespace LINQPad.Extensibility.Internal
{
    using LINQPad;
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;

    public static class EntityFrameworkFactoryConnectionHelper
    {
        public static DbConnection CreateFactoryConnection(string nameOrConnectionString, object innerFactory)
        {
            DbConnection proxy = innerFactory.GetType().GetInterface("System.Data.Entity.Infrastructure.IDbConnectionFactory").GetMethod("CreateConnection", new Type[] { typeof(string) }).Invoke(innerFactory, new object[] { nameOrConnectionString }) as DbConnection;
            if (!(((!(proxy is SqlConnection) || !LINQPadDbController.IsSqlPatched) && ((!(proxy.GetType().Namespace == "System.Data.SqlServerCe") || (proxy.GetType().Assembly.GetName().Version.Major != 3)) || !LINQPadDbController.IsSqlCE35Patched)) ? ((!(proxy.GetType().Namespace == "System.Data.SqlServerCe") || (proxy.GetType().Assembly.GetName().Version.Major != 4)) || !LINQPadDbController.IsSqlCE40Patched) : false))
            {
                return new LINQPadDbConnection(proxy);
            }
            return proxy;
        }
    }
}

