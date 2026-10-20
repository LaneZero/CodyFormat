using System.Text.Json;
using CodyFormat.Models;

namespace CodyFormat.Services;

public static class FormattingProfileV2Service
{
    private const long MaxProfileBytes = 128 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static IReadOnlyList<FormattingProfileV2> GetAvailableProfiles()
    {
        var result = FormattingProfileService.Profiles
            .Select(FromLegacy)
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var profile in GetUserProfiles())
            result[profile.Name] = profile;

        return result.Values.OrderBy(x => x.Name.Equals("Custom", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static FormattingProfileV2? Resolve(string? nameOrPath)
    {
        if (string.IsNullOrWhiteSpace(nameOrPath))
            return null;

        if (File.Exists(nameOrPath))
            return TryLoad(nameOrPath, out var fromFile, out _) ? fromFile : null;

        return GetAvailableProfiles().FirstOrDefault(x => x.Name.Equals(nameOrPath, StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyList<FormattingProfileV2> GetUserProfiles()
    {
        var directory = GetProfilesDirectory();
        if (!Directory.Exists(directory))
            return Array.Empty<FormattingProfileV2>();

        var result = new List<FormattingProfileV2>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly))
        {
            if (TryLoad(path, out var profile, out _) && profile is not null)
                result.Add(profile);
        }
        return result;
    }

    public static bool TryLoad(string path, out FormattingProfileV2? profile, out string error)
    {
        profile = null;
        error = string.Empty;
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists)
            {
                error = "Profile file not found.";
                return false;
            }
            if (info.Length > MaxProfileBytes)
            {
                error = "Profile file is too large.";
                return false;
            }

            using var stream = File.OpenRead(path);
            return TryLoad(stream, out profile, out error);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool TryLoad(Stream stream, out FormattingProfileV2? profile, out string error)
    {
        profile = null;
        error = string.Empty;
        try
        {
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            if (buffer.Length > MaxProfileBytes)
            {
                error = "Profile file is too large.";
                return false;
            }

            var bytes = buffer.ToArray();
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                error = "Profile JSON root must be an object.";
                return false;
            }

            if (root.TryGetProperty("schemaVersion", out var versionElement) ||
                root.TryGetProperty("SchemaVersion", out versionElement))
            {
                if (versionElement.ValueKind != JsonValueKind.Number || !versionElement.TryGetInt32(out var version))
                {
                    error = "Invalid profile schemaVersion.";
                    return false;
                }
                if (version != 2)
                {
                    error = $"Unsupported profile schema version: {version}.";
                    return false;
                }

                profile = JsonSerializer.Deserialize<FormattingProfileV2>(bytes, JsonOptions);
            }
            else
            {
                // Legacy v1 profile shape used the original flat FormattingProfile fields.
                var legacy = JsonSerializer.Deserialize<FormattingProfile>(bytes, JsonOptions);
                if (legacy is null)
                {
                    error = "Profile content is invalid.";
                    return false;
                }
                profile = FromLegacy(legacy);
            }

            return Validate(profile, out error);
        }
        catch (JsonException ex)
        {
            error = $"Invalid profile JSON: {ex.Message}";
            profile = null;
            return false;
        }
        catch (IOException ex)
        {
            error = ex.Message;
            profile = null;
            return false;
        }
    }

    public static string Import(FormattingProfileV2 profile)
    {
        if (!Validate(profile, out var error))
            throw new ArgumentException(error, nameof(profile));

        var directory = GetProfilesDirectory();
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, SanitizeFileName(profile.Name) + ".codyprofile.json");
        Export(path, profile);
        return path;
    }

    public static void Export(string path, FormattingProfileV2 profile)
    {
        if (!Validate(profile, out var error))
            throw new ArgumentException(error, nameof(profile));

        var full = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(full);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(profile, JsonOptions);
        var temporary = full + ".tmp";
        File.WriteAllText(temporary, json);
        File.Move(temporary, full, overwrite: true);
    }

    public static async Task WriteAsync(Stream stream, FormattingProfileV2 profile, CancellationToken cancellationToken = default)
    {
        if (!Validate(profile, out var error))
            throw new ArgumentException(error, nameof(profile));

        if (stream.CanSeek)
            stream.SetLength(0);
        await JsonSerializer.SerializeAsync(stream, profile, JsonOptions, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public static bool TryFindProjectProfile(
        string filePath,
        out FormattingProfileV2? profile,
        out string? profilePath,
        out string? error)
    {
        profile = null;
        profilePath = null;
        error = null;

        var directory = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".");
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".codyformat", "profile.json");
            if (File.Exists(candidate))
            {
                profilePath = candidate;
                if (TryLoad(candidate, out profile, out var loadError))
                    return true;
                error = loadError;
                return false;
            }
            directory = directory.Parent;
        }

        return false;
    }

