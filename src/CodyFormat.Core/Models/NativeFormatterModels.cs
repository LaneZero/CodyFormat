namespace CodyFormat.Models;

public sealed record NativeFormatterDescriptor(
    string Id,
    string DisplayName,
    string ExecutableName,
    IReadOnlyList<string> Languages);

public sealed record NativeFormatterStatus(
    string Id,
    string DisplayName,
    string ExecutableName,
    bool Available,
    string? ExecutablePath,
    IReadOnlyList<string> Languages);

public sealed record FormatExecutionResult(
    string Code,
    bool Changed,
    bool UsedNative,
    string? ProviderId = null,
    string? Notice = null,
    string? Error = null);
