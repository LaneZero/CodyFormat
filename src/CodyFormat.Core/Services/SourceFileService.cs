using System.Text;

namespace CodyFormat.Services;

/// <summary>
/// Safely decodes local source files and derives a language hint from their name.
/// The service is UI-framework agnostic so Windows, macOS and Linux use the same rules.
/// </summary>
public static class SourceFileService
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> ExtensionLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        [".cs"] = "C#", [".sql"] = "SQL", [".js"] = "JavaScript", [".mjs"] = "JavaScript", [".cjs"] = "JavaScript",
        [".ts"] = "TypeScript", [".tsx"] = "TypeScript", [".py"] = "Python", [".json"] = "JSON", [".xml"] = "XML",
        [".html"] = "HTML", [".htm"] = "HTML", [".css"] = "CSS", [".ps1"] = "PowerShell", [".psm1"] = "PowerShell",
        [".sh"] = "Bash", [".bash"] = "Bash", [".zsh"] = "Bash", [".rs"] = "Rust", [".go"] = "Go", [".php"] = "PHP", [".phtml"] = "PHP",
        [".java"] = "Java", [".c"] = "C", [".h"] = "C", [".cpp"] = "C++", [".cc"] = "C++", [".cxx"] = "C++",
        [".hpp"] = "C++", [".kt"] = "Kotlin", [".kts"] = "Kotlin", [".swift"] = "Swift", [".dart"] = "Dart",
        [".rb"] = "Ruby", [".lua"] = "Lua", [".r"] = "R", [".pl"] = "Perl", [".pm"] = "Perl", [".yaml"] = "YAML",
        [".yml"] = "YAML", [".md"] = "Markdown", [".markdown"] = "Markdown", [".m"] = "Objective-C", [".mm"] = "Objective-C",
        [".scala"] = "Scala", [".sc"] = "Scala", [".groovy"] = "Groovy", [".gradle"] = "Groovy"
    };

    public static bool TryDecode(ReadOnlySpan<byte> bytes, string fileName, out string text, out string languageHint, out string error)
    {
        text = string.Empty;
        languageHint = "Plain Text";
        error = string.Empty;

        if (bytes.Length > MaxFileSizeBytes)
        {
            error = $"The file is larger than {MaxFileSizeBytes / 1024 / 1024} MB. CodyFormat limits editor imports to keep the application responsive.";
            return false;
        }

        try
        {
            // Recognize Unicode BOMs before binary detection because UTF-16/UTF-32 text legitimately
            // contains NUL bytes. Unmarked text is required to be strict UTF-8.
            if (StartsWith(bytes, 0xEF, 0xBB, 0xBF))
            {
                text = new UTF8Encoding(false, true).GetString(bytes[3..]);
            }
            else if (StartsWith(bytes, 0xFF, 0xFE, 0x00, 0x00))
            {
                text = new UTF32Encoding(false, false, true).GetString(bytes[4..]);
            }
            else if (StartsWith(bytes, 0x00, 0x00, 0xFE, 0xFF))
            {
                text = new UTF32Encoding(true, false, true).GetString(bytes[4..]);
            }
            else if (StartsWith(bytes, 0xFF, 0xFE))
            {
                text = new UnicodeEncoding(false, false, true).GetString(bytes[2..]);
            }
            else if (StartsWith(bytes, 0xFE, 0xFF))
            {
                text = new UnicodeEncoding(true, false, true).GetString(bytes[2..]);
            }
            else
            {
                if (LooksBinary(bytes))
                {
                    error = "The selected file appears to be binary and was not opened.";
                    return false;
                }

                text = new UTF8Encoding(false, true).GetString(bytes);
            }

            languageHint = LanguageFromPath(fileName) ?? LanguageDetector.Detect(text);
            return true;
        }
        catch (DecoderFallbackException)
        {
            error = "The selected file is not valid UTF-8/UTF-16/UTF-32 text.";
            return false;
        }
    }

    public static async Task<(bool Success, string Text, string LanguageHint, string Error)> ReadAsync(
        Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var buffer = new MemoryStream();
        var chunk = new byte[64 * 1024];
        long total = 0;

        while (true)
        {
            var read = await stream.ReadAsync(chunk.AsMemory(0, chunk.Length), cancellationToken);
            if (read == 0) break;
            total += read;
            if (total > MaxFileSizeBytes)
                return (false, string.Empty, "Plain Text", $"The file is larger than {MaxFileSizeBytes / 1024 / 1024} MB.");
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }

        var bytes = buffer.ToArray();
        return TryDecode(bytes, fileName, out var text, out var hint, out var error)
            ? (true, text, hint, string.Empty)
            : (false, string.Empty, "Plain Text", error);
    }

    public static string? LanguageFromPath(string path)
    {
        var fileName = Path.GetFileName(path);
        if (fileName.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals(".bashrc", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals(".zshrc", StringComparison.OrdinalIgnoreCase))
            return "Bash";

        return ExtensionLanguages.GetValueOrDefault(Path.GetExtension(path));
    }

    private static bool LooksBinary(ReadOnlySpan<byte> bytes)
    {
        var count = Math.Min(bytes.Length, 4096);
        for (var i = 0; i < count; i++)
            if (bytes[i] == 0) return true;
        return false;
    }

    private static bool StartsWith(ReadOnlySpan<byte> bytes, params byte[] prefix)
        => bytes.Length >= prefix.Length && bytes[..prefix.Length].SequenceEqual(prefix);
}
