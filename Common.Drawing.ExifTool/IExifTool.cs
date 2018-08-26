using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Scar.Common.Drawing.Metadata;

namespace Scar.Common.Drawing.ExifTool
{
    public interface IExifTool
    {
        event EventHandler<FilePathErrorEventArgs> Error;
        event EventHandler<FilePathProgressEventArgs> Progress;

        [NotNull]
        Task SetOrientationAsync(Orientation orientation, [NotNull] string[] paths, bool backup, CancellationToken token);

        [NotNull]
        Task SetOrientationAsync(Orientation orientation, [NotNull] string path, bool backup, CancellationToken token);

        [NotNull]
        Task ShiftDateAsync(TimeSpan shiftBy, bool plus, [NotNull] string[] paths, bool backup, CancellationToken token);

        [NotNull]
        Task ShiftDateAsync(TimeSpan shiftBy, bool plus, [NotNull] string path, bool backup, CancellationToken token);
    }
}