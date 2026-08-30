# Changelog

## v0.3.1

- Added the official CodyFormat application icon supplied by LaneZeroO.
- Added a multi-resolution Windows `.ico` for the executable.
- Applied the icon to the main window, Help window, Donate window, taskbar, Alt+Tab, and Windows executable metadata.

## 0.3.0

- Renamed the application from CodeCraft Local to **CodyFormat**.
- Renamed output executable and project metadata to CodyFormat.
- Changed the project GitHub target to `https://github.com/LaneZero/CodyFormat`.
- Added Rust, Go, PHP, Java, C, C++, Kotlin, Swift, Dart, Ruby, Lua, R, Perl, YAML, Markdown, Objective-C, Scala and Groovy.
- Expanded auto language detection and syntax keyword/builtin coverage.
- Reworked SQL formatting to use token-aware clause formatting rather than whitespace-only cleanup.
- Added configurable 2-space / 4-space indentation.
- Improved brace-language indentation while protecting strings and comments.
- Added HTML nesting formatting and block formatting for Bash/Ruby/Lua.
- Normalized syntax-significant indentation conservatively for Python/YAML.
- Added `Shift+Alt+F` and `Ctrl+Enter` Format Code shortcuts.
- Added **Format Code** to the editor context menu.
- Added a dedicated **None background** checkbox; Preview, Word clipboard and DOCX omit background fill when enabled.
- Rebuilt ComboBox templates to fix dark-mode text/background contrast.
- Added Donate window and copy actions for Bitcoin, Tether TRC20, EVM networks and Solana.
- Added donation safety warning.
- Updated Help content for formatter behavior, shortcuts, privacy and supported languages.
