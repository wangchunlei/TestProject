namespace LINQPad.ObjectGraph
{
    using System;

    internal interface IObjectGraphVisitor
    {
        object Visit(EmptyNode g);
        object Visit(ListNode g);
        object Visit(MemberNode g);
        object Visit(MultiDimArrayNode g);
        object Visit(SimpleNode g);
    }
}

