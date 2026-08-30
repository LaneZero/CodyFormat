namespace CodyFormat.Models;

public enum TokenKind
{
    Plain,
    Keyword,
    ControlKeyword,
    String,
    Comment,
    Number,
    Type,
    Function,
    Property,
    Variable,
    Tag,
    Attribute,
    Operator,
    Constant
}
