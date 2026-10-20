# Security Policy

## Supported release line

The current supported stable line is:

```text
CodyFormat v0.6.0
```

The WPF `v0.5.0` build is retained as a historical rollback baseline but is no longer the primary development line.

## Reporting a vulnerability

Please do **not** publish sensitive vulnerability details, private source code, credentials, tokens, or exploit material in a public GitHub issue.

Use a private GitHub security-reporting channel when available, or contact the project maintainer through the contact method published on the repository profile.

A useful security report includes:

- affected CodyFormat version
- operating system and architecture
- clear reproduction steps
- expected vs actual behavior
- security impact
- whether the issue requires a specially crafted source file or clipboard payload

## Security model

CodyFormat is designed as a local formatter/highlighter. It should not intentionally:

- upload source code
- execute formatted code
- execute shell/PowerShell scripts
- run SQL queries
- fetch URLs found in source code
- require an online formatter API
- send telemetry

Source files must still be treated as untrusted input. Parsers, file handling, clipboard export, and document generation should validate data and avoid executing source content.

Future external formatter-provider integration must remain explicit and local. CodyFormat must not silently download or execute unknown formatter binaries.


## Manual update checks

CodyFormat does not perform automatic or background update checks. When the user explicitly invokes **Check Updates**, CodyFormat sends an HTTPS GET request to the public GitHub Releases API for the CodyFormat repository. Source code, editor contents, file paths, clipboard contents, profiles, and formatting inputs are not included in that request. Formatting and export features do not depend on this network request.

## Native formatter providers (v0.7 beta)

Native formatter execution is disabled by default and is treated as local external-process execution. CodyFormat uses a fixed provider allowlist, resolves executables only from absolute PATH entries, does not use a shell, does not accept executable commands from Profiles v2, applies a 12-second timeout, and falls back to the built-in formatter when a provider is missing or fails. CodyFormat never downloads or installs native formatters automatically.
