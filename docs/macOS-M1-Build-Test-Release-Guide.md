# CodyFormat macOS M1 Build, Test & Release Guide

> Target: **Apple Silicon M1 / macOS arm64**
> CodyFormat version: **v0.6.0 Stable**
> Main runtime target: **osx-arm64**

> **Validated hardware:** The macOS ARM64 build has been functionally tested successfully on a real **Apple M1 Mac**. This guide is retained for rebuild, regression testing, signing, notarization, and future release maintenance.

---

## 1. فایل پروژه را روی Mac کپی کن

فایل زیر را روی Mac منتقل کن:

```text
CodyFormat-v0.6.0-github-ready-source.zip
```

پیشنهاد:

```text
~/Downloads/
```

---

## 2. معماری Mac را بررسی کن

Terminal را باز کن:

```bash
uname -m
```

باید خروجی این باشد:

```text
arm64
```

نسخه macOS:

```bash
sw_vers
```

---

## 3. نصب Xcode Command Line Tools

```bash
xcode-select --install
```

بعد از اتمام نصب:

```bash
xcode-select -p
```

خروجی باید شبیه این باشد:

```text
/Library/Developer/CommandLineTools
```

بررسی Git:

```bash
git --version
```

---

## 4. نصب .NET 10 SDK برای Apple Silicon

نسخه موردنیاز:

```text
.NET 10 SDK
macOS Arm64
```

بعد از نصب، Terminal جدید باز کن و اجرا کن:

```bash
dotnet --version
```

باید چیزی شبیه این ببینی:

```text
10.0.xxx
```

سپس:

```bash
dotnet --info
dotnet --list-sdks
```

حداقل یک SDK از سری `10.0.x` باید وجود داشته باشد.

---

## 5. Extract پروژه

```bash
cd ~/Downloads
```

```bash
unzip CodyFormat-v0.6.0-github-ready-source.zip
```

بعد وارد پوشه پروژه شو:

```bash
cd CodyFormat-v0.6.0-github-ready
```

بررسی:

```bash
pwd
```

---

## 6. اجازه اجرای Scriptها

```bash
chmod +x scripts/*.sh
```

بررسی:

```bash
ls -l scripts
```

فایل‌های `.sh` باید executable باشند.

---

## 7. بررسی نسخه SDK پروژه

```bash
cat global.json
```

انتظار:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

سپس:

```bash
dotnet --version
```

---

## 8. Restore پروژه

```bash
dotnet restore CodyFormat.sln
```

اگر Error نداشت:

```text
PASS
```

اگر Restore خطا داد، ادامه نده.

---

## 9. Smoke Test

```bash
./scripts/smoke-test.sh
```

انتظار:

```text
CodyFormat smoke tests PASS
```

اگر Fail شد، Build را ادامه نده.

---

## 10. اصلاح Version در Info.plist

```bash
plutil -replace CFBundleShortVersionString -string "0.6.0" packaging/macos/Info.plist
```

```bash
plutil -replace CFBundleVersion -string "6006" packaging/macos/Info.plist
```

بررسی:

```bash
plutil -p packaging/macos/Info.plist
```

مقادیر مهم:

```text
CFBundleShortVersionString = 0.6.0
CFBundleVersion = 6006
CFBundleIdentifier = io.github.lanezero.codyformat
LSMinimumSystemVersion = 14.0
```

---

## 11. Build نسخه macOS

```bash
./scripts/publish-macos.sh
```

انتظار داریم این دو Bundle ساخته شوند:

```text
artifacts/osx-arm64/CodyFormat.app
artifacts/osx-x64/CodyFormat.app
```

برای M1 فعلاً هدف اصلی:

```text
artifacts/osx-arm64/CodyFormat.app
```

---

## 12. بررسی ساختار Bundle

```bash
find artifacts/osx-arm64/CodyFormat.app -maxdepth 3 -type f | head -30
```

ساختار اصلی:

```text
CodyFormat.app
└── Contents
    ├── Info.plist
    ├── MacOS
    │   └── CodyFormat
    └── Resources
        └── CodyFormat.icns
```

