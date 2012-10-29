namespace LINQPad.Expressions
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal abstract class ExpressionToken
    {
        private static Dictionary<ExpressionType, string> _binaryExpressionStrings = new Dictionary<ExpressionType, string>();
        private static Dictionary<Type, MethodInfo> _visitMethods;
        public int SplitIndent;
        public bool Splittable;

        static ExpressionToken()
        {
            ExpressionType[] typeArray = new ExpressionType[0x17];
            typeArray[1] = ExpressionType.AddChecked;
            typeArray[2] = ExpressionType.And;
            typeArray[3] = ExpressionType.AndAlso;
            typeArray[4] = ExpressionType.Coalesce;
            typeArray[5] = ExpressionType.Divide;
            typeArray[6] = ExpressionType.Equal;
            typeArray[7] = ExpressionType.ExclusiveOr;
            typeArray[8] = ExpressionType.GreaterThan;
            typeArray[9] = ExpressionType.GreaterThanOrEqual;
            typeArray[10] = ExpressionType.LeftShift;
            typeArray[11] = ExpressionType.LessThan;
            typeArray[12] = ExpressionType.LessThanOrEqual;
            typeArray[13] = ExpressionType.Modulo;
            typeArray[14] = ExpressionType.Multiply;
            typeArray[15] = ExpressionType.MultiplyChecked;
            typeArray[0x10] = ExpressionType.NotEqual;
            typeArray[0x11] = ExpressionType.Or;
            typeArray[0x12] = ExpressionType.OrElse;
            typeArray[0x13] = ExpressionType.Power;
            typeArray[20] = ExpressionType.RightShift;
            typeArray[0x15] = ExpressionType.Subtract;
            typeArray[0x16] = ExpressionType.SubtractChecked;
            ExpressionType[] typeArray2 = typeArray;
            string[] strArray = "+ + & && ?? . == ^ > >= << < <= % * * != | || ^ >> - -".Split(new char[0]);
            int num = 0;
            foreach (ExpressionType type in typeArray2)
            {
                _binaryExpressionStrings[type] = strArray[num++];
            }
            _visitMethods = (from exType in typeof(Expression).Assembly.GetTypes()
                where exType.IsSubclassOf(typeof(Expression)) || exType.IsSubclassOf(typeof(MemberBinding))
                select exType into exType
                join method in from m in typeof(ExpressionToken).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                    where m.Name == "Visit"
                    select m on exType equals method.GetParameters()[0].ParameterType
                select new { exType = exType, method = method }).ToDictionary(t => t.exType, t => t.method);
        }

        protected ExpressionToken()
        {
        }

        private static string CleanIdentifier(string name)
        {
            if (name == null)
            {
                return null;
            }
            if (name.StartsWith("<>h__TransparentIdentifier", StringComparison.Ordinal))
            {
                return ("temp" + name.Substring(0x1a));
            }
            return name;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            this.Write(sb, 0);
            return sb.ToString();
        }

        private static ExpressionToken Visit(BinaryExpression exp)
        {
            string str;
            CompositeExpressionToken body = new CompositeExpressionToken();
            ExpressionToken item = Visit(exp.Left);
            ExpressionToken token3 = Visit(exp.Right);
            if (exp.NodeType == ExpressionType.ArrayIndex)
            {
                body.Tokens.Add(item);
                body.Tokens.Add(new BracketedExpressionToken("[", "]", token3));
                return body;
            }
            if (_binaryExpressionStrings.TryGetValue(exp.NodeType, out str))
            {
                body.Tokens.Add(item);
                body.AddStringToken(" " + str + " ");
                body.Tokens.Add(token3);
                token3.Splittable = true;
                token3.SplitIndent = 1;
                return new BracketedExpressionToken("(", ")", true, body);
            }
            return body;
        }

        private static ExpressionToken Visit(ConditionalExpression exp)
        {
            CompositeExpressionToken token = new CompositeExpressionToken {
                Tokens = { Visit(exp.Test) }
            };
            token.AddStringToken(" ? ", true);
            token.Tokens.Add(Visit(exp.IfTrue));
            token.AddStringToken(" : ", true);
            token.Tokens.Add(Visit(exp.IfFalse));
            return token;
        }

        private static ExpressionToken Visit(ConstantExpression exp)
        {
            Func<PropertyInfo, bool> predicate = null;
            if (((exp.Value != null) && exp.Type.IsGenericType) && (exp.Type.GetGenericTypeDefinition() == typeof(Table<>)))
            {
                PropertyInfo property = exp.Value.GetType().GetProperty("Context");
                if (property != null)
                {
                    object obj2 = property.GetValue(exp.Value, null);
                    if (obj2 != null)
                    {
                        if (predicate == null)
                        {
                            predicate = p => p.PropertyType == exp.Type;
                        }
                        PropertyInfo info2 = obj2.GetType().GetProperties().First<PropertyInfo>(predicate);
                        if (info2 != null)
                        {
                            return new LeafExpressionToken(info2.Name);
                        }
                    }
                }
            }
            else if (((exp.Value != null) && exp.Type.IsGenericType) && ((exp.Type.FullName.StartsWith("System.Data.Objects.ObjectQuery`1", StringComparison.Ordinal) || exp.Type.FullName.StartsWith("System.Data.Objects.ObjectSet`1", StringComparison.Ordinal)) || exp.Type.FullName.StartsWith("System.Data.Services.Client.DataServiceQuery`1")))
            {
                return new LeafExpressionToken(exp.Type.GetGenericArguments()[0].Name);
            }
            return new LeafExpressionToken((exp.Value == null) ? "null" : ((exp.Value is string) ? ('"' + ((string) exp.Value) + '"') : exp.Value.ToString()));
        }

        public static ExpressionToken Visit(Expression expr)
        {
            return Visit(expr);
        }

        private static ExpressionToken Visit(InvocationExpression exp)
        {
            CompositeExpressionToken token = new CompositeExpressionToken();
            token.AddStringToken("Invoke" + ((exp.Arguments.Count == 0) ? "" : " "), true);
            token.Tokens.Add(new BracketedExpressionToken("(", ")", new CompositeExpressionToken(from a in exp.Arguments select Visit(a), true)));
            return token;
        }

        private static ExpressionToken Visit(LambdaExpression exp)
        {
            CompositeExpressionToken token = new CompositeExpressionToken();
            string s = "";
            if (exp.Parameters.Count != 1)
            {
                s = "(";
            }
            s = s + string.Join(", ", (from p in exp.Parameters select CleanIdentifier(p.Name)).ToArray<string>());
            if (exp.Parameters.Count != 1)
            {
                s = s + ")";
            }
            s = s + " => ";
            token.AddStringToken(s);
            ExpressionToken item = Visit(exp.Body);
            if (item != null)
            {
                item.Splittable = true;
                item.SplitIndent = 1;
                token.Tokens.Add(item);
            }
            return token;
        }

        private static ExpressionToken Visit(ListInitExpression exp)
        {
            CompositeExpressionToken token = new CompositeExpressionToken {
                Tokens = { Visit(exp.NewExpression) }
            };
            CompositeExpressionToken body = new CompositeExpressionToken {
                AddCommas = true
            };
            foreach (ElementInit init in exp.Initializers)
            {
                body.Tokens.Add(new BracketedExpressionToken("(", ")", true, new CompositeExpressionToken(from a in init.Arguments select Visit(a), true)));
            }
            token.Tokens.Add(new BracketedExpressionToken(" { ", " } ", body));
            return token;
        }

        private static ExpressionToken Visit(MemberAssignment mb)
        {
            CompositeExpressionToken token = new CompositeExpressionToken();
            token.AddStringToken(CleanIdentifier(mb.Member.Name) + " = ");
            token.Tokens.Add(Visit(mb.Expression));
            token.Splittable = true;
            return token;
        }

        private static ExpressionToken Visit(MemberExpression exp)
        {
            CompositeExpressionToken token = new CompositeExpressionToken();
            ConstantExpression expression = exp.Expression as ConstantExpression;
            if (!((((expression == null) || (expression.Value == null)) || (!expression.Value.GetType().IsNested || !expression.Value.GetType().Name.StartsWith("<", StringComparison.Ordinal))) ? ((expression == null) || !(expression.Value is DataContext)) : false))
            {
                return new LeafExpressionToken(exp.Member.Name);
            }
            if (exp.Expression != null)
            {
                token.Tokens.Add(Visit(exp.Expression));
            }
            else
            {
                token.AddStringToken(exp.Member.DeclaringType.Name);
            }
            token.AddStringToken("." + CleanIdentifier(exp.Member.Name));
            return token;
        }

        private static ExpressionToken Visit(MemberInitExpression exp)
        {
            CompositeExpressionToken token = new CompositeExpressionToken();
            if (exp.NewExpression.Type.IsAnonymous())
            {
                token.AddStringToken("new ");
            }
            else
            {
                token.Tokens.Add(Visit(exp.NewExpression));
            }
            CompositeExpressionToken body = new CompositeExpressionToken(from b in exp.Bindings select Visit(b), true) {
                MultiLine = true
            };
            BracketedExpressionToken item = new BracketedExpressionToken("{", "}", body) {
                NewLineBefore = true
            };
            token.Tokens.Add(item);
            return token;
        }

        private static ExpressionToken Visit(MemberListBinding mb)
        {
            return null;
        }

        private static ExpressionToken Visit(MemberMemberBinding mb)
        {
            return null;
        }

        private static ExpressionToken Visit(MethodCallExpression exp)
        {
            Func<PropertyInfo, bool> predicate = null;
            bool flag = Attribute.IsDefined(exp.Method, typeof(ExtensionAttribute));
            string name = exp.Method.Name;
            CompositeExpressionToken token = new CompositeExpressionToken();
            if (flag)
            {
                if (exp.Method.DeclaringType == typeof(Queryable))
                {
                    token.MultiLine = true;
                }
                token.Tokens.Add(Visit(exp.Arguments[0]));
                token.AddStringToken("." + name + " ", true);
                token.Tokens.Last<ExpressionToken>().SplitIndent = 1;
                CompositeExpressionToken token2 = new CompositeExpressionToken(from a in exp.Arguments.Skip<Expression>(1) select Visit(a), true);
                if ((exp.Method.DeclaringType == typeof(Queryable)) && (exp.Arguments.Count<Expression>() > 2))
                {
                    token2.MultiLine = true;
                }
                token2.SplitIndent = 1;
                token.Tokens.Add(new BracketedExpressionToken("(", ")", token2));
                return token;
            }
            if (exp.Object == null)
            {
                token.AddStringToken(exp.Method.DeclaringType.FormatTypeName());
            }
            else
            {
                token.Tokens.Add(Visit(exp.Object));
            }
            if (exp.Method.IsSpecialName)
            {
                if (predicate == null)
                {
                    predicate = p => p.GetAccessors().Contains<MethodInfo>(exp.Method);
                }
                if (exp.Method.DeclaringType.GetProperties().Where<PropertyInfo>(predicate).FirstOrDefault<PropertyInfo>() != null)
                {
                    token.Tokens.Add(new BracketedExpressionToken(" [", "]", new CompositeExpressionToken(from a in exp.Arguments select Visit(a), true)));
                    return token;
                }
            }
            token.AddStringToken("." + name + ((exp.Arguments.Count == 0) ? "" : " "), true);
            token.Tokens.Last<ExpressionToken>().SplitIndent = 1;
            CompositeExpressionToken body = new CompositeExpressionToken(from a in exp.Arguments select Visit(a), true) {
                SplitIndent = 1
            };
            token.Tokens.Add(new BracketedExpressionToken("(", ")", body));
            return token;
        }

        private static ExpressionToken Visit(NewArrayExpression exp)
        {
            bool flag = exp.NodeType == ExpressionType.NewArrayInit;
            CompositeExpressionToken token = new CompositeExpressionToken();
            bool flag2 = exp.Type.IsAnonymous();
            Type t = exp.Type;
            if (t.IsArray)
            {
                t = t.GetElementType();
            }
            string str = flag2 ? "" : t.FormatTypeName();
            string s = "new " + str;
            if (flag && exp.Expressions.Any<Expression>())
            {
                s = s + "[] ";
            }
            else if (flag)
            {
                s = s + "[0]";
            }
            token.AddStringToken(s);
            if (!flag || exp.Expressions.Any<Expression>())
            {
                token.Tokens.Add(new BracketedExpressionToken(flag ? "{ " : "[", flag ? " } " : "]", new CompositeExpressionToken(from a in exp.Expressions select Visit(a), true)));
            }
            return token;
        }

        private static ExpressionToken Visit(NewExpression exp)
        {
            CompositeExpressionToken token = new CompositeExpressionToken();
            Type declaringType = exp.Type;
            if (exp.Constructor != null)
            {
                declaringType = exp.Constructor.DeclaringType;
            }
            token.AddStringToken("new " + (declaringType.IsAnonymous() ? "" : declaringType.FormatTypeName()) + ((exp.Arguments.Count == 0) ? "" : " "));
            if ((exp.Members == null) || (exp.Members.Count == 0))
            {
                CompositeExpressionToken token2 = new CompositeExpressionToken(from a in exp.Arguments select Visit(a), true);
                token.Tokens.Add(new BracketedExpressionToken("(", ")", token2));
                return token;
            }
            int num = 0;
            CompositeExpressionToken body = new CompositeExpressionToken {
                MultiLine = true,
                AddCommas = true
            };
            foreach (Expression expression in exp.Arguments)
            {
                MemberInfo mi = exp.Members[num++];
                PropertyInfo info = mi as PropertyInfo;
                if (info == null)
                {
                    info = mi.DeclaringType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault<PropertyInfo>(p => p.GetAccessors().Contains<MemberInfo>(mi));
                }
                if (info != null)
                {
                    CompositeExpressionToken token5 = new CompositeExpressionToken();
                    token5.AddStringToken(CleanIdentifier(info.Name) + " = ");
                    token5.Tokens.Add(Visit(expression));
                    token5.Splittable = true;
                    body.Tokens.Add(token5);
                }
            }
            BracketedExpressionToken item = new BracketedExpressionToken("{", "}", body) {
                NewLineBefore = true
            };
            token.Tokens.Add(item);
            return token;
        }

        private static ExpressionToken Visit(ParameterExpression exp)
        {
            string name = exp.Name ?? "<param>";
            return new LeafExpressionToken(CleanIdentifier(name));
        }

        private static ExpressionToken Visit(TypeBinaryExpression exp)
        {
            CompositeExpressionToken body = new CompositeExpressionToken {
                Tokens = { Visit(exp.Expression) }
            };
            body.AddStringToken(" is ");
            body.AddStringToken(exp.TypeOperand.FormatTypeName());
            return new BracketedExpressionToken("(", ")", body);
        }

        private static ExpressionToken Visit(UnaryExpression exp)
        {
            if (exp.NodeType == ExpressionType.Quote)
            {
                return Visit(exp.Operand);
            }
            CompositeExpressionToken token2 = new CompositeExpressionToken();
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    token2.AddStringToken("-");
                    break;

                case ExpressionType.UnaryPlus:
                    token2.AddStringToken("+");
                    break;

                case ExpressionType.Not:
                    token2.AddStringToken("!");
                    break;

                case ExpressionType.Convert:
                    if (exp.Operand.Type.IsSubclassOf(exp.Type))
                    {
                        return Visit(exp.Operand);
                    }
                    token2.AddStringToken("(" + exp.Type.FormatTypeName() + ")");
                    break;

                case ExpressionType.TypeAs:
                    token2.Tokens.Add(Visit(exp.Operand));
                    token2.AddStringToken(" as ");
                    token2.AddStringToken(exp.Type.FormatTypeName());
                    return token2;

                default:
                    token2.AddStringToken(exp.NodeType.ToString());
                    break;
            }
            token2.Tokens.Add(new BracketedExpressionToken("(", ")", true, Visit(exp.Operand)));
            return token2;
        }

        private static ExpressionToken Visit(object expr)
        {
            MethodInfo info;
            if (expr == null)
            {
                return new LeafExpressionToken("null");
            }
            Type key = expr.GetType();
            while (!key.IsPublic)
            {
                key = key.BaseType;
            }
            if (_visitMethods.TryGetValue(key, out info) || ((key.BaseType != null) && _visitMethods.TryGetValue(key.BaseType, out info)))
            {
                return (ExpressionToken) info.Invoke(null, new object[] { expr });
            }
            if (key.FullName == "System.Data.Services.Client.ResourceSetExpression")
            {
                PropertyInfo property = key.GetProperty("MemberExpression", BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null)
                {
                    ConstantExpression expression = property.GetValue(expr, null) as ConstantExpression;
                    if ((expression != null) && (expression.Value != null))
                    {
                        return new LeafExpressionToken(expression.Value.ToString());
                    }
                }
            }
            return null;
        }

        public abstract void Write(StringBuilder sb, int indent);
        protected void WriteNewLine(StringBuilder sb, int indent)
        {
            // This item is obfuscated and can not be translated.
            int num = 0;
            for (int i = sb.Length - 1; i <= 0; i--)
            {
            Label_000E:
                if (0 == 0)
                {
                    if ((i > 0) && (sb[i] == '\n'))
                    {
                        int length = num - (indent * 3);
                        if (((length == 0) || ((length > 0) && (sb.Remove(sb.Length - length, length) || true))) || (sb.Append("".PadRight(-length)) || true))
                        {
                            return;
                        }
                    }
                    sb.Append("\r\n".PadRight(2 + (indent * 3)));
                    return;
                }
                num++;
            }
            goto Label_000E;
        }

        public abstract int Length { get; }

        public abstract bool MultiLine { get; set; }
    }
}

