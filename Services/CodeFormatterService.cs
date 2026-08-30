using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CodyFormat.Models;

namespace CodyFormat.Services;

public static class CodeFormatterService
{
    private static readonly HashSet<string> SqlKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "DISTINCT", "TOP", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER", "CROSS", "ON",
        "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC", "UNION", "ALL", "EXCEPT", "INTERSECT", "INSERT", "INTO", "VALUES",
        "UPDATE", "SET", "DELETE", "CREATE", "ALTER", "DROP", "TABLE", "VIEW", "PROCEDURE", "FUNCTION", "INDEX", "DATABASE",
        "SCHEMA", "AS", "AND", "OR", "NOT", "NULL", "IS", "IN", "EXISTS", "BETWEEN", "LIKE", "CASE", "WHEN", "THEN", "ELSE",
        "END", "WITH", "RECURSIVE", "OVER", "PARTITION", "ROWS", "RANGE", "LIMIT", "OFFSET", "FETCH", "NEXT", "ONLY", "RETURNING",
        "OUTPUT", "MERGE", "USING", "MATCHED", "BEGIN", "COMMIT", "ROLLBACK", "TRANSACTION", "TRY", "CATCH", "IF", "ELSE"
    };

    public static FormatResult Format(string? code, string language, int indentSize = 4)
    {
        var original = NormalizeNewLines(code ?? string.Empty);
        if (string.IsNullOrWhiteSpace(original))
            return new FormatResult(original, false);

        indentSize = Math.Clamp(indentSize, 2, 8);

        try
        {
            var formatted = language switch
            {
                "JSON" => FormatJson(original, indentSize),
                "XML" => FormatXml(original, indentSize),
                "HTML" => FormatHtml(original, indentSize),
                "SQL" => FormatSql(original, indentSize),
                "Python" => FormatPython(original, indentSize),
                "Bash" => FormatKeywordBlocks(original, indentSize, ShellOpen, ShellClose, ShellMiddle),
                "Ruby" => FormatKeywordBlocks(original, indentSize, RubyOpen, RubyClose, RubyMiddle),
                "Lua" => FormatKeywordBlocks(original, indentSize, LuaOpen, LuaClose, LuaMiddle),
                "YAML" => FormatYaml(original, indentSize),
                "Markdown" => FormatWhitespaceOnly(original),
                _ when LanguageCatalog.IsBraceLanguage(language) => FormatBraceLanguage(original, indentSize, language),
                _ => FormatWhitespaceOnly(original)
            };

            formatted = TrimOuterBlankLines(formatted);
            return new FormatResult(formatted, !string.Equals(original, formatted, StringComparison.Ordinal));
        }
        catch (Exception ex)
        {
            // Never replace valid user source with a partially formatted result.
            return new FormatResult(original, false, $"Formatter kept the original code: {ex.Message}");
        }
    }

    private static string FormatJson(string code, int indentSize)
    {
        using var document = JsonDocument.Parse(code, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        var json = JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
        return indentSize == 2 ? json : ConvertIndent(json, 2, indentSize);
    }

    private static string FormatXml(string code, int indentSize)
    {
        var document = XDocument.Parse(code, LoadOptions.None);
        var normalized = document.ToString(SaveOptions.None);
        return indentSize == 2 ? normalized : ConvertIndent(normalized, 2, indentSize);
    }

    private static string FormatHtml(string code, int indentSize)
    {
        var tokens = Regex.Split(code, "(?=<)|(?<=>)")
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();

        var sb = new StringBuilder(code.Length + 64);
        var indent = 0;
        var rawTag = string.Empty;

        foreach (var token in tokens)
        {
            if (rawTag.Length > 0)
            {
                AppendLine(sb, indent, token, indentSize);
                if (token.StartsWith($"</{rawTag}", StringComparison.OrdinalIgnoreCase))
                    rawTag = string.Empty;
                continue;
            }

            var isClosing = Regex.IsMatch(token, "^</");
            var isComment = token.StartsWith("<!--", StringComparison.Ordinal);
            var isDirective = token.StartsWith("<!", StringComparison.Ordinal) || token.StartsWith("<?", StringComparison.Ordinal);
            var isSelfClosing = token.EndsWith("/>", StringComparison.Ordinal) || IsHtmlVoidElement(token);

            if (isClosing)
                indent = Math.Max(0, indent - 1);

            if (token.StartsWith("<", StringComparison.Ordinal))
                AppendLine(sb, indent, token, indentSize);
            else
                AppendWrappedText(sb, indent, token, indentSize);

            if (!isClosing && !isSelfClosing && !isComment && !isDirective && token.StartsWith("<", StringComparison.Ordinal))
            {
                var tag = GetTagName(token);
                if (tag is "script" or "style" or "pre" or "textarea") rawTag = tag;
                indent++;
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatSql(string code, int indentSize)
    {
        var tokens = TokenizeSql(code);
        var sb = new StringBuilder(code.Length + 96);
        var indent = 0;
        var parenDepth = 0;
        var caseDepth = 0;
        var selectList = false;
        var selectListDepth = 0;
        var subqueryParens = new Stack<bool>();
        SqlToken? previous = null;

        void NewLine(int extra = 0)
        {
            TrimTrailingSpaces(sb);
            if (sb.Length > 0 && sb[^1] != '\n') sb.Append('\n');
            sb.Append(' ', Math.Max(0, indent + extra) * indentSize);
        }

        void Append(string text, bool forceSpace = true)
        {
            if (forceSpace && sb.Length > 0 && sb[^1] is not (' ' or '\n' or '(' or '.')) sb.Append(' ');
            sb.Append(text);
        }

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var text = token.Text;
            var upper = token.Kind == SqlTokenKind.Word ? text.ToUpperInvariant() : text;

            if (token.Kind == SqlTokenKind.LineComment || token.Kind == SqlTokenKind.BlockComment)
            {
                if (sb.Length > 0 && sb[^1] != '\n') sb.Append(' ');
                sb.Append(text.TrimEnd());
                NewLine();
                previous = token;
                continue;
            }

            if (token.Kind == SqlTokenKind.Word && SqlKeywords.Contains(upper)) text = upper;

            if (token.Kind == SqlTokenKind.Word && upper == "GROUP" && PeekWord(tokens, i + 1, "BY"))
            {
                NewLine();
                Append("GROUP BY", false);
                i = SkipWhitespaceTokens(tokens, i + 1);
                selectList = false;
                previous = token;
                continue;
            }

            if (token.Kind == SqlTokenKind.Word && upper == "ORDER" && PeekWord(tokens, i + 1, "BY"))
            {
                NewLine();
                Append("ORDER BY", false);
                i = SkipWhitespaceTokens(tokens, i + 1);
                selectList = false;
                previous = token;
                continue;
            }

            if (token.Kind == SqlTokenKind.Word && (upper is "LEFT" or "RIGHT" or "FULL" or "INNER" or "CROSS") && PeekWord(tokens, i + 1, "JOIN"))
            {
                NewLine();
                Append($"{upper} JOIN", false);
                i = SkipWhitespaceTokens(tokens, i + 1);
                selectList = false;
                previous = token;
                continue;
            }

            if (token.Kind == SqlTokenKind.Word)
            {
                if (upper == "CASE")
                {
                    Append(text);
                    caseDepth++;
                    indent++;
                    previous = token;
                    continue;
                }

                if (upper == "END" && caseDepth > 0)
                {
                    indent = Math.Max(0, indent - 1);
                    caseDepth--;
                    NewLine();
                    Append(text, false);
                    previous = token;
                    continue;
                }

                if (upper == "WHEN")
                {
                    NewLine();
                    Append(text, false);
                    previous = token;
                    continue;
                }

                if (upper is "THEN" or "ELSE")
                {
                    Append(text);
                    previous = token;
                    continue;
                }

                if (IsMajorSqlClause(upper))
                {
                    NewLine();
                    Append(text, false);
                    selectList = upper == "SELECT";
                    if (selectList) selectListDepth = parenDepth;
                    previous = token;
                    continue;
                }

                if (upper == "JOIN")
                {
                    NewLine();
                    Append(text, false);
                    selectList = false;
                    previous = token;
                    continue;
                }

                if (upper == "ON")
                {
                    NewLine(1);
                    Append(text, false);
                    previous = token;
                    continue;
                }

                if (upper is "AND" or "OR")
                {
                    NewLine(1);
                    Append(text, false);
                    previous = token;
                    continue;
                }
            }

            if (text == "(")
            {
                var subquery = NextWordIs(tokens, i + 1, "SELECT") || NextWordIs(tokens, i + 1, "WITH");
                var previousUpper = previous?.Kind == SqlTokenKind.Word ? previous.Text.ToUpperInvariant() : string.Empty;
                var spaceBeforeParen = previousUpper is "IN" or "EXISTS" or "VALUES";
                Append("(", spaceBeforeParen);
                parenDepth++;
                subqueryParens.Push(subquery);
                if (subquery)
                {
                    indent++;
                    NewLine();
                }
                previous = token;
                continue;
            }

            if (text == ")")
            {
                var closesSubquery = subqueryParens.Count > 0 && subqueryParens.Pop();
                if (parenDepth > 0) parenDepth--;
                if (closesSubquery)
                {
                    indent = Math.Max(0, indent - 1);
                    NewLine();
                }
                TrimTrailingSpaces(sb);
                Append(")", false);
                previous = token;
                continue;
            }

            if (text == ",")
            {
                TrimTrailingSpaces(sb);
                sb.Append(',');
                if (selectList && parenDepth == selectListDepth) NewLine(1);
                else sb.Append(' ');
                previous = token;
                continue;
            }

            if (text == ";")
            {
                TrimTrailingSpaces(sb);
                sb.Append(';');
                NewLine();
                selectList = false;
                previous = token;
                continue;
            }

            if (text == ".")
            {
                TrimTrailingSpaces(sb);
                sb.Append('.');
                previous = token;
                continue;
            }

            var forceSpace = previous?.Text != "." && text != ".";
            Append(text, forceSpace);
            previous = token;
        }

        return RemoveExcessBlankLines(sb.ToString()).Trim();
    }

    private static string FormatBraceLanguage(string code, int indentSize, string language)
    {
        // PHP heredoc/nowdoc and C# raw strings need a full lexer. Preserve source rather than corrupt them.
        if ((language == "PHP" && code.Contains("<<<", StringComparison.Ordinal)) ||
            (language == "C#" && code.Contains("\"\"\"", StringComparison.Ordinal)) ||
            (language == "Rust" && Regex.IsMatch(code, "\\br#+\\\"")))
            return FormatWhitespaceOnly(code);

        var sb = new StringBuilder(code.Length + 96);
        var indent = 0;
        var parenDepth = 0;
        var state = LexState.Code;
        var quote = '\0';
        var escaped = false;
        var verbatimString = false;
        var atLineStart = true;

        void EnsureIndent()
        {
            if (!atLineStart) return;
            sb.Append(' ', Math.Max(0, indent) * indentSize);
            atLineStart = false;
        }

        void NewLine()
        {
            TrimTrailingSpaces(sb);
            if (sb.Length == 0 || sb[^1] != '\n') sb.Append('\n');
            atLineStart = true;
        }

        for (var i = 0; i < code.Length; i++)
        {
            var ch = code[i];
            var next = i + 1 < code.Length ? code[i + 1] : '\0';

            if (state == LexState.LineComment)
            {
                EnsureIndent();
                if (ch == '\r') continue;
                sb.Append(ch);
                if (ch == '\n') { state = LexState.Code; atLineStart = true; }
                continue;
            }

            if (state == LexState.BlockComment)
            {
                EnsureIndent();
                sb.Append(ch);
                if (ch == '*' && next == '/')
                {
                    sb.Append('/');
                    i++;
                    state = LexState.Code;
                }
                continue;
            }

            if (state == LexState.String)
            {
                EnsureIndent();
                sb.Append(ch);
                if (verbatimString && ch == '"' && next == '"')
                {
                    sb.Append(next);
                    i++;
                    continue;
                }
                if (escaped) { escaped = false; continue; }
                if (!verbatimString && ch == '\\') { escaped = true; continue; }
                if (ch == quote)
                {
                    state = LexState.Code;
                    verbatimString = false;
                }
                continue;
            }

            if (ch == '/' && next == '/')
            {
                EnsureIndent();
                sb.Append("//");
                i++;
                state = LexState.LineComment;
                continue;
            }

            if (ch == '/' && next == '*')
            {
                EnsureIndent();
                sb.Append("/*");
                i++;
                state = LexState.BlockComment;
                continue;
            }

            if (language == "PHP" && ch == '#')
            {
                EnsureIndent();
                sb.Append(ch);
                state = LexState.LineComment;
                continue;
            }

            if (ch is '\'' or '"' or '`')
            {
                EnsureIndent();
                sb.Append(ch);
                quote = ch;
                verbatimString = language == "C#" && ch == '"' && i > 0 && code[i - 1] == '@';
                state = LexState.String;
                continue;
            }

            if (ch is '(' or '[')
            {
                EnsureIndent();
                sb.Append(ch);
                parenDepth++;
                continue;
            }

            if (ch is ')' or ']')
            {
                EnsureIndent();
                sb.Append(ch);
                parenDepth = Math.Max(0, parenDepth - 1);
                continue;
            }

            if (ch == '{')
            {
                if (!atLineStart && sb.Length > 0 && sb[^1] is not (' ' or '\n')) sb.Append(' ');
                EnsureIndent();
                sb.Append('{');
                indent++;
                NewLine();
                continue;
            }

            if (ch == '}')
            {
                if (!atLineStart) NewLine();
                indent = Math.Max(0, indent - 1);
                EnsureIndent();
                sb.Append('}');
                if (next is not ';' and not ',' and not ')' and not ']' && !StartsContinuationWord(code, i + 1)) NewLine();
                continue;
            }

            if (ch == ';' && parenDepth == 0)
            {
                EnsureIndent();
                sb.Append(';');
                NewLine();
                continue;
            }

            if (ch == ',')
            {
                EnsureIndent();
                TrimTrailingSpaces(sb);
                sb.Append(',');
                if (parenDepth > 0) sb.Append(' ');
                continue;
            }

            if (ch == '\r') continue;
            if (ch == '\n')
            {
                if (!atLineStart) NewLine();
                continue;
            }

            if (char.IsWhiteSpace(ch))
            {
                if (!atLineStart && sb.Length > 0 && sb[^1] is not (' ' or '\n' or '\t')) sb.Append(' ');
                continue;
            }

            EnsureIndent();
            sb.Append(ch);
        }

        return RemoveExcessBlankLines(sb.ToString()).Trim();
    }

    private static string FormatPython(string code, int indentSize)
    {
        // Python indentation is syntax. Preserve block depth and normalize the existing indentation unit only.
        return NormalizeIndentationSensitive(code, indentSize);
    }

    private static string FormatKeywordBlocks(
        string code,
        int indentSize,
        Func<string, bool> isOpen,
        Func<string, bool> isClose,
        Func<string, bool> isMiddle)
    {
        var lines = NormalizeNewLines(code).Split('\n');
        var sb = new StringBuilder(code.Length + 32);
        var indent = 0;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0)
            {
                if (sb.Length > 0 && sb[^1] != '\n') sb.Append('\n');
                continue;
            }

            if (isClose(line) || isMiddle(line)) indent = Math.Max(0, indent - 1);
            sb.Append(' ', indent * indentSize).Append(line).Append('\n');
            if (isOpen(line) || isMiddle(line)) indent++;
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatYaml(string code, int indentSize)
    {
        // YAML indentation is data structure. Never infer hierarchy from keys; normalize existing levels only.
        return NormalizeIndentationSensitive(code, indentSize);
    }

    private static string NormalizeIndentationSensitive(string code, int indentSize)
    {
        var lines = NormalizeNewLines(code).Split('\n');
        var positiveIndents = new List<int>();
        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var width = LeadingIndentWidth(raw);
            if (width > 0) positiveIndents.Add(width);
        }

        var sourceUnit = positiveIndents.Count == 0 ? indentSize : positiveIndents.Min();
        sourceUnit = Math.Max(1, sourceUnit);
        var output = new List<string>(lines.Length);

        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                output.Add(string.Empty);
                continue;
            }

            var width = LeadingIndentWidth(raw);
            var levels = width / sourceUnit;
            var content = raw.TrimStart(' ', '\t').TrimEnd();
            output.Add(new string(' ', levels * indentSize) + content);
        }

        return RemoveExcessBlankLines(string.Join("\n", output));
    }

    private static int LeadingIndentWidth(string line)
    {
        var width = 0;
        foreach (var ch in line)
        {
            if (ch == ' ') width++;
            else if (ch == '\t') width += 4;
            else break;
        }
        return width;
    }

    private static List<SqlToken> TokenizeSql(string code)
    {
        var result = new List<SqlToken>();
        var current = new StringBuilder();

        void FlushWord()
        {
            if (current.Length == 0) return;
            result.Add(new SqlToken(current.ToString(), SqlTokenKind.Word));
            current.Clear();
        }

        for (var i = 0; i < code.Length; i++)
        {
            var ch = code[i];
            var next = i + 1 < code.Length ? code[i + 1] : '\0';

            if (char.IsWhiteSpace(ch)) { FlushWord(); continue; }

            if (ch == '-' && next == '-')
            {
                FlushWord();
                var start = i;
                i += 2;
                while (i < code.Length && code[i] != '\n') i++;
                result.Add(new SqlToken(code[start..i], SqlTokenKind.LineComment));
                i--;
                continue;
            }

            if (ch == '/' && next == '*')
            {
                FlushWord();
                var start = i;
                i += 2;
                while (i + 1 < code.Length && !(code[i] == '*' && code[i + 1] == '/')) i++;
                i = Math.Min(code.Length - 1, i + 1);
                result.Add(new SqlToken(code[start..(i + 1)], SqlTokenKind.BlockComment));
                continue;
            }

            if (ch is '\'' or '"' or '[' or '`')
            {
                FlushWord();
                var close = ch == '[' ? ']' : ch;
                var start = i;
                i++;
                while (i < code.Length)
                {
                    if (code[i] == close)
                    {
                        if (close == '\'' && i + 1 < code.Length && code[i + 1] == '\'') { i += 2; continue; }
                        break;
                    }
                    i++;
                }
                i = Math.Min(i, code.Length - 1);
                result.Add(new SqlToken(code[start..(i + 1)], SqlTokenKind.Literal));
                continue;
            }

            if (",;().".Contains(ch))
            {
                FlushWord();
                result.Add(new SqlToken(ch.ToString(), SqlTokenKind.Punctuation));
                continue;
            }

            if ("=<>!+-*/%".Contains(ch))
            {
                FlushWord();
                if (i + 1 < code.Length && "=<>".Contains(code[i + 1]))
                {
                    result.Add(new SqlToken(code.Substring(i, 2), SqlTokenKind.Operator));
                    i++;
                }
                else result.Add(new SqlToken(ch.ToString(), SqlTokenKind.Operator));
                continue;
            }

            current.Append(ch);
        }

        FlushWord();
        return result;
    }

    private static bool PeekWord(List<SqlToken> tokens, int start, string expected)
    {
        var index = SkipWhitespaceTokens(tokens, start);
        return index < tokens.Count && tokens[index].Kind == SqlTokenKind.Word && tokens[index].Text.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static bool NextWordIs(List<SqlToken> tokens, int start, string expected) => PeekWord(tokens, start, expected);
    private static int SkipWhitespaceTokens(List<SqlToken> tokens, int start) => Math.Min(start, tokens.Count);

    private static bool IsMajorSqlClause(string upper) => upper is
        "SELECT" or "FROM" or "WHERE" or "HAVING" or "UNION" or "EXCEPT" or "INTERSECT" or
        "INSERT" or "UPDATE" or "DELETE" or "MERGE" or "VALUES" or "SET" or "RETURNING" or "OUTPUT" or
        "LIMIT" or "OFFSET" or "FETCH" or "WITH";

    private static bool StartsContinuationWord(string code, int index)
    {
        while (index < code.Length && char.IsWhiteSpace(code[index])) index++;
        var remainder = code[index..];
        return Regex.IsMatch(remainder, "^(else|catch|finally|while)\\b", RegexOptions.IgnoreCase);
    }

    private static bool EndsWithPythonBlockColon(string line)
    {
        if (!line.EndsWith(':')) return false;
        var codeOnly = StripTrailingComment(line);
        return codeOnly.EndsWith(':');
    }

    private static string StripTrailingComment(string line)
    {
        var single = false;
        var dbl = false;
        var escaped = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (escaped) { escaped = false; continue; }
            if (ch == '\\' && (single || dbl)) { escaped = true; continue; }
            if (ch == '\'' && !dbl) single = !single;
            else if (ch == '"' && !single) dbl = !dbl;
            else if (ch == '#' && !single && !dbl) return line[..i].TrimEnd();
        }
        return line.TrimEnd();
    }

    private static int CountOutsideStrings(string line, string opens, string closes)
    {
        var depth = 0;
        var single = false;
        var dbl = false;
        var escaped = false;
        foreach (var ch in line)
        {
            if (escaped) { escaped = false; continue; }
            if (ch == '\\' && (single || dbl)) { escaped = true; continue; }
            if (ch == '\'' && !dbl) { single = !single; continue; }
            if (ch == '"' && !single) { dbl = !dbl; continue; }
            if (single || dbl) continue;
            if (opens.Contains(ch)) depth++;
            if (closes.Contains(ch)) depth--;
        }
        return depth;
    }

    private static bool ShellOpen(string line) => Regex.IsMatch(line, "^(if\\b.*then\\b|for\\b.*do\\b|while\\b.*do\\b|until\\b.*do\\b|case\\b.*\\bin$|select\\b.*do\\b|function\\b.*\\{|[A-Za-z_][A-Za-z0-9_]*\\s*\\(\\)\\s*\\{)", RegexOptions.IgnoreCase);
    private static bool ShellClose(string line) => Regex.IsMatch(line, "^(fi|done|esac)\\b|^}", RegexOptions.IgnoreCase);
    private static bool ShellMiddle(string line) => Regex.IsMatch(line, "^(else|elif\\b)", RegexOptions.IgnoreCase);
    private static bool RubyOpen(string line) => Regex.IsMatch(line, "^(class|module|def|if|unless|case|begin|while|until|for)\\b|\\bdo\\s*(\\|.*\\|)?$", RegexOptions.IgnoreCase);
    private static bool RubyClose(string line) => Regex.IsMatch(line, "^end\\b", RegexOptions.IgnoreCase);
    private static bool RubyMiddle(string line) => Regex.IsMatch(line, "^(else|elsif|when|rescue|ensure)\\b", RegexOptions.IgnoreCase);
    private static bool LuaOpen(string line) => Regex.IsMatch(line, "^(function\\b|if\\b.*then$|for\\b.*do$|while\\b.*do$|repeat$|do$)", RegexOptions.IgnoreCase);
    private static bool LuaClose(string line) => Regex.IsMatch(line, "^(end|until\\b)", RegexOptions.IgnoreCase);
    private static bool LuaMiddle(string line) => Regex.IsMatch(line, "^(else|elseif\\b)", RegexOptions.IgnoreCase);

    private static bool IsHtmlVoidElement(string token)
    {
        var tag = GetTagName(token);
        return tag is "area" or "base" or "br" or "col" or "embed" or "hr" or "img" or "input" or "link" or "meta" or "param" or "source" or "track" or "wbr";
    }

    private static string GetTagName(string token)
    {
        var match = Regex.Match(token, "^</?\\s*([A-Za-z][A-Za-z0-9:-]*)");
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : string.Empty;
    }

    private static void AppendWrappedText(StringBuilder sb, int indent, string text, int indentSize)
    {
        var normalized = Regex.Replace(text, "\\s+", " ").Trim();
        if (normalized.Length > 0) AppendLine(sb, indent, normalized, indentSize);
    }

    private static void AppendLine(StringBuilder sb, int indent, string text, int indentSize)
    {
        if (sb.Length > 0 && sb[^1] != '\n') sb.Append('\n');
        sb.Append(' ', Math.Max(0, indent) * indentSize).Append(text).Append('\n');
    }

    private static int FindYamlColon(string line)
    {
        var single = false;
        var dbl = false;
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '\'' && !dbl) single = !single;
            else if (line[i] == '"' && !single) dbl = !dbl;
            else if (line[i] == ':' && !single && !dbl) return i;
        }
        return -1;
    }

    private static string ConvertIndent(string text, int sourceSize, int targetSize)
    {
        return string.Join("\n", NormalizeNewLines(text).Split('\n').Select(line =>
        {
            var count = 0;
            while (count < line.Length && line[count] == ' ') count++;
            var levels = count / sourceSize;
            return new string(' ', levels * targetSize) + line[count..];
        }));
    }

    private static string FormatWhitespaceOnly(string code)
    {
        var lines = NormalizeNewLines(code).Split('\n');
        var cleaned = lines.Select(line => line.TrimEnd()).ToArray();
        return RemoveExcessBlankLines(string.Join("\n", cleaned));
    }

    private static void TrimTrailingSpaces(StringBuilder sb)
    {
        while (sb.Length > 0 && sb[^1] is ' ' or '\t') sb.Length--;
    }

    private static string RemoveExcessBlankLines(string text)
        => Regex.Replace(NormalizeNewLines(text), "\\n{3,}", "\n\n");

    private static string TrimOuterBlankLines(string text)
        => NormalizeNewLines(text).Trim('\n', '\r');

    private static string NormalizeNewLines(string text)
        => text.Replace("\r\n", "\n").Replace('\r', '\n');

    private enum LexState { Code, String, LineComment, BlockComment }
    private enum SqlTokenKind { Word, Literal, Operator, Punctuation, LineComment, BlockComment }
    private sealed record SqlToken(string Text, SqlTokenKind Kind);
}
