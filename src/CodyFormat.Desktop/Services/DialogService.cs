using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace CodyFormat.Desktop.Services;

public static class DialogService
{
    public static async Task<bool> ConfirmAsync(Window owner, string title, string message, string acceptText = "Continue")
    {
        var result = false;
        var dialog = CreateBase(title, message);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
        var cancel = new Button { Content = "Cancel" };
        var accept = new Button { Content = acceptText };
        accept.Classes.Add("accent");
        cancel.Click += (_, _) => dialog.Close();
        accept.Click += (_, _) => { result = true; dialog.Close(); };
        buttons.Children.Add(cancel);
        buttons.Children.Add(accept);
        ((StackPanel)dialog.Content!).Children.Add(buttons);
        await dialog.ShowDialog(owner);
        return result;
    }

    public static async Task ShowAsync(Window owner, string title, string message)
    {
        var dialog = CreateBase(title, message);
        var ok = new Button { Content = "OK", HorizontalAlignment = HorizontalAlignment.Right };
        ok.Classes.Add("accent");
        ok.Click += (_, _) => dialog.Close();
        ((StackPanel)dialog.Content!).Children.Add(ok);
        await dialog.ShowDialog(owner);
    }

    private static Window CreateBase(string title, string message)
    {
        var stack = new StackPanel { Margin = new Thickness(22), Spacing = 18 };
        stack.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MaxWidth = 520
        });

        return new Window
        {
            Title = title,
            Width = 560,
            SizeToContent = SizeToContent.Height,
            MinHeight = 160,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = stack
        };
    }
}
