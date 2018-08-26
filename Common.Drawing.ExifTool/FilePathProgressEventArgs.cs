using System;
using JetBrains.Annotations;
using Scar.Common.Events;

namespace Scar.Common.Drawing.ExifTool
{
    public sealed class FilePathProgressEventArgs : ProgressEventArgs
    {
        public FilePathProgressEventArgs(int current, int total, [NotNull] string filePath)
            : base(current, total)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        [NotNull]
        public string FilePath { get; }

        public override string ToString()
        {
            return $"[{FilePath}] {base.ToString()}";
        }
    }
}