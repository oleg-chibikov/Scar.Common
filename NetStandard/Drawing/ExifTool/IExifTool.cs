using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.Drawing.Metadata;

namespace Scar.Common.Drawing
{
    public interface IExifTool
    {
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "This name is OK")]
        event EventHandler<FilePathErrorEventArgs> Error;

        event EventHandler<FilePathProgressEventArgs> Progress;

        Task SetOrientationAsync(Orientation orientation, string[] paths, bool backup, CancellationToken token);

        Task SetOrientationAsync(Orientation orientation, string path, bool backup, CancellationToken token);

        Task ShiftDateAsync(TimeSpan shiftBy, bool plus, string[] paths, bool backup, CancellationToken token);

        Task ShiftDateAsync(TimeSpan shiftBy, bool plus, string path, bool backup, CancellationToken token);
    }
}
