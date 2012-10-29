namespace LINQPad.UI
{
    using System;

    internal class NewQueryArgs : EventArgs
    {
        private string _queryText;
        private Func<string> _queryTextFunc;
        public bool IntoGrids;
        public QueryLanguage? Language;
        public bool ListMembers;
        public string QueryName;
        public bool RunNow;
        public bool ShowParams;

        public NewQueryArgs() : this("", null)
        {
        }

        public NewQueryArgs(Func<string> queryTextFunc)
        {
            this._queryTextFunc = queryTextFunc;
        }

        public NewQueryArgs(string queryText, QueryLanguage? language) : this(queryText, language, false)
        {
        }

        public NewQueryArgs(string queryText, QueryLanguage? language, bool runNow)
        {
            this._queryText = queryText;
            this.Language = language;
            this.RunNow = runNow;
        }

        public string QueryText
        {
            get
            {
                if (this._queryText == null)
                {
                    if (this._queryTextFunc != null)
                    {
                        this._queryText = this._queryTextFunc();
                    }
                    if (this._queryText == null)
                    {
                        this._queryText = "";
                    }
                }
                return this._queryText;
            }
        }
    }
}

