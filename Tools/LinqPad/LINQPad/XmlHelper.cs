namespace LINQPad
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal class XmlHelper
    {
        public static object FromXmlString(string s, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return s;
            }
            Type enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if ((enumType != targetType) && string.IsNullOrEmpty(s))
            {
                return null;
            }
            if (enumType.IsEnum)
            {
                return Enum.Parse(enumType, s);
            }
            if (targetType.Namespace != "System")
            {
                throw new InvalidOperationException("Unknown XML type");
            }
            switch (enumType.Name)
            {
                case "Boolean":
                    return XmlConvert.ToBoolean(s);

                case "Char":
                    return XmlConvert.ToChar(s);

                case "Decimal":
                    return XmlConvert.ToDecimal(s);

                case "SByte":
                    return XmlConvert.ToSByte(s);

                case "Int16":
                    return XmlConvert.ToInt16(s);

                case "Int32":
                    return XmlConvert.ToInt32(s);

                case "Int64":
                    return XmlConvert.ToInt64(s);

                case "Byte":
                    return XmlConvert.ToByte(s);

                case "UInt16":
                    return XmlConvert.ToUInt16(s);

                case "UInt32":
                    return XmlConvert.ToUInt32(s);

                case "UInt64":
                    return XmlConvert.ToUInt64(s);

                case "Single":
                    return XmlConvert.ToSingle(s);

                case "Double":
                    return XmlConvert.ToDouble(s);

                case "TimeSpan":
                    return XmlConvert.ToTimeSpan(s);

                case "DateTime":
                    return XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Unspecified);

                case "DateTimeOffset":
                    return XmlConvert.ToDateTimeOffset(s);

                case "Guid":
                    return XmlConvert.ToGuid(s);
            }
            throw new InvalidOperationException("Unknown XML type");
        }

        public static string ToFormattedString(XmlNode node)
        {
            string outerXml;
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    XmlWriterSettings settings = new XmlWriterSettings {
                        Indent = true,
                        IndentChars = "   ",
                        ConformanceLevel = ConformanceLevel.Fragment
                    };
                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
                    {
                        node.WriteTo(writer);
                    }
                    stream.Position = 0L;
                    outerXml = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch
            {
                outerXml = node.OuterXml;
            }
            return outerXml;
        }
    }
}

