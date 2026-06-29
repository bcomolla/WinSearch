# WinSearch

Global hotkey search overlay for Windows settings, apps, and files. Built with C# WPF on .NET 8.

## Build

```bash
dotnet restore
dotnet build
dotnet run --project src/WinSearch/WinSearch.csproj
dotnet test tests/WinSearch.Tests/
dotnet publish src/WinSearch/WinSearch.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

## Architecture

- **Core/** — SearchEngine, FuzzyMatcher, AliasEngine, FrecencyTracker, IndexCache
- **Providers/** — One ISearchProvider per scope (Settings, ControlPanel, Apps, Files, Processes, Registry)
- **Services/** — HotkeyService (Win32 RegisterHotKey), TrayService (NotifyIcon), ThemeService, AcrylicHelper
- **UI/** — OverlayWindow (main search), SettingsWindow; MVVM via CommunityToolkit.Mvvm
- **Data/** — aliases.yaml, settings-index.json (~90 ms-settings: entries), controlpanel-index.json

## Key notes

- Single-instance enforced via Mutex in App.xaml.cs
- Default hotkey: Win+Space (configurable in settings)
- All provider searches are async; search box debounced 150ms
- Acrylic: Windows 11 uses DWM Mica; Windows 10 uses SetWindowCompositionAttribute
- FileIndexProvider only activates if Windows Search service (WSearch) is running
