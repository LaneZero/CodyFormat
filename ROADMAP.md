# CodyFormat Roadmap

This roadmap starts from the **v0.6.0 Stable** cross-platform baseline.

CodyFormat v0.6.0 is the current stable rollback point. New features must be developed on a new version branch, validated through preview/release-candidate builds, and only then merged into `main`.

## Current Stable — v0.6.0

Validated runtime platforms:

- Windows x64 — tested
- Ubuntu Linux x64 — tested
- macOS Apple Silicon ARM64 — tested on a real Apple M1 Mac

Additional published/build targets may include Windows ARM64, Linux ARM64, and macOS Intel x64, but they remain architecture-validation targets until equivalent real-hardware testing is completed.

Stable capabilities include offline formatting, syntax highlighting, language detection, format preview, selection formatting, rich Word copy, Telegram copy, editable DOCX export, themes, profiles, drag/drop, file opening, and cross-platform packaging.

### Stable maintenance rule

The `v0.6.x` line is for bug fixes, security fixes, packaging fixes, and compatibility corrections only.

New product features belong to `v0.7.0` and later.

---

# v0.7.0 — Developer Workflow

## Product goal

Turn CodyFormat from a desktop formatting utility into a practical local developer-workflow tool without turning it into an IDE.

## Development train

### v0.7.0-alpha.1 — CLI Foundation + Formatter Quality Pass

Status: **completed / maintainer-tested, including the published GitHub Release asset**

Primary scope:

- Add `CodyFormat.Cli`
- Reuse `CodyFormat.Core`; do not duplicate formatter logic
- Format one file
- Read from stdin and write to stdout
- Explicit language override
- Profile/indent selection
- `--check` mode
- `--write` mode
- `--version`
- `--help`
- Deterministic exit codes
- UTF-8 and BOM handling
- LF/CRLF preservation policy

Example target commands:

```bash
codyformat file.cs
codyformat file.cs --write
codyformat file.sql --language sql
cat query.sql | codyformat --stdin --language sql
codyformat file.py --check
```

Safety requirements:

- no code execution
- no shell evaluation
- no network dependency
- `--write` must use safe/atomic replacement
- formatting failures must not truncate the original file

Exit-code contract must be documented before Beta.


### Formatter Quality Gate (v0.7.0-alpha.1)

The first v0.7 development build also includes a formatter regression pass across every selectable language. This was added after a confirmed Python formatting issue in the v0.6.0 line.

Required work:

- Python formatter is Priority 0.
- Add representative regression cases for every language in `LanguageCatalog`.
- Preserve strings, comments and whitespace-sensitive constructs.
- Do not silently discard source text during formatting.
- JSON comments must never be silently removed.
- HTML raw `script`, `style`, `pre` and `textarea` bodies must not be split incorrectly on `<` characters.
- Markdown and Plain Text must remain preservation-oriented.
- Formatter failures must return the original source rather than a partial rewrite.

This is a regression/safety gate, not a claim that the built-in formatter is a full parser for every language.

### v0.7.0-alpha.2 — Batch / Folder Formatting

Status: **local validation passed; superseded by alpha.3 development**

Primary scope:

- recursive directory discovery
- supported-extension filtering
- include glob patterns
- exclude glob patterns
- ignore `.git`, `bin`, `obj`, `artifacts` and common generated folders by default
- dry-run summary
- changed/unchanged/failed file counts
- optional backup mode
- explicit `--write` requirement
- cancellation support
- per-file error isolation
- manual opt-in GitHub Release update check
- no automatic/background network check

Example:

```bash
codyformat ./src --recursive --check
codyformat ./src --recursive --write
codyformat . --include "**/*.cs" --exclude "**/Generated/**" --check
```

Required dry-run summary:

```text
Scanned:   143
Supported: 98
Changed:   24
Unchanged: 73
Failed:     1
Written:    0
```

No batch operation may modify files when running in check/dry-run mode.

The update checker is manual-only. Normal CodyFormat formatting workflows remain offline and must not depend on GitHub availability.

### v0.7.0-alpha.3 — .editorconfig + Profiles v2

Status: **active development phase**

`.editorconfig` initial support:

- `indent_style`
- `indent_size`
- `tab_width`
- `end_of_line`
- `insert_final_newline`
- `trim_trailing_whitespace`

Behavior:

- nested `.editorconfig` files must follow directory precedence
- `root = true` must stop parent traversal
- unsupported properties must be ignored safely
- GUI and CLI must resolve the same effective settings

Profiles v2:

- versioned profile schema
- import/export
- project profile
- output-target presets
- backward migration from existing profiles
- invalid profile files must not corrupt current settings

### v0.7.0-beta.1 — Native Formatter Providers

Optional local formatter providers:

- `dotnet format`
- `clang-format`
- `rustfmt`
- `gofmt`
- `black`
- `prettier`

Architecture requirements:

