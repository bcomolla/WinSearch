using System.IO;
using WinSearch.Core;
using Xunit;

namespace WinSearch.Tests;

public class AliasEngineTests
{
    private AliasEngine LoadFromYaml(string yaml)
    {
        var path = Path.GetTempFileName() + ".yaml";
        File.WriteAllText(path, yaml);
        var engine = new AliasEngine();
        engine.Load(path);
        File.Delete(path);
        return engine;
    }

    [Fact]
    public void Resolve_ReturnsOriginalQuery()
    {
        var engine = new AliasEngine();
        var results = engine.Resolve("anything").ToList();
        Assert.Contains("anything", results);
    }

    [Fact]
    public void Resolve_ExpandsAlias()
    {
        var yaml = """
            aliases:
              - terms: [wifi, wireless]
                targets: ["Network & Internet", "Wi-Fi"]
            """;
        var engine = LoadFromYaml(yaml);
        var results = engine.Resolve("wifi").ToList();
        Assert.Contains("Network & Internet", results);
        Assert.Contains("Wi-Fi", results);
    }

    [Fact]
    public void Resolve_IsCaseInsensitive()
    {
        var yaml = """
            aliases:
              - terms: [wifi]
                targets: ["Network & Internet"]
            """;
        var engine = LoadFromYaml(yaml);
        Assert.Contains("Network & Internet", engine.Resolve("WIFI"));
    }

    [Fact]
    public void Resolve_NonMatchingQuery_ReturnsOnlyOriginal()
    {
        var engine = new AliasEngine();
        var results = engine.Resolve("xyzzy").ToList();
        Assert.Single(results);
        Assert.Equal("xyzzy", results[0]);
    }
}
