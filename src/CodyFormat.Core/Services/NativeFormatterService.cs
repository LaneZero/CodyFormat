using System.Diagnostics;
using System.Text;
using CodyFormat.Models;

namespace CodyFormat.Services;

/// <summary>
/// Executes a small, fixed allowlist of native formatters when the user explicitly opts in.
/// No shell is used, no command string comes from profiles, and only absolute executables
/// resolved from absolute PATH entries are launched.
/// </summary>
public static class NativeFormatterService
{
    private static readonly NativeProvider[] Providers =
    [
        new("black", "Black", "black", ["Python"]),
        new("prettier", "Prettier", "prettier", ["JavaScript", "TypeScript", "JSON", "CSS", "HTML", "Markdown", "YAML"]),
        new("clang-format", "clang-format", "clang-format", ["C", "C++", "Objective-C"]),
        new("rustfmt", "rustfmt", "rustfmt", ["Rust"]),
        new("gofmt", "gofmt", "gofmt", ["Go"])
    ];

    public static IReadOnlyList<NativeFormatterDescriptor> Descriptors => Providers
        .Select(p => new NativeFormatterDescriptor(p.Id, p.DisplayName, p.ExecutableName, p.Languages))
        .ToArray();

    public static bool IsSupportedLanguage(string language)
        => Providers.Any(p => p.Languages.Contains(language, StringComparer.OrdinalIgnoreCase));

    public static IReadOnlyList<NativeFormatterStatus> GetStatuses()
        => Providers.Select(p =>
        {
            var path = ResolveExecutable(p.ExecutableName);
            return new NativeFormatterStatus(
                p.Id,
                p.DisplayName,
                p.ExecutableName,
                path is not null,
                path,
                p.Languages);
        }).ToArray();

    public static async Task<FormatExecutionResult> FormatAsync(
        string source,
        string language,
        int indentSize,
        string indentStyle,
        bool useNative,
        CancellationToken cancellationToken = default)
    {
        if (!useNative)
            return BuiltIn(source, language, indentSize);

        var provider = Providers.FirstOrDefault(p =>
            p.Languages.Contains(language, StringComparer.OrdinalIgnoreCase));

        if (provider is null)
        {
            var fallback = BuiltIn(source, language, indentSize);
            return fallback with
            {
                Notice = $"No native formatter is configured for {language}; built-in formatter used."
            };
        }

        // Black intentionally owns Python's canonical four-space indentation. If the
        // active policy asks for something else, use CodyFormat's built-in engine so the
        // resolved profile/.editorconfig policy remains authoritative.
        if (provider.Id == "black" &&
            (!indentStyle.Equals("space", StringComparison.OrdinalIgnoreCase) || indentSize != 4))
        {
            var fallback = BuiltIn(source, language, indentSize);
            return fallback with
            {
                Notice = "Black requires its canonical four-space style; built-in formatter used to honor the active indentation policy."
            };
        }

        var executablePath = ResolveExecutable(provider.ExecutableName);
        if (executablePath is null)
        {
            var fallback = BuiltIn(source, language, indentSize);
            return fallback with
            {
                Notice = $"Native formatter '{provider.DisplayName}' was requested but was not found in safe PATH entries; built-in formatter used."
            };
        }

        var native = await ExecuteAsync(
            provider,
            executablePath,
            source,
            language,
            Math.Clamp(indentSize, 1, 16),
            indentStyle,
            cancellationToken);

        if (native.Error is null)
            return native;

        var builtIn = BuiltIn(source, language, indentSize);
        if (builtIn.Error is not null)
            return builtIn with { Notice = native.Error };

        return builtIn with
        {
            Notice = $"{native.Error} Built-in formatter used instead."
        };
    }

    private static FormatExecutionResult BuiltIn(string source, string language, int indentSize)
    {
        var result = CodeFormatterService.Format(source, language, indentSize);
        return new FormatExecutionResult(
            result.Code,
            !string.Equals(source, result.Code, StringComparison.Ordinal),
            false,
            Error: result.Warning);
    }

