namespace LINQPad.Lexers
{
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
    using System;

    public class FSharpDynamicSyntaxLanguage : DynamicOutliningSyntaxLanguage
    {
        public FSharpDynamicSyntaxLanguage()
        {
        }

        public FSharpDynamicSyntaxLanguage(string key, bool secure) : base(key, secure)
        {
        }

        public override void GetTokenOutliningAction(TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction)
        {
            string str = tokenStream.Peek().get_Key();
            switch (str)
            {
                case null:
                    break;

                case "OpenCurlyBraceToken":
                    outliningKey = "CodeBlock";
                    tokenAction = 1;
                    break;

                case "CloseCurlyBraceToken":
                    outliningKey = "CodeBlock";
                    tokenAction = 2;
                    break;

                default:
                    if (!(str == "MultiLineCommentStartToken"))
                    {
                        if (str == "MultiLineCommentEndToken")
                        {
                            outliningKey = "MultiLineComment";
                            tokenAction = 2;
                        }
                    }
                    else
                    {
                        outliningKey = "MultiLineComment";
                        tokenAction = 1;
                    }
                    break;
            }
        }

        public override void ResetLineCommentDelimiter()
        {
            base.set_LineCommentDelimiter("//");
        }

        public override bool ShouldSerializeLineCommentDelimiter()
        {
            return (base.get_LineCommentDelimiter() != "//");
        }
    }
}

