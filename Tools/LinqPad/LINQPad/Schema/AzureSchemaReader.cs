namespace LINQPad.Schema
{
    using LINQPad.Extensibility.DataContext.DbSchema;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    internal class AzureSchemaReader : SqlServerSchemaReader
    {
        protected override string GetColumnSql()
        {
            if (base.SystemSchema == null)
            {
                return base.GetColumnSql();
            }
            string str2 = base.Repository.ExcludeRoutines ? "'U', 'V'" : "'U', 'V', 'TF', 'IF', 'FN', 'FS', 'FT', 'P'";
            return ("select \r\n\to.object_id TableID, \r\n\tschema_name (o.schema_id) SchemaName, \r\n\to.name TableName,\r\n\to.type,\r\n\tc.column_id,\r\n\tc.column_id colorder,\r\n\tc.name ColName,\r\n\tc.user_type_id,\r\n\tc.is_nullable,\r\n\tc.max_length,\r\n\tc.precision,\r\n\tc.scale,\r\n\tc.is_identity,\r\n\tc.is_computed\r\nfrom sys.all_objects o\r\nleft join sys.all_columns c on o.object_id = c.object_id and (o.type not in ('TF', 'IF', 'FN', 'FS', 'FT', 'P') or c.name not like '@%')\r\nwhere o.type in (" + str2 + ") and schema_name(o.schema_id) = '" + base.SystemSchema + "'\r\norder by o.name, c.column_id");
        }

        protected override string GetRelationSql()
        {
            return "select object_name (constraint_object_id),\r\nreferenced_object_id, referenced_column_id,\r\nparent_object_id, parent_column_id\r\nfrom sys.foreign_key_columns";
        }

        protected override string GetRoutineSql()
        {
            if (base.SystemSchema == null)
            {
                return base.GetRoutineSql();
            }
            return "select\r\n\t'PROCEDURE' as ROUTINE_TYPE,\r\n\t'sys' AS SPECIFIC_SCHEMA,\r\n\tOBJECT_NAME(o.object_id) as SPECIFIC_NAME,\t\r\n\t(case p.parameter_id when 0 THEN 'YES' ELSE 'NO' END) AS IS_RESULT,\r\n\tp.name as PARAMETER_NAME,\r\n\tconvert(nvarchar(10), CASE WHEN p.parameter_id = 0 THEN 'OUT' WHEN p.is_output = 1 THEN 'INOUT' ELSE 'IN' END) AS PARAMETER_MODE,\r\n\tp.parameter_id AS ORDINAL_POSITION,\r\n\tp.system_type_id AS DATA_TYPE\t\r\nfrom sys.all_objects o\r\njoin sys.all_parameters p\r\non o.object_id = p.object_id\r\norder by object_name (o.object_id), p.parameter_id";
        }

        protected override void PopulateVersionAndTypes()
        {
            base._sqlVersion = "10";
            List<KeyValuePair<int, byte>> list = new List<KeyValuePair<int, byte>>();
            using (SqlDataReader reader = new SqlCommand("select system_type_id, user_type_id, name from sys.types where system_type_id = user_type_id", base._cx).ExecuteReader())
            {
                while (reader.Read())
                {
                    DbTypeInfo dbTypeInfo = SqlSchemaReader.GetDbTypeInfo(reader.GetString(2));
                    if (!((dbTypeInfo == null) || base._sqlTypes.ContainsKey(reader.GetInt32(1))))
                    {
                        base._sqlTypes.Add(reader.GetInt32(1), dbTypeInfo);
                    }
                    else
                    {
                        list.Add(new KeyValuePair<int, byte>(reader.GetInt32(1), reader.GetByte(0)));
                    }
                }
            }
            foreach (KeyValuePair<int, byte> pair in list)
            {
                if (base._sqlTypes.ContainsKey(pair.Value))
                {
                    base._sqlTypes[pair.Key] = base._sqlTypes[pair.Value];
                }
            }
        }

        protected override bool AllowOtherDatabases
        {
            get
            {
                return true;
            }
        }

        protected override bool ThunkToMasterForSystemSchemas
        {
            get
            {
                return false;
            }
        }
    }
}

