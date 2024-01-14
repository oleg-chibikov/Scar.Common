using System;
using System.Windows;

namespace Scar.Common.WPF.View.WindowButtons;

public partial class CloseButton
{
    public CloseButton()
    {
        InitializeComponent();
    }

    void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Window.GetWindow(sender as DependencyObject ?? throw new InvalidOperationException("sender is null"))?.Close();
    }
}