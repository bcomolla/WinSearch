using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WinSearch.Services;

public class HotkeyService : IDisposable
{
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;
    private const int HotkeyId = 9001;

    private HwndSource? _source;
    private Action? _onHotkey;

    public bool Register(Window window, uint modifiers, uint vk, Action onHotkey)
    {
        _onHotkey = onHotkey;
        var handle = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(handle);
        _source?.AddHook(WndProc);
        return RegisterHotKey(handle, HotkeyId, modifiers, vk);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            _onHotkey?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_source != null)
        {
            UnregisterHotKey(_source.Handle, HotkeyId);
            _source.RemoveHook(WndProc);
        }
    }
}
