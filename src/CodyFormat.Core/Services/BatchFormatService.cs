using System.Text;
using CodyFormat.Models;

namespace CodyFormat.Services;

public static class BatchFormatService
{
    private static readonly HashSet<string> DefaultExcludedDirectories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".git", "bin", "obj", "artifacts", "TestResults", "node_modules"
        };

    public static async Task<BatchFormatSummary> RunAsync(
        BatchFormatRequest request,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(request.RootPath);
        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"Directory not found: {root}");
        if (!request.Recursive)
            throw new ArgumentException("Directory formatting requires --recursive.");

        var results = new List<BatchFileResult>();
        var scanned = 0;
        var supported = 0;
        var changed = 0;
        var unchanged = 0;
        var failed = 0;
        var written = 0;

        foreach (var filePath in EnumerateFiles(root, request, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            scanned++;

            var language = ResolveLanguage(request.LanguageOverride, filePath);
            if (language is null)
                continue;

            supported++;
            var relative = NormalizeRelative(Path.GetRelativePath(root, filePath));

            try
            {
                var doc = await TextFileCodecService.ReadAsync(filePath, cancellationToken);
                var policy = new EditorConfigSettings
                {
                    IndentStyle = request.IndentStyle,
                    IndentSize = request.IndentSize
                };

                if (request.UseEditorConfig)
                    policy = EditorConfigSettings.Merge(policy, EditorConfigService.Resolve(filePath));

                if (request.IndentWasExplicit)
                    policy = policy with { IndentSize = request.IndentSize };

                var effectiveIndent = policy.EffectiveIndentSize(request.IndentSize);
                var formattedResult = CodeFormatterService.Format(doc.Text, language, effectiveIndent);

                if (!string.IsNullOrWhiteSpace(formattedResult.Warning))
                {
                    failed++;
                    results.Add(new BatchFileResult(relative, BatchFileStatus.Failed, formattedResult.Warning));
                    continue;
                }

                var formatted = FormattingPolicyService.Apply(
                    formattedResult.Code, policy, effectiveIndent, doc.NewLine, documentRules: true);
                var isChanged = !string.Equals(doc.Text, formatted, StringComparison.Ordinal);

                if (!isChanged)
                {
                    unchanged++;
                    results.Add(new BatchFileResult(relative, BatchFileStatus.Unchanged));
                    continue;
                }

                changed++;
                if (!request.Write)
                {
                    results.Add(new BatchFileResult(relative, BatchFileStatus.Changed));
                    continue;
                }

                var backup = await TextFileCodecService.WriteSafelyAsync(
                    filePath, formatted, doc, request.Backup, cancellationToken);

                written++;
                results.Add(new BatchFileResult(relative, BatchFileStatus.Written, BackupPath: backup));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (
                ex is IOException or UnauthorizedAccessException or DecoderFallbackException or ArgumentException)
            {
                failed++;
                results.Add(new BatchFileResult(relative, BatchFileStatus.Failed, ex.Message));
            }
        }

        return new BatchFormatSummary(scanned, supported, changed, unchanged, failed, written, results);
    }

    private static IEnumerable<string> EnumerateFiles(
        string root,
        BatchFormatRequest request,
        CancellationToken cancellationToken)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = pending.Pop();

            string[] entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(directory).ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (IOException)
            {
                continue;
            }

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                FileAttributes attributes;
                try
                {
                    attributes = File.GetAttributes(entry);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                    continue;

                if ((attributes & FileAttributes.Directory) != 0)
                {
                    var name = Path.GetFileName(entry);
                    if (DefaultExcludedDirectories.Contains(name))
                        continue;

                    var relativeDirectory = NormalizeRelative(Path.GetRelativePath(root, entry));
                    if (MatchesAny(relativeDirectory, request.ExcludePatterns) ||
                        MatchesAny(relativeDirectory + "/", request.ExcludePatterns))
                        continue;

                    pending.Push(entry);
                    continue;
                }

                var relative = NormalizeRelative(Path.GetRelativePath(root, entry));
                if (request.IncludePatterns.Count > 0 && !MatchesAny(relative, request.IncludePatterns))
                    continue;
                if (MatchesAny(relative, request.ExcludePatterns))
                    continue;

                yield return entry;
            }
        }
    }

    private static string? ResolveLanguage(string? requested, string filePath)
    {
        if (!string.IsNullOrWhiteSpace(requested) &&
            !requested.Equals("Auto Detect", StringComparison.OrdinalIgnoreCase))
        {
            var canonical = LanguageCatalog.Languages.FirstOrDefault(
                x => x.Equals(requested, StringComparison.OrdinalIgnoreCase) && x != "Auto Detect");
            if (canonical is null)
                throw new ArgumentException($"Unsupported language: {requested}");
            return canonical;
        }

        return SourceFileService.LanguageFromPath(filePath);
    }

    private static bool MatchesAny(string relativePath, IReadOnlyList<string> patterns)
        => patterns.Any(pattern => GlobMatcher.IsMatch(relativePath, pattern));

    private static string NormalizeRelative(string path) => path.Replace('\\', '/');
}
