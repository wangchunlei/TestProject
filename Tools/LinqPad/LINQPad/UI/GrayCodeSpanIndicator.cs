namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using System;
    using System.Drawing;

    internal class GrayCodeSpanIndicator : SpanIndicator
    {
        public GrayCodeSpanIndicator() : base("Custom")
        {
        }

        protected override void ApplyHighlightingStyleAdornments(HighlightingStyleResolver resolver)
        {
        }

        protected override void ApplyHighlightingStyleColors(HighlightingStyleResolver resolver)
        {
            resolver.SetForeColor(Color.FromArgb(210, 210, 210));
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

