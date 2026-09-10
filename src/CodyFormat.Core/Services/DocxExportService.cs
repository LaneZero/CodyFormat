using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using CodyFormat.Models;

namespace CodyFormat.Services;

public static class DocxExportService
{
    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    public static void Export(
        string path,
        string code,
        string language,
        ThemeDefinition theme,
        string background,
        bool lineNumbers,
        bool border,
        int indentSize,
        string fontFamily,
        double fontSize,
        SyntaxHighlighterService highlighter)
    {
        using var file = File.Create(path);
        Export(file, code, language, theme, background, lineNumbers, border, indentSize, fontFamily, fontSize, highlighter);
    }

    public static void Export(
        Stream output,
        string code,
        string language,
        ThemeDefinition theme,
        string background,
        bool lineNumbers,
        bool border,
        int indentSize,
        string fontFamily,
        double fontSize,
        SyntaxHighlighterService highlighter)
    {
        ArgumentNullException.ThrowIfNull(output);
        using var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);
        WriteEntry(archive, "[Content_Types].xml", ContentTypes());
        WriteEntry(archive, "_rels/.rels", RootRelationships());
        WriteEntry(archive, "word/document.xml", BuildDocument(
            code, language, theme, background, lineNumbers, border,
            indentSize, fontFamily, fontSize, highlighter));
    }

    private static string BuildDocument(
        string code,
        string language,
        ThemeDefinition theme,
        string background,
        bool lineNumbers,
        bool border,
        int indentSize,
        string fontFamily,
        double fontSize,
        SyntaxHighlighterService highlighter)
    {
        var body = new XElement(W + "body");
        var lines = RichCodeExportService.NormalizeLines(code);
        var digits = Math.Max(2, lines.Length.ToString().Length);
        var fill = background == "None" ? null : background.TrimStart('#');

        for (var i = 0; i < lines.Length; i++)
        {
            var pPr = new XElement(W + "pPr",
                new XElement(W + "spacing",
                    new XAttribute(W + "before", "0"),
                    new XAttribute(W + "after", "0"),
                    new XAttribute(W + "line", "300"),
                    new XAttribute(W + "lineRule", "auto")));

            if (!string.IsNullOrWhiteSpace(fill))
                pPr.Add(new XElement(W + "shd",
                    new XAttribute(W + "val", "clear"),
                    new XAttribute(W + "color", "auto"),
                    new XAttribute(W + "fill", fill)));

            if (border)
            {
                var pBdr = new XElement(W + "pBdr");
                if (i == 0) pBdr.Add(BorderElement("top", theme.Border));
                if (i == lines.Length - 1) pBdr.Add(BorderElement("bottom", theme.Border));
                pBdr.Add(BorderElement("left", theme.Border));
                pBdr.Add(BorderElement("right", theme.Border));
                pPr.Add(pBdr);
            }

            var paragraph = new XElement(W + "p", pPr);

            if (lineNumbers)
            {
                var number = (i + 1).ToString().PadLeft(digits) + "  ";
                paragraph.Add(CreateRun(number, theme.LineNumber, fontFamily, fontSize));
            }

            foreach (var token in highlighter.HighlightLine(lines[i], language))
            {
                paragraph.Add(CreateRun(
                    token.Text.Replace("\t", new string(' ', Math.Clamp(indentSize, 2, 8))),
                    RichCodeExportService.ColorFor(token.Kind, theme, language),
                    fontFamily,
                    fontSize));
            }

            if (paragraph.Elements(W + "r").Any() == false)
                paragraph.Add(CreateRun(" ", theme.Foreground, fontFamily, fontSize));

            body.Add(paragraph);
        }

        body.Add(new XElement(W + "sectPr",
            new XElement(W + "pgSz", new XAttribute(W + "w", "11906"), new XAttribute(W + "h", "16838")),
            new XElement(W + "pgMar",
                new XAttribute(W + "top", "900"),
                new XAttribute(W + "right", "900"),
                new XAttribute(W + "bottom", "900"),
                new XAttribute(W + "left", "900"),
                new XAttribute(W + "header", "720"),
                new XAttribute(W + "footer", "720"),
                new XAttribute(W + "gutter", "0"))));

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(W + "document",
                new XAttribute(XNamespace.Xmlns + "w", W),
                new XAttribute(XNamespace.Xmlns + "r", R),
                body));
        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static XElement CreateRun(string text, string hexColor, string fontFamily, double fontSize)
    {
        var color = hexColor.TrimStart('#');
        var halfPoints = Math.Clamp((int)Math.Round(fontSize * 2), 16, 64).ToString();

        return new XElement(W + "r",
            new XElement(W + "rPr",
                new XElement(W + "rFonts",
                    new XAttribute(W + "ascii", fontFamily),
                    new XAttribute(W + "hAnsi", fontFamily),
                    new XAttribute(W + "cs", fontFamily)),
                new XElement(W + "color", new XAttribute(W + "val", color)),
                new XElement(W + "sz", new XAttribute(W + "val", halfPoints)),
                new XElement(W + "szCs", new XAttribute(W + "val", halfPoints))),
            new XElement(W + "t", new XAttribute(XNamespace.Xml + "space", "preserve"), text));
    }

    private static XElement BorderElement(string side, string hexColor) =>
        new(W + side,
            new XAttribute(W + "val", "single"),
            new XAttribute(W + "sz", "4"),
            new XAttribute(W + "space", "0"),
            new XAttribute(W + "color", hexColor.TrimStart('#')));

    private static string ContentTypes() => """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
          <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
          <Default Extension="xml" ContentType="application/xml"/>
          <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
        </Types>
        """;

    private static string RootRelationships() => """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
        </Relationships>
        """;

    private static void WriteEntry(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new System.Text.UTF8Encoding(false));
        writer.Write(content);
    }
}
