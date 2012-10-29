namespace LINQPad.Lexers
{
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
    using System;

    public class XmlDynamicSyntaxLanguage : DynamicOutliningSyntaxLanguage
    {
        public XmlDynamicSyntaxLanguage()
        {
        }

        public XmlDynamicSyntaxLanguage(string key, bool secure) : base(key, secure)
        {
        }

        private void CompleteElementTag(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, int offset)
        {
            string str = null;
            TextStream textStream = syntaxEditor.get_Document().GetTextStream(offset);
            if (textStream.GoToPreviousToken() && ((textStream.get_Token().get_Key() == "StartTagEndToken") && ((offset - textStream.get_Offset()) == 1)))
            {
                bool flag = false;
                while (textStream.GoToPreviousToken())
                {
                    string str2 = textStream.get_Token().get_Key();
                    if (str2 != null)
                    {
                        if (!(str2 == "StartTagNameToken"))
                        {
                            if ((str2 == "StartTagStartToken") || (str2 == "EndTagEndToken"))
                            {
                                return;
                            }
                        }
                        else
                        {
                            str = textStream.get_TokenText().Trim();
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                }
                if (str != null)
                {
                    textStream.set_Offset(offset);
                    flag = false;
                    while (!textStream.get_IsAtDocumentEnd())
                    {
                        switch (textStream.get_Token().get_Key())
                        {
                            case "EndTagDefaultToken":
                                if (str == textStream.get_TokenText().Trim())
                                {
                                    return;
                                }
                                flag = true;
                                break;

                            case "StartTagStartToken":
                            case "EndTagEndToken":
                                flag = true;
                                break;
                        }
                        if (flag)
                        {
                            break;
                        }
                        textStream.GoToNextToken();
                    }
                    syntaxEditor.get_SelectedView().InsertSurroundingText(0, null, "</" + str + ">");
                }
            }
        }

        public override void GetTokenOutliningAction(TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction)
        {
            IToken token = tokenStream.Peek();
            if ((token.get_Key() == "StartTagStartToken") && (token.get_DeclaringLexicalState().get_Key() == "StartTagState"))
            {
                outliningKey = "Region";
                tokenAction = 1;
            }
            else if (((token.get_Key() == "StartTagEndToken") && (token.get_DeclaringLexicalState().get_Key() == "StartTagState")) && (tokenStream.get_Document().GetSubstring(token.get_TextRange()) == "/>"))
            {
                outliningKey = "Region";
                tokenAction = 2;
            }
            else if ((token.get_Key() == "EndTagEndToken") && (token.get_DeclaringLexicalState().get_Key() == "EndTagState"))
            {
                outliningKey = "Region";
                tokenAction = 2;
            }
        }

        protected override void OnSyntaxEditorTriggerActivated(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, TriggerEventArgs e)
        {
            string str = e.get_Trigger().get_Key();
            if (((str != null) && (str == "TagAutoCompleteTrigger")) && !syntaxEditor.get_SelectedView().get_Selection().get_IsReadOnly())
            {
                this.CompleteElementTag(syntaxEditor, syntaxEditor.get_Caret().get_Offset());
            }
        }
    }
}

