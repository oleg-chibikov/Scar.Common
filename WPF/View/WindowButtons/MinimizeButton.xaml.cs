using System;
using System.Windows;

namespace Scar.Common.WPF.View.WindowButtons
{
    public partial class MinimizeButton
    {
        public MinimizeButton()
        {
            InitializeComponent();
        }

        void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject ?? throw new InvalidOperationException("sender is null"));
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}
