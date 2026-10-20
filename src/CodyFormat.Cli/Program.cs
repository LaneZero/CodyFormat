using System.Text;
using CodyFormat;
using CodyFormat.Models;
using CodyFormat.Services;

namespace CodyFormat.Cli;

internal static class Program
{
    private const int Success = 0;
    private const int ChangesRequired = 1;
    private const int UsageError = 2;
    private const int IoError = 3;
    private const int FormatError = 4;
    private const int Cancelled = 5;

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        var parse = CliOptions.Parse(args);
        if (!parse.Success)
        {
            Console.Error.WriteLine(parse.Error);
            Console.Error.WriteLine();
            PrintHelp();
            return UsageError;
        }

        var options = parse.Options!;
        if (options.ShowHelp) { PrintHelp(); return Success; }
        if (options.ShowVersion) { Console.WriteLine(AppInfo.Version); return Success; }

        try
        {
            var baseFormatting = ResolveBaseFormatting(options);
            if (!options.UseStdin && options.InputPath is not null && Directory.Exists(Path.GetFullPath(options.InputPath)))
                return await RunBatchAsync(options, baseFormatting);
            return await RunSingleAsync(options, baseFormatting);
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Operation cancelled.");
            return Cancelled;
        }
        catch (DecoderFallbackException ex)
        {
            Console.Error.WriteLine($"Text decoding failed: {ex.Message}");
            return IoError;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return UsageError;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"Access denied: {ex.Message}");
            return IoError;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"I/O error: {ex.Message}");
            return IoError;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return FormatError;
        }
    }

    private static async Task<int> RunSingleAsync(CliOptions options, BaseFormatting baseFormatting)
    {
        if (options.Recursive || options.IncludePatterns.Count > 0 || options.ExcludePatterns.Count > 0)
            throw new ArgumentException("--recursive, --include and --exclude are directory/batch options.");

        string source;
        TextFileDocument? metadata = null;
        string language;
        string? filePath = null;

        if (options.UseStdin)
        {
            source = await Console.In.ReadToEndAsync();
            language = ResolveLanguage(options.Language, null, source);
        }
        else
        {
            filePath = Path.GetFullPath(options.InputPath!);
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"File not found: {filePath}");
                return IoError;
            }
            metadata = await TextFileCodecService.ReadAsync(filePath);
            source = metadata.Text;
            language = ResolveLanguage(options.Language, filePath, source);
        }

        var policy = new EditorConfigSettings
        {
            IndentStyle = baseFormatting.IndentStyle,
            IndentSize = baseFormatting.IndentSize
        };
        if (filePath is not null && !options.NoEditorConfig)
            policy = EditorConfigSettings.Merge(policy, EditorConfigService.Resolve(filePath));
        if (options.IndentWasExplicit)
            policy = policy with { IndentSize = options.IndentSize };

        var effectiveIndent = policy.EffectiveIndentSize(baseFormatting.IndentSize);
        var result = CodeFormatterService.Format(source, language, effectiveIndent);
        if (!string.IsNullOrWhiteSpace(result.Warning))
        {
            Console.Error.WriteLine(result.Warning);
            return FormatError;
        }

        var formatted = FormattingPolicyService.Apply(
            result.Code,
            policy,
            effectiveIndent,
            metadata?.NewLine ?? "\n",
            documentRules: true);

        if (options.Check)
        {
            var changed = !string.Equals(source, formatted, StringComparison.Ordinal);
            if (!options.Quiet)
            {
                var configText = policy.SourceFiles.Count > 0 ? $" • editorconfig={policy.SourceFiles.Count}" : string.Empty;
                Console.Error.WriteLine(changed ? $"Would format ({language}){configText}" : $"Already formatted ({language}){configText}");
            }
            return changed ? ChangesRequired : Success;
        }

        if (options.Write)
        {
            if (filePath is null || metadata is null)
                throw new ArgumentException("--write requires a file.");

            string? backup = null;
            if (!string.Equals(source, formatted, StringComparison.Ordinal))
                backup = await TextFileCodecService.WriteSafelyAsync(filePath, formatted, metadata, options.Backup);

            if (!options.Quiet)
            {
                Console.Error.WriteLine($"Formatted {filePath} ({language})");
                if (backup is not null) Console.Error.WriteLine($"Backup: {backup}");
            }
            return Success;
        }

        Console.Write(formatted);
        if (formatted.Length > 0 && !formatted.EndsWith('\n') && !Console.IsOutputRedirected)
            Console.WriteLine();
        return Success;
    }

    private static async Task<int> RunBatchAsync(CliOptions options, BaseFormatting baseFormatting)
    {
        if (!options.Recursive)
            throw new ArgumentException("Directory formatting requires --recursive.");

        var root = Path.GetFullPath(options.InputPath!);
        using var cts = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, eventArgs) => { eventArgs.Cancel = true; cts.Cancel(); };
        Console.CancelKeyPress += handler;
        try
        {
            var request = new BatchFormatRequest(
                root,
                Recursive: true,
                CheckOnly: options.Check,
                Write: options.Write,
                Backup: options.Backup,
                IndentSize: baseFormatting.IndentSize,
                IndentStyle: baseFormatting.IndentStyle,
                IndentWasExplicit: options.IndentWasExplicit,
                UseEditorConfig: !options.NoEditorConfig,
                LanguageOverride: options.Language,
                IncludePatterns: options.IncludePatterns,
                ExcludePatterns: options.ExcludePatterns);

            var summary = await BatchFormatService.RunAsync(request, cts.Token);
            if (!options.Quiet)
            {
                foreach (var file in summary.Files)
                {
                    if (file.Status is BatchFileStatus.Changed or BatchFileStatus.Written or BatchFileStatus.Failed)
                    {
                        var suffix = file.Message is null ? string.Empty : $": {file.Message}";
                        Console.Error.WriteLine($"{file.Status,-9} {file.RelativePath}{suffix}");
                    }
                }
                Console.Error.WriteLine();
                Console.Error.WriteLine($"Scanned:   {summary.Scanned}");
                Console.Error.WriteLine($"Supported: {summary.Supported}");
                Console.Error.WriteLine($"Changed:   {summary.Changed}");
                Console.Error.WriteLine($"Unchanged: {summary.Unchanged}");
                Console.Error.WriteLine($"Failed:    {summary.Failed}");
                Console.Error.WriteLine($"Written:   {summary.Written}");
            }

            if (summary.Failed > 0) return FormatError;
            if (options.Check && summary.Changed > 0) return ChangesRequired;
            return Success;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }

    private static BaseFormatting ResolveBaseFormatting(CliOptions options)
    {
        var indent = options.IndentSize;
        var style = "space";
        if (!string.IsNullOrWhiteSpace(options.ProfileName))
        {
            var profile = FormattingProfileV2Service.Resolve(options.ProfileName);
            if (profile is null)
                throw new ArgumentException($"Unknown or invalid profile: {options.ProfileName}");
            style = profile.Formatting.IndentStyle;
            if (!options.IndentWasExplicit)
                indent = profile.Formatting.IndentSize;
        }
        return new BaseFormatting(indent, style);
    }

    private static string ResolveLanguage(string? requested, string? path, string source)
    {
        if (!string.IsNullOrWhiteSpace(requested) && !requested.Equals("Auto Detect", StringComparison.OrdinalIgnoreCase))
        {
            var canonical = LanguageCatalog.Languages.FirstOrDefault(x => x.Equals(requested, StringComparison.OrdinalIgnoreCase));
            if (canonical is null || canonical == "Auto Detect")
                throw new ArgumentException($"Unsupported language: {requested}");
            return canonical;
        }
        if (!string.IsNullOrWhiteSpace(path))
        {
            var fromPath = SourceFileService.LanguageFromPath(path);
            if (!string.IsNullOrWhiteSpace(fromPath)) return fromPath;
        }
        return LanguageDetector.Detect(source);
    }

    private static void PrintHelp()
    {
        Console.WriteLine($"CodyFormat CLI {AppInfo.Version}");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  codyformat <file> [options]");
        Console.WriteLine("  codyformat <directory> --recursive [options]");
        Console.WriteLine("  codyformat --stdin [options]");
        Console.WriteLine();
        Console.WriteLine("Formatting options:");
        Console.WriteLine("  --language <name>   Override language detection");
        Console.WriteLine("  --indent <1-16>     Explicit indentation size (overrides profile/.editorconfig size)");
        Console.WriteLine("  --profile <value>   Built-in/user profile name or a profile JSON path");
        Console.WriteLine("  --no-editorconfig   Ignore .editorconfig for this operation");
        Console.WriteLine("  --check             Do not write; exit 1 if changes are required");
        Console.WriteLine("  --write             Safely replace changed input files");
        Console.WriteLine("  --backup            Create .codyformat.bak files when writing");
        Console.WriteLine();
        Console.WriteLine("Batch options:");
        Console.WriteLine("  --recursive         Recursively scan a directory");
        Console.WriteLine("  --include <glob>    Include matching relative paths; repeatable");
        Console.WriteLine("  --exclude <glob>    Exclude matching relative paths; repeatable");
        Console.WriteLine();
        Console.WriteLine("Other:");
        Console.WriteLine("  --stdin             Read source from standard input");
        Console.WriteLine("  --quiet             Suppress status and batch summaries");
        Console.WriteLine("  --version           Print version");
        Console.WriteLine("  --help, -h          Show this help");
        Console.WriteLine();
        Console.WriteLine(".editorconfig is enabled by default for local files and directories.");
        Console.WriteLine("Explicit --indent overrides the resolved .editorconfig indent_size.");
        Console.WriteLine("Batch mode is a dry run unless --write is explicitly supplied.");
        Console.WriteLine("Exit codes: 0=success, 1=changes required, 2=usage, 3=I/O, 4=format failure, 5=cancelled");
    }

    private sealed record BaseFormatting(int IndentSize, string IndentStyle);

    private sealed class CliOptions
    {
        public string? InputPath { get; private set; }
        public bool UseStdin { get; private set; }
        public string? Language { get; private set; }
        public int IndentSize { get; private set; } = 4;
        public bool IndentWasExplicit { get; private set; }
        public string? ProfileName { get; private set; }
        public bool NoEditorConfig { get; private set; }
        public bool Check { get; private set; }
        public bool Write { get; private set; }
        public bool Backup { get; private set; }
        public bool Recursive { get; private set; }
        public List<string> IncludePatterns { get; } = [];
        public List<string> ExcludePatterns { get; } = [];
        public bool Quiet { get; private set; }
        public bool ShowHelp { get; private set; }
        public bool ShowVersion { get; private set; }

        public static ParseResult Parse(string[] args)
        {
            var o = new CliOptions();
            var positional = new List<string>();
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--help": case "-h": o.ShowHelp = true; break;
                    case "--version": o.ShowVersion = true; break;
                    case "--stdin": o.UseStdin = true; break;
                    case "--check": o.Check = true; break;
                    case "--write": o.Write = true; break;
                    case "--backup": o.Backup = true; break;
                    case "--recursive": o.Recursive = true; break;
                    case "--quiet": o.Quiet = true; break;
                    case "--no-editorconfig": o.NoEditorConfig = true; break;
                    case "--language":
                        if (!TryValue(args, ref i, out var language)) return ParseResult.Fail("--language requires a value.");
                        o.Language = language; break;
                    case "--indent":
                        if (!TryValue(args, ref i, out var indentText) || !int.TryParse(indentText, out var indent) || indent is < 1 or > 16)
                            return ParseResult.Fail("--indent must be an integer from 1 to 16.");
                        o.IndentSize = indent; o.IndentWasExplicit = true; break;
                    case "--profile":
                        if (!TryValue(args, ref i, out var profile)) return ParseResult.Fail("--profile requires a name or path.");
                        o.ProfileName = profile; break;
                    case "--include":
                        if (!TryValue(args, ref i, out var include)) return ParseResult.Fail("--include requires a glob pattern.");
                        o.IncludePatterns.Add(include); break;
                    case "--exclude":
                        if (!TryValue(args, ref i, out var exclude)) return ParseResult.Fail("--exclude requires a glob pattern.");
                        o.ExcludePatterns.Add(exclude); break;
                    default:
                        if (args[i].StartsWith('-')) return ParseResult.Fail($"Unknown option: {args[i]}");
                        positional.Add(args[i]); break;
                }
            }

            if (o.ShowHelp || o.ShowVersion) return ParseResult.Ok(o);
            if (o.Check && o.Write) return ParseResult.Fail("--check and --write cannot be used together.");
            if (o.Backup && !o.Write) return ParseResult.Fail("--backup requires --write.");
            if (positional.Count > 1) return ParseResult.Fail("Provide only one input file or directory.");
            if (o.UseStdin && positional.Count > 0) return ParseResult.Fail("Use either a file/directory or --stdin, not both.");
            if (!o.UseStdin && positional.Count == 0) return ParseResult.Fail("Provide an input file/directory or use --stdin.");
            if (o.UseStdin && (o.Write || o.Backup || o.Recursive || o.IncludePatterns.Count > 0 || o.ExcludePatterns.Count > 0))
                return ParseResult.Fail("--stdin cannot be combined with write or batch options.");
            if (positional.Count == 1) o.InputPath = positional[0];
            return ParseResult.Ok(o);
        }

        private static bool TryValue(string[] args, ref int index, out string value)
        {
            if (index + 1 >= args.Length) { value = string.Empty; return false; }
            value = args[++index];
            return value.Length > 0;
        }
    }

    private sealed record ParseResult(bool Success, CliOptions? Options, string Error)
    {
        public static ParseResult Ok(CliOptions options) => new(true, options, string.Empty);
        public static ParseResult Fail(string error) => new(false, null, error);
    }
}
