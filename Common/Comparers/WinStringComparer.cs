using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Scar.Common.Comparers;

public class WinStringComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        return StrCmpLogicalW(x, y);
    }

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int StrCmpLogicalW(string? x, string? y);
}