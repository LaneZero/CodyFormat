# CodyFormat v0.6.0-preview.3


### Preview.3 startup fix

Preview.3 removes manual `InitializeComponent()` implementations from Avalonia window code-behind files so the Avalonia source generator can wire all named controls correctly. This fixes the startup crash seen in preview.2.

> Preview.2 includes Avalonia 12 AXAML compatibility fixes discovered during the first real Windows publish test.

This is the first cross-platform migration preview of CodyFormat.

The formatter/export engine has been separated from WPF and the desktop interface has been ported to Avalonia 12 for Windows, macOS and Linux. Windows `v0.5.0` remains the stable release while this preview is tested.

This preview is intended for build and compatibility validation, especially native clipboard behavior and Microsoft Word on macOS.
