using Microsoft.Win32;

namespace WinSearch.Helpers;

public static class WindowsVersionHelper
{
    private static readonly Version _version = Environment.OSVersion.Version;

    public static bool IsWindows11() => _version.Build >= 22000;

    public static bool IsWindows10() => _version.Major == 10 && _version.Build < 22000;
}
