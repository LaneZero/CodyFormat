namespace CodyFormat.Services;

/// <summary>
/// Builds Telegram-friendly fenced code blocks and copies them as plain Unicode text.
/// Telegram controls the final syntax-highlight theme; CodyFormat only supplies the
/// language identifier and source code fence locally.
/// </summary>
public static class TelegramExportService
{
    private static readonly IReadOnlyDictionary<string, string> LanguageTags =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["C#"] = "csharp",
            ["SQL"] = "sql",
            ["JavaScript"] = "javascript",
            ["TypeScript"] = "typescript",
            ["Python"] = "python",
            ["JSON"] = "json",
            ["XML"] = "xml",
            ["HTML"] = "html",
            ["CSS"] = "css",
            ["PowerShell"] = "powershell",
            ["Bash"] = "bash",
            ["Rust"] = "rust",
            ["Go"] = "go",
            ["PHP"] = "php",
            ["Java"] = "java",
            ["C"] = "c",
            ["C++"] = "cpp",
            ["Kotlin"] = "kotlin",
            ["Swift"] = "swift",
            ["Dart"] = "dart",
            ["Ruby"] = "ruby",
            ["Lua"] = "lua",
            ["R"] = "r",
            ["Perl"] = "perl",
            ["YAML"] = "yaml",
            ["Markdown"] = "markdown",
            ["Objective-C"] = "objectivec",
            ["Scala"] = "scala",
            ["Groovy"] = "groovy",
            ["Plain Text"] = "text"
        };

    /// <summary>
    /// Returns the exact fenced representation copied to the clipboard.
    /// The source text is preserved apart from normalizing line endings to LF.
    /// </summary>
    public static string BuildBlock(string? code, string language)
    {
        var tag = GetLanguageTag(language);
        var normalized = (code ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

        // The closing fence must start on its own line. Existing trailing newlines
        // are preserved; one is added only when the source does not already end in LF.
        if (!normalized.EndsWith('\n'))
            normalized += "\n";

        return $"```{tag}\n{normalized}```";
    }

    /// <summary>
    /// Maps CodyFormat display names to common fenced-code language identifiers.
    /// </summary>
    public static string GetLanguageTag(string language)
    {
        if (LanguageTags.TryGetValue(language, out var tag))
            return tag;

        var fallback = new string((language ?? "text")
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());

        return string.IsNullOrWhiteSpace(fallback) ? "text" : fallback;
    }

    /// <summary>
    /// Telegram fences cannot safely contain an unescaped triple-backtick sequence.
    /// CodyFormat never mutates the source to hide that condition; callers can warn.
    /// </summary>
    public static bool ContainsFence(string? code)
        => (code ?? string.Empty).Contains("```", StringComparison.Ordinal);
}
