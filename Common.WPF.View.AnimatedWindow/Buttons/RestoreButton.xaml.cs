using System.Windows;
using JetBrains.Annotations;

namespace Scar.Common.WPF.View.Buttons
{
    public partial class RestoreButton
    {
        public RestoreButton()
        {
            InitializeComponent();
        }

        private void RestoreButton_Click([NotNull] object sender, RoutedEventArgs e)
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