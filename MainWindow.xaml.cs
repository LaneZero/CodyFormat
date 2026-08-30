using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CodyFormat.Models;
using CodyFormat.Services;
using Microsoft.Win32;

namespace CodyFormat;

public partial class MainWindow : Window
{
    private readonly SyntaxHighlighterService _highlighter = new();
    private bool _initializing = true;

    private static readonly Dictionary<string, string> Backgrounds = new()
    {
        ["Theme Default"] = string.Empty,
        ["VS Code Dark"] = "#1E1E1E",
        ["Midnight"] = "#0F172A",
        ["GitHub Dark"] = "#0D1117",
        ["Soft Gray"] = "#F6F8FA",
        ["White"] = "#FFFFFF",
        ["Warm Paper"] = "#FFF9ED"
    };

    public MainWindow()
    {
        InitializeComponent();

        Title = $"{AppInfo.ProductName} v{AppInfo.Version}";
        LanguageBox.ItemsSource = LanguageCatalog.Languages;
        LanguageBox.SelectedItem = "Auto Detect";
        ThemeBox.ItemsSource = new[] { "Dark", "Light" };
        ThemeBox.SelectedItem = "Dark";
        BackgroundBox.ItemsSource = Backgrounds.Keys;
        BackgroundBox.SelectedItem = "Theme Default";
        IndentBox.ItemsSource = new[] { 2, 4 };
        IndentBox.SelectedItem = 4;

        InputBox.Text = "SELECT c.CustomerId,c.FullName,COUNT(o.OrderId) AS OrderCount,SUM(o.TotalAmount) AS Revenue FROM Customers c LEFT JOIN Orders o ON o.CustomerId=c.CustomerId WHERE c.IsActive=1 AND o.CreatedAt>=DATEADD(day,-30,GETDATE()) GROUP BY c.CustomerId,c.FullName ORDER BY Revenue DESC;";

        _initializing = false;
        ApplyThemeAndRefresh();
    }

    private int IndentSize => IndentBox.SelectedItem is int value ? value : 4;

    private void InputChanged(object sender, TextChangedEventArgs e)
    {
        if (!_initializing) RefreshPreview();
    }

    private void OptionsChanged(object sender, RoutedEventArgs e)
    {
        if (!_initializing) RefreshPreview();
    }

