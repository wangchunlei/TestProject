namespace LINQPad.Extensibility.DataContext.UI
{
    using LINQPad.UI;
    using System;
    using System.Windows.Forms;

    public static class Dialogs
    {
        public static object PickFromList(string windowTitle, object[] items)
        {
            using (ListPicker picker = new ListPicker(items))
            {
                picker.Text = windowTitle;
                if (picker.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }
                return picker.SelectedItem;
            }
        }
    }
}

