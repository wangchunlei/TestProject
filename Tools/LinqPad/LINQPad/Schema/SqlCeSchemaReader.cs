namespace LINQPad.Schema
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext.DbSchema;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class SqlCeSchemaReader : SqlSchemaReader
    {
        private IDbConnection _cx;
        private Repository _repository;

        private IEnumerable<ColumnAssociation> GetAssociations()
        {
            return new <GetAssociations>d__f(-2) { <>4__this = this };
        }

        private string GetAssociationSql()
        {
            return "select c.CONSTRAINT_NAME, \r\n\tc.UNIQUE_CONSTRAINT_TABLE_NAME ParentTable, colParent.COLUMN_NAME ParentColumn,\r\n\tc.CONSTRAINT_TABLE_NAME ChildTable, colChild.COLUMN_NAME ChildColumn \r\nfrom INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c\r\njoin INFORMATION_SCHEMA.KEY_COLUMN_USAGE colChild on c.CONSTRAINT_NAME = colChild.CONSTRAINT_NAME\r\njoin INFORMATION_SCHEMA.KEY_COLUMN_USAGE colParent on c.UNIQUE_CONSTRAINT_NAME = colParent.CONSTRAINT_NAME\r\nand colChild.ORDINAL_POSITION = colParent.ORDINAL_POSITION";
        }

        private Column GetColumn(IDataReader r)
        {
            int i = 0;
            SqlColumn column = new SqlColumn();
            if (!r.IsDBNull(0))
            {
                column.SchemaName = r.GetString(i);
            }
            i++;
            column.ObjectName = r.GetString(i++);
            column.ColumnOrdinal = r.GetInt32(i++);
            column.ColumnID = column.ColumnOrdinal;
            column.ColumnName = r.GetString(i++);
            string sqlTypeName = r.GetString(i++);
            column.IsNullable = r.GetString(i++).ToLowerInvariant() == "yes";
            if (!r.IsDBNull(i))
            {
                int.TryParse(r[i].ToString(), out column.Length);
            }
            i++;
            if (!r.IsDBNull(i))
            {
                int.TryParse(r[i].ToString(), out column.Precision);
            }
            i++;
            if (!r.IsDBNull(i))
            {
                int.TryParse(r[i].ToString(), out column.Scale);
            }
            i++;
            if (!r.IsDBNull(i))
            {
                column.IsAutoGen = true;
            }
            i++;
            column.IsTimeStamp = sqlTypeName.ToLowerInvariant() == "timestamp";
            column.IsComputed = column.IsAutoGen || column.IsTimeStamp;
            column.SqlType = SqlSchemaReader.GetDbTypeInfo(sqlTypeName);
            if (column.SqlType == null)
            {
                return null;
            }
            column.ClrType = column.SqlType.Type;
            if (column.IsNullable && column.ClrType.IsValueType)
            {
                column.ClrType = typeof(Nullable<>).MakeGenericType(new Type[] { column.ClrType });
            }
            return column;
        }

        public virtual IEnumerable<Column> GetColumns(HashSet<string> pkeys)
        {
            return new <GetColumns>d__0(-2) { <>4__this = this, <>3__pkeys = pkeys };
        }

        private string GetColumnSql()
        {
            return "SELECT\r\n\tTABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION, COLUMN_NAME, DATA_TYPE, IS_NULLABLE, \r\n\tCHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, AUTOINC_INCREMENT AUTOINCREMENT\r\nFROM\r\n\tINFORMATION_SCHEMA.COLUMNS\r\nORDER BY\r\n\tTABLE_NAME, ORDINAL_POSITION";
        }

        public Database GetDatabase(Repository r)
        {
            this._repository = r;
            using (this._cx = this._repository.Open())
            {
                HashSet<string> pkeys = new HashSet<string>(this.GetPKeys());
                List<Column> columns = this.GetColumns(pkeys).ToList<Column>();
                return new Database(r.DynamicSchemaOptions, columns, this.GetAssociations().ToList<ColumnAssociation>(), null);
            }
        }

        private IEnumerable<string> GetPKeys()
        {
            return new <GetPKeys>d__6(-2) { <>4__this = this };
        }

        private string GetPKeySql()
        {
            return "select tc.TABLE_SCHEMA, tc.TABLE_NAME, kcu.COLUMN_NAME\r\nfrom INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc\r\n\tinner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu\r\n\ton tc.TABLE_NAME = kcu.TABLE_NAME \r\n\tand tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME\r\nwhere tc.CONSTRAINT_TYPE = 'PRIMARY KEY'\r\norder by tc.TABLE_NAME, kcu.ORDINAL_POSITION";
        }

        [CompilerGenerated]
        private sealed class <GetAssociations>d__f : IEnumerable<ColumnAssociation>, IEnumerable, IEnumerator<ColumnAssociation>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private ColumnAssociation <>2__current;
            public SqlCeSchemaReader <>4__this;
            public ColumnAssociation <>g__initLocale;
            private int <>l__initialThreadId;
            public IDbCommand <cmd>5__10;
            public IDataReader <reader>5__11;

            [DebuggerHidden]
            public <GetAssociations>d__f(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    bool flag = true;
                    int num = this.<>1__state;
                    if (num != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<cmd>5__10 = this.<>4__this._cx.CreateCommand();
                        this.<cmd>5__10.CommandText = this.<>4__this.GetAssociationSql();
                        this.<reader>5__11 = this.<cmd>5__10.ExecuteReader();
                    }
                    try
                    {
                        num = this.<>1__state;
                        if (num == 1)
                        {
                            if (this.$__disposing)
                            {
                                return false;
                            }
                            this.<>1__state = 0;
                        }
                        if (this.<reader>5__11.Read())
                        {
                            this.<>g__initLocale = new ColumnAssociation();
                            this.<>g__initLocale.RelationshipName = this.<reader>5__11.GetString(0);
                            this.<>g__initLocale.ParentTable = this.<reader>5__11.GetString(1);
                            this.<>g__initLocale.ParentColumn = this.<reader>5__11.GetString(2);
                            this.<>g__initLocale.ChildTable = this.<reader>5__11.GetString(3);
                            this.<>g__initLocale.ChildColumn = this.<reader>5__11.GetString(4);
                            this.<>2__current = this.<>g__initLocale;
                            this.<>1__state = 1;
                            flag = false;
                            return true;
                        }
                    }
                    finally
                    {
                        if (flag && (this.<reader>5__11 != null))
                        {
                            this.<reader>5__11.Dispose();
                        }
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
            IEnumerator<ColumnAssociation> IEnumerable<ColumnAssociation>.GetEnumerator()
            {
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    return this;
                }
                return new SqlCeSchemaReader.<GetAssociations>d__f(0) { <>4__this = this.<>4__this };
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
        private sealed class <GetColumns>d__0 : IEnumerable<Column>, IEnumerable, IEnumerator<Column>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private Column <>2__current;
            public HashSet<string> <>3__pkeys;
            public SqlCeSchemaReader <>4__this;
            private int <>l__initialThreadId;
            public Column <c>5__3;
            public IDbCommand <cmd>5__1;
            public IDataReader <reader>5__2;
            public HashSet<string> pkeys;

            [DebuggerHidden]
            public <GetColumns>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    bool flag = true;
                    int num = this.<>1__state;
                    if (num != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<cmd>5__1 = this.<>4__this._cx.CreateCommand();
                        this.<cmd>5__1.CommandText = this.<>4__this.GetColumnSql();
                        this.<reader>5__2 = this.<cmd>5__1.ExecuteReader();
                    }
                    try
                    {
                        num = this.<>1__state;
                        if (num == 1)
                        {
                            if (this.$__disposing)
                            {
                                return false;
                            }
                            this.<>1__state = 0;
                        }
                        while (this.<reader>5__2.Read())
                        {
                            this.<c>5__3 = this.<>4__this.GetColumn(this.<reader>5__2);
                            if (this.<c>5__3 != null)
                            {
                                goto Label_00D3;
                            }
                        }
                        goto Label_0172;
                    Label_00D3:;
                        this.<c>5__3.IsKey = this.pkeys.Contains(this.<c>5__3.SchemaName + "." + this.<c>5__3.ObjectName + "." + this.<c>5__3.ColumnName);
                        this.<>2__current = this.<c>5__3;
                        this.<>1__state = 1;
                        flag = false;
                        return true;
                    }
                    finally
                    {
                        if (flag && (this.<reader>5__2 != null))
                        {
                            this.<reader>5__2.Dispose();
                        }
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
            Label_0172:
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
            {
                SqlCeSchemaReader.<GetColumns>d__0 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new SqlCeSchemaReader.<GetColumns>d__0(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.pkeys = this.<>3__pkeys;
                return d__;
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
        private sealed class <GetPKeys>d__6 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private string <>2__current;
            public SqlCeSchemaReader <>4__this;
            private int <>l__initialThreadId;
            public IDbCommand <cmd>5__7;
            public string <colName>5__b;
            public IDataReader <reader>5__8;
            public string <schemaName>5__9;
            public string <tableName>5__a;

            [DebuggerHidden]
            public <GetPKeys>d__6(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    bool flag = true;
                    int num = this.<>1__state;
                    if (num != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<cmd>5__7 = this.<>4__this._cx.CreateCommand();
                        this.<cmd>5__7.CommandText = this.<>4__this.GetPKeySql();
                        this.<reader>5__8 = this.<cmd>5__7.ExecuteReader();
                    }
                    try
                    {
                        num = this.<>1__state;
                        if (num == 1)
                        {
                            if (this.$__disposing)
                            {
                                return false;
                            }
                            this.<>1__state = 0;
                        }
                        if (this.<reader>5__8.Read())
                        {
                            this.<schemaName>5__9 = "";
                            if (!this.<reader>5__8.IsDBNull(0))
                            {
                                this.<schemaName>5__9 = this.<reader>5__8.GetString(0);
                            }
                            this.<tableName>5__a = this.<reader>5__8.GetString(1);
                            this.<colName>5__b = this.<reader>5__8.GetString(2);
                            this.<>2__current = this.<schemaName>5__9 + "." + this.<tableName>5__a + "." + this.<colName>5__b;
                            this.<>1__state = 1;
                            flag = false;
                            return true;
                        }
                    }
                    finally
                    {
                        if (flag && (this.<reader>5__8 != null))
                        {
                            this.<reader>5__8.Dispose();
                        }
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
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    return this;
                }
                return new SqlCeSchemaReader.<GetPKeys>d__6(0) { <>4__this = this.<>4__this };
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

