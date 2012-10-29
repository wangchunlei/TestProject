namespace LINQPad.Lexers
{
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
    using System;

    public class JavaScriptDynamicSyntaxLanguage : DynamicOutliningSyntaxLanguage
    {
        public JavaScriptDynamicSyntaxLanguage()
        {
        }

        public JavaScriptDynamicSyntaxLanguage(string key, bool secure) : base(key, secure)
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

        public override void SetOutliningNodeCollapsedText(OutliningNode node)
        {
            TokenCollection tokens = node.get_Document().get_Tokens();
            int index = tokens.IndexOf(node.get_StartOffset());
            string str = tokens.get_Item(index).get_Key();
            if ((str != null) && (str == "MultiLineCommentStartToken"))
            {
                node.set_CollapsedText("/**/");
            }
        }

        public override bool ShouldSerializeLineCommentDelimiter()
        {
            return (base.get_LineCommentDelimiter() != "//");
        }
    }
}

