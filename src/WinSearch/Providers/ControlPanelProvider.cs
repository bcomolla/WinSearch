using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WinSearch.Core;
using WinSearch.Models;

namespace WinSearch.Providers;

public class ControlPanelProvider : ISearchProvider
{
    public string Name => "Control Panel";

    private List<ControlPanelEntry> _entries = new();

    public void Load(string indexPath)
    {
        try
        {
            var json = File.ReadAllText(indexPath);
            _entries = JsonSerializer.Deserialize<List<ControlPanelEntry>>(json,
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
                var cmd = entry.Command;
                results.Add(new SearchResult
                {
                    Id = entry.Id,
                    Title = entry.Title,
                    Subtitle = "Control Panel",
                    Category = SearchCategory.ControlPanel,
                    Score = score,
                    Action = () => Process.Start(new ProcessStartInfo
                    {
                        FileName = "control.exe",
                        Arguments = cmd,
                        UseShellExecute = true
                    })
                });
            }
        }
        return Task.FromResult<IEnumerable<SearchResult>>(results);
    }

    private class ControlPanelEntry
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Command { get; set; } = "";
        public List<string> Keywords { get; set; } = new();
    }
}
