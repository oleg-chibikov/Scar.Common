using System;
using System.IO;

namespace Scar.Common;

public static class StreamExtensions
{
    public static void CopyBuffered(this Stream input, Stream output)
    {
        _ = input ?? throw new ArgumentNullException(nameof(input));
        _ = output ?? throw new ArgumentNullException(nameof(output));
        var buffer = new byte[byte.MaxValue];
        int count;
        while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, count);
        }
    }

    public static MemoryStream CreateMemoryStream(this Stream input)
    {
        _ = input ?? throw new ArgumentNullException(nameof(input));
        var memoryStream = new MemoryStream();
        input.CopyBuffered(memoryStream);
        memoryStream.Seek(0L, SeekOrigin.Begin);
        return memoryStream;
    }
}