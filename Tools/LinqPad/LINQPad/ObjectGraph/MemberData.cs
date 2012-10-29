namespace LINQPad.ObjectGraph
{
    using System;

    internal class MemberData
    {
        public readonly string Name;
        public readonly System.Type Type;
        public ObjectNode Value;

        public MemberData(string name, System.Type type, ObjectNode value)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
        }
    }
}

