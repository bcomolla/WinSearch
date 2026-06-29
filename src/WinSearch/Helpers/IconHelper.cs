using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinSearch.Models;

namespace WinSearch.Helpers;

public static class IconHelper
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
        ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_SMALLICON = 0x1;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

    private static readonly Dictionary<string, ImageSource?> _cache = new();

    // Well-known icon sources: shell32.dll icon indices
    private static readonly Dictionary<SearchCategory, (string dll, int index)> CategoryIcons = new()
    {
        [SearchCategory.Settings]     = (@"C:\Windows\System32\shell32.dll", 21),   // gear/settings
        [SearchCategory.ControlPanel] = (@"C:\Windows\System32\shell32.dll", 22),   // control panel
        [SearchCategory.App]          = (@"C:\Windows\System32\shell32.dll", 2),    // application
        [SearchCategory.File]         = (@"C:\Windows\System32\shell32.dll", 1),    // document
        [SearchCategory.Process]      = (@"C:\Windows\System32\shell32.dll", 3),    // running cog
        [SearchCategory.Registry]     = (@"C:\Windows\System32\regedit.exe", 0),   // regedit icon
    };

    public static ImageSource? GetCategoryIcon(SearchCategory category)
    {
        var key = $"cat:{category}";
        if (_cache.TryGetValue(key, out var cached)) return cached;

        if (!CategoryIcons.TryGetValue(category, out var src))
        {
            _cache[key] = null;
            return null;
        }

        var icon = ExtractIconFromDll(src.dll, src.index);
        _cache[key] = icon;
        return icon;
    }

    public static ImageSource? GetIcon(string path)
    {
        if (_cache.TryGetValue(path, out var cached)) return cached;

        try
        {
            var info = new SHFILEINFO();
            SHGetFileInfo(path, FILE_ATTRIBUTE_NORMAL, ref info, (uint)Marshal.SizeOf(info),
                SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);

            if (info.hIcon == IntPtr.Zero) { _cache[path] = null; return null; }

            var source = IconHandleToImageSource(info.hIcon);
            DestroyIcon(info.hIcon);
            _cache[path] = source;
            return source;
        }
        catch { _cache[path] = null; return null; }
    }

    private static ImageSource? ExtractIconFromDll(string dllPath, int index)
    {
        try
        {
            var hIcon = ExtractIcon(IntPtr.Zero, dllPath, index);
            if (hIcon == IntPtr.Zero || hIcon == (IntPtr)1) return null;
            var src = IconHandleToImageSource(hIcon);
            DestroyIcon(hIcon);
            return src;
        }
        catch { return null; }
    }

    private static ImageSource? IconHandleToImageSource(IntPtr hIcon)
    {
        try
        {
            var source = Imaging.CreateBitmapSourceFromHIcon(hIcon,
                Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        catch { return null; }
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
