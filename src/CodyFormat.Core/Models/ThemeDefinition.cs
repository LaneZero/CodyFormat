namespace CodyFormat.Models;

public sealed record ThemeDefinition(
    string Name,
    string Foreground,
    string Background,
    string Keyword,
    string ControlKeyword,
    string String,
    string Comment,
    string Number,
    string Type,
    string Function,
    string Property,
    string Variable,
    string Tag,
    string Attribute,
    string Operator,
    string Constant,
    string LineNumber,
    string Border);
