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

// Every selectable language must have a formatter path that returns valid text without
// throwing. These are intentionally small representative snippets, not parser conformance tests.
var formatterSamples = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["C#"] = "class A{void M(){var x=1;if(x==1){x+=1;}}}",
    ["SQL"] = "select a,b from users where active=1 order by a;",
    ["JavaScript"] = "function f(){const x=1;if(x===1){return x;}}",
    ["TypeScript"] = "interface A{value:number} function f(a:A){const x=1;return a.value;}",
    ["Python"] = "def f(x):\n  if x:\n    return x\n  return 0",
    ["JSON"] = "{\"a\":1,\"b\":[2,3]}",
    ["XML"] = "<root><item id=\"1\">x</item></root>",
    ["HTML"] = "<div><span>x</span><script>function f(){return 1;}</script></div>",
    ["CSS"] = ".a{color:red;margin:0;}",
    ["PowerShell"] = "function Test-X {$x=1;if($x -eq 1){Write-Output $x}}",
    ["Bash"] = "if true; then\necho ok\nfi",
    ["Rust"] = "fn main(){let x=1;if x==1{println!(\"x\");}}",
    ["Go"] = "package main\nfunc main(){x:=1;if x==1{println(x)}}",
    ["PHP"] = "<?php function f(){$x=1;if($x==1){echo $x;}}",
    ["Java"] = "class A{void f(){int x=1;if(x==1){x+=1;}}}",
    ["C"] = "int main(){int x=1;if(x==1){x+=1;}return x;}",
    ["C++"] = "int main(){int x=1;if(x==1){x+=1;}return x;}",
    ["Kotlin"] = "fun main(){var x=1;if(x==1){x+=1}}",
    ["Swift"] = "func f(){var x=1;if x==1{x+=1}}",
    ["Dart"] = "void main(){var x=1;if(x==1){x+=1;}}",
    ["Ruby"] = "def f\nif true\nputs 'x'\nend\nend",
    ["Lua"] = "local function f()\nif true then\nprint('x')\nend\nend",
    ["R"] = "f<-function(x){if(x==1){x=2};x}",
    ["Perl"] = "sub f{my $x=1;if($x==1){return $x;}}",
    ["YAML"] = "root:\n  child:value\n  list:\n    -item",
    ["Markdown"] = "# Title\n\n- item",
    ["Objective-C"] = "@implementation A\n- (void)f { int x=1; if(x==1){x+=1;} }\n@end",
    ["Scala"] = "object A{def f(x:Int)={if(x==1){x}else{0}}}",
    ["Groovy"] = "class A{def f(){def x=1;if(x==1){return x}}}",
    ["Plain Text"] = "plain  text\n  keep indentation"
};

foreach (var language in LanguageCatalog.Languages.Where(x => x != "Auto Detect"))
{
    Check(formatterSamples.TryGetValue(language, out var sample), $"Formatter sample exists: {language}");
    if (sample is null) continue;
    var formatted = CodeFormatterService.Format(sample, language, 4);
    Check(formatted.Warning is null, $"Formatter warning-free: {language}");
    Check(!string.IsNullOrWhiteSpace(formatted.Code), $"Formatter output non-empty: {language}");
}

var braceSpacing = CodeFormatterService.Format("class A{void M(){var x=1;if(x==1){x+=1;}}}", "C#", 4).Code;
Check(braceSpacing.Contains("x = 1", StringComparison.Ordinal) && braceSpacing.Contains("x == 1", StringComparison.Ordinal), "Brace-language operator spacing");
var css = CodeFormatterService.Format(".a{color:red;margin:0;}", "CSS", 4).Code;
Check(css.Contains("color: red", StringComparison.Ordinal), "CSS declaration spacing");
var yaml = CodeFormatterService.Format("root:\n  child:value", "YAML", 4).Code;
Check(yaml.Contains("child: value", StringComparison.Ordinal), "YAML key spacing");
var rustLifetime = CodeFormatterService.Format("fn borrow<'a>(x: &'a str){let y=x;}", "Rust", 4);
Check(rustLifetime.Warning is null && rustLifetime.Code.Contains("'a", StringComparison.Ordinal), "Rust lifetime preservation");
var templateLiteral = "function f(){const x=`{ keep }`;return x;}";
var jsTemplate = CodeFormatterService.Format(templateLiteral, "JavaScript", 4);
Check(jsTemplate.Warning is null && jsTemplate.Code.Contains("`{ keep }`", StringComparison.Ordinal), "JavaScript template-literal safety");

var theme = ThemeService.Get("Dark");
var highlighter = new SyntaxHighlighterService();
var html = RichCodeExportService.BuildHtmlFragment("SELECT 1;", "SQL", theme, "None", false, false, 4, "Consolas", 12, highlighter);
Check(!html.Contains("background:", StringComparison.OrdinalIgnoreCase), "None background HTML");
Check(html.Contains(theme.Keyword, StringComparison.OrdinalIgnoreCase), "Syntax color in HTML");
var htmlDocument = RichCodeExportService.BuildHtmlDocument(html);
Check(htmlDocument.Contains("<!--StartFragment-->", StringComparison.Ordinal) && htmlDocument.Contains("charset=utf-8", StringComparison.OrdinalIgnoreCase), "Linux/macOS HTML clipboard document");

var rtf = RichCodeExportService.BuildRtf("SELECT 1;", "SQL", theme, "None", false, false, 4, "Consolas", 12, highlighter);
Check(rtf.StartsWith(@"{\rtf1", StringComparison.Ordinal), "RTF clipboard header");
Check(!rtf.Contains(@"\highlight", StringComparison.Ordinal), "None background RTF");
var rtfWithBackground = RichCodeExportService.BuildRtf("SELECT 1;", "SQL", theme, theme.Background, false, true, 4, "Consolas", 12, highlighter);
Check(rtfWithBackground.Contains(@"\highlight", StringComparison.Ordinal), "RTF background");
Check(rtfWithBackground.Contains(@"\box", StringComparison.Ordinal), "RTF border");

var cfHtml = RichCodeExportService.BuildCfHtml(html);
Check(cfHtml.StartsWith("Version:1.0", StringComparison.Ordinal) && cfHtml.Contains("StartFragment:", StringComparison.Ordinal), "Windows CF_HTML envelope");

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
