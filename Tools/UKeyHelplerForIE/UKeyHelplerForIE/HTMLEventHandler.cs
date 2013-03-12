using System.Runtime.InteropServices;

namespace UKeyHelplerForIE
{

    // The delegate of the handler method.
    public delegate void HtmlEvent(mshtml.IHTMLEventObj e);

    [ComVisible(true)]
    public class HTMLEventHandler
    {

        private mshtml.HTMLDocument htmlDocument;

        public event HtmlEvent eventHandler;

        public HTMLEventHandler(mshtml.HTMLDocument htmlDocument)
        {
            this.htmlDocument = htmlDocument;
        }

        [DispId(0)]
        public void FireEvent()
        {
            this.eventHandler(this.htmlDocument.parentWindow.@event);
        }
    }
}