---

## 13. بررسی معماری Binary

```bash
file artifacts/osx-arm64/CodyFormat.app/Contents/MacOS/CodyFormat
```

باید عبارت زیر را ببینی:

```text
arm64
```

اگر `x86_64` بود، Build اشتباه است.

---

## 14. اجرای مستقیم از Terminal

```bash
./artifacts/osx-arm64/CodyFormat.app/Contents/MacOS/CodyFormat
```

اگر برنامه باز شد:

```text
PASS
```

این روش برای دیدن Crash و Exception مناسب‌تر است.

---

## 15. اجرای عادی App

برنامه را ببند و اجرا کن:

```bash
open artifacts/osx-arm64/CodyFormat.app
```

---

# تست‌های Runtime

## 16. تست آیکن

آیکن CodyFormat را در این قسمت‌ها بررسی کن:

- Finder
- Dock
- `Cmd + Tab`
- Application Window

اگر آیکن مشکل داشت:

```bash
ls -lh artifacts/osx-arm64/CodyFormat.app/Contents/Resources/CodyFormat.icns
```

و:

```bash
plutil -p artifacts/osx-arm64/CodyFormat.app/Contents/Info.plist | grep Icon
```

باید:

```text
CodyFormat.icns
```

نمایش داده شود.

---

## 17. تست UI

موارد زیر را بررسی کن:

- [ ] Dark Theme
- [ ] Light Theme
- [ ] Language Dropdown
- [ ] Auto Detect
- [ ] Background
- [ ] None Background
- [ ] Border
- [ ] Line Numbers
- [ ] Font
- [ ] Font Size
- [ ] Indent 2
- [ ] Indent 4
- [ ] Profiles

---

## 18. تست Formatter با SQL

کد تست:

```sql
select u.id,u.username,o.total from users u left join orders o on o.user_id=u.id where u.active=1 and o.total>1000 order by o.total desc;
```

بعد از Format، خروجی باید خواناتر شود، مثلاً:

```sql
SELECT
    u.id,
    u.username,
    o.total
FROM users u
LEFT JOIN orders o
    ON o.user_id = u.id
WHERE
    u.active = 1
    AND o.total > 1000
ORDER BY
    o.total DESC;
```

زبان‌های زیر را هم تست کن:

- [ ] C#
- [ ] C
- [ ] C++
- [ ] Java
- [ ] JavaScript
- [ ] TypeScript
- [ ] Rust
- [ ] Go
- [ ] PHP
- [ ] Python
- [ ] JSON
- [ ] XML
- [ ] HTML
- [ ] CSS
- [ ] Bash
- [ ] PowerShell
- [ ] YAML

---

## 19. Format Preview و Undo

یک کد نامرتب وارد کن.

Shortcut:

```text
Shift + Option + F
```

اگر Review before apply فعال است، باید پنجره:

```text
Original | Formatted
```

باز شود.

بعد Apply کن.

Undo:

```text
Cmd + Z
```

باید کد قبلی برگردد.

---

## 20. Open File

```text
Cmd + O
```

فایل‌هایی مثل:

```text
.cs
.sql
.rs
.go
.php
.py
```

را باز کن.

باید:

- [ ] فایل باز شود
- [ ] زبان تشخیص داده شود
- [ ] Preview به‌روز شود

---

## 21. Drag & Drop

از Finder یک فایل Source را داخل CodyFormat بکش.

باید:

```text
File opens
Language detected
Preview updated
```

---

## 22. Copy Plain

در CodyFormat:

```text
Copy Plain
```

داخل TextEdit:

```text
Cmd + V
```

فقط Code ساده باید Paste شود.

---

## 23. Copy Telegram

مثلاً برای SQL:

```text
Copy Telegram
```

خروجی باید شبیه این باشد:

````text
```sql
SELECT *
FROM users;
```
````

---

# مهم‌ترین تست macOS

## 24. Copy for Word

Microsoft Word for Mac را باز کن.

داخل CodyFormat:

```text
Copy for Word
```

بعد داخل Word:

```text
Cmd + V
```

باید این موارد حفظ شوند:

