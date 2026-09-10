using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace CodyFormat.Desktop.Services;

public static class DesktopThemeService
{
    public static void Apply(string theme)
    {
        var dark = !theme.Equals("Light", StringComparison.OrdinalIgnoreCase);
        if (Application.Current is not { } app) return;

        app.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        Set(app, "WindowBgBrush", dark ? "#0B1020" : "#F6F8FA");
        Set(app, "PanelBrush", dark ? "#111827" : "#FFFFFF");
        Set(app, "PanelAltBrush", dark ? "#172033" : "#F3F4F6");
        Set(app, "EditorBrush", dark ? "#0D1424" : "#FFFFFF");
        Set(app, "PrimaryTextBrush", dark ? "#F8FAFC" : "#111827");
        Set(app, "SecondaryTextBrush", dark ? "#94A3B8" : "#57606A");
        Set(app, "MutedTextBrush", dark ? "#64748B" : "#6E7781");
        Set(app, "ControlBrush", dark ? "#1F2937" : "#EEF2F6");
        Set(app, "ControlHoverBrush", dark ? "#2B384D" : "#E2E8F0");
        Set(app, "StrokeBrush", dark ? "#263244" : "#D8DEE4");
        Set(app, "AccentBrush", dark ? "#2563EB" : "#0969DA");
        Set(app, "AccentHoverBrush", dark ? "#3B82F6" : "#0550AE");
        Set(app, "SuccessBrush", dark ? "#0F766E" : "#0A7F66");
        Set(app, "InputForegroundBrush", dark ? "#E5E7EB" : "#24292F");
        Set(app, "SelectionBrush", dark ? "#264F78" : "#ADD6FF");
        Set(app, "BadgeBrush", dark ? "#172554" : "#DDF4FF");
        Set(app, "BadgeTextBrush", dark ? "#93C5FD" : "#0969DA");
        Set(app, "OfflineBadgeBrush", dark ? "#052E2B" : "#DAFBE1");
        Set(app, "OfflineBadgeTextBrush", dark ? "#5EEAD4" : "#1A7F37");
    }

    private static void Set(Application app, string key, string color) =>
        app.Resources[key] = new SolidColorBrush(Color.Parse(color));
}
