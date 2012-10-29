namespace LINQPad
{
    using LINQPad.ExecutionModel;
    using LINQPad.ObjectGraph;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    internal static class LiveDumper
    {
        private static Server _lastServer;
        private static Assembly _rxConcurrencyDll;
        private static MethodInfo _rxScheduleMethod;
        private static Type _rxSchedulerType;
        private static object _threadLocker = new object();

        public static void DumpLive<T>(IObservable<T> obs, string heading)
        {
            ObservablePresenter presenter;
            OutputPanel outPanel;
            ManualResetEvent ready;
            IDisposable extensionToken;
            Server currentServer = Server.CurrentServer;
            if (currentServer != null)
            {
                object obj2;
                presenter = null;
                outPanel = null;
                ready = new ManualResetEvent(false);
                Action action = delegate {
                    presenter = new ObservablePresenter();
                    outPanel = PanelManager.StackWpfElement(WrapInHeading(presenter, heading), "Live &Observables");
                    ready.Set();
                };
                lock ((obj2 = _threadLocker))
                {
                    if (currentServer != _lastServer)
                    {
                        _lastServer = currentServer;
                    }
                    if (currentServer.IsOnMainQueryThread)
                    {
                        action();
                    }
                    else
                    {
                        currentServer.RunOnMessageLoopThread(action);
                    }
                }
                int num = 0;
                while (true)
                {
                    lock ((obj2 = _threadLocker))
                    {
                        if ((((currentServer != _lastServer) || currentServer.MessageLoopEnded) || currentServer.CancelRequest) || (num++ > 20))
                        {
                            return;
                        }
                    }
                    if (ready.WaitOne(500, false))
                    {
                        extensionToken = Util.GetQueryLifeExtensionToken();
                        try
                        {
                            IDisposable subscription = null;
                            CancelToken cancelToken = new CancelToken();
                            Action cancel = delegate {
                                IDisposable disposable1 = subscription;
                                if (disposable1 != null)
                                {
                                    disposable1.Dispose();
                                }
                                else
                                {
                                    cancelToken.Cancel();
                                }
                            };
                            outPanel.QueryEnded += delegate (object sender, EventArgs e) {
                                cancel();
                                if (!(!outPanel.IsQueryCanceled || outPanel.IsPanelClosing))
                                {
                                    presenter.SetCanceled();
                                }
                            };
                            outPanel.PanelClosed += delegate (object sender, EventArgs e) {
                                cancel();
                                extensionToken.Dispose();
                            };
                            Action subscribe = delegate {
                                Action<T> onNext = null;
                                Action<Exception> onError = null;
                                Action onCompleted = null;
                                try
                                {
                                    if (onNext == null)
                                    {
                                        onNext = val => base.CS$<>8__localsd.presenter.SetNext(val);
                                    }
                                    if (onError == null)
                                    {
                                        onError = delegate (Exception ex) {
                                            base.CS$<>8__localsd.presenter.SetError(ex);
                                            base.CS$<>8__localsd.extensionToken.Dispose();
                                        };
                                    }
                                    if (onCompleted == null)
                                    {
                                        onCompleted = delegate {
                                            base.CS$<>8__localsd.presenter.SetComplete();
                                            base.CS$<>8__localsd.extensionToken.Dispose();
                                        };
                                    }
                                    subscription = Subscribe<T>(obs, onNext, onError, onCompleted, presenter.Dispatcher, cancelToken);
                                    if (cancelToken.IsCancellationRequested)
                                    {
                                        subscription.Dispose();
                                    }
                                }
                                catch (Exception exception)
                                {
                                    if (subscription != null)
                                    {
                                        subscription.Dispose();
                                    }
                                    if (exception is ThreadAbortException)
                                    {
                                        Thread.ResetAbort();
                                    }
                                    else if (!(exception is OperationCanceledException))
                                    {
                                        Log.Write(exception, "Rx Subscription Failure");
                                        exception.Dump<Exception>();
                                    }
                                }
                            };
                            new Thread(delegate {
                                bool flag = false;
                                try
                                {
                                    flag = RunOnRxScheduler(subscribe);
                                }
                                catch (Exception exception)
                                {
                                    Log.Write(exception);
                                }
                                if (!flag)
                                {
                                    subscribe();
                                }
                            }) { IsBackground = true, Name = "Rx Subscriber" }.Start();
                        }
                        catch
                        {
                            extensionToken.Dispose();
                        }
                        return;
                    }
                }
            }
        }

        private static Assembly GetRxConcurrencyAssembly()
        {
            if (_rxConcurrencyDll != null)
            {
                return _rxConcurrencyDll;
            }
            Server currentServer = Server.CurrentServer;
            if (currentServer == null)
            {
                return null;
            }
            Assembly assembly2 = currentServer.ShadowLoad("System.Reactive.Core", null, null, null);
            if (assembly2 == null)
            {
                assembly2 = currentServer.ShadowLoad("System.Reactive", null, null, null);
            }
            if (assembly2 == null)
            {
                return null;
            }
            return (_rxConcurrencyDll = assembly2);
        }

        private static MethodInfo GetRxScheduleMethod()
        {
            if (_rxScheduleMethod != null)
            {
                return _rxScheduleMethod;
            }
            Type rxSchedulerType = GetRxSchedulerType();
            if (rxSchedulerType == null)
            {
                return null;
            }
            return (_rxScheduleMethod = rxSchedulerType.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault<MethodInfo>(m => (((m.Name == "Schedule") && (m.GetParameters().Length == 2)) && (m.GetParameters()[0].ParameterType.FullName == "System.Reactive.Concurrency.IScheduler")) && (m.GetParameters()[1].ParameterType == typeof(Action))));
        }

        private static Type GetRxSchedulerType()
        {
            if (_rxSchedulerType != null)
            {
                return _rxSchedulerType;
            }
            Assembly rxConcurrencyAssembly = GetRxConcurrencyAssembly();
            if (rxConcurrencyAssembly == null)
            {
                return null;
            }
            return (_rxSchedulerType = rxConcurrencyAssembly.GetType("System.Reactive.Concurrency.Scheduler"));
        }

        private static bool RunOnRxScheduler(Action a)
        {
            MethodInfo rxScheduleMethod = GetRxScheduleMethod();
            if (rxScheduleMethod == null)
            {
                return false;
            }
            PropertyInfo property = GetRxSchedulerType().GetProperty("CurrentThread");
            if (property == null)
            {
                return false;
            }
            object obj2 = property.GetValue(null, null);
            PropertyInfo info3 = obj2.GetType().GetProperty("ScheduleRequired");
            if ((info3 != null) && 0.Equals(info3.GetValue(obj2, null)))
            {
                return false;
            }
            rxScheduleMethod.Invoke(null, new object[] { obj2, a });
            return true;
        }

        public static IDisposable Subscribe<TSource>(IObservable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted, Dispatcher dispatcher, CancelToken cancelToken)
        {
            DispatcherThrottler throttler = new DispatcherThrottler();
            Thread subscribingThread = Thread.CurrentThread;
            return ObservableHelper.Subscribe<TSource>(source, delegate (TSource item) {
                throttler.Run(dispatcher, () => onNext(item), false, cancelToken, subscribingThread);
            }, delegate (Exception ex) {
                throttler.Run(dispatcher, () => onError(ex), true, null, null);
            }, delegate {
                throttler.Run(dispatcher, onCompleted, true, null, null);
            });
        }

        private static UIElement WrapInHeading(UIElement child, string heading)
        {
            if (string.IsNullOrEmpty(heading))
            {
                return child;
            }
            DockPanel panel2 = new DockPanel {
                Margin = new Thickness(0.0, 1.0, 3.0, 1.0)
            };
            TextBlock element = new TextBlock {
                Text = heading,
                Padding = new Thickness(4.0, 0.0, 4.0, 1.0),
                Margin = new Thickness(0.0, 0.0, 3.0, 0.0),
                Background = new SolidColorBrush(Color.FromRgb(0xdd, 0xee, 0xff))
            };
            TextOptions.SetTextFormattingMode(element, TextFormattingMode.Display);
            element.SetValue(DockPanel.DockProperty, Dock.Left);
            panel2.Children.Add(element);
            panel2.Children.Add(child);
            return panel2;
        }

        private class ObservablePresenter : DockPanel
        {
            private UIElement _glyph;
            private ContentPresenter _presenter;

            public ObservablePresenter()
            {
                ContentPresenter presenter = new ContentPresenter {
                    Content = "-",
                    Margin = new Thickness(0.0, 0.0, 0.0, 2.0)
                };
                this._presenter = presenter;
                TextOptions.SetTextFormattingMode(this._presenter, TextFormattingMode.Display);
                this.Glyph = GetRunGlyph();
                base.Children.Add(this._presenter);
            }

            private static UIElement GetErrorElement(Exception ex)
            {
                return new TextBlock { Foreground = Brushes.Red, Text = ex.Message, ToolTip = ex.GetType().Name };
            }

            private static UIElement GetRunGlyph()
            {
                SolidColorBrush brush = new SolidColorBrush(Colors.Green);
                Polygon polygon = new Polygon {
                    Fill = brush,
                    Stroke = Brushes.Green,
                    StrokeThickness = 1.5,
                    Margin = new Thickness(3.0, 0.0, 4.0, 0.0)
                };
                polygon.Points.Add(new Point(0.0, 4.0));
                polygon.Points.Add(new Point(0.0, 13.0));
                polygon.Points.Add(new Point(5.0, 8.0));
                ColorAnimation element = new ColorAnimation(Colors.Green, Color.FromRgb(0, 220, 0), new Duration(TimeSpan.FromMilliseconds(1500.0)));
                Storyboard.SetTarget(element, polygon);
                Storyboard.SetTargetProperty(element, new PropertyPath("Fill.Color", new object[0]));
                Storyboard storyboard = new Storyboard {
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                storyboard.Children.Add(element);
                storyboard.Begin();
                return polygon;
            }

            private static UIElement GetStopGlyph()
            {
                return new Rectangle { Fill = Brushes.DarkRed, Margin = new Thickness(2.0, 3.0, 4.0, 2.0), Width = 5.0, Height = 5.0 };
            }

            public void SetCanceled()
            {
                this.Glyph = GetStopGlyph();
            }

            public void SetComplete()
            {
                this.Glyph = null;
            }

            public void SetError(Exception ex)
            {
                this.Glyph = null;
                TextBlock block = new TextBlock {
                    Foreground = Brushes.Red,
                    Text = ex.Message,
                    ToolTip = ex.GetType().Name
                };
                this._presenter.Content = block;
            }

            public void SetNext(object content)
            {
                if (content == null)
                {
                    this._presenter.Content = "(null)";
                }
                else
                {
                    try
                    {
                        this._presenter.Content = TranslateContent(content);
                    }
                    catch (Exception exception)
                    {
                        this._presenter.Content = "(" + exception.Message + ")";
                    }
                }
            }

            private static object TranslateContent(object content)
            {
                if (content is Image)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        ((Image) content).Save(stream, ImageFormat.Bmp);
                        content = new ImageBlob(stream.ToArray());
                    }
                }
                if (!(content is ImageRef) && !(content is ImageBlob))
                {
                    return content;
                }
                Image image2 = new Image {
                    Stretch = Stretch.None,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                if (content is ImageRef)
                {
                    image2.Source = new BitmapImage(((ImageRef) content).Uri);
                    return image2;
                }
                BitmapImage image3 = new BitmapImage();
                image3.BeginInit();
                MemoryStream stream2 = new MemoryStream(((ImageBlob) content).Data) {
                    Position = 0L
                };
                image3.StreamSource = stream2;
                image3.EndInit();
                image2.Source = image3;
                return image2;
            }

            private UIElement Glyph
            {
                get
                {
                    return this._glyph;
                }
                set
                {
                    if (this._glyph != null)
                    {
                        base.Children.Remove(this._glyph);
                        this._glyph = null;
                    }
                    this._glyph = value;
                    if (this._glyph != null)
                    {
                        this._glyph.SetValue(DockPanel.DockProperty, Dock.Left);
                        base.Children.Insert(0, this._glyph);
                    }
                }
            }
        }
    }
}

