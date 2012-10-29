namespace LINQPad.UI
{
    using LINQPad.ExecutionModel;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;

    internal class PluginServer
    {
        public static int CustomCount = 1;

        public static PluginControl AddControl(Control c, string heading)
        {
            Server currentServer = Server.CurrentServer;
            if (currentServer == null)
            {
                return null;
            }
            PluginWindowManager pluginWindowManager = currentServer.PluginWindowManager;
            if (pluginWindowManager == null)
            {
                return null;
            }
            bool flag = false;
            if (pluginWindowManager.Form == null)
            {
                flag = true;
                pluginWindowManager.Form = new PluginForm(pluginWindowManager);
            }
            if (string.IsNullOrEmpty(heading))
            {
                heading = "Custom" + ((CustomCount == 1) ? "" : (" " + CustomCount));
                if (CustomCount == 1)
                {
                    heading = "&" + heading;
                }
                CustomCount++;
            }
            PluginControl plugin = pluginWindowManager.Form.AddControl(c, heading);
            if (flag)
            {
                currentServer.NotifyPluginFormCreated(pluginWindowManager.Form);
            }
            pluginWindowManager.PluginJustAdded(plugin);
            return plugin;
        }

        public static void DisposeWpfElement(UIElement e)
        {
            Server currentServer = Server.CurrentServer;
            if (currentServer != null)
            {
                PluginWindowManager pluginWindowManager = currentServer.PluginWindowManager;
                if ((pluginWindowManager != null) && (pluginWindowManager.Form != null))
                {
                    ElementHost host = pluginWindowManager.Form.Controls.OfType<ElementHost>().FirstOrDefault<ElementHost>(h => h.Child == e);
                    if (host != null)
                    {
                        host.Dispose();
                    }
                }
            }
        }

        public static PluginControl[] GetExistingControls()
        {
            Server currentServer = Server.CurrentServer;
            if (currentServer == null)
            {
                return new PluginControl[0];
            }
            PluginWindowManager pluginWindowManager = currentServer.PluginWindowManager;
            if ((pluginWindowManager == null) || (pluginWindowManager.Form == null))
            {
                return new PluginControl[0];
            }
            return pluginWindowManager.GetControls();
        }
    }
}

