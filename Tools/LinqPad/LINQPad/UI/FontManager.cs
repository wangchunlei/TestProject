namespace LINQPad.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal static class FontManager
    {
        private static Font _defaultFont;

        internal static Font GetDefaultFont()
        {
            if (_defaultFont == null)
            {
                try
                {
                    _defaultFont = SystemFonts.MessageBoxFont;
                    new Font(_defaultFont, FontStyle.Bold).Dispose();
                    new Font(_defaultFont, FontStyle.Italic).Dispose();
                    new Font(_defaultFont, FontStyle.Underline).Dispose();
                    new Font(_defaultFont, FontStyle.Underline | FontStyle.Italic | FontStyle.Bold).Dispose();
                }
                catch
                {
                    _defaultFont = Control.DefaultFont;
                }
            }
            return _defaultFont;
        }
    }
}

