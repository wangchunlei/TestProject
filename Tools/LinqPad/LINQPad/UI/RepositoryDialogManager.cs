namespace LINQPad.UI
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using System;
    using System.Windows.Forms;
    using System.Xml.Linq;

    internal static class RepositoryDialogManager
    {
        public static bool Show(Repository repository, bool isNewRepository)
        {
            DCDriverLoader driverLoader = repository.DriverLoader;
            if (isNewRepository)
            {
                using (DCDriverForm form = new DCDriverForm())
                {
                    form.SelectedDriverLoader = repository.DriverLoader;
                    if (form.ShowDialog() != DialogResult.OK)
                    {
                        return false;
                    }
                    driverLoader = form.SelectedDriverLoader;
                    if (driverLoader == null)
                    {
                        return false;
                    }
                    repository.DriverLoader = driverLoader;
                }
            }
            if (driverLoader.InternalID != null)
            {
                if (!driverLoader.Driver.ShowConnectionDialog(repository, isNewRepository))
                {
                    return false;
                }
                return true;
            }
            WorkingForm.FlashForm("Loading...", 0x5dc);
            string text = driverLoader.ShowConnectionDialog(repository.GetStore().ToString(), isNewRepository);
            if (text == null)
            {
                return false;
            }
            repository.LoadFromStore(XElement.Parse(text));
            return true;
        }

        private class DialogInvoker : MarshalByRefObject
        {
            public string ShowConnectionDialog(DCDriverLoader loader, string repositoryData, bool isNewRepository)
            {
                return loader.Driver.ShowConnectionDialog(repositoryData, isNewRepository);
            }
        }
    }
}

