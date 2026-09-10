using System.Text.RegularExpressions;
using CodyFormat.Models;

namespace CodyFormat.Services;

public sealed class SyntaxHighlighterService
{
    private static readonly Regex GenericTokenRegex = new(
        "(?<string>@\"(?:\"\"|[^\"])*\"|\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'|`(?:\\\\.|[^`\\\\])*`)|" +
        "(?<blockcomment>/\\*.*?\\*/|<#.*?#>)|" +
        "(?<number>\\b(?:0x[0-9A-Fa-f]+|0b[01]+|\\d+(?:\\.\\d+)?)\\b)|" +
        "(?<variable>\\$[A-Za-z_][A-Za-z0-9_:]*|@[A-Za-z_][A-Za-z0-9_]*)|" +
        "(?<word>\\b[A-Za-z_][A-Za-z0-9_]*\\b)|" +
        "(?<operator>===|!==|==|!=|<=|>=|=>|->|::|:=|&&|\\|\\||\\+\\+|--|\\+=|-=|\\*=|/=|%=|<<|>>|[+\\-*\\/%=<>!&|?:.^~])",
        RegexOptions.Compiled);

    private static readonly Regex MarkupRegex = new(
        "(?<comment><!--.*?-->)|(?<tag></?[A-Za-z_:][\\w:.-]*)|(?<attr>\\s+[A-Za-z_:][\\w:.-]*(?=\\s*=))|(?<string>\"[^\"]*\"|'[^']*')|(?<punct>[<>/=])",
        RegexOptions.Compiled);

    private static readonly Regex CssRegex = new(
        "(?<comment>/\\*.*?\\*/)|(?<string>\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*')|(?<atrule>@[A-Za-z-]+)|(?<property>--?[A-Za-z][\\w-]*(?=\\s*:)|[A-Za-z][\\w-]*(?=\\s*:))|(?<number>\\b\\d+(?:\\.\\d+)?(?:px|em|rem|vh|vw|%|s|ms|deg)?\\b)|(?<color>#[0-9A-Fa-f]{3,8}\\b)|(?<word>\\b[A-Za-z_-][A-Za-z0-9_-]*\\b)|(?<operator>[{}:;,>+~*=()])",
        RegexOptions.Compiled);

