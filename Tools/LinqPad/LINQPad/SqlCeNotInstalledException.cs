namespace LINQPad
{
    using System;

    [Serializable]
    internal class SqlCeNotInstalledException : Exception
    {
        public SqlCeNotInstalledException(string msg) : base(msg)
        {
        }

        public SqlCeNotInstalledException(string msg, Exception innerEx) : base(msg, innerEx)
        {
        }
    }
}

