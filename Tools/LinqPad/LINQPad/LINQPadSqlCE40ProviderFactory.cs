namespace LINQPad
{
    using System;
    using System.Data.Common;

    internal class LINQPadSqlCE40ProviderFactory : LINQPadDbProviderFactory
    {
        private static DbProviderFactory _originalFactory;
        public static readonly LINQPadSqlCE40ProviderFactory Instance = new LINQPadSqlCE40ProviderFactory();

        public LINQPadSqlCE40ProviderFactory() : base(OriginalFactory)
        {
        }

        public static void SaveOriginalFactory()
        {
            if (_originalFactory == null)
            {
                try
                {
                    _originalFactory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
                }
                catch
                {
                }
            }
        }

        public static DbProviderFactory OriginalFactory
        {
            get
            {
                SaveOriginalFactory();
                return _originalFactory;
            }
        }
    }
}

