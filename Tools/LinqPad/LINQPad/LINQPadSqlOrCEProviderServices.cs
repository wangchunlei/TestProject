namespace LINQPad
{
    using System;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Reflection;

    internal class LINQPadSqlOrCEProviderServices : DbProviderServices
    {
        private DbProviderServices _proxy;

        public LINQPadSqlOrCEProviderServices(DbConnection prototypeCx)
        {
            this._proxy = DbProviderServices.GetProviderServices(prototypeCx);
        }

        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            DbCommand proxy = (DbCommand) this._proxy.GetType().GetMethod("CreateCommand", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(DbProviderManifest), typeof(DbCommandTree) }, null).Invoke(this._proxy, new object[] { providerManifest, commandTree });
            proxy = new LINQPadDbCommand(proxy);
            return this.CreateCommandDefinition(proxy);
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return (DbProviderManifest) this._proxy.GetType().GetMethod("GetDbProviderManifest", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string) }, null).Invoke(this._proxy, new object[] { manifestToken });
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            MethodInfo info = this._proxy.GetType().GetMethod("GetDbProviderManifestToken", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(DbConnection) }, null);
            if (connection is LINQPadDbConnection)
            {
                connection = ((LINQPadDbConnection) connection).Proxy;
            }
            return (string) info.Invoke(this._proxy, new object[] { connection });
        }
    }
}

