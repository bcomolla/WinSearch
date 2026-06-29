using System.Data.OleDb;
using System.Diagnostics;
using WinSearch.Core;
using WinSearch.Models;

namespace WinSearch.Providers;

public class FileIndexProvider : ISearchProvider
{
    public string Name => "Files";

    public bool IsAvailable { get; private set; }

    public void Initialize()
    {
        try
        {
            var svc = System.ServiceProcess.ServiceController.GetServices()
                .FirstOrDefault(s => s.ServiceName == "WSearch");
            IsAvailable = svc?.Status == System.ServiceProcess.ServiceControllerStatus.Running;
        }
        catch { IsAvailable = false; }
    }

    public async Task<IEnumerable<SearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<SearchResult>();

        var results = new List<SearchResult>();
        try
        {
            const string connStr = "Provider=Search.CollatorDSO.1;Extended Properties='Application=Windows';";
            using var conn = new OleDbConnection(connStr);
            await conn.OpenAsync(ct);

            var sql = $"SELECT System.ItemName, System.ItemPathDisplay FROM SystemIndex WHERE CONTAINS(System.FileName, '{query.Replace("'", "''")}') AND scope='file:' ORDER BY System.Search.Rank DESC";
            using var cmd = new OleDbCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync(ct);

            while (reader.Read() && results.Count < 10)
            {
                var name = reader[0] as string ?? "";
                var path = reader[1] as string ?? "";
                double score = FuzzyMatcher.Score(query, name);
                var p = path;
                results.Add(new SearchResult
                {
                    Id = $"file:{path}",
                    Title = name,
                    Subtitle = path,
                    Category = SearchCategory.File,
                    Score = score > 0 ? score : 0.5,
                    Action = () => Process.Start(new ProcessStartInfo { FileName = p, UseShellExecute = true })
                });
            }
        }
        catch { }
        return results;
    }
}
