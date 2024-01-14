using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;

namespace Scar.Common.WPF.Screen;

public class WPFScreen : IWPFScreen
{
    readonly System.Windows.Forms.Screen _screen;

    public WPFScreen(System.Windows.Forms.Screen screen)
    {
        _screen = screen;
    }

    public static WPFScreen Primary => new (System.Windows.Forms.Screen.PrimaryScreen!);

    public Rect DeviceBounds => GetRect(_screen.Bounds);

    public Rect WorkingArea => GetRect(_screen.WorkingArea);

    public bool IsPrimary => _screen.Primary;

    public string DeviceName => _screen.DeviceName;

    public static IEnumerable<WPFScreen> AllScreens()
    {
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            yield return new WPFScreen(screen);
        }
    }

    public static WPFScreen GetScreenFrom(Window window)
    {
        var windowInteropHelper = new WindowInteropHelper(window);
        var screen = System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);
        var wpfScreen = new WPFScreen(screen);
        return wpfScreen;
    }

    public static WPFScreen GetScreenFrom(System.Windows.Point point)
    {
        var x = (int)Math.Round(point.X);
        var y = (int)Math.Round(point.Y);

        // are x,y device-independent-pixels ??
        var drawingPoint = new System.Drawing.Point(x, y);
        var screen = System.Windows.Forms.Screen.FromPoint(drawingPoint);
        var wpfScreen = new WPFScreen(screen);

        return wpfScreen;
    }

    static Rect GetRect(Rectangle value)
    {
        // should x, y, width, height be device-independent-pixels ??
        return new Rect { X = value.X, Y = value.Y, Width = value.Width, Height = value.Height };
    }
}