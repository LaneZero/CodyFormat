using System.Text;
using Avalonia.Input;
using Avalonia.Input.Platform;
using CodyFormat.Services;

namespace CodyFormat.Desktop.Services;

/// <summary>
/// Bridges CodyFormat's platform-neutral export builders to the system clipboard.
/// Linux prefers a native X11/XWayland selection provider for office-suite interoperability;
/// Avalonia DataTransfer remains the cross-platform fallback.
/// </summary>
public static class PlatformClipboardService
{
    public static async Task CopyTextAsync(IClipboard clipboard, string? text)
    {
        await clipboard.SetTextAsync(text ?? string.Empty);
        await clipboard.FlushAsync();
    }

    public static async Task<string> CopyRichAsync(
        IClipboard clipboard,
        string plainText,
        string htmlFragment,
        string rtf)
    {
        var htmlDocument = RichCodeExportService.BuildHtmlDocument(htmlFragment);

        // Microsoft Word on Windows expects the registered Win32 clipboard formats
        // "HTML Format" and "Rich Text Format". Publishing RTF as an Avalonia custom
        // string can make Word paste the RTF source itself, so Windows uses a native
        // clipboard backend first.
        if (OperatingSystem.IsWindows())
        {
            var cfHtml = RichCodeExportService.BuildCfHtml(htmlFragment);
            if (WindowsNativeRichClipboardService.TrySet(plainText, cfHtml, rtf, out var windowsBackend))
                return windowsBackend;

            // Safe fallback: do not publish RTF through the portable custom-string path.
            // If native RTF registration failed, HTML + plain text is preferable to
            // exposing raw RTF control words in Word.
            var fallbackItem = new DataTransferItem();
            fallbackItem.Set(DataFormat.CreateStringPlatformFormat("HTML Format"), cfHtml);
            fallbackItem.Set(DataFormat.Text, plainText ?? string.Empty);

            var fallbackTransfer = new DataTransfer();
            fallbackTransfer.Add(fallbackItem);
            await clipboard.SetDataAsync(fallbackTransfer);
            await clipboard.FlushAsync();
            return "Windows Avalonia CF_HTML fallback (native clipboard unavailable)";
        }

        // Office applications on Linux vary in how they negotiate Avalonia custom formats.
        // Own the X11 CLIPBOARD selection directly when X11/XWayland is available, exposing
        // standard HTML/RTF targets exactly as native Linux applications expect them.
        if (OperatingSystem.IsLinux() &&
            LinuxX11RichClipboardService.TrySet(plainText, htmlDocument, rtf, out var linuxBackend))
        {
            return linuxBackend;
        }

        var item = new DataTransferItem();

        if (OperatingSystem.IsMacOS())
        {
            var richText = DataFormat.CreateStringPlatformFormat("public.rtf");
            var html = DataFormat.CreateStringPlatformFormat("public.html");
            item.Set(richText, rtf);
            item.Set(html, htmlDocument);
        }
        else
        {
            // Wayland/native or other Unix fallback. Publish UTF-8 bytes under the exact
            // MIME targets office applications commonly request, plus string aliases for
            // clipboard backends that prefer textual custom formats.
            var htmlBytes = Encoding.UTF8.GetBytes(htmlDocument);
            var rtfBytes = Encoding.ASCII.GetBytes(rtf);

            item.Set(DataFormat.CreateBytesPlatformFormat("text/html"), htmlBytes);
            item.Set(DataFormat.CreateBytesPlatformFormat("text/html;charset=utf-8"), htmlBytes);
            item.Set(DataFormat.CreateBytesPlatformFormat("text/rtf"), rtfBytes);
            item.Set(DataFormat.CreateBytesPlatformFormat("text/richtext"), rtfBytes);
            item.Set(DataFormat.CreateBytesPlatformFormat("application/rtf"), rtfBytes);
            item.Set(DataFormat.CreateStringPlatformFormat("text/html"), htmlDocument);
            item.Set(DataFormat.CreateStringPlatformFormat("text/rtf"), rtf);
            item.Set(DataFormat.CreateStringPlatformFormat("text/richtext"), rtf);
        }

        item.Set(DataFormat.Text, plainText ?? string.Empty);

        var transfer = new DataTransfer();
        transfer.Add(item);
        await clipboard.SetDataAsync(transfer);
        await clipboard.FlushAsync();

        return OperatingSystem.IsLinux()
            ? "Linux Avalonia rich clipboard fallback"
            : OperatingSystem.IsMacOS()
                ? "macOS native pasteboard"
                : "Portable rich clipboard fallback";
    }
}
