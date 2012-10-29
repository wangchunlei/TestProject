namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using LINQPad;
    using System;
    using System.Drawing;

    internal class GrayPinkBackgroundSpanIndicator : SpanIndicator
    {
        public GrayPinkBackgroundSpanIndicator() : base("Custom")
        {
        }

        protected override void ApplyHighlightingStyleAdornments(HighlightingStyleResolver resolver)
        {
        }

        protected override void ApplyHighlightingStyleColors(HighlightingStyleResolver resolver)
        {
            Color actualEditorBackColor = UserOptions.Instance.ActualEditorBackColor;
            if (actualEditorBackColor.GetBrightness() < 0.4f)
            {
                actualEditorBackColor = Color.FromArgb(0x80, 0x80, 0x80);
            }
            else
            {
                actualEditorBackColor = Color.FromArgb((actualEditorBackColor.R * 0x13) / 20, (actualEditorBackColor.G * 9) / 10, (actualEditorBackColor.B * 7) / 8);
            }
            resolver.SetBackColor(actualEditorBackColor);
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

