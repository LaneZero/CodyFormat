using System.Windows;
using CodyFormat.Services;

namespace CodyFormat;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        UiThemeService.Apply("Dark");
        base.OnStartup(e);
    }
}
