using System.Diagnostics;
using System.Windows;

namespace CodyFormat;

public partial class HelpWindow : Window
{
    public HelpWindow() => InitializeComponent();

    private void GitHubClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(AppInfo.GitHubUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open the GitHub page.\n\n{AppInfo.GitHubUrl}\n\n{ex.Message}", "CodyFormat", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CloseClick(object sender, RoutedEventArgs e) => Close();
}
