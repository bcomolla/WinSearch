using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WinSearch.Core;
using WinSearch.Models;

namespace WinSearch.Providers;

public class WindowsSettingsProvider : ISearchProvider
{
    public string Name => "Windows Settings";

    private List<SettingsEntry> _entries = new();

    public void Load(string indexPath)
    {
        try
        {
            var json = File.ReadAllText(indexPath);
            _entries = JsonSerializer.Deserialize<List<SettingsEntry>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { }
    }

    public Task<IEnumerable<SearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        var results = new List<SearchResult>();
        foreach (var entry in _entries)
        {
            double score = FuzzyMatcher.Score(query, entry.Title, entry.Keywords);
            if (score > 0)
            {
                var uri = entry.Uri;
                results.Add(new SearchResult
                {
                    Id = entry.Id,
                    Title = entry.Title,
                    Subtitle = entry.Subtitle,
                    Category = SearchCategory.Settings,
                    Score = score,
                    Action = () => Process.Start(new ProcessStartInfo
                    {
                        FileName = uri,
                        UseShellExecute = true
                    })
                });
            }
        }
        return Task.FromResult<IEnumerable<SearchResult>>(results);
    }

    private class SettingsEntry
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Category { get; set; } = "";
        public List<string> Keywords { get; set; } = new();
        public string Uri { get; set; } = "";
    }
}
