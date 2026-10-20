using CodyFormat.Models;

namespace CodyFormat.Services;

/// <summary>
/// Resolves the EditorConfig properties supported by CodyFormat. Resolution is shared by CLI and GUI.
/// The implementation is deliberately local-only and never searches outside the file-system ancestry.
/// </summary>
public static class EditorConfigService
{
    private const long MaxEditorConfigBytes = 256 * 1024;

    public static EditorConfigSettings Resolve(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return new EditorConfigSettings();

        var fullPath = Path.GetFullPath(filePath);
        var startDirectory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(startDirectory))
            return new EditorConfigSettings();

        var chain = new List<(string Path, ParsedEditorConfig Config)>();
        var current = new DirectoryInfo(startDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, ".editorconfig");
            if (File.Exists(candidate))
            {
                var parsed = Parse(candidate);
                if (parsed is not null)
                {
                    chain.Add((candidate, parsed));
                    if (parsed.IsRoot)
                        break;
                }
            }

            current = current.Parent;
        }

        chain.Reverse();
        var effective = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sources = new List<string>();

        foreach (var item in chain)
        {
            sources.Add(item.Path);
            var configDirectory = Path.GetDirectoryName(item.Path)!;
            var relative = Normalize(Path.GetRelativePath(configDirectory, fullPath));

            foreach (var section in item.Config.Sections)
            {
                if (!Matches(relative, section.Pattern))
                    continue;

                foreach (var property in section.Properties)
                {
                    if (property.Value.Equals("unset", StringComparison.OrdinalIgnoreCase))
                        effective.Remove(property.Key);
                    else
                        effective[property.Key] = property.Value;
                }
            }
        }

        return BuildSettings(effective, sources);
    }

    private static ParsedEditorConfig? Parse(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (info.Length > MaxEditorConfigBytes)
                return null;

            var config = new ParsedEditorConfig();
            Section? current = null;

            foreach (var raw in File.ReadLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(';'))
                    continue;

                if (line.StartsWith('[') && line.EndsWith(']') && line.Length > 2)
                {
                    current = new Section(line[1..^1].Trim());
                    config.Sections.Add(current);
                    continue;
                }

                var equals = line.IndexOf('=');
                if (equals <= 0)
                    continue;

                var key = line[..equals].Trim();
                var value = line[(equals + 1)..].Trim();
                if (key.Length == 0)
                    continue;

                if (current is null)
                {
                    if (key.Equals("root", StringComparison.OrdinalIgnoreCase) && TryBool(value, out var root))
                        config.IsRoot = root;
                    continue;
                }

                current.Properties[key] = value;
            }

            return config;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static EditorConfigSettings BuildSettings(
        IReadOnlyDictionary<string, string> values,
        IReadOnlyList<string> sources)
    {
        string? indentStyle = null;
        if (values.TryGetValue("indent_style", out var style) &&
            (style.Equals("space", StringComparison.OrdinalIgnoreCase) || style.Equals("tab", StringComparison.OrdinalIgnoreCase)))
            indentStyle = style.ToLowerInvariant();

        int? indentSize = null;
        if (values.TryGetValue("indent_size", out var indentText) &&
            int.TryParse(indentText, out var indent) && indent is >= 1 and <= 16)
            indentSize = indent;

        int? tabWidth = null;
        if (values.TryGetValue("tab_width", out var tabText) &&
            int.TryParse(tabText, out var tab) && tab is >= 1 and <= 16)
            tabWidth = tab;

        string? endOfLine = null;
        if (values.TryGetValue("end_of_line", out var eol) &&
            (eol.Equals("lf", StringComparison.OrdinalIgnoreCase) ||
             eol.Equals("crlf", StringComparison.OrdinalIgnoreCase) ||
             eol.Equals("cr", StringComparison.OrdinalIgnoreCase)))
            endOfLine = eol.ToLowerInvariant();

        bool? insertFinal = null;
        if (values.TryGetValue("insert_final_newline", out var finalText) && TryBool(finalText, out var final))
            insertFinal = final;

        bool? trimTrailing = null;
        if (values.TryGetValue("trim_trailing_whitespace", out var trimText) && TryBool(trimText, out var trim))
            trimTrailing = trim;

        return new EditorConfigSettings
        {
            IndentStyle = indentStyle,
            IndentSize = indentSize,
            TabWidth = tabWidth,
            EndOfLine = endOfLine,
            InsertFinalNewline = insertFinal,
            TrimTrailingWhitespace = trimTrailing,
            SourceFiles = sources.ToArray()
        };
    }

    private static bool Matches(string relativePath, string pattern)
    {
        foreach (var expanded in ExpandBraces(pattern))
            if (GlobMatcher.IsMatch(relativePath, expanded.TrimStart('/')))
                return true;
        return false;
    }

    private static IEnumerable<string> ExpandBraces(string pattern)
    {
        var open = pattern.IndexOf('{');
        if (open < 0)
        {
            yield return pattern;
            yield break;
        }

        var close = pattern.IndexOf('}', open + 1);
        if (close < 0)
        {
            yield return pattern;
            yield break;
        }

        var body = pattern[(open + 1)..close];
        var parts = body.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            yield return pattern;
            yield break;
        }

        foreach (var part in parts)
        {
            var candidate = pattern[..open] + part + pattern[(close + 1)..];
            foreach (var nested in ExpandBraces(candidate))
                yield return nested;
        }
    }

    private static bool TryBool(string value, out bool result)
    {
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }
        if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }
        result = false;
        return false;
    }

    private static string Normalize(string path) => path.Replace('\\', '/');

    private sealed class ParsedEditorConfig
    {
        public bool IsRoot { get; set; }
        public List<Section> Sections { get; } = [];
    }

    private sealed class Section(string pattern)
    {
        public string Pattern { get; } = pattern;
        public Dictionary<string, string> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
