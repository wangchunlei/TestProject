namespace LINQPad.Expressions
{
    using System;
    using System.Linq;
    using System.Text;

    internal class BracketedExpressionToken : ExpressionToken
    {
        public readonly ExpressionToken Body;
        public readonly string CloseBracket;
        public bool NewLineBefore;
        public bool OmitBracketsForSingle;
        public readonly string OpenBracket;

        public BracketedExpressionToken(string openBracket, string closeBracket, ExpressionToken body) : this(openBracket, closeBracket, false, body)
        {
        }

        public BracketedExpressionToken(string openBracket, string closeBracket, bool omitBracketsForSingle, ExpressionToken body)
        {
            this.OpenBracket = openBracket;
            this.CloseBracket = closeBracket;
            this.OmitBracketsForSingle = omitBracketsForSingle;
            this.Body = body;
        }

        public override void Write(StringBuilder sb, int indent)
        {
            bool flag = !(this.Body is CompositeExpressionToken) || (((CompositeExpressionToken) this.Body).Tokens.Count<ExpressionToken>() == 1);
            if (!(this.OmitBracketsForSingle && !this.NewLineBefore))
            {
                flag = false;
            }
            int num = indent;
            if (this.NewLineBefore)
            {
                base.WriteNewLine(sb, indent);
                num = indent;
                sb.Append(this.OpenBracket);
            }
            bool flag2 = ((sb.Length > 2) && char.IsWhiteSpace(sb[sb.Length - 1])) && char.IsWhiteSpace(sb[sb.Length - 2]);
            if (this.Body.MultiLine)
            {
                indent++;
            }
            if (this.NewLineBefore || flag2)
            {
                base.WriteNewLine(sb, indent);
            }
            if (!(this.NewLineBefore || flag))
            {
                sb.Append(this.OpenBracket);
                num = indent;
            }
            this.Body.Write(sb, indent + this.Body.SplitIndent);
            if (this.Body.MultiLine)
            {
                base.WriteNewLine(sb, num);
            }
            if (!flag)
            {
                sb.Append(this.CloseBracket);
            }
        }

        public override int Length
        {
            get
            {
                return ((this.Body.Length + this.OpenBracket.Length) + this.CloseBracket.Length);
            }
        }

        public override bool MultiLine
        {
            get
            {
                return (this.Body.MultiLine || this.NewLineBefore);
            }
            set
            {
            }
        }
    }
}

