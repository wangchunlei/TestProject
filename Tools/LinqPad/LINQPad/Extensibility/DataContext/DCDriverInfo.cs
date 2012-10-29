namespace LINQPad.Extensibility.DataContext
{
    using System;

    [Serializable]
    internal class DCDriverInfo
    {
        public string Author;
        public bool IsAuto;
        public DCDriverLoader Loader;
        public string Name;
        public string Version;

        public DCDriverInfo()
        {
        }

        public DCDriverInfo(DataContextDriver driver)
        {
            this.Loader = driver.Loader;
            this.Name = driver.Name;
            this.Author = driver.Author;
            this.Version = driver.Version.ToString();
            this.IsAuto = driver is DynamicDataContextDriver;
        }
    }
}

