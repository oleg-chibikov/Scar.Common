using System;
using System.Windows;

namespace Scar.Common.WPF.View.WindowButtons
{
    public partial class RestoreButton
    {
        public RestoreButton()
        {
            InitializeComponent();
        }

        void RestoreButton_Click(object? sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject ?? throw new InvalidOperationException("sender is null"));
            if (window == null)
            {
                return;
            }

            window.WindowState = WindowState.Normal;
        }
    }
}
