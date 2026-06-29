using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WinSearch.Helpers;

namespace WinSearch.Services;

public static class AcrylicHelper
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMSBT_MAINWINDOW = 2; // Mica

    public static void Apply(Window window)
    {
        if (!WindowsVersionHelper.IsWindows11()) return;
        var handle = new WindowInteropHelper(window).EnsureHandle();
        ApplyMica(handle);
    }

    private static void ApplyMica(IntPtr handle)
    {
        try
        {
            int value = DWMSBT_MAINWINDOW;
            DwmSetWindowAttribute(handle, (uint)DWMWA_SYSTEMBACKDROP_TYPE, ref value, 4);
        }
        catch { }
    }

    private static void ApplyAcrylic(IntPtr handle)
    {
        try
        {
            var accent = new AccentPolicy
            {
                AccentState = 4, // ACCENT_ENABLE_ACRYLICBLURBEHIND
                GradientColor = unchecked((int)0x99000000)
            };
            var data = new WindowCompositionAttributeData
            {
                Attribute = 19, // WCA_ACCENT_POLICY
                Data = Marshal.AllocHGlobal(Marshal.SizeOf(accent)),
                SizeOfData = Marshal.SizeOf(accent)
            };
            Marshal.StructureToPtr(accent, data.Data, false);
            SetWindowCompositionAttribute(handle, ref data);
            Marshal.FreeHGlobal(data.Data);
        }
        catch { }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCompositionAttributeData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }
}
