using Avalonia.Controls;
using Avalonia.Interactivity;
using CodyFormat.Desktop.Services;

namespace CodyFormat.Desktop.Views;

public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Version {AppInfo.Version}  •  {PlatformInfo.Name} {PlatformInfo.Architecture}  •  Developer: {AppInfo.Developer}";
    }

    private void CloseClick(object? sender, RoutedEventArgs e) => Close();
    private async void GitHubClick(object? sender, RoutedEventArgs e)
    {
        if (!BrowserLauncher.TryOpen(AppInfo.GitHubUrl, out var error))
            await DialogService.ShowAsync(this, "CodyFormat", $"Could not open the GitHub page.\n\n{error}");
    }
}