- [ ] Syntax Colors
- [ ] Indentation
- [ ] Font
- [ ] Background
- [ ] Line Numbers
- [ ] Border

### خطاهای غیرقابل قبول

نباید RTF خام دیده شود:

```text
\rtf1
\ansi
\cf1
```

و نباید فقط Plain Text بدون Style منتقل شود.

اگر یکی از این اتفاق‌ها افتاد:

```text
STOP RELEASE
```

---

## 25. تست None Background

فعال کن:

```text
None Background
```

بعد:

```text
Copy for Word
```

داخل Word Paste کن.

انتظار:

```text
Syntax Colors = YES
Background Fill = NO
```

---

## 26. Export DOCX

در CodyFormat:

```text
Export DOCX
```

فایل را در Word for Mac باز کن.

بررسی:

- [ ] Syntax Colors
- [ ] Font
- [ ] Indentation
- [ ] Background
- [ ] Border
- [ ] Line Numbers
- [ ] Editable Document

---

## 27. Help / Donate / GitHub

این پنجره‌ها را باز کن:

- [ ] Help
- [ ] Donate
- [ ] GitHub

هیچ‌کدام نباید Crash کنند.

GitHub باید Browser پیش‌فرض را باز کند.

---

## 28. Settings Persistence

Theme را تغییر بده.

Font را تغییر بده.

برنامه را ببند.

دوباره:

```bash
open artifacts/osx-arm64/CodyFormat.app
```

تنظیمات باید باقی مانده باشند.

---

# اگر تمام Runtime Testها PASS شدند

تا اینجا برای تست برنامه به Apple Developer نیاز نیست.

مرحله بعد:

```text
Developer ID Application
Hardened Runtime
Code Signing
Notarization
Stapling
Gatekeeper Validation
GitHub Release ZIP
```

---

# Signing & Notarization

## 29. نصب Full Xcode

Xcode را از Mac App Store نصب کن.

بعد:

```bash
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
```

```bash
sudo xcodebuild -license accept
```

بررسی:

```bash
xcodebuild -version
```

```bash
xcrun notarytool --help
```

---

## 30. بررسی Developer ID Certificate

بعد از ساخت و نصب Certificate از نوع:

```text
Developer ID Application
```

اجرا کن:

```bash
security find-identity -v -p codesigning
```

خروجی باید چیزی شبیه این باشد:

```text
Developer ID Application: YOUR NAME (TEAMID)
```

Variable بساز:

```bash
export IDENTITY='Developer ID Application: YOUR NAME (TEAMID)'
```

---

## 31. انتخاب App

```bash
export APP="$PWD/artifacts/osx-arm64/CodyFormat.app"
```

بررسی:

```bash
echo "$APP"
```

---

## 32. Sign فایل‌های Mach-O

```bash
find "$APP/Contents/MacOS" -type f -print0 |
while IFS= read -r -d '' filePath; do
    if file "$filePath" | grep -q "Mach-O"; then
        echo "Signing: $filePath"
        codesign \
          --force \
          --options runtime \
          --timestamp \
          --sign "$IDENTITY" \
          "$filePath"
    fi
done
```

---

## 33. Sign خود Bundle

```bash
codesign \
  --force \
  --options runtime \
  --timestamp \
  --sign "$IDENTITY" \
  "$APP"
```

---

## 34. Verify Signature

```bash
codesign --verify --deep --strict --verbose=2 "$APP"
```

```bash
codesign -dv --verbose=4 "$APP"
```

---

## 35. تست Signed App

```bash
open "$APP"
```

حداقل این موارد را دوباره تست کن:

- [ ] Startup
- [ ] Format
- [ ] Copy Telegram
- [ ] Copy for Word
- [ ] DOCX Export

---

## 36. ساخت ZIP برای Notarization

```bash
ditto \
  -c \
  -k \
  --keepParent \
  "$APP" \
  artifacts/CodyFormat-notarize-arm64.zip
```

---

## 37. ذخیره Credential برای Notary

