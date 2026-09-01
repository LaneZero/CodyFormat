using CodyFormat.Models;

namespace CodyFormat.Services;

public static class ThemeService
{
    public static ThemeDefinition Get(string name) => name switch
    {
        "Light" => new ThemeDefinition(
            Name: "Light",
            Foreground: "#1F2328",
            Background: "#FFFFFF",
            Keyword: "#0000FF",
            ControlKeyword: "#AF00DB",
            String: "#A31515",
            Comment: "#008000",
            Number: "#098658",
            Type: "#267F99",
            Function: "#795E26",
            Property: "#001080",
            Variable: "#001080",
            Tag: "#800000",
            Attribute: "#FF0000",
            Operator: "#1F2328",
            Constant: "#0070C1",
            LineNumber: "#6E7781",
            Border: "#D0D7DE"),
        _ => new ThemeDefinition(
            Name: "Dark",
            Foreground: "#D4D4D4",
            Background: "#1E1E1E",
            Keyword: "#569CD6",
            ControlKeyword: "#C586C0",
            String: "#CE9178",
            Comment: "#6A9955",
            Number: "#B5CEA8",
            Type: "#4EC9B0",
            Function: "#DCDCAA",
            Property: "#9CDCFE",
            Variable: "#9CDCFE",
            Tag: "#569CD6",
            Attribute: "#9CDCFE",
            Operator: "#D4D4D4",
            Constant: "#4FC1FF",
            LineNumber: "#858585",
            Border: "#3E3E42")
    };
}
