using System.Diagnostics;
using WinSearch.Core;
using WinSearch.Models;

namespace WinSearch.Providers;

public class ProcessProvider : ISearchProvider
{
    public string Name => "Running Processes";

    public Task<IEnumerable<SearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        var results = new List<SearchResult>();
        try
        {
            foreach (var proc in Process.GetProcesses())
            {
                double score = FuzzyMatcher.Score(query, proc.ProcessName);
                if (score > 0)
                {
                    var pid = proc.Id;
                    results.Add(new SearchResult
                    {
                        Id = $"proc:{proc.Id}",
                        Title = proc.ProcessName,
                        Subtitle = $"PID {proc.Id} — Running process",
                        Category = SearchCategory.Process,
                        Score = score,
                        Action = () =>
                        {
                            try { Process.GetProcessById(pid).Kill(); } catch { }
                        }
                    });
                }
            }
        }
        catch { }
        return Task.FromResult<IEnumerable<SearchResult>>(results);
    }
}
