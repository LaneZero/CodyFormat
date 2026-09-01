using System.Text.Json;
using CodyFormat.Models;

namespace CodyFormat.Services;

/// <summary>
/// Persists only UI preferences in the operating system's per-user application-data directory.
/// Source code is never serialized by this service.
/// </summary>
public static class AppSettingsService
{
    private static readonly string SettingsDirectory = GetSettingsDirectory();
    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return new AppSettings();
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            // Corrupt or inaccessible preferences must never prevent CodyFormat from starting.
            return new AppSettings();
        }
    }

    public static bool TrySave(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            // Write to a temporary sibling and replace the settings file in one move. This reduces
            // the chance of leaving a half-written JSON file if the process is interrupted.
            var temporaryPath = SettingsPath + ".tmp";
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, SettingsPath, overwrite: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string GetSettingsDirectory()
    {
        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", "CodyFormat");
        }

        if (OperatingSystem.IsLinux())
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (!string.IsNullOrWhiteSpace(xdg) && Path.IsPathFullyQualified(xdg))
                return Path.Combine(xdg, "CodyFormat");

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".local", "share", "CodyFormat");
        }

        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(local))
            local = AppContext.BaseDirectory;
        return Path.Combine(local, "CodyFormat");
    }
}
