using System;
using Scar.Common.Events;

namespace Scar.Common.ImageProcessing.ExifExtraction
{
    public sealed class FilePathProgressEventArgs : ProgressEventArgs
    {
        public FilePathProgressEventArgs(int current, int total, string filePath) : base(current, total)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public string FilePath { get; }

        public override string ToString()
        {
            return $"[{FilePath}] {base.ToString()}";
        }
    }
}
