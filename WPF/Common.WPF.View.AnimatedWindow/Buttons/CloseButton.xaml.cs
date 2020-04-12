using System.Windows;

namespace Scar.Common.WPF.View.Buttons
{
    public partial class CloseButton
    {
        public CloseButton()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow((DependencyObject)sender)?.Close();
        }
    }
}
