namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;

    internal class LinqToSqlDriver : StaticDataContextDriver
    {
        public override void ClearConnectionPools(IConnectionInfo c)
        {
            if (c.DatabaseInfo.IsSqlServer)
            {
                try
                {
                    using (SqlConnection connection = (SqlConnection) this.GetIDbConnection(c))
                    {
                        SqlConnection.ClearPool(connection);
                    }
                }
                catch
                {
                }
            }
        }

        public override string GetConnectionDescription(IConnectionInfo r)
        {
            return null;
        }

        internal override Type GetContextBaseType()
        {
            return typeof(DataContext);
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo r)
        {
            return new object[] { DataContextBase.GetConnection(r.DatabaseInfo.GetCxString(), r.DatabaseInfo.Provider) };
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo r)
        {
            return new ParameterDescriptor[] { new ParameterDescriptor("connection", "System.Data.IDbConnection") };
        }

        internal override string GetImageKey(IConnectionInfo r)
        {
            return "L2S";
        }

        private IEnumerable<ExplorerItem> GetRoutines(Type contextType)
        {
            return (from <>h__TransparentIdentifier3d in from mi in contextType.GetMethods()
                let funcAtt = mi.GetCustomAttributes(true).OfType<FunctionAttribute>().FirstOrDefault<FunctionAttribute>()
                where funcAtt != null
                let isFunc = funcAtt.IsComposable
                let isTableFunc = (isFunc && mi.ReturnType.IsGenericType) && (mi.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                let isTableProc = (!isFunc && mi.ReturnType.IsGenericType) && (mi.ReturnType.GetGenericTypeDefinition() == typeof(ISingleResult<>))
                select new { <>h__TransparentIdentifier3c = <>h__TransparentIdentifier3c, isSimpleRetType = mi.ReturnType.Assembly == typeof(int).Assembly }
                group new ExplorerItem { Text = <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.<>h__TransparentIdentifier39.mi.Name + (<>h__TransparentIdentifier3d.isSimpleRetType ? (" -> " + <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.<>h__TransparentIdentifier39.mi.ReturnType.FormatTypeName()) : ""), ToolTipText = <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.<>h__TransparentIdentifier39.funcAtt.Name ?? <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.<>h__TransparentIdentifier39.mi.Name, DragText = <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.<>h__TransparentIdentifier39.mi.Name + "(" + (<>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.<>h__TransparentIdentifier39.mi.GetParameters().Any<ParameterInfo>() ? " " : "") + ")", Kind = ExplorerItemKind.QueryableObject, IsEnumerable = <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.isTableFunc, Icon = <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.isTableFunc ? ExplorerIcon.TableFunction : (<>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.isFunc ? ExplorerIcon.ScalarFunction : ExplorerIcon.StoredProc), Children = (from param in <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.<>h__TransparentIdentifier39.mi.GetParameters() select new ExplorerItem { Text = param.Name + " (" + param.ParameterType.FormatTypeName() + ")", DragText = param.Name, Icon = ExplorerIcon.Parameter, Kind = ExplorerItemKind.Parameter }).Union<ExplorerItem>((from col in (<>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.isTableFunc || <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.isTableProc) ? ((IEnumerable<PropertyInfo>) <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.<>h__TransparentIdentifier39.mi.ReturnType.GetGenericArguments()[0].GetProperties()) : ((IEnumerable<PropertyInfo>) new PropertyInfo[0])
                    let colAtt = col.GetCustomAttributes(true).OfType<ColumnAttribute>().FirstOrDefault<ColumnAttribute>()
                    where colAtt != null
                    select new ExplorerItem { Text = col.Name + " (" + col.PropertyType.FormatTypeName() + ")", DragText = col.Name, Icon = ExplorerIcon.Column, Kind = ExplorerItemKind.Property })).ToList<ExplorerItem>() } by <>h__TransparentIdentifier3d.<>h__TransparentIdentifier3c.<>h__TransparentIdentifier3b.<>h__TransparentIdentifier3a.isFunc into g
                orderby g.Key
                select new ExplorerItem { Text = g.Key ? "Functions" : "Stored Procedures", Icon = g.Key ? ExplorerIcon.ScalarFunction : ExplorerIcon.StoredProc, Children = (from m in g
                    orderby m.Text
                    select m).ToList<ExplorerItem>() });
        }

        public override List<ExplorerItem> GetSchema(IConnectionInfo r, Type customType)
        {
            return this.GetTables(customType).Union<ExplorerItem>(this.GetRoutines(customType)).ToList<ExplorerItem>();
        }

        private IEnumerable<ExplorerItem> GetTables(Type contextType)
        {
            var source = (from pi in contextType.GetProperties()
                where pi.PropertyType.IsGenericType
                where typeof(Table<>).IsAssignableFrom(pi.PropertyType.GetGenericTypeDefinition())
                let genArg = pi.PropertyType.GetGenericArguments()
                where genArg.Length == 1
                orderby pi.Name.ToUpperInvariant()
                let tableProps = (from p in genArg[0].GetProperties() select new { Name = p.Name, MemberType = p.PropertyType, Attributes = p.GetCustomAttributes(true) }).Union(from f in genArg[0].GetFields() select new { Name = f.Name, MemberType = f.FieldType, Attributes = f.GetCustomAttributes(true) })
                select new { TableType = genArg[0], MemberInfo = new ExplorerItem { Text = pi.Name, ToolTipText = pi.PropertyType.FormatTypeName(), DragText = pi.Name, Kind = ExplorerItemKind.QueryableObject, Icon = ExplorerIcon.Table, IsEnumerable = true, Children = (from col in tableProps
                    let colAtt = col.Attributes.OfType<ColumnAttribute>().FirstOrDefault<ColumnAttribute>()
                    where colAtt != null
                    select new ExplorerItem { Text = col.Name + " (" + col.MemberType.FormatTypeName() + ")", DragText = col.Name, Icon = colAtt.IsPrimaryKey ? ExplorerIcon.Key : ExplorerIcon.Column, Kind = ExplorerItemKind.Property }).ToList<ExplorerItem>() }, AssociatedChildren = (from col in tableProps
                    let assAtt = col.Attributes.OfType<AssociationAttribute>().FirstOrDefault<AssociationAttribute>()
                    where assAtt != null
                    orderby col.MemberType.IsGenericType
                    select new { MemberType = col.MemberType.IsGenericType ? col.MemberType.GetGenericArguments()[0] : col.MemberType, AssociationName = assAtt.Name, MemberInfo = new ExplorerItem { Text = col.Name, DragText = col.Name, ToolTipText = col.MemberType.FormatTypeName(), Icon = col.MemberType.IsGenericType ? ExplorerIcon.OneToMany : ExplorerIcon.ManyToOne, Kind = col.MemberType.IsGenericType ? ExplorerItemKind.CollectionLink : ExplorerItemKind.ReferenceLink } }).ToList() }).ToList();
            ILookup<Type, ExplorerItem> lookup = source.ToLookup(m => m.TableType, m => m.MemberInfo);
            HashSet<string> set = new HashSet<string>(from c in from t in source select t.AssociatedChildren
                where c.MemberInfo.Kind == ExplorerItemKind.ReferenceLink
                group c by c.AssociationName into g
                where g.Count() > 1
                select g.Key);
            foreach (var typee in source)
            {
                foreach (var typed in typee.AssociatedChildren)
                {
                    if (lookup.Contains(typed.MemberType))
                    {
                        typed.MemberInfo.HyperlinkTarget = lookup[typed.MemberType].First<ExplorerItem>();
                    }
                    if (set.Contains(typed.AssociationName))
                    {
                        typed.MemberInfo.Icon = ExplorerIcon.OneToOne;
                        ExplorerItem memberInfo = typed.MemberInfo;
                        memberInfo.ToolTipText = memberInfo.ToolTipText + " (1:1 relationship)";
                    }
                }
            }
            foreach (var typee in source)
            {
                typee.MemberInfo.Children.AddRange(from c in typee.AssociatedChildren select c.MemberInfo);
            }
            return (from t in source select t.MemberInfo);
        }

        public override bool ShowConnectionDialog(IConnectionInfo repository, bool isNewRepository)
        {
            using (CxForm form = new CxForm((Repository) repository, isNewRepository))
            {
                return (form.ShowDialog() == DialogResult.OK);
            }
        }

        public override string Author
        {
            get
            {
                return "(built in)";
            }
        }

        internal override string ContextBaseTypeName
        {
            get
            {
                return "System.Data.Linq.DataContext";
            }
        }

        internal override string InternalID
        {
            get
            {
                return "LinqToSql";
            }
        }

        internal override int InternalSortOrder
        {
            get
            {
                return 10;
            }
        }

        internal override bool IsBuiltIn
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "LINQ to SQL";
            }
        }
    }
}

