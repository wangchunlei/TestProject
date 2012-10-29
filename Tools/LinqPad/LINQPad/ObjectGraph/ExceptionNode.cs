namespace LINQPad.ObjectGraph
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class ExceptionNode : ClrMemberNode
    {
        private static Regex _binaryKiller = new Regex(@"[\u0000-\u001F]");

        public ExceptionNode(ObjectNode parent, Exception ex, int maxDepth) : base(parent, ex, maxDepth, null)
        {
            List<MemberData> list = new List<MemberData>();
            foreach (MemberData data in base.Members)
            {
                string objectValue;
                if (data.Name == "StackTrace")
                {
                    objectValue = data.Value.ObjectValue as string;
                    if (objectValue != null)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (string str2 in objectValue.Split(new char[] { '\n' }))
                        {
                            string str3 = str2.Trim();
                            if (!str3.StartsWith("at UserQuery.", StringComparison.Ordinal) && !str3.StartsWith("at LINQPad.", StringComparison.Ordinal))
                            {
                                if (builder.Length > 0)
                                {
                                    builder.AppendLine();
                                }
                                builder.Append(_binaryKiller.Replace(str2, ""));
                            }
                        }
                        data.Value = new SimpleNode(this, builder.ToString());
                    }
                }
                else if (data.Name == "Source")
                {
                    objectValue = data.Value.ObjectValue as string;
                    if (objectValue != null)
                    {
                        if (objectValue.StartsWith("query_", StringComparison.Ordinal))
                        {
                            list.Add(data);
                        }
                        else
                        {
                            data.Value = new SimpleNode(this, _binaryKiller.Replace(objectValue, ""));
                        }
                    }
                }
                else if (data.Name == "TargetSite")
                {
                    MethodInfo info = data.Value.ObjectValue as MethodInfo;
                    if ((info != null) && !(((info.DeclaringType == null) || !(info.DeclaringType.Name == "UserQuery")) ? !_binaryKiller.IsMatch(info.ToString()) : false))
                    {
                        list.Add(data);
                    }
                }
            }
            foreach (MemberData data in list)
            {
                base.Members.Remove(data);
            }
            base.Summary = _binaryKiller.Replace(ex.Message, "");
            if (base.Summary.Length > 150)
            {
                base.Summary = base.Summary.Substring(0, 150) + "...";
            }
            base.InitiallyHidden = ex.InnerException == null;
        }

        public override object Accept(IObjectGraphVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

