namespace LINQPad.ObjectGraph
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class ListNode : ObjectNode
    {
        public readonly Dictionary<string, decimal> Counts;
        public readonly Type ElementType;
        public readonly List<ObjectNode> Items;
        public readonly bool ItemsTruncated;
        public readonly string Name;
        public readonly Dictionary<string, decimal> Totals;

        public ListNode(ObjectNode parent, IEnumerable list, int maxDepth, DataContextDriver dcDriver) : this(parent, list, maxDepth, dcDriver, null)
        {
        }

        public ListNode(ObjectNode parent, IEnumerable list, int maxDepth, DataContextDriver dcDriver, string name) : base(parent, list, maxDepth, dcDriver)
        {
            this.Items = new List<ObjectNode>();
            this.Totals = new Dictionary<string, decimal>();
            this.Counts = new Dictionary<string, decimal>();
            if (name == null)
            {
                this.Name = list.GetType().FormatTypeName();
            }
            else
            {
                this.Name = name;
            }
            this.ElementType = (from itype in list.GetType().GetInterfaces()
                where itype.IsGenericType && (itype.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                select itype.GetGenericArguments().First<Type>()).OrderByDescending<Type, Type>(itype => itype, new SubTypeComparer()).FirstOrDefault<Type>();
            if (base.CyclicReference == null)
            {
                if (base.IsAtNestingLimit() || ((parent != null) && ((parent.ObjectValue is Type) || (parent.ObjectValue is Assembly))))
                {
                    base.GraphTruncated = true;
                }
                else
                {
                    IEnumerator enumerator = list.GetEnumerator();
                    try
                    {
                        int? nullable;
                        ObjectNode node;
                        MemberNode node2;
                        if (!((enumerator != null) && enumerator.MoveNext()))
                        {
                            return;
                        }
                        Type type = (from t in list.GetType().GetInterfaces()
                            where t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                            select t).FirstOrDefault<Type>();
                        if (type != null)
                        {
                            PropertyInfo property = type.GetProperty("Key");
                            if (property != null)
                            {
                                this.GroupKey = ObjectNode.Create(this, property.GetValue(list, null), maxDepth, dcDriver);
                                this.GroupKeyText = "Key";
                            }
                        }
                        int num = 0x3e8;
                        if (((UserOptions.Instance != null) && UserOptions.Instance.MaxQueryRows.HasValue) && (((nullable = UserOptions.Instance.MaxQueryRows).GetValueOrDefault() > 0x3e8) && nullable.HasValue))
                        {
                            num = UserOptions.Instance.MaxQueryRows.Value;
                        }
                        int num2 = 0;
                        goto Label_033B;
                    Label_0215:
                        this.Items.Add(node);
                        if (++num2 == num)
                        {
                            goto Label_0364;
                        }
                        if (enumerator.MoveNext())
                        {
                            goto Label_033B;
                        }
                        return;
                    Label_0244:
                        foreach (MemberData data in node2.Members)
                        {
                            if (((data.Type.IsNumeric() && !ObjectNode.IsKey(data.Name, data.Type)) && (data.Value != null)) && (data.Value.ObjectValue != null))
                            {
                                decimal num3;
                                decimal num4;
                                decimal num5;
                                this.Totals.TryGetValue(data.Name, out num3);
                                this.Counts.TryGetValue(data.Name, out num4);
                                if (decimal.TryParse(data.Value.ObjectValue.ToString(), out num5))
                                {
                                    this.Totals[data.Name] = num3 + num5;
                                    this.Counts[data.Name] = decimal.op_Increment(num4);
                                }
                            }
                        }
                        goto Label_0215;
                    Label_033B:
                        node = ObjectNode.Create(this, enumerator.Current, maxDepth, dcDriver);
                        node2 = node as MemberNode;
                        if (node2 == null)
                        {
                            goto Label_0215;
                        }
                        goto Label_0244;
                    Label_0364:
                        this.ItemsTruncated = true;
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable != null)
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public ObjectNode GroupKey { get; protected set; }

        public string GroupKeyText { get; protected set; }
    }
}

