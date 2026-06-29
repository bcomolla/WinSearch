using System.Diagnostics;

namespace WinSearch.Services;

public static class ElevationHelper
{
    public static bool IsElevated =>
        new System.Security.Principal.WindowsPrincipal(
            System.Security.Principal.WindowsIdentity.GetCurrent())
            .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

    public static void RunElevated(string fileName, string? arguments = null)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments ?? "",
            Verb = "runas",
            UseShellExecute = true
        });
    }
}