```bash
xcrun notarytool store-credentials "CodyFormatNotary" \
  --apple-id "YOUR_APPLE_ID" \
  --team-id "YOUR_TEAM_ID" \
  --password "YOUR_APP_SPECIFIC_PASSWORD"
```

از App-specific Password استفاده کن.

---

## 38. ارسال برای Notarization

```bash
xcrun notarytool submit \
  artifacts/CodyFormat-notarize-arm64.zip \
  --keychain-profile "CodyFormatNotary" \
  --wait
```

انتظار:

```text
status: Accepted
```

---

## 39. اگر Notarization Fail شد

```bash
xcrun notarytool log \
  YOUR_SUBMISSION_ID \
  --keychain-profile "CodyFormatNotary"
```

---

## 40. Staple

اگر وضعیت:

```text
Accepted
```

بود:

```bash
xcrun stapler staple "$APP"
```

سپس:

```bash
xcrun stapler validate "$APP"
```

---

## 41. Gatekeeper Validation

```bash
spctl \
  --assess \
  --type execute \
  --verbose=4 \
  "$APP"
```

انتظار:

```text
accepted
source=Notarized Developer ID
```

---

## 42. ساخت ZIP نهایی GitHub Release

فایل موقت Notarization را Release نکن.

فایل نهایی را از App امضا و Staple‌شده بساز:

```bash
ditto \
  -c \
  -k \
  --sequesterRsrc \
  --keepParent \
  "$APP" \
  artifacts/CodyFormat-v0.6.0-macos-arm64.zip
```

---

## 43. SHA-256

```bash
shasum -a 256 \
  artifacts/CodyFormat-v0.6.0-macos-arm64.zip
```

Hash را به:

```text
SHA256SUMS.txt
```

اضافه کن.

---

# GitHub Release Assets

در نهایت Release می‌تواند شامل این فایل‌ها باشد:

```text
CodyFormat-v0.6.0-win-x64.zip
CodyFormat-v0.6.0-win-arm64.zip

CodyFormat-v0.6.0-linux-x64.tar.gz
CodyFormat-v0.6.0-linux-arm64.tar.gz

CodyFormat-v0.6.0-macos-arm64.zip

SHA256SUMS.txt
```

GitHub خودش این دو مورد را اضافه می‌کند:

```text
Source code (zip)
Source code (tar.gz)
```

---

# Quick Command Sequence

این بخش برای اجرای سریع مراحل Build و تست اولیه است:

```bash
uname -m
sw_vers

xcode-select --install

git --version
dotnet --version
dotnet --info
dotnet --list-sdks

cd ~/Downloads

unzip CodyFormat-v0.6.0-github-ready-source.zip

cd CodyFormat-v0.6.0-github-ready

chmod +x scripts/*.sh

cat global.json

dotnet restore CodyFormat.sln

./scripts/smoke-test.sh

plutil -replace CFBundleShortVersionString -string "0.6.0" packaging/macos/Info.plist

plutil -replace CFBundleVersion -string "6006" packaging/macos/Info.plist

plutil -p packaging/macos/Info.plist

./scripts/publish-macos.sh

find artifacts/osx-arm64/CodyFormat.app -maxdepth 3 -type f | head -30

file artifacts/osx-arm64/CodyFormat.app/Contents/MacOS/CodyFormat

./artifacts/osx-arm64/CodyFormat.app/Contents/MacOS/CodyFormat
```

---

# Release Gate

قبل از Signing و Notarization باید همه این موارد PASS باشند:

```text
[ ] App launches on M1
[ ] arm64 binary confirmed
[ ] Icon correct
[ ] Dark/Light themes
[ ] Language detection
[ ] Formatter
[ ] Format preview
[ ] Undo
[ ] Open file
[ ] Drag & drop
[ ] Copy Plain
[ ] Copy Telegram
[ ] Copy for Word
[ ] None Background
[ ] DOCX export
[ ] Help
[ ] Donate
[ ] GitHub link
[ ] Settings persistence
```

اگر حتی یکی از موارد اصلی زیر Fail شد:

```text
Startup
Copy for Word
DOCX Export
Formatter
```

Release macOS را متوقف کن.
