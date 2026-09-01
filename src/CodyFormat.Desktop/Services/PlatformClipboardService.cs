using Avalonia.Input;
using Avalonia.Input.Platform;
using CodyFormat.Services;

namespace CodyFormat.Desktop.Services;

/// <summary>
/// Bridges CodyFormat's platform-neutral export builders to Avalonia's clipboard.
/// Text works on all desktop targets. Rich HTML uses the native clipboard format
/// expected by Windows, macOS, or Linux so Word-compatible copy can remain local.
/// </summary>
public static class PlatformClipboardService
{
    public static async Task CopyTextAsync(IClipboard clipboard, string? text)
    {
        await clipboard.SetTextAsync(text ?? string.Empty);
        // Flush is a no-op on unsupported backends and keeps clipboard data alive after exit where supported.
        await clipboard.FlushAsync();
    }

    public static async Task CopyRichAsync(IClipboard clipboard, string plainText, string htmlFragment)
    {
        var item = new DataTransferItem();
        item.Set(DataFormat.Text, plainText ?? string.Empty);

        if (OperatingSystem.IsWindows())
        {
            var html = DataFormat.CreateStringPlatformFormat("HTML Format");
            item.Set(html, RichCodeExportService.BuildCfHtml(htmlFragment));
        }
        else if (OperatingSystem.IsMacOS())
        {
            var html = DataFormat.CreateStringPlatformFormat("public.html");
            item.Set(html, htmlFragment);
        }
        else
        {
            var html = DataFormat.CreateStringPlatformFormat("text/html");
            item.Set(html, htmlFragment);
        }

        var transfer = new DataTransfer();
        transfer.Add(item);
        await clipboard.SetDataAsync(transfer);
        await clipboard.FlushAsync();
    }
}
