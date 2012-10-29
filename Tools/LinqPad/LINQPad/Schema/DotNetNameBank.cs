namespace LINQPad.Schema
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal class DotNetNameBank
    {
        private HashSet<string> _dotNetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _sqlNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static Regex _translater = new Regex("[`!@#$%^&*()+{}\\\\|;:'\",./<>? \\[\\]=]");
        private Dictionary<string, string> _translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public bool ContainsDotNetName(string name)
        {
            return this._dotNetNames.Contains(name);
        }

        public string GetUniqueDotNetName(string sqlName)
        {
            string str = null;
            if (!this._translations.TryGetValue(sqlName, out str))
            {
                str = this.TranslateName(sqlName);
                this._translations.Add(sqlName, str);
            }
            return str;
        }

        public void RegisterName(string sqlName)
        {
            this._sqlNames.Add(sqlName);
        }

        public static string ToDotNetName(string sqlName)
        {
            string s = _translater.Replace(sqlName.Replace('-', '_'), "");
            if (s.Length == 0)
            {
                return null;
            }
            if (char.IsDigit(s, 0))
            {
                s = "_" + s;
            }
            return s;
        }

        private string TranslateName(string sqlName)
        {
            string str = ToDotNetName(sqlName);
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            if ((str != sqlName) && this._sqlNames.Contains(str))
            {
                return null;
            }
            if (this._dotNetNames.Contains(str))
            {
                return null;
            }
            this._dotNetNames.Add(str);
            return str;
        }
    }
}

