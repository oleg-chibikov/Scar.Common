using System.Windows;

namespace Scar.Common.WPF.Screen
{
    public interface IWPFScreen
    {
        Rect DeviceBounds { get; }

        string DeviceName { get; }

        bool IsPrimary { get; }

        Rect WorkingArea { get; }
    }
}
