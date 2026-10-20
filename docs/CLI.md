# CodyFormat CLI — v0.7.0-beta.1

The first CLI build reuses `CodyFormat.Core`; it does not contain a second formatter implementation.

## Examples

```bash
codyformat file.py
codyformat file.py --check
codyformat file.py --write
codyformat file.sql --language SQL
codyformat file.py --indent 4
codyformat file.py --profile Documentation
cat query.sql | codyformat --stdin --language SQL
```

## Exit codes

| Code | Meaning |
|---:|---|
| 0 | Success / already formatted |
| 1 | `--check`: changes are required |
| 2 | Invalid CLI usage |
| 3 | File/encoding/I/O error |
| 4 | Formatter failure or safety warning |

`--check` never writes files. `--write` uses a same-directory temporary file and replacement step so a formatter failure does not partially overwrite the original file.

The CLI preserves detected BOM/encoding and original line-ending style for file writes.


## Batch mode — alpha.2

```bash
codyformat <directory> --recursive --check
codyformat <directory> --recursive --write
codyformat <directory> --recursive --write --backup
```

Repeatable filters:

```text
--include <glob>
--exclude <glob>
```

Examples:

```bash
codyformat . --recursive --check --include "**/*.py"
codyformat . --recursive --write --include "**/*.cs" --exclude "**/Generated/**"
```

Batch mode formats recognized source-file types by default and skips `.git`, `bin`, `obj`, `artifacts`, `TestResults`, and `node_modules`. Directory symlinks/reparse points are not traversed.

Exit code `1` means `--check` found formatting changes. Exit code `4` means one or more files failed. Exit code `5` means cancellation.


## Update checking

`v0.7.0-alpha.2` adds a **manual desktop Check Updates** action. It is intentionally not executed in the background and is not required by CLI formatting or batch operations. CLI behavior remains offline-first.


## Project configuration — alpha.3

`.editorconfig` is enabled by default for local files and batch operations.

```bash
codyformat file.py --check
codyformat file.py --write
codyformat file.py --write --no-editorconfig
codyformat file.py --profile ./team.codyprofile.json
```

`--profile` accepts a built-in/user profile name or a profile JSON path. Explicit `--indent` overrides the resolved `.editorconfig` indentation size.


## Native formatter providers (Beta.1)

`--native` explicitly enables allowlisted local formatter providers with built-in fallback. `--native-status` reports discovery status without formatting a file. CodyFormat never installs providers automatically and does not use shell command concatenation.

```bash
codyformat --native-status
codyformat app.py --native --check
codyformat ./src --recursive --native --write
```
