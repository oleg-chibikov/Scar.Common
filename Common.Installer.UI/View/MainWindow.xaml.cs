using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Scar.Common.Installer.UI.ViewModel;

namespace Scar.Common.Installer.UI.View
{
    public partial class MainWindow
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Initialized in Loaded event
        public MainWindow()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        SetupViewModel SetupViewModel { get; set; }

        protected override bool CheckCloseShouldBeCancelled()
        {
            return SetupViewModel.IsRunning;
        }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
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

        void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
