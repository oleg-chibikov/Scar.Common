using System;
using JetBrains.Annotations;

namespace Scar.Common.Drawing.ExifTool
{
    public sealed class FilePathErrorEventArgs : EventArgs
    {
        public FilePathErrorEventArgs([NotNull] string message, [NotNull] string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        [NotNull]
        public string FilePath { get; }

        [NotNull]
        public string Message { get; }

        public override string ToString()
        {
            return $"[{FilePath}] {Message}";
        }
    }
}