# CodyFormat Roadmap

This roadmap starts from the **v0.6.0 Stable** cross-platform baseline.

The rule for future work is simple: Stable remains a rollbackable baseline. New capabilities are developed on a new branch/version line, validated, and only then promoted.

## Current stable baseline — v0.6.0

Validated runtime platforms:

- Windows x64 — tested
- Ubuntu Linux x64 — tested
- macOS Apple Silicon ARM64 — tested on a real Apple M1 Mac

Additional build targets:

- Windows ARM64 — build available, broader hardware validation pending
- Linux ARM64 — build available, broader hardware validation pending
- macOS Intel x64 — build available, real Intel Mac validation pending

Core stable capabilities include offline formatting, syntax highlighting, language detection, rich Word copy, Telegram copy, DOCX export, profiles, themes, formatting preview, selection formatting, drag/drop, and cross-platform packaging.

---

## v0.7.0 — Developer workflow

Goal: move CodyFormat from a desktop-only formatting utility into a practical developer workflow tool without turning it into an IDE.

Planned scope:

- **CLI**
  - format a file
  - format stdin/stdout
  - `--write` mode
  - language override
  - indentation/profile selection
  - deterministic exit codes for automation
- **Batch / folder formatting**
  - recursive discovery
  - supported-file summary
  - preview changed/unchanged files
  - optional backup before write
  - safe exclusions
- **`.editorconfig` support**
  - indentation style/size
  - line endings
  - final newline
  - trailing-whitespace policy
- **Native formatter providers**
  - detect trusted local formatter tools where available
  - examples: `dotnet format`, `clang-format`, `rustfmt`, `gofmt`, `black`, `prettier`
  - built-in CodyFormat engine remains the offline fallback
  - external providers are opt-in/configurable and never silently downloaded
- **Profiles v2**
  - project-oriented profiles
  - import/export profile files
  - output-target presets
- **SQL formatter improvements**
  - SQL Server
  - PostgreSQL
  - MySQL
  - Oracle
  - SQLite
  - Generic SQL
  - better CTE, window, CASE, JOIN/APPLY, MERGE, PIVOT and PL/SQL/T-SQL layout
- **Editor essentials**
  - Find
  - Replace
  - regex/whole-word options where practical

Release gate:

- CLI and GUI produce compatible formatting behavior for the same built-in formatter profile.
- Batch formatting must provide a preview/dry-run path before destructive writes.
- No automatic installation or download of external formatter executables.

---

## v0.8.0 — Export and presentation

Goal: make CodyFormat a stronger documentation and publishing tool.

Planned scope:

- **Export as PNG**
  - padding
  - rounded corners
  - filename/title
  - line numbers
  - transparent background
- **Export as SVG**
  - scalable syntax-highlighted output
  - presentation/documentation friendly
- **Copy as Markdown**
  - GitHub-style fenced blocks
  - generic Markdown mode
- **Copy as HTML**
  - embeddable HTML
  - optional inline CSS
  - standalone snippet mode
- **Advanced Diff**
  - before/after view
  - changed line summary
  - indentation/spacing change summary
- **Formatting Safety Report**
  - protected strings/comments status
  - line-ending status
  - token-preservation checks where practical
  - conservative risk indicator
- **Theme Editor**
  - token color customization
  - import/export CodyFormat themes
- **VS Code theme import**
  - map compatible token colors into CodyFormat's theme model
- **Beautify / Minify**
  - JSON
  - HTML
  - CSS
  - JavaScript where safe
  - XML
- **JSON / XML utilities**
  - validate
  - pretty print
  - minify
  - JSON sort keys as an explicit operation

Release gate:

- Exported visuals must remain sharp at intended dimensions.
- Theme import must not silently overwrite user themes.
- Minify operations must remain explicit and separate from normal formatting.

---

## v0.9.0 — Desktop productivity and scale

Goal: improve speed, operating-system integration, and large-workflow handling.

Planned scope:

- **Command Palette**
  - keyboard-first access to major actions
- **OS file-context integration**
  - Windows: Format/Open with CodyFormat
  - Linux: desktop/Open With integration
  - macOS: Open With integration
- **Portable Mode**
  - optional local settings/profiles beside the application
- **Session Recovery**
  - privacy-aware restore behavior
  - explicit option to never persist editor content
- **Large File Mode**
  - reduce expensive live highlighting/preview work
  - manual formatting for large sources
- **Performance statistics**
  - line count
  - formatting time
  - highlighting time
- **Structure-aware formatting actions**
  - current block
  - current function
  - current class where safe parsing is available
- **Project Mode improvements**
  - better multi-file navigation
  - project-level formatting summary

Release gate:

- Large-file handling must not freeze the main UI under defined test inputs.
- Session recovery must remain local and privacy-configurable.
- OS integrations must have clean uninstall/removal paths.

---

## v1.0.0 — Long-term stable toolchain

Goal: establish CodyFormat as a mature cross-platform formatter/export tool with stable behavior and documented compatibility guarantees.

Planned scope:

- Stable Windows, Linux, and macOS desktop release line
- Stable CLI command/exit-code behavior
- Stable formatter/provider contracts
- Stable profile/theme formats
- Full regression suite across primary platforms
- Accessibility completion
  - keyboard navigation
  - focus visibility
  - screen-reader labels
  - high-contrast behavior
  - scalable UI
- Packaging automation
  - checksums
  - signed/notarized macOS release pipeline
  - reproducible asset naming
- Performance and memory regression gates
- Compatibility and upgrade policy
- Documentation freeze for public v1 behavior

Primary v1 validation matrix:

- Windows x64
- Linux x64 on Ubuntu
- macOS ARM64 on Apple Silicon
- ARM64/Intel compatibility builds validated as hardware is available

---

## Non-goals

CodyFormat should not become:

- a full IDE
- a compiler
- a code execution sandbox
- an online/cloud formatter
- a source-code upload service
- a package manager for third-party formatter binaries

The product focus remains: **format, highlight, review, copy, and export code cleanly and locally**.
