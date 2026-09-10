namespace CodyFormat.Desktop.Services;

public static class PlatformInfo
{
    public static string Name =>
        OperatingSystem.IsWindows() ? "Windows" :
        OperatingSystem.IsMacOS() ? "macOS" :
        OperatingSystem.IsLinux() ? "Linux" : "Desktop";

    public static string Architecture => System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
}
