namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Linq;
    using System.Reflection;

    internal class EntityFrameworkMemberProvider : ICustomMemberProvider
    {
        private string[] _names;
        private Type[] _types;
        private object[] _values;

        public EntityFrameworkMemberProvider(object objectToWrite)
        {
            var selector = null;
            HashSet<string> navProps = new HashSet<string>(this.GetNavPropNames(objectToWrite) ?? ((IEnumerable<string>) new string[0]));
            if (selector == null)
            {
                selector = m => new { Name = m.Name, type = this.GetFieldPropType(m), value = this.GetFieldPropValue(objectToWrite, m) };
            }
            var list = (from m in objectToWrite.GetType().GetMembers()
                where (m.MemberType == MemberTypes.Field) || (m.MemberType == MemberTypes.Property)
                where (((m.MemberType != MemberTypes.Field) || !(m.Name == "_entityWrapper")) && ((m.MemberType != MemberTypes.Property) || !(m.Name == "RelationshipManager"))) ? ((IEnumerable<MemberInfo>) !navProps.Contains(m.Name)) : ((IEnumerable<MemberInfo>) false)
                orderby m.MetadataToken
                select m).Select(selector).ToList();
            this._names = (from q in list select q.Name).ToArray<string>();
            this._types = (from q in list select q.type).ToArray<Type>();
            this._values = (from q in list select q.value).ToArray<object>();
        }

        private Type GetFieldPropType(MemberInfo m)
        {
            if (m is FieldInfo)
            {
                return ((FieldInfo) m).FieldType;
            }
            if (!(m is PropertyInfo))
            {
                throw new InvalidOperationException("Expected FieldInfo or PropertyInfo");
            }
            return ((PropertyInfo) m).PropertyType;
        }

        private object GetFieldPropValue(object value, MemberInfo m)
        {
            try
            {
                if (m is FieldInfo)
                {
                    return ((FieldInfo) m).GetValue(value);
                }
                if (!(m is PropertyInfo))
                {
                    throw new InvalidOperationException("Expected FieldInfo or PropertyInfo");
                }
                return ((PropertyInfo) m).GetValue(value, null);
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        public IEnumerable<string> GetNames()
        {
            return this._names;
        }

        private IEnumerable<string> GetNavPropNames(object entity)
        {
            EntityContainer container;
            EntitySet set;
            if (entity == null)
            {
                return null;
            }
            FieldInfo field = entity.GetType().GetField("_entityWrapper");
            if (field == null)
            {
                return null;
            }
            object obj2 = field.GetValue(entity);
            PropertyInfo property = obj2.GetType().GetProperty("EntityKey");
            if (property == null)
            {
                return null;
            }
            EntityKey key = property.GetValue(obj2, null) as EntityKey;
            if (key == null)
            {
                return null;
            }
            PropertyInfo info3 = obj2.GetType().GetProperty("Context");
            if (info3 == null)
            {
                return null;
            }
            ObjectContext context = info3.GetValue(obj2, null) as ObjectContext;
            if (context == null)
            {
                return null;
            }
            if (!(context.MetadataWorkspace.TryGetItem<EntityContainer>(key.EntityContainerName, DataSpace.CSpace, out container) && (container != null)))
            {
                return null;
            }
            if (!(container.TryGetEntitySetByName(key.EntitySetName, false, out set) && (set != null)))
            {
                return null;
            }
            return (from np in set.ElementType.NavigationProperties select np.Name);
        }

        public IEnumerable<Type> GetTypes()
        {
            return this._types;
        }

        public IEnumerable<object> GetValues()
        {
            return this._values;
        }

        public static bool IsEntity(Type t)
        {
            if ((t.IsValueType || t.IsPrimitive) || t.AssemblyQualifiedName.EndsWith("=b77a5c561934e089"))
            {
                return false;
            }
            if (t.GetField("_entityWrapper") == null)
            {
            }
            return (t.GetField("_entityWrapper") != null);
        }
    }
}

