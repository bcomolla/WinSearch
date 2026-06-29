using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinSearch.Models;

namespace WinSearch.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly string _settingsPath;

    [ObservableProperty] private AppSettings _settings = new();

    public SettingsViewModel()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsPath = Path.Combine(appData, "WinSearch", "settings.json");
        Load();
    }

    [RelayCommand]
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return;
            var json = File.ReadAllText(_settingsPath);
            Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new();
        }
        catch { Settings = new(); }
    }
}
