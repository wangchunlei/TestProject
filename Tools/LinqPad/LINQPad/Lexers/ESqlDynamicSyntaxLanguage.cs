namespace LINQPad.Lexers
{
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
    using System;

    public class ESqlDynamicSyntaxLanguage : DynamicOutliningSyntaxLanguage
    {
        public ESqlDynamicSyntaxLanguage()
        {
        }

        public ESqlDynamicSyntaxLanguage(string key, bool secure) : base(key, secure)
        {
        }

        public override void GetTokenOutliningAction(TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction)
        {
            string str = tokenStream.Peek().get_Key();
            switch (str)
            {
                case null:
                    break;

                case "MultiLineCommentStartToken":
                    outliningKey = "MultiLineComment";
                    tokenAction = 1;
                    break;

                case "MultiLineCommentEndToken":
                    outliningKey = "MultiLineComment";
                    tokenAction = 2;
                    break;

                default:
                    if (!(str == "RegionStartToken"))
                    {
                        if (str == "EndRegionStartToken")
                        {
                            outliningKey = "Region";
                            tokenAction = 2;
                        }
                    }
                    else
                    {
                        outliningKey = "Region";
                        tokenAction = 1;
                    }
                    break;
            }
        }

        protected override void OnDocumentAutomaticOutliningComplete(Document document, DocumentModificationEventArgs e)
        {
            if (e.get_IsProgrammaticTextReplacement())
            {
                document.get_Outlining().get_RootNode().CollapseDescendants("Region");
            }
        }

        public override void ResetLineCommentDelimiter()
        {
            base.set_LineCommentDelimiter("--");
        }

        public override void SetOutliningNodeCollapsedText(OutliningNode node)
        {
            string str2;
            TokenCollection tokens = node.get_Document().get_Tokens();
            int index = tokens.IndexOf(node.get_StartOffset());
            string str = tokens.get_Item(index).get_Key();
            switch (str)
            {
                case null:
                    return;

                case "MultiLineCommentStartToken":
                    node.set_CollapsedText("/**/");
                    return;

                default:
                    if (!(str == "RegionStartToken"))
                    {
                        return;
                    }
                    str2 = string.Empty;
                    while (++index < tokens.Count)
                    {
                        if (tokens.get_Item(index).get_Key() == "CommentStringEndToken")
                        {
                            break;
                        }
                        str2 = str2 + tokens.get_Document().GetTokenText(tokens.get_Item(index));
                    }
                    break;
            }
            node.set_CollapsedText(" " + str2.Trim() + " ");
        }

        public override bool ShouldSerializeLineCommentDelimiter()
        {
            return (base.get_LineCommentDelimiter() != "--");
        }
    }
}

