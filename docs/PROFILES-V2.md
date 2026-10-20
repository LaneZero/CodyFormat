# CodyFormat Profiles v2

Profiles v2 are portable JSON preference files.

```json
{
  "schemaVersion": 2,
  "name": "Team Style",
  "description": "Shared project style",
  "formatting": { "indentStyle": "space", "indentSize": 4 },
  "appearance": {
    "theme": "Dark",
    "background": "Theme Default",
    "noBackground": false,
    "lineNumbers": true,
    "border": true,
    "fontFamily": "Cascadia Mono",
    "fontSize": 14
  },
  "output": { "formatBeforeOutput": true, "target": "Documentation" }
}
```

The schema intentionally contains no command/executable hooks.

Project profile: place a valid v2 profile at `.codyformat/profile.json`. CodyFormat detects it when a local project file is opened. `.editorconfig` remains authoritative for supported file-formatting properties such as indentation and line endings.

CLI `--profile` accepts a built-in/user profile name or a profile JSON path.
