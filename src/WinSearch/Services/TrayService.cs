using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace WinSearch.Services;

public class TrayService : IDisposable
{
    private NotifyIcon? _tray;

    public event EventHandler? ShowOverlay;
    public event EventHandler? OpenSettings;
    public event EventHandler? RebuildIndex;

    public void Initialize(string iconPath)
    {
        _tray = new NotifyIcon
        {
            Text = "WinSearch",
            Visible = true
        };

        if (File.Exists(iconPath))
            _tray.Icon = new Icon(iconPath);

        var menu = new ContextMenuStrip();
        menu.Items.Add("Open Search", null, (_, _) => ShowOverlay?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Settings", null, (_, _) => OpenSettings?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Rebuild Index", null, (_, _) => RebuildIndex?.Invoke(this, EventArgs.Empty));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => Application.Current.Shutdown());

        _tray.ContextMenuStrip = menu;
        _tray.DoubleClick += (_, _) => ShowOverlay?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _tray?.Dispose();
    }
}
