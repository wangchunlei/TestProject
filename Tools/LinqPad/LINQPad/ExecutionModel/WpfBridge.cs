namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;

    internal class WpfBridge
    {
        public static bool DumpWpfElement(object element, string heading)
        {
            if (element is Window)
            {
                ((Window) element).Show();
                return true;
            }
            if (!(element is UIElement))
            {
                return false;
            }
            if (!(((element is DockPanel) || (element is Canvas)) ? !double.IsNaN(((FrameworkElement) element).Height) : true))
            {
                PanelManager.DisplayWpfElement((UIElement) element, heading ?? "WPF");
                return true;
            }
            if (!string.IsNullOrEmpty(heading))
            {
                TextBlock e = new TextBlock {
                    Text = heading,
                    FontSize = 13.0,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Green,
                    Margin = new Thickness(0.0, 5.0, 0.0, 0.0)
                };
                PanelManager.StackWpfElement(e, "WPF");
            }
            PanelManager.StackWpfElement((UIElement) element, "WPF");
            return true;
        }

        private static List<Dispatcher> GetActiveDispatchers()
        {
            List<Dispatcher> list = new List<Dispatcher>();
            try
            {
                FieldInfo field = typeof(Dispatcher).GetField("_dispatchers", BindingFlags.NonPublic | BindingFlags.Static);
                if (field == null)
                {
                    return list;
                }
                IEnumerable<WeakReference> enumerable = field.GetValue(null) as IEnumerable<WeakReference>;
                if (enumerable == null)
                {
                    return list;
                }
                list.AddRange(from dr in enumerable
                    select dr.Target as Dispatcher into dr
                    where (((dr != null) && (dr.Thread != Thread.CurrentThread)) && dr.Thread.IsAlive) && !dr.HasShutdownFinished
                    select dr);
            }
            catch
            {
            }
            return list;
        }

        public static bool HasWpfMsgLoopRun()
        {
            FieldInfo field = typeof(Application).GetField("_appCreatedInThisAppDomain", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                return false;
            }
            object obj2 = field.GetValue(null);
            bool flag2 = true;
            return flag2.Equals(obj2);
        }

        public static void ShutdownCurrentDispatcher()
        {
            if (GetActiveDispatchers().Any<Dispatcher>(d => d.Thread == Thread.CurrentThread))
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }
        }

        private static void ShutdownDispatcher(Dispatcher dispatcher, Countdown countdown)
        {
            EventHandler handler = null;
            if ((dispatcher != null) && !dispatcher.HasShutdownFinished)
            {
                countdown.Increment();
                try
                {
                    if (handler == null)
                    {
                        handler = (sender, e) => countdown.Decrement();
                    }
                    dispatcher.ShutdownFinished += handler;
                    if (dispatcher.HasShutdownFinished)
                    {
                        countdown.Decrement();
                    }
                    else
                    {
                        dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                    }
                }
                catch
                {
                    countdown.Decrement();
                }
            }
        }

        public static void ShutdownWPF(bool waitUntilDone)
        {
            // This item is obfuscated and can not be translated.
            ShutdownCurrentDispatcher();
            Countdown countdown = new Countdown();
            try
            {
                foreach (Dispatcher dispatcher in GetActiveDispatchers())
                {
                    ShutdownDispatcher(dispatcher, countdown);
                }
            }
            finally
            {
                if (waitUntilDone)
                {
                    while (countdown.Wait(0x1388))
                    {
                    Label_004C:
                        if (0 == 0)
                        {
                            goto Label_006E;
                        }
                    }
                    goto Label_004C;
                }
            Label_006E:;
            }
        }
    }
}

