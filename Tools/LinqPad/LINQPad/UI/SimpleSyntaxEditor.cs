namespace LINQPad.UI
{
    using ActiproSoftware.Drawing;
    using ActiproSoftware.SyntaxEditor;
    using LINQPad;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class SimpleSyntaxEditor : UserControl
    {
        private ActiproSoftware.SyntaxEditor.SyntaxEditor _editor;

        public SimpleSyntaxEditor(string text, SyntaxLanguageStyle language)
        {
            ActiproSoftware.SyntaxEditor.SyntaxEditor editor = new ActiproSoftware.SyntaxEditor.SyntaxEditor {
                Dock = DockStyle.Fill
            };
            editor.set_IndicatorMarginVisible(false);
            editor.set_ScrollBarType(0);
            this._editor = editor;
            base.Controls.Add(this._editor);
            try
            {
                this.Font = new Font("Consolas", 10f);
                if (this.Font.Name != "Consolas")
                {
                    this.Font = new Font("Courier New", 10f);
                }
            }
            catch
            {
            }
            string name = language.ToString();
            if (name == "VB")
            {
                name = name + "DotNet";
            }
            if (language != SyntaxLanguageStyle.None)
            {
                this._editor.get_Document().set_Language(DocumentManager.GetDynamicLanguage(name, SystemColors.Window.GetBrightness()));
                this._editor.get_Document().get_Outlining().set_Mode(2);
                this._editor.set_BracketHighlightingVisible(true);
            }
            this._editor.get_Document().set_ReadOnly(true);
            VisualStudio2005SyntaxEditorRenderer renderer = new VisualStudio2005SyntaxEditorRenderer();
            SimpleBorder border = new SimpleBorder();
            border.set_Style(0);
            renderer.set_Border(border);
            VisualStudio2005SyntaxEditorRenderer renderer2 = renderer;
            this._editor.set_Renderer(renderer2);
            this.Text = text;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            if (this._editor != null)
            {
                this._editor.Font = this.Font;
            }
            base.OnFontChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (this._editor != null)
            {
                this._editor.get_Document().set_Text(this.Text);
            }
            base.OnTextChanged(e);
        }
    }
}

