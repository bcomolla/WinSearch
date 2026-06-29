using System.Windows.Media;

namespace WinSearch.Models;

public class SearchResult
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public SearchCategory Category { get; init; }
    public ImageSource? Icon { get; set; }
    public Action? Action { get; init; }
    public Action? AdminAction { get; init; }
    public bool RequiresElevation { get; init; }
    public double Score { get; set; }
    public double FrecencyBoost { get; set; }
    public double FinalScore => Score + FrecencyBoost;
}
