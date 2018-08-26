using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Scar.Common.Installer.UI.ViewModel;

namespace Scar.Common.Installer.UI.View
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private SetupViewModel SetupViewModel { get; set; }

        protected override bool CheckCloseShouldBeCancelled()
        {
            return SetupViewModel.IsRunning;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupViewModel = new SetupViewModel(ConfigurationManager.AppSettings["MsiPath"])
            {
                InUiThread = action =>
                {
                    if (Dispatcher.CheckAccess())
                    {
                        action();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
                    }
                }
            };
            DataContext = SetupViewModel;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}