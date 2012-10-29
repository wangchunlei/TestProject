namespace LINQPad
{
    using System;
    using System.Data.Common;
    using System.Reflection;

    internal class SqlCE35Factory : DbFactory
    {
        public SqlCE35Factory() : base("System.Data.SqlServerCe.3.5")
        {
        }

        public override void CreateDatabaseFile(string cxString)
        {
            try
            {
                using (DbConnection connection = this.GetProviderFactory().CreateConnection())
                {
                    Type type = connection.GetType().Assembly.GetType("System.Data.SqlServerCe.SqlCeEngine");
                    object obj2 = Activator.CreateInstance(type, new object[] { cxString });
                    type.GetMethod("CreateDatabase").Invoke(obj2, null);
                }
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
        }

        public override DbProviderFactory GetProviderFactory()
        {
            DbProviderFactory providerFactory;
            try
            {
                providerFactory = base.GetProviderFactory();
            }
            catch (Exception exception)
            {
                Log.Write(exception);
                throw new SqlCeNotInstalledException("Unable to load the SQL CE 3.5 provider: " + exception.Message, exception);
            }
            return providerFactory;
        }
    }
}

