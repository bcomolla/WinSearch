using System.Diagnostics;
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
                            var exe = sub?.GetValue("DisplayIcon") as string;
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            apps.Add(new AppEntry { Name = name, ExecutablePath = exe ?? "" });
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        _apps = apps.DistinctBy(a => a.Name).ToList();
    }

    public Task<IEnumerable<SearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        var results = new List<SearchResult>();
        foreach (var app in _apps)
        {
            double score = FuzzyMatcher.Score(query, app.Name);
            if (score > 0)
            {
                var exePath = app.ExecutablePath.Split(',')[0].Trim('"');
                results.Add(new SearchResult
                {
                    Id = $"app:{app.Name}",
                    Title = app.Name,
                    Subtitle = "Application",
                    Category = SearchCategory.App,
                    Score = score,
                    Action = string.IsNullOrEmpty(exePath) ? null : () =>
                        Process.Start(new ProcessStartInfo { FileName = exePath, UseShellExecute = true })
                });
            }
        }
        return Task.FromResult<IEnumerable<SearchResult>>(results);
    }

    private class AppEntry
    {
        public string Name { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
    }
}
