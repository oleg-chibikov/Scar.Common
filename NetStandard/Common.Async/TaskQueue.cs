using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Scar.Common.Async
{
    [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "This is a proper name")]
    public sealed class TaskQueue : IAppendable<Func<Task>>, IDisposable
    {
        readonly ILog _logger;
        readonly BlockingCollection<Func<Task>> _queue = new BlockingCollection<Func<Task>>();
        readonly Task _worker;

        public TaskQueue(ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _worker = Task.Factory.StartNew(async () => await PollQueue(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current).Unwrap();
        }

        public int CurrentlyQueuedTasks => _queue.Count;

        public void Append(Func<Task> task)
        {
            _ = task ?? throw new ArgumentNullException(nameof(task));
            _queue.Add(task);
        }

        public async void Dispose()
        {
            _queue.CompleteAdding();
            await _worker.ConfigureAwait(false);
            _queue.Dispose();
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Don't know underlying exceptions")]
        async Task PollQueue()
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
