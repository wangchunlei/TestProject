namespace LINQPad
{
    using LINQPad.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class HtmlILStyler : ILStyler
    {
        private bool _createAnchors;
        private static Dictionary<string, string> _opCodeDocLookup;

        public HtmlILStyler(bool createAnchors)
        {
            this._createAnchors = createAnchors;
        }

        private static void PopulateOpcodeDescriptions()
        {
            try
            {
                _opCodeDocLookup = (from line in Resources.OpCodes.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries) select line.Split(new char[] { '\t' })).ToDictionary<string[], string, string>(spl => spl[0], spl => spl[1], StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                _opCodeDocLookup = new Dictionary<string, string>();
            }
        }

        public override string StyleIdentifier(string s)
        {
            bool flag = false;
            string str = "UserQuery.<LINQPad.RunUserAuthoredQuery>";
            if (s.StartsWith(str, StringComparison.Ordinal))
            {
                flag = true;
                s = s.Substring(str.Length);
            }
            str = "<LINQPad.RunUserAuthoredQuery>";
            if (s.StartsWith(str, StringComparison.Ordinal))
            {
                flag = true;
                s = s.Substring(str.Length);
            }
            str = "UserQuery.<RunUserAuthoredQuery>";
            if (s.StartsWith(str, StringComparison.Ordinal))
            {
                flag = true;
                s = s.Substring(str.Length);
            }
            str = "<RunUserAuthoredQuery>";
            if (s.StartsWith(str, StringComparison.Ordinal))
            {
                flag = true;
                s = s.Substring(str.Length);
            }
            string str2 = "<span class='ident'>";
            if (flag)
            {
                if (this._createAnchors)
                {
                    str2 = str2 + "<a name='" + s + "' />";
                }
                else
                {
                    str2 = str2 + "<a href='#" + s + "'>";
                }
            }
            str2 = str2 + WebHelper.HtmlEncode(s);
            if (!(!flag || this._createAnchors))
            {
                str2 = str2 + "</a>";
            }
            return (str2 + "</span>");
        }

        public override string StyleOpCode(string s)
        {
            if (_opCodeDocLookup == null)
            {
                PopulateOpcodeDescriptions();
            }
            string str = "";
            _opCodeDocLookup.TryGetValue(s.Trim().Replace('.', '_'), out str);
            str = WebHelper.HtmlEncode(str ?? "");
            return ("<span class='opcode' title=\"" + str + "\">" + s + "</span>");
        }

        public override string StyleString(string s)
        {
            return ("<span class='string'>" + WebHelper.HtmlEncode(s) + "</span>");
        }
    }
}

