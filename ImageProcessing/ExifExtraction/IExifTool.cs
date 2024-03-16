using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.ImageProcessing.Metadata;

namespace Scar.Common.ImageProcessing.ExifExtraction;

public interface IExifTool
{
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "This name is OK")]
    event EventHandler<FilePathErrorEventArgs> Error;

    event EventHandler<FilePathProgressEventArgs> Progress;

    Task SetOrientationAsync(Orientation orientation, string[] paths, bool backup, CancellationToken cancellationToken);

    Task SetOrientationAsync(Orientation orientation, string path, bool backup, CancellationToken cancellationToken);

    Task ShiftDateAsync(TimeSpan shiftBy, bool plus, string[] paths, bool backup, CancellationToken cancellationToken);

    Task ShiftDateAsync(TimeSpan shiftBy, bool plus, string path, bool backup, CancellationToken cancellationToken);
}
