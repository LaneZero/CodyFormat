using System.Text;
using System.IO.Compression;
using CodyFormat.Services;

var failures = new List<string>();
void Check(bool condition, string name)
{
    if (!condition) failures.Add(name);
}

Check(LanguageDetector.Detect("package main\nfunc main() { println() }") == "Go", "Go language detection");
Check(SourceFileService.LanguageFromPath("main.rs") == "Rust", "Rust extension mapping");
Check(TelegramExportService.BuildBlock("SELECT 1;", "SQL") == "```sql\nSELECT 1;\n```", "Telegram SQL fence");
Check(TelegramExportService.GetLanguageTag("C#") == "csharp", "Telegram C# tag");

var utf16Payload = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("SELECT 1;")).ToArray();
Check(SourceFileService.TryDecode(utf16Payload, "query.sql", out var utf16Text, out var utf16Language, out _)
      && utf16Text == "SELECT 1;" && utf16Language == "SQL", "UTF-16 source decoding");
Check(!SourceFileService.TryDecode(new byte[] { 0x00, 0x01, 0x02, 0x03 }, "payload.bin", out _, out _, out _), "Binary source rejection");
Check(AppSettingsService.GetSettingsDirectory().EndsWith("CodyFormat", StringComparison.OrdinalIgnoreCase), "Cross-platform settings path");

var json = CodeFormatterService.Format("{\"a\":1,\"b\":[2,3]}", "JSON", 4);
Check(json.Changed && json.Code.Contains("\n    \"a\"", StringComparison.Ordinal), "JSON 4-space formatting");

var sql = CodeFormatterService.Format("select a,b from users where active=1 order by a;", "SQL", 4);
Check(sql.Code.Contains("SELECT", StringComparison.Ordinal) && sql.Code.Contains("\nFROM", StringComparison.Ordinal), "SQL clause formatting");

var theme = ThemeService.Get("Dark");
var highlighter = new SyntaxHighlighterService();
var html = RichCodeExportService.BuildHtmlFragment("SELECT 1;", "SQL", theme, "None", false, false, 4, "Consolas", 12, highlighter);
Check(!html.Contains("background:", StringComparison.OrdinalIgnoreCase), "None background HTML");
Check(html.Contains(theme.Keyword, StringComparison.OrdinalIgnoreCase), "Syntax color in HTML");

using var docx = new MemoryStream();
DocxExportService.Export(docx, "SELECT 1;", "SQL", theme, "None", false, false, 4, "Consolas", 12, highlighter);
docx.Position = 0;
using (var archive = new ZipArchive(docx, ZipArchiveMode.Read, leaveOpen: true))
    Check(archive.GetEntry("word/document.xml") is not null, "DOCX document.xml");

if (failures.Count > 0)
{
    Console.Error.WriteLine("CodyFormat smoke tests FAILED:");
    foreach (var failure in failures) Console.Error.WriteLine($" - {failure}");
    return 1;
}

Console.WriteLine("CodyFormat smoke tests PASS");
return 0;
