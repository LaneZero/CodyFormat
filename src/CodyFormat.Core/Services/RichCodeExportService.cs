using System.Globalization;
using System.Net;
using System.Text;
using CodyFormat.Models;

namespace CodyFormat.Services;

/// <summary>
/// Builds rich, platform-neutral representations of highlighted source code.
/// HTML is used where a target understands HTML clipboard data, while RTF is
/// provided as a second rich representation for word processors and clipboard bridges.
/// </summary>
public static class RichCodeExportService
{
    public static string BuildHtmlFragment(
        string code,
        string language,
        ThemeDefinition theme,
        string background,
        bool lineNumbers,
        bool border,
        int indentSize,
        string fontFamily,
        double fontSize,
        SyntaxHighlighterService highlighter)
    {
        var backgroundStyle = background == "None" ? string.Empty : $"background:{background};";
        var borderStyle = border ? $"border:1px solid {theme.Border};" : string.Empty;
        var safeFont = fontFamily.Replace("'", "\\'", StringComparison.Ordinal);
        var size = Math.Clamp(fontSize, 8, 32).ToString("0.#", CultureInfo.InvariantCulture);
        var sb = new StringBuilder();

        sb.Append($"<div style=\"{backgroundStyle}{borderStyle}border-radius:8px;padding:14px 16px;font-family:'{safeFont}',Consolas,'Courier New',monospace;font-size:{size}pt;line-height:1.45;color:{theme.Foreground};white-space:pre;\">");

        var lines = NormalizeLines(code);
        var digits = Math.Max(2, lines.Length.ToString().Length);
        var tabReplacement = new string('\u00A0', Math.Clamp(indentSize, 2, 8));

        for (var i = 0; i < lines.Length; i++)
        {
            if (lineNumbers)
            {
                var number = (i + 1).ToString().PadLeft(digits).Replace(" ", "\u00A0");
                sb.Append($"<span style=\"color:{theme.LineNumber};user-select:none;\">{number}\u00A0\u00A0</span>");
            }

            foreach (var token in highlighter.HighlightLine(lines[i], language))
            {
                var color = ColorFor(token.Kind, theme, language);
                var encoded = WebUtility.HtmlEncode(token.Text)
                    .Replace(" ", "\u00A0")
                    .Replace("\t", tabReplacement);
                sb.Append($"<span style=\"color:{color};\">{encoded}</span>");
            }

            if (i < lines.Length - 1) sb.Append("<br>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    /// <summary>
    /// Wraps a formatted fragment in a complete UTF-8 HTML document. Linux office suites
    /// are generally more reliable when the text/html clipboard target contains explicit
    /// document and charset metadata instead of a bare fragment.
    /// </summary>
    public static string BuildHtmlDocument(string fragment)
        => "<!DOCTYPE html><html><head><meta charset=\"utf-8\">" +
           "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>" +
           "<body><!--StartFragment-->" + (fragment ?? string.Empty) + "<!--EndFragment--></body></html>";

    /// <summary>
    /// Builds the Windows CF_HTML payload. Offsets are UTF-8 byte offsets, as
    /// required by the Windows clipboard HTML format specification.
    /// </summary>
    public static string BuildCfHtml(string fragment)
    {
        const string startMarker = "<!--StartFragment-->";
        const string endMarker = "<!--EndFragment-->";
        const string headerTemplate = "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\n";
        var body = $"<html><body>{startMarker}{fragment}{endMarker}</body></html>";
        var emptyHeader = string.Format(headerTemplate, 0, 0, 0, 0);
        var startHtml = Encoding.UTF8.GetByteCount(emptyHeader);
        var startFragment = startHtml + Encoding.UTF8.GetByteCount($"<html><body>{startMarker}");
        var endFragment = startFragment + Encoding.UTF8.GetByteCount(fragment);
        var endHtml = startHtml + Encoding.UTF8.GetByteCount(body);
        return string.Format(headerTemplate, startHtml, endHtml, startFragment, endFragment) + body;
    }

    /// <summary>
    /// Builds an RTF representation of the same highlighted code. RTF is deliberately
    /// emitted alongside HTML because Microsoft Word and remote/virtual clipboard
    /// bridges often preserve RTF even when a platform-specific HTML MIME type is lost.
    /// </summary>
    public static string BuildRtf(
        string code,
        string language,
        ThemeDefinition theme,
        string background,
        bool lineNumbers,
        bool border,
        int indentSize,
        string fontFamily,
        double fontSize,
        SyntaxHighlighterService highlighter)
    {
        var colors = new List<string>();
        var colorIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        int RegisterColor(string value)
        {
            var normalized = NormalizeHexColor(value);
            if (colorIndexes.TryGetValue(normalized, out var existing)) return existing;
            colors.Add(normalized);
            var index = colors.Count; // RTF color table index 0 is the automatic color.
            colorIndexes[normalized] = index;
            return index;
        }

        // Register every color that a token can use before writing the color table.
        foreach (var value in new[]
        {
            theme.Foreground, theme.Keyword, theme.ControlKeyword, theme.String,
            theme.Comment, theme.Number, theme.Type, theme.Function, theme.Property,
            theme.Variable, theme.Tag, theme.Attribute, theme.Operator, theme.Constant,
            theme.LineNumber, theme.Border
        })
        {
            RegisterColor(value);
        }

        var backgroundIndex = background == "None" ? 0 : RegisterColor(background);
        var borderIndex = RegisterColor(theme.Border);
        var foregroundIndex = RegisterColor(theme.Foreground);
        var lineNumberIndex = RegisterColor(theme.LineNumber);

        var sb = new StringBuilder();
        sb.Append(@"{\rtf1\ansi\deff0\uc1");
        sb.Append(@"{\fonttbl{\f0\fmodern ");
        sb.Append(EscapeRtfAscii(fontFamily));
        sb.Append(";}}");
        sb.Append(@"{\colortbl ;");

        foreach (var color in colors)
        {
            var (r, g, b) = ParseRgb(color);
            sb.Append($"\\red{r}\\green{g}\\blue{b};");
        }

        sb.Append('}');
        sb.Append(@"\viewkind4\pard\sa0\sb0\f0");
        sb.Append($"\\fs{(int)Math.Round(Math.Clamp(fontSize, 8, 32) * 2)}");
        sb.Append($"\\cf{foregroundIndex}");

        if (backgroundIndex > 0)
            sb.Append($"\\highlight{backgroundIndex}");

        if (border)
            sb.Append($"\\box\\brdrs\\brdrw10\\brdrcf{borderIndex}\\brsp40");

        sb.Append(' ');

        var lines = NormalizeLines(code);
        var digits = Math.Max(2, lines.Length.ToString().Length);
        var tabReplacement = new string(' ', Math.Clamp(indentSize, 2, 8));

        for (var i = 0; i < lines.Length; i++)
        {
            if (lineNumbers)
            {
                sb.Append($"\\cf{lineNumberIndex} ");
                AppendRtfText(sb, (i + 1).ToString().PadLeft(digits) + "  ");
            }

            foreach (var token in highlighter.HighlightLine(lines[i], language))
            {
                var tokenColor = RegisterColor(ColorFor(token.Kind, theme, language));
                sb.Append($"\\cf{tokenColor} ");
                AppendRtfText(sb, token.Text.Replace("\t", tabReplacement, StringComparison.Ordinal));
            }

            if (i < lines.Length - 1)
                sb.Append("\\line\n");
        }

        sb.Append('}');
        return sb.ToString();
    }

    public static string ColorFor(TokenKind kind, ThemeDefinition theme, string? language = null)
    {
        // SQL keeps clauses and control words in one coherent keyword family.
        if (language?.Equals("SQL", StringComparison.OrdinalIgnoreCase) == true &&
            kind is TokenKind.Keyword or TokenKind.ControlKeyword)
            return theme.Keyword;

        return kind switch
        {
            TokenKind.Keyword => theme.Keyword,
            TokenKind.ControlKeyword => theme.ControlKeyword,
            TokenKind.String => theme.String,
            TokenKind.Comment => theme.Comment,
            TokenKind.Number => theme.Number,
            TokenKind.Type => theme.Type,
            TokenKind.Function => theme.Function,
            TokenKind.Property => theme.Property,
            TokenKind.Variable => theme.Variable,
            TokenKind.Tag => theme.Tag,
            TokenKind.Attribute => theme.Attribute,
            TokenKind.Operator => theme.Operator,
            TokenKind.Constant => theme.Constant,
            _ => theme.Foreground
        };
    }

    public static string[] NormalizeLines(string? code)
        => (code ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    private static void AppendRtfText(StringBuilder sb, string text)
    {
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '\\': sb.Append(@"\\"); break;
                case '{': sb.Append(@"\{"); break;
                case '}': sb.Append(@"\}"); break;
                case '\t': sb.Append(@"\tab "); break;
                default:
                    if (ch <= 0x7F)
                    {
                        sb.Append(ch);
                    }
                    else
                    {
                        var signed = ch > 32767 ? ch - 65536 : ch;
                        sb.Append($"\\u{signed}?");
                    }
                    break;
            }
        }
    }

    private static string EscapeRtfAscii(string? value)
        => (value ?? "Cascadia Mono")
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("{", "\\{", StringComparison.Ordinal)
            .Replace("}", "\\}", StringComparison.Ordinal)
            .Replace(";", string.Empty, StringComparison.Ordinal);

    private static string NormalizeHexColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "#000000";
        var color = value.Trim();
        if (!color.StartsWith('#')) return "#000000";
        if (color.Length == 7) return color.ToUpperInvariant();
        if (color.Length == 4)
        {
            return $"#{color[1]}{color[1]}{color[2]}{color[2]}{color[3]}{color[3]}".ToUpperInvariant();
        }
        return "#000000";
    }

    private static (int R, int G, int B) ParseRgb(string color)
    {
        var normalized = NormalizeHexColor(color);
        return (
            int.Parse(normalized.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            int.Parse(normalized.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            int.Parse(normalized.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }
}
