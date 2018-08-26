using System.Windows;
using JetBrains.Annotations;

namespace Scar.Common.WPF.View.Buttons
{
    public partial class CloseButton
    {
        public CloseButton()
        {
            InitializeComponent();
        }

        private void CloseButton_Click([NotNull] object sender, RoutedEventArgs e)
        {
            Window.GetWindow((DependencyObject)sender)?.Close();
        }
    }
}