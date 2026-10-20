# Formatter Quality Gate — v0.7.0-alpha.1

The v0.7 development line begins with a formatter regression pass because Python formatting was confirmed to be weak in the v0.6.0 baseline.

## Priority 0: Python

Alpha.1 checks:

- existing indentation levels are normalized to the chosen indent width
- comma spacing
- assignment/comparison spacing outside strings/comments
- inline comments are preserved
- string contents are protected
- triple-quoted string bodies are treated conservatively

## All-language audit

Every selectable language in `LanguageCatalog` has a representative smoke sample. Additional safety cases cover whitespace-sensitive or multiline syntax such as:

- JSON comments/trailing commas: preserve with warning instead of silently deleting content
- YAML block scalars
- Bash/Ruby heredocs
- Lua long strings
- PowerShell here-strings and block-comment-sensitive input
- C/C++ raw strings and multiline macros
- Rust lifetimes/raw strings
- JavaScript/TypeScript template literals
- Markdown and Plain Text whitespace preservation

CodyFormat follows a conservative rule: when the lightweight formatter cannot safely understand a construct, preserving source is better than aggressively rewriting it.
