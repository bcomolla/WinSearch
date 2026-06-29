using WinSearch.Models;

namespace WinSearch.Providers;

public interface ISearchProvider
{
    string Name { get; }
    Task<IEnumerable<SearchResult>> SearchAsync(string query, CancellationToken ct = default);
}
