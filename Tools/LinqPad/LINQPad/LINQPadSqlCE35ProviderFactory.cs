namespace LINQPad
{
    using System;
    using System.Data.Common;

    internal class LINQPadSqlCE35ProviderFactory : LINQPadDbProviderFactory
    {
        private static DbProviderFactory _originalFactory;
        public static readonly LINQPadSqlCE35ProviderFactory Instance = new LINQPadSqlCE35ProviderFactory();

        public LINQPadSqlCE35ProviderFactory() : base(OriginalFactory)
        {
        }

        public static void SaveOriginalFactory()
        {
            if (_originalFactory == null)
            {
                try
                {
                    _originalFactory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.3.5");
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

