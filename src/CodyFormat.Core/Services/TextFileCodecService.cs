using System.Text;
using CodyFormat.Models;

namespace CodyFormat.Services;

public static class TextFileCodecService
{
    public static async Task<TextFileDocument> ReadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var info = new FileInfo(path);
        if (!info.Exists)
            throw new FileNotFoundException("File not found.", path);

        if (info.Length > SourceFileService.MaxFileSizeBytes)
            throw new IOException($"File is larger than {SourceFileService.MaxFileSizeBytes / 1024 / 1024} MB: {path}");

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        Encoding encoding;
        int preambleLength;

        if (StartsWith(bytes, 0xEF, 0xBB, 0xBF))
        {
            encoding = new UTF8Encoding(false, true);
            preambleLength = 3;
        }
        else if (StartsWith(bytes, 0xFF, 0xFE, 0x00, 0x00))
        {
            encoding = new UTF32Encoding(false, false, true);
            preambleLength = 4;
        }
        else if (StartsWith(bytes, 0x00, 0x00, 0xFE, 0xFF))
        {
            encoding = new UTF32Encoding(true, false, true);
            preambleLength = 4;
        }
        else if (StartsWith(bytes, 0xFF, 0xFE))
        {
            encoding = new UnicodeEncoding(false, false, true);
            preambleLength = 2;
        }
        else if (StartsWith(bytes, 0xFE, 0xFF))
        {
            encoding = new UnicodeEncoding(true, false, true);
            preambleLength = 2;
        }
        else
        {
            encoding = new UTF8Encoding(false, true);
            preambleLength = 0;
        }

        var text = encoding.GetString(bytes, preambleLength, bytes.Length - preambleLength);
        return new TextFileDocument(text, encoding, preambleLength > 0, DetectNewLine(text));
    }

    public static async Task<string?> WriteSafelyAsync(
        string path,
        string text,
        TextFileDocument metadata,
        bool createBackup = false,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path) ?? ".";
        var fileName = Path.GetFileName(path);
        var temp = Path.Combine(directory, $".{fileName}.codyformat.{Guid.NewGuid():N}.tmp");
        string? backupPath = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var body = metadata.Encoding.GetBytes(text);
            var preamble = metadata.HasBom ? metadata.Encoding.GetPreamble() : Array.Empty<byte>();
            var payload = new byte[preamble.Length + body.Length];
            preamble.CopyTo(payload, 0);
            body.CopyTo(payload, preamble.Length);

            await File.WriteAllBytesAsync(temp, payload, cancellationToken);

            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    var mode = File.GetUnixFileMode(path);
                    File.SetUnixFileMode(temp, mode);
                }
                catch (PlatformNotSupportedException) { }
                catch (UnauthorizedAccessException) { }
            }

            if (createBackup)
            {
                backupPath = GetAvailableBackupPath(path);
                File.Copy(path, backupPath, overwrite: false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temp, path, overwrite: true);
            return backupPath;
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
    }

    public static string RestoreNewLines(string text, string newline)
    {
        var normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

        return newline == "\n"
            ? normalized
            : normalized.Replace("\n", newline, StringComparison.Ordinal);
    }

    private static string GetAvailableBackupPath(string path)
    {
        var first = path + ".codyformat.bak";
        if (!File.Exists(first))
            return first;

        for (var i = 1; i < 10_000; i++)
        {
            var candidate = path + $".codyformat.bak.{i}";
            if (!File.Exists(candidate))
                return candidate;
        }

        throw new IOException($"Could not allocate a backup filename for: {path}");
    }

    private static string DetectNewLine(string text)
        => text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n"
            : text.Contains('\r') ? "\r"
            : "\n";

    private static bool StartsWith(ReadOnlySpan<byte> bytes, params byte[] prefix)
        => bytes.Length >= prefix.Length && bytes[..prefix.Length].SequenceEqual(prefix);
}
