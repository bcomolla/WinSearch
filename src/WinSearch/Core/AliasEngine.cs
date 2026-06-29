using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WinSearch.Core;

public class AliasEngine
{
    private readonly List<AliasEntry> _entries = new();

    public void Load(string yamlPath)
    {
        if (!File.Exists(yamlPath)) return;

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        using var reader = new StreamReader(yamlPath);
        var root = deserializer.Deserialize<AliasRoot>(reader);
        if (root?.Aliases != null)
            _entries.AddRange(root.Aliases);
    }

    public IEnumerable<string> Resolve(string query)
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { query };
        var q = query.ToLowerInvariant().Trim();

        foreach (var entry in _entries)
        {
            if (entry.Terms.Any(t => t.Equals(q, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var target in entry.Targets)
                    results.Add(target);
            }
        }

        return results;
    }

    private class AliasRoot
    {
        public List<AliasEntry> Aliases { get; set; } = new();
    }

    private class AliasEntry
    {
        public List<string> Terms { get; set; } = new();
        public List<string> Targets { get; set; } = new();
    }
}
