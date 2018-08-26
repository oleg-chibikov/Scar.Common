using System;
using System.Drawing;
using System.Windows;
using JetBrains.Annotations;

namespace Scar.Common.WPF.View.Contracts
{
    public interface IWindow
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

        [CanBeNull]
        TimeSpan? AutoCloseTimeout { get; set; }

        AdvancedWindowStartupLocation AdvancedWindowStartupLocation { get; set; }

        bool Draggable { get; set; }

        Rectangle ActiveScreenArea { get; }

        bool IsFullHeight { get; }

        #endregion

        #region Methods

        [CanBeNull]
        bool? ShowDialog();

        void Show();

        void Close();

        void Restore();

        void AssociateDisposable([NotNull] IDisposable disposable);

        bool UnassociateDisposable([NotNull] IDisposable disposable);

        #endregion

        #region Events

        event SizeChangedEventHandler SizeChanged;

        event EventHandler Closed;

        event RoutedEventHandler Loaded;

        #endregion
    }
}