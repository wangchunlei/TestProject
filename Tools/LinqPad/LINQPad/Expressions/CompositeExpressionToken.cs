namespace LINQPad.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class CompositeExpressionToken : ExpressionToken
    {
        private int? _length;
        private bool? _multiLine;
        public bool AddCommas;
        public readonly List<ExpressionToken> Tokens;

        public CompositeExpressionToken()
        {
            this.Tokens = new List<ExpressionToken>();
        }

        public CompositeExpressionToken(IEnumerable<ExpressionToken> tokens, bool addCommas)
        {
            this.Tokens = new List<ExpressionToken>();
            this.Tokens.AddRange(from t in tokens
                where t != null
                select t);
            this.AddCommas = addCommas;
            if (addCommas)
            {
                foreach (ExpressionToken token in this.Tokens)
                {
                    if (token != null)
                    {
                        token.Splittable = true;
                    }
                }
            }
        }

        public void AddStringToken(string s)
        {
            this.AddStringToken(s, false);
        }

        public void AddStringToken(string s, bool splittable)
        {
            LeafExpressionToken item = new LeafExpressionToken(s) {
                Splittable = splittable
            };
            this.Tokens.Add(item);
        }

        public override void Write(StringBuilder sb, int indent)
        {
            bool flag = true;
            foreach (ExpressionToken token in this.Tokens)
            {
                if (token != null)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else if (this.AddCommas)
                    {
                        sb.Append(", ");
                    }
                    if (this.MultiLine && token.Splittable)
                    {
                        base.WriteNewLine(sb, indent + token.SplitIndent);
                    }
                    token.Write(sb, indent + token.SplitIndent);
                }
            }
        }

        public override int Length
        {
            get
            {
                if (!this._length.HasValue)
                {
                    this._length = new int?(this.Tokens.Sum<ExpressionToken>((Func<ExpressionToken, int>) (t => t.Length)));
                }
                return this._length.Value;
            }
        }

        public override bool MultiLine
        {
            get
            {
                if (!this._multiLine.HasValue)
                {
                    if ((this.Length <= 90) && (this.Tokens.Count<ExpressionToken>() <= 5))
                    {
                    }
                    this._multiLine = new bool?((CS$<>9__CachedAnonymousMethodDelegate6 != null) || this.Tokens.Any<ExpressionToken>(CS$<>9__CachedAnonymousMethodDelegate6));
                }
                return this._multiLine.Value;
            }
            set
            {
                this._multiLine = new bool?(value);
            }
        }
    }
}

