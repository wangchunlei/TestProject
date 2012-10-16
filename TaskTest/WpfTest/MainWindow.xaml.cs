using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SynchronizationContext _uiSyncContext;
        public MainWindow()
        {
            InitializeComponent();

            _uiSyncContext = SynchronizationContext.Current;

            new Thread(Work).Start();
        }

        void Work()
        {
            Thread.Sleep(5000);
            UpdateMessage("The answer");
        }

        private void UpdateMessage(string p)
        {
            _uiSyncContext.Post(_ => this.txtMessage.Text = p, null);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var wc = new WindowTest();
            wc.ShowDialog();
        }
    }
}
