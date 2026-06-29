using System.Diagnostics;
using WinSearch.Core;
using WinSearch.Models;

namespace WinSearch.Providers;

public class RegistryProvider : ISearchProvider
{
    public string Name => "Registry";

    private static readonly List<(string Id, string Title, string Path)> CommonKeys = new()
    {
        ("reg:run", "Startup Programs (Registry)", @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"),
        ("reg:runonce", "Run Once (Registry)", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce"),
        ("reg:uninstall", "Installed Programs (Registry)", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        ("reg:env", "Environment Variables (Registry)", @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment"),
        ("reg:theme", "Theme Settings (Registry)", @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"),
    };

    public Task<IEnumerable<SearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        var results = new List<SearchResult>();
        foreach (var (id, title, path) in CommonKeys)
        {
            double score = FuzzyMatcher.Score(query, title);
            if (score > 0)
            {
                var p = path;
                results.Add(new SearchResult
                {
                    Id = id,
                    Title = title,
                    Subtitle = path,
                    Category = SearchCategory.Registry,
                    Score = score,
                    Action = () => Process.Start(new ProcessStartInfo
                    {
                        FileName = "regedit.exe",
                        UseShellExecute = true
                    })
                });
            }
        }
        return Task.FromResult<IEnumerable<SearchResult>>(results);
    }
}
