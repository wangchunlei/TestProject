namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.EntityClient;
    using System.Data.Objects;
    using System.Linq;
    using System.Reflection;

    internal static class EntityFrameworkHelper
    {
        internal static void ExecuteESqlQuery(string cxString, string query)
        {
            EntityConnection connection = new EntityConnection(cxString);
            EntityCommand command = new EntityCommand(query, connection);
            connection.Open();
            try
            {
                command.ExecuteReader(CommandBehavior.SequentialAccess).Dump<EntityDataReader>();
            }
            finally
            {
                connection.Close();
            }
        }

        private static IEnumerable<ExplorerItem> GetRoutines(Type contextType)
        {
            return (from mi in from mi in contextType.GetMethods()
                where mi.DeclaringType != typeof(ObjectContext)
                where mi.ReturnType.IsGenericType
                where typeof(ObjectResult<>).IsAssignableFrom(mi.ReturnType.GetGenericTypeDefinition())
                select mi
                group new ExplorerItem { Text = mi.Name, ToolTipText = mi.Name, DragText = mi.Name + "(" + (mi.GetParameters().Any<ParameterInfo>() ? " " : "") + ")", Kind = ExplorerItemKind.QueryableObject, Icon = ExplorerIcon.StoredProc, Children = (from param in mi.GetParameters() select new ExplorerItem { Text = param.Name + " (" + param.ParameterType.FormatTypeName() + ")", DragText = param.Name, Icon = ExplorerIcon.Parameter, Kind = ExplorerItemKind.Parameter }).Union<ExplorerItem>((from col in mi.ReturnType.GetGenericArguments()[0].GetProperties()
                    let colAtt = col.GetCustomAttributes(true).OfType<EdmScalarPropertyAttribute>().FirstOrDefault<EdmScalarPropertyAttribute>()
                    where colAtt != null
                    select new ExplorerItem { Text = col.Name + " (" + col.PropertyType.FormatTypeName() + ")", DragText = col.Name, Icon = ExplorerIcon.Column, Kind = ExplorerItemKind.Property })).ToList<ExplorerItem>() } by false into g
                orderby g.Key
                select new ExplorerItem { Text = g.Key ? "Functions" : "Stored Procedures", Icon = g.Key ? ExplorerIcon.ScalarFunction : ExplorerIcon.StoredProc, Children = (from m in g
                    orderby m.Text
                    select m).ToList<ExplorerItem>() });
        }

        internal static List<ExplorerItem> GetSchema(Type contextType)
        {
            return GetTables(contextType).Union<ExplorerItem>(GetRoutines(contextType)).ToList<ExplorerItem>();
        }

        private static IEnumerable<ExplorerItem> GetTables(Type contextType)
        {
            var source = (from pi in contextType.GetProperties()
                where pi.PropertyType.IsGenericType
                where typeof(ObjectQuery<>).IsAssignableFrom(pi.PropertyType.GetGenericTypeDefinition()) || typeof(ObjectSet<>).IsAssignableFrom(pi.PropertyType.GetGenericTypeDefinition())
                let genArg = pi.PropertyType.GetGenericArguments()
                where genArg.Length == 1
                orderby pi.Name.ToUpperInvariant()
                let tableProps = (from p in genArg[0].GetProperties() select new { Name = p.Name, MemberType = p.PropertyType, Attributes = p.GetCustomAttributes(true) }).Union(from f in genArg[0].GetFields() select new { Name = f.Name, MemberType = f.FieldType, Attributes = f.GetCustomAttributes(true) })
                select new { TableType = genArg[0], MemberInfo = new ExplorerItem { Text = pi.Name, ToolTipText = pi.PropertyType.FormatTypeName(), DragText = pi.Name, Kind = ExplorerItemKind.QueryableObject, Icon = ExplorerIcon.Table, IsEnumerable = true, Children = (from col in tableProps
                    let colAtt = col.Attributes.OfType<EdmScalarPropertyAttribute>().FirstOrDefault<EdmScalarPropertyAttribute>()
                    where colAtt != null
                    select new ExplorerItem { Text = col.Name + " (" + col.MemberType.FormatTypeName() + ")", DragText = col.Name, Icon = colAtt.EntityKeyProperty ? ExplorerIcon.Key : ExplorerIcon.Column, Kind = ExplorerItemKind.Property }).ToList<ExplorerItem>() }, AssociatedChildren = (from col in tableProps
                    let assAtt = col.Attributes.OfType<EdmRelationshipNavigationPropertyAttribute>().FirstOrDefault<EdmRelationshipNavigationPropertyAttribute>()
                    where assAtt != null
                    orderby col.MemberType.IsGenericType
                    select new { MemberType = col.MemberType.IsGenericType ? col.MemberType.GetGenericArguments()[0] : col.MemberType, AssociationName = assAtt.RelationshipName, MemberInfo = new ExplorerItem { Text = col.Name, DragText = col.Name, ToolTipText = col.MemberType.FormatTypeName(), Icon = col.MemberType.IsGenericType ? ExplorerIcon.OneToMany : ExplorerIcon.ManyToOne, Kind = col.MemberType.IsGenericType ? ExplorerItemKind.CollectionLink : ExplorerItemKind.ReferenceLink } }).ToList() }).ToList();
            ILookup<Type, ExplorerItem> lookup = source.ToLookup(m => m.TableType, m => m.MemberInfo);
            HashSet<string> set = new HashSet<string>(from c in from t in source select t.AssociatedChildren
                where c.MemberInfo.Kind == ExplorerItemKind.ReferenceLink
                group c by c.AssociationName into g
                where g.Count() > 1
                select g.Key);
            HashSet<string> set2 = new HashSet<string>(from c in from t in source select t.AssociatedChildren
                where c.MemberInfo.Kind == ExplorerItemKind.CollectionLink
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
                    else if (set2.Contains(typed.AssociationName))
                    {
                        typed.MemberInfo.Icon = ExplorerIcon.ManyToMany;
                        ExplorerItem local2 = typed.MemberInfo;
                        local2.ToolTipText = local2.ToolTipText + " (many:many relationship)";
                    }
                }
            }
            foreach (var typee in source)
            {
                typee.MemberInfo.Children.AddRange(from c in typee.AssociatedChildren select c.MemberInfo);
            }
            return (from t in source select t.MemberInfo);
        }
    }
}

