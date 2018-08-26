using System.Windows;
using JetBrains.Annotations;

namespace Scar.Common.WPF.View.Buttons
{
    public partial class MinimizeButton
    {
        public MinimizeButton()
        {
            InitializeComponent();
        }

        private void MinimizeButton_Click([NotNull] object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}