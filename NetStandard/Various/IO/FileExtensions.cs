using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Scar.Common.IO
{
    public static class FileExtensions
    {
        public static string GetFreeFileName(string filePath)
        {
            _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
            var count = 1;

            var fileNameOnly = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath == null)
            {
                throw new InvalidOperationException("DirectoryName is null");
            }

            var newFilePath = filePath;

            while (File.Exists(newFilePath))
            {
                var tempFileName = $"{fileNameOnly} ({count++})";
                newFilePath = Path.Combine(directoryPath, tempFileName + extension);
            }

            return newFilePath;
        }

        public static void OpenFile(this string filePath)
        {
            _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
            {
                return;
            }

            using var process = new Process { StartInfo = new ProcessStartInfo(filePath) { UseShellExecute = false } };
            process.Start();
        }

        public static void OpenFileInExplorer(this string filePath)
        {
            _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
            {
                return;
            }

            using var process = new Process { StartInfo = { UseShellExecute = true, FileName = "explorer.exe", Arguments = $"/select,\"{filePath}\"" } };
            process.Start();
        }

        public static async Task<byte[]> ReadFileAsync(this string filename, CancellationToken cancellationToken)
        {
            await using var file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var buff = new byte[file.Length];
            await file.ReadAsync(buff.AsMemory(0, (int)file.Length), cancellationToken).ConfigureAwait(false);
            return buff;
        }

        public static string RenameFile(this string oldFilePath, string newFilePath)
        {
            _ = oldFilePath ?? throw new ArgumentNullException(nameof(oldFilePath));
            _ = newFilePath ?? throw new ArgumentNullException(nameof(newFilePath));
            newFilePath = GetFreeFileName(newFilePath);
            File.Move(oldFilePath, newFilePath);
            return newFilePath;
        }
    }
}
