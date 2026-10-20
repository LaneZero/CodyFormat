namespace CodyFormat.Models;

public sealed record BatchFormatRequest(
    string RootPath,
    bool Recursive,
    bool CheckOnly,
    bool Write,
    bool Backup,
    int IndentSize,
    string IndentStyle,
    bool IndentWasExplicit,
    bool UseEditorConfig,
    string? LanguageOverride,
    IReadOnlyList<string> IncludePatterns,
    IReadOnlyList<string> ExcludePatterns);

public enum BatchFileStatus
{
    Changed,
    Unchanged,
    Written,
    Failed
}

public sealed record BatchFileResult(
    string RelativePath,
    BatchFileStatus Status,
    string? Message = null,
    string? BackupPath = null);

public sealed record BatchFormatSummary(
    int Scanned,
    int Supported,
    int Changed,
    int Unchanged,
    int Failed,
    int Written,
    IReadOnlyList<BatchFileResult> Files);
