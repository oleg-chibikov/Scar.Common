using System;

namespace Scar.Common.Events;

public class ProgressEventArgs(int current, int total) : EventArgs
{
    public int Current { get; } = current;

    public int Percentage { get; } = current * 100 / total;

    public int Total { get; } = total;

    public override string ToString()
    {
        return $"{Current} of {Total} ({Percentage}%)";
    }
}
