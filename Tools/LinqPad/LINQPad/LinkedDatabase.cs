namespace LINQPad
{
    using System;

    internal class LinkedDatabase : IComparable<LinkedDatabase>
    {
        public readonly string Database;
        public readonly string Server;

        public LinkedDatabase(string database) : this(null, database)
        {
        }

        public LinkedDatabase(string server, string database)
        {
            if (server != null)
            {
                server = server.Trim();
            }
            this.Server = (server == "") ? null : server;
            this.Database = database.Trim();
        }

        public int CompareTo(LinkedDatabase other)
        {
            int num = (this.Server ?? "").CompareTo(other.Server ?? "");
            if (num != 0)
            {
                return num;
            }
            return this.Database.CompareTo(other.Database);
        }

        public override bool Equals(object obj)
        {
            LinkedDatabase database = obj as LinkedDatabase;
            if (database == null)
            {
                return false;
            }
            return (string.Equals(this.Server, database.Server, StringComparison.InvariantCultureIgnoreCase) && string.Equals(this.Database, database.Database, StringComparison.InvariantCultureIgnoreCase));
        }

        public override int GetHashCode()
        {
            return ((this.Server ?? "").ToLowerInvariant() + this.Database.ToLowerInvariant()).GetHashCode();
        }

        public override string ToString()
        {
            if (this.Server == null)
            {
                return this.Database;
            }
            return (this.Server + "." + this.Database);
        }

        public string QualifiedPrefix
        {
            get
            {
                string str = '[' + this.Database + ']';
                if (this.Server == null)
                {
                    return str;
                }
                return string.Concat(new object[] { '[', this.Server, "].", str });
            }
        }
    }
}

