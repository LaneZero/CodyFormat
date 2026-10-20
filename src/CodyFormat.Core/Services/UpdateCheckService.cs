using System.Net.Http.Headers;
using System.Text.Json;
using CodyFormat.Models;

namespace CodyFormat.Services;

/// <summary>
/// Manual, opt-in update checker for CodyFormat GitHub Releases.
/// No request is sent unless the user explicitly invokes Check for Updates.
/// </summary>
public static class UpdateCheckService
{
    private const string ReleasesApi = "https://api.github.com/repos/LaneZero/CodyFormat/releases?per_page=30";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public static async Task<UpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var current = AppInfo.Version;

        try
        {
            using var client = new HttpClient { Timeout = Timeout };
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AppInfo.ProductName, SanitizeProductVersion(current)));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await client.GetAsync(ReleasesApi, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Failure(current, $"GitHub returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var includePrereleases = VersionComparisonService.IsPrerelease(current);
            string? latestVersion = null;
            string? latestUrl = null;
            var latestPrerelease = false;

            foreach (var release in document.RootElement.EnumerateArray())
            {
                if (release.TryGetProperty("draft", out var draftElement) && draftElement.GetBoolean())
                    continue;

                var isPrerelease = release.TryGetProperty("prerelease", out var prereleaseElement) && prereleaseElement.GetBoolean();
                if (isPrerelease && !includePrereleases)
                    continue;

                if (!release.TryGetProperty("tag_name", out var tagElement))
                    continue;

                var tag = tagElement.GetString();
                if (string.IsNullOrWhiteSpace(tag))
                    continue;

                // Ignore malformed/non-semver release tags instead of failing the entire update check.
                try
                {
                    _ = VersionComparisonService.Compare(tag, current);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (latestVersion is null || VersionComparisonService.Compare(tag, latestVersion) > 0)
                {
                    latestVersion = tag.TrimStart('v', 'V');
                    latestPrerelease = isPrerelease;
                    latestUrl = release.TryGetProperty("html_url", out var urlElement)
                        ? urlElement.GetString()
                        : AppInfo.GitHubReleasesUrl;
                }
            }

            if (latestVersion is null)
            {
                return Failure(current, includePrereleases
                    ? "No compatible published GitHub Release was found."
                    : "No stable published GitHub Release was found.");
            }

            var available = VersionComparisonService.IsNewer(latestVersion, current);
            var comparison = VersionComparisonService.Compare(current, latestVersion);

            var message = available
                ? $"A newer CodyFormat version is available: v{latestVersion}."
                : comparison > 0
                    ? $"You are running v{current}, which is newer than the latest published release v{latestVersion}."
                    : $"CodyFormat v{current} is up to date.";

            return new UpdateCheckResult(
                true,
                available,
                current,
                latestVersion,
                latestUrl ?? AppInfo.GitHubReleasesUrl,
                latestPrerelease,
                message);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Failure(current, "The update check timed out.");
        }
        catch (HttpRequestException ex)
        {
            return Failure(current, $"Could not reach GitHub Releases: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Failure(current, $"GitHub returned an unexpected response: {ex.Message}");
        }
    }

    private static UpdateCheckResult Failure(string current, string message)
        => new(false, false, current, null, AppInfo.GitHubReleasesUrl, false, message);

    private static string SanitizeProductVersion(string value)
    {
        // HTTP ProductInfoHeaderValue version tokens are stricter than SemVer prerelease strings.
        return value.Replace('+', '.').Replace('-', '.');
    }
}
