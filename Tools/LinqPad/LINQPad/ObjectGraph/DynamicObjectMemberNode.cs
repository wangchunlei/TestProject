namespace LINQPad.ObjectGraph
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;

    internal class DynamicObjectMemberNode : MemberNode
    {
        public DynamicObjectMemberNode(ObjectNode parent, DynamicObject item, int maxDepth, DataContextDriver dcDriver) : base(parent, item, maxDepth, dcDriver)
        {
            base.Name = "DynamicObject";
            base.Summary = item.ToString();
            if (base.Summary.Length > 150)
            {
                base.Summary = base.Summary.Substring(0, 150) + "...";
            }
            IEnumerable<string> dynamicMemberNames = item.GetDynamicMemberNames();
            if (dynamicMemberNames.Any<string>() && (base.CyclicReference == null))
            {
                if (!(!base.IsAtNestingLimit() || (base.Parent is ListNode)))
                {
                    base.GraphTruncated = true;
                }
                else
                {
                    foreach (string str in dynamicMemberNames)
                    {
                        object propValue = this.GetPropValue(item, str);
                        base.Members.Add(new MemberData(str, null, ObjectNode.Create(this, propValue, false, maxDepth, dcDriver)));
                    }
                }
            }
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }

        private object GetPropValue(DynamicObject item, string propName)
        {
            try
            {
                object obj2;
                item.TryGetMember(new MyGetMemberBinder(propName, false), out obj2);
                return obj2;
            }
            catch (Exception innerException)
            {
                if ((innerException is TargetInvocationException) && (innerException.InnerException != null))
                {
                    innerException = innerException.InnerException;
                }
                return innerException;
            }
        }

        private class MyGetMemberBinder : GetMemberBinder
        {
            public MyGetMemberBinder(string name, bool ignoreCase) : base(name, ignoreCase)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }
    }
}

