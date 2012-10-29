namespace LINQPad
{
    using System;

    internal class ProviderNames
    {
        public const string SqlCe35 = "System.Data.SqlServerCe.3.5";
        public const string SqlCe40 = "System.Data.SqlServerCe.4.0";
        public const string SqlClient = "System.Data.SqlClient";

        public static bool IsSqlCE(string name)
        {
            return name.StartsWith("System.Data.SqlServerCe", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsSqlCE35(string name)
        {
            return string.Equals(name, "System.Data.SqlServerCe.3.5", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsSqlCE40(string name)
        {
            return string.Equals(name, "System.Data.SqlServerCe.4.0", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsSqlServer(string name)
        {
            return string.Equals(name, "System.Data.SqlClient", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

