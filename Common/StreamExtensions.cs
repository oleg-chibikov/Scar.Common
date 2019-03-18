using System;
using System.IO;

namespace Scar.Common
{
    public static class StreamExtensions
    {
        public static void CopyBuffered(this Stream input, Stream output)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var buffer = new byte[byte.MaxValue];
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
            }
        }

        public static MemoryStream CreateMemoryStream(this Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var memoryStream = new MemoryStream();
            input.CopyBuffered(memoryStream);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}