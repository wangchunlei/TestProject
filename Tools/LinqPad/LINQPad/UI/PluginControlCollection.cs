namespace LINQPad.UI
{
    using System.Collections.ObjectModel;
    using System.Windows.Forms;

    internal class PluginControlCollection : KeyedCollection<Control, PluginControl>
    {
        protected override Control GetKeyForItem(PluginControl item)
        {
            return item.Control;
        }
    }
}

