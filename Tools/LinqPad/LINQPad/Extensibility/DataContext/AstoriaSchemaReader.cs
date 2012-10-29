namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    internal class AstoriaSchemaReader
    {
        private Dictionary<string, Dictionary<string, XElement>> associations;
        private XNamespace edm;
        private XNamespace edmx;
        private XElement edmxNode;

        public AstoriaSchemaReader(XDocument data)
        {
            var collectionSelector = null;
            var predicate = null;
            var elementSelector = null;
            this.edmxNode = data.Root.Elements().FirstOrDefault<XElement>(e => (e.Name.LocalName == "DataServices") && e.Name.Namespace.NamespaceName.StartsWith("http://schemas.microsoft.com"));
            if (this.edmxNode == null)
            {
                throw new DisplayToUserException("No DataServices element present");
            }
            this.edmx = this.edmxNode.Name.Namespace;
            XElement element = this.edmxNode.Elements().FirstOrDefault<XElement>(e => (e.Name.LocalName == "Schema") && e.Name.Namespace.NamespaceName.StartsWith("http://schemas.microsoft.com"));
            if (element == null)
            {
                throw new DisplayToUserException("No Schema element present");
            }
            this.edm = element.Name.Namespace;
            if (typeof(DataServiceContext).Assembly.GetType("System.Data.Services.Client.DataServiceCollection`1") == null)
            {
                Match match = Regex.Match(this.edm.NamespaceName, @"(\d{4})/(\d{2})/edm$");
                if (match.Success)
                {
                    int num = (int.Parse(match.Groups[1].Value) * 100) + int.Parse(match.Groups[2].Value);
                    if (num > 0x31001)
                    {
                        throw new DisplayToUserException("Schema version requires .NET Framework 4.0 support. Download LINQPad 4.0 or later.");
                    }
                }
            }
            if (collectionSelector == null)
            {
                collectionSelector = <>h__TransparentIdentifier0 => <>h__TransparentIdentifier0.schema.Elements((XName) (this.edm + "Association"));
            }
            if (predicate == null)
            {
                predicate = delegate (<>f__AnonymousTypec<<>f__AnonymousTypeb<XElement, string>, XElement> <>h__TransparentIdentifier1) {
                    if (<>h__TransparentIdentifier1.association.Attribute("Name") != null)
                    {
                    }
                    return (CS$<>9__CachedAnonymousMethodDelegate16 == null) && <>h__TransparentIdentifier1.association.Elements((XName) (this.edm + "End")).Any<XElement>(CS$<>9__CachedAnonymousMethodDelegate16);
                };
            }
            if (elementSelector == null)
            {
                elementSelector = a => a.association.Elements((XName) (this.edm + "End")).ToDictionary<XElement, string>(e => e.Attribute("Role").Value);
            }
            this.associations = (from <>h__TransparentIdentifier1 in (from schema in this.edmxNode.Elements((XName) (this.edm + "Schema")) select new { schema = schema, schemaNS = ((string) schema.Attribute("Namespace")) ?? "" }).SelectMany(collectionSelector, (<>h__TransparentIdentifier0, association) => new { <>h__TransparentIdentifier0 = <>h__TransparentIdentifier0, association = association }).Where(predicate) select new { schemaNS = <>h__TransparentIdentifier1.<>h__TransparentIdentifier0.schemaNS, association = <>h__TransparentIdentifier1.association }).ToDictionary(a => a.schemaNS + "." + a.association.Attribute("Name").Value, elementSelector);
        }

        private IEnumerable<ExplorerItem> GetAssociatedChildren(XElement entityType)
        {
            return (from prop in entityType.Elements((XName) (this.edm + "NavigationProperty"))
                where (((prop.Attribute("Name") != null) && (prop.Attribute("Relationship") != null)) && (prop.Attribute("FromRole") != null)) && (prop.Attribute("ToRole") != null)
                let propertyName = prop.Attribute("Name").Value
                let association = this.associations[prop.Attribute("Relationship").Value]
                let fromRole = association[prop.Attribute("FromRole").Value]
                let toRole = association[prop.Attribute("ToRole").Value]
                let fromMultiplicity = fromRole.Attribute("Multiplicity").Value
                let toMultiplicity = toRole.Attribute("Multiplicity").Value
                let icon = (!fromMultiplicity.Contains("1") || !toMultiplicity.Contains("1")) ? (((fromMultiplicity != "*") || (toMultiplicity != "*")) ? ((fromMultiplicity == "*") ? ((IEnumerable<ExplorerItem>) ExplorerIcon.ManyToOne) : ((IEnumerable<ExplorerItem>) ExplorerIcon.OneToMany)) : ((IEnumerable<ExplorerItem>) ExplorerIcon.ManyToMany)) : ((IEnumerable<ExplorerItem>) ExplorerIcon.OneToOne)
                orderby icon, propertyName
                select new ExplorerItem { Text = propertyName, DragText = propertyName, ToolTipText = (((string) toRole.Attribute("Type")) ?? "").Split(new char[] { '.' }).Last<string>(), Icon = icon, Kind = ((((ExplorerIcon) icon) == ExplorerIcon.OneToMany) || (((ExplorerIcon) icon) == ExplorerIcon.ManyToMany)) ? ExplorerItemKind.CollectionLink : ExplorerItemKind.ReferenceLink, Tag = (string) toRole.Attribute("Type") });
        }

        public List<ExplorerItem> GetSchema(out string typeName, out string nameSpace)
        {
            XNamespace dr = "http://schemas.microsoft.com/dallas/2010/04";
            var source = (from <>h__TransparentIdentifier1a in (from schema in this.edmxNode.Elements((XName) (this.edm + "Schema"))
                let schemaNS = schema.Attribute("Namespace").Value ?? ""
                from entityType in schema.Elements((XName) (this.edm + "EntityType"))
                select new { <>h__TransparentIdentifier18 = <>h__TransparentIdentifier18, entityType = entityType }).Select(delegate (<>f__AnonymousTypee<<>f__AnonymousTypeb<XElement, string>, XElement> <>h__TransparentIdentifier19) {
                if ((<>h__TransparentIdentifier19.entityType.Element((XName) (this.edm + "Key")) != null) && (CS$<>9__CachedAnonymousMethodDelegate48 == null))
                {
                    CS$<>9__CachedAnonymousMethodDelegate48 = e => (string) e.Attribute("Name");
                }
                return new { <>h__TransparentIdentifier19 = <>h__TransparentIdentifier19, keys = (CS$<>9__CachedAnonymousMethodDelegate49 != null) ? new HashSet<string>() : new HashSet<string>(<>h__TransparentIdentifier19.entityType.Element((XName) (this.edm + "Key")).Elements((XName) (this.edm + "PropertyRef")).Select<XElement, string>(CS$<>9__CachedAnonymousMethodDelegate48).Where<string>(CS$<>9__CachedAnonymousMethodDelegate49)) };
            }) select new { SchemaNamespace = <>h__TransparentIdentifier1a.<>h__TransparentIdentifier19.<>h__TransparentIdentifier18.schemaNS, EntityTypeName = <>h__TransparentIdentifier1a.<>h__TransparentIdentifier19.entityType.Attribute("Name").Value, Children = (from <>h__TransparentIdentifier1d in (from prop in <>h__TransparentIdentifier1a.<>h__TransparentIdentifier19.entityType.Elements((XName) (this.edm + "Property"))
                let propName = prop.Attribute("Name").Value
                select new { <>h__TransparentIdentifier1b = <>h__TransparentIdentifier1b, propTypeName = ((string) prop.Attribute("Type")) ?? "?" }).Select(delegate (<>f__AnonymousType11<<>f__AnonymousType10<XElement, string>, string> <>h__TransparentIdentifier1c) {
                bool? nullable = (bool?) <>h__TransparentIdentifier1c.<>h__TransparentIdentifier1b.prop.Attribute((XName) (dr + "Queryable"));
                return new { <>h__TransparentIdentifier1c = <>h__TransparentIdentifier1c, isQueryable = nullable.HasValue ? nullable.GetValueOrDefault() : true };
            }) select new ExplorerItem { Text = <>h__TransparentIdentifier1d.<>h__TransparentIdentifier1c.<>h__TransparentIdentifier1b.propName + " (" + <>h__TransparentIdentifier1d.<>h__TransparentIdentifier1c.propTypeName.Split(new char[] { '.' }).Last<string>() + (<>h__TransparentIdentifier1d.isQueryable ? "" : ", NoQuery") + ")", DragText = <>h__TransparentIdentifier1d.<>h__TransparentIdentifier1c.<>h__TransparentIdentifier1b.propName, Icon = <>h__TransparentIdentifier1a.keys.Contains(<>h__TransparentIdentifier1d.<>h__TransparentIdentifier1c.<>h__TransparentIdentifier1b.propName) ? ExplorerIcon.Key : (<>h__TransparentIdentifier1d.isQueryable ? ExplorerIcon.Column : ExplorerIcon.Blank), Kind = ExplorerItemKind.Property }).ToList<ExplorerItem>(), AssociatedChildren = this.GetAssociatedChildren(<>h__TransparentIdentifier1a.<>h__TransparentIdentifier19.entityType).ToList<ExplorerItem>() }).ToList();
            var entitiesDictionary = source.ToDictionary(e => e.SchemaNamespace + "." + e.EntityTypeName);
            XElement element = (from c in this.edmxNode.Elements((XName) (this.edm + "Schema")).Elements<XElement>((XName) (this.edm + "EntityContainer"))
                let isDefaultAtt = c.Attributes().FirstOrDefault<XAttribute>(a => a.Name.LocalName == "IsDefaultEntityContainer")
                let attOrder = ((isDefaultAtt == null) || !((bool) isDefaultAtt)) ? ((IEnumerable<XElement>) 2) : ((IEnumerable<XElement>) 1)
                orderby attOrder
                select c).FirstOrDefault<XElement>();
            if (element == null)
            {
                throw new DisplayToUserException("No EntityContainer present");
            }
            typeName = element.Attribute("Name").Value;
            nameSpace = source.Any() ? source.First().SchemaNamespace : "LINQPad.User";
            var list2 = (from entitySet in element.Elements((XName) (this.edm + "EntitySet"))
                let entityType = entitiesDictionary[entitySet.Attribute("EntityType").Value]
                select new { FullTypeName = entityType.SchemaNamespace + "." + entityType.EntityTypeName, UserMemberInfo = new ExplorerItem { Text = entitySet.Attribute("Name").Value, DragText = entitySet.Attribute("Name").Value, ToolTipText = entityType.EntityTypeName, Icon = ExplorerIcon.Table, IsEnumerable = true, Kind = ExplorerItemKind.QueryableObject, Children = entityType.Children.Concat<ExplorerItem>(entityType.AssociatedChildren).ToList<ExplorerItem>() } }).ToList();
            Dictionary<string, ExplorerItem> dictionary = list2.ToDictionary(e => e.FullTypeName, e => e.UserMemberInfo);
            foreach (var type in source)
            {
                foreach (ExplorerItem item in type.AssociatedChildren)
                {
                    if (dictionary.ContainsKey(item.Tag as string))
                    {
                        item.HyperlinkTarget = dictionary[item.Tag as string];
                    }
                }
            }
            return (from es in list2
                orderby es.UserMemberInfo.Text
                select es.UserMemberInfo).ToList<ExplorerItem>();
        }
    }
}

