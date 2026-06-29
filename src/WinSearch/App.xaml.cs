using System.Threading;
using System.Windows;
using WinSearch.Core;
using System.IO;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using WinSearch.Providers;
using WinSearch.Services;
using WinSearch.UI;

namespace WinSearch;

public partial class App : Application
{
    private static Mutex? _mutex;
    private HotkeyService? _hotkey;
    private TrayService? _tray;
    private ThemeService? _theme;
    private OverlayWindow? _overlay;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, "WinSearch_SingleInstance", out bool isNew);
        if (!isNew)
        {
            MessageBox.Show("WinSearch is already running.", "WinSearch");
            Shutdown();
            return;
        }

        base.OnStartup(e);

        var dataDir = AppContext.BaseDirectory;
        var aliasPath = Path.Combine(dataDir, "Data", "aliases.yaml");
        var settingsIndexPath = Path.Combine(dataDir, "Data", "settings-index.json");
        var controlPanelIndexPath = Path.Combine(dataDir, "Data", "controlpanel-index.json");

        var aliases = new AliasEngine();
        aliases.Load(aliasPath);

        var frecency = new FrecencyTracker();
        frecency.Load();

        var engine = new SearchEngine(aliases, frecency);

        var settingsProvider = new WindowsSettingsProvider();
        settingsProvider.Load(settingsIndexPath);
        engine.AddProvider(settingsProvider);

        var cpProvider = new ControlPanelProvider();
        cpProvider.Load(controlPanelIndexPath);
        engine.AddProvider(cpProvider);

        var appsProvider = new InstalledAppsProvider();
        appsProvider.Initialize();
        engine.AddProvider(appsProvider);

        engine.AddProvider(new ProcessProvider());
        engine.AddProvider(new RegistryProvider());

        var fileProvider = new FileIndexProvider();
        fileProvider.Initialize();
        if (fileProvider.IsAvailable)
            engine.AddProvider(fileProvider);

        _theme = new ThemeService();
        _theme.StartWatching();
        _theme.ThemeChanged += (_, _) => _theme.Apply(Resources);

        _overlay = new OverlayWindow(engine);

        _hotkey = new HotkeyService();
        // We need a window handle to register the hotkey. Show the overlay hidden,
        // wait for it to load (which creates the HWND), then register and hide it.
        _overlay.Show();
        _overlay.Hide();
        bool hotkeyRegistered = _hotkey.Register(_overlay, 0x0008 /* MOD_WIN */, 0x20 /* VK_SPACE */,
            () => Dispatcher.Invoke(ToggleOverlay));
        if (!hotkeyRegistered)
            MessageBox.Show("Could not register Win+Space hotkey — it may be in use by another application.\nYou can change it in Settings.", "WinSearch");

        _tray = new TrayService();
        _tray.ShowOverlay += (_, _) => Dispatcher.Invoke(ToggleOverlay);
        _tray.OpenSettings += (_, _) => Dispatcher.Invoke(OpenSettings);
        _tray.Initialize(Path.Combine(dataDir, "assets", "icon-tray.ico"));
    }

    private void ToggleOverlay()
    {
        if (_overlay == null) return;
        if (_overlay.IsVisible)
            _overlay.HideOverlay();
        else
            _overlay.ShowOverlay();
    }

    private void OpenSettings()
    {
        var win = new SettingsWindow();
        win.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkey?.Dispose();
        _tray?.Dispose();
        _mutex?.ReleaseMutex();
        base.OnExit(e);
    }
}
