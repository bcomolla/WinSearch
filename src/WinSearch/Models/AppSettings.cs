namespace WinSearch.Models;

public class AppSettings
{
    public string Hotkey { get; set; } = "Win+Space";
    public int HotkeyModifiers { get; set; } = 0x0008; // MOD_WIN
    public int HotkeyVk { get; set; } = 0x20;          // VK_SPACE
    public int ResultLimit { get; set; } = 8;
    public bool EnableControlPanel { get; set; } = true;
    public bool EnableWindowsSettings { get; set; } = true;
    public bool EnableInstalledApps { get; set; } = true;
    public bool EnableFiles { get; set; } = true;
    public bool EnableProcesses { get; set; } = true;
    public bool EnableRegistry { get; set; } = false;
    public bool LaunchOnStartup { get; set; } = false;
    public List<string> FileExcludePaths { get; set; } = new();
    public string AccentColor { get; set; } = "#0078D4";
    public int FontSize { get; set; } = 14;
}
