namespace LINQPad.ObjectGraph
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal class CustomMemberProviderNode : MemberNode
    {
        public CustomMemberProviderNode(ObjectNode parent, object item, int maxDepth, DataContextDriver dcDriver, bool useDataContextDriver) : base(parent, item, maxDepth, dcDriver)
        {
            base.Name = item.GetType().FormatTypeName();
            if (base.IsAtNestingLimit())
            {
                base.GraphTruncated = true;
            }
            else
            {
                IEnumerable<string> names;
                IEnumerable<Type> types;
                IEnumerable<object> values;
                if (useDataContextDriver)
                {
                    ICustomMemberProvider customDisplayMemberProvider = dcDriver.GetCustomDisplayMemberProvider(item);
                    names = customDisplayMemberProvider.GetNames();
                    types = customDisplayMemberProvider.GetTypes();
                    values = customDisplayMemberProvider.GetValues();
                }
                else
                {
                    Type type2 = item.GetType().GetInterfaces().First<Type>(t => t.FullName == typeof(ICustomMemberProvider).FullName);
                    MethodInfo method = type2.GetMethod("GetNames");
                    MethodInfo info2 = type2.GetMethod("GetTypes");
                    MethodInfo info3 = type2.GetMethod("GetValues");
                    names = (IEnumerable<string>) method.Invoke(item, null);
                    types = (IEnumerable<Type>) info2.Invoke(item, null);
                    values = (IEnumerable<object>) info3.Invoke(item, null);
                }
                IEnumerator<Type> enumerator = types.GetEnumerator();
                enumerator.MoveNext();
                IEnumerator<object> enumerator2 = values.GetEnumerator();
                enumerator2.MoveNext();
                foreach (string str in names)
                {
                    object current;
                    try
                    {
                        current = enumerator2.Current;
                    }
                    catch (Exception innerException)
                    {
                        if ((innerException is TargetInvocationException) && (innerException.InnerException != null))
                        {
                            innerException = innerException.InnerException;
                        }
                        current = innerException;
                    }
                    base.Members.Add(new MemberData(str, enumerator.Current, ObjectNode.Create(this, current, maxDepth, dcDriver)));
                    enumerator.MoveNext();
                    enumerator2.MoveNext();
                }
                if ((base.Members.Count > 50) && (base.NestingDepth > 1))
                {
                    base.InitiallyHidden = true;
                }
            }
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

