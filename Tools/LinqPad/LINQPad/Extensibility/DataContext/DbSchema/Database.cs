namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.Schema;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class Database
    {
        private IEnumerable<ColumnAssociation> _associations;
        private IEnumerable<Column> _columns;
        private Database[] _linkedDatabases;
        private bool _populated;
        private IEnumerable<Parameter> _routineParameters;
        private IDynamicSchemaOptions _schemaOptions;
        public readonly string CatalogName;
        public readonly Dictionary<string, SchemaObject> Objects;
        public readonly string ServerName;
        public readonly string SystemSchema;

        public Database(IDynamicSchemaOptions schemaOptions, IEnumerable<Column> columns, IEnumerable<ColumnAssociation> associations, IEnumerable<Parameter> routineParameters) : this(null, null, null, schemaOptions, columns, associations, routineParameters, null)
        {
        }

        public Database(string serverName, string catalogName, string systemSchema, IDynamicSchemaOptions schemaOptions, IEnumerable<Column> columns, IEnumerable<ColumnAssociation> associations, IEnumerable<Parameter> routineParameters, Database[] linkedDatabases)
        {
            this.Objects = new Dictionary<string, SchemaObject>();
            this.ServerName = serverName;
            this.CatalogName = catalogName;
            this.SystemSchema = systemSchema;
            this._schemaOptions = schemaOptions;
            this._columns = columns;
            this._associations = associations;
            this._routineParameters = routineParameters;
            this._linkedDatabases = linkedDatabases;
            this.LinkedDatabases = new Database[0];
            if (base.GetType() == typeof(Database))
            {
                this.Populate();
            }
        }

        private void AssignColumnPropertyNames()
        {
            foreach (SchemaObject obj2 in this.Objects.Values)
            {
                DotNetNameBank bank = new DotNetNameBank();
                foreach (Column column in obj2.Columns.Values)
                {
                    bank.RegisterName(column.ColumnName);
                }
                HashSet<string> set = new HashSet<string>();
                foreach (Column column in obj2.Columns.Values.ToArray<Column>())
                {
                    string uniqueDotNetName = bank.GetUniqueDotNetName(column.ColumnName);
                    if (uniqueDotNetName == null)
                    {
                        obj2.Columns.Remove(column.ColumnID);
                    }
                    else
                    {
                        column.PropertyName = this.TransformIdentifier(uniqueDotNetName);
                        if (column.PropertyName == column.ClrObjectName)
                        {
                            if (bank.ContainsDotNetName("Content"))
                            {
                                obj2.Columns.Remove(column.ColumnID);
                            }
                            else
                            {
                                column.PropertyName = "Content";
                            }
                        }
                        if (set.Contains(column.PropertyName))
                        {
                            obj2.Columns.Remove(column.ColumnID);
                        }
                        else
                        {
                            set.Add(column.PropertyName);
                        }
                    }
                }
            }
        }

        private List<ExplorerItem> GetChildItems(SchemaObject o, bool isSqlServer)
        {
            IEnumerable<ExplorerItem> first = Enumerable.Empty<ExplorerItem>();
            if (o.Parameters.Count > 0)
            {
                first = first.Concat<ExplorerItem>(this.GetParams(o));
            }
            first = first.Concat<ExplorerItem>(from col in o.ColumnsInOrder
                let text = col.PropertyName + " (" + col.ClrType.FormatTypeName() + ")"
                let sqlTypeDec = col.GetFullSqlTypeDeclaration()
                select new ExplorerItem(text, ExplorerItemKind.Property, col.IsKey ? ExplorerIcon.Key : ExplorerIcon.Column) { DragText = col.PropertyName, ToolTipText = (sqlTypeDec == null) ? null : (col.ColumnName + " " + sqlTypeDec), SqlName = col.ColumnName, SqlTypeDeclaration = sqlTypeDec });
            Table t = o as Table;
            if (t != null)
            {
                IOrderedEnumerable<ExplorerItem> second = from a in this.GetParents(t).Concat<ExplorerItem>(this.GetChildren(t))
                    orderby a.Icon != ExplorerIcon.OneToOne
                    select a;
                first = first.Concat<ExplorerItem>(second);
                if (!(t.HasKey || !isSqlServer))
                {
                    first = first.Concat<ExplorerItem>(new ExplorerItem[] { new ExplorerItem("(Warning: No primary key, updates will fail)", ExplorerItemKind.Property, ExplorerIcon.Inherited) });
                }
            }
            return first.ToList<ExplorerItem>();
        }

        private IEnumerable<ExplorerItem> GetChildren(Table t)
        {
            return (from c in t.ChildRelations
                orderby c.PropNameForParent
                select new ExplorerItem(c.PropNameForParent, c.IsOneToOne ? ExplorerItemKind.ReferenceLink : ExplorerItemKind.CollectionLink, c.IsOneToOne ? ExplorerIcon.OneToOne : ExplorerIcon.OneToMany) { DragText = c.PropNameForParent, ToolTipText = c.IsOneToOne ? c.ChildTable.DotNetName : ("IEnumerable <" + c.ChildTable.DotNetName + ">"), Tag = c.ChildTable });
        }

        private ExplorerIcon GetExplorerIcon(DbObjectKind objectKind)
        {
            switch (objectKind)
            {
                case DbObjectKind.Table:
                    return ExplorerIcon.Table;

                case DbObjectKind.View:
                    return ExplorerIcon.View;

                case DbObjectKind.StoredProc:
                    return ExplorerIcon.StoredProc;

                case DbObjectKind.ScalarFunction:
                    return ExplorerIcon.ScalarFunction;

                case DbObjectKind.TableFunction:
                    return ExplorerIcon.TableFunction;
            }
            return ExplorerIcon.Table;
        }

        public List<ExplorerItem> GetExplorerSchema()
        {
            return this.GetExplorerSchema(false, null, false);
        }

        internal List<ExplorerItem> GetExplorerSchema(bool isSqlServer, string dbPrefix, bool flattenSchema)
        {
            List<ExplorerItem> objectsAsExplorerItems = this.GetObjectsAsExplorerItems(isSqlServer, dbPrefix);
            Dictionary<object, ExplorerItem> dictionary = (from o in objectsAsExplorerItems
                where o.Icon == ExplorerIcon.Table
                select o).ToDictionary<ExplorerItem, object, ExplorerItem>(o => o.Tag, o => o);
            foreach (ExplorerItem item in objectsAsExplorerItems)
            {
                foreach (ExplorerItem item2 in item.Children)
                {
                    if ((item2.Tag != null) && dictionary.ContainsKey(item2.Tag))
                    {
                        item2.HyperlinkTarget = dictionary[item2.Tag];
                    }
                }
            }
            var typeArray = (from <>h__TransparentIdentifier1d in from item in objectsAsExplorerItems select new { item = item, so = (SchemaObject) item.Tag }
                group <>h__TransparentIdentifier1d.item by (string.IsNullOrEmpty(<>h__TransparentIdentifier1d.so.SchemaName) || (<>h__TransparentIdentifier1d.so.SchemaName.ToLowerInvariant() == "dbo")) ? ((IEnumerable<<>f__AnonymousType23<string, IGrouping<ExplorerIcon, ExplorerItem>, IGrouping<ExplorerIcon, ExplorerItem>, IGrouping<ExplorerIcon, ExplorerItem>, IGrouping<ExplorerIcon, ExplorerItem>, IGrouping<ExplorerIcon, ExplorerItem>>>) "") : ((IEnumerable<<>f__AnonymousType23<string, IGrouping<ExplorerIcon, ExplorerItem>, IGrouping<ExplorerIcon, ExplorerItem>, IGrouping<ExplorerIcon, ExplorerItem>, IGrouping<ExplorerIcon, ExplorerItem>, IGrouping<ExplorerIcon, ExplorerItem>>>) <>h__TransparentIdentifier1d.so.SchemaName) into g
                orderby g.Key
                let iconGroups = from i in g group i by i.Icon
                select new { SchemaName = g.Key, Tables = iconGroups.FirstOrDefault<IGrouping<ExplorerIcon, ExplorerItem>>(ig => ((ExplorerIcon) ig.Key) == ExplorerIcon.Table), Views = iconGroups.FirstOrDefault<IGrouping<ExplorerIcon, ExplorerItem>>(ig => ((ExplorerIcon) ig.Key) == ExplorerIcon.View), StoredProcs = iconGroups.FirstOrDefault<IGrouping<ExplorerIcon, ExplorerItem>>(ig => ((ExplorerIcon) ig.Key) == ExplorerIcon.StoredProc), ScalarFunctions = iconGroups.FirstOrDefault<IGrouping<ExplorerIcon, ExplorerItem>>(ig => ((ExplorerIcon) ig.Key) == ExplorerIcon.ScalarFunction), TableFunctions = iconGroups.FirstOrDefault<IGrouping<ExplorerIcon, ExplorerItem>>(ig => ((ExplorerIcon) ig.Key) == ExplorerIcon.TableFunction) }).ToArray();
            List<ExplorerItem> list2 = new List<ExplorerItem>();
            foreach (var type in typeArray)
            {
                List<ExplorerItem> list3;
                if ((type.SchemaName == "") || flattenSchema)
                {
                    list3 = list2;
                }
                else
                {
                    ExplorerItem item3 = new ExplorerItem(type.SchemaName, ExplorerItemKind.Schema, ExplorerIcon.Schema) {
                        SqlName = type.SchemaName
                    };
                    list2.Add(item3);
                    list3 = item3.Children = new List<ExplorerItem>();
                }
                if (type.Tables != null)
                {
                    list3.AddRange(type.Tables);
                }
                if (type.Views != null)
                {
                    ExplorerItem item4 = new ExplorerItem("Views", ExplorerItemKind.Category, ExplorerIcon.View) {
                        Children = type.Views.ToList<ExplorerItem>()
                    };
                    list3.Add(item4);
                }
                if (type.StoredProcs != null)
                {
                    ExplorerItem item5 = new ExplorerItem("Stored Procs", ExplorerItemKind.Category, ExplorerIcon.StoredProc) {
                        Children = type.StoredProcs.ToList<ExplorerItem>()
                    };
                    list3.Add(item5);
                }
                if ((type.ScalarFunctions != null) || (type.TableFunctions != null))
                {
                    ExplorerItem item6 = new ExplorerItem("Functions", ExplorerItemKind.Category, ExplorerIcon.ScalarFunction) {
                        Children = new List<ExplorerItem>()
                    };
                    if (type.ScalarFunctions != null)
                    {
                        item6.Children.AddRange(type.ScalarFunctions);
                    }
                    if (type.TableFunctions != null)
                    {
                        item6.Children.AddRange(type.TableFunctions);
                    }
                    list3.Add(item6);
                }
            }
            foreach (Database database in this.LinkedDatabases)
            {
                ExplorerItem item7 = new ExplorerItem(database.ClrName + ((database.ServerName == null) ? "" : (" (in " + database.ServerName + ')')), ExplorerItemKind.Schema, (database.SystemSchema == null) ? ExplorerIcon.LinkedDatabase : ExplorerIcon.Box) {
                    Children = database.GetExplorerSchema(isSqlServer, database.ClrName, database.SystemSchema != null)
                };
                list2.Add(item7);
            }
            return list2;
        }

        private List<ExplorerItem> GetObjectsAsExplorerItems(bool isSqlServer, string dbPrefix)
        {
            dbPrefix = dbPrefix ?? "";
            if (dbPrefix.Length > 0)
            {
                dbPrefix = dbPrefix + ".";
            }
            return (from t in this.Objects.Values
                orderby t.PropertyName.ToUpperInvariant()
                select new ExplorerItem(t.PropertyName + ((t.ReturnInfo == null) ? "" : (" -> " + t.ReturnInfo.ClrTypeName)), ExplorerItemKind.QueryableObject, this.GetExplorerIcon(t.Kind)) { Tag = t, DragText = dbPrefix + t.PropertyName, IsEnumerable = true, ToolTipText = t.PropertyTypeDescription.Replace("<" + t.DotNetName + ">", "<" + dbPrefix + t.DotNetName + ">"), SqlName = t.SqlName, SupportsDDLEditing = isSqlServer && t.SupportsDDLEditing, Children = this.GetChildItems(t, isSqlServer) }).ToList<ExplorerItem>();
        }

        private IEnumerable<ExplorerItem> GetParams(SchemaObject o)
        {
            return (from p in o.Parameters select new ExplorerItem(p.ClrName + " (" + p.ClrType.FormatTypeName() + ")", ExplorerItemKind.Parameter, ExplorerIcon.Parameter) { DragText = p.ClrName, SqlName = p.ParamName });
        }

        private IEnumerable<ExplorerItem> GetParents(Table t)
        {
            return (from p in t.ParentRelations
                orderby p.PropNameForChild
                select new ExplorerItem(p.PropNameForChild, ExplorerItemKind.ReferenceLink, p.IsOneToOne ? ExplorerIcon.OneToOne : ExplorerIcon.ManyToOne) { DragText = p.PropNameForChild, ToolTipText = p.ParentTable.DotNetName, Tag = p.ParentTable });
        }

        private Relationship GetRelationship(IDictionary<string, Relationship> existingRelationships, ColumnAssociation ca)
        {
            Relationship relationship;
            if (!existingRelationships.TryGetValue(ca.RelationshipName, out relationship))
            {
                SchemaObject obj2;
                SchemaObject obj3;
                if (!(((this.Objects.TryGetValue(ca.ChildSchema + "." + ca.ChildTable, out obj2) && this.Objects.TryGetValue(ca.ParentSchema + "." + ca.ParentTable, out obj3)) && obj2.HasKey) && obj3.HasKey))
                {
                    return null;
                }
                if (!((obj3 is Table) && (obj2 is Table)))
                {
                    return null;
                }
                relationship = new Relationship(ca.RelationshipName, (Table) obj3, (Table) obj2, Program.AllowOneToOne);
                existingRelationships.Add(ca.RelationshipName, relationship);
            }
            Column item = relationship.ChildTable.Columns.Values.FirstOrDefault<Column>(c => c.ColumnName == ca.ChildColumn);
            Column column2 = relationship.ParentTable.Columns.Values.FirstOrDefault<Column>(c => c.ColumnName == ca.ParentColumn);
            if ((item != null) && (column2 != null))
            {
                relationship.ChildCols.Add(item);
                relationship.ParentCols.Add(column2);
            }
            return relationship;
        }

        protected void Populate()
        {
            if (!this._populated)
            {
                this._populated = true;
                this.ReadColumns(this._columns);
                this.AssignColumnPropertyNames();
                this.ReadAssociations(this._associations);
                this.ReadRoutineParameters(this._routineParameters);
                HashSet<string> set = new HashSet<string>(from o in this.Objects.Values select o.DotNetName);
                List<Database> list = new List<Database>();
                if (this._linkedDatabases != null)
                {
                    string[] source = (from db in this._linkedDatabases
                        where !string.IsNullOrEmpty(db.CatalogName)
                        group db by (db.SystemSchema == null) ? ((IEnumerable<string>) db.CatalogName) : ((IEnumerable<string>) db.SystemSchema) into g
                        where g.Count<Database>() > 1
                        select g.Key).ToArray<string>();
                    foreach (Database database in this._linkedDatabases)
                    {
                        if (database.SystemSchema != null)
                        {
                            database.ClrName = database.SystemSchema;
                        }
                        else
                        {
                            string str = this.TransformIdentifier(database.ServerName);
                            string str3 = this.TransformIdentifier(database.CatalogName);
                            if (!(!source.Contains<string>(str3) || string.IsNullOrEmpty(str)))
                            {
                                str3 = str + "_" + str3;
                            }
                            database.ClrName = DotNetNameBank.ToDotNetName(str3);
                            if (!(string.IsNullOrEmpty(database.ClrName) || !set.Contains(database.ClrName)))
                            {
                                database.ClrName = database.ClrName + "Db";
                            }
                        }
                        if (!(string.IsNullOrEmpty(database.ClrName) || set.Contains(database.ClrName)))
                        {
                            list.Add(database);
                        }
                    }
                    this.LinkedDatabases = list.ToArray();
                }
            }
        }

        private void ReadAssociations(IEnumerable<ColumnAssociation> associations)
        {
            Dictionary<string, Relationship> existingRelationships = new Dictionary<string, Relationship>();
            foreach (ColumnAssociation association in associations)
            {
                Relationship relationship = this.GetRelationship(existingRelationships, association);
                if (relationship != null)
                {
                    int num = 0;
                    foreach (Column column in relationship.ChildCols)
                    {
                        Column column2 = relationship.ParentCols[num++];
                        if (column2.Object.SingularName == null)
                        {
                            column2.Object.SingularName = StringUtil.GetSingularParentName(column2.Object.DotNetName, column.PropertyName);
                        }
                    }
                }
            }
            foreach (SchemaObject obj2 in this.Objects.Values)
            {
                if (((obj2.IsPluralizable && (obj2.SingularName == null)) && obj2.DotNetName.EndsWith("s", StringComparison.Ordinal)) && obj2.HasKey)
                {
                    if (new string[] { "authors", "publishers", "stores" }.Contains<string>(obj2.DotNetName.ToLowerInvariant()))
                    {
                        obj2.SingularName = obj2.DotNetName.Substring(0, obj2.DotNetName.Length - 1);
                    }
                    else
                    {
                        foreach (Column column3 in obj2.Columns.Values)
                        {
                            if (column3.IsKey)
                            {
                                obj2.SingularName = StringUtil.GetSingularParentName(obj2.DotNetName, column3.PropertyName);
                                if (obj2.SingularName != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            using (Dictionary<string, Relationship>.ValueCollection.Enumerator enumerator5 = existingRelationships.Values.GetEnumerator())
            {
                Func<Relationship, bool> predicate = null;
                Func<Relationship, bool> func2 = null;
                Func<Relationship, bool> func3 = null;
                Func<Relationship, bool> func4 = null;
                Func<Column, bool> func5 = null;
                Func<Column, bool> func6 = null;
                Relationship r;
                while (enumerator5.MoveNext())
                {
                    r = enumerator5.Current;
                    if ((r.ChildCols.Count > 0) && (r.ParentCols.Count > 0))
                    {
                        r.AssignPropNames(this);
                        if ((r.PropNameForChild.Length > 0) && (r.PropNameForParent.Length > 0))
                        {
                            if (predicate == null)
                            {
                                predicate = rel => rel.PropNameForChild == r.PropNameForChild;
                            }
                            if (!r.ChildTable.ParentRelations.Any<Relationship>(predicate))
                            {
                                if (func2 == null)
                                {
                                    func2 = rel => rel.PropNameForParent == r.PropNameForChild;
                                }
                                if (!r.ChildTable.ChildRelations.Any<Relationship>(func2))
                                {
                                    if (func3 == null)
                                    {
                                        func3 = rel => rel.PropNameForParent == r.PropNameForParent;
                                    }
                                    if (!r.ParentTable.ChildRelations.Any<Relationship>(func3))
                                    {
                                        if (func4 == null)
                                        {
                                            func4 = rel => rel.PropNameForChild == r.PropNameForParent;
                                        }
                                        if (!r.ParentTable.ParentRelations.Any<Relationship>(func4))
                                        {
                                            if (func5 == null)
                                            {
                                                func5 = c => c.PropertyName == r.PropNameForChild;
                                            }
                                            if (!r.ChildTable.Columns.Values.Any<Column>(func5))
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!((func6 != null) || r.ParentTable.Columns.Values.Any<Column>((func6 = c => c.PropertyName == r.PropNameForParent))))
                        {
                            r.ChildTable.ParentRelations.Add(r);
                            r.ParentTable.ChildRelations.Add(r);
                        }
                    }
                }
            }
        }

        private void ReadColumns(IEnumerable<Column> columns)
        {
            string str2;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            List<Column> list = new List<Column>(0x1000);
            foreach (Column column in columns)
            {
                string str;
                list.Add(column);
                if (dictionary.TryGetValue(column.ObjectName, out str))
                {
                    if ((str != null) && (str != column.SchemaName))
                    {
                        dictionary[column.ObjectName] = null;
                    }
                }
                else
                {
                    dictionary.Add(column.ObjectName, column.SchemaName);
                }
            }
            DotNetNameBank bank = new DotNetNameBank();
            foreach (Column column in list)
            {
                str2 = dictionary[column.ObjectName];
                string sqlName = (((str2 != null) || (column.SchemaName == "dbo")) || (column.SchemaName == "")) ? column.ObjectName : (column.SchemaName + "_" + column.ObjectName);
                bank.RegisterName(sqlName);
            }
            HashSet<string> set = new HashSet<string>();
            foreach (Column column in list)
            {
                if (dictionary.TryGetValue(column.ObjectName, out str2))
                {
                    string str4 = (((str2 != null) || (column.SchemaName == "dbo")) || (column.SchemaName == "")) ? column.ObjectName : (column.SchemaName + "_" + column.ObjectName);
                    string uniqueDotNetName = bank.GetUniqueDotNetName(str4);
                    if (uniqueDotNetName != null)
                    {
                        if ((column.ObjectKind == DbObjectKind.Table) || (column.ObjectKind == DbObjectKind.View))
                        {
                            uniqueDotNetName = this.TransformIdentifier(uniqueDotNetName);
                        }
                        string key = column.SchemaName + "." + column.ObjectName;
                        if (!this.Objects.ContainsKey(key))
                        {
                            if (set.Contains(uniqueDotNetName))
                            {
                                continue;
                            }
                            set.Add(uniqueDotNetName);
                            SchemaObject obj2 = SchemaObject.Create(column, uniqueDotNetName);
                            this.Objects.Add(key, obj2);
                        }
                        SchemaObject obj3 = this.Objects[key];
                        if (column.IsKey)
                        {
                            obj3.HasKey = true;
                        }
                        column.ClrObjectName = uniqueDotNetName;
                        column.Object = obj3;
                        obj3.Columns[column.ColumnID] = column;
                    }
                }
            }
            foreach (SchemaObject obj2 in this.Objects.Values)
            {
                obj2.OriginalName = obj2.PropertyName;
                if (this.Pluralize && obj2.IsPluralizable)
                {
                    string pluralName = StringUtil.GetPluralName(obj2.PropertyName);
                    if (!set.Contains(pluralName))
                    {
                        obj2.PropertyName = pluralName;
                    }
                }
            }
        }

        private void ReadRoutineParameters(IEnumerable<Parameter> routineParameters)
        {
            if (routineParameters != null)
            {
                SchemaObject obj2 = null;
                string str = null;
                foreach (Parameter parameter in routineParameters)
                {
                    string key = parameter.RoutineSchema + "." + parameter.RoutineName;
                    if (key != str)
                    {
                        if (!this.Objects.TryGetValue(key, out obj2))
                        {
                            obj2 = null;
                        }
                        str = key;
                    }
                    if (obj2 != null)
                    {
                        if (!parameter.IsValid)
                        {
                            this.Objects.Remove(key);
                        }
                        else if (parameter.IsResult)
                        {
                            obj2.ReturnInfo = parameter;
                        }
                        else
                        {
                            obj2.Parameters.Add(parameter);
                        }
                    }
                }
                foreach (SchemaObject obj3 in this.Objects.Values)
                {
                    DotNetNameBank bank = new DotNetNameBank();
                    foreach (Parameter parameter2 in obj3.Parameters)
                    {
                        bank.RegisterName(parameter2.ParamName);
                    }
                    foreach (Parameter parameter2 in obj3.Parameters.ToArray<Parameter>())
                    {
                        string uniqueDotNetName = bank.GetUniqueDotNetName(parameter2.ParamName);
                        if (uniqueDotNetName == null)
                        {
                            obj2.Parameters.Remove(parameter2);
                        }
                        else
                        {
                            parameter2.ClrName = uniqueDotNetName;
                        }
                    }
                }
            }
        }

        public virtual string TransformIdentifier(string identifier)
        {
            if (this.Capitalize)
            {
                identifier = StringUtil.Pascal(identifier);
            }
            return identifier;
        }

        internal bool Capitalize
        {
            get
            {
                return !this._schemaOptions.NoCapitalization;
            }
        }

        public string ClrName { get; private set; }

        internal bool ExcludeRoutines
        {
            get
            {
                return this._schemaOptions.ExcludeRoutines;
            }
        }

        public Database[] LinkedDatabases { get; private set; }

        internal bool Pluralize
        {
            get
            {
                return !this._schemaOptions.NoPluralization;
            }
        }

        public IEnumerable<Table> Tables
        {
            get
            {
                return this.Objects.Values.OfType<Table>();
            }
        }
    }
}

