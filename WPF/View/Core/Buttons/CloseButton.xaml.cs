using System.Windows;

namespace Scar.Common.WPF.View.Core.Buttons
{
    public partial class CloseButton
    {
        public CloseButton()
        {
            InitializeComponent();
        }

        void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow((DependencyObject)sender)?.Close();
        }
    }
}
