using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scar.Common.Drawing.Metadata;
using Scar.Common.Events;
using Scar.Common.Processes;

namespace Scar.Common.ImageProcessing.ExifExtraction
{
    public sealed class ExifTool : IExifTool, IDisposable
    {
        static readonly Regex ProgressRegex = new Regex(@"======== (.*) \[(\d+)\/(\d+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex ErrorRegex = new Regex(@"Error\: (.*) - (.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static readonly string[] MessageSplitters =
        {
            Environment.NewLine
        };

        /// <summary>
        /// Allows only one exif operation at a time
        /// //TODO: Allow simultaneous operations for different paths? - dictionary of semaphores.
        /// </summary>
        readonly SemaphoreSlim _exifOperationSemaphore = new SemaphoreSlim(1, 1);

        readonly string _exifToolPath = "exiftool.exe";
        readonly ILogger _logger;
        readonly IProcessUtility _processUtility;

        // TODO: TEST BMP, png etc
        public ExifTool(IProcessUtility processUtility, ILogger<ProcessUtility> logger)
        {
            _processUtility = processUtility ?? throw new ArgumentNullException(nameof(processUtility));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processUtility.ProcessMessageFired += ProcessUtility_ProcessMessageFired;
            _processUtility.ProcessErrorFired += ProcessUtility_ProcessErrorFired;
        }

        public event EventHandler<FilePathProgressEventArgs>? Progress;

        public event EventHandler<FilePathErrorEventArgs>? Error;

        public async Task SetOrientationAsync(Orientation orientation, string[] paths, bool backup, CancellationToken token)
        {
            _ = paths ?? throw new ArgumentNullException(nameof(paths));
            _logger.LogInformation($"Setting orientation to {orientation} for {paths.Length} paths...");
            await PerformExifOperationAsync(paths, backup, $"-Orientation={(int)orientation}", token).ConfigureAwait(false);
        }

        public async Task SetOrientationAsync(Orientation orientation, string path, bool backup, CancellationToken token)
        {
            await SetOrientationAsync(
                    orientation,
                    new[]
                    {
                        path
                    },
                    backup,
                    token)
                .ConfigureAwait(false);
        }

        public async Task ShiftDateAsync(TimeSpan shiftBy, bool plus, string[] paths, bool backup, CancellationToken token)
        {
            _ = paths ?? throw new ArgumentNullException(nameof(paths));
            var sign = GetSign(plus);
            _logger.LogInformation($"Shifting date by {sign}{shiftBy} for {paths.Length} paths...");
            await PerformExifOperationAsync(paths, backup, $"-AllDates{sign}=\"{shiftBy:dd\\ hh\\:mm\\:ss}\"", token).ConfigureAwait(false);
        }

        public async Task ShiftDateAsync(TimeSpan shiftBy, bool plus, string path, bool backup, CancellationToken token)
        {
            await ShiftDateAsync(
                    shiftBy,
                    plus,
                    new[]
                    {
                        path
                    },
                    backup,
                    token)
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            _processUtility.ProcessMessageFired -= ProcessUtility_ProcessMessageFired;
            _processUtility.ProcessErrorFired -= ProcessUtility_ProcessErrorFired;
            _exifOperationSemaphore.Dispose();
        }

        static string DecodeFromUtf8(string path)
        {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(path));
        }

        static string EncodeToUtf8(string path)
        {
            return Encoding.Default.GetString(Encoding.UTF8.GetBytes(path));
        }

        static string GetSign(bool plus)
        {
            return plus ? "+" : "-";
        }

        void OnError(FilePathErrorEventArgs eventArgs)
        {
            Error?.Invoke(this, eventArgs);
        }

        void OnProgress(FilePathProgressEventArgs eventArgs)
        {
            Progress?.Invoke(this, eventArgs);
        }

        async Task PerformExifOperationAsync(string[] paths, bool backup, string operation, CancellationToken token)
        {
            _ = paths ?? throw new ArgumentNullException(nameof(paths));
            _ = operation ?? throw new ArgumentNullException(nameof(operation));
            var backupArg = backup ? null : " -overwrite_original -progress";
            await _exifOperationSemaphore.WaitAsync(token).ConfigureAwait(false);

            // By default exif tool receives ??? instead of valid cyrillic paths - need to reencode
            var encodedPaths = paths.Select(path => $"\"{EncodeToUtf8(path)}\"");
            var command = $"-charset filename=utf8 {operation}{backupArg} -n {string.Join(" ", encodedPaths)}";
            try
            {
                var processResult = await _processUtility.ExecuteCommandAsync(_exifToolPath, command, token).ConfigureAwait(false);
                if (processResult.IsError)
                {
                    throw new InvalidOperationException(processResult.Error);
                }
            }
            finally
            {
                _exifOperationSemaphore.Release();
            }
        }

        void ProcessUtility_ProcessErrorFired(object sender, EventArgs<string> e)
        {
            _logger.LogDebug($"Received error from process utility: {e.Parameter}");
            var messages = e.Parameter.Split(MessageSplitters, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            foreach (var message in messages)
            {
                _logger.LogTrace($"Processing {message}...");
                var match = ErrorRegex.Match(message);
                if (!match.Success)
                {
                    continue;
                }

                var groups = match.Groups;
                var filePath = DecodeFromUtf8(groups[2].Value).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                var errorMessage = groups[1].Value;
                var eventArgs = new FilePathErrorEventArgs(errorMessage, filePath);
                _logger.LogTrace($"Reporting error: {eventArgs}...");
                OnError(eventArgs);
            }
        }

        void ProcessUtility_ProcessMessageFired(object sender, EventArgs<string> e)
        {
            _logger.LogDebug($"Received message from process utility: {e.Parameter}");
            var messages = e.Parameter.Split(MessageSplitters, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            foreach (var message in messages)
            {
                _logger.LogTrace($"Processing {message}...");
                var match = ProgressRegex.Match(message);
                if (!match.Success)
                {
                    continue;
                }

                var groups = match.Groups;
                var filePath = DecodeFromUtf8(groups[1].Value).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                var current = int.Parse(groups[2].Value, CultureInfo.InvariantCulture);
                var total = int.Parse(groups[3].Value, CultureInfo.InvariantCulture);
                _logger.LogTrace($"Progress detected:  {current} of {total}");
                if (current > total)
                {
                    _logger.LogWarning("Incorrect percentage");
                    return;
                }

                var eventArgs = new FilePathProgressEventArgs(current, total, filePath);
                _logger.LogTrace($"Reporting progress: {eventArgs}...");
                OnProgress(eventArgs);
            }
        }
    }
}
