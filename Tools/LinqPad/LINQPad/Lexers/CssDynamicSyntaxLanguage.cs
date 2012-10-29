namespace LINQPad.Lexers
{
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
    using System;

    public class CssDynamicSyntaxLanguage : DynamicOutliningSyntaxLanguage
    {
        public CssDynamicSyntaxLanguage()
        {
        }

        public CssDynamicSyntaxLanguage(string key, bool secure) : base(key, secure)
        {
        }

        public override void GetTokenOutliningAction(TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction)
        {
            string str = tokenStream.Peek().get_Key();
            switch (str)
            {
                case null:
                    break;

                case "PropertyStartToken":
                    outliningKey = "PropertyBlock";
                    tokenAction = 1;
                    break;

                case "PropertyEndToken":
                    outliningKey = "PropertyBlock";
                    tokenAction = 2;
                    break;

                default:
                    if (!(str == "CommentStartToken"))
                    {
                        if (str == "CommentEndToken")
                        {
                            outliningKey = "Comment";
                            tokenAction = 2;
                        }
                    }
                    else
                    {
                        outliningKey = "Comment";
                        tokenAction = 1;
                    }
                    break;
            }
        }

        public override void SetOutliningNodeCollapsedText(OutliningNode node)
        {
            if (node.get_ParseData() != null)
            {
                string str = node.get_ParseData().get_Key();
                if (str != null)
                {
                    if (!(str == "Comment"))
                    {
                        if (str == "PropertyBlock")
                        {
                            node.set_CollapsedText("{...}");
                        }
                    }
                    else
                    {
                        node.set_CollapsedText("/**/");
                    }
                }
            }
        }
    }
}

