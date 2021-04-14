using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Threading;
using Scar.Common.View.Contracts;
using Scar.Common.View.WindowCreation;

namespace Scar.Common.WPF.View.WindowCreation
{
    public class StaThreadWindowDisplayer : IWindowDisplayer
    {
        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows-only")]
        public Action<Action<TWindow>> DisplayWindow<TWindow>(Func<TWindow> createWindow)
            where TWindow : class, IDisplayable
        {
            Dispatcher? threadDispatcher = null;
            TWindow? window = null;
            var thread = new Thread(
                () =>
                {
                    threadDispatcher = Dispatcher.CurrentDispatcher;
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(threadDispatcher));

                    window = createWindow();
                    window.Restore();

                    Dispatcher.Run();
                });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            void ExecuteWithDispatcher(Action<TWindow> action)
            {
                _ = window ?? throw new InvalidOperationException("Window is null");
                _ = threadDispatcher ?? throw new InvalidOperationException("ThreadDispatcher is null");

                threadDispatcher.BeginInvoke(() => { action(window); });
            }

            return ExecuteWithDispatcher;
        }
    }
}
