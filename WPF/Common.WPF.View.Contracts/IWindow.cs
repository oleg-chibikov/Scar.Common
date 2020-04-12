using System;
using System.Drawing;
using System.Windows;
using Scar.Common.View.Contracts;

namespace Scar.Common.WPF.View.Contracts
{
    public interface IWindow : IDisplayable
    {
        #region Properties

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

        #endregion

        #region Custom Properties

        TimeSpan? AutoCloseTimeout { get; set; }

        AdvancedWindowStartupLocation AdvancedWindowStartupLocation { get; set; }

        bool Draggable { get; set; }

        Rectangle ActiveScreenArea { get; }

        bool IsFullHeight { get; }

        #endregion
    }
}
