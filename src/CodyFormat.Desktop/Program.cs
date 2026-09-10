using Avalonia;

namespace CodyFormat.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // Keep the X11 WM_CLASS aligned with packaging/linux/codyformat.desktop so
            // GNOME/KDE can associate the running window with the installed icon.
            .With(new X11PlatformOptions { WmClass = "CodyFormat" })
            .LogToTrace();
}
