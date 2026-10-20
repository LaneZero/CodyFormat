namespace CodyFormat.Models;

/// <summary>
/// Effective formatting policy resolved from one or more .editorconfig files.
/// Null properties mean that CodyFormat should keep its existing/default behavior.
/// </summary>
public sealed record EditorConfigSettings
{
    public string? IndentStyle { get; init; }
    public int? IndentSize { get; init; }
    public int? TabWidth { get; init; }
    public string? EndOfLine { get; init; }
    public bool? InsertFinalNewline { get; init; }
    public bool? TrimTrailingWhitespace { get; init; }
    public IReadOnlyList<string> SourceFiles { get; init; } = Array.Empty<string>();

    public bool HasRules =>
        IndentStyle is not null || IndentSize is not null || TabWidth is not null ||
        EndOfLine is not null || InsertFinalNewline is not null || TrimTrailingWhitespace is not null;

    public int EffectiveIndentSize(int fallback)
    {
        var value = IndentSize ?? TabWidth ?? fallback;
        return value is >= 1 and <= 16 ? value : fallback;
    }

    public static EditorConfigSettings Merge(EditorConfigSettings baseline, EditorConfigSettings overrides)
        => new()
        {
            IndentStyle = overrides.IndentStyle ?? baseline.IndentStyle,
            IndentSize = overrides.IndentSize ?? baseline.IndentSize,
            TabWidth = overrides.TabWidth ?? baseline.TabWidth,
            EndOfLine = overrides.EndOfLine ?? baseline.EndOfLine,
            InsertFinalNewline = overrides.InsertFinalNewline ?? baseline.InsertFinalNewline,
            TrimTrailingWhitespace = overrides.TrimTrailingWhitespace ?? baseline.TrimTrailingWhitespace,
            SourceFiles = overrides.SourceFiles.Count > 0 ? overrides.SourceFiles : baseline.SourceFiles
        };
}
