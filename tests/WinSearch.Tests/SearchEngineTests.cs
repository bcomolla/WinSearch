using System.IO;
using WinSearch.Core;
using WinSearch.Models;
using WinSearch.Providers;
using Xunit;

namespace WinSearch.Tests;

public class SearchEngineTests
{
    private SearchEngine BuildEngine(string settingsIndexJson)
    {
        var aliases = new AliasEngine();
        var frecency = new FrecencyTracker();
        var engine = new SearchEngine(aliases, frecency);

        var path = Path.GetTempFileName();
        File.WriteAllText(path, settingsIndexJson);

        var provider = new WindowsSettingsProvider();
        provider.Load(path);
        engine.AddProvider(provider);

        File.Delete(path);
        return engine;
    }

    [Fact]
    public async Task SearchAsync_ReturnsResults_ForKnownQuery()
    {
        var json = """
            [{"id":"ms-settings:display","title":"Display settings","subtitle":"Resolution","category":"System","keywords":["display","resolution"],"uri":"ms-settings:display"}]
            """;
        var engine = BuildEngine(json);
        var results = (await engine.SearchAsync("display")).ToList();
        Assert.NotEmpty(results);
        Assert.Equal("Display settings", results[0].Title);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var engine = BuildEngine("[]");
        var results = await engine.SearchAsync("");
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_RespectsResultLimit()
    {
        var entries = Enumerable.Range(1, 20).Select(i =>
            $"{{\"id\":\"s{i}\",\"title\":\"Setting {i}\",\"subtitle\":\"\",\"category\":\"System\",\"keywords\":[\"setting\"],\"uri\":\"ms-settings:display\"}}");
        var json = $"[{string.Join(",", entries)}]";

        var engine = BuildEngine(json);
        engine.SetResultLimit(5);
        var results = (await engine.SearchAsync("setting")).ToList();
        Assert.True(results.Count <= 5);
    }
}
