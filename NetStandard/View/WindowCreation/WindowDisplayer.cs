using System;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowCreation
{
    public class WindowDisplayer : IAsyncWindowDisplayer
    {
        readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public async Task<Action<Action<TWindow>>> DisplayWindowAsync<TWindow>(Func<Task<TWindow>> createWindowAsync, CancellationToken cancellationToken)
            where TWindow : class, IDisplayable
        {
            var task = await Task.Factory.StartNew(
                    async () =>
                    {
                        var window = await createWindowAsync().ConfigureAwait(false);

                        return (Action<Action<TWindow>>)ExecuteWithDispatcher;

                        void ExecuteWithDispatcher(Action<TWindow> action)
                        {
                            _ = window ?? throw new InvalidOperationException("Window is null");

                            action(window);
                        }
                    },
                    cancellationToken,
                    TaskCreationOptions.None,
                    _scheduler)
                .ConfigureAwait(false);
            return await task.ConfigureAwait(false) ?? throw new InvalidOperationException("window is null");
        }
    }
}
