using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CodyFormat.Desktop.Services;
using CodyFormat.Models;
using CodyFormat.Services;

namespace CodyFormat.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly SyntaxHighlighterService _highlighter = new();
    private bool _initializing = true;
    private bool _applyingProfile;
    private bool _suspendRefresh;
    private string? _currentFileName;
    private string? _currentFilePath;
    private string? _currentFileLanguageHint;
    private EditorConfigSettings _currentEditorConfig = new();
    private FormattingProfileV2? _projectProfile;
    private string _activeProfileIndentStyle = "space";

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

    private static readonly string[] CodeFonts =
    [
        "Cascadia Mono", "Cascadia Code", "Consolas", "JetBrains Mono", "Fira Code",
        "Menlo", "Monaco", "SF Mono", "DejaVu Sans Mono", "Liberation Mono", "Courier New"
    ];

    public MainWindow()
    {
        InitializeComponent();
        Title = $"{AppInfo.ProductName} v{AppInfo.Version}";
        PlatformBadgeText.Text = $"● Local Processing  •  {PlatformInfo.Name}";
        InitializeControls();
        LoadSettings();
        InputBox.Text = string.Empty;
        CurrentFileText.Text = "Untitled";
        _initializing = false;
        BackgroundBox.IsEnabled = NoBackgroundBox.IsChecked != true;
        ApplyEditorTypography();
        ApplyThemeAndRefresh();
        SetStatus($"Ready  •  {PlatformInfo.Name} {PlatformInfo.Architecture}  •  Drop a source file here or press Ctrl/Cmd+O");
    }


    private int IndentSize => IndentBox.SelectedItem is int value ? value : 4;
    private double CurrentFontSize => FontSizeBox.SelectedItem is double value ? value : 14;
    private string CurrentFontFamily => FontBox.SelectedItem?.ToString() ?? CodeFonts[0];

    private void InitializeControls()
    {
        LanguageBox.ItemsSource = LanguageCatalog.Languages;
        ThemeBox.ItemsSource = new[] { "Dark", "Light" };
        BackgroundBox.ItemsSource = Backgrounds.Keys.ToArray();
        IndentBox.ItemsSource = Enumerable.Range(1, 16).ToArray();
        RefreshProfileList();
        FontBox.ItemsSource = CodeFonts;
        FontSizeBox.ItemsSource = new double[] { 10, 11, 12, 13, 14, 15, 16, 18, 20, 22 };

        LanguageBox.SelectedItem = "Auto Detect";
        ThemeBox.SelectedItem = "Dark";
        BackgroundBox.SelectedItem = "Theme Default";
        IndentBox.SelectedItem = 4;
        ProfileBox.SelectedItem = "Custom";
        FontBox.SelectedIndex = 0;
        FontSizeBox.SelectedItem = 14d;
        NoBackgroundBox.IsChecked = false;
        LineNumbersBox.IsChecked = true;
        BorderBox.IsChecked = true;
        FormatOnOutputBox.IsChecked = false;
        PreviewBeforeApplyBox.IsChecked = true;
        EditorConfigBox.IsChecked = true;
        NativeFormatterBox.IsChecked = false;
    }

    private void LoadSettings()
    {
        var settings = AppSettingsService.Load();
        SelectOrDefault(LanguageBox, settings.Language, "Auto Detect");
        SelectOrDefault(ThemeBox, settings.Theme, "Dark");
        SelectOrDefault(BackgroundBox, settings.Background, "Theme Default");
        SelectOrDefault(IndentBox, settings.IndentSize, 4);
        SelectOrDefault(ProfileBox, settings.Profile, "Custom");
        SelectOrDefault(FontSizeBox, settings.FontSize, 14d);
        SelectOrDefault(FontBox, settings.FontFamily, CodeFonts[0]);

        NoBackgroundBox.IsChecked = settings.NoBackground;
        LineNumbersBox.IsChecked = settings.LineNumbers;
        BorderBox.IsChecked = settings.Border;
        FormatOnOutputBox.IsChecked = settings.FormatBeforeOutput;
        PreviewBeforeApplyBox.IsChecked = settings.PreviewBeforeApply;
        EditorConfigBox.IsChecked = settings.UseEditorConfig;
        NativeFormatterBox.IsChecked = settings.UseNativeFormatters;
    }

    protected override void OnClosed(EventArgs e)
    {
        AppSettingsService.TrySave(new AppSettings
        {
            Language = LanguageBox.SelectedItem?.ToString() ?? "Auto Detect",
            Theme = ThemeBox.SelectedItem?.ToString() ?? "Dark",
            Background = BackgroundBox.SelectedItem?.ToString() ?? "Theme Default",
            NoBackground = NoBackgroundBox.IsChecked == true,
            IndentSize = IndentSize,
            LineNumbers = LineNumbersBox.IsChecked == true,
            Border = BorderBox.IsChecked == true,
            FormatBeforeOutput = FormatOnOutputBox.IsChecked == true,
            PreviewBeforeApply = PreviewBeforeApplyBox.IsChecked == true,
            FontFamily = CurrentFontFamily,
            FontSize = CurrentFontSize,
            Profile = ProfileBox.SelectedItem?.ToString() ?? "Custom",
            UseEditorConfig = EditorConfigBox.IsChecked == true,
            UseNativeFormatters = NativeFormatterBox.IsChecked == true
        });
        base.OnClosed(e);
    }

    private void InputChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_initializing && !_suspendRefresh) RefreshPreview();
    }

    private void OptionsChanged(object? sender, RoutedEventArgs e)
    {
        if (_initializing || _suspendRefresh) return;
        MarkProfileCustom();
        RefreshPreview();
    }

    private void LanguageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_initializing && !_suspendRefresh) RefreshPreview();
    }

    private void OptionsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing || _suspendRefresh) return;
        MarkProfileCustom();
        RefreshPreview();
    }

    private void TypographySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing || _suspendRefresh) return;
        MarkProfileCustom();
        ApplyEditorTypography();
        RefreshPreview();
    }

    private void NoBackgroundChanged(object? sender, RoutedEventArgs e)
    {
        BackgroundBox.IsEnabled = NoBackgroundBox.IsChecked != true;
        if (_initializing || _suspendRefresh) return;
        MarkProfileCustom();
        RefreshPreview();
    }

    private void ThemeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing || _suspendRefresh) return;
        MarkProfileCustom();
        ApplyThemeAndRefresh();
    }

    private void ProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing || _applyingProfile || _suspendRefresh) return;
        var profile = ResolveProfile(ProfileBox.SelectedItem?.ToString());
        if (profile is null) return;
        if (profile.Name == "Custom")
        {
            _activeProfileIndentStyle = "space";
            return;
        }
        ApplyProfileV2(profile);
    }

    private void ApplyProfileV2(FormattingProfileV2 profile)
    {
        _activeProfileIndentStyle = profile.Formatting.IndentStyle;
        ApplyProfile(FormattingProfileV2Service.ToLegacy(profile));
    }

    private void ApplyProfile(FormattingProfile profile)
    {
        _applyingProfile = true;
        _suspendRefresh = true;
        try
        {
            SelectOrDefault(ThemeBox, profile.Theme, "Dark");
            SelectOrDefault(BackgroundBox, profile.Background, "Theme Default");
            SelectOrDefault(IndentBox, profile.IndentSize, 4);
            SelectOrDefault(FontSizeBox, profile.FontSize, 14d);
            SelectOrDefault(FontBox, profile.FontFamily, CodeFonts[0]);
            NoBackgroundBox.IsChecked = profile.NoBackground;
            BackgroundBox.IsEnabled = !profile.NoBackground;
            LineNumbersBox.IsChecked = profile.LineNumbers;
            BorderBox.IsChecked = profile.Border;
            FormatOnOutputBox.IsChecked = profile.FormatBeforeOutput;
        }
        finally
        {
            _suspendRefresh = false;
            _applyingProfile = false;
        }

        ApplyEditorTypography();
        DesktopThemeService.Apply(ThemeBox.SelectedItem?.ToString() ?? "Dark");
        RefreshPreview();
        SetStatus($"✓ Applied profile: {profile.Name}  •  {profile.Description}");
    }

    private void MarkProfileCustom()
    {
        if (_applyingProfile || ProfileBox.SelectedItem?.ToString() == "Custom") return;
        _applyingProfile = true;
        ProfileBox.SelectedItem = "Custom";
        _applyingProfile = false;
    }

    private void ApplyThemeAndRefresh()
    {
        DesktopThemeService.Apply(ThemeBox.SelectedItem?.ToString() ?? "Dark");
        RefreshPreview();
    }

    private void ApplyEditorTypography()
    {
        var font = SafeFont(CurrentFontFamily);
        InputBox.FontFamily = font;
        InputBox.FontSize = CurrentFontSize;
    }

    private void RefreshPreview()
    {
        if (_suspendRefresh || LanguageBox.SelectedItem is null || ThemeBox.SelectedItem is null) return;

        var language = ResolveLanguage();
        var theme = ThemeService.Get(ThemeBox.SelectedItem.ToString()!);
        var background = ResolveBackground(theme);
        var previewCode = GetOutputCode(language, out var formatWarning);

        DetectedLanguageText.Text = LanguageBox.SelectedItem?.ToString() == "Auto Detect" ? $"Detected: {language}" : language;
        InputMetaText.Text = $"{CountLines(InputBox.Text)} lines  •  {(InputBox.Text?.Length ?? 0):N0} chars";
        var backgroundName = NoBackgroundBox.IsChecked == true ? "None" : BackgroundBox.SelectedItem?.ToString() ?? "Theme Default";
        var previewPolicy = GetEffectiveFormattingPolicy();
        var previewIndent = previewPolicy.EffectiveIndentSize(IndentSize);
        var previewIndentStyle = previewPolicy.IndentStyle == "tab" ? "tabs" : "spaces";
        PreviewModeText.Text = FormatOnOutputBox.IsChecked == true
            ? $"{theme.Name}  •  {backgroundName}  •  {previewIndent} {previewIndentStyle}  •  formatted"
            : $"{theme.Name}  •  {backgroundName}";

        PreviewPanel.Children.Clear();
        CodePreviewBorder.Background = background == "None" ? Brushes.Transparent : Brush(background);
        CodePreviewBorder.BorderBrush = Brush(theme.Border);
        CodePreviewBorder.BorderThickness = BorderBox.IsChecked == true ? new Thickness(1) : new Thickness(0);

        var lines = RichCodeExportService.NormalizeLines(previewCode);
        var digits = Math.Max(2, lines.Length.ToString().Length);
        var font = SafeFont(CurrentFontFamily);
        var lineHeight = Math.Max(18, CurrentFontSize * 1.5);

        for (var i = 0; i < lines.Length; i++)
        {
            var block = new TextBlock
            {
                FontFamily = font,
                FontSize = CurrentFontSize,
                LineHeight = lineHeight,
                TextWrapping = TextWrapping.NoWrap
            };

            if (LineNumbersBox.IsChecked == true)
                block.Inlines!.Add(new Run((i + 1).ToString().PadLeft(digits) + "  ") { Foreground = Brush(theme.LineNumber) });

            foreach (var token in _highlighter.HighlightLine(lines[i], language))
            {
                block.Inlines!.Add(new Run(token.Text.Replace("\t", new string(' ', previewIndent)))
                {
                    Foreground = Brush(RichCodeExportService.ColorFor(token.Kind, theme, language))
                });
            }

            if (block.Inlines!.Count == 0) block.Inlines.Add(new Run(" "));
            PreviewPanel.Children.Add(block);
        }

        SetStatus(formatWarning ?? $"Ready  •  {language}  •  {lines.Length} preview lines");
    }

    private string ResolveLanguage()
    {
        var selected = LanguageBox.SelectedItem?.ToString() ?? "Auto Detect";
        if (selected != "Auto Detect") return selected;
        if (!string.IsNullOrWhiteSpace(_currentFileLanguageHint) && _currentFileLanguageHint != "Plain Text")
            return _currentFileLanguageHint;
        return LanguageDetector.Detect(InputBox.Text ?? string.Empty);
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
        var source = InputBox.Text ?? string.Empty;
        if (FormatOnOutputBox.IsChecked != true) return source;
        var policy = GetEffectiveFormattingPolicy();
        var indent = policy.EffectiveIndentSize(IndentSize);
        var result = CodeFormatterService.Format(source, language, indent);
        warning = result.Warning;
        return FormattingPolicyService.Apply(
            result.Code, policy, indent, FormattingPolicyService.DetectNewLine(source), documentRules: true);
    }

    private async void FormatDocumentClick(object? sender, RoutedEventArgs e) => await FormatCurrentCodeAsync(false);
    private async void FormatSelectionClick(object? sender, RoutedEventArgs e) => await FormatCurrentCodeAsync(true);

    private async Task FormatCurrentCodeAsync(bool selectionOnly)
    {
        var selectionLength = Math.Abs(InputBox.SelectionEnd - InputBox.SelectionStart);
        if (selectionOnly && selectionLength == 0)
        {
            SetStatus("Select part of the code first, then use Format Selection");
            return;
        }

        var language = ResolveLanguage();
        if (language is "Plain Text" or "Markdown")
        {
            SetStatus($"No structural formatter is required for {language}");
            return;
        }

        var start = Math.Min(InputBox.SelectionStart, InputBox.SelectionEnd);
        var end = Math.Max(InputBox.SelectionStart, InputBox.SelectionEnd);
        var original = selectionOnly ? InputBox.SelectedText ?? string.Empty : InputBox.Text ?? string.Empty;
        var policy = GetEffectiveFormattingPolicy();
        var effectiveIndent = policy.EffectiveIndentSize(IndentSize);
        var execution = await NativeFormatterService.FormatAsync(
            original,
            language,
            effectiveIndent,
            policy.IndentStyle ?? _activeProfileIndentStyle,
            NativeFormatterBox.IsChecked == true);

        if (execution.Error is not null)
        {
            SetStatus(execution.Error);
            await DialogService.ShowAsync(this, "CodyFormat Formatter", execution.Error);
            return;
        }

        var policyFormatted = FormattingPolicyService.Apply(
            execution.Code, policy, effectiveIndent, FormattingPolicyService.DetectNewLine(InputBox.Text ?? string.Empty), !selectionOnly);
        var formatted = selectionOnly ? PreserveSelectionBaseIndent(policyFormatted, start) : policyFormatted;
        if (string.Equals(original, formatted, StringComparison.Ordinal))
        {
            var engine = execution.UsedNative ? $"native: {execution.ProviderId}" : "built-in";
            SetStatus(execution.Notice ?? $"✓ {language} code is already formatted  •  {engine}");
            return;
        }

        if (PreviewBeforeApplyBox.IsChecked == true)
        {
            var preview = new FormatPreviewWindow(original, formatted, language, selectionOnly, CurrentFontFamily, CurrentFontSize);
            var apply = await preview.ShowDialog<bool>(this);
            if (!apply)
            {
                SetStatus("Formatting cancelled; editor text was not changed");
                return;
            }
        }

        if (selectionOnly)
        {
            InputBox.SelectionStart = start;
            InputBox.SelectionEnd = end;
            InputBox.SelectedText = formatted;
            InputBox.SelectionStart = start;
            InputBox.SelectionEnd = start + formatted.Length;
        }
        else
        {
            var caret = InputBox.CaretIndex;
            InputBox.SelectAll();
            InputBox.SelectedText = formatted;
            InputBox.CaretIndex = Math.Min(caret, formatted.Length);
        }

        InputBox.Focus();
        var engineText = execution.UsedNative ? $"native: {execution.ProviderId}" : "built-in";
        var fallbackText = string.IsNullOrWhiteSpace(execution.Notice) ? string.Empty : $"  •  fallback: {execution.Notice}";
        SetStatus(selectionOnly
            ? $"✓ Formatted selection as {language}  •  {engineText}  •  Ctrl/Cmd+Z to undo{fallbackText}"
            : $"✓ Formatted {language}  •  {engineText}  •  {effectiveIndent}-{(policy.IndentStyle == "tab" ? "tab" : "space")} policy  •  Ctrl/Cmd+Z to undo{fallbackText}");
    }

    private string PreserveSelectionBaseIndent(string formatted, int selectionStart)
    {
        var input = InputBox.Text ?? string.Empty;
        if (selectionStart < 0 || selectionStart > input.Length) return formatted;
        var lineStart = selectionStart == 0 ? 0 : input.LastIndexOf('\n', Math.Max(0, selectionStart - 1)) + 1;
        var beforeSelection = input[lineStart..selectionStart];
        if (beforeSelection.Any(ch => !char.IsWhiteSpace(ch))) return formatted;

        var lineEnd = input.IndexOf('\n', lineStart);
        if (lineEnd < 0) lineEnd = input.Length;
        var fullLine = input[lineStart..lineEnd];
        var leadingLength = 0;
        while (leadingLength < fullLine.Length && fullLine[leadingLength] is ' ' or '\t') leadingLength++;
        if (leadingLength == 0) return formatted;

        var remainingIndentLength = Math.Max(0, leadingLength - beforeSelection.Length);
        var firstLinePrefix = remainingIndentLength == 0 ? string.Empty : fullLine.Substring(beforeSelection.Length, remainingIndentLength);
        var continuationPrefix = fullLine[..leadingLength];
        var lines = RichCodeExportService.NormalizeLines(formatted);
        for (var i = 0; i < lines.Length; i++)
            if (lines[i].Length > 0) lines[i] = (i == 0 ? firstLinePrefix : continuationPrefix) + lines[i];
        return string.Join("\n", lines);
    }

    private async void WindowKeyDown(object? sender, KeyEventArgs e)
    {
        var primary = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
        var hasPrimary = e.KeyModifiers.HasFlag(primary);

        if (hasPrimary && e.Key == Key.O)
        {
            e.Handled = true;
            await OpenFileAsync();
            return;
        }
        if (hasPrimary && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && e.Key == Key.F)
        {
            e.Handled = true;
            await FormatCurrentCodeAsync(true);
            return;
        }
        if ((e.KeyModifiers.HasFlag(KeyModifiers.Shift) && e.KeyModifiers.HasFlag(KeyModifiers.Alt) && e.Key == Key.F) ||
            (hasPrimary && e.Key == Key.Enter))
        {
            e.Handled = true;
            await FormatCurrentCodeAsync(false);
        }
    }

    private async void OpenFileClick(object? sender, RoutedEventArgs e) => await OpenFileAsync();

    private async Task OpenFileAsync()
    {
        var storage = StorageProvider;
        if (!storage.CanOpen)
        {
            await DialogService.ShowAsync(this, "CodyFormat", "This platform does not provide an open-file picker.");
            return;
        }

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open source file",
            AllowMultiple = false,
            FileTypeFilter = new[] { SourceCodeFileType, FilePickerFileTypes.TextPlain, FilePickerFileTypes.All }
        });
        if (files.Count > 0) await LoadSourceFileAsync(files[0]);
    }

    private void WindowDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Formats.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private async void WindowDrop(object? sender, DragEventArgs e)
    {
        var file = e.DataTransfer.TryGetFiles()?.OfType<IStorageFile>().FirstOrDefault();
        if (file is not null) await LoadSourceFileAsync(file);
    }

    private async Task LoadSourceFileAsync(IStorageFile file)
    {
        if (!string.IsNullOrWhiteSpace(InputBox.Text))
        {
            var replace = await DialogService.ConfirmAsync(this, "Replace editor", "Opening this file will replace the current editor text. Continue?", "Open file");
            if (!replace) return;
        }

        await using var stream = await file.OpenReadAsync();
        var result = await SourceFileService.ReadAsync(stream, file.Name);
        if (!result.Success)
        {
            SetStatus(result.Error);
            await DialogService.ShowAsync(this, "CodyFormat - Open File", result.Error);
            return;
        }

        _currentFileName = file.Name;
        _currentFilePath = file.Path.IsFile ? file.Path.LocalPath : null;
        _currentFileLanguageHint = result.LanguageHint;
        _projectProfile = null;

        if (_currentFilePath is not null &&
            FormattingProfileV2Service.TryFindProjectProfile(_currentFilePath, out var projectProfile, out _, out _))
        {
            _projectProfile = projectProfile;
            RefreshProfileList();
            if (projectProfile is not null)
            {
                SelectOrDefault(ProfileBox, projectProfile.Name, "Custom");
                ApplyProfileV2(projectProfile);
            }
        }

        RefreshEditorConfig();
        InputBox.Text = result.Text;
        InputBox.CaretIndex = 0;
        CurrentFileText.Text = file.Name;
        Title = $"{file.Name} — {AppInfo.ProductName} v{AppInfo.Version}";
        RefreshPreview();
        var configStatus = _currentEditorConfig.SourceFiles.Count > 0
            ? $"  •  .editorconfig: {_currentEditorConfig.SourceFiles.Count}"
            : string.Empty;
        SetStatus($"✓ Opened {file.Name}  •  language hint: {result.LanguageHint}{configStatus}  •  local file only");
    }

    private async void CopyWordClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is null) return;
            var language = ResolveLanguage();
            var theme = ThemeService.Get(ThemeBox.SelectedItem?.ToString() ?? "Dark");
            var code = GetOutputCode(language, out var warning);
            var background = ResolveBackground(theme);
            var exportIndent = GetEffectiveFormattingPolicy().EffectiveIndentSize(IndentSize);
            var fragment = RichCodeExportService.BuildHtmlFragment(
                code, language, theme, background,
                LineNumbersBox.IsChecked == true, BorderBox.IsChecked == true,
                exportIndent, CurrentFontFamily, CurrentFontSize, _highlighter);
            var rtf = RichCodeExportService.BuildRtf(
                code, language, theme, background,
                LineNumbersBox.IsChecked == true, BorderBox.IsChecked == true,
                exportIndent, CurrentFontFamily, CurrentFontSize, _highlighter);
            var clipboardBackend = await PlatformClipboardService.CopyRichAsync(clipboard, code, fragment, rtf);
            SetStatus(warning ?? $"✓ Copied rich code for Word  •  {language}  •  {clipboardBackend}");
        }
        catch (Exception ex)
        {
            await DialogService.ShowAsync(this, "Clipboard operation failed", ex.Message);
        }
    }

    private async void CopyPlainClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is null) return;
            var language = ResolveLanguage();
            var code = GetOutputCode(language, out var warning);
            await PlatformClipboardService.CopyTextAsync(clipboard, code);
            SetStatus(warning ?? "✓ Plain text copied");
        }
        catch (Exception ex)
        {
            await DialogService.ShowAsync(this, "Clipboard operation failed", ex.Message);
        }
    }

    private async void CopyTelegramClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is null) return;
            var language = ResolveLanguage();
            var code = GetOutputCode(language, out var warning);
            var tag = TelegramExportService.GetLanguageTag(language);
            await PlatformClipboardService.CopyTextAsync(clipboard, TelegramExportService.BuildBlock(code, language));
            SetStatus(warning ?? (TelegramExportService.ContainsFence(code)
                ? $"✓ Copied for Telegram  •  {tag}  •  warning: source contains ```"
                : $"✓ Copied for Telegram  •  ```{tag}"));
        }
        catch (Exception ex)
        {
            await DialogService.ShowAsync(this, "Telegram copy failed", ex.Message);
        }
    }

    private async void ExportDocxClick(object? sender, RoutedEventArgs e)
    {
        if (!StorageProvider.CanSave) return;
        var defaultName = string.IsNullOrWhiteSpace(_currentFileName)
            ? "formatted-code.docx"
            : $"{Path.GetFileNameWithoutExtension(_currentFileName)}-formatted.docx";

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export formatted code",
            SuggestedFileName = defaultName,
            DefaultExtension = "docx",
            FileTypeChoices = new[] { new FilePickerFileType("Word document") { Patterns = new[] { "*.docx" } } }
        });
        if (file is null) return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            if (stream.CanSeek) stream.SetLength(0);
            var language = ResolveLanguage();
            var theme = ThemeService.Get(ThemeBox.SelectedItem?.ToString() ?? "Dark");
            var code = GetOutputCode(language, out var warning);
            var exportIndent = GetEffectiveFormattingPolicy().EffectiveIndentSize(IndentSize);
            DocxExportService.Export(stream, code, language, theme, ResolveBackground(theme),
                LineNumbersBox.IsChecked == true, BorderBox.IsChecked == true,
                exportIndent, CurrentFontFamily, CurrentFontSize, _highlighter);
            await stream.FlushAsync();
            SetStatus(warning ?? $"✓ Exported: {file.Name}");
        }
        catch (Exception ex)
        {
            await DialogService.ShowAsync(this, "DOCX export failed", ex.Message);
        }
    }

    private async void CheckUpdatesClick(object? sender, RoutedEventArgs e)
    {
        CheckUpdatesButton.IsEnabled = false;
        SetStatus("Checking GitHub Releases for updates...");

        try
        {
            var result = await UpdateCheckService.CheckAsync();
            SetStatus(result.Message);

            if (!result.Succeeded)
            {
                await DialogService.ShowAsync(
                    this,
                    "Check for Updates",
                    $"{result.Message}\n\nCodyFormat does not check for updates automatically. Formatting remains fully offline.");
                return;
            }

            if (result.UpdateAvailable)
            {
                var channel = result.LatestIsPrerelease ? "pre-release" : "release";
                var open = await DialogService.ConfirmAsync(
                    this,
                    "Update Available",
                    $"{result.Message}\n\nCurrent: v{result.CurrentVersion}\nLatest {channel}: v{result.LatestVersion}\n\nOpen the GitHub Release page?",
                    "Open Release");

                if (open && !BrowserLauncher.TryOpen(result.ReleaseUrl ?? AppInfo.GitHubReleasesUrl, out var error))
                    await DialogService.ShowAsync(this, "CodyFormat", $"Could not open GitHub Releases.\n\n{error}");

                return;
            }

            await DialogService.ShowAsync(
                this,
                "Check for Updates",
                $"{result.Message}\n\nNo automatic background update checks are performed.");
        }
        finally
        {
            CheckUpdatesButton.IsEnabled = true;
        }
    }

    private async void GitHubClick(object? sender, RoutedEventArgs e)
    {
        if (!BrowserLauncher.TryOpen(AppInfo.GitHubUrl, out var error))
            await DialogService.ShowAsync(this, "CodyFormat", $"Could not open GitHub.\n\n{error}");
    }

    private async void DonateClick(object? sender, RoutedEventArgs e) => await new DonateWindow().ShowDialog(this);
    private async void HelpClick(object? sender, RoutedEventArgs e) => await new HelpWindow().ShowDialog(this);
    private void UndoEditorClick(object? sender, RoutedEventArgs e) { if (InputBox.CanUndo) InputBox.Undo(); InputBox.Focus(); }
    private void CutEditorClick(object? sender, RoutedEventArgs e) => InputBox.Cut();
    private void CopyEditorClick(object? sender, RoutedEventArgs e) => InputBox.Copy();
    private void PasteEditorClick(object? sender, RoutedEventArgs e) => InputBox.Paste();
    private void SelectAllEditorClick(object? sender, RoutedEventArgs e) { InputBox.SelectAll(); InputBox.Focus(); }

    private EditorConfigSettings GetEffectiveFormattingPolicy()
    {
        var baseline = new EditorConfigSettings
        {
            IndentStyle = _activeProfileIndentStyle,
            IndentSize = IndentSize
        };
        return EditorConfigBox.IsChecked == true
            ? EditorConfigSettings.Merge(baseline, _currentEditorConfig)
            : baseline;
    }

    private void RefreshEditorConfig()
    {
        _currentEditorConfig = EditorConfigBox.IsChecked == true && !string.IsNullOrWhiteSpace(_currentFilePath)
            ? EditorConfigService.Resolve(_currentFilePath)
            : new EditorConfigSettings();

        // Do not mutate the user's visible/base indent selection here. The effective
        // EditorConfig policy is applied at format time, so disabling EditorConfig cleanly
        // returns to the selected profile/manual settings.
    }

    private void EditorConfigChanged(object? sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        RefreshEditorConfig();
        RefreshPreview();
        SetStatus(_currentEditorConfig.SourceFiles.Count > 0
            ? $"✓ .editorconfig enabled  •  {_currentEditorConfig.SourceFiles.Count} file(s) resolved"
            : EditorConfigBox.IsChecked == true ? "No .editorconfig found for the current local file" : ".editorconfig disabled");
    }

    private void NativeFormatterChanged(object? sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        SetStatus(NativeFormatterBox.IsChecked == true
            ? "Native formatter opt-in enabled for explicit Format actions only; live preview/export remain built-in and offline."
            : "Native formatter opt-in disabled; CodyFormat built-in formatter will be used.");
    }

    private async void NativeStatusClick(object? sender, RoutedEventArgs e)
    {
        var statuses = NativeFormatterService.GetStatuses();
        var lines = statuses.Select(status =>
            $"{status.DisplayName}: {(status.Available ? "Available" : "Not found")}\n  {string.Join(", ", status.Languages)}" +
            (status.Available ? $"\n  {status.ExecutablePath}" : string.Empty));
        await DialogService.ShowAsync(
            this,
            "Native Formatter Status",
            "Native formatters are opt-in, never auto-installed, and launched without a shell.\n\n" + string.Join("\n\n", lines));
    }

    private void RefreshProfileList()
    {
        var names = FormattingProfileV2Service.GetAvailableProfiles().Select(x => x.Name).ToList();
        if (_projectProfile is not null && !names.Contains(_projectProfile.Name, StringComparer.OrdinalIgnoreCase))
            names.Add(_projectProfile.Name);
        ProfileBox.ItemsSource = names.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private FormattingProfileV2? ResolveProfile(string? name)
    {
        if (_projectProfile is not null && _projectProfile.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            return _projectProfile;
        return FormattingProfileV2Service.Resolve(name);
    }

    private FormattingProfileV2 CurrentProfileV2()
    {
        var selected = ResolveProfile(ProfileBox.SelectedItem?.ToString());
        if (selected is not null && selected.Name != "Custom")
            return selected;

        return new FormattingProfileV2
        {
            Name = "Custom",
            Description = "Exported custom CodyFormat settings.",
            Formatting = new ProfileFormattingOptions { IndentStyle = _activeProfileIndentStyle, IndentSize = IndentSize },
            Appearance = new ProfileAppearanceOptions
            {
                Theme = ThemeBox.SelectedItem?.ToString() ?? "Dark",
                Background = BackgroundBox.SelectedItem?.ToString() ?? "Theme Default",
                NoBackground = NoBackgroundBox.IsChecked == true,
                LineNumbers = LineNumbersBox.IsChecked == true,
                Border = BorderBox.IsChecked == true,
                FontFamily = CurrentFontFamily,
                FontSize = CurrentFontSize
            },
            Output = new ProfileOutputOptions
            {
                FormatBeforeOutput = FormatOnOutputBox.IsChecked == true,
                Target = "General"
            }
        };
    }

    private async void ImportProfileClick(object? sender, RoutedEventArgs e)
    {
        if (!StorageProvider.CanOpen) return;
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import CodyFormat Profile v2",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("CodyFormat profile") { Patterns = new[] { "*.codyprofile.json", "*.json" } },
                FilePickerFileTypes.All
            }
        });
        if (files.Count == 0) return;

        try
        {
            await using var stream = await files[0].OpenReadAsync();
            if (!FormattingProfileV2Service.TryLoad(stream, out var profile, out var error) || profile is null)
            {
                await DialogService.ShowAsync(this, "Import Profile", error);
                return;
            }

            FormattingProfileV2Service.Import(profile);
            RefreshProfileList();
            SelectOrDefault(ProfileBox, profile.Name, "Custom");
            ApplyProfileV2(profile);
            SetStatus($"✓ Imported Profile v2: {profile.Name}");
        }
        catch (Exception ex)
        {
            await DialogService.ShowAsync(this, "Import Profile", ex.Message);
        }
    }

    private async void ExportProfileClick(object? sender, RoutedEventArgs e)
    {
        if (!StorageProvider.CanSave) return;
        var profile = CurrentProfileV2();
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export CodyFormat Profile v2",
            SuggestedFileName = profile.Name.Replace(' ', '-') + ".codyprofile.json",
            DefaultExtension = "json",
            FileTypeChoices = new[] { new FilePickerFileType("CodyFormat profile") { Patterns = new[] { "*.codyprofile.json", "*.json" } } }
        });
        if (file is null) return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            await FormattingProfileV2Service.WriteAsync(stream, profile);
            SetStatus($"✓ Exported Profile v2: {profile.Name}");
        }
        catch (Exception ex)
        {
            await DialogService.ShowAsync(this, "Export Profile", ex.Message);
        }
    }

    private void ClearClick(object? sender, RoutedEventArgs e)
    {
        InputBox.Clear();
        _currentFileName = null;
        _currentFilePath = null;
        _currentFileLanguageHint = null;
        _currentEditorConfig = new EditorConfigSettings();
        _projectProfile = null;
        CurrentFileText.Text = "Untitled";
        Title = $"{AppInfo.ProductName} v{AppInfo.Version}";
        InputBox.Focus();
        SetStatus("Editor cleared");
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private static int CountLines(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 1;
        var count = 1;
        foreach (var ch in text) if (ch == '\n') count++;
        return count;
    }

    private static IBrush Brush(string hex) => new SolidColorBrush(Color.Parse(hex));

    private static FontFamily SafeFont(string name)
    {
        try { return new FontFamily(name); }
        catch { return FontFamily.Default; }
    }

    private static void SelectOrDefault(ComboBox comboBox, object desired, object fallback)
    {
        if (comboBox.ItemsSource is not System.Collections.IEnumerable items) return;
        var values = items.Cast<object>().ToArray();
        comboBox.SelectedItem = values.FirstOrDefault(x => Equals(x, desired) || x.ToString() == desired.ToString())
            ?? values.FirstOrDefault(x => Equals(x, fallback) || x.ToString() == fallback.ToString())
            ?? values.FirstOrDefault();
    }

    private static readonly FilePickerFileType SourceCodeFileType = new("Source code")
    {
        Patterns = new[]
        {
            "*.cs", "*.sql", "*.js", "*.mjs", "*.cjs", "*.ts", "*.tsx", "*.py", "*.json", "*.xml",
            "*.html", "*.htm", "*.css", "*.ps1", "*.psm1", "*.sh", "*.bash", "*.zsh", "*.rs", "*.go",
            "*.php", "*.phtml", "*.java", "*.c", "*.h", "*.cpp", "*.cc", "*.cxx", "*.hpp", "*.kt", "*.kts",
            "*.swift", "*.dart", "*.rb", "*.lua", "*.r", "*.pl", "*.pm", "*.yaml", "*.yml", "*.md", "*.markdown",
            "*.m", "*.mm", "*.scala", "*.sc", "*.groovy", "*.gradle", "*.txt"
        },
        AppleUniformTypeIdentifiers = new[] { "public.source-code", "public.plain-text" },
        MimeTypes = new[] { "text/plain" }
    };
}
