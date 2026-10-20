using CodyFormat.Models;

namespace CodyFormat.Services;

/// <summary>
/// Applies indentation and file-level whitespace rules after language-aware formatting.
/// This keeps the formatter engine independent from project configuration discovery.
/// </summary>
public static class FormattingPolicyService
{
    public static string Apply(
        string text,
        EditorConfigSettings settings,
        int fallbackIndentSize,
        string fallbackNewLine = "\n",
        bool documentRules = true)
    {
        var indentSize = settings.EffectiveIndentSize(fallbackIndentSize);
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var lines = normalized.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            if (settings.TrimTrailingWhitespace == true)
                lines[i] = lines[i].TrimEnd(' ', '\t');

            if (settings.IndentStyle == "tab")
                lines[i] = LeadingSpacesToTabs(lines[i], indentSize);
            else if (settings.IndentStyle == "space")
                lines[i] = LeadingTabsToSpaces(lines[i], indentSize);
        }

        var result = string.Join("\n", lines);

        if (documentRules && settings.InsertFinalNewline is bool finalNewline)
        {
            result = result.TrimEnd('\n');
            if (finalNewline)
                result += "\n";
        }

        if (!documentRules)
            return result;

        var newline = settings.EndOfLine switch
        {
            "crlf" => "\r\n",
            "cr" => "\r",
            "lf" => "\n",
            _ => fallbackNewLine
        };

        return newline == "\n"
            ? result
            : result.Replace("\n", newline, StringComparison.Ordinal);
    }

    public static string DetectNewLine(string text)
        => text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n"
            : text.Contains('\r') ? "\r"
            : "\n";

    private static string LeadingSpacesToTabs(string line, int width)
    {
        var spaces = 0;
        while (spaces < line.Length && line[spaces] == ' ')
            spaces++;

        if (spaces < width)
            return line;

        var tabs = spaces / width;
        var remainder = spaces % width;
        return new string('\t', tabs) + new string(' ', remainder) + line[spaces..];
    }

    private static string LeadingTabsToSpaces(string line, int width)
    {
        var tabs = 0;
        while (tabs < line.Length && line[tabs] == '\t')
            tabs++;

        return tabs == 0 ? line : new string(' ', tabs * width) + line[tabs..];
    }
}