- provider abstraction lives outside UI code
- explicit executable discovery
- no silent downloads
- no package-manager installation
- no shell command concatenation
- arguments passed as structured process arguments
- process timeout
- stdout/stderr capture
- cancellation
- built-in CodyFormat formatter remains available
- user can force built-in mode

Provider status example:

```text
Rust
Provider: rustfmt
Status: detected
Fallback: CodyFormat Built-in
```

Security rule: CodyFormat must never execute formatter commands derived from source-code contents.

### v0.7.0-beta.2 — SQL + Editor Workflow

SQL work:

- SQL Server dialect
- Oracle dialect
- PostgreSQL dialect
- MySQL dialect
- SQLite dialect
- Generic SQL fallback
- better formatting for CTE
- CASE
- JOIN/APPLY
- MERGE
- PIVOT
- window functions
- PL/SQL blocks
- T-SQL blocks

Editor essentials:

- Find
- Replace
- Find next/previous
- case-sensitive option
- whole-word option
- regex mode where safe
- replace all
- preserve Undo behavior

No SQL statement is ever executed. SQL support remains formatting-only.

### v0.7.0-rc.1 — Integration & Hardening

Feature freeze.

Required work:

- cross-platform regression pass
- CLI/GUI parity tests
- batch destructive-write tests
- malformed input tests
- `.editorconfig` precedence tests
- provider timeout/failure tests
- profile migration tests
- SQL dialect regression tests
- documentation update
- release packaging verification
- SHA-256 generation
- real-system smoke tests

Primary validation matrix:

- Windows x64
- Ubuntu Linux x64
- macOS ARM64 on Apple Silicon

Other architecture packages must keep their validation status explicit.

## Mandatory GitHub Release Asset Verification Gate

Every Alpha, Beta, RC, and Stable milestone must be verified from the **actual asset downloaded back from GitHub Releases**, not only from a local build.

Required sequence:

1. Run local automated validation.
2. Build/package the release asset.
3. Generate SHA-256 checksums.
4. Publish the milestone on GitHub Releases.
5. Download the published asset again from GitHub.
6. Verify its SHA-256 against `SHA256SUMS.txt`.
7. Extract/install into a fresh location.
8. Confirm the application/CLI reports the expected version.
9. Run the milestone runtime smoke test against the downloaded package.
10. Record the tested OS and architecture.
11. Only then mark the milestone accepted and move to the next phase.

If signing, notarization, repackaging, or release automation changes the bytes after local testing, the GitHub-downloaded package is the authoritative release artifact.

---

## v0.7.0 Stable release gate

Stable is allowed only when:

- CLI and GUI use the same core formatting behavior
- CLI exit-code behavior is documented and tested
- `--check` never modifies files
- `--write` cannot truncate a source file after formatter failure
- batch mode has explicit dry-run/check behavior
- `.editorconfig` resolution is deterministic
- external providers are opt-in/configurable and never downloaded automatically
- provider execution has timeout and cancellation
- formatting remains fully usable with no external formatter installed
- no regression in Word, Telegram, DOCX, file opening, drag/drop, themes, or existing language support
- Windows x64, Ubuntu Linux x64, and macOS Apple Silicon pass the release smoke matrix

---

# v0.8.0 — Export & Presentation

## Product goal

Make CodyFormat a stronger code-publishing and documentation tool.

## Planned development train

### v0.8.0-alpha.1 — Rendering Pipeline

- reusable render model shared by preview/export
- deterministic code-block dimensions
- padding
- border
- line numbers
- title/filename
- background
- transparent-background option
- high-DPI rendering tests

### v0.8.0-alpha.2 — PNG / SVG Export

PNG:

- selectable scale
- transparent background
- predictable output dimensions
- clipboard/save-to-file flow

SVG:

- sharp scalable output
- embedded styling
- safe escaping
- no remote font/resource dependency

### v0.8.0-alpha.3 — Markdown / HTML Output

- Copy Markdown
- GitHub fenced Markdown
- Generic Markdown
- Copy HTML
- inline-CSS HTML
- standalone HTML snippet
- escaped code content
- output preview

Telegram remains a separate target because destination behavior differs from generic Markdown.

### v0.8.0-beta.1 — Advanced Diff + Safety Report

Diff:

- line additions/removals
- before/after view
- whitespace-aware display
- changed-line count
- indentation/spacing summary

Safety report:

- strings preserved
- comments preserved
- line-ending status
- token-count/token-preservation checks where reliable
- conservative warning level
- never claim semantic equivalence when CodyFormat cannot prove it

### v0.8.0-beta.2 — Theme System v2

- custom Theme Editor
- duplicate existing theme
- import/export `.codytheme`
- versioned theme schema
- preview before applying
- VS Code theme import for compatible token categories
- no silent overwrite of user themes

### v0.8.0-beta.3 — Beautify / Minify Utilities

Explicit utilities for:

- JSON
- XML
- HTML
- CSS
- JavaScript only where safe

JSON extras:

