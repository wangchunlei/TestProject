namespace LINQPad.Extensibility.DataContext
{
    using System;
    using System.Data;
    using System.Data.Common;

    public interface IDatabaseInfo
    {
        IDbConnection GetConnection();
        string GetCxString();
        string GetDatabaseDescription();
        DbProviderFactory GetProviderFactory();
        bool IsEquivalent(IDatabaseInfo other);

        bool AttachFile { get; set; }

        string AttachFileName { get; set; }

        string CustomCxString { get; set; }

        string Database { get; set; }

        string DbVersion { get; set; }

        bool EncryptCustomCxString { get; set; }

        bool EncryptTraffic { get; set; }

        bool IsSqlCE { get; }

        bool IsSqlServer { get; }

        int MaxDatabaseSize { get; set; }

        string Password { get; set; }

        string Provider { get; set; }

        string Server { get; set; }

        bool SqlSecurity { get; set; }

        bool UserInstance { get; set; }

        string UserName { get; set; }
    }
}

