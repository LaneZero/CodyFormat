using System.Globalization;

namespace CodyFormat.Services;

/// <summary>
/// Minimal SemVer-compatible comparison used for GitHub Release update checks.
/// Supports stable versions and dot-separated prerelease identifiers such as alpha.2, beta.1 and rc.1.
/// Build metadata is ignored for ordering.
/// </summary>
public static class VersionComparisonService
{
    public static int Compare(string left, string right)
    {
        if (!TryParse(left, out var a))
            throw new ArgumentException($"Invalid version: {left}", nameof(left));
        if (!TryParse(right, out var b))
            throw new ArgumentException($"Invalid version: {right}", nameof(right));

        var core = a.Major.CompareTo(b.Major);
        if (core != 0) return core;
        core = a.Minor.CompareTo(b.Minor);
        if (core != 0) return core;
        core = a.Patch.CompareTo(b.Patch);
        if (core != 0) return core;

        if (a.PreRelease.Count == 0 && b.PreRelease.Count == 0) return 0;
        if (a.PreRelease.Count == 0) return 1;   // Stable > prerelease.
        if (b.PreRelease.Count == 0) return -1;

        var count = Math.Max(a.PreRelease.Count, b.PreRelease.Count);
        for (var i = 0; i < count; i++)
        {
            if (i >= a.PreRelease.Count) return -1;
            if (i >= b.PreRelease.Count) return 1;

            var x = a.PreRelease[i];
            var y = b.PreRelease[i];
            var xNumeric = int.TryParse(x, NumberStyles.None, CultureInfo.InvariantCulture, out var xi);
            var yNumeric = int.TryParse(y, NumberStyles.None, CultureInfo.InvariantCulture, out var yi);

            if (xNumeric && yNumeric)
            {
                var numeric = xi.CompareTo(yi);
                if (numeric != 0) return numeric;
                continue;
            }

            if (xNumeric != yNumeric)
                return xNumeric ? -1 : 1; // SemVer: numeric identifiers have lower precedence.

            var lexical = string.CompareOrdinal(x, y);
            if (lexical != 0) return lexical;
        }

        return 0;
    }

    public static bool IsNewer(string candidate, string current)
        => Compare(candidate, current) > 0;

    public static bool IsPrerelease(string version)
        => TryParse(version, out var parsed) && parsed.PreRelease.Count > 0;

    private static bool TryParse(string value, out ParsedVersion parsed)
    {
        parsed = default!;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();
        if (text.StartsWith('v') || text.StartsWith('V'))
            text = text[1..];

        var plus = text.IndexOf('+');
        if (plus >= 0)
            text = text[..plus];

        string? preRelease = null;
        var dash = text.IndexOf('-');
        if (dash >= 0)
        {
            preRelease = text[(dash + 1)..];
            text = text[..dash];
            if (preRelease.Length == 0)
                return false;
        }

        var parts = text.Split('.');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var major) ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var minor) ||
            !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var patch))
            return false;

        var ids = preRelease is null
            ? Array.Empty<string>()
            : preRelease.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (preRelease is not null && ids.Length == 0)
            return false;

        parsed = new ParsedVersion(major, minor, patch, ids);
        return true;
    }

    private sealed record ParsedVersion(int Major, int Minor, int Patch, IReadOnlyList<string> PreRelease);
}