- validate
- sort keys as explicit action
- escape/unescape where appropriate

XML extras:

- validate
- pretty print
- compact/minify

Minify is always a separate explicit action from normal Format.

### v0.8.0-rc.1 — Export Regression

Release gate:

- PNG remains sharp at documented scales
- SVG renders correctly in major browsers/editors
- exported HTML contains no remote dependencies
- Markdown/HTML escape untrusted source text correctly
- Theme import cannot overwrite an existing theme without confirmation
- malformed theme files fail safely
- minify never runs as part of normal formatting
- existing Word/DOCX/Telegram outputs remain regression-tested

---

# v0.9.0 — Desktop Productivity & Scale

## Product goal

Improve native desktop integration, performance, privacy controls, and large-file/project workflows.

## Planned development train

### v0.9.0-alpha.1 — Command Palette

- `Ctrl/Cmd + Shift + P`
- searchable commands
- keyboard-first actions
- language switch
- theme switch
- formatter actions
- export actions
- open file
- settings navigation

### v0.9.0-alpha.2 — OS Integration

Windows:

- Open with CodyFormat
- optional shell/context integration
- clean uninstall

Linux:

- desktop entry
- MIME/open-with integration
- user-local install/uninstall

macOS:

- Open With integration
- bundle/file associations where appropriate

OS integration must be optional and reversible.

### v0.9.0-alpha.3 — Portable Mode

- opt-in portable marker
- local settings
- local profiles
- local themes
- clear precedence between portable and normal settings
- no migration surprises

### v0.9.0-beta.1 — Privacy-aware Session Recovery

- optional recovery
- opt-out: never persist editor contents
- local-only storage
- bounded retention
- crash recovery
- clear-session action
- no telemetry/cloud sync

### v0.9.0-beta.2 — Large File Mode

Threshold-based behavior:

- reduce live highlighting
- disable/reduce live preview
- manual formatting
- cancellation
- progress feedback
- bounded memory behavior

Performance test datasets should include large SQL, JSON, source-code and mixed-line-ending files.

### v0.9.0-beta.3 — Project Mode

- folder/project navigation
- changed-file summary
- filter by language
- project-level profile
- project `.editorconfig`
- batch integration
- format current block/function/class only where a reliable parser/structure detector exists

### v0.9.0-rc.1 — Performance & Desktop Gate

Required release checks:

- defined large-file tests do not freeze the UI
- cancellation remains responsive
- session recovery respects privacy settings
- portable mode leaves normal settings untouched
- context integrations uninstall cleanly
- project operations do not modify excluded/generated files
- startup, memory and formatting regressions are measured against v0.8 stable

---

# v1.0.0 — Stable Toolchain Contract

## Product goal

Freeze and document the public behavior expected from CodyFormat as a mature local formatting/export tool.

## Scope

- stable Windows, Linux and macOS desktop release line
- stable CLI syntax and exit codes
- stable formatter-provider contract
- stable Profile v2 schema
- stable Theme v2 schema
- migration guarantees for supported config formats
- full regression suite
- accessibility completion
- release automation hardening
- signed/notarized macOS release workflow
- reproducible release asset naming
- checksums
- performance and memory gates
- compatibility policy
- upgrade documentation
- public documentation freeze for v1 contracts

## Accessibility gate

- complete keyboard navigation
- visible focus
- screen-reader labels
- high-contrast support
- scalable UI
- reduced-motion behavior where animation exists

## v1.0 release gate

- no known data-loss bug
- no known source-code execution path
- no required cloud dependency
- no telemetry requirement
- CLI/API/config compatibility documented
- stable configuration migration tested
- primary-platform regression matrix green
- release packages rebuilt from a tagged clean commit
- release checksums verified
- documentation matches actual binaries

---

# Versioning Policy

## Stable fixes

Use patch releases for stable fixes:

```text
v0.6.1
v0.6.2
```

Examples:

- crash fix
- clipboard compatibility fix
- packaging correction
- security fix
- documentation correction tied to release behavior

Do not put major new features into the `v0.6.x` maintenance line.

## Feature development

Use a dedicated development branch:

```text
develop/v0.7.0
```

Feature branches:

```text
feature/v0.7.0-cli
feature/v0.7.0-batch
feature/v0.7.0-editorconfig
feature/v0.7.0-providers
feature/v0.7.0-sql
feature/v0.7.0-find-replace
```

Stable fixes:

```text
fix/v0.6.1-<short-name>
```

## Release progression

Recommended:

```text
alpha.N
→ beta.N
→ rc.N
→ stable
```

Do not promote a build only because it compiles. Runtime validation and regression tests remain release requirements.

---

# Continuous Non-goals

CodyFormat should not become:

- a full IDE
- a compiler
- a code execution sandbox
- a cloud formatter
- a source-code upload service
- an AI code execution agent
- a package manager for formatter binaries

The product focus remains:

**format, highlight, review, copy, export, and automate code formatting locally and safely.**
