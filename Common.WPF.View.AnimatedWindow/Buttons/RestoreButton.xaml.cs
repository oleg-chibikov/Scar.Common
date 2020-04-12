using System.Windows;

namespace Scar.Common.WPF.View.Buttons
{
    public partial class RestoreButton
    {
        public RestoreButton()
        {
            InitializeComponent();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var window = (BaseWindow)Window.GetWindow((DependencyObject)sender);
            if (window == null)
            {
                return;
            }

            window.WindowState = WindowState.Normal;
        }
    }
}
