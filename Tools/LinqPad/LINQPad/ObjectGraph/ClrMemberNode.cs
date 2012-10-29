namespace LINQPad.ObjectGraph
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Data.Linq.Mapping;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class ClrMemberNode : MemberNode
    {
        public ClrMemberNode(ObjectNode parent, object item, int maxDepth, DataContextDriver dcDriver) : base(parent, item, maxDepth, dcDriver)
        {
            base.Name = item.GetType().FormatTypeName();
            Type type = item.GetType();
            if (!((base.CyclicReference != null) || base.IsAtNestingLimit()))
            {
                base.InitiallyHidden = ((((item is MemberInfo) || (item is RuntimeMethodHandle)) || ((item is CultureInfo) || (item is ProcessModule))) || (((item is Uri) || (item is Version)) || ((type.Namespace == "Microsoft.SqlServer.Types") || (type.FullName == "System.Data.EntityKey")))) || ((type.Namespace != null) && (type.Namespace.StartsWith("System.Reflection", StringComparison.Ordinal) || type.Namespace.StartsWith("System.IO", StringComparison.Ordinal)));
            }
            if (item is Type)
            {
                base.Name = "typeof(" + ((Type) item).Name + ")";
            }
            if (!base.Name.StartsWith("{", StringComparison.Ordinal))
            {
                if ((item is MethodBase) && (((MethodBase) item).DeclaringType != null))
                {
                    MethodBase base2 = (MethodBase) item;
                    string[] strArray = new string[] { base2.DeclaringType.FormatTypeName(), ".", base2.Name, " (", string.Join(", ", (from p in base2.GetParameters() select p.ParameterType.FormatTypeName() + " " + p.Name).ToArray<string>()), ")" };
                    base.Summary = string.Concat(strArray);
                }
                else
                {
                    try
                    {
                        base.Summary = item.ToString();
                    }
                    catch
                    {
                    }
                }
            }
            if (base.Summary.Length > 150)
            {
                base.Summary = base.Summary.Substring(0, 150) + "...";
            }
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (((fields.Length != 0) || (properties.Length != 0)) && (base.CyclicReference == null))
            {
                if (!((!base.IsAtNestingLimit() || Util.IsMetaGraphNode(item)) || (base.Parent is ListNode)))
                {
                    base.GraphTruncated = true;
                }
                else
                {
                    object obj2;
                    bool isAnonType = base.IsAnonType;
                    Func<object> getter = null;
                    foreach (FieldInfo fi in fields)
                    {
                        if (isAnonType || (((fi.GetCustomAttributes(typeof(AssociationAttribute), true).Length == 0) && !fi.FieldType.FullName.StartsWith("System.Data.Objects.DataClasses.EntityReference")) && !IsUnloadedEntityAssociation(item, fi)))
                        {
                            if (getter == null)
                            {
                                getter = () => fi.GetValue(item);
                            }
                            obj2 = this.GetValue(fi, getter, isAnonType);
                            base.Members.Add(new MemberData(fi.Name, fi.FieldType, ObjectNode.Create(this, obj2, item is Exception, maxDepth, dcDriver)));
                        }
                    }
                    foreach (PropertyInfo pi in properties)
                    {
                        if ((pi.GetIndexParameters().Length == 0) && (isAnonType || ((((pi.GetCustomAttributes(typeof(AssociationAttribute), true).Length == 0) && !pi.PropertyType.FullName.StartsWith("System.Data.Objects.DataClasses.EntityReference")) && ((pi.PropertyType.FullName != "System.Data.EntityKey") && (pi.PropertyType.FullName != "System.Data.EntityState"))) && !IsUnloadedEntityAssociation(item, pi))))
                        {
                            bool exceptionThrown = false;
                            obj2 = this.GetValue(pi, () => this.GetPropValue(pi, item, out exceptionThrown), isAnonType);
                            bool flag2 = exceptionThrown && ((item is Exception) || ((parent != null) && (parent.ObjectValue is Exception)));
                            base.Members.Add(new MemberData(pi.Name, pi.PropertyType, ObjectNode.Create(this, obj2, item is Exception, flag2 ? 1 : maxDepth, dcDriver)));
                        }
                    }
                    if ((base.Members.Count > 50) && (base.NestingDepth > 1))
                    {
                        base.InitiallyHidden = true;
                    }
                }
            }
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }

        private object GetPropValue(PropertyInfo pi, object item, out bool exceptionThrown)
        {
            exceptionThrown = false;
            try
            {
                return pi.GetValue(item, null);
            }
            catch (Exception innerException)
            {
                exceptionThrown = true;
                if ((innerException is TargetInvocationException) && (innerException.InnerException != null))
                {
                    innerException = innerException.InnerException;
                }
                return innerException;
            }
        }

        private object GetValue(MemberInfo mi, Func<object> getter, bool expandAssocations)
        {
            return (((mi.GetCustomAttributes(typeof(AssociationAttribute), true).Length <= 0) || expandAssocations) ? getter() : "<Association>");
        }

        private static bool IsUnloadedEntityAssociation(object item, MemberInfo fieldOrProp)
        {
            if (fieldOrProp is PropertyInfo)
            {
                Type propertyType = ((PropertyInfo) fieldOrProp).PropertyType;
            }
            else
            {
                if (!(fieldOrProp is FieldInfo))
                {
                    return false;
                }
                Type fieldType = ((FieldInfo) fieldOrProp).FieldType;
            }
            if (!fieldOrProp.GetCustomAttributes(true).Any<object>(a => (a.GetType().Name == "EdmRelationshipNavigationPropertyAttribute")))
            {
                return false;
            }
            return EFHelper.IsUnloadedEntityAssociation(item, fieldOrProp);
        }
    }
}

