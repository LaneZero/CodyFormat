using CodyFormat.Models;

namespace CodyFormat.Services;

/// <summary>
/// Provides built-in output profiles without requiring configuration files or internet access.
/// </summary>
public static class FormattingProfileService
{
    public static readonly FormattingProfile[] Profiles =
    [
        new("Custom", "Dark", "Theme Default", false, 4, true, true, "Cascadia Mono", 14, false,
            "Manual settings."),
        new("Word Document", "Light", "White", true, 4, false, false, "Consolas", 11, true,
            "Clean output for Microsoft Word with no forced background."),
        new("Documentation", "Dark", "GitHub Dark", false, 4, true, true, "Cascadia Mono", 12, true,
            "Balanced code blocks for technical documentation."),
        new("PowerPoint", "Dark", "Midnight", false, 4, true, true, "Cascadia Code", 16, true,
            "Larger code for slides and presentations."),
        new("Minimal", "Light", "White", true, 4, false, false, "Consolas", 11, false,
            "Syntax colors only, without background or border."),
        new("Presentation", "Dark", "VS Code Dark", false, 4, true, true, "Cascadia Code", 18, true,
            "High-visibility code blocks for projectors and large screens.")
    ];

    public static FormattingProfile? Get(string? name) =>
        Profiles.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
