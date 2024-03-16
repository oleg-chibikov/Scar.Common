using System.Collections;
using System.Runtime.InteropServices;

namespace Scar.Common.Comparers;

public class WinComparer : IComparer
{
    public int Compare(object? x, object? y)
    {
        return StrCmpLogicalW(x?.ToString(), y?.ToString());
    }

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int StrCmpLogicalW(string? x, string? y);
}
