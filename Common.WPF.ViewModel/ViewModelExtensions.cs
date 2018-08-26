using System;
using System.Windows;
using JetBrains.Annotations;

namespace Scar.Common.WPF.ViewModel
{
    public static class ViewModelExtensions
    {
        public static void HandleDisposableViewModel([NotNull] this FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            RoutedEventHandler unloadedHandler = null;
            EventHandler shutdownStartedHandler = null;

            void Dispose()
            {
                var dataContext = element.DataContext as IDisposable;
                dataContext?.Dispose();
                // ReSharper disable once AccessToModifiedClosure
                element.Unloaded -= unloadedHandler;
                // ReSharper disable once AccessToModifiedClosure
                element.Dispatcher.ShutdownStarted -= shutdownStartedHandler;
            }

            unloadedHandler = (s, ea) => Dispose();
            shutdownStartedHandler = (s, ea) => Dispose();
            element.Unloaded += unloadedHandler;
            element.Dispatcher.ShutdownStarted += shutdownStartedHandler;
        }
    }
}