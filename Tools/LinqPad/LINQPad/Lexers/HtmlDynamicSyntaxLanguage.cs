namespace LINQPad.Lexers
{
    using ActiproSoftware.SyntaxEditor;
    using System;

    public class HtmlDynamicSyntaxLanguage : XmlDynamicSyntaxLanguage
    {
        public HtmlDynamicSyntaxLanguage()
        {
        }

        public HtmlDynamicSyntaxLanguage(string key, bool secure) : base(key, secure)
        {
        }

        public override void GetTokenOutliningAction(TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction)
        {
            IToken token = tokenStream.Peek();
            switch ((token.get_Language().get_Key() + "_" + token.get_Key()))
            {
                case "CSS_PropertyStartToken":
                    outliningKey = "CSS_PropertyBlock";
                    tokenAction = 1;
                    break;

                case "CSS_PropertyEndToken":
                    outliningKey = "CSS_PropertyBlock";
                    tokenAction = 2;
                    break;

                case "CSS_CommentStartToken":
                    outliningKey = "CSS_Comment";
                    tokenAction = 1;
                    break;

                case "CSS_CommentEndToken":
                    outliningKey = "CSS_Comment";
                    tokenAction = 2;
                    break;

                case "JScript_OpenCurlyBraceToken":
                    outliningKey = "JScript_CodeBlock";
                    tokenAction = 1;
                    break;

                case "JScript_CloseCurlyBraceToken":
                    outliningKey = "JScript_CodeBlock";
                    tokenAction = 2;
                    break;

                case "JScript_MultiLineCommentStartToken":
                    outliningKey = "JScript_MultiLineComment";
                    tokenAction = 1;
                    break;

                case "JScript_MultiLineCommentEndToken":
                    outliningKey = "JScript_MultiLineComment";
                    tokenAction = 2;
                    break;

                default:
                    if (tokenAction != 0)
                    {
                        if (token.HasFlag(0x40))
                        {
                            if ((tokenStream.get_Position() > 0) && (tokenStream.ReadReverse().get_LexicalState().get_Key() == "ASPDirectiveResponseWriteState"))
                            {
                                outliningKey = null;
                                tokenAction = 0;
                            }
                        }
                        else if (token.HasFlag(0x80))
                        {
                            tokenStream.Read();
                            if (!tokenStream.get_IsDocumentEnd() && (tokenStream.Peek().get_LexicalState().get_Key() == "ASPDirectiveResponseWriteState"))
                            {
                                outliningKey = null;
                                tokenAction = 0;
                            }
                        }
                    }
                    break;
            }
        }

        protected override void OnSyntaxEditorTriggerActivated(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, TriggerEventArgs e)
        {
            string str = e.get_Trigger().get_Key();
            if (str != null)
            {
                if (!(str == "TagAutoCompleteTrigger"))
                {
                    if (str == "TagListTrigger")
                    {
                        IntelliPromptMemberList list = syntaxEditor.get_IntelliPrompt().get_MemberList();
                        list.ResetAllowedCharacters();
                        list.set_ImageList(ActiproSoftware.SyntaxEditor.SyntaxEditor.get_ReflectionImageList());
                        list.Clear();
                        list.Add(new IntelliPromptMemberListItem("<!-- -->", 0x2d, null, "!-- ", " -->"));
                        list.Add(new IntelliPromptMemberListItem("a", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("acronym", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("address", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("applet", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("area", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("b", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("base", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("basefont", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("bdo", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("bgsound", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("big", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("blockquote", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("body", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("br", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("button", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("caption", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("center", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("cite", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("code", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("col", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("colgroup", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("dd", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("del", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("dfn", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("dir", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("div", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("dl", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("dt", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("em", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("embed", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("fieldset", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("form", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("frame", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("frameset", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("h1", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("h2", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("h3", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("h4", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("h5", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("h6", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("head", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("hr", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("html", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("i", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("iframe", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("img", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("input", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("ins", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("isindex", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("kbd", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("label", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("legend", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("li", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("link", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("listing", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("map", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("marquee", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("menu", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("meta", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("nobr", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("noframes", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("noscript", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("object", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("ol", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("option", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("p", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("param", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("plaintext", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("pre", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("q", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("s", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("samp", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("script", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("select", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("small", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("span", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("strike", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("strong", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("style", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("sub", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("sup", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("table", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("tbody", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("td", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("textarea", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("tfoot", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("th", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("thead", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("title", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("tr", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("tt", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("u", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("ul", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("var", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("xml", 0x2b));
                        list.Add(new IntelliPromptMemberListItem("xmp", 0x2b));
                        if (list.get_Count() > 0)
                        {
                            list.Show();
                        }
                    }
                }
                else
                {
                    base.OnSyntaxEditorTriggerActivated(syntaxEditor, e);
                }
            }
        }

        public override void SetOutliningNodeCollapsedText(OutliningNode node)
        {
            if (node.get_ParseData() != null)
            {
                string str = node.get_ParseData().get_Key();
                if (str != null)
                {
                    if (str != "CSS_Comment")
                    {
                        if (!(str == "CSS_PropertyBlock"))
                        {
                            if (str == "JScript_MultiLineComment")
                            {
                                node.set_CollapsedText("/**/");
                            }
                        }
                        else
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

