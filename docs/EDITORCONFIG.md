# .editorconfig support

CodyFormat v0.7.0-alpha.3 resolves project `.editorconfig` files locally for opened/formatted files.

Supported properties: `indent_style`, `indent_size`, `tab_width`, `end_of_line`, `insert_final_newline`, and `trim_trailing_whitespace`.

Precedence: parent `.editorconfig` → nearer `.editorconfig` → explicit CLI `--indent` for indentation size. `root = true` stops parent traversal.

CLI: use `--no-editorconfig` to temporarily ignore project rules.

GUI: the `.editorconfig` checkbox enables/disables project rule application for the current local file.

CodyFormat does not execute commands from `.editorconfig`. Unsupported properties are ignored.

Pattern support in alpha.3 covers common `*`, `?`, `**`, and brace alternatives such as `*.{cs,fs}`. More specialized EditorConfig glob syntax remains a compatibility target for later hardening.
