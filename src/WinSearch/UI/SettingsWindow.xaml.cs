using System.Windows;
using WinSearch.UI.ViewModels;

namespace WinSearch.UI;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm;

    public SettingsWindow()
    {
        InitializeComponent();
        _vm = new SettingsViewModel();
        DataContext = _vm;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _vm.SaveCommand.Execute(null);
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
