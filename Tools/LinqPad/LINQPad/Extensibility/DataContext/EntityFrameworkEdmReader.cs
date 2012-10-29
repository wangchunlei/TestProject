namespace LINQPad.Extensibility.DataContext
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Linq;

    internal class EntityFrameworkEdmReader
    {
        private static EntityContainer GetContainer(ObjectContext objectContext)
        {
            return objectContext.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace).First<EntityContainer>();
        }

        public static List<ExplorerItem> GetSchema(ObjectContext objectContext)
        {
            List<ExplorerItem> source = (from es in GetContainer(objectContext).BaseEntitySets
                where ((es.BuiltInTypeKind == BuiltInTypeKind.EntitySet) && (es.ElementType is EntityType)) && (es.ElementType.FullName != "CodeFirstNamespace.EdmMetadata")
                let et = (EntityType) es.ElementType
                orderby es.Name
                select new ExplorerItem(es.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table) { DragText = es.Name, ToolTipText = et.Name, IsEnumerable = true, Tag = es.ElementType, Children = (from p in et.Properties select new ExplorerItem(p.Name + " (" + p.TypeUsage.EdmType.Name + (p.Nullable ? "?" : "") + ")", ExplorerItemKind.Property, et.KeyMembers.Contains(p) ? ExplorerIcon.Key : ExplorerIcon.Column) { DragText = p.Name }).Concat<ExplorerItem>((from p in et.NavigationProperties
                    let fromMany = p.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                    let toMany = p.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                    select new ExplorerItem(p.Name, toMany ? ExplorerItemKind.CollectionLink : ExplorerItemKind.ReferenceLink, (!fromMany || !toMany) ? ((fromMany || toMany) ? ((!fromMany || toMany) ? ExplorerIcon.OneToMany : ExplorerIcon.ManyToOne) : ExplorerIcon.OneToOne) : ExplorerIcon.ManyToMany) { ToolTipText = p.ToEndMember.GetEntityType().Name, Tag = p.ToEndMember.GetEntityType() })).ToList<ExplorerItem>() }).ToList<ExplorerItem>();
            foreach (ExplorerItem item in source)
            {
                using (IEnumerator<ExplorerItem> enumerator2 = (from c in item.Children
                    where c.Tag != null
                    select c).GetEnumerator())
                {
                    ExplorerItem child;
                    while (enumerator2.MoveNext())
                    {
                        child = enumerator2.Current;
                        child.HyperlinkTarget = source.FirstOrDefault<ExplorerItem>(r => r.Tag == child.Tag);
                    }
                }
            }
            return source.ToList<ExplorerItem>();
        }
    }
}

