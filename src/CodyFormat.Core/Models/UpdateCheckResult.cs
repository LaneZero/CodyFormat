namespace CodyFormat.Models;

public sealed record UpdateCheckResult(
    bool Succeeded,
    bool UpdateAvailable,
    string CurrentVersion,
    string? LatestVersion,
    string? ReleaseUrl,
    bool LatestIsPrerelease,
    string Message);
