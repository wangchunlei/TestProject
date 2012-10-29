namespace LINQPad
{
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal static class StringUtil
    {
        public static string ConvertSpacesToTabs(string text)
        {
            int tabSize = UserOptions.Instance.TabSizeActual;
            return Regex.Replace(text, "^[ \x00a0]{" + tabSize + ",}", (MatchEvaluator) (m => ("".PadRight(m.Length / tabSize, '\t') + "".PadRight(m.Length % tabSize))), RegexOptions.Multiline);
        }

        public static string ConvertTabsToSpaces(string text)
        {
            int tabSize = UserOptions.Instance.TabSizeActual;
            return Regex.Replace(text, "^\t+", (MatchEvaluator) (m => "".PadRight(m.Length * tabSize)), RegexOptions.Multiline);
        }

        public static string EscapeStringForLanguage(string input, QueryLanguage language)
        {
            CodeDomProvider provider;
            StringWriter writer = new StringWriter();
            if (language <= QueryLanguage.Program)
            {
                provider = new CSharpCodeProvider();
            }
            else
            {
                provider = new VBCodeProvider();
            }
            provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
            return writer.GetStringBuilder().ToString();
        }

        public static string GetDistinguishingIdentifier(string name, string parentTableName, string parentKeyName, bool stripStem)
        {
            name = StripTrailingKey(name, parentKeyName, stripStem);
            parentTableName = parentTableName.ToLowerInvariant();
            if (name.ToLowerInvariant().StartsWith(parentTableName))
            {
                name = name.Substring(parentTableName.Length);
                if (name.StartsWith("_"))
                {
                    name = name.Substring(1);
                }
                return name;
            }
            return StripTrailingKey(name, parentTableName, stripStem);
        }

        public static string GetIndent()
        {
            if (UserOptions.Instance.ConvertTabsToSpaces)
            {
                return "".PadRight(UserOptions.Instance.TabSizeActual, ' ');
            }
            return "\t";
        }

        public static string GetPluralName(string name)
        {
            int index = name.IndexOf("Of", StringComparison.Ordinal);
            if ((((index > 3) && (index < (name.Length - 4))) && (char.IsLower(name, index - 1) && (name[index - 1] != 's'))) && char.IsUpper(name, index + 2))
            {
                return (name.Substring(0, index) + "s" + name.Substring(index));
            }
            string str2 = "";
            string str3 = name.ToLowerInvariant();
            if (str3.EndsWith("created", StringComparison.Ordinal) || name.EndsWith("updated", StringComparison.Ordinal))
            {
                if (name.Length < 10)
                {
                    return name;
                }
                string str4 = str3.Substring(name.Length - 9, 2);
                string str5 = str3.Substring(name.Length - 10, 3);
                switch (str4)
                {
                    case "co":
                    case "re":
                    case "un":
                        return name;
                }
                switch (str5)
                {
                    case "mis":
                    case "pro":
                        return name;
                }
                str2 = name.Substring(name.Length - 7);
                name = name.Substring(0, name.Length - 7);
            }
            str3 = name.ToLowerInvariant();
            if ((((!str3.EndsWith("information") && !str3.EndsWith("complete")) && (!str3.EndsWith("_info") && !str3.EndsWith("_data"))) && (!name.EndsWith("Info") && !name.EndsWith("Data"))) && !name.EndsWith("Staff"))
            {
                if (((str3.EndsWith("x") || str3.EndsWith("ch")) || str3.EndsWith("ss")) || str3.EndsWith("status"))
                {
                    name = name + "es";
                }
                else if (!((!str3.EndsWith("y") || (str3.Length <= 1)) || IsVowel(str3[str3.Length - 2])))
                {
                    name = name.Substring(0, name.Length - 1) + "ies";
                }
                else if (!str3.EndsWith("s"))
                {
                    name = name + "s";
                }
            }
            return (name + str2);
        }

        public static string GetSingularParentName(string s, string childPropName)
        {
            if (s.EndsWith("s", StringComparison.Ordinal))
            {
                string str3;
                if (GetPluralName(childPropName).ToLowerInvariant() == s.ToLowerInvariant())
                {
                    return childPropName;
                }
                if ((childPropName.Length < s.Length) || (childPropName.Length > (s.Length + 5)))
                {
                    return null;
                }
                string str2 = childPropName.ToLowerInvariant();
                if (str2.EndsWith("_key"))
                {
                    str3 = childPropName.Substring(0, childPropName.Length - 4);
                }
                else if (str2.EndsWith("_id") || str2.EndsWith("key"))
                {
                    str3 = childPropName.Substring(0, childPropName.Length - 3);
                }
                else
                {
                    if (!str2.EndsWith("id"))
                    {
                        return null;
                    }
                    str3 = childPropName.Substring(0, childPropName.Length - 2);
                }
                if (str3.Length >= 2)
                {
                    string str4 = GetPluralName(str3).ToLowerInvariant();
                    if ((str4 != str3.ToLowerInvariant()) && (str4 == s.ToLowerInvariant()))
                    {
                        return str3;
                    }
                }
            }
            return null;
        }

        private static bool IsVowel(char c)
        {
            return "aeiuo".Contains<char>(char.ToLowerInvariant(c));
        }

        public static string LimitLength(string text, int limit)
        {
            if (((text == null) || (text.Length <= limit)) || (limit < 5))
            {
                return text;
            }
            return (text.Substring(0, limit - 3) + "...");
        }

        public static string Pascal(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return (char.ToUpper(input[0]) + input.Substring(1));
        }

        public static string StripTrailingKey(string s, string key, bool stripStem)
        {
            string str = s.ToLowerInvariant();
            if ((stripStem && (key != null)) && str.EndsWith(key.ToLowerInvariant()))
            {
                s = s.Substring(0, s.Length - key.Length);
            }
            else if (str.EndsWith("_ref"))
            {
                s = s.Substring(0, s.Length - 4);
            }
            else if (((str.EndsWith("_id") || str.EndsWith("_key")) || s.EndsWith("Key")) || s.EndsWith("Ref"))
            {
                s = s.Substring(0, s.Length - 3);
            }
            else
            {
                if ((!s.EndsWith("ID") || (s.Length <= 4)) || !char.IsLower(s, s.Length - 3))
                {
                    return s;
                }
                s = s.Substring(0, s.Length - 2);
            }
            while (s.EndsWith("_"))
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s;
        }

        public static string StripTrailingWord(string s, int wordLen)
        {
            if ((s == null) || (s.Length < (wordLen + 2)))
            {
                return s;
            }
            return s.Substring(0, s.Length - ((s[(s.Length - wordLen) - 1] == '_') ? (wordLen + 1) : wordLen));
        }
    }
}