    private void OptionsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initializing) RefreshPreview();
    }


    private void NoBackgroundChanged(object sender, RoutedEventArgs e)
    {
        if (BackgroundBox is not null) BackgroundBox.IsEnabled = NoBackgroundBox.IsChecked != true;
        if (!_initializing) RefreshPreview();
    }

    private void ThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing) return;
        ApplyThemeAndRefresh();
    }

    private void ApplyThemeAndRefresh()
    {
        var themeName = ThemeBox.SelectedItem?.ToString() ?? "Dark";
        UiThemeService.Apply(themeName);
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        if (PreviewBox is null || LanguageBox.SelectedItem is null || ThemeBox.SelectedItem is null) return;

        var language = ResolveLanguage();
        var theme = ThemeService.Get(ThemeBox.SelectedItem.ToString()!);
        var background = ResolveBackground(theme);
        var previewCode = GetOutputCode(language, out var formatWarning);

        DetectedLanguageText.Text = LanguageBox.SelectedItem?.ToString() == "Auto Detect"
            ? $"Detected: {language}"
            : language;

        InputMetaText.Text = $"{CountLines(InputBox.Text)} lines  •  {InputBox.Text.Length:N0} chars";
        var backgroundName = NoBackgroundBox.IsChecked == true ? "None" : BackgroundBox.SelectedItem?.ToString() ?? "Theme Default";
        PreviewModeText.Text = FormatOnOutputBox.IsChecked == true
            ? $"{theme.Name}  •  {backgroundName}  •  {IndentSize} spaces  •  formatted"
            : $"{theme.Name}  •  {backgroundName}";

        StatusText.Text = formatWarning ?? $"Ready  •  {language}  •  {CountLines(previewCode)} preview lines";

        PreviewBox.Document.Blocks.Clear();
        PreviewBox.Foreground = Brush(theme.Foreground);
        PreviewBox.Document.PagePadding = new Thickness(0);
        CodePreviewBorder.Background = background == "None" ? Brushes.Transparent : Brush(background);
        CodePreviewBorder.BorderBrush = Brush(theme.Border);
        CodePreviewBorder.BorderThickness = BorderBox.IsChecked == true ? new Thickness(1) : new Thickness(0);

        var lines = ClipboardExportService.NormalizeLines(previewCode);
        var digits = Math.Max(2, lines.Length.ToString().Length);

        for (var i = 0; i < lines.Length; i++)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0), Padding = new Thickness(0), LineHeight = 21 };

            if (LineNumbersBox.IsChecked == true)
            {
                paragraph.Inlines.Add(new Run((i + 1).ToString().PadLeft(digits) + "  ")
                {
                    Foreground = Brush(theme.LineNumber)
                });
            }

            foreach (var token in _highlighter.HighlightLine(lines[i], language))
            {
                paragraph.Inlines.Add(new Run(token.Text.Replace("\t", new string(' ', IndentSize)))
                {
                    Foreground = Brush(ClipboardExportService.ColorFor(token.Kind, theme, language))
                });
            }

            if (paragraph.Inlines.Count == 0) paragraph.Inlines.Add(new Run(" "));
            PreviewBox.Document.Blocks.Add(paragraph);
        }
    }

    private string ResolveLanguage()
    {
        var selected = LanguageBox.SelectedItem?.ToString() ?? "Auto Detect";
        return selected == "Auto Detect" ? LanguageDetector.Detect(InputBox.Text) : selected;
    }

    private string ResolveBackground(ThemeDefinition theme)
    {
        if (NoBackgroundBox.IsChecked == true) return "None";
        var selected = BackgroundBox.SelectedItem?.ToString() ?? "Theme Default";
        var value = Backgrounds.GetValueOrDefault(selected, string.Empty);
        return string.IsNullOrEmpty(value) ? theme.Background : value;
    }

    private string GetOutputCode(string language, out string? warning)
    {
        warning = null;
        if (FormatOnOutputBox.IsChecked != true) return InputBox.Text;

        var result = CodeFormatterService.Format(InputBox.Text, language, IndentSize);
        warning = result.Warning;
        return result.Code;
    }

    private void FormatCodeClick(object sender, RoutedEventArgs e) => FormatCurrentCode();

    private void FormatCurrentCode()
    {
        var language = ResolveLanguage();
        if (language is "Plain Text" or "Markdown")
        {
            SetStatus($"No structural formatter is required for {language}");
            return;
        }

        var result = CodeFormatterService.Format(InputBox.Text, language, IndentSize);
        if (result.Warning is not null)
        {
            SetStatus(result.Warning);
            MessageBox.Show(result.Warning, "CodyFormat Formatter", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!result.Changed)
        {
            SetStatus($"✓ {language} code is already formatted");
            return;
        }

        var caret = InputBox.CaretIndex;
        InputBox.Text = result.Code;
        InputBox.CaretIndex = Math.Min(caret, InputBox.Text.Length);
        InputBox.Focus();
        SetStatus($"✓ Formatted {language}  •  {IndentSize}-space indentation");
    }

    private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var shiftAltF = e.Key == Key.F && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
        var ctrlEnter = e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        if (!shiftAltF && !ctrlEnter) return;

        e.Handled = true;
        FormatCurrentCode();
    }

    private void CopyWordClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var language = ResolveLanguage();
            var theme = ThemeService.Get(ThemeBox.SelectedItem?.ToString() ?? "Dark");
            var code = GetOutputCode(language, out var warning);

            ClipboardExportService.CopyRich(
                code, language, theme, ResolveBackground(theme),
                LineNumbersBox.IsChecked == true, BorderBox.IsChecked == true,
                IndentSize, _highlighter);

            SetStatus(warning ?? $"✓ Copied for Word  •  {language}  •  {BackgroundBox.SelectedItem}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Clipboard operation failed.\n\n{ex.Message}", "CodyFormat", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CopyPlainClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var language = ResolveLanguage();
            var code = GetOutputCode(language, out var warning);
            ClipboardExportService.CopyPlain(code);
            SetStatus(warning ?? "✓ Plain text copied");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Clipboard operation failed.\n\n{ex.Message}", "CodyFormat", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportDocxClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export formatted code",
            Filter = "Word document (*.docx)|*.docx",
            DefaultExt = ".docx",
            AddExtension = true,
            FileName = "formatted-code.docx"
        };

        if (dialog.ShowDialog(this) != true) return;

        try
        {
            var language = ResolveLanguage();
            var theme = ThemeService.Get(ThemeBox.SelectedItem?.ToString() ?? "Dark");
            var code = GetOutputCode(language, out var warning);

            DocxExportService.Export(
                dialog.FileName, code, language, theme, ResolveBackground(theme),
                LineNumbersBox.IsChecked == true, BorderBox.IsChecked == true,
                IndentSize, _highlighter);

            SetStatus(warning ?? $"✓ Exported: {Path.GetFileName(dialog.FileName)}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"DOCX export failed.\n\n{ex.Message}", "CodyFormat", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GitHubClick(object sender, RoutedEventArgs e) => OpenUrl(AppInfo.GitHubUrl);

    private void DonateClick(object sender, RoutedEventArgs e)
    {
        var window = new DonateWindow { Owner = this };
        window.ShowDialog();
    }

    private void HelpClick(object sender, RoutedEventArgs e)
    {
        var window = new HelpWindow { Owner = this };
        window.ShowDialog();
    }


    private void CutEditorClick(object sender, RoutedEventArgs e) => InputBox.Cut();
    private void CopyEditorClick(object sender, RoutedEventArgs e) => InputBox.Copy();
    private void PasteEditorClick(object sender, RoutedEventArgs e) => InputBox.Paste();
    private void SelectAllEditorClick(object sender, RoutedEventArgs e) { InputBox.SelectAll(); InputBox.Focus(); }

    private void ClearClick(object sender, RoutedEventArgs e)
    {
        InputBox.Clear();
        InputBox.Focus();
        SetStatus("Editor cleared");
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open the web page.\n\n{url}\n\n{ex.Message}", "CodyFormat", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private static int CountLines(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 1;
        var count = 1;
        foreach (var ch in text) if (ch == '\n') count++;
        return count;
    }

    private static SolidColorBrush Brush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }
}
