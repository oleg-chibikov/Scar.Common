using System;
using System.Windows;
using Scar.Common.View.Contracts;

namespace Scar.Common.WPF.View.Contracts
{
    public interface IWindow : IDisplayable
    {
        double Top { get; set; }

        double Left { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        WindowState WindowState { get; set; }

        WindowStartupLocation WindowStartupLocation { get; set; }

        WindowStyle WindowStyle { get; set; }

        ResizeMode ResizeMode { get; set; }

        Visibility Visibility { get; set; }

        bool Topmost { get; set; }

        bool ShowActivated { get; set; }

        TimeSpan? AutoCloseTimeout { get; set; }

        AdvancedWindowStartupLocation AdvancedWindowStartupLocation { get; set; }

        bool Draggable { get; set; }
    }
}
