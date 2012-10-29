namespace LINQPad.Lexers
{
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
    using System;

    public class CSharpDynamicSyntaxLanguage : DynamicOutliningSyntaxLanguage
    {
        public CSharpDynamicSyntaxLanguage()
        {
        }

        public CSharpDynamicSyntaxLanguage(string key, bool secure) : base(key, secure)
        {
        }

        public override void GetTokenOutliningAction(TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction)
        {
            switch (tokenStream.Peek().get_Key())
            {
                case "OpenCurlyBraceToken":
                    outliningKey = "CodeBlock";
                    tokenAction = 1;
                    break;

                case "CloseCurlyBraceToken":
                    outliningKey = "CodeBlock";
                    tokenAction = 2;
                    break;

                case "MultiLineCommentStartToken":
                    outliningKey = "MultiLineComment";
                    tokenAction = 1;
                    break;

                case "MultiLineCommentEndToken":
                    outliningKey = "MultiLineComment";
                    tokenAction = 2;
                    break;

                case "XMLCommentStartToken":
                    outliningKey = "XMLComment";
                    tokenAction = 1;
                    break;

                case "XMLCommentEndToken":
                    outliningKey = "XMLComment";
                    tokenAction = 2;
                    break;

                case "RegionPreProcessorDirectiveStartToken":
                    outliningKey = "RegionPreProcessorDirective";
                    tokenAction = 1;
                    break;

                case "EndRegionPreProcessorDirectiveEndToken":
                    outliningKey = "RegionPreProcessorDirective";
                    tokenAction = 2;
                    break;
            }
        }

        protected override void OnDocumentAutomaticOutliningComplete(Document document, DocumentModificationEventArgs e)
        {
            if (e.get_IsProgrammaticTextReplacement())
            {
                document.get_Outlining().get_RootNode().CollapseDescendants("RegionPreProcessorDirective");
            }
        }

        protected override void OnSyntaxEditorTriggerActivated(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, TriggerEventArgs e)
        {
            string str = e.get_Trigger().get_Key();
            if ((str != null) && (str == "XMLCommentTagListTrigger"))
            {
                IntelliPromptMemberList list = syntaxEditor.get_IntelliPrompt().get_MemberList();
                list.ResetAllowedCharacters();
                list.set_ImageList(ActiproSoftware.SyntaxEditor.SyntaxEditor.get_ReflectionImageList());
                list.Clear();
                list.Add(new IntelliPromptMemberListItem("c", 0x33, "Indicates that text within the tag should be marked as code.  Use &lt;code&gt; to indicate multiple lines as code."));
                list.Add(new IntelliPromptMemberListItem("code", 0x33, "Indicates multiple lines as code. Use &lt;c&gt; to indicate that text within a description should be marked as code."));
                list.Add(new IntelliPromptMemberListItem("example", 0x33, "Specifies an example of how to use a method or other library member."));
                list.Add(new IntelliPromptMemberListItem("exception", 0x33, "Specifies which exceptions a class can throw.", "exception cref=\"", "\""));
                list.Add(new IntelliPromptMemberListItem("include", 0x33, "Refers to comments in another file that describe the types and members in your source code.", "include file='", "' path='[@name=\"\"]'/>"));
                list.Add(new IntelliPromptMemberListItem("list", 0x33, "Provides a container for list items.", "list type=\"", "\""));
                list.Add(new IntelliPromptMemberListItem("listheader", 0x33, "Defines the heading row of either a table or definition list."));
                list.Add(new IntelliPromptMemberListItem("item", 0x33, "Defines an item in a table or definition list."));
                list.Add(new IntelliPromptMemberListItem("term", 0x33, "A term to define, which will be defined in text."));
                list.Add(new IntelliPromptMemberListItem("description", 0x33, "Either an item in a bullet or numbered list or the definition of a term."));
                list.Add(new IntelliPromptMemberListItem("para", 0x33, "Provides a paragraph container."));
                list.Add(new IntelliPromptMemberListItem("param", 0x33, "Describes one of the parameters for the method.", "param name=\"", "\"/>"));
                list.Add(new IntelliPromptMemberListItem("paramref", 0x33, "Indicates that a word is a parameter.", "paramref name=\"", "\"/>"));
                list.Add(new IntelliPromptMemberListItem("permission", 0x33, "Documents the access of a member.", "permission cref=\"", "\""));
                list.Add(new IntelliPromptMemberListItem("remarks", 0x33, "Specifies overview information about a class or other type."));
                list.Add(new IntelliPromptMemberListItem("returns", 0x33, "Describes the return value for a method declaration."));
                list.Add(new IntelliPromptMemberListItem("see", 0x33, "Specifies a link from within text.", "see cref=\"", "\"/>"));
                list.Add(new IntelliPromptMemberListItem("seealso", 0x33, "Specifies the text that you might want to appear in a See Also section.", "seealso cref=\"", "\"/>"));
                list.Add(new IntelliPromptMemberListItem("summary", 0x33, "Describes a member for a type."));
                list.Add(new IntelliPromptMemberListItem("value", 0x33, "Describes the value for a property declaration."));
                if (list.get_Count() > 0)
                {
                    list.Show();
                }
            }
        }

        public override void ResetLineCommentDelimiter()
        {
            base.set_LineCommentDelimiter("//");
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
                case "XMLCommentStartToken":
                    node.set_CollapsedText("/**/");
                    return;

                default:
                    if (!(str == "RegionPreProcessorDirectiveStartToken"))
                    {
                        return;
                    }
                    str2 = string.Empty;
                    while (++index < tokens.Count)
                    {
                        if (tokens.get_Item(index).get_Key() == "PreProcessorDirectiveEndToken")
                        {
                            break;
                        }
                        str2 = str2 + tokens.get_Document().GetTokenText(tokens.get_Item(index));
                    }
                    break;
            }
            node.set_CollapsedText(str2.Trim());
        }

        public override bool ShouldSerializeLineCommentDelimiter()
        {
            return (base.get_LineCommentDelimiter() != "//");
        }
    }
}

