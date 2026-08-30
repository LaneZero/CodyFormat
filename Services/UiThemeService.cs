using System.Windows;
using System.Windows.Media;

namespace CodyFormat.Services;

public static class UiThemeService
{
    public static void Apply(string themeName)
    {
        var dark = !themeName.Equals("Light", StringComparison.OrdinalIgnoreCase);
        Set("WindowBgBrush", dark ? "#0B1020" : "#F6F8FA");
        Set("PanelBrush", dark ? "#111827" : "#FFFFFF");
        Set("PanelAltBrush", dark ? "#172033" : "#F3F4F6");
        Set("EditorBrush", dark ? "#0D1424" : "#FFFFFF");
        Set("PrimaryTextBrush", dark ? "#F8FAFC" : "#111827");
        Set("SecondaryTextBrush", dark ? "#94A3B8" : "#57606A");
        Set("MutedTextBrush", dark ? "#64748B" : "#6E7781");
        Set("ControlBrush", dark ? "#1F2937" : "#EEF2F6");
        Set("ControlHoverBrush", dark ? "#2B384D" : "#E2E8F0");
        Set("StrokeBrush", dark ? "#263244" : "#D8DEE4");
        Set("AccentBrush", dark ? "#2563EB" : "#0969DA");
        Set("AccentHoverBrush", dark ? "#3B82F6" : "#0550AE");
        Set("SuccessBrush", dark ? "#0F766E" : "#0A7F66");
        Set("SuccessHoverBrush", dark ? "#0D9488" : "#08705A");
        Set("InputForegroundBrush", dark ? "#E5E7EB" : "#24292F");
        Set("CaretBrush", dark ? "#FFFFFF" : "#111827");
        Set("SelectionBrush", dark ? "#264F78" : "#ADD6FF");
        Set("BadgeBrush", dark ? "#172554" : "#DDF4FF");
        Set("BadgeTextBrush", dark ? "#93C5FD" : "#0969DA");
        Set("OfflineBadgeBrush", dark ? "#052E2B" : "#DAFBE1");
        Set("OfflineBadgeTextBrush", dark ? "#5EEAD4" : "#1A7F37");
        Set("DangerBrush", dark ? "#7F1D1D" : "#FFEBE9");
        Set("DangerTextBrush", dark ? "#FCA5A5" : "#CF222E");
    }

    private static void Set(string key, string color)
    {
        if (Application.Current is null) return;
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        brush.Freeze();
        Application.Current.Resources[key] = brush;
    }
}
