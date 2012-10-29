namespace LINQPad
{
    using System;
    using System.Data.Common;

    internal class LINQPadSqlClientProviderFactory : LINQPadDbProviderFactory
    {
        private static DbProviderFactory _originalFactory;
        public static readonly LINQPadSqlClientProviderFactory Instance = new LINQPadSqlClientProviderFactory();

        public LINQPadSqlClientProviderFactory() : base(OriginalFactory)
        {
        }

        public static void SaveOriginalFactory()
        {
            if (_originalFactory == null)
            {
                try
                {
                    _originalFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
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

