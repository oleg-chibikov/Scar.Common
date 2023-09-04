using System;
using System.Diagnostics;
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

        public ProcessUtility(ILogger<ProcessUtility> logger)
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
                        _logger.LogTrace($"Running {commandPath} with arguments {arguments}");
                        var processInfo = new ProcessStartInfo(commandPath, arguments ?? string.Empty)
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

                            _logger.LogTrace($"The process {commandPath} has exited with exit code {process.ExitCode}");
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogTrace($"The process {commandPath} is canceled");
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

                        void OutputHandler(object? sender, DataReceivedEventArgs e)
                        {
                            _ = outputStringBuilder ?? throw new InvalidOperationException("outputStringBuilder is null");

                            if (!string.IsNullOrWhiteSpace(e.Data))
                            {
                                OnMessage(e.Data);
                                outputStringBuilder.AppendLine(e.Data);
                                _logger.LogTrace(e.Data);
                            }
                        }

                        void ErrorHandler(object? sender, DataReceivedEventArgs e)
                        {
                            _ = errorStringBuilder ?? throw new InvalidOperationException("errorStringBuilder is null");

                            if (string.IsNullOrWhiteSpace(e.Data))
                            {
                                return;
                            }

                            OnError(e.Data);
                            errorStringBuilder.AppendLine(e.Data);
                            _logger.LogWarning(e.Data);
                        }
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

            _logger.LogTrace($"Killing {processName}...");
            Process.Start("taskkill", $"/F /IM {processName}.exe");
            _logger.LogDebug($"{processName} is killed");
            _logger.LogTrace($"Sleeping for {TaskKillSleepTime}...");
            await Task.Delay(TaskKillSleepTime, token).ConfigureAwait(false);
        }

        static bool ProcessExists(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }

        void OnError(string message)
        {
            ProcessErrorFired?.Invoke(this, new EventArgs<string>(message));
        }

        void OnMessage(string message)
        {
            ProcessMessageFired?.Invoke(this, new EventArgs<string>(message));
        }

        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="name">The name of the process for logging.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return immediately as canceled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        async Task WaitForExitAsync(Process process, string name, CancellationToken cancellationToken)
        {
            _ = process ?? throw new ArgumentNullException(nameof(process));
            _ = name ?? throw new ArgumentNullException(nameof(name));
            var taskCompletionSource = new TaskCompletionSource<object?>();
            process.EnableRaisingEvents = true;

            // No need to wait for the already exited process;
            if (process.HasExited)
            {
                return;
            }

            process.Exited += OnProcessExited;

            cancellationToken.Register(
                () =>
                {
                    process.Exited -= OnProcessExited;
                    _logger.LogTrace($"Checking the process {name} should be canceled");
                    if (taskCompletionSource.Task.IsCompleted)
                    {
                        _logger.LogTrace($"The process {name} has already exited - no need to cancel");
                        return;
                    }

                    _logger.LogTrace($"Canceling process {name}...");
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

            await taskCompletionSource.Task.ConfigureAwait(false);
            return;

            void OnProcessExited(object? sender, EventArgs args)
            {
                _logger.LogTrace($"Handling process {name} exit...");

                var setResult = taskCompletionSource?.TrySetResult(null) ?? throw new InvalidOperationException("taskCompletionSource is null");
                if (setResult)
                {
                    _logger.LogTrace($"The process {name} has exited successfully");
                }
                else
                {
                    _logger.LogWarning($"The process {name} result cannot be set");
                }

                // ReSharper disable once AccessToModifiedClosure - the outer code needs to await the returning task before disposal
                process.Exited -= OnProcessExited;
            }
        }
    }
}
