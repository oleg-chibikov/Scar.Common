using System;

namespace Scar.Common.ImageProcessing.ExifExtraction;

public sealed class FilePathErrorEventArgs(string message, string filePath) : EventArgs
{
    public string FilePath { get; } = filePath ?? throw new ArgumentNullException(nameof(filePath));

    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

    public override string ToString()
    {
        return $"[{FilePath}] {Message}";
    }
}
