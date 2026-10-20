# Native Formatter Providers — v0.7.0-beta.1

Native providers are an explicit opt-in. CodyFormat never downloads, installs, or silently executes external formatters.

Supported provider mapping in Beta.1:

| Provider | Languages |
|---|---|
| Black | Python |
| Prettier | JavaScript, TypeScript, JSON, CSS, HTML, Markdown, YAML |
| clang-format | C, C++, Objective-C |
| rustfmt | Rust |
| gofmt | Go |

C# intentionally remains on CodyFormat's built-in formatter in this beta. `dotnet format` is project-scoped and may modify multiple files, so it is not used as a stdin-style provider.

## Security contract

- off by default
- fixed provider allowlist
- no arbitrary command or executable path in Profiles v2
- no shell execution
- only absolute PATH directories are searched; empty, relative, and current-directory entries are skipped
- `.cmd` and `.bat` wrappers are not launched on Windows because that would require a shell
- structured `ProcessStartInfo.ArgumentList` arguments only
- isolated temporary working directory
- 12-second timeout and cancellation
- stdout/stderr captured
- failed or missing providers fall back to CodyFormat's built-in formatter

The GUI uses native providers only after the user enables **Native formatter** and explicitly clicks Format Code / Format Selection. Live preview, Copy/Export, and startup do not spawn them.

CLI examples:

```bash
codyformat --native-status
codyformat app.py --native --check
codyformat app.py --native --write
codyformat ./src --recursive --native --check
```
