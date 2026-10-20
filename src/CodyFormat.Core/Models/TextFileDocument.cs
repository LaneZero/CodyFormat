using System.Text;

namespace CodyFormat.Models;

public sealed record TextFileDocument(
    string Text,
    Encoding Encoding,
    bool HasBom,
    string NewLine);
