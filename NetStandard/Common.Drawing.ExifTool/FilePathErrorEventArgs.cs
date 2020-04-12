using System;

namespace Scar.Common.Drawing.ExifTool
{
    public sealed class FilePathErrorEventArgs : EventArgs
    {
        public FilePathErrorEventArgs(string message, string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string FilePath { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"[{FilePath}] {Message}";
        }
    }
}