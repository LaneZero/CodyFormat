# CodyFormat

<p align="center">
  <strong>Format. Highlight. Copy. Export.</strong>
  <br>
  A lightweight, privacy-first, fully offline code formatter for Windows.
</p>

<p align="center">
  Format your code beautifully and paste it into <strong>Microsoft Word</strong>, PowerPoint, documentation, reports, tutorials, and technical documents without losing syntax highlighting or indentation.
</p>

---

## ✨ Why CodyFormat?

Copying code into Word should not feel like fighting with a document editor from 2003.

**CodyFormat** makes it easy to:

* 🧹 Format messy source code
* 🎨 Apply syntax highlighting
* 👀 Preview the final result
* 📋 Copy styled code directly into Word
* 📄 Export formatted code as `.docx`
* 🖌️ Customize code backgrounds and borders
* 🔢 Show or hide line numbers
* 🌙 Switch between Dark and Light themes
* 🔒 Keep your source code completely local

**No account. No cloud. No telemetry. No code uploads.**

---

## 🚀 Features

| Feature                        | Description                                        |
| ------------------------------ | -------------------------------------------------- |
| 📴 **Fully Offline**           | Works without an internet connection               |
| 🔒 **Privacy First**           | Your source code stays on your computer            |
| 🎨 **Syntax Highlighting**     | Language-aware syntax colors                       |
| 🧹 **Code Formatting**         | Improve indentation and structure                  |
| 🤖 **Auto Detect**             | Automatically detect the programming language      |
| 🌙 **Dark / Light**            | Choose your preferred editor theme                 |
| 🖌️ **Custom Backgrounds**     | Customize the appearance of code blocks            |
| 🚫 **No Background**           | Keep syntax colors without a colored code block    |
| 🔢 **Line Numbers**            | Enable or disable line numbers                     |
| 📋 **Copy for Word**           | Paste formatted code directly into Microsoft Word  |
| 📄 **DOCX Export**             | Export formatted code as an editable Word document |
| ⌨️ **Keyboard Shortcuts**      | Format code without reaching for the mouse         |
| 🖱️ **Right-Click Formatting** | Quickly format code from the editor                |
| 📚 **Built-in Help**           | Documentation available inside the application     |

---

# 🔒 Privacy First

CodyFormat is built around one simple principle:

> **Your code belongs on your computer, not on someone else's server.**

CodyFormat processes your source code locally.

It does **not**:

* ❌ Upload source code
* ❌ Use a formatting API
* ❌ Send analytics
* ❌ Send telemetry
* ❌ Require registration
* ❌ Require a cloud account
* ❌ Execute pasted source code
* ❌ Fetch URLs found inside source code
* ❌ Send source code over the network

You can use CodyFormat completely offline.

---

# 💻 Supported Languages

CodyFormat currently supports syntax highlighting and formatting for many commonly used programming, scripting, query, and markup languages.

### .NET & JVM

`C#` · `Java` · `Kotlin` · `Scala` · `Groovy`

### Systems Programming

`C` · `C++` · `Rust` · `Go` · `Objective-C`

### Web Development

`JavaScript` · `TypeScript` · `PHP` · `HTML` · `CSS`

### Scripting

`Python` · `PowerShell` · `Bash` · `Ruby` · `Perl` · `Lua`

### Mobile Development

`Swift` · `Dart` · `Kotlin`

### Data & Query

`SQL` · `JSON` · `XML` · `YAML` · `R`

> More languages and formatter improvements may be added in future releases.

---

# 🎨 Syntax Highlighting

CodyFormat uses language-aware syntax highlighting instead of applying one generic color scheme to everything.

Depending on the language, it can distinguish:

* Keywords
* Control keywords
* Types
* Functions and methods
* Properties
* Variables
* Strings
* Numbers
* Constants
* Comments
* Operators
* HTML/XML tags
* Attributes

Dark and Light themes use separate color palettes to keep code readable in different environments.

---

# 🧹 Code Formatting

CodyFormat formats code locally while focusing on readability and preserving its original meaning.

Formatting may include:

* Indentation
* Block alignment
* Brace alignment
* SQL clause separation
* Nested structure indentation
* HTML/XML nesting
* JSON pretty printing
* XML pretty printing
* Removal of unnecessary trailing whitespace

### Indentation

```text
2 Spaces
4 Spaces
```

For languages where whitespace can affect program behavior, CodyFormat intentionally uses more conservative formatting rules.

> **The goal is simple: make code easier to read without silently changing what it does.**

---

# 🗃️ SQL Formatting

SQL gets special treatment because apparently writing:

```sql
SELECT a,b,c FROM table WHERE x=1 AND y=2 ORDER BY c;
```

was considered acceptable by civilization.

CodyFormat understands common SQL structures such as:

```sql
SELECT
FROM
WHERE
JOIN
LEFT JOIN
RIGHT JOIN
INNER JOIN
ON
AND
OR
GROUP BY
ORDER BY
HAVING
CASE
WHEN
THEN
ELSE
END
```

Nested queries and common SQL blocks are indented to make long queries easier to read.

---

# 📋 Copy Code to Microsoft Word

One of CodyFormat's main goals is making syntax-highlighted code work properly in Microsoft Word.

Select:

```text
Copy for Word
```

CodyFormat prepares the clipboard content with:

* 🎨 Syntax colors
* ↔️ Indentation
* 🔤 Font formatting
* 🖌️ Selected background
* ▫️ Optional border
* 🔢 Line structure

Then simply paste into Word:

```text
Ctrl + V
```

The result is designed to closely match the preview shown inside CodyFormat.

---

# 🚫 No Background

Sometimes you want the syntax highlighting, but you don't want a giant colored rectangle sitting inside your document like it pays rent.

Enable:

```text
None Background
```

This removes the code block background from:

* Preview
* Word clipboard output
* DOCX export

This is particularly useful when you want the code to inherit the background of an existing Word document.

---

# 🔢 Line Numbers

Line numbers can be enabled or disabled independently.

Useful for:

**Documentation**

When line numbers are unnecessary.

**Technical Reports**

When you need to reference specific lines of code.

---

# 📄 Export to DOCX

CodyFormat can export formatted source code directly to an editable Microsoft Word document.

Select:

```text
Export DOCX
```

Choose your destination and save.

The generated `.docx` file can then be opened and edited normally in Microsoft Word.

---

# ⚡ Format Before Copy / Export

Don't want to format manually first?

Enable:

```text
Format before copy/export
```

CodyFormat will automatically format the code before generating:

* Preview
* Clipboard output
* DOCX output

If you want to update the code directly inside the editor, use:

```text
Format Code
```

---

# ⌨️ Keyboard Shortcuts

### Format Code

```text
Shift + Alt + F
```

You can also use:

```text
Ctrl + Enter
```

### Right-Click

Right-click inside the editor and select:

```text
Format Code
```

---

# 🚀 Getting Started

## Portable Version

The portable version is the easiest way to get started.

1. Open the **Releases** section.
2. Download the latest Windows portable release.
3. Extract the ZIP file.
4. Run:

```text
CodyFormat.exe
```

The portable version includes the required .NET runtime and does not require a separate .NET installation.

---

## Framework-Dependent Version

The framework-dependent build is smaller but requires the appropriate Microsoft .NET Desktop Runtime.

Download the release, extract it, and run:

```text
CodyFormat.exe
```

If the required runtime is missing, Windows may ask you to install it.

---

# 🛠️ Build From Source

## Requirements

* Windows 10 or Windows 11
* x64 system
* .NET 10 SDK
* Git

Visual Studio is optional.

## Clone

```bash
git clone https://github.com/LaneZero/CodyFormat.git
cd CodyFormat
```

## Framework-Dependent Build

```bat
build-release.cmd
```

Output:

```text
dist\win-x64\
```

Run:

```text
CodyFormat.exe
```

## Portable Build

```bat
build-portable.cmd
```

Output:

```text
dist\portable-win-x64\
```

The portable build packages the required .NET runtime with CodyFormat.

---

# 🗂️ Project Structure

```text
CodyFormat/
│
├── Models/
├── Services/
├── Resources/
│
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
│
├── CodyFormat.csproj
├── app.manifest
│
├── build-release.cmd
├── build-portable.cmd
│
├── README.md
├── CHANGELOG.md
├── LICENSE
└── .gitignore
```

---

# 🛡️ Security

CodyFormat treats pasted content as **text**.

It does not intentionally:

* Execute source code
* Execute PowerShell commands
* Execute shell scripts
* Execute SQL queries
* Load remote scripts
* Run macros
* Send source code over the network

As always, review files downloaded from third parties before opening or processing them.

---

# ❓ FAQ

### Does CodyFormat require internet access?

**No.**

CodyFormat is designed to work completely offline.

### Is my source code uploaded anywhere?

**No.**

Formatting and syntax highlighting happen locally.

### Does CodyFormat execute my code?

**No.**

Source code is treated as text for formatting and highlighting.

### Can I use CodyFormat with confidential company code?

CodyFormat is designed around local processing and does not require source code to leave your computer.

You should still follow your organization's security and data-handling policies.

### Why can Auto Detect be incorrect?

Very short or ambiguous snippets may not contain enough information to reliably identify a language.

For example:

```text
print("Hello")
```

could belong to more than one language depending on the context.

When accuracy matters, manually select the language.

### Why does formatting differ between languages?

Different languages have different syntax and formatting rules.

For example, JSON and XML can be structurally parsed very reliably, while whitespace-sensitive languages require more conservative formatting.

CodyFormat prioritizes preserving code behavior over aggressive formatting.

---

# 🤝 Contributing

Contributions are welcome.

If you find a bug or have an idea:

1. Open an **Issue**
2. Describe the problem or feature request
3. Include the programming language involved when reporting formatting problems
4. Provide a minimal reproducible example when possible

For code contributions:

```bash
git checkout -b feature/my-feature
```

Make your changes and commit:

```bash
git commit -m "feat: add my feature"
```

Push your branch:

```bash
git push origin feature/my-feature
```

Then open a Pull Request.

---

# 🐛 Reporting Formatting Bugs

Formatting bugs are much easier to investigate when the original code is included.

Please provide:

```text
Language:
CodyFormat version:
Expected result:
Actual result:
Original code:
Formatted code:
```

Before posting code publicly, remove:

* Passwords
* Credentials
* API keys
* Tokens
* Private URLs
* Confidential company information

---

# 🗺️ Roadmap

Possible future improvements include:

* More programming languages
* Improved language detection
* More advanced SQL formatting
* Additional themes
* Custom syntax themes
* More export formats
* Improved large-file performance
* Additional formatter rules
* Better language-specific formatting
* User-defined formatting preferences
* Improved accessibility
* Additional keyboard shortcuts

> Roadmap items are not guaranteed and may change based on development priorities.

---

# ❤️ Support CodyFormat

CodyFormat is free software.

Keeping a free developer tool alive takes development, testing, documentation, bug fixing, and the occasional argument with a compiler that has technically done nothing wrong.

If CodyFormat saves you time and you'd like to support continued development, donations are welcome.

**Donations are completely optional.**

## ₿ Bitcoin

**Network:** Bitcoin

```text
bc1q5tl36qpk27hf7upl8l753xa0gcm57adrvmwgkz
```

## 💵 Tether

**Network:** TRON / TRC20

```text
TA5pibChqS7CeHiDvfP7V3uQVMynk6SxLq
```

## 🟣 EVM Networks

**Supported Networks:**

* Ethereum
* BNB Smart Chain
* Polygon

```text
0x6634E26BA0e323182B7A9a89278E9a7BbCa9aF70
```

## ☀️ Solana

**Network:** Solana

```text
91kuBFEZAnRk8ixsuzAQnAVvtzLAsQpPFRsfe285ZKUC
```

> ⚠️ **Important**
>
> Always verify the complete wallet address and blockchain network before transferring cryptocurrency.
>
> Cryptocurrency transactions generally cannot be reversed.

---

# 📜 License

CodyFormat is distributed under the **MIT License**.

See:

```text
LICENSE
```

for the complete license terms.

The MIT License allows use, modification, distribution, and private or commercial use while preserving the required copyright and license notice.

---

# 👨‍💻 Developer

**LaneZeroO**

GitHub:
https://github.com/LaneZero

Project:
https://github.com/LaneZero/CodyFormat

---

# ⭐ Support the Project

If CodyFormat saves you time, consider giving the repository a ⭐ **Star**.

Stars help other developers discover the project and provide a simple way to support continued open-source development.

---

<p align="center">
  <strong>CodyFormat</strong>
  <br>
  <sub>Format locally. Keep your code private.</sub>
</p>
