namespace LINQPad
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;

    internal class BlockWatcher : UserControl
    {
        private object _block;
        private StackPanel _blocksPanel;
        private double _blockWidth;
        private ToggleButton _chkDetail;
        private PropertyInfo _decliningProp;
        private PropertyInfo _exeProp;
        private PropertyInfo _hasValueProp;
        private Type _iDataFlowBlockType;
        private PropertyInfo _inputProp;
        private int _lastExeCount;
        private IEnumerable<string> _lastInput;
        private int _lastInputCount;
        private bool _lastInputOK;
        private IEnumerable<string> _lastOutput;
        private int _lastOutputCount;
        private bool _lastOutputOK;
        private int _lastPostponedCount;
        private Border _mainPanel;
        private PropertyInfo _outputCountProp;
        private PropertyInfo _outputProp;
        private PropertyInfo _postponedProp;
        private object _proxy;
        private TextBlock _txtDeclining;
        private TextBlock _txtPostPoned;
        private PropertyInfo _valueProp;
        private const int MaxBlockWidth = 20;

        public BlockWatcher(object block, string name)
        {
            EventHandler handler = null;
            RoutedEventHandler handler2 = null;
            RoutedEventHandler handler3 = null;
            this._blockWidth = 20.0;
            StackPanel panel = new StackPanel {
                Orientation = Orientation.Horizontal
            };
            this._blocksPanel = panel;
            ToggleButton button = new ToggleButton();
            TextBlock block2 = new TextBlock {
                Text = "A",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(2.0, 0.0, 1.5, 0.0),
                VerticalAlignment = VerticalAlignment.Center
            };
            button.Content = block2;
            button.IsChecked = false;
            button.Margin = new Thickness(1.0, 0.0, 5.0, 0.0);
            button.VerticalAlignment = VerticalAlignment.Stretch;
            button.Cursor = Cursors.Hand;
            button.ToolTip = "Show message content";
            button.Visibility = Visibility.Collapsed;
            this._chkDetail = button;
            TextBlock block3 = new TextBlock {
                Text = "\x00d5",
                FontSize = 20.0,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkRed,
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0.0, 0.0, 3.0, 0.0),
                ToolTip = "Block is permanently declining messages"
            };
            this._txtDeclining = block3;
            TextBlock block4 = new TextBlock {
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Postedponed Messages",
                Foreground = Brushes.Blue
            };
            this._txtPostPoned = block4;
            Action<Task> continuationAction = null;
            this._block = block;
            try
            {
                this._txtPostPoned.FontFamily = new FontFamily("Verdana");
            }
            catch
            {
            }
            try
            {
                this._txtDeclining.FontFamily = new FontFamily("WingDings");
            }
            catch
            {
            }
            Type type = block.GetType();
            this._iDataFlowBlockType = type.GetInterface("IDataflowBlock");
            if (type.IsGenericType)
            {
                Type[] genericArguments = type.GetGenericArguments();
                if (((genericArguments.Length <= 2) && this.ShouldExpandType(genericArguments.First<Type>())) && this.ShouldExpandType(genericArguments.Last<Type>()))
                {
                    this._chkDetail.IsChecked = true;
                }
            }
            DebuggerTypeProxyAttribute attribute = (DebuggerTypeProxyAttribute) block.GetType().GetCustomAttributes(typeof(DebuggerTypeProxyAttribute), true).FirstOrDefault<object>();
            if (attribute == null)
            {
                throw new InvalidOperationException("Object has no DebuggerTypeProxyAttribute");
            }
            Type type3 = Type.GetType(attribute.ProxyTypeName).MakeGenericType(block.GetType().GetGenericArguments());
            this._proxy = Activator.CreateInstance(type3, new object[] { block });
            this._exeProp = type3.GetProperty("CurrentDegreeOfParallelism");
            this._inputProp = type3.GetProperty("InputQueue");
            this._outputProp = type3.GetProperty("OutputQueue");
            this._postponedProp = type3.GetProperty("PostponedMessages");
            if ((this._inputProp == null) && (this._outputProp == null))
            {
                this._outputProp = type3.GetProperty("Queue");
            }
            if (this._outputProp == null)
            {
                this._outputCountProp = type3.GetProperty("OutputCount");
            }
            this._hasValueProp = type3.GetProperty("HasValue");
            this._valueProp = type3.GetProperty("Value");
            this._decliningProp = type3.GetProperty("IsDecliningPermanently");
            StackPanel panel3 = new StackPanel {
                Orientation = Orientation.Horizontal
            };
            if ((this._txtDeclining.FontFamily.FamilyNames != null) && (this._txtDeclining.FontFamily.FamilyNames.Count > 0))
            {
            }
            if ((CS$<>9__CachedAnonymousMethodDelegate10 == null) && this._txtDeclining.FontFamily.FamilyNames.Any<KeyValuePair<XmlLanguage, string>>(CS$<>9__CachedAnonymousMethodDelegate10))
            {
                panel3.Children.Add(this._txtDeclining);
            }
            panel3.Children.Add(this._chkDetail);
            panel3.Children.Add(this._txtPostPoned);
            panel3.Children.Add(this._blocksPanel);
            GroupBox box = new GroupBox();
            TextBlock block5 = new TextBlock {
                Text = name,
                FontWeight = FontWeights.SemiBold
            };
            box.Header = block5;
            box.BorderThickness = new Thickness(2.0);
            box.BorderBrush = Brushes.LightSkyBlue;
            box.Content = panel3;
            box.Margin = new Thickness(0.0, 0.0, 5.0, 0.0);
            box.HorizontalAlignment = HorizontalAlignment.Left;
            GroupBox groupBox = box;
            Border border2 = new Border {
                Child = groupBox,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            base.Content = this._mainPanel = border2;
            DispatcherTimer timer2 = new DispatcherTimer(DispatcherPriority.Normal, base.Dispatcher) {
                Interval = TimeSpan.FromMilliseconds(100.0),
                IsEnabled = true
            };
            if (handler == null)
            {
                handler = (sender, e) => this.UpdateBlocksPanel();
            }
            timer2.Tick += handler;
            PropertyInfo property = this._iDataFlowBlockType.GetProperty("Completion");
            if (property != null)
            {
                Task task = property.GetValue(this._block, null) as Task;
                if (task != null)
                {
                    if (continuationAction == null)
                    {
                        continuationAction = delegate (Task ant) {
                            try
                            {
                                bool faulted = ant.IsFaulted;
                                string msg = faulted ? (ant.Exception.InnerException.GetType().Name + ": " + ant.Exception.InnerException.Message + " ") : null;
                                Action method = delegate {
                                    try
                                    {
                                        if (faulted)
                                        {
                                            groupBox.BorderBrush = Brushes.Red;
                                        }
                                        this._txtDeclining.Text = "\x00fc";
                                        this._txtDeclining.ToolTip = "Block has completed";
                                        this._txtDeclining.Foreground = Brushes.Green;
                                        this._txtDeclining.Visibility = Visibility.Visible;
                                        if (faulted)
                                        {
                                            groupBox.Content = msg;
                                        }
                                    }
                                    catch
                                    {
                                    }
                                };
                                groupBox.Dispatcher.BeginInvoke(method, new object[0]);
                            }
                            catch
                            {
                            }
                        };
                    }
                    task.ContinueWith(continuationAction);
                }
            }
            if (handler2 == null)
            {
                handler2 = (sender, e) => this._lastExeCount = -1;
            }
            this._chkDetail.Checked += handler2;
            if (handler3 == null)
            {
                handler3 = (sender, e) => this._lastExeCount = -1;
            }
            this._chkDetail.Unchecked += handler3;
        }

        private void AddBlocks(UIElementCollection container, Brush color, IEnumerable<string> items)
        {
            foreach (string str in items)
            {
                Border element = new Border {
                    MinWidth = this._blockWidth,
                    MaxWidth = 200.0,
                    MinHeight = 23.0,
                    Background = color,
                    Margin = new Thickness(1.0, 0.0, 1.0, 0.0),
                    CornerRadius = new CornerRadius(4.0)
                };
                if (str != null)
                {
                    TextBlock block = new TextBlock {
                        Text = str,
                        Foreground = Brushes.White,
                        Margin = new Thickness(2.0, 0.0, 2.0, 0.0),
                        HorizontalAlignment = (str.Length > 10) ? HorizontalAlignment.Left : HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        LineStackingStrategy = LineStackingStrategy.MaxHeight
                    };
                    element.Child = block;
                }
                container.Add(element);
            }
        }

        public void Cancel()
        {
            try
            {
                MethodInfo method = this._iDataFlowBlockType.GetMethod("Fault", new Type[] { typeof(Exception) });
                if (method != null)
                {
                    method.Invoke(this._block, new OperationCanceledException[] { new OperationCanceledException("LINQPad script was canceled") });
                }
            }
            catch
            {
            }
        }

        private UIElementCollection CreateBufferPanel(string text, Color color)
        {
            StackPanel element = new StackPanel {
                Orientation = Orientation.Horizontal,
                MinHeight = 23.0
            };
            DockPanel panel2 = new DockPanel {
                Cursor = Cursors.Hand
            };
            panel2.MouseDown += delegate (object sender, MouseButtonEventArgs e) {
                this._chkDetail.IsChecked = new bool?(!this._chkDetail.IsChecked.Value);
            };
            TextBlock block = new TextBlock {
                Text = text,
                Foreground = new SolidColorBrush(color),
                Margin = new Thickness(2.0, -1.0, 2.0, 0.0),
                LineStackingStrategy = LineStackingStrategy.MaxHeight
            };
            panel2.Children.Add(block);
            panel2.Children.Add(element);
            block.SetValue(DockPanel.DockProperty, Dock.Bottom);
            Border border = new Border {
                CornerRadius = new CornerRadius(4.0),
                Margin = new Thickness(0.0, 0.0, 1.0, 0.0),
                Background = new SolidColorBrush(Color.FromArgb(50, color.R, color.G, color.B)),
                Child = panel2
            };
            this._blocksPanel.Children.Add(border);
            return element.Children;
        }

        private bool ShouldExpandType(Type t)
        {
            return (((t.IsPrimitive || (t == typeof(decimal))) || (t == typeof(string))) || (t.IsArray && (t.GetArrayRank() == 1)));
        }

        private string StringConversion(object o)
        {
            if (o == null)
            {
                return "(null)";
            }
            Type type = o.GetType();
            if (type.IsArray && (type.GetArrayRank() == 1))
            {
                Array source = (Array) o;
                if ((source.Length <= 15) && this.ShouldExpandType(type.GetElementType()))
                {
                    return string.Join(",\r\n", source.Cast<object>().Select<object, string>(new Func<object, string>(this.StringConversion)));
                }
                return string.Concat(new object[] { type.GetElementType().Name, "[", source.Length, "]" });
            }
            return o.ToString();
        }

        private void UpdateBlocksPanel()
        {
            if (!this._chkDetail.IsChecked.Value)
            {
                this._lastInput = (IEnumerable<string>) (this._lastOutput = null);
            }
            IList source = null;
            IList list2 = null;
            int num = 0;
            int num2 = 0;
            int count = 0;
            int num4 = 0;
            bool flag = false;
            bool flag2 = true;
            if (this._exeProp != null)
            {
                try
                {
                    count = (int) this._exeProp.GetValue(this._proxy, null);
                }
                catch (InvalidOperationException)
                {
                    if (this._lastExeCount > 0)
                    {
                        return;
                    }
                }
                catch
                {
                }
            }
            if (this._inputProp != null)
            {
                try
                {
                    source = this._inputProp.GetValue(this._proxy, null) as IList;
                    this._lastInputOK = true;
                }
                catch (InvalidOperationException)
                {
                    if (this._lastInputOK)
                    {
                        return;
                    }
                }
                catch
                {
                }
            }
            if (this._outputProp != null)
            {
                try
                {
                    list2 = this._outputProp.GetValue(this._proxy, null) as IList;
                    this._lastOutputOK = true;
                    goto Label_01D8;
                }
                catch (InvalidOperationException)
                {
                    if (this._lastOutputOK)
                    {
                        return;
                    }
                    goto Label_01D8;
                }
                catch
                {
                    goto Label_01D8;
                }
            }
            if (this._outputCountProp != null)
            {
                try
                {
                    num2 = (int) this._outputCountProp.GetValue(this._proxy, null);
                    this._lastOutputOK = true;
                    goto Label_01D8;
                }
                catch (InvalidOperationException)
                {
                    if (this._lastOutputOK)
                    {
                        return;
                    }
                    goto Label_01D8;
                }
                catch
                {
                    goto Label_01D8;
                }
            }
            if ((this._hasValueProp != null) && (this._valueProp != null))
            {
                bool flag3 = false;
                try
                {
                    flag3 = (bool) this._hasValueProp.GetValue(this._proxy, null);
                }
                catch
                {
                }
                if (!flag3)
                {
                    goto Label_01D8;
                }
                try
                {
                    list2 = new object[] { this._valueProp.GetValue(this._proxy, null) };
                    goto Label_01D8;
                }
                catch
                {
                    goto Label_01D8;
                }
            }
            flag2 = false;
        Label_01D8:
            if (this._postponedProp != null)
            {
                try
                {
                    object obj2 = this._postponedProp.GetValue(this._proxy, null);
                    if (obj2 != null)
                    {
                        num4 = (int) obj2.GetType().GetProperty("Count", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(obj2, null);
                    }
                }
                catch
                {
                }
            }
            if (this._decliningProp != null)
            {
                try
                {
                    flag = (bool) this._decliningProp.GetValue(this._proxy, null);
                }
                catch
                {
                }
            }
            if (flag && (this._txtDeclining.Visibility == Visibility.Collapsed))
            {
                this._txtDeclining.Visibility = Visibility.Visible;
            }
            if (source == null)
            {
                source = new string[0];
            }
            if (list2 == null)
            {
                list2 = new string[0];
            }
            num = Math.Max(num, source.Count);
            num2 = Math.Max(num2, list2.Count);
            if ((((num == this._lastInputCount) && (num2 == this._lastOutputCount)) && (count == this._lastExeCount)) && (num4 == this._lastPostponedCount))
            {
                if (!this._chkDetail.IsChecked.Value)
                {
                    return;
                }
                List<string> second = source.Cast<object>().Select<object, string>(new Func<object, string>(this.StringConversion)).ToList<string>();
                List<string> list4 = list2.Cast<object>().Select<object, string>(new Func<object, string>(this.StringConversion)).ToList<string>();
                if ((((this._lastInput != null) && (this._lastOutput != null)) && this._lastInput.SequenceEqual<string>(second)) && this._lastOutput.SequenceEqual<string>(list4))
                {
                    return;
                }
                this._lastInput = second;
                this._lastOutput = list4;
            }
            if (num4 != this._lastPostponedCount)
            {
                if (num4 == 0)
                {
                    this._txtPostPoned.Text = "";
                    this._txtPostPoned.Margin = new Thickness(0.0);
                }
                else
                {
                    this._txtPostPoned.Text = "(+" + num4.ToString() + ")";
                    this._txtPostPoned.Margin = new Thickness(0.0, 0.0, 3.0, 0.0);
                }
            }
            this._lastExeCount = count;
            this._lastInputCount = num;
            this._lastOutputCount = num2;
            this._lastPostponedCount = num4;
            this._blocksPanel.Children.Clear();
            int num5 = (num + count) + num2;
            if (((num5 * 0x16) + 20) < Math.Max(500.0, this._mainPanel.ActualWidth))
            {
                this._blockWidth = 20.0;
            }
            else
            {
                this._blockWidth = 5.0;
            }
            if (this._inputProp != null)
            {
                this.AddBlocks(this.CreateBufferPanel("Input", Colors.Blue), Brushes.Blue, (this._lastInput != null) ? this._lastInput.Reverse<string>() : Enumerable.Repeat<string>(null, num));
            }
            if (this._exeProp != null)
            {
                this.AddBlocks(this.CreateBufferPanel("EXE", Colors.Red), Brushes.Red, Enumerable.Repeat<string>(null, count));
            }
            bool flag4 = (this._inputProp == null) && (this._exeProp == null);
            if (flag2)
            {
                this.AddBlocks(this.CreateBufferPanel(flag4 ? "Buffer" : "Output", Colors.Green), Brushes.Green, (this._lastOutput != null) ? this._lastOutput.Reverse<string>() : Enumerable.Repeat<string>(null, num2));
            }
        }
    }
}

