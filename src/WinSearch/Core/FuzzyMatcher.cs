using System.Globalization;
using System.Text;

namespace WinSearch.Core;

public static class FuzzyMatcher
{
    public static double Score(string query, string target, IEnumerable<string>? keywords = null)
    {
        if (string.IsNullOrWhiteSpace(query)) return 0;

        var q = Normalize(query);
        var t = Normalize(target);

        double best = ScorePair(q, t);

        if (keywords != null)
        {
            foreach (var kw in keywords)
            {
                var s = ScorePair(q, Normalize(kw));
                if (s > best) best = s;
            }
        }

        return best;
    }

    private static double ScorePair(string q, string t)
    {
        if (t == q) return 1.0;
        if (t.StartsWith(q)) return 0.85;
        if (t.Contains(q)) return 0.70;

        // Levenshtein scaled
        int maxLen = Math.Max(q.Length, t.Length);
        if (maxLen == 0) return 1.0;
        int dist = LevenshteinDistance(q, t);
        double ratio = 1.0 - (double)dist / maxLen;
        // Only consider it a match if 60%+ similar
        return ratio >= 0.6 ? ratio * 0.6 : 0;
    }

    public static string Normalize(string s)
    {
        s = s.ToLowerInvariant();
        var sb = new StringBuilder();
        foreach (char c in s.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString();
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var dp = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        for (int j = 1; j <= b.Length; j++)
        {
            int cost = a[i - 1] == b[j - 1] ? 0 : 1;
            dp[i, j] = Math.Min(
                Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                dp[i - 1, j - 1] + cost);
        }

        return dp[a.Length, b.Length];
    }
}
