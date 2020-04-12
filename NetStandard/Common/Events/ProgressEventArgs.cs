using System;

namespace Scar.Common.Events
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(int current, int total)
        {
            Percentage = current * 100 / total;
            Current = current;
            Total = total;
        }

        public int Current { get; }

        public int Percentage { get; }

        public int Total { get; }

        public override string ToString()
        {
            return $"{Current} of {Total} ({Percentage}%)";
        }
    }
}