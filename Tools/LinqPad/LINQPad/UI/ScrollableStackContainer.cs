namespace LINQPad.UI
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    internal class ScrollableStackContainer : ScrollViewer
    {
        private StackPanel _content;
        private static double _fontSize = double.NaN;

        public ScrollableStackContainer()
        {
            StackPanel panel = new StackPanel {
                Margin = new Thickness(3.0)
            };
            this._content = panel;
            if (!double.IsNaN(_fontSize))
            {
                base.FontSize = _fontSize;
            }
            base.Content = this._content;
            base.CanContentScroll = true;
            base.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            base.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            base.Background = Brushes.White;
            base.UseLayoutRounding = true;
        }

        public void AddChild(UIElement e)
        {
            this._content.Children.Add(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double num = e.Delta / 100;
                if (base.FontSize > 25.0)
                {
                    num *= 3.0;
                }
                else if (base.FontSize > 16.0)
                {
                    num *= 2.0;
                }
                base.FontSize = _fontSize = Math.Max(5.0, Math.Min((double) 100.0, (double) (base.FontSize + num)));
            }
            base.OnMouseWheel(e);
        }
    }
}

