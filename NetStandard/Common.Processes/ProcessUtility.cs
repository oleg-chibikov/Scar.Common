using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scar.Common.Events;

namespace Scar.Common.Processes
{
    public sealed class ProcessUtility : IProcessUtility
    {
        static readonly TimeSpan TaskKillSleepTime = TimeSpan.FromSeconds(5);
        readonly ILogger _logger;

        public ProcessUtility(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler<EventArgs<string>>? ProcessMessageFired;

        public event EventHandler<EventArgs<string>>? ProcessErrorFired;

        public async Task<ProcessResult> ExecuteCommandAsync(string commandPath, string? arguments, CancellationToken token, TimeSpan? timeout, string? workingDirectory)
        {
            _ = commandPath ?? throw new ArgumentNullException(nameof(commandPath));
            return await Task.Run(
                    async () =>
                    {
                        _logger.LogInformation($"Running {commandPath} with arguments {arguments}");
                        var processInfo = new ProcessStartInfo(commandPath, arguments)
                        {
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        };
                        if (!string.IsNullOrEmpty(workingDirectory))
                        {
                            processInfo.WorkingDirectory = workingDirectory;
                        }

                        using var process = Process.Start(processInfo);
                        if (process == null)
                        {
                            throw new InvalidOperationException($"Command {commandPath} not found");
                        }

                        _logger.LogTrace($"Waiting command to exit for process {commandPath}...");

                        var outputStringBuilder = new StringBuilder();
                        var errorStringBuilder = new StringBuilder();

                        void OutputHandler(object sender, DataReceivedEventArgs e)
                        {
                            if (!string.IsNullOrWhiteSpace(e.Data))
                            {
                                OnMessage(e.Data);
                                outputStringBuilder.AppendLine(e.Data);
                                _logger.LogTrace(e.Data);
                            }
                        }

                        void ErrorHandler(object sender, DataReceivedEventArgs e)
                        {
                            if (string.IsNullOrWhiteSpace(e.Data))
                            {
                                return;
                            }

                            OnError(e.Data);
                            errorStringBuilder.AppendLine(e.Data);
                            _logger.LogWarning(e.Data);
                        }

                        process.OutputDataReceived += OutputHandler;
                        process.ErrorDataReceived += ErrorHandler;
                        try
                        {
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            CancellationTokenSource? linkedTokenSource = null;

                            if (timeout.HasValue)
                            {
                                linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, new CancellationTokenSource(timeout.Value).Token);
                                token = linkedTokenSource.Token;
                            }

                            var exitTask = WaitForExitAsync(process, commandPath, token);
                            await exitTask.ConfigureAwait(false);
                            linkedTokenSource?.Dispose();

                            _logger.LogDebug($"The process {commandPath} has exited with exit code {process.ExitCode}");
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogDebug($"The process {commandPath} is canceled");
                            throw;
                        }
                        finally
                        {
                            process.OutputDataReceived -= OutputHandler;
                            process.ErrorDataReceived -= ErrorHandler;
                        }

                        var output = outputStringBuilder.ToString();
                        var error = errorStringBuilder.ToString();
                        return new ProcessResult(string.IsNullOrEmpty(output) ? null : output, string.IsNullOrEmpty(error) ? null : error, process.ExitCode);
                    },
                    token)
                .ConfigureAwait(false);
        }

        public async Task TaskKillAsync(string processName, CancellationToken token)
        {
            _ = processName ?? throw new ArgumentNullException(nameof(processName));
            if (!ProcessExists(processName))
            {
                return;
            }

            _logger.LogDebug($"Killing {processName}...");
            Process.Start("taskkill", $"/F /IM {processName}.exe");
            _logger.LogDebug($"{processName} is killed");
            _logger.LogDebug($"Sleeping for {TaskKillSleepTime}...");
            await Task.Delay(TaskKillSleepTime, token).ConfigureAwait(false);
        }

        void OnError(string message)
        {
            ProcessErrorFired?.Invoke(this, new EventArgs<string>(message));
        }

        void OnMessage(string message)
        {
            ProcessMessageFired?.Invoke(this, new EventArgs<string>(message));
        }

        bool ProcessExists(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="name">The name of the process for logging.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return immediately as canceled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        Task WaitForExitAsync(Process process, string name, CancellationToken cancellationToken)
        {
            _ = process ?? throw new ArgumentNullException(nameof(process));
            _ = name ?? throw new ArgumentNullException(nameof(name));
            var taskCompletionSource = new TaskCompletionSource<object?>();
            process.EnableRaisingEvents = true;

            void OnProcessExited(object sender, EventArgs args)
            {
                _logger.LogDebug($"Handling process {name} exit...");

                var setResult = taskCompletionSource.TrySetResult(null);
                if (setResult)
                {
                    _logger.LogDebug($"The process {name} has exited successfully");
                }
                else
                {
                    _logger.LogWarning($"The process {name} result cannot be set");
                }

                // ReSharper disable once AccessToModifiedClosure - the outer code needs to await the returning task before disposal
                process.Exited -= OnProcessExited;
            }

            // No need to wait for the already exited process;
            if (process.HasExited)
            {
                return Task.CompletedTask;
            }

            process.Exited += OnProcessExited;

            cancellationToken.Register(
                () =>
                {
                    process.Exited -= OnProcessExited;
                    _logger.LogDebug($"Checking the process {name} should be canceled");
                    if (taskCompletionSource.Task.IsCompleted)
                    {
                        _logger.LogDebug($"The process {name} has already exited - no need to cancel");
                        return;
                    }

                    _logger.LogDebug($"Canceling process {name}...");
                    var cancelResult = taskCompletionSource.TrySetCanceled(cancellationToken);
                    if (cancelResult)
                    {
                        _logger.LogDebug($"The process {name} was canceled");
                    }
                    else
                    {
                        _logger.LogWarning($"The process {name} cannot be canceled");
                    }
                });

            return taskCompletionSource.Task;
        }
    }
}
