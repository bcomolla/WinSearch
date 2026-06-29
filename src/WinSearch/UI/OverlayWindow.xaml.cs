using System.Windows;
using System.Windows.Input;
using WinSearch.Core;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using WinSearch.Services;
using WinSearch.UI.ViewModels;

namespace WinSearch.UI;

public partial class OverlayWindow : Window
{
    private readonly OverlayViewModel _vm;

    public OverlayWindow(SearchEngine engine)
    {
        InitializeComponent();
        _vm = new OverlayViewModel(engine);
        DataContext = _vm;

        Loaded += (_, _) =>
        {
            AcrylicHelper.Apply(this);
            CentreOnPrimaryMonitor();
        };
    }

    private void CentreOnPrimaryMonitor()
    {
        var screen = SystemParameters.WorkArea;
        Left = (screen.Width - Width) / 2;
        Top = screen.Height * 0.25;
    }

    public void ShowOverlay()
    {
        Show();
        Activate();
        SearchBox.Focus();
        SearchBox.SelectAll();
    }

    public void HideOverlay()
    {
        _vm.Clear();
        Hide();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Escape:
                HideOverlay();
                break;
            case Key.Down:
                _vm.SelectNext();
                break;
            case Key.Up:
                _vm.SelectPrev();
                break;
        }
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            bool admin = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            _vm.ExecuteSelected(admin);
            HideOverlay();
            e.Handled = true;
        }
    }

    private void Result_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _vm.ExecuteSelectedCommand.Execute(false);
        HideOverlay();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        HideOverlay();
    }
}
