using System.Windows;

namespace CodyFormat;

public partial class DonateWindow : Window
{
    public DonateWindow() => InitializeComponent();

    private void CopyBitcoinClick(object sender, RoutedEventArgs e) => Copy(AppInfo.BitcoinAddress, "Bitcoin address copied");
    private void CopyTetherClick(object sender, RoutedEventArgs e) => Copy(AppInfo.TetherTrc20Address, "Tether TRC20 address copied");
    private void CopyEvmClick(object sender, RoutedEventArgs e) => Copy(AppInfo.EvmAddress, "EVM address copied");
    private void CopySolanaClick(object sender, RoutedEventArgs e) => Copy(AppInfo.SolanaAddress, "Solana address copied");

    private void Copy(string value, string status)
    {
        Clipboard.SetText(value);
        CopyStatus.Text = $"✓ {status}";
    }

    private void CloseClick(object sender, RoutedEventArgs e) => Close();
}
