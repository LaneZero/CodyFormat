using System.Diagnostics;

namespace CodyFormat.Desktop.Services;

public static class BrowserLauncher
{
    public static bool TryOpen(string url, out string? error)
    {
        error = null;
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
