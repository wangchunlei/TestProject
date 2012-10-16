using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfTest
{
    class WindowTest : Window
    {
        private Button _button = new Button { Content = "Go" };
        TextBlock _results = new TextBlock();
        public WindowTest()
        {
            var panel = new StackPanel();
            panel.Children.Add(_button);
            panel.Children.Add(_results);
            Content = panel;

            _button.Click += (sender, agrs) => Go();
        }

        async void Go()
        {
            for (int i = 1; i < 5; i++)
            {
                _results.Text += await GetPrimesCountAsync(i * 1000000, 1000000) +
                                 " primes between " + (i * 1000000) + " and " + ((i + 1) * 1000000 - 1) +
                                 Environment.NewLine;
            }
        }
        Task<int> GetPrimesCountAsync(int start, int count)
        {
            return Task.Run(() =>

                            ParallelEnumerable.Range(start, count).Count(n =>

                                                                         Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All
                                                                             (i => n % i > 0)));
        }
        int GetPrimesCount(int start, int count)
        {
            var items = ParallelEnumerable.Range(start, count).Count(item=>Enumerable.Range(2, (int)Math.Sqrt(item) - 1).All(i => item % i > 0));
            return items;
        }
    }
}
