using System.Runtime.InteropServices;
using MyBHOTool;
using mshtml;

namespace UKeyHelplerForIE
{
    [ComVisible(true)]
    public class HTMLDocumentEventHelper
    {
        private HTMLDocument document;

        public HTMLDocumentEventHelper(HTMLDocument document)
        {
            this.document = document;
        }

        public event HtmlEvent oncontextmenu
        {
            add
            {
                DispHTMLDocument dispDoc = this.document as DispHTMLDocument;

                object existingHandler = dispDoc.oncontextmenu;
                HTMLEventHandler handler = existingHandler is HTMLEventHandler ?
                    existingHandler as HTMLEventHandler : 
                    new HTMLEventHandler(this.document);

                dispDoc.oncontextmenu = handler;

                handler.eventHandler += value;
            }
            remove
            {
                DispHTMLDocument dispDoc = this.document as DispHTMLDocument;
                object existingHandler = dispDoc.oncontextmenu;

                HTMLEventHandler handler = existingHandler is HTMLEventHandler ?
                    existingHandler as HTMLEventHandler : null;

                if (handler != null)
                    handler.eventHandler -= value;
            }
        }
    }

}
