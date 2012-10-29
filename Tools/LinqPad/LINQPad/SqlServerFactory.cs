namespace LINQPad
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    internal class SqlServerFactory : DbFactory
    {
        public SqlServerFactory() : base("System.Data.SqlClient")
        {
        }

        public override DbDataAdapter CreateDataAdapter(string cmdText, IDbConnection cx)
        {
            return new SqlDataAdapter(cmdText, (SqlConnection) cx);
        }

        public override IDbConnection GetCx(string cxString)
        {
            return new SqlConnection(cxString);
        }
    }
}

