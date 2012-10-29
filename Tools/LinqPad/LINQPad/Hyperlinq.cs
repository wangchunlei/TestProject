namespace LINQPad
{
    using LINQPad.ObjectGraph;
    using System;

    [MetaGraphNode]
    public class Hyperlinq
    {
        public readonly int? EditorColumn;
        public readonly int? EditorRow;
        public readonly string Query;
        public readonly LINQPad.QueryLanguage QueryLanguage;
        public readonly string Text;
        public readonly string Uri;

        public Hyperlinq(string uri) : this(uri, uri)
        {
        }

        public Hyperlinq(LINQPad.QueryLanguage queryLanguage, string query) : this(queryLanguage, query, query)
        {
        }

        public Hyperlinq(string uri, string text)
        {
            if (uri.StartsWith("www."))
            {
                uri = "http://" + uri;
            }
            this.Uri = uri;
            this.Text = text;
        }

        public Hyperlinq(LINQPad.QueryLanguage queryLanguage, string query, string text)
        {
            this.QueryLanguage = queryLanguage;
            this.Query = query;
            this.Text = text;
            this.Uri = "";
        }

        internal Hyperlinq(int editorRow, int editorColumn, string text)
        {
            this.EditorRow = new int?(editorRow);
            this.EditorColumn = new int?(editorColumn);
            this.Text = text;
            this.Uri = "";
        }

        public bool IsValid
        {
            get
            {
                if ((this.Query != null) || this.EditorRow.HasValue)
                {
                    return true;
                }
                if (this.Uri == null)
                {
                    return false;
                }
                return (System.Uri.IsWellFormedUriString(this.Uri.Replace("|", "+"), UriKind.Absolute) || this.Uri.ToLowerInvariant().Contains("javascript"));
            }
        }
    }
}