    private static async Task<FormatExecutionResult> ExecuteAsync(
        NativeProvider provider,
        string executablePath,
        string source,
        string language,
        int indentSize,
        string indentStyle,
        CancellationToken cancellationToken)
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "CodyFormat", "native", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(12));

        var psi = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = tempRoot,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false)
        };

        foreach (var arg in BuildArguments(provider.Id, language, indentSize, indentStyle))
            psi.ArgumentList.Add(arg);

        Process? process = null;
        try
        {
            process = new Process { StartInfo = psi };
            if (!process.Start())
                return Failure(source, provider, "could not be started");

            var stdoutTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(timeout.Token);

            await process.StandardInput.WriteAsync(source.AsMemory(), timeout.Token);
            await process.StandardInput.FlushAsync(timeout.Token);
            process.StandardInput.Close();

            await process.WaitForExitAsync(timeout.Token);
            var stdout = await stdoutTask;
            var stderr = (await stderrTask).Trim();

            if (process.ExitCode != 0)
            {
                var detail = CompactError(stderr);
                return Failure(source, provider,
                    $"exited with code {process.ExitCode}{(detail.Length == 0 ? string.Empty : $": {detail}")}");
            }

            if (source.Length > 0 && stdout.Length == 0)
                return Failure(source, provider, "returned empty output");

            return new FormatExecutionResult(
                stdout,
                !string.Equals(source, stdout, StringComparison.Ordinal),
                true,
                provider.Id);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            return Failure(source, provider, "timed out after 12 seconds");
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            TryKill(process);
            return Failure(source, provider, $"failed to execute: {ex.Message}");
        }
        finally
        {
            process?.Dispose();
            try { Directory.Delete(tempRoot, recursive: true); } catch { }
        }
    }

    private static FormatExecutionResult Failure(string source, NativeProvider provider, string reason)
        => new(source, false, false, Error: $"Native formatter '{provider.DisplayName}' {reason}.");

    private static IReadOnlyList<string> BuildArguments(
        string providerId,
        string language,
        int indentSize,
        string indentStyle)
    {
        var useTabs = indentStyle.Equals("tab", StringComparison.OrdinalIgnoreCase);
        return providerId switch
        {
            "black" => ["--quiet", "-"],
            "prettier" =>
            [
                "--parser", PrettierParser(language),
                "--tab-width", indentSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                useTabs ? "--use-tabs" : "--no-use-tabs"
            ],
            "clang-format" =>
            [
                "--assume-filename=" + ClangAssumeFile(language),
                "--style={BasedOnStyle: LLVM, IndentWidth: " + indentSize + ", UseTab: " + (useTabs ? "Always" : "Never") + "}"
            ],
            "rustfmt" =>
            [
                "--emit", "stdout",
                "--config", $"tab_spaces={indentSize},hard_tabs={(useTabs ? "true" : "false")}"
            ],
            "gofmt" => [],
            _ => []
        };
    }

    private static string PrettierParser(string language) => language switch
    {
        "JavaScript" => "babel",
        "TypeScript" => "typescript",
        "JSON" => "json",
        "CSS" => "css",
        "HTML" => "html",
        "Markdown" => "markdown",
        "YAML" => "yaml",
        _ => throw new ArgumentException($"No Prettier parser configured for {language}.")
    };

    private static string ClangAssumeFile(string language) => language switch
    {
        "C" => "input.c",
        "Objective-C" => "input.m",
        _ => "input.cpp"
    };

    private static string? ResolveExecutable(string executableName)
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue)) return null;

        var names = CandidateNames(executableName);
        foreach (var raw in pathValue.Split(Path.PathSeparator, StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string directory;
            try
            {
                if (!Path.IsPathFullyQualified(raw)) continue;
                directory = Path.GetFullPath(raw);
            }
            catch
            {
                continue;
            }

            foreach (var name in names)
            {
                var candidate = Path.Combine(directory, name);
                try
                {
                    if (File.Exists(candidate)) return Path.GetFullPath(candidate);
                }
                catch
                {
                    // Ignore malformed/inaccessible PATH entries and keep searching.
                }
            }
        }

        return null;
    }

    private static IReadOnlyList<string> CandidateNames(string executableName)
    {
        if (!OperatingSystem.IsWindows()) return [executableName];

        // Intentionally do not run .cmd/.bat shims because that would require a shell.
        // Standalone .exe tools and .NET tool .exe shims are supported.
        return [executableName + ".exe", executableName];
    }

    private static string CompactError(string stderr)
    {
        if (string.IsNullOrWhiteSpace(stderr)) return string.Empty;
        var oneLine = string.Join(" ", stderr
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return oneLine.Length <= 240 ? oneLine : oneLine[..240] + "…";
    }

    private static void TryKill(Process? process)
    {
        if (process is null) return;
        try
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        catch { }
    }

    private sealed record NativeProvider(
        string Id,
        string DisplayName,
        string ExecutableName,
        IReadOnlyList<string> Languages);
}
