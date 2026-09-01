using Avalonia.Controls;
using Avalonia.Interactivity;
using CodyFormat.Desktop.Services;

namespace CodyFormat.Desktop.Views;

public partial class DonateWindow : Window
{
    public DonateWindow()
    {
        InitializeComponent();
        BitcoinBox.Text = AppInfo.BitcoinAddress;
        TetherBox.Text = AppInfo.TetherTrc20Address;
        EvmBox.Text = AppInfo.EvmAddress;
        SolanaBox.Text = AppInfo.SolanaAddress;
    }

    private void CloseClick(object? sender, RoutedEventArgs e) => Close();

    private async void CopyAddressClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard) return;
        var (label, value) = button.Tag?.ToString() switch
        {
            "Bitcoin" => ("Bitcoin", AppInfo.BitcoinAddress),
            "Tether" => ("Tether TRC20", AppInfo.TetherTrc20Address),
            "Evm" => ("EVM", AppInfo.EvmAddress),
            "Solana" => ("Solana", AppInfo.SolanaAddress),
            _ => ("Wallet", string.Empty)
        };
        if (value.Length == 0) return;
        await PlatformClipboardService.CopyTextAsync(clipboard, value);
        StatusText.Text = $"✓ {label} address copied";
    }
}
