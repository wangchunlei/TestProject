namespace LINQPad
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    internal static class SqlParameterFormatter
    {
        public static string GetDeclaration(SqlParameter p)
        {
            string str = (p.SqlDbType == SqlDbType.Text) ? "VarChar" : ((p.SqlDbType == SqlDbType.NText) ? "NVarChar" : ((p.SqlDbType == SqlDbType.Image) ? "VarBinary" : p.SqlDbType.ToString()));
            string str2 = "DECLARE " + GetLegalParamName(p) + " " + str;
            switch (p.SqlDbType)
            {
                case SqlDbType.Binary:
                case SqlDbType.Char:
                case SqlDbType.Image:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                    return string.Concat(new object[] { str2, "(", Math.Min(0x3e8, Math.Max(1, p.Size)), ")" });

                case SqlDbType.Bit:
                case SqlDbType.DateTime:
                case SqlDbType.Float:
                case SqlDbType.Int:
                case SqlDbType.Money:
                    return str2;

                case SqlDbType.Decimal:
                    if ((p.Value != null) && !(p.Value is DBNull))
                    {
                        string str4 = p.Value.ToString();
                        return string.Concat(new object[] { str2, "(", str4.Length + p.Scale, ",", p.Scale, ")" });
                    }
                    return (str2 + "(1,0)");

                case SqlDbType.Timestamp:
                case SqlDbType.TinyInt:
                    return str2;
            }
            return str2;
        }

        public static string GetInitializer(SqlParameter p, bool sql2008)
        {
            string declaration = GetDeclaration(p);
            if ((p.Direction != ParameterDirection.Input) && (p.Direction != ParameterDirection.InputOutput))
            {
                return declaration;
            }
            if (!sql2008)
            {
                declaration = declaration + " SET " + GetLegalParamName(p);
            }
            return (declaration + " = " + GetNativeValue(p));
        }

        private static string GetLegalParamName(SqlParameter p)
        {
            if (p.ParameterName.StartsWith("@"))
            {
                return p.ParameterName;
            }
            return ("@" + p.ParameterName);
        }

        public static string GetNativeValue(SqlParameter p)
        {
            if ((p.Value == null) || (p.Value is DBNull))
            {
                return "null";
            }
            object obj2 = p.Value;
            if (obj2.GetType().IsNumeric())
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { obj2 });
            }
            if (obj2 is bool)
            {
                return (((bool) obj2) ? "1" : "0");
            }
            if ((obj2 is DateTime) || (obj2 is DateTimeOffset))
            {
                string str2 = "yyyy-MM-dd";
                if (p.SqlDbType != SqlDbType.Date)
                {
                    str2 = str2 + " HH:mm:ss";
                    if (p.SqlDbType != SqlDbType.SmallDateTime)
                    {
                        str2 = str2 + ".fff";
                        if ((p.SqlDbType == SqlDbType.DateTime2) || (p.SqlDbType == SqlDbType.DateTimeOffset))
                        {
                            str2 = str2 + "ffff";
                        }
                    }
                }
                if (obj2 is DateTimeOffset)
                {
                    str2 = str2 + "zzz";
                }
                return ("'" + string.Format(CultureInfo.InvariantCulture, "{0:" + str2 + "}", new object[] { obj2 }) + "'");
            }
            if (obj2 is TimeSpan)
            {
                return ("'" + string.Format(CultureInfo.InvariantCulture, "{0}'", new object[] { obj2 }));
            }
            if (!(obj2 is byte[]))
            {
                if (obj2 is XElement)
                {
                    obj2 = ((XElement) obj2).ToString(SaveOptions.DisableFormatting);
                }
                if ((obj2 is char) || (obj2 is string))
                {
                    bool flag;
                    string source = obj2.ToString();
                    int length = source.Length;
                    if (flag = length > 0x3e8)
                    {
                        source = source.Substring(0, 0x3e5) + "...";
                    }
                    source = "'" + source.Replace("'", "''") + "'";
                    if (flag)
                    {
                        object obj3 = source;
                        source = string.Concat(new object[] { obj3, " -- (first 1000 characters/", length, " shown)" });
                    }
                    if (source.Any<char>(c => (c < '\x001f') || (c > '\x007f')))
                    {
                        source = "N" + source;
                    }
                    return source;
                }
                if (obj2 is Guid)
                {
                    return ("'" + obj2.ToString() + "'");
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { obj2 });
            }
            byte[] buffer = (byte[]) obj2;
            char[] chArray = new char[buffer.Length * 2];
            int num = 0;
            foreach (byte num3 in buffer)
            {
                string str3 = num3.ToString("X2");
                chArray[num++] = str3[0];
                chArray[num++] = str3[1];
                if (num >= 500)
                {
                    return string.Concat(new object[] { "0x", new string(chArray), " -- (first 500 bytes/", buffer.Length, " shown)" });
                }
            }
            return ("0x" + new string(chArray));
        }
    }
}

