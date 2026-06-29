using WinSearch.Models;
using WinSearch.Providers;

namespace WinSearch.Core;

public class SearchEngine
{
    private readonly List<ISearchProvider> _providers = new();
    private readonly AliasEngine _aliases;
    private readonly FrecencyTracker _frecency;
    private int _resultLimit = 8;

    public SearchEngine(AliasEngine aliases, FrecencyTracker frecency)
    {
        _aliases = aliases;
        _frecency = frecency;
    }

    public void AddProvider(ISearchProvider provider) => _providers.Add(provider);
    public void SetResultLimit(int limit) => _resultLimit = limit;

    public async Task<IEnumerable<SearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<SearchResult>();

        var terms = _aliases.Resolve(query).ToList();

        var tasks = _providers.Select(p => SearchProviderAsync(p, terms, ct));
        var allResults = (await Task.WhenAll(tasks)).SelectMany(r => r).ToList();

        // Deduplicate by Id, keeping highest score
        var deduped = allResults
            .GroupBy(r => r.Id)
            .Select(g => g.OrderByDescending(r => r.Score).First())
            .ToList();

        // Apply frecency boost
        foreach (var r in deduped)
            r.FrecencyBoost = _frecency.GetBoost(r.Id);

        return deduped
            .OrderByDescending(r => r.FinalScore)
            .Take(_resultLimit);
    }

    private async Task<IEnumerable<SearchResult>> SearchProviderAsync(
        ISearchProvider provider, IEnumerable<string> terms, CancellationToken ct)
    {
        try
        {
            var tasks = terms.Select(t => provider.SearchAsync(t, ct));
            var results = (await Task.WhenAll(tasks)).SelectMany(r => r);
            return results
                .GroupBy(r => r.Id)
                .Select(g => g.OrderByDescending(r => r.Score).First());
        }
        catch { return Enumerable.Empty<SearchResult>(); }
    }

    public void RecordSelection(string resultId) => _frecency.Record(resultId);
}
