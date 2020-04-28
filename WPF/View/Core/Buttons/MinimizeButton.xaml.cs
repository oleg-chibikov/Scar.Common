using System.Windows;

namespace Scar.Common.WPF.View.Buttons
{
    public partial class MinimizeButton
    {
        public MinimizeButton()
        {
            InitializeComponent();
        }

        void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}
