using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinSearch.Core;
using WinSearch.Models;

namespace WinSearch.UI.ViewModels;

public partial class OverlayViewModel : ObservableObject
{
    private readonly SearchEngine _engine;
    private CancellationTokenSource _cts = new();
    private System.Timers.Timer? _debounce;

    [ObservableProperty] private string _query = "";
    [ObservableProperty] private SearchResult? _selectedResult;
    [ObservableProperty] private bool _isSearching;

    public ObservableCollection<SearchResult> Results { get; } = new();

    public OverlayViewModel(SearchEngine engine)
    {
        _engine = engine;
        _debounce = new System.Timers.Timer(150) { AutoReset = false };
        _debounce.Elapsed += async (_, _) => await RunSearchAsync();
    }

    partial void OnQueryChanged(string value)
    {
        _debounce?.Stop();
        if (string.IsNullOrWhiteSpace(value))
        {
            Results.Clear();
            return;
        }
        _debounce?.Start();
    }

    private async Task RunSearchAsync()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsSearching = true;
        try
        {
            var results = (await _engine.SearchAsync(Query, ct)).ToList();
            if (ct.IsCancellationRequested) return;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Results.Clear();
                foreach (var r in results) Results.Add(r);
                SelectedResult = Results.FirstOrDefault();
            });
        }
        catch (OperationCanceledException) { }
        finally { IsSearching = false; }
    }

    [RelayCommand]
    public void ExecuteSelected(bool asAdmin = false)
    {
        if (SelectedResult == null) return;
        _engine.RecordSelection(SelectedResult.Id);
        try
        {
            if (asAdmin && SelectedResult.AdminAction != null)
                SelectedResult.AdminAction();
            else
                SelectedResult.Action?.Invoke();
        }
        catch { }
    }

    public void SelectNext()
    {
        if (Results.Count == 0) return;
        var idx = SelectedResult == null ? 0 : Results.IndexOf(SelectedResult) + 1;
        SelectedResult = Results[Math.Min(idx, Results.Count - 1)];
    }

    public void SelectPrev()
    {
        if (Results.Count == 0) return;
        var idx = SelectedResult == null ? 0 : Results.IndexOf(SelectedResult) - 1;
        SelectedResult = Results[Math.Max(idx, 0)];
    }

    public void Clear()
    {
        Query = "";
        Results.Clear();
        SelectedResult = null;
    }
}
