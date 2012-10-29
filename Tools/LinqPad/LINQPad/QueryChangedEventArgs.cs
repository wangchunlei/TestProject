namespace LINQPad
{
    using System;

    internal class QueryChangedEventArgs : EventArgs
    {
        public bool DbChanged;
        public bool NamespacesChanged;
        public bool ReferencesChanged;
        public static QueryChangedEventArgs Refresh;
        public bool SourceChanged;

        static QueryChangedEventArgs()
        {
            QueryChangedEventArgs args = new QueryChangedEventArgs {
                SourceChanged = true,
                DbChanged = true,
                ReferencesChanged = true,
                NamespacesChanged = true
            };
            Refresh = args;
        }
    }
}

