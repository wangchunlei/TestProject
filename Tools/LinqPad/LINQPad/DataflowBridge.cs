namespace LINQPad
{
    using System;

    internal class DataflowBridge
    {
        public static bool Dump(object block, string heading)
        {
            if (block == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(heading))
            {
                heading = block.GetType().FormatTypeName();
            }
            try
            {
                BlockWatcher watcher = new BlockWatcher(block, heading);
                OutputPanel panel = PanelManager.StackWpfElement(watcher, "Dataflow");
                panel.QueryEnded += delegate (object sender, EventArgs e) {
                    if (panel.IsQueryCanceled)
                    {
                        watcher.Cancel();
                    }
                };
                panel.PanelClosed += delegate (object sender, EventArgs e) {
                    watcher.Cancel();
                };
                return true;
            }
            catch (Exception exception)
            {
                Log.Write(exception, "Dataflow Dump");
                return false;
            }
        }
    }
}