    private static readonly Dictionary<string, HashSet<string>> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["C#"] = Set("abstract as base bool byte char checked class const decimal delegate double enum event explicit extern fixed float implicit int interface internal is long namespace object operator out override params private protected public readonly record ref sbyte sealed short sizeof stackalloc static string struct this typeof uint ulong unchecked unsafe ushort using virtual void volatile dynamic var required init partial global file"),
        ["SQL"] = Set("SELECT FROM JOIN INNER LEFT RIGHT FULL OUTER CROSS ON AS NULL IS IN EXISTS BETWEEN LIKE DISTINCT TOP LIMIT OFFSET FETCH INSERT INTO VALUES UPDATE SET DELETE CREATE ALTER DROP TABLE VIEW PROCEDURE FUNCTION INDEX PRIMARY KEY FOREIGN REFERENCES CONSTRAINT DATABASE SCHEMA TEMP TEMPORARY WITH RECURSIVE OVER PARTITION BY ROWS RANGE ASC DESC MERGE USING MATCHED PIVOT UNPIVOT RETURNING OUTPUT UNION EXCEPT INTERSECT"),
        ["JavaScript"] = Set("class const debugger delete export extends function import in instanceof let new static super this typeof var void with yield async await of get set"),
        ["TypeScript"] = Set("class const debugger delete export extends function import in instanceof let new static super this typeof var void with yield async await of interface type enum implements public private protected readonly abstract declare namespace module keyof infer unknown never any number string boolean object symbol bigint satisfies override get set"),
        ["Python"] = Set("as assert async await class def del from global import lambda nonlocal pass raise with yield match case"),
        ["PowerShell"] = Set("begin class data define dynamicparam end enum filter from function in inlinescript parallel param process sequence using var workflow"),
        ["Bash"] = Set("case esac function in select time coproc readonly local declare export unset shift source alias"),
        ["Rust"] = Set("as async await const crate dyn enum extern fn impl in let loop match mod move mut pub ref self Self static struct super trait type unsafe use where macro_rules"),
        ["Go"] = Set("break default func interface select case defer go map struct chan else goto package switch const fallthrough if range type continue for import return var"),
        ["PHP"] = Set("abstract and array as break callable case catch class clone const continue declare default die do echo else elseif empty enddeclare endfor endforeach endif endswitch endwhile eval exit extends final finally fn for foreach function global goto if implements include include_once instanceof insteadof interface isset list match namespace new or print private protected public readonly require require_once return static switch throw trait try unset use var while xor yield"),
        ["Java"] = Set("abstract assert boolean break byte case catch char class const default double enum exports extends final float implements import instanceof int interface long module native new non-sealed opens package permits private protected public record requires sealed short static strictfp super synchronized this throws to transient transitive uses var void volatile with"),
        ["C"] = Set("auto break case char const double enum extern float int long register restrict short signed sizeof static struct typedef union unsigned void volatile _Bool _Complex _Atomic"),
        ["C++"] = Set("alignas alignof asm auto bool char char8_t char16_t char32_t class concept const consteval constexpr constinit const_cast decltype double dynamic_cast enum explicit export extern float friend inline int long mutable namespace new noexcept nullptr operator private protected public register reinterpret_cast requires short signed sizeof static static_assert static_cast struct template thread_local typedef typeid typename union unsigned using virtual void volatile wchar_t"),
        ["Kotlin"] = Set("as break class continue do else false for fun if in interface is null object package return super this throw true try typealias typeof val var when while by catch constructor delegate dynamic field file finally get import init param property receiver set setparam where actual abstract annotation companion const crossinline data enum expect external final infix inline inner internal lateinit noinline open operator out override private protected public reified sealed suspend tailrec vararg"),
        ["Swift"] = Set("associatedtype class deinit enum extension fileprivate func import init inout internal let open operator private protocol public rethrows static struct subscript typealias var break case continue default defer do else fallthrough for guard if in repeat return switch where while as catch false is nil super self Self throw throws true try"),
        ["Dart"] = Set("abstract as assert async await base break case catch class const continue covariant default deferred do dynamic else enum export extends extension external factory false final finally for Function get hide if implements import in interface is late library mixin new null of on operator part required rethrow return sealed set show static super switch sync this throw true try type typedef var void when while with yield"),
        ["Ruby"] = Set("BEGIN END alias and begin break case class def defined do else elsif end ensure false for if in module next nil not or redo rescue retry return self super then true undef unless until when while yield"),
        ["Lua"] = Set("and break do else elseif end false for function goto if in local nil not or repeat return then true until while"),
        ["R"] = Set("break else FALSE for function if Inf NA NaN next NULL repeat TRUE while"),
        ["Perl"] = Set("BEGIN END CHECK INIT UNITCHECK package sub my our local state use require if elsif else unless while until for foreach continue given when default last next redo goto return die warn eval do format"),
        ["Objective-C"] = Set("interface implementation protocol property synthesize dynamic selector encode synchronized autoreleasepool try catch finally throw class public protected private package end import typedef struct enum static const void int float double char BOOL id Class SEL self super nil YES NO"),
        ["Scala"] = Set("abstract case catch class def do else extends false final finally for forSome if implicit import lazy match new null object override package private protected return sealed super this throw trait try true type val var while with yield given using enum export extension inline opaque open transparent"),
        ["Groovy"] = Set("as assert break case catch class const continue def default do else enum extends false finally for goto if implements import in instanceof interface new null package return super switch this throw throws trait true try while"),
        ["JSON"] = Set(string.Empty)
    };

    private static readonly Dictionary<string, HashSet<string>> ControlKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["C#"] = Set("break case catch continue default do else false finally for foreach goto if lock new null return switch throw true try while async await"),
        ["SQL"] = Set("WHERE AND OR NOT GROUP HAVING ORDER UNION ALL CASE WHEN THEN ELSE END IF BEGIN COMMIT ROLLBACK TRANSACTION TRY CATCH"),
        ["JavaScript"] = Set("break case catch continue default do else false finally for if null return switch throw true try while"),
        ["TypeScript"] = Set("break case catch continue default do else false finally for if null return switch throw true try while"),
        ["Python"] = Set("and break continue elif else except False finally for if is None not or return True try while"),
        ["Rust"] = Set("break continue else false if return true while"),
        ["Go"] = Set("break case continue default else fallthrough for goto if return select switch"),
        ["PHP"] = Set("break case catch continue default else elseif false finally for foreach if null return switch throw true try while"),
        ["Java"] = Set("break case catch continue default do else false finally for if null return switch throw true try while"),
        ["C"] = Set("break case continue default do else for goto if return switch while"),
        ["C++"] = Set("break case catch continue default do else false for goto if nullptr return switch throw true try while"),
        ["Kotlin"] = Set("break catch continue else false finally for if null return throw true try when while"),
        ["Swift"] = Set("break case catch continue default else false for guard if nil repeat return switch throw true while"),
        ["Dart"] = Set("break case catch continue default else false finally for if null return switch throw true try while"),
        ["PowerShell"] = Set("break catch continue do else elseif exit finally for foreach if return switch throw trap try until while"),
        ["Bash"] = Set("if then else elif fi for while until do done break continue return"),
        ["Ruby"] = Set("break case else elsif ensure false if next nil rescue return true unless until when while"),
        ["Lua"] = Set("break else elseif false if nil repeat return true until while"),
        ["JSON"] = Set("true false null")
    };

    private static readonly Dictionary<string, HashSet<string>> BuiltIns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SQL"] = Set("COUNT SUM AVG MIN MAX COALESCE ISNULL NVL CAST CONVERT SUBSTRING CONCAT UPPER LOWER LEN LENGTH TRIM LTRIM RTRIM DATEADD DATEDIFF GETDATE CURRENT_DATE CURRENT_TIMESTAMP ROW_NUMBER RANK DENSE_RANK LAG LEAD FIRST_VALUE LAST_VALUE STRING_AGG JSON_VALUE JSON_QUERY"),
        ["C#"] = Set("Console Math Convert String Array Enumerable Task File Directory Path Guid DateTime DateTimeOffset TimeSpan Regex JsonSerializer"),
        ["JavaScript"] = Set("console document window Math JSON Object Array String Number Boolean Date Promise Set Map RegExp fetch parseInt parseFloat"),
        ["TypeScript"] = Set("console document window Math JSON Object Array String Number Boolean Date Promise Set Map RegExp fetch parseInt parseFloat"),
        ["Python"] = Set("print len range enumerate zip map filter list dict set tuple str int float bool type isinstance open sum min max sorted reversed any all abs round input super property staticmethod classmethod"),
        ["Rust"] = Set("println print vec String Vec Option Result Some None Ok Err Box Rc Arc Mutex HashMap HashSet"),
        ["Go"] = Set("fmt Println Printf Sprintf make len cap append copy delete panic recover new complex real imag close"),
        ["PHP"] = Set("echo print count strlen array_map array_filter array_reduce json_encode json_decode isset empty in_array explode implode sprintf printf date time preg_match preg_replace"),
        ["Java"] = Set("System String Math Arrays Collections Objects Optional Stream List Map Set HashMap ArrayList LocalDate LocalDateTime"),
        ["PowerShell"] = Set("Write-Host Write-Output Get-Item Get-ChildItem Get-Content Set-Content New-Item Remove-Item Select-Object Where-Object ForEach-Object Measure-Object Sort-Object Test-Path Invoke-RestMethod Invoke-WebRequest"),
        ["Bash"] = Set("echo printf grep awk sed cat head tail sort uniq cut tr find xargs chmod chown mkdir rm cp mv curl wget jq")
    };

    private static readonly HashSet<string> TypeWords = Set("string int long short byte bool double float decimal char object DateTime DateTimeOffset Guid Task List Dictionary HashSet IEnumerable IQueryable Exception Stream FileInfo DirectoryInfo CancellationToken number boolean any unknown never Array Promise HTMLElement dict list tuple set str bytes bool i8 i16 i32 i64 i128 isize u8 u16 u32 u64 u128 usize f32 f64 String Vec Option Result error rune uint uint8 uint16 uint32 uint64 uintptr int8 int16 int32 int64 float32 float64 complex64 complex128 mixed iterable callable resource self static void Object dynamic num String bool List Map Set Future Widget BigInt");

    public IReadOnlyList<StyledToken> HighlightLine(string line, string language)
    {
        if (language.Equals("Plain Text", StringComparison.OrdinalIgnoreCase) || language == "Markdown")
            return [new StyledToken(line, TokenKind.Plain)];
        if (language is "HTML" or "XML") return HighlightMarkup(line);
        if (language == "CSS") return HighlightCss(line);
        return HighlightGeneric(line, language);
    }

    private static IReadOnlyList<StyledToken> HighlightGeneric(string line, string language)
    {
        var commentIndex = FindLineCommentStart(line, language);
        var codePart = commentIndex >= 0 ? line[..commentIndex] : line;
        var commentPart = commentIndex >= 0 ? line[commentIndex..] : null;
        var tokens = new List<StyledToken>();
        var index = 0;
        var keywords = Keywords.GetValueOrDefault(language) ?? EmptySet;
        var controls = ControlKeywords.GetValueOrDefault(language) ?? EmptySet;
        var builtIns = BuiltIns.GetValueOrDefault(language) ?? EmptySet;

        foreach (Match match in GenericTokenRegex.Matches(codePart))
        {
            if (match.Index > index) tokens.Add(new StyledToken(codePart[index..match.Index], TokenKind.Plain));
            var value = match.Value;
            TokenKind kind;

            if (match.Groups["string"].Success) kind = TokenKind.String;
            else if (match.Groups["blockcomment"].Success) kind = TokenKind.Comment;
            else if (match.Groups["number"].Success) kind = TokenKind.Number;
            else if (match.Groups["variable"].Success) kind = value.StartsWith('@') ? TokenKind.Attribute : TokenKind.Variable;
            else if (match.Groups["operator"].Success) kind = TokenKind.Operator;
            else kind = ClassifyWord(codePart, match, value, language, keywords, controls, builtIns);

            tokens.Add(new StyledToken(value, kind));
            index = match.Index + match.Length;
        }

        if (index < codePart.Length) tokens.Add(new StyledToken(codePart[index..], TokenKind.Plain));
        if (commentPart is not null) tokens.Add(new StyledToken(commentPart, TokenKind.Comment));
        return tokens;
    }

    private static TokenKind ClassifyWord(string line, Match match, string value, string language, HashSet<string> keywords, HashSet<string> controls, HashSet<string> builtIns)
    {
        if (controls.Contains(value)) return TokenKind.ControlKeyword;
        if (keywords.Contains(value)) return TokenKind.Keyword;
        if (builtIns.Contains(value)) return TokenKind.Function;
        if (TypeWords.Contains(value)) return TokenKind.Type;
        if (language == "SQL") return IsFollowedBy(line, match.Index + match.Length, '(') ? TokenKind.Function : TokenKind.Plain;
        if (language == "JSON" && IsFollowedBy(line, match.Index + match.Length, ':')) return TokenKind.Property;
        if (IsPrecededByDot(line, match.Index) || (language == "PHP" && IsPrecededByArrow(line, match.Index))) return TokenKind.Property;
        if (IsFollowedBy(line, match.Index + match.Length, '(')) return TokenKind.Function;
        if (value is "true" or "false" or "null" or "True" or "False" or "None" or "nil") return TokenKind.Constant;
        return TokenKind.Plain;
    }

    private static IReadOnlyList<StyledToken> HighlightMarkup(string line)
    {
        var tokens = new List<StyledToken>();
        var index = 0;
        foreach (Match match in MarkupRegex.Matches(line))
        {
            if (match.Index > index) tokens.Add(new StyledToken(line[index..match.Index], TokenKind.Plain));
            var kind = match.Groups["comment"].Success ? TokenKind.Comment
                : match.Groups["tag"].Success ? TokenKind.Tag
                : match.Groups["attr"].Success ? TokenKind.Attribute
                : match.Groups["string"].Success ? TokenKind.String
                : TokenKind.Operator;
            tokens.Add(new StyledToken(match.Value, kind));
            index = match.Index + match.Length;
        }
        if (index < line.Length) tokens.Add(new StyledToken(line[index..], TokenKind.Plain));
        return tokens;
    }

    private static IReadOnlyList<StyledToken> HighlightCss(string line)
    {
        var tokens = new List<StyledToken>();
        var index = 0;
        foreach (Match match in CssRegex.Matches(line))
        {
            if (match.Index > index) tokens.Add(new StyledToken(line[index..match.Index], TokenKind.Plain));
            var kind = match.Groups["comment"].Success ? TokenKind.Comment
                : match.Groups["string"].Success ? TokenKind.String
                : match.Groups["atrule"].Success ? TokenKind.Keyword
                : match.Groups["property"].Success ? TokenKind.Property
                : match.Groups["number"].Success ? TokenKind.Number
                : match.Groups["color"].Success ? TokenKind.Constant
                : match.Groups["operator"].Success ? TokenKind.Operator
                : TokenKind.Plain;
            tokens.Add(new StyledToken(match.Value, kind));
            index = match.Index + match.Length;
        }
        if (index < line.Length) tokens.Add(new StyledToken(line[index..], TokenKind.Plain));
        return tokens;
    }

    private static int FindLineCommentStart(string line, string language)
    {
        var markers = language switch
        {
            "SQL" or "Lua" => new[] { "--" },
            "Python" or "PowerShell" or "Bash" or "Ruby" or "R" or "Perl" or "YAML" => new[] { "#" },
            "PHP" => new[] { "//", "#" },
            "C#" or "JavaScript" or "TypeScript" or "Rust" or "Go" or "Java" or "C" or "C++" or "Kotlin" or "Swift" or "Dart" or "Objective-C" or "Scala" or "Groovy" => new[] { "//" },
            _ => Array.Empty<string>()
        };

        if (markers.Length == 0) return -1;
        var inSingle = false;
        var inDouble = false;
        var inBacktick = false;
        var escaped = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (escaped) { escaped = false; continue; }
            if (ch == '\\' && (inSingle || inDouble || inBacktick)) { escaped = true; continue; }
            if (ch == '\'' && !inDouble && !inBacktick) inSingle = !inSingle;
            else if (ch == '"' && !inSingle && !inBacktick) inDouble = !inDouble;
            else if (ch == '`' && !inSingle && !inDouble) inBacktick = !inBacktick;
            if (inSingle || inDouble || inBacktick) continue;

            foreach (var marker in markers)
                if (i + marker.Length <= line.Length && line.AsSpan(i, marker.Length).SequenceEqual(marker.AsSpan())) return i;
        }
        return -1;
    }

    private static bool IsFollowedBy(string line, int index, char expected)
    {
        for (var i = index; i < line.Length; i++) { if (char.IsWhiteSpace(line[i])) continue; return line[i] == expected; }
        return false;
    }

    private static bool IsPrecededByDot(string line, int index)
    {
        for (var i = index - 1; i >= 0; i--) { if (char.IsWhiteSpace(line[i])) continue; return line[i] == '.'; }
        return false;
    }

    private static bool IsPrecededByArrow(string line, int index)
    {
        var prefix = line[..Math.Max(0, index)].TrimEnd();
        return prefix.EndsWith("->", StringComparison.Ordinal) || prefix.EndsWith("::", StringComparison.Ordinal);
    }

    private static readonly HashSet<string> EmptySet = new(StringComparer.OrdinalIgnoreCase);
    private static HashSet<string> Set(string words) => new(words.Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
}
