namespace LINQPad.Schema
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext.DbSchema;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class SqlServerSchemaReader : SqlSchemaReader
    {
        protected SqlConnection _cx;
        private bool _excludeFuncs;
        private LINQPad.Repository _repository;
        private string _serverPrefix;
        protected Dictionary<int, DbTypeInfo> _sqlTypes = new Dictionary<int, DbTypeInfo>();
        protected string _sqlVersion;
        private string _systemSchema;

        private SqlServerSchemaReader CreateNew()
        {
            return (SqlServerSchemaReader) Activator.CreateInstance(base.GetType());
        }

        protected virtual string GetColumnSql()
        {
            string str = ((this._sqlVersion[0] == '9') || (this._sqlVersion[0] == '1')) ? "schema_name" : "user_name";
            string str2 = this._repository.ExcludeRoutines ? "'U', 'V'" : (this._excludeFuncs ? "'U', 'V', 'P'" : "'U', 'V', 'TF', 'IF', 'FN', 'FS', 'FT', 'P'");
            string str3 = "";
            if (this._systemSchema != null)
            {
                str3 = " and " + str + "(o.uid) = '" + this._systemSchema + "'";
            }
            return ("select\r\n\to.id TableID, " + str + "(o.uid) SchemaName,\r\n\to.name TableName,\r\n\to.xtype,\r\n\tc.colid,\r\n\tc.colorder,\r\n\tc.name ColName,\r\n\tc.xusertype, \r\n\tcast (case c.isnullable when 1 then 1 else 0 end as bit) isnullable,\r\n\tc.length, \r\n\tc.prec, \r\n\tc.scale,\r\n\tcast (case when c.status & 0x80 = 0x80 then 1 else 0 end as bit) isautogen,\r\n\tcast (case c.iscomputed when 1 then 1 else 0 end as bit) iscomputed\r\nfrom " + this._serverPrefix + "dbo.sysobjects o\r\nleft join " + this._serverPrefix + "dbo.syscolumns c on o.id = c.id and (o.xtype not in ('TF', 'IF', 'FN', 'FS', 'FT', 'P') or c.name not like '@%')\r\nwhere o.xtype in (" + str2 + ")" + str3 + "\r\norder by o.name, c.colorder");
        }

        public Database GetDatabase(LINQPad.Repository r)
        {
            return this.GetDatabase(r, null, false, null);
        }

        public Database GetDatabase(LINQPad.Repository r, LinkedDatabase serverLinkedDb, bool sameServerLink, string systemSchema)
        {
            Func<LINQPad.Repository, Database> selector = null;
            Func<LinkedDatabase, Database> func2 = null;
            Database database;
            if (systemSchema != null)
            {
                sameServerLink = true;
            }
            this._repository = r;
            this._serverPrefix = (serverLinkedDb == null) ? "" : (serverLinkedDb.QualifiedPrefix + ".");
            this._excludeFuncs = serverLinkedDb != null;
            this._systemSchema = systemSchema;
            using (this._cx = (SqlConnection) this._repository.Open())
            {
                this.PopulateVersionAndTypes();
                using (SqlDataReader reader = this.GetMainSchemaReader())
                {
                    HashSet<string> keys = new HashSet<string>(this.ReadPKeys(reader));
                    reader.NextResult();
                    List<Column> columns = this.ReadColumns(reader, keys).ToList<Column>();
                    reader.NextResult();
                    List<ColumnAssociation> associations = this.ReadAssociations(columns, reader).ToList<ColumnAssociation>();
                    reader.NextResult();
                    List<Parameter> routineParameters = this.ReadParameters(reader).ToList<Parameter>();
                    if (serverLinkedDb != null)
                    {
                        return new Database(serverLinkedDb.Server, serverLinkedDb.Database, systemSchema, r.DynamicSchemaOptions, columns, associations, routineParameters, null);
                    }
                    if (sameServerLink || (systemSchema != null))
                    {
                        return new Database(null, r.Database, systemSchema, r.DynamicSchemaOptions, columns, associations, routineParameters, null);
                    }
                    List<Database> list4 = new List<Database>();
                    if (this.AllowOtherDatabases)
                    {
                        if ((r.IncludeSystemObjects && (r.Database != "master")) && (string.IsNullOrEmpty(this._sqlVersion) || (this._sqlVersion[0] != '8')))
                        {
                            LINQPad.Repository repository = r.Clone();
                            repository.LinkedDatabases = null;
                            repository.IncludeSystemObjects = false;
                            if (this.ThunkToMasterForSystemSchemas)
                            {
                                repository.Database = "master";
                            }
                            repository.AttachFile = false;
                            repository.AttachFileName = "";
                            list4.Add(this.CreateNew().GetDatabase(repository, null, true, "sys"));
                            list4.Add(this.CreateNew().GetDatabase(repository, null, true, "INFORMATION_SCHEMA"));
                        }
                        if (selector == null)
                        {
                            selector = lr => this.CreateNew().GetDatabase(lr, null, true, null);
                        }
                        list4.AddRange(r.GetSameServerLinkedRepositories().Select<LINQPad.Repository, Database>(selector));
                        if (func2 == null)
                        {
                            func2 = ld => this.CreateNew().GetDatabase(r, ld, false, null);
                        }
                        list4.AddRange(r.GetOtherServerLinkedDatabases().Select<LinkedDatabase, Database>(func2));
                    }
                    database = new Database(null, null, null, r.DynamicSchemaOptions, columns, associations, routineParameters, list4.ToArray());
                }
            }
            return database;
        }

        private SqlDataReader GetMainSchemaReader()
        {
            SqlCommand command = new SqlCommand(this.GetPKeySql() + "\r\n\r\n" + this.GetColumnSql() + "\r\n\r\n" + this.GetRelationSql() + "\r\n\r\n" + this.GetRoutineSql(), this._cx) {
                CommandTimeout = 120
            };
            return command.ExecuteReader();
        }

        private string GetPKeySql()
        {
            string str = "select tc.TABLE_SCHEMA, tc.TABLE_NAME, kcu.COLUMN_NAME\r\nfrom " + this._serverPrefix + "INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc\r\n\tinner join " + this._serverPrefix + "INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu\r\n\ton tc.TABLE_NAME = kcu.TABLE_NAME \r\n\tand tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA\r\n\tand tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME\r\nwhere tc.CONSTRAINT_TYPE = 'PRIMARY KEY'" + ((this._systemSchema == null) ? "" : " and 1=0") + "\r\norder by tc.TABLE_NAME, kcu.ORDINAL_POSITION";
            if ((this._sqlVersion[0] == '9') || (this._sqlVersion[0] == '1'))
            {
                str = str + " option (force order)";
            }
            return str;
        }

        protected virtual string GetRelationSql()
        {
            string str = "select object_name (constid), rkeyid, rkey, fkeyid, fkey from " + this._serverPrefix + "dbo.sysforeignkeys f1";
            if (this._systemSchema != null)
            {
                str = str + " where 1=0";
            }
            return str;
        }

        protected virtual string GetRoutineSql()
        {
            if ((this._systemSchema != null) || (this._repository.Database == "master"))
            {
                return ("select\r\n\t'PROCEDURE' as ROUTINE_TYPE,\r\n\t'sys' AS SPECIFIC_SCHEMA,\r\n\tOBJECT_NAME(o.id) as SPECIFIC_NAME,\t\r\n\t(case p.parameter_id when 0 THEN 'YES' ELSE 'NO' END) AS IS_RESULT,\r\n\tp.name as PARAMETER_NAME,\r\n\tconvert(nvarchar(10), CASE WHEN p.parameter_id = 0 THEN 'OUT' WHEN p.is_output = 1 THEN 'INOUT' ELSE 'IN' END) AS PARAMETER_MODE,\r\n\tp.parameter_id AS ORDINAL_POSITION,\r\n\tp.system_type_id AS DATA_TYPE\t\r\nfrom master.sys.sysobjects o\r\njoin master.sys.all_parameters p\r\non o.id = p.object_id\r\n" + ((this._systemSchema == "INFORMATION_SCHEMA") ? "where 1=0\r\n" : "") + "order by object_name (o.id), p.parameter_id");
            }
            return ("select r.ROUTINE_TYPE, r.SPECIFIC_SCHEMA, r.SPECIFIC_NAME,\r\n\tp.IS_RESULT, p.PARAMETER_NAME, p.PARAMETER_MODE, p.ORDINAL_POSITION, p.DATA_TYPE\t\r\nfrom " + this._serverPrefix + "INFORMATION_SCHEMA.ROUTINES r \r\njoin " + this._serverPrefix + "INFORMATION_SCHEMA.PARAMETERS p\r\non r.SPECIFIC_SCHEMA = p.SPECIFIC_SCHEMA and r.SPECIFIC_NAME = p.SPECIFIC_NAME \r\nwhere r.ROUTINE_TYPE in ('PROCEDURE', 'FUNCTION') \r\norder by r.SPECIFIC_SCHEMA, r.SPECIFIC_NAME, p.ORDINAL_POSITION");
        }

        protected virtual void PopulateVersionAndTypes()
        {
            string cmdText = "select SERVERPROPERTY('productversion')\r\nselect xtype, xusertype, name from " + this._serverPrefix + "dbo.systypes";
            List<KeyValuePair<short, byte>> list = new List<KeyValuePair<short, byte>>();
            using (SqlDataReader reader = new SqlCommand(cmdText, this._cx).ExecuteReader())
            {
                reader.Read();
                this._sqlVersion = reader.GetString(0);
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        DbTypeInfo dbTypeInfo = SqlSchemaReader.GetDbTypeInfo(reader.GetString(2));
                        if (!((dbTypeInfo == null) || this._sqlTypes.ContainsKey(reader.GetInt16(1))))
                        {
                            this._sqlTypes.Add(reader.GetInt16(1), dbTypeInfo);
                        }
                        else
                        {
                            list.Add(new KeyValuePair<short, byte>(reader.GetInt16(1), reader.GetByte(0)));
                        }
                    }
                }
            }
            foreach (KeyValuePair<short, byte> pair in list)
            {
                if (this._sqlTypes.ContainsKey(pair.Value))
                {
                    this._sqlTypes[pair.Key] = this._sqlTypes[pair.Value];
                }
            }
        }

        private IEnumerable<ColumnAssociation> ReadAssociations(IEnumerable<Column> columns, SqlDataReader reader)
        {
            return new <ReadAssociations>d__16(-2) { <>4__this = this, <>3__columns = columns, <>3__reader = reader };
        }

        private IEnumerable<Column> ReadColumns(SqlDataReader r, HashSet<string> keys)
        {
            return new <ReadColumns>d__d(-2) { <>4__this = this, <>3__r = r, <>3__keys = keys };
        }

        private IEnumerable<Parameter> ReadParameters(SqlDataReader r)
        {
            return new <ReadParameters>d__24(-2) { <>4__this = this, <>3__r = r };
        }

        private IEnumerable<string> ReadPKeys(SqlDataReader reader)
        {
            return new <ReadPKeys>d__7(-2) { <>4__this = this, <>3__reader = reader };
        }

        protected virtual bool AllowOtherDatabases
        {
            get
            {
                return true;
            }
        }

        protected LINQPad.Repository Repository
        {
            get
            {
                return this._repository;
            }
        }

        protected string SystemSchema
        {
            get
            {
                return this._systemSchema;
            }
        }

        protected virtual bool ThunkToMasterForSystemSchemas
        {
            get
            {
                return true;
            }
        }

        [CompilerGenerated]
        private sealed class <ReadAssociations>d__16 : IEnumerable<ColumnAssociation>, IEnumerable, IEnumerator<ColumnAssociation>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private ColumnAssociation <>2__current;
            public IEnumerable<Column> <>3__columns;
            public SqlDataReader <>3__reader;
            public SqlServerSchemaReader <>4__this;
            public ColumnAssociation <>g__initLocal15;
            private int <>l__initialThreadId;
            public SqlColumn <c>5__18;
            public Column <childColumn>5__21;
            public int <childColumnID>5__1e;
            public int <childTableID>5__1d;
            public Dictionary<int, Dictionary<int, Column>> <colLookup>5__17;
            public Dictionary<int, Column> <columnDict>5__19;
            public Dictionary<int, Column> <columnDict>5__1f;
            public string <name>5__1a;
            public Column <parentColumn>5__20;
            public int <parentColumnID>5__1c;
            public int <parentTableID>5__1b;
            public IEnumerable<Column> columns;
            public SqlDataReader reader;

            [DebuggerHidden]
            public <ReadAssociations>d__16(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    bool flag = true;
                    if (this.<>1__state != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<colLookup>5__17 = new Dictionary<int, Dictionary<int, Column>>();
                        IEnumerator<Column> enumerator = this.columns.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                this.<c>5__18 = (SqlColumn) enumerator.Current;
                                if (!this.<colLookup>5__17.TryGetValue(this.<c>5__18.ObjectID, out this.<columnDict>5__19))
                                {
                                    this.<colLookup>5__17[this.<c>5__18.ObjectID] = this.<columnDict>5__19 = new Dictionary<int, Column>();
                                }
                                this.<columnDict>5__19[this.<c>5__18.ColumnID] = this.<c>5__18;
                            }
                        }
                        finally
                        {
                            if (flag && (enumerator != null))
                            {
                                enumerator.Dispose();
                            }
                        }
                    }
                    else
                    {
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>1__state = 0;
                    }
                    while (this.reader.Read())
                    {
                        if (!this.reader.IsDBNull(0))
                        {
                            this.<name>5__1a = this.reader.GetString(0);
                            this.<parentTableID>5__1b = this.reader.GetInt32(1);
                            this.<parentColumnID>5__1c = (this.reader[2] is short) ? this.reader.GetInt16(2) : this.reader.GetInt32(2);
                            this.<childTableID>5__1d = this.reader.GetInt32(3);
                            this.<childColumnID>5__1e = (this.reader[4] is short) ? this.reader.GetInt16(4) : this.reader.GetInt32(4);
                            if ((this.<colLookup>5__17.TryGetValue(this.<parentTableID>5__1b, out this.<columnDict>5__1f) && this.<columnDict>5__1f.TryGetValue(this.<parentColumnID>5__1c, out this.<parentColumn>5__20)) && (this.<colLookup>5__17.TryGetValue(this.<childTableID>5__1d, out this.<columnDict>5__1f) && this.<columnDict>5__1f.TryGetValue(this.<childColumnID>5__1e, out this.<childColumn>5__21)))
                            {
                                goto Label_0229;
                            }
                        }
                    }
                    goto Label_02EC;
                Label_0229:
                    this.<>g__initLocal15 = new ColumnAssociation();
                    this.<>g__initLocal15.RelationshipName = this.<name>5__1a;
                    this.<>g__initLocal15.ParentSchema = this.<parentColumn>5__20.SchemaName;
                    this.<>g__initLocal15.ParentTable = this.<parentColumn>5__20.ObjectName;
                    this.<>g__initLocal15.ParentColumn = this.<parentColumn>5__20.ColumnName;
                    this.<>g__initLocal15.ChildSchema = this.<childColumn>5__21.SchemaName;
                    this.<>g__initLocal15.ChildTable = this.<childColumn>5__21.ObjectName;
                    this.<>g__initLocal15.ChildColumn = this.<childColumn>5__21.ColumnName;
                    this.<>2__current = this.<>g__initLocal15;
                    this.<>1__state = 1;
                    flag = false;
                    return true;
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
            Label_02EC:
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<ColumnAssociation> IEnumerable<ColumnAssociation>.GetEnumerator()
            {
                SqlServerSchemaReader.<ReadAssociations>d__16 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new SqlServerSchemaReader.<ReadAssociations>d__16(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.columns = this.<>3__columns;
                d__.reader = this.<>3__reader;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<LINQPad.Extensibility.DataContext.DbSchema.ColumnAssociation>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            ColumnAssociation IEnumerator<ColumnAssociation>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <ReadColumns>d__d : IEnumerable<Column>, IEnumerable, IEnumerator<Column>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private Column <>2__current;
            public HashSet<string> <>3__keys;
            public SqlDataReader <>3__r;
            public SqlServerSchemaReader <>4__this;
            private int <>l__initialThreadId;
            public bool <allowChars>5__12;
            public SqlColumn <c>5__e;
            public int <colType>5__11;
            public int <i>5__f;
            public string <tableKind>5__10;
            public HashSet<string> keys;
            public SqlDataReader r;

            [DebuggerHidden]
            public <ReadColumns>d__d(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 1:
                            if (!this.$__disposing)
                            {
                                break;
                            }
                            return false;

                        case 2:
                            if (!this.$__disposing)
                            {
                                goto Label_0071;
                            }
                            return false;

                        case -1:
                            return false;

                        default:
                            if (!this.$__disposing)
                            {
                                goto Label_0484;
                            }
                            return false;
                    }
                    this.<>1__state = 0;
                    goto Label_0484;
                Label_0071:
                    this.<>1__state = 0;
                Label_0484:
                    while (this.r.Read())
                    {
                        this.<c>5__e = new SqlColumn();
                        this.<i>5__f = 0;
                        this.<c>5__e.ObjectID = this.r.GetInt32(this.<i>5__f++);
                        if (!this.r.IsDBNull(this.<i>5__f))
                        {
                            this.<c>5__e.SchemaName = this.r.GetString(this.<i>5__f);
                        }
                        else
                        {
                            this.<c>5__e.SchemaName = "dbo";
                        }
                        this.<i>5__f++;
                        if ((!string.IsNullOrEmpty(this.<>4__this._systemSchema) || !(this.<c>5__e.SchemaName == "sys")) || !(this.<>4__this._repository.Database != "master"))
                        {
                            this.<c>5__e.ObjectName = this.r.GetString(this.<i>5__f++);
                            this.<tableKind>5__10 = this.r.GetString(this.<i>5__f++).Trim();
                            this.<c>5__e.ObjectKind = (((this.<tableKind>5__10 == "TF") || (this.<tableKind>5__10 == "IF")) || (this.<tableKind>5__10 == "FT")) ? DbObjectKind.TableFunction : (((this.<tableKind>5__10 == "FN") || (this.<tableKind>5__10 == "FS")) ? DbObjectKind.ScalarFunction : ((this.<tableKind>5__10 == "P") ? DbObjectKind.StoredProc : ((this.<tableKind>5__10 == "V") ? DbObjectKind.View : DbObjectKind.Table)));
                            if (this.r.IsDBNull(this.<i>5__f))
                            {
                                goto Label_0499;
                            }
                            this.<c>5__e.ColumnID = int.Parse(this.r.GetValue(this.<i>5__f++).ToString());
                            this.<c>5__e.ColumnOrdinal = int.Parse(this.r.GetValue(this.<i>5__f++).ToString());
                            if (!this.r.IsDBNull(this.<i>5__f))
                            {
                                this.<c>5__e.ColumnName = this.r.GetString(this.<i>5__f++);
                                if (!this.r.IsDBNull(this.<i>5__f))
                                {
                                    this.<colType>5__11 = int.Parse(this.r.GetValue(this.<i>5__f++).ToString());
                                    this.<c>5__e.IsNullable = this.r.GetBoolean(this.<i>5__f++);
                                    this.<c>5__e.Length = this.r.GetInt16(this.<i>5__f++);
                                    if (!this.r.IsDBNull(this.<i>5__f))
                                    {
                                        this.<c>5__e.Precision = int.Parse(this.r.GetValue(this.<i>5__f).ToString());
                                    }
                                    this.<i>5__f++;
                                    if (!this.r.IsDBNull(this.<i>5__f))
                                    {
                                        this.<c>5__e.Scale = int.Parse(this.r.GetValue(this.<i>5__f).ToString());
                                    }
                                    this.<i>5__f++;
                                    this.<c>5__e.IsAutoGen = this.r.GetBoolean(this.<i>5__f++);
                                    this.<c>5__e.IsComputed = this.r.GetBoolean(this.<i>5__f++);
                                    if (this.<>4__this._sqlTypes.TryGetValue(this.<colType>5__11, out this.<c>5__e.SqlType))
                                    {
                                        goto Label_04B3;
                                    }
                                }
                            }
                        }
                    }
                    goto Label_0665;
                Label_0499:
                    this.<>2__current = this.<c>5__e;
                    this.<>1__state = 1;
                    return true;
                Label_04B3:
                    this.<c>5__e.ClrType = this.<c>5__e.SqlType.Type;
                    this.<allowChars>5__12 = Program.AllowOneToOne;
                    if (((this.<allowChars>5__12 && (this.<c>5__e.ClrType == typeof(string))) && (this.<c>5__e.Precision == 1)) && ((this.<c>5__e.SqlType.Name == "Char") || (this.<c>5__e.SqlType.Name == "NChar")))
                    {
                        this.<c>5__e.ClrType = typeof(char);
                    }
                    this.<c>5__e.IsTimeStamp = this.<c>5__e.SqlType.Name.ToLowerInvariant() == "timestamp";
                    if (this.<c>5__e.IsNullable && this.<c>5__e.ClrType.IsValueType)
                    {
                        this.<c>5__e.ClrType = typeof(Nullable<>).MakeGenericType(new Type[] { this.<c>5__e.ClrType });
                    }
                    this.<c>5__e.IsKey = this.keys.Contains(this.<c>5__e.SchemaName + "." + this.<c>5__e.ObjectName + "." + this.<c>5__e.ColumnName);
                    this.<>2__current = this.<c>5__e;
                    this.<>1__state = 2;
                    return true;
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
            Label_0665:
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
            {
                SqlServerSchemaReader.<ReadColumns>d__d _d;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _d = this;
                }
                else
                {
                    _d = new SqlServerSchemaReader.<ReadColumns>d__d(0) {
                        <>4__this = this.<>4__this
                    };
                }
                _d.r = this.<>3__r;
                _d.keys = this.<>3__keys;
                return _d;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<LINQPad.Extensibility.DataContext.DbSchema.Column>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            Column IEnumerator<Column>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <ReadParameters>d__24 : IEnumerable<Parameter>, IEnumerable, IEnumerator<Parameter>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private Parameter <>2__current;
            public SqlDataReader <>3__r;
            public SqlServerSchemaReader <>4__this;
            private int <>l__initialThreadId;
            public int <i>5__26;
            public Parameter <p>5__25;
            public string <paramMode>5__27;
            public object <paramOrder>5__28;
            public string <paramTypeName>5__29;
            public SqlDataReader r;

            [DebuggerHidden]
            public <ReadParameters>d__24(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    if (this.<>1__state != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>1__state = 0;
                    }
                    if (this.r.Read())
                    {
                        this.<p>5__25 = new Parameter();
                        this.<i>5__26 = 0;
                        this.<p>5__25.IsFunction = this.r.GetString(this.<i>5__26++).Trim().ToUpperInvariant() == "FUNCTION";
                        this.<p>5__25.RoutineSchema = this.r.GetString(this.<i>5__26++);
                        this.<p>5__25.RoutineName = this.r.GetString(this.<i>5__26++);
                        if (!this.r.IsDBNull(this.<i>5__26))
                        {
                            this.<p>5__25.IsResult = this.r.GetString(this.<i>5__26).Trim().ToUpperInvariant() == "YES";
                        }
                        this.<i>5__26++;
                        if (!this.r.IsDBNull(this.<i>5__26))
                        {
                            this.<p>5__25.ParamName = this.r.GetString(this.<i>5__26);
                        }
                        this.<i>5__26++;
                        if (!this.r.IsDBNull(this.<i>5__26))
                        {
                            this.<paramMode>5__27 = this.r.GetString(this.<i>5__26).Trim();
                            this.<p>5__25.IsIn = this.<paramMode>5__27.IndexOf("in", StringComparison.OrdinalIgnoreCase) > -1;
                            this.<p>5__25.IsOut = this.<paramMode>5__27.IndexOf("out", StringComparison.OrdinalIgnoreCase) > -1;
                        }
                        this.<i>5__26++;
                        if (!this.r.IsDBNull(this.<i>5__26))
                        {
                            this.<paramOrder>5__28 = this.r.GetValue(this.<i>5__26);
                            if (this.<paramOrder>5__28 is int)
                            {
                                this.<p>5__25.ParamOrdinal = (int) this.<paramOrder>5__28;
                            }
                            else if (this.<paramOrder>5__28 is short)
                            {
                                this.<p>5__25.ParamOrdinal = (short) this.<paramOrder>5__28;
                            }
                        }
                        this.<i>5__26++;
                        if (!this.r.IsDBNull(this.<i>5__26))
                        {
                            this.<paramTypeName>5__29 = null;
                            if (this.r.GetValue(this.<i>5__26) is string)
                            {
                                this.<paramTypeName>5__29 = this.r.GetString(this.<i>5__26);
                                if (!string.IsNullOrEmpty(this.<paramTypeName>5__29))
                                {
                                    this.<p>5__25.ParamDbType = SqlSchemaReader.GetDbTypeInfo(this.<paramTypeName>5__29);
                                }
                            }
                            else
                            {
                                this.<>4__this._sqlTypes.TryGetValue(this.r.GetByte(this.<i>5__26), out this.<p>5__25.ParamDbType);
                            }
                        }
                        this.<i>5__26++;
                        if (this.<p>5__25.ParamDbType != null)
                        {
                            this.<p>5__25.ClrType = this.<p>5__25.ParamDbType.Type;
                        }
                        if (this.<p>5__25.ClrType != null)
                        {
                            if (this.<p>5__25.ClrType.IsValueType)
                            {
                                this.<p>5__25.ClrType = typeof(Nullable<>).MakeGenericType(new Type[] { this.<p>5__25.ClrType });
                            }
                            this.<p>5__25.IsValid = true;
                        }
                        this.<>2__current = this.<p>5__25;
                        this.<>1__state = 1;
                        return true;
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Parameter> IEnumerable<Parameter>.GetEnumerator()
            {
                SqlServerSchemaReader.<ReadParameters>d__24 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new SqlServerSchemaReader.<ReadParameters>d__24(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.r = this.<>3__r;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<LINQPad.Extensibility.DataContext.DbSchema.Parameter>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            Parameter IEnumerator<Parameter>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <ReadPKeys>d__7 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private string <>2__current;
            public SqlDataReader <>3__reader;
            public SqlServerSchemaReader <>4__this;
            private int <>l__initialThreadId;
            public string <colName>5__a;
            public string <schemaName>5__8;
            public string <tableName>5__9;
            public SqlDataReader reader;

            [DebuggerHidden]
            public <ReadPKeys>d__7(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    if (this.<>1__state != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>1__state = 0;
                    }
                    if (this.reader.Read())
                    {
                        this.<schemaName>5__8 = "";
                        if (!this.reader.IsDBNull(0))
                        {
                            this.<schemaName>5__8 = this.reader.GetString(0);
                        }
                        this.<tableName>5__9 = this.reader.GetString(1);
                        this.<colName>5__a = this.reader.GetString(2);
                        this.<>2__current = this.<schemaName>5__8 + "." + this.<tableName>5__9 + "." + this.<colName>5__a;
                        this.<>1__state = 1;
                        return true;
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                SqlServerSchemaReader.<ReadPKeys>d__7 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new SqlServerSchemaReader.<ReadPKeys>d__7(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.reader = this.<>3__reader;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            string IEnumerator<string>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

