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

// v0.7.0-alpha.1 formatter quality gate retained in alpha.2.
var python = CodeFormatterService.Format("def add(a,b):\n  x=1\n  if x==1:\n    return a+b  # keep comment\n  return 0", "Python", 4);
Check(python.Warning is null, "Python formatter warning-free");
Check(python.Code.Contains("def add(a, b):", StringComparison.Ordinal), "Python comma spacing");
Check(python.Code.Contains("    x = 1", StringComparison.Ordinal), "Python indentation + assignment spacing");
Check(python.Code.Contains("    if x == 1:", StringComparison.Ordinal), "Python comparison spacing");
Check(python.Code.Contains("        return a+b  # keep comment", StringComparison.Ordinal), "Python inline comment preservation");

var invalidJson = "{\n  // keep me\n  \"a\": 1,\n}";
var jsonSafety = CodeFormatterService.Format(invalidJson, "JSON", 4);
Check(jsonSafety.Warning is not null && jsonSafety.Code == invalidJson, "JSON non-standard syntax preserved instead of stripped");

var yamlBlock = "message: |\n  keep\n    literal indentation\nnext: value";
Check(CodeFormatterService.Format(yamlBlock, "YAML", 4).Code == yamlBlock, "YAML block scalar preserved");

var bashHeredoc = "cat <<EOF\n{literal}\nEOF";
Check(CodeFormatterService.Format(bashHeredoc, "Bash", 4).Code == bashHeredoc, "Bash heredoc preserved");

var rubyHeredoc = "text = <<TXT\n{literal}\nTXT";
Check(CodeFormatterService.Format(rubyHeredoc, "Ruby", 4).Code == rubyHeredoc, "Ruby heredoc preserved");

var luaLong = "local s = [[{literal}]]";
Check(CodeFormatterService.Format(luaLong, "Lua", 4).Code == luaLong, "Lua long string preserved");

var psHere = "@\"\n{literal}\n\"@";
Check(CodeFormatterService.Format(psHere, "PowerShell", 4).Code == psHere, "PowerShell here-string preserved");

var cppRaw = "auto s = R\"({literal})\";";
Check(CodeFormatterService.Format(cppRaw, "C++", 4).Code == cppRaw, "C++ raw string preserved");

var markdownExact = "# Title\n\n- item  ";
Check(CodeFormatterService.Format(markdownExact, "Markdown", 4).Code == markdownExact.Trim('\n', '\r'), "Markdown whitespace-sensitive preservation");


// Manual update-check version ordering gate. No network request is made by smoke tests.
Check(VersionComparisonService.IsNewer("0.7.0-alpha.3", "0.7.0-alpha.2"), "Version compare: alpha.3 > alpha.2");
Check(VersionComparisonService.IsNewer("0.7.0-beta.1", "0.7.0-alpha.9"), "Version compare: beta > alpha");
Check(VersionComparisonService.IsNewer("0.7.0-rc.1", "0.7.0-beta.9"), "Version compare: rc > beta");
Check(VersionComparisonService.IsNewer("0.7.0", "0.7.0-rc.9"), "Version compare: stable > rc");
Check(VersionComparisonService.Compare("v0.7.0-alpha.3", "0.7.0-alpha.3") == 0, "Version compare: optional v prefix");
Check(VersionComparisonService.IsNewer("0.7.0-beta.1", "0.7.0-alpha.3"), "Version compare: beta.1 > alpha.3");

// v0.7.0-alpha.2 batch/folder formatting gate.
Check(GlobMatcher.IsMatch("src/sample.py", "**/*.py"), "Batch glob recursive include");
Check(GlobMatcher.IsMatch("sample.py", "**/*.py"), "Batch glob root include");
Check(!GlobMatcher.IsMatch("src/sample.cs", "**/*.py"), "Batch glob extension exclusion");

