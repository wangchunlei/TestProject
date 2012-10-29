namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using System;
    using System.IO;
    using System.Linq;

    internal static class MyExtensions
    {
        private static string[] _additionalRefs;
        private static QueryCore _query;

        public static void UpdateAdditionalRefs()
        {
            if (File.Exists(QueryFilePath))
            {
                try
                {
                    QueryCore q = new QueryCore();
                    q.Open(QueryFilePath);
                    UpdateAdditionalRefs(q);
                    return;
                }
                catch
                {
                }
            }
            _additionalRefs = new string[0];
        }

        public static void UpdateAdditionalRefs(QueryCore q)
        {
            try
            {
                _query = q;
                _additionalRefs = q.AllFileReferences.Union<string>((from r in q.AdditionalGACReferences
                    select GacResolver.FindPath(r) into r
                    where r != null
                    select r)).ToArray<string>();
            }
            catch
            {
                _additionalRefs = "".Split(new char[0]);
            }
        }

        public static string[] AdditionalRefs
        {
            get
            {
                if (_additionalRefs == null)
                {
                    UpdateAdditionalRefs();
                }
                return _additionalRefs;
            }
        }

        public static QueryCore Query
        {
            get
            {
                if (_additionalRefs == null)
                {
                    UpdateAdditionalRefs();
                }
                return _query;
            }
        }

        public static string QueryFilePath
        {
            get
            {
                string str = "40";
                return Path.Combine(UserOptions.Instance.GetPluginsFolder(false), "MyExtensions.FW" + str + ".linq");
            }
        }
    }
}

