namespace LINQPad.Expressions
{
    using System;
    using System.Text;

    internal class LeafExpressionToken : ExpressionToken
    {
        public readonly string Text;

        public LeafExpressionToken(string text)
        {
            this.Text = text;
        }

        public override void Write(StringBuilder sb, int indent)
        {
            sb.Append(this.Text);
        }

        public override int Length
        {
            get
            {
                return this.Text.Length;
            }
        }

        public override bool MultiLine
        {
            get
            {
                return false;
            }
            set
            {
            }
        }
    }
}

