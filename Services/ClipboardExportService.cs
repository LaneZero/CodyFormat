using System.Net;
using System.Text;
using System.Windows;
using CodyFormat.Models;

namespace CodyFormat.Services;

public static class ClipboardExportService
{
    public static void CopyPlain(string code) => Clipboard.SetText(code ?? string.Empty, TextDataFormat.UnicodeText);

    public static void CopyRich(
        string code,
        string language,
        ThemeDefinition theme,
        string background,
        bool lineNumbers,
        bool border,
        int indentSize,
        SyntaxHighlighterService highlighter)
    {
        var fragment = BuildHtmlFragment(code, language, theme, background, lineNumbers, border, indentSize, highlighter);
        var html = BuildCfHtml(fragment);

        var data = new DataObject();
        data.SetData(DataFormats.UnicodeText, code ?? string.Empty);
        data.SetData(DataFormats.Html, html);
        Clipboard.SetDataObject(data, true);
    }

    public static string BuildHtmlFragment(
        string code,
        string language,
        ThemeDefinition theme,
        string background,
        bool lineNumbers,
        bool border,
        int indentSize,
        SyntaxHighlighterService highlighter)
    {
        var backgroundStyle = background == "None" ? string.Empty : $"background:{background};";
        var borderStyle = border ? $"border:1px solid {theme.Border};" : string.Empty;
        var sb = new StringBuilder();

        sb.Append($"<div style=\"{backgroundStyle}{borderStyle}border-radius:8px;padding:14px 16px;font-family:'Cascadia Mono',Consolas,'Courier New',monospace;font-size:10.5pt;line-height:1.5;color:{theme.Foreground};white-space:pre;\">");

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

    private static string BuildCfHtml(string fragment)
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

    internal static string ColorFor(TokenKind kind, ThemeDefinition theme, string? language = null)
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

    internal static string[] NormalizeLines(string? code)
        => (code ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
}
