using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Scar.Common.WPF.Converters
{
    static class IconExtensions
    {
        public static ImageSource ToImageSource(this Icon icon)
        {
            _ = icon ?? throw new ArgumentNullException(nameof(icon));
            var bitmap = icon.ToBitmap();

            // ReSharper disable once StyleCop.SA1305
            var hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

        // ReSharper disable once StyleCop.SA1305
        static extern bool DeleteObject(IntPtr hObject);
    }
}
