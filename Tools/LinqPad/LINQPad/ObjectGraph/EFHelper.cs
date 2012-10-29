namespace LINQPad.ObjectGraph
{
    using System;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    using System.Reflection;

    internal class EFHelper
    {
        public static bool IsUnloadedEntityAssociation(object item, MemberInfo fieldOrProp)
        {
            Type propertyType = null;
            bool flag2;
            if (fieldOrProp is PropertyInfo)
            {
                propertyType = ((PropertyInfo) fieldOrProp).PropertyType;
            }
            else
            {
                if (!(fieldOrProp is FieldInfo))
                {
                    return false;
                }
                propertyType = ((FieldInfo) fieldOrProp).FieldType;
            }
            EdmRelationshipNavigationPropertyAttribute attribute = (EdmRelationshipNavigationPropertyAttribute) fieldOrProp.GetCustomAttributes(typeof(EdmRelationshipNavigationPropertyAttribute), true).FirstOrDefault<object>();
            if (attribute == null)
            {
                return false;
            }
            IEntityWithRelationships relationships = item as IEntityWithRelationships;
            if (relationships == null)
            {
                return true;
            }
            Type type2 = (flag2 = propertyType.IsGenericType && propertyType.Name.StartsWith("EntityCollection")) ? propertyType.GetGenericArguments()[0] : propertyType;
            string str = attribute.RelationshipNamespaceName + "." + attribute.RelationshipName;
            MethodInfo method = typeof(RelationshipManager).GetMethod(flag2 ? "GetRelatedCollection" : "GetRelatedReference", new Type[] { typeof(string), typeof(string) });
            if (method == null)
            {
                return true;
            }
            object obj2 = method.MakeGenericMethod(new Type[] { type2 }).Invoke(relationships.RelationshipManager, new object[] { str, attribute.TargetRoleName });
            if (obj2 == null)
            {
                return true;
            }
            Type type3 = flag2 ? typeof(EntityCollection<>) : typeof(EntityReference<>);
            PropertyInfo property = type3.MakeGenericType(new Type[] { type2 }).GetProperty("IsLoaded");
            return ((property == null) || !((bool) property.GetValue(obj2, null)));
        }
    }
}

