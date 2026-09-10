using System.Runtime.InteropServices;
using System.Text;

namespace CodyFormat.Desktop.Services;

/// <summary>
/// Publishes rich clipboard data through the native Win32 clipboard API.
/// Microsoft Word expects the registered clipboard formats named
/// "HTML Format" and "Rich Text Format" rather than MIME-like aliases.
/// </summary>
internal static class WindowsNativeRichClipboardService
{
    private const uint GmemMoveable = 0x0002;
    private const uint CfUnicodeText = 13;

    public static bool TrySet(
        string plainText,
        string cfHtml,
        string rtf,
        out string backend)
    {
        backend = string.Empty;
        if (!OperatingSystem.IsWindows())
            return false;

        var htmlFormat = RegisterClipboardFormatW("HTML Format");
        var rtfFormat = RegisterClipboardFormatW("Rich Text Format");
        if (htmlFormat == 0 || rtfFormat == 0)
        {
            backend = "Windows native clipboard format registration failed";
            return false;
        }

        if (!TryOpenClipboard())
        {
            backend = "Windows native clipboard unavailable";
            return false;
        }

        try
        {
            if (!EmptyClipboard())
            {
                backend = $"Windows EmptyClipboard failed ({Marshal.GetLastWin32Error()})";
                return false;
            }

            var plainOk = SetUnicodeText(CfUnicodeText, plainText ?? string.Empty);
            var htmlOk = SetBytes(htmlFormat, Encoding.UTF8.GetBytes((cfHtml ?? string.Empty) + "\0"));
            var rtfOk = SetBytes(rtfFormat, Encoding.ASCII.GetBytes((rtf ?? string.Empty) + "\0"));

            // Word can consume either registered rich format. Keep plain Unicode text as
            // a standards-compliant fallback for applications that request only text.
            if (plainOk && (htmlOk || rtfOk))
            {
                backend = htmlOk && rtfOk
                    ? "Windows native CF_HTML + RTF clipboard"
                    : htmlOk
                        ? "Windows native CF_HTML clipboard (RTF unavailable)"
                        : "Windows native RTF clipboard (CF_HTML unavailable)";
                return true;
            }

            backend = $"Windows native clipboard incomplete (text={plainOk}, html={htmlOk}, rtf={rtfOk})";
            return false;
        }
        finally
        {
            CloseClipboard();
        }
    }

    private static bool TryOpenClipboard()
    {
        // Clipboard ownership can briefly be held by Word, Office clipboard helpers,
        // RDP, or another desktop process. A short bounded retry avoids random failures
        // without blocking the UI for a noticeable amount of time.
        for (var attempt = 0; attempt < 10; attempt++)
        {
            if (OpenClipboard(IntPtr.Zero))
                return true;

            Thread.Sleep(25 + (attempt * 10));
        }

        return false;
    }

    private static bool SetUnicodeText(uint format, string value)
    {
        var bytes = Encoding.Unicode.GetBytes(value + "\0");
        return SetBytes(format, bytes);
    }

    private static bool SetBytes(uint format, byte[] bytes)
    {
        var handle = GlobalAlloc(GmemMoveable, new UIntPtr((uint)bytes.Length));
        if (handle == IntPtr.Zero)
            return false;

        var locked = GlobalLock(handle);
        if (locked == IntPtr.Zero)
        {
            GlobalFree(handle);
            return false;
        }

        try
        {
            Marshal.Copy(bytes, 0, locked, bytes.Length);
        }
        finally
        {
            GlobalUnlock(handle);
        }

        // On success SetClipboardData transfers ownership of the HGLOBAL to Windows.
        // We must only free the handle when SetClipboardData fails.
        if (SetClipboardData(format, handle) == IntPtr.Zero)
        {
            GlobalFree(handle);
            return false;
        }

        return true;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint RegisterClipboardFormatW(string lpszFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);
}
