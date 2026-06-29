using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WinSearch.Core;
using WinSearch.Models;
using Timer = System.Threading.Timer;

namespace WinSearch.Providers;

public class InstalledAppsProvider : ISearchProvider
{
    public string Name => "Installed Apps";

    private List<AppEntry> _apps = new();
    private Timer? _refreshTimer;

    public void Initialize()
    {
        Refresh();
        _refreshTimer = new Timer(_ => Refresh(), null, TimeSpan.FromHours(6), TimeSpan.FromHours(6));
    }

    private void Refresh()
    {
        var apps = new List<AppEntry>();
        var keys = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (var key in keys)
        {
            foreach (var hive in new[] { Registry.LocalMachine, Registry.CurrentUser })
            {
                try
                {
                    using var root = hive.OpenSubKey(key);
                    if (root == null) continue;
                    foreach (var subName in root.GetSubKeyNames())
                    {
                        try
                        {
                            using var sub = root.OpenSubKey(subName);
                            var name = sub?.GetValue("DisplayName") as string;
                            var iconRaw = sub?.GetValue("DisplayIcon") as string;
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            apps.Add(new AppEntry
                            {
                                Name = name,
                                ExecutablePath = iconRaw?.Split(',')[0].Trim('"') ?? "",
                                Icon = ExtractIcon(iconRaw)
                            });
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        _apps = apps.DistinctBy(a => a.Name).ToList();
    }

    private static BitmapSource? ExtractIcon(string? rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath)) return null;
        var path = rawPath.Split(',')[0].Trim('"').Trim();
        if (!System.IO.File.Exists(path)) return null;
        try
        {
            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
            if (icon == null) return null;
            var bmp = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    public Task<IEnumerable<SearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        var results = new List<SearchResult>();
        foreach (var app in _apps)
        {
            double score = FuzzyMatcher.Score(query, app.Name);
            if (score > 0)
            {
                results.Add(new SearchResult
                {
                    Id = $"app:{app.Name}",
                    Title = app.Name,
                    Subtitle = "Application",
                    Category = SearchCategory.App,
                    Score = score,
                    Icon = app.Icon,
                    Action = string.IsNullOrEmpty(app.ExecutablePath) ? null : () =>
                        Process.Start(new ProcessStartInfo { FileName = app.ExecutablePath, UseShellExecute = true })
                });
            }
        }
        return Task.FromResult<IEnumerable<SearchResult>>(results);
    }

    private class AppEntry
    {
        public string Name { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public BitmapSource? Icon { get; set; }
    }
}
