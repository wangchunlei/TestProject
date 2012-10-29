namespace LINQPad
{
    using System;

    internal class LocalUserOptions : SerializableOptions
    {
        private static LocalUserOptions _instance;
        [Serialize]
        public bool ExplorerPanelsHidden;
        public static readonly string FileName = Path.Combine(Program.LocalUserDataFolder, "UserOptions.xml");
        [Serialize]
        public bool IsMaximized;
        [Serialize]
        public float MainSplitterHoriz;
        [Serialize]
        public float MainSplitterVert;
        [Serialize]
        public bool PluginsOnTop;
        [Serialize]
        public bool VerticalResultsLayout;
        [Serialize]
        public int WindowHeight;
        [Serialize]
        public int WindowLeft;
        [Serialize]
        public int WindowTop;
        [Serialize]
        public int WindowWidth;

        public override string FullPath
        {
            get
            {
                return FileName;
            }
        }

        public static LocalUserOptions Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = new LocalUserOptions();
                        _instance.Deserialize();
                    }
                    catch
                    {
                    }
                }
                return _instance;
            }
        }
    }
}

