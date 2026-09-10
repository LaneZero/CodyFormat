namespace CodyFormat.Models;

/// <summary>
/// Stores local user preferences. No source code is persisted in this object.
/// </summary>
public sealed class AppSettings
{
    public string Language { get; set; } = "Auto Detect";
    public string Theme { get; set; } = "Dark";
    public string Background { get; set; } = "Theme Default";
    public bool NoBackground { get; set; }
    public int IndentSize { get; set; } = 4;
    public bool LineNumbers { get; set; } = true;
    public bool Border { get; set; } = true;
    public bool FormatBeforeOutput { get; set; }
    public bool PreviewBeforeApply { get; set; } = true;
    public string FontFamily { get; set; } = "Cascadia Mono";
    public double FontSize { get; set; } = 14;
    public string Profile { get; set; } = "Custom";
}
