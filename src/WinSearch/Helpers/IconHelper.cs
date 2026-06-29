using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WinSearch.Helpers;

public static class IconHelper
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
        ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_SMALLICON = 0x1;

    private static readonly Dictionary<string, ImageSource?> _cache = new();

    public static ImageSource? GetIcon(string path)
    {
        if (_cache.TryGetValue(path, out var cached)) return cached;

        try
        {
            var info = new SHFILEINFO();
            SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf(info), SHGFI_ICON | SHGFI_SMALLICON);
            if (info.hIcon == IntPtr.Zero) { _cache[path] = null; return null; }

            using var icon = Icon.FromHandle(info.hIcon);
            var source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            _cache[path] = source;
            return source;
        }
        catch { _cache[path] = null; return null; }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }
}
