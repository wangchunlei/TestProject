namespace LINQPad
{
    using LINQPad.UI;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;

    public static class PanelManager
    {
        public static OutputPanel DisplayControl(Control c)
        {
            return DisplayControl(c, null);
        }

        public static OutputPanel DisplayControl(Control c, string panelTitle)
        {
            PluginControl control = PluginServer.AddControl(c, panelTitle);
            if (control == null)
            {
                return null;
            }
            return control.OutputPanel;
        }

        public static OutputPanel DisplaySyntaxColoredText(string text, SyntaxLanguageStyle language)
        {
            return DisplaySyntaxColoredText(text, language, null);
        }

        public static OutputPanel DisplaySyntaxColoredText(string text, SyntaxLanguageStyle language, string panelTitle)
        {
            return DisplayControl(new SimpleSyntaxEditor(text, language), panelTitle);
        }

        public static OutputPanel DisplayWpfElement(UIElement e)
        {
            return DisplayWpfElement(e, null);
        }

        public static OutputPanel DisplayWpfElement(UIElement e, string panelTitle)
        {
            ElementHost c = new ElementHost {
                Child = e
            };
            return DisplayControl(c, panelTitle);
        }

        public static OutputPanel GetOutputPanel(string panelTitle)
        {
            return (from pic in PluginServer.GetExistingControls() select pic.OutputPanel).FirstOrDefault<OutputPanel>(p => (p.Heading == panelTitle));
        }

        public static OutputPanel[] GetOutputPanels()
        {
            return (from pic in PluginServer.GetExistingControls() select pic.OutputPanel).ToArray<OutputPanel>();
        }

        public static OutputPanel[] GetOutputPanels(string panelTitle)
        {
            return (from pic in PluginServer.GetExistingControls()
                select pic.OutputPanel into p
                where p.Heading == panelTitle
                select p).ToArray<OutputPanel>();
        }

        public static OutputPanel StackWpfElement(UIElement e)
        {
            return StackWpfElement(e, null);
        }

        public static OutputPanel StackWpfElement(UIElement e, string panelTitle)
        {
            ScrollableStackContainer wpfElement;
            if (string.IsNullOrEmpty(panelTitle))
            {
                panelTitle = "&Custom";
            }
            OutputPanel panel = GetOutputPanels(panelTitle).FirstOrDefault<OutputPanel>(c => c.GetWpfElement() is ScrollableStackContainer);
            if (panel == null)
            {
                wpfElement = new ScrollableStackContainer();
                panel = DisplayWpfElement(wpfElement, panelTitle);
            }
            else
            {
                wpfElement = (ScrollableStackContainer) panel.GetWpfElement();
            }
            wpfElement.AddChild(e);
            return panel;
        }
    }
}

