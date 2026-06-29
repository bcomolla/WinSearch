using System.IO;
using System.Text.Json;

namespace WinSearch.Core;

public class FrecencyTracker
{
    private readonly string _path;
    private Dictionary<string, FrecencyEntry> _data = new();

    public FrecencyTracker()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _path = Path.Combine(appData, "WinSearch", "frecency.json");
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            var json = File.ReadAllText(_path);
            _data = JsonSerializer.Deserialize<Dictionary<string, FrecencyEntry>>(json) ?? new();
        }
        catch { _data = new(); }
    }

    public void Record(string resultId)
    {
        if (!_data.TryGetValue(resultId, out var entry))
            entry = _data[resultId] = new FrecencyEntry();

        entry.Count++;
        entry.LastUsed = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Save();
    }

    public double GetBoost(string resultId)
    {
        if (!_data.TryGetValue(resultId, out var entry)) return 0;

        var ageHours = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - entry.LastUsed) / 3600.0;
        double recency = Math.Exp(-ageHours / 168.0); // decay over ~1 week
        return Math.Min(0.3, entry.Count * 0.02 * recency);
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(_data));
        }
        catch { }
    }

    private class FrecencyEntry
    {
        public int Count { get; set; }
        public long LastUsed { get; set; }
    }
}
