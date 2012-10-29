namespace LINQPad.Schema
{
    using LINQPad.Extensibility.DataContext.DbSchema;
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Xml.Linq;

    internal class SqlSchemaReader
    {
        private static readonly Type _geographyType;
        private static readonly Type _geometryType;
        private static readonly Type _hierarchyIdType;
        private static readonly Dictionary<string, DbTypeInfo> ByName = new Dictionary<string, DbTypeInfo>(StringComparer.OrdinalIgnoreCase);

        static SqlSchemaReader()
        {
            DbTypeInfo[] infoArray2 = new DbTypeInfo[] { 
                new DbTypeInfo("BigInt", typeof(long)), new DbTypeInfo("Binary", typeof(Binary), true), new DbTypeInfo("Bit", typeof(bool)), new DbTypeInfo("Char", typeof(string), true), new DbTypeInfo("DateTime", typeof(DateTime)), new DbTypeInfo("DateTime2", typeof(DateTime)), new DbTypeInfo("DateTimeOffset", typeof(DateTimeOffset)), new DbTypeInfo("Date", typeof(DateTime)), new DbTypeInfo("Time", typeof(TimeSpan)), new DbTypeInfo("Decimal", typeof(decimal), true, true), new DbTypeInfo("Float", typeof(double)), new DbTypeInfo("Image", typeof(Binary)), new DbTypeInfo("Int", typeof(int)), new DbTypeInfo("Money", typeof(decimal)), new DbTypeInfo("NChar", typeof(string), true), new DbTypeInfo("NText", typeof(string)), 
                new DbTypeInfo("Numeric", typeof(decimal), true, true), new DbTypeInfo("NVarchar", typeof(string), true), new DbTypeInfo("Real", typeof(float)), new DbTypeInfo("RowVersion", typeof(Binary)), new DbTypeInfo("SmallDateTime", typeof(DateTime)), new DbTypeInfo("SmallInt", typeof(short)), new DbTypeInfo("SmallMoney", typeof(decimal)), new DbTypeInfo("Sql_Variant", typeof(object)), new DbTypeInfo("Text", typeof(string)), new DbTypeInfo("Timestamp", typeof(Binary)), new DbTypeInfo("TinyInt", typeof(byte)), new DbTypeInfo("UniqueIdentifier", typeof(Guid)), new DbTypeInfo("VarBinary", typeof(Binary), true), new DbTypeInfo("VarChar", typeof(string), true), new DbTypeInfo("Xml", typeof(XElement))
             };
            foreach (DbTypeInfo info in infoArray2)
            {
                ByName.Add(info.Name, info);
            }
            try
            {
                _geometryType = Type.GetType("Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                if (_geometryType != null)
                {
                    ByName.Add("Geometry", new DbTypeInfo("Geometry", _geometryType));
                }
            }
            catch
            {
            }
            try
            {
                _geographyType = Type.GetType("Microsoft.SqlServer.Types.SqlGeography, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                if (_geographyType != null)
                {
                    ByName.Add("Geography", new DbTypeInfo("Geography", _geographyType));
                }
            }
            catch
            {
            }
            try
            {
                _hierarchyIdType = Type.GetType("Microsoft.SqlServer.Types.SqlHierarchyId, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                if (_hierarchyIdType != null)
                {
                    ByName.Add("HierarchyId", new DbTypeInfo("HierarchyId", _hierarchyIdType));
                }
            }
            catch
            {
            }
        }

        public static DbTypeInfo GetDbTypeInfo(string sqlTypeName)
        {
            if (!ByName.ContainsKey(sqlTypeName))
            {
                return null;
            }
            return ByName[sqlTypeName];
        }
    }
}

