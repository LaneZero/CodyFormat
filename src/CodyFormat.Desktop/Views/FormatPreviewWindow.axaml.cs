using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace CodyFormat.Desktop.Views;

public partial class FormatPreviewWindow : Window
{
    public FormatPreviewWindow()
    {
        InitializeComponent();
    }

    public FormatPreviewWindow(string original, string formatted, string language, bool selectionOnly, string fontFamily, double fontSize)
        : this()
    {
        TitleText.Text = selectionOnly ? $"Review {language} selection" : $"Review {language} document";
        OriginalBox.Text = original;
        FormattedBox.Text = formatted;
        var font = SafeFont(fontFamily);
        OriginalBox.FontFamily = font;
        FormattedBox.FontFamily = font;
        OriginalBox.FontSize = fontSize;
        FormattedBox.FontSize = fontSize;
    }

    private void ApplyClick(object? sender, RoutedEventArgs e) => Close(true);
    private void CancelClick(object? sender, RoutedEventArgs e) => Close(false);

    private static FontFamily SafeFont(string name)
    {
        try { return new FontFamily(name); }
        catch { return FontFamily.Default; }
    }
}
