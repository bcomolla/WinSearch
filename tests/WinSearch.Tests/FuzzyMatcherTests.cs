using WinSearch.Core;
using Xunit;

namespace WinSearch.Tests;

public class FuzzyMatcherTests
{
    [Theory]
    [InlineData("display", "Display settings", 0.85)]
    [InlineData("displya", "Display settings", 0.0)]  // typo — levenshtein should still score
    [InlineData("wifi", "Wi-Fi", 0.0)]
    [InlineData("sound", "Sound settings", 0.85)]
    public void Score_ReturnsExpectedRange(string query, string target, double minExpected)
    {
        var score = FuzzyMatcher.Score(query, target);
        Assert.True(score >= minExpected, $"Score for '{query}' vs '{target}' was {score}, expected >= {minExpected}");
    }

    [Fact]
    public void ExactMatch_ReturnsOne()
    {
        Assert.Equal(1.0, FuzzyMatcher.Score("display", "display"));
    }

    [Fact]
    public void StartsWith_ReturnsHighScore()
    {
        var score = FuzzyMatcher.Score("disp", "display settings");
        Assert.True(score >= 0.7);
    }

    [Fact]
    public void Contains_ReturnsModerateScore()
    {
        var score = FuzzyMatcher.Score("play", "display settings");
        Assert.True(score >= 0.6);
    }

    [Fact]
    public void Normalize_HandlesCase()
    {
        Assert.Equal(FuzzyMatcher.Normalize("HELLO"), FuzzyMatcher.Normalize("hello"));
    }

    [Fact]
    public void Score_MatchesKeyword()
    {
        var score = FuzzyMatcher.Score("resolution", "Display settings",
            new[] { "resolution", "screen", "brightness" });
        Assert.True(score >= 0.85);
    }
}
