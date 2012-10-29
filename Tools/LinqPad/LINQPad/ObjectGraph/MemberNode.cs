namespace LINQPad.ObjectGraph
{
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal abstract class MemberNode : ObjectNode
    {
        public readonly List<MemberData> Members;

        public MemberNode(ObjectNode parent, object item, int maxDepth, DataContextDriver dcDriver) : base(parent, item, maxDepth, dcDriver)
        {
            this.Members = new List<MemberData>();
            this.Summary = "";
        }

        public string Name { get; protected set; }

        public string Summary { get; protected set; }
    }
}

