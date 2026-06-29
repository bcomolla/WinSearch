using System.IO;
using System.Text.Json;
using WinSearch.Models;
using Timer = System.Threading.Timer;

namespace WinSearch.Core;

public class IndexCache
{
    private readonly string _path;
    private List<SearchResult> _cached = new();
    private Timer? _refreshTimer;

    public IndexCache()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _path = Path.Combine(appData, "WinSearch", "index.json");
    }

    public IReadOnlyList<SearchResult> Cached => _cached;

    public void StartAutoRefresh(Func<Task<IEnumerable<SearchResult>>> builder)
    {
        _refreshTimer = new Timer(async _ =>
        {
            var results = await builder();
            _cached = results.ToList();
            Save();
        }, null, TimeSpan.Zero, TimeSpan.FromHours(6));
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var dto = _cached.Select(r => new CachedResultDto
            {
                Id = r.Id, Title = r.Title, Subtitle = r.Subtitle, Category = r.Category
            });
            File.WriteAllText(_path, JsonSerializer.Serialize(dto));
        }
        catch { }
    }

    private class CachedResultDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public SearchCategory Category { get; set; }
    }
}
