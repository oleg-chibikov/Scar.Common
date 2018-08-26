using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace Scar.Common.Async
{
    [UsedImplicitly]
    public sealed class TaskQueue : IAppendable<Func<Task>>, IDisposable
    {
        [NotNull]
        private readonly ILog _logger;

        [NotNull]
        private readonly BlockingCollection<Func<Task>> _queue = new BlockingCollection<Func<Task>>();

        [NotNull]
        private readonly Task _worker;

        public TaskQueue([NotNull] ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _worker = Task.Factory.StartNew(async () => await PollQueue(), TaskCreationOptions.LongRunning).Unwrap();
        }

        public int CurrentlyQueuedTasks => _queue.Count;

        public void Append([NotNull] Func<Task> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            _queue.Add(task);
        }

        public async void Dispose()
        {
            _queue.CompleteAdding();
            await _worker.ConfigureAwait(false);
            _queue.Dispose();
        }

        private async Task PollQueue()
        {
            _logger.Trace("Starting polling queue...");
            while (true)
            {
                if (_queue.IsCompleted)
                {
                    break;
                }

                foreach (var task in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        await task().ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Warn("Task is cancelled");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error processing task", ex);
                    }
                }
            }

            _logger.Trace("Finished polling queue");
        }
    }
}