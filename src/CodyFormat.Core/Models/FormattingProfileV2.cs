namespace CodyFormat.Models;

/// <summary>
/// Versioned, portable CodyFormat profile schema. Profiles contain presentation/formatting preferences only;
/// executable commands and source-code contents are intentionally not part of the schema.
/// </summary>
public sealed class FormattingProfileV2
{
    public int SchemaVersion { get; set; } = 2;
    public string Name { get; set; } = "Custom";
    public string Description { get; set; } = string.Empty;
    public ProfileFormattingOptions Formatting { get; set; } = new();
    public ProfileAppearanceOptions Appearance { get; set; } = new();
    public ProfileOutputOptions Output { get; set; } = new();
}

public sealed class ProfileFormattingOptions
{
    public string IndentStyle { get; set; } = "space";
    public int IndentSize { get; set; } = 4;
}

public sealed class ProfileAppearanceOptions
{
    public string Theme { get; set; } = "Dark";
    public string Background { get; set; } = "Theme Default";
    public bool NoBackground { get; set; }
    public bool LineNumbers { get; set; } = true;
    public bool Border { get; set; } = true;
    public string FontFamily { get; set; } = "Cascadia Mono";
    public double FontSize { get; set; } = 14;
}

public sealed class ProfileOutputOptions
{
    public bool FormatBeforeOutput { get; set; }
    public string Target { get; set; } = "General";
}
