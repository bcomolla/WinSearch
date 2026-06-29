using Microsoft.Win32;
using System.Windows;
using Application = System.Windows.Application;

namespace WinSearch.Services;

public class ThemeService
{
    private const string ThemeKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string ThemeValue = "AppsUseLightTheme";

    public bool IsLightTheme => GetIsLightTheme();

    public event EventHandler<bool>? ThemeChanged;

    public void StartWatching()
    {
        SystemEvents.UserPreferenceChanged += (_, _) =>
        {
            Application.Current?.Dispatcher.Invoke(() =>
                ThemeChanged?.Invoke(this, IsLightTheme));
        };
    }

    private static bool GetIsLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(ThemeKey);
            return key?.GetValue(ThemeValue) is int v && v == 1;
        }
        catch { return true; }
    }

    public void Apply(ResourceDictionary appResources)
    {
        var theme = IsLightTheme ? "Light" : "Dark";
        var uri = new Uri($"pack://application:,,,/UI/Themes/{theme}.xaml", UriKind.Absolute);
        var existing = appResources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Themes/") == true);
        if (existing != null) appResources.MergedDictionaries.Remove(existing);
        appResources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
    }
}
