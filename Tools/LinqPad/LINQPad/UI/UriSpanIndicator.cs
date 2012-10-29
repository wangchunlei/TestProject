namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using LINQPad;
    using System;
    using System.Drawing;

    internal class UriSpanIndicator : SpanIndicator
    {
        public UriSpanIndicator() : base("Custom")
        {
        }

        protected override void ApplyHighlightingStyleAdornments(HighlightingStyleResolver resolver)
        {
            resolver.ApplyUnderline((UserOptions.Instance.ActualEditorBackColor.GetBrightness() < 0.4f) ? Color.FromArgb(120, 150, 0xff) : Color.Blue, 0, 0);
        }

        protected override void ApplyHighlightingStyleColors(HighlightingStyleResolver resolver)
        {
            resolver.ApplyHighlightingStyleColors(new HighlightingStyle("URI", "URI", Color.Blue, Color.Transparent));
        }

        public override bool HasFontChange
        {
            get
            {
                return false;
            }
        }
    }
}

