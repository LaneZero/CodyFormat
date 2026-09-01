namespace CodyFormat.Services;

public static class LanguageCatalog
{
    public static readonly string[] Languages =
    [
        "Auto Detect",
        "C#", "SQL", "JavaScript", "TypeScript", "Python", "JSON", "XML", "HTML", "CSS",
        "PowerShell", "Bash", "Rust", "Go", "PHP", "Java", "C", "C++", "Kotlin", "Swift",
        "Dart", "Ruby", "Lua", "R", "Perl", "YAML", "Markdown", "Objective-C", "Scala", "Groovy",
        "Plain Text"
    ];

    public static bool IsBraceLanguage(string language) => language is
        "C#" or "JavaScript" or "TypeScript" or "CSS" or "Rust" or "Go" or "PHP" or "Java" or
        "C" or "C++" or "Kotlin" or "Swift" or "Dart" or "Perl" or "R" or "Objective-C" or
        "Scala" or "Groovy" or "PowerShell";
}