var batchRoot = Path.Combine(Path.GetTempPath(), "codyformat-batch-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(batchRoot);
try
{
    Directory.CreateDirectory(Path.Combine(batchRoot, "src"));
    Directory.CreateDirectory(Path.Combine(batchRoot, "bin"));
    Directory.CreateDirectory(Path.Combine(batchRoot, "Generated"));

    var batchPython = Path.Combine(batchRoot, "src", "sample.py");
    var ignoredPython = Path.Combine(batchRoot, "bin", "ignored.py");
    var excludedPython = Path.Combine(batchRoot, "Generated", "generated.py");

    File.WriteAllText(batchPython, "def f(a,b):\n  x=1\n  return a+b\n", new UTF8Encoding(false));
    File.WriteAllText(ignoredPython, "def f(a,b):\n  return a+b\n", new UTF8Encoding(false));
    File.WriteAllText(excludedPython, "def f(a,b):\n  return a+b\n", new UTF8Encoding(false));

    var checkBatch = BatchFormatService.RunAsync(new CodyFormat.Models.BatchFormatRequest(
        batchRoot,
        Recursive: true,
        CheckOnly: true,
        Write: false,
        Backup: false,
        IndentSize: 4,
        IndentStyle: "space",
        IndentWasExplicit: false,
        UseEditorConfig: true,
        LanguageOverride: null,
        IncludePatterns: Array.Empty<string>(),
        ExcludePatterns: new[] { "**/Generated/**" })).GetAwaiter().GetResult();

    Check(checkBatch.Supported == 1, "Batch supported-file discovery + default/custom exclusions");
    Check(checkBatch.Changed == 1 && checkBatch.Written == 0, "Batch check is non-destructive");
    Check(File.ReadAllText(batchPython).Contains("def f(a,b):", StringComparison.Ordinal), "Batch check preserved original file");

    var writeBatch = BatchFormatService.RunAsync(new CodyFormat.Models.BatchFormatRequest(
        batchRoot,
        Recursive: true,
        CheckOnly: false,
        Write: true,
        Backup: true,
        IndentSize: 4,
        IndentStyle: "space",
        IndentWasExplicit: false,
        UseEditorConfig: true,
        LanguageOverride: null,
        IncludePatterns: new[] { "**/*.py" },
        ExcludePatterns: new[] { "**/Generated/**" })).GetAwaiter().GetResult();

    Check(writeBatch.Written == 1 && writeBatch.Failed == 0, "Batch write changes expected file");
    Check(File.ReadAllText(batchPython).Contains("def f(a, b):", StringComparison.Ordinal), "Batch write formatted Python");
    Check(File.Exists(batchPython + ".codyformat.bak"), "Batch backup created");

    var secondCheck = BatchFormatService.RunAsync(new CodyFormat.Models.BatchFormatRequest(
        batchRoot,
        Recursive: true,
        CheckOnly: true,
        Write: false,
        Backup: false,
        IndentSize: 4,
        IndentStyle: "space",
        IndentWasExplicit: false,
        UseEditorConfig: true,
        LanguageOverride: null,
        IncludePatterns: new[] { "**/*.py" },
        ExcludePatterns: new[] { "**/Generated/**" })).GetAwaiter().GetResult();

    Check(secondCheck.Changed == 0, "Batch second check is clean");
}
finally
{
    if (Directory.Exists(batchRoot))
        Directory.Delete(batchRoot, recursive: true);
}


// v0.7.0-alpha.3 .editorconfig + Profiles v2 gate.
var editorOuter = Path.Combine(Path.GetTempPath(), "codyformat-editorconfig-" + Guid.NewGuid().ToString("N"));
var editorRoot = Path.Combine(editorOuter, "project");
Directory.CreateDirectory(Path.Combine(editorRoot, "src", "nested"));
try
{
    File.WriteAllText(Path.Combine(editorOuter, ".editorconfig"), "[*.py]\nindent_size = 8\n");
    File.WriteAllText(Path.Combine(editorRoot, ".editorconfig"), "root = true\n\n[*.py]\nindent_style = space\nindent_size = 2\nend_of_line = lf\ninsert_final_newline = true\ntrim_trailing_whitespace = true\n");
    File.WriteAllText(Path.Combine(editorRoot, "src", ".editorconfig"), "[*.{py,pyw}]\nindent_size = 4\n");
    var editorFile = Path.Combine(editorRoot, "src", "nested", "sample.py");
    File.WriteAllText(editorFile, "def f():\n  x=1   \n", new UTF8Encoding(false));

    var resolved = EditorConfigService.Resolve(editorFile);
    Check(resolved.IndentStyle == "space", "EditorConfig indent_style resolved");
    Check(resolved.IndentSize == 4, "EditorConfig nested precedence + brace pattern");
    Check(resolved.EndOfLine == "lf", "EditorConfig inherited end_of_line");
    Check(resolved.InsertFinalNewline == true && resolved.TrimTrailingWhitespace == true, "EditorConfig whitespace policy");
    Check(resolved.SourceFiles.Count == 2, "EditorConfig source chain + root=true stops parent ancestry");

    var policyOutput = FormattingPolicyService.Apply("\tvalue   \n", resolved with { IndentSize = 4 }, 4, "\r\n", true);
    Check(policyOutput == "    value\n", "EditorConfig formatting policy application");
}
finally
{
    if (Directory.Exists(editorOuter)) Directory.Delete(editorOuter, true);
}

var legacyProfile = FormattingProfileService.Get("Documentation")!;
var migrated = FormattingProfileV2Service.FromLegacy(legacyProfile);
Check(migrated.SchemaVersion == 2 && migrated.Name == "Documentation", "Profiles v2 legacy migration");
Check(FormattingProfileV2Service.Validate(migrated, out _), "Profiles v2 validation");

using (var profileStream = new MemoryStream())
{
    FormattingProfileV2Service.WriteAsync(profileStream, migrated).GetAwaiter().GetResult();
    profileStream.Position = 0;
    Check(FormattingProfileV2Service.TryLoad(profileStream, out var loadedProfile, out _), "Profiles v2 export/import roundtrip");
    Check(loadedProfile?.Formatting.IndentSize == migrated.Formatting.IndentSize, "Profiles v2 roundtrip values");
}

using (var invalidProfile = new MemoryStream(Encoding.UTF8.GetBytes("{\"schemaVersion\":99,\"name\":\"Bad\"}")))
    Check(!FormattingProfileV2Service.TryLoad(invalidProfile, out _, out _), "Profiles v2 rejects unsupported schema");

var projectRoot = Path.Combine(Path.GetTempPath(), "codyformat-project-profile-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(Path.Combine(projectRoot, ".codyformat"));
Directory.CreateDirectory(Path.Combine(projectRoot, "src"));
try
{
    var projectProfile = FormattingProfileV2Service.FromLegacy(FormattingProfileService.Get("Minimal")!);
    FormattingProfileV2Service.Export(Path.Combine(projectRoot, ".codyformat", "profile.json"), projectProfile);
    var projectFile = Path.Combine(projectRoot, "src", "a.cs");
    File.WriteAllText(projectFile, "class A{}");
    Check(FormattingProfileV2Service.TryFindProjectProfile(projectFile, out var foundProjectProfile, out _, out _), "Project profile discovery");
    Check(foundProjectProfile?.Name == "Minimal", "Project profile value");
}
finally
{
    if (Directory.Exists(projectRoot)) Directory.Delete(projectRoot, true);
}


// v0.7.0-beta.1 native formatter provider gate. No external formatter is required.
Check(NativeFormatterService.Descriptors.Any(x => x.Id == "black" && x.Languages.Contains("Python")), "Native provider allowlist: Black/Python");
Check(NativeFormatterService.Descriptors.Any(x => x.Id == "prettier" && x.Languages.Contains("TypeScript")), "Native provider allowlist: Prettier/TypeScript");
Check(!NativeFormatterService.IsSupportedLanguage("C#"), "Native provider safety: C# does not invoke project-scoped dotnet format");

var savedPath = Environment.GetEnvironmentVariable("PATH");
var emptyNativePath = Path.Combine(Path.GetTempPath(), "codyformat-native-empty-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(emptyNativePath);
try
{
    Environment.SetEnvironmentVariable("PATH", emptyNativePath);
    var nativeFallback = NativeFormatterService.FormatAsync(
        "def f(a,b):\n  return a+b\n", "Python", 4, "space", useNative: true).GetAwaiter().GetResult();
    Check(!nativeFallback.UsedNative, "Native provider missing => built-in fallback");
    Check(nativeFallback.Error is null, "Native fallback remains successful");
    Check(nativeFallback.Notice?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true, "Native fallback reports provider absence");
    Check(nativeFallback.Code.Contains("def f(a, b):", StringComparison.Ordinal), "Native fallback formatted Python with built-in engine");
}
finally
{
    Environment.SetEnvironmentVariable("PATH", savedPath);
    Directory.Delete(emptyNativePath, true);
}

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