    public static FormattingProfileV2 FromLegacy(FormattingProfile legacy)
        => new()
        {
            SchemaVersion = 2,
            Name = legacy.Name,
            Description = legacy.Description,
            Formatting = new ProfileFormattingOptions
            {
                IndentStyle = "space",
                IndentSize = legacy.IndentSize
            },
            Appearance = new ProfileAppearanceOptions
            {
                Theme = legacy.Theme,
                Background = legacy.Background,
                NoBackground = legacy.NoBackground,
                LineNumbers = legacy.LineNumbers,
                Border = legacy.Border,
                FontFamily = legacy.FontFamily,
                FontSize = legacy.FontSize
            },
            Output = new ProfileOutputOptions
            {
                FormatBeforeOutput = legacy.FormatBeforeOutput,
                Target = legacy.Name switch
                {
                    "Word Document" => "Word",
                    "PowerPoint" or "Presentation" => "Presentation",
                    "Documentation" => "Documentation",
                    _ => "General"
                }
            }
        };

    public static FormattingProfile ToLegacy(FormattingProfileV2 profile)
        => new(
            profile.Name,
            profile.Appearance.Theme,
            profile.Appearance.Background,
            profile.Appearance.NoBackground,
            profile.Formatting.IndentSize,
            profile.Appearance.LineNumbers,
            profile.Appearance.Border,
            profile.Appearance.FontFamily,
            profile.Appearance.FontSize,
            profile.Output.FormatBeforeOutput,
            profile.Description);

    public static bool Validate(FormattingProfileV2? profile, out string error)
    {
        if (profile is null)
        {
            error = "Profile is empty.";
            return false;
        }
        if (profile.SchemaVersion != 2)
        {
            error = $"Unsupported profile schema version: {profile.SchemaVersion}.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(profile.Name) || profile.Name.Length > 80)
        {
            error = "Profile name must contain 1-80 characters.";
            return false;
        }
        var indentStyle = profile.Formatting.IndentStyle?.Trim().ToLowerInvariant();
        if (indentStyle is not ("space" or "tab"))
        {
            error = "Profile indentStyle must be 'space' or 'tab'.";
            return false;
        }
        profile.Formatting.IndentStyle = indentStyle;
        if (profile.Formatting.IndentSize is < 1 or > 16)
        {
            error = "Profile indentSize must be between 1 and 16.";
            return false;
        }
        if (profile.Appearance.FontSize is < 6 or > 72)
        {
            error = "Profile fontSize must be between 6 and 72.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(profile.Appearance.Theme) ||
            string.IsNullOrWhiteSpace(profile.Appearance.Background) ||
            string.IsNullOrWhiteSpace(profile.Appearance.FontFamily))
        {
            error = "Profile appearance fields must not be empty.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static string GetProfilesDirectory()
        => Path.Combine(AppSettingsService.GetSettingsDirectory(), "profiles");

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "profile" : cleaned;
    }
}
