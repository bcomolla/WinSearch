using Microsoft.Win32;

namespace WinSearch.Helpers;

public static class RegistryHelper
{
    public static T? GetValue<T>(RegistryKey hive, string keyPath, string valueName, T? defaultValue = default)
    {
        try
        {
            using var key = hive.OpenSubKey(keyPath);
            var val = key?.GetValue(valueName);
            if (val is T typed) return typed;
            return defaultValue;
        }
        catch { return defaultValue; }
    }

    public static void SetStartup(string appName, string exePath, bool enable)
    {
        const string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(runKey, writable: true);
            if (enable)
                key?.SetValue(appName, $"\"{exePath}\"");
            else
                key?.DeleteValue(appName, throwOnMissingValue: false);
        }
        catch { }
    }
}
