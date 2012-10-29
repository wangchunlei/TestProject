namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class TypeUtil
    {
        public static string FormatTypeName(this Type t)
        {
            return t.FormatTypeName(false);
        }

        public static string FormatTypeName(this Type t, bool fullname)
        {
            return FormatTypeName(t, fullname, false, 0, new HashSet<Type>());
        }

        private static string FormatTypeName(Type t, bool fullname, bool emptyAnon, int nestLevel, HashSet<Type> visitedTypes)
        {
            if (t == null)
            {
                return "";
            }
            if (t.IsByRef)
            {
                t = t.GetElementType();
            }
            if (!t.IsPublic)
            {
                Type type = (from i in t.GetInterfaces()
                    where i.IsPublic
                    where i.Namespace.StartsWith("System.Collections", StringComparison.Ordinal) || i.Namespace.StartsWith("System.Linq", StringComparison.Ordinal)
                    select i).OrderByDescending<Type, Type>(itype => itype, new SubTypeComparer()).FirstOrDefault<Type>();
                if (type != null)
                {
                    t = type;
                }
            }
            if (((!t.IsGenericType && !t.IsArray) || (nestLevel > 5)) || visitedTypes.Contains(t))
            {
                return (fullname ? t.FullName : t.Name);
            }
            visitedTypes.Add(t);
            StringBuilder builder = new StringBuilder();
            string str2 = "";
            string str3 = "";
            string str4 = "";
            if (t.IsGenericType)
            {
                if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    str4 = "?";
                }
                else
                {
                    str2 = t.GetGenericTypeDefinition().Name.Split(new char[] { '`' })[0];
                    if (str2.Contains("<"))
                    {
                        if (!fullname)
                        {
                            return (emptyAnon ? "" : "\x00f8");
                        }
                        str2 = "";
                        str3 = "{";
                        str4 = "}";
                    }
                    else
                    {
                        str3 = "<";
                        str4 = ">";
                    }
                }
            }
            else
            {
                try
                {
                    str4 = "[".PadRight(t.GetArrayRank() - 1, ',') + "]";
                }
                catch
                {
                    return "";
                }
            }
            builder.Append(str2 + str3);
            if (t.IsGenericType)
            {
                bool flag = true;
                foreach (Type type2 in t.GetGenericArguments())
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(',');
                    }
                    builder.Append(FormatTypeName(type2, fullname, true, nestLevel + 1, visitedTypes));
                }
            }
            else
            {
                try
                {
                    builder.Append(FormatTypeName(t.GetElementType(), fullname, false, nestLevel + 1, visitedTypes));
                }
                catch
                {
                    return "";
                }
            }
            builder.Append(str4);
            return builder.ToString();
        }

        public static bool IsAnonymous(string typeName)
        {
            return ((typeName.Length > 5) && ((((typeName[0] != '<') || (typeName[1] != '>')) || (((typeName[5] != 'A') || (typeName[6] != 'n')) && (typeName.IndexOf("anon", StringComparison.OrdinalIgnoreCase) <= -1))) ? ((((typeName[0] == 'V') && (typeName[1] == 'B')) && ((typeName[2] == '$') && (typeName[3] == 'A'))) && (typeName[4] == 'n')) : true));
        }

        public static bool IsAnonymous(this Type type)
        {
            if (!(string.IsNullOrEmpty(type.Namespace) && type.IsGenericType))
            {
                return false;
            }
            return IsAnonymous(type.Name);
        }

        public static bool IsNumeric(this Type t)
        {
            if (t == null)
            {
                return false;
            }
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                t = t.GetGenericArguments()[0];
            }
            if (t == typeof(decimal))
            {
                return true;
            }
            if (!t.IsPrimitive)
            {
                return false;
            }
            return ((t != typeof(char)) && (t != typeof(bool)));
        }

        public static bool IsSimple(this Type t)
        {
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                t = t.GetGenericArguments()[0];
            }
            return (((t.IsPrimitive || (t == typeof(object))) || (Type.GetTypeCode(t) != TypeCode.Object)) || ((t == typeof(TimeSpan)) || typeof(IFormattable).IsAssignableFrom(t)));
        }
    }
}

