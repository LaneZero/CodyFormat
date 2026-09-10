using System.Text.RegularExpressions;

namespace CodyFormat.Services;

public static class LanguageDetector
{
    public static string Detect(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "Plain Text";

        var scores = LanguageCatalog.Languages
            .Where(x => x is not "Auto Detect" and not "Plain Text")
            .ToDictionary(x => x, _ => 0, StringComparer.OrdinalIgnoreCase);

        Add(scores, "C#", code, @"\b(namespace|using|public|private|protected|internal|class|record|struct|interface|async|await|var|string|bool|void)\b", 2);
        Add(scores, "C#", code, @"\b(Console\.Write(Line)?|Task<|IEnumerable<|DateTime|Guid)\b", 4);
        Add(scores, "SQL", code, @"\b(SELECT|FROM|WHERE|JOIN|GROUP\s+BY|ORDER\s+BY|INSERT\s+INTO|UPDATE|DELETE\s+FROM|CREATE\s+(TABLE|VIEW|PROCEDURE))\b", 3, true);
        Add(scores, "Python", code, @"(?m)^\s*(def|class|from|import|for|while|if|elif|with|try|except)\b.*:?$", 3);
        Add(scores, "TypeScript", code, @"\b(interface|type|enum|implements|readonly|keyof|unknown|never)\b|:\s*(string|number|boolean)\b", 3);
        Add(scores, "JavaScript", code, @"\b(const|let|var|function|async|await)\b|=>|console\.log|document\.|window\.", 2);
        Add(scores, "PowerShell", code, @"\b(Get-|Set-|New-|Remove-|Write-Host|Write-Output|param\s*\()|\$[A-Za-z_]", 2, true);
        Add(scores, "Bash", code, @"(?m)^#!.*\b(bash|sh)\b|\b(echo|grep|awk|sed|chmod|sudo|apt|dnf|export)\b", 3);
        Add(scores, "Rust", code, @"\b(fn|let\s+mut|impl|trait|match|pub\s+(struct|enum|fn)|Result<|Option<|println!)\b|::", 3);
        Add(scores, "Go", code, @"(?m)^\s*package\s+\w+|\bfunc\s+\w+\s*\(|:=|\b(go|defer|chan|interface|range)\b", 4);
        Add(scores, "PHP", code, @"<\?php|\$[A-Za-z_]\w*|\b(function|namespace|use|echo|foreach)\b", 3, true);
        Add(scores, "Java", code, @"\b(public\s+static\s+void\s+main|System\.out\.println|package\s+[\w.]+;|class\s+\w+|implements\s+\w+)\b", 3);
        Add(scores, "C++", code, @"#include\s*<[^>]+>|\b(std::|cout\s*<<|cin\s*>>|template\s*<|namespace\s+std)\b", 4);
        Add(scores, "C", code, @"#include\s*<[^>]+>|\b(printf|scanf|malloc|free|sizeof)\s*\(|\bstruct\s+\w+", 2);
        Add(scores, "Kotlin", code, @"\b(fun\s+main|val\s+\w+|var\s+\w+|data\s+class|when\s*\()\b|println\(", 3);
        Add(scores, "Swift", code, @"\b(import\s+Foundation|func\s+\w+|let\s+\w+|var\s+\w+|guard\s+|protocol\s+)\b|print\(", 3);
        Add(scores, "Dart", code, @"\b(import\s+['""]dart:|void\s+main\s*\(|Widget\s+build|Future<|final\s+\w+)\b", 3);
        Add(scores, "Ruby", code, @"(?m)^\s*(def|class|module|require)\b|\b(puts|attr_reader|attr_accessor)\b", 3);
        Add(scores, "Lua", code, @"(?m)^\s*(local|function)\b|\b(then|elseif|end)\b|--", 2);
        Add(scores, "R", code, @"<-|\b(function|data\.frame|library|ggplot|c)\s*\(", 3);
        Add(scores, "Perl", code, @"(?m)^#!.*perl|\b(my|our|use\s+strict|use\s+warnings|sub)\b|\$[A-Za-z_]", 2);
        Add(scores, "Objective-C", code, @"@(interface|implementation|property|synthesize|autoreleasepool)|#import\s+[<""]", 4);
        Add(scores, "Scala", code, @"\b(object\s+\w+|def\s+main|val\s+\w+|case\s+class|extends\s+App)\b", 3);
        Add(scores, "Groovy", code, @"\b(def\s+\w+|println\s+|trait\s+|@Grab|Closure<)\b", 3);
        Add(scores, "CSS", code, @"[.#][A-Za-z_-][\w-]*\s*\{[^}]*:[^}]*;", 5);
        Add(scores, "YAML", code, @"(?m)^\s*[A-Za-z_][\w.-]*:\s*(?:$|[^{}\[])|^\s*-\s+[A-Za-z_]", 2);
        Add(scores, "Markdown", code, @"(?m)^(#{1,6}\s+|[-*+]\s+|```|>\s+)|\[[^\]]+\]\([^\)]+\)", 2);

        if (LooksLikeJson(code)) scores["JSON"] += 14;
        if (Regex.IsMatch(code, @"<\?xml\b", RegexOptions.IgnoreCase)) scores["XML"] += 14;
        if (Regex.IsMatch(code, @"<!DOCTYPE\s+html|<html\b|<(div|span|body|head|script|style|section|main|nav)\b", RegexOptions.IgnoreCase)) scores["HTML"] += 10;
        else if (Regex.IsMatch(code, @"<([A-Za-z_][\w:.-]*)(\s[^>]*)?>.*</\1>", RegexOptions.Singleline)) scores["XML"] += 6;

        var best = scores.OrderByDescending(x => x.Value).First();
        return best.Value <= 1 ? "Plain Text" : best.Key;
    }

    private static void Add(Dictionary<string, int> scores, string language, string code, string pattern, int weight, bool ignoreCase = false)
    {
        var options = RegexOptions.Multiline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        scores[language] += Regex.Matches(code, pattern, options).Count * weight;
    }

    private static bool LooksLikeJson(string code)
    {
        var text = code.Trim();
        if (!(text.StartsWith('{') && text.EndsWith('}')) && !(text.StartsWith('[') && text.EndsWith(']'))) return false;
        return Regex.IsMatch(text, "\"[^\"]+\"\\s*:") && !Regex.IsMatch(text, @"\b(class|function|SELECT|namespace)\b", RegexOptions.IgnoreCase);
    }
}
