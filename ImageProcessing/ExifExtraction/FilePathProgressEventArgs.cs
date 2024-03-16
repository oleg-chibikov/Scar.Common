using System;
using Scar.Common.Events;

namespace Scar.Common.ImageProcessing.ExifExtraction;

public sealed class FilePathProgressEventArgs(int current, int total, string filePath) : ProgressEventArgs(
    current, total)
{
    public string FilePath { get; } = filePath ?? throw new ArgumentNullException(nameof(filePath));

    public override string ToString()
    {
        return $"[{FilePath}] {base.ToString()}";
    }
}
