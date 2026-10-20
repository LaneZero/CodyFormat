# CodyFormat v0.7.0-beta.1

Beta.1 introduces optional native formatter providers while keeping CodyFormat's built-in formatter as the safe fallback.

## Highlights

- Black for Python
- Prettier for JavaScript, TypeScript, JSON, CSS, HTML, Markdown, and YAML
- clang-format for C, C++, and Objective-C
- rustfmt for Rust
- gofmt for Go
- CLI `--native` and `--native-status`
- GUI opt-in and provider diagnostics

Native tools are never installed automatically, never run at startup, and never run from live preview. CodyFormat launches allowlisted executables without a shell, uses a 12-second timeout, captures output, and falls back to its built-in formatter on provider absence/failure.

C# remains on the built-in formatter in Beta.1. Project-scoped `dotnet format` is intentionally not executed against arbitrary projects.

`v0.6.0` remains the stable release until the v0.7 line completes its beta/RC gates.
