using System.Windows;

namespace Scar.Common.WPF.View.Buttons
{
    public partial class MaximizeButton
    {
        public MaximizeButton()
        {
            InitializeComponent();
        }

        void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
            {
                window.WindowState = WindowState.Maximized;
            }
        }
    }
}
