namespace CodyFormat.Models;

/// <summary>
/// Represents a reusable visual and formatting preset for code output.
/// </summary>
public sealed record FormattingProfile(
    string Name,
    string Theme,
    string Background,
    bool NoBackground,
    int IndentSize,
    bool LineNumbers,
    bool Border,
    string FontFamily,
    double FontSize,
    bool FormatBeforeOutput,
    string Description);
