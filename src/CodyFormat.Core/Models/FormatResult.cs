namespace CodyFormat.Models;

public sealed record FormatResult(string Code, bool Changed, string? Warning = null);
