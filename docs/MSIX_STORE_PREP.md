# WordsNote WPF -> Microsoft Store (MSIX) Preparation

Date: 2026-04-19

This document describes the technical baseline to publish `WordsNote.Desktop` to Microsoft Store through MSIX.

## 1. What has been implemented

- Desktop runtime config moved to `appsettings.json` with environment-variable override support.
- WPF startup switched to Generic Host + Dependency Injection (`IConfiguration`, `IOptions`, service registrations).
- Local file persistence standardized to `%LocalAppData%` via `IAppDataPathProvider`.
- JSON serialization hardened for trimming via source generation (`WordsNoteJsonSerializerContext`).
- Linker descriptor root added (`TrimmerRoots.xml`) to preserve critical app model/config types.
- Advanced linker descriptor added (`TrimmerFrameworkExclusions.xml`) for System.Data-related framework assemblies.
- Publish profiles added for startup/perf optimization:
  - `win-x64-store.pubxml`
  - `win-arm64-store.pubxml`
  - Controlled noise suppression for known framework trim codes in CI logs: `IL2026`, `IL2067`, `IL2075`, `IL2104`
- Windows Application Packaging Project scaffolded:
  - `src/desktop/WordsNote.Package/WordsNote.Package.wapproj`
  - `src/desktop/WordsNote.Package/Package.appxmanifest`

## 2. Configuration model

Desktop config section is `Desktop`:

- `ApiBaseUrl`
- `GoogleClientId`
- `ThemeMode`
- `LocalDataFolderName`

Current repository default preconfigures `Desktop:GoogleClientId` in desktop appsettings for optional browser login.
Desktop Settings UI intentionally does not require end users to manually set Google Client ID.

Environment variable prefix is `WORDSNOTE_`.

Examples:

- `WORDSNOTE_Desktop__ApiBaseUrl=https://api.example.com`
- `WORDSNOTE_Desktop__GoogleClientId=...`
- `WORDSNOTE_Desktop__ThemeMode=dark`

## 3. Packaging manifest placeholders to fill

In `Package.appxmanifest`, update these values before Store submission:

- `Identity Name`
- `Publisher`
- `DisplayName`
- `PublisherDisplayName`
- `Description`

Capabilities currently included:

- `internetClient` (required for cloud sync)
- `runFullTrust` (required for packaged WPF app)

## 4. Required icon assets (important)

Current `Assets/*.png` are placeholders only. Replace with production assets before upload.

At minimum, provide valid images for:

- `StoreLogo.png`
- `Square44x44Logo.png`
- `Square150x150Logo.png`
- `Wide310x150Logo.png`
- `Square310x310Logo.png`
- `SplashScreen.png`

Also prepare all Store-required scale variants (100/150/200/400) in final package workflow.

## 5. Build and publish workflow

### Desktop app build

```powershell
dotnet build src/desktop/WordsNote.Desktop/WordsNote.Desktop.csproj -c Release -r win-x64
dotnet build src/desktop/WordsNote.Desktop/WordsNote.Desktop.csproj -c Release -r win-arm64
```

### Publish optimization profiles

```powershell
dotnet publish src/desktop/WordsNote.Desktop/WordsNote.Desktop.csproj -p:PublishProfile=win-x64-store
dotnet publish src/desktop/WordsNote.Desktop/WordsNote.Desktop.csproj -p:PublishProfile=win-arm64-store
```

### MSIX packaging

Use Visual Studio (recommended) for WAP/MSIX packaging:

1. Open `src/desktop/WordsNote.Desktop.sln`
2. Set `WordsNote.Package` as startup packaging project
3. Choose `Release | x64` (and later `Release | ARM64`)
4. Create/attach temporary test certificate for local sideload testing
5. Build package and validate install/update/uninstall flow

Note:

- `dotnet msbuild` cannot build `.wapproj` unless Desktop Bridge targets are installed.
- If you see missing `Microsoft.DesktopBridge.props`, install Visual Studio workload for Windows app packaging and build from Visual Studio.

## 6. Known constraints and best practices

- Store-packaged apps must not write to installation directory.
- Keep all writable data in `%LocalAppData%` (already implemented for desktop data/settings).
- Trimming in WPF can affect reflection-based features. Monitor warnings from publish output and add trim descriptors if needed.
- Current Store publish profiles are validated with clean output in this workspace after applying hardening plus controlled suppression.
- Suppression scope is limited to Store publish profiles to keep day-to-day local build diagnostics unaffected.
- For real Store submission, Microsoft replaces your local test certificate with Store signing.

## 7. Tomorrow quick checklist (recommended order)

Use this exact sequence when you try again tomorrow.

1. Prepare Partner Center app identity first

- Create app in Partner Center and reserve app name.
- Copy exact identity values from Partner Center into `src/desktop/WordsNote.Package/Package.appxmanifest`:
  - `Identity Name`
  - `Identity Publisher`
  - `DisplayName`
  - `PublisherDisplayName`
  - `Description`

Current placeholders still need replacement:

- `Name="com.yourcompany.wordsnote"`
- `Publisher="CN=YOUR_TEST_CERTIFICATE"`
- `PublisherDisplayName>YOUR_PUBLISHER_NAME<`

2. Replace all package images

- Replace all files under `src/desktop/WordsNote.Package/Assets/` with production assets.
- Keep naming exactly as referenced in manifest.

3. Build Store package from Visual Studio

- Open `src/desktop/WordsNote.Desktop.sln`.
- Right-click `WordsNote.Package` -> `Publish` -> `Create App Packages...`.
- Choose `Microsoft Store` and sign in to Partner Center account.
- Choose architectures: `x64` + `ARM64`.
- Build `Release` package and produce `.msixbundle`.

4. Validate package before submission

- Run WACK (Windows App Certification Kit) on generated package.
- Smoke test install/update/uninstall on clean machine/VM.

5. Submit in Partner Center

- Upload `.msixbundle`.
- Fill Store listing content (description/screenshots/privacy URL/category/age rating).
- Submit for certification.

## 8. About current local build lock (`vgc` process)

The failure you saw (`MSB3027`, `MSB3021`) is file lock on `WordsNote.Desktop.exe`, not code compile failure.

Current project is already adjusted for local Debug convenience:

- `src/desktop/WordsNote.Desktop/WordsNote.Desktop.csproj` sets `UseAppHost=false` in Debug.

Implication:

- `dotnet build` and `dotnet run` can still work with DLL output even if old EXE is locked.
- Store packaging should be done in `Release` via Visual Studio packaging workflow (not affected the same way as this local debug lock path).

If lock noise is annoying, close old app instances or reboot before packaging.

## 9. What Copilot can do for you next

I can prepare these items before you submit:

- Fill `Package.appxmanifest` with your real Partner Center identity values.
- Verify all required logo files exist and match manifest references.
- Add a release checklist + version bump checklist (`desktop + package`) before each Store upload.
- Review generated package metadata and common Store rejection risks.

## 10. Current Partner Center values (provided)

Use these exact values for Store packaging and submission tracking:

- Package Identity Name: `ThinHN.Wordsnote`
- Package Identity Publisher: `CN=...`
- PublisherDisplayName: `ThiệnHN`
- Package Family Name (PFN): `ThinHN.Wordsnote_x6hx...`
- Package SID: `S-1-15-2-11....`
- Store ID: `9NLP...`
- Store deep link: available after product is live
- Web Store URL: available after product is live

## 11. Exact PowerShell commands (copy/paste)

Important:

- For `.wapproj`, use Visual Studio MSBuild, not `dotnet build`.
- In PowerShell, path containing spaces/parentheses must be called with `&` and wrapped in quotes.

Correct command:

```powershell
Set-Location e:\workspace\srcPrj\WordsNote\src\desktop\WordsNote.Package

& "D:\Programs (x86)\Visual_Studio_2022\MSBuild\Current\Bin\MSBuild.exe" \
  ".\WordsNote.Package.wapproj" \
  /restore \
  '/p:Configuration=Release' \
  '/p:Platform=x64' \
  '/p:UapAppxPackageBuildMode=StoreUpload' \
  '/p:AppxBundle=Always' \
  '/p:AppxBundlePlatforms=x64|arm64' \
  '/p:AppxPackageDir=e:\workspace\srcPrj\WordsNote\src\desktop\WordsNote.Package\AppPackages\'
```

Expected output artifact:

- `src/desktop/WordsNote.Package/AppPackages/WordsNote.Package_1.1.3.0_x64_arm64_bundle.msixupload`

## 12. Partner Center validation pitfalls (already hit)

1. Version format rule

- Store rejects app manifest versions where `Revision` (4th segment) is not `0`.
- Use format like: `1.1.1.0`, `1.1.2.0`, `1.2.0.0`.
- Avoid: `1.1.0.1`.

2. Restricted capability warning

- `runFullTrust` appears as warning in package validation and requires Microsoft approval in certification workflow.
- For packaged WPF desktop app (`Windows.FullTrustApplication`), this capability is expected.
- Keep capability, and provide clear justification in Store listing/submission notes when requested.

## 13. If Partner Center still reports old DisplayName

Use this recovery flow to eliminate stale package metadata from draft submissions:

1. Increase package version (keep revision at 0)

- Example: `1.1.1.0 -> 1.1.2.0`.

2. Clean all package outputs before build

- Remove these folders under `src/desktop/WordsNote.Package`: `bin`, `obj`, `AppPackages`, `BundleArtifacts`.

3. Rebuild StoreUpload with MSBuild command in section 11.

4. Verify metadata inside generated `.msixupload` before uploading

- Confirm in packaged `AppxManifest.xml`:
  - `Identity Name="ThinHN.Wordsnote"`
  - `DisplayName="Words note"`
  - `uap:VisualElements DisplayName="Words note"`
  - `ShortName="Words note"`

5. In Partner Center submission draft

- Remove old package rows that failed validation.
- Upload only the newest `.msixupload` artifact.
- Re-run validation.

## 14. Store listing EN-US (copy/paste template)

Use this section for Partner Center -> Store listing -> English (United States).

Required now:

- Product name
- Description
- At least 1 screenshot (PNG)

Can be left blank if not available:

- What's new in this version (recommended, but optional)
- Product features (recommended, but optional)
- Trailers and all trailer-related images
- Xbox-only art fields
- Supplemental fields (Short title, Voice title)
- Additional license terms

### Product name (required)

Words note

### Description (required)

Words note is a vocabulary and flashcard learning app built for focused daily study.

Create and organize your own word collections, add cards quickly, and review with three learning modes:

- Flashcards for quick recall
- Learn mode for typed-answer practice
- Practice mode for multiple-choice drills

The app supports local-first usage, so you can build and review your collections even without signing in. When you choose to sign in with Google, you can sync your data between local storage and cloud to continue learning across sessions.

Words note is designed for language learners, students, and anyone who wants a simple but effective tool to memorize terms, definitions, and phrases.

### What's new in this version (optional)

Improved Store package validation and publishing reliability.
Updated package identity and metadata alignment for Microsoft Store submission.
General quality fixes and preparation updates for the desktop release.

If this is your first release, you can leave this field blank.

### Product features (optional but recommended)

- Create and manage custom vocabulary collections
- Study with Flashcards, Learn, and Practice modes
- Type answers with normalized text comparison
- Import cards quickly from text
- Local-first experience for offline-friendly usage
- Optional Google sign-in for cloud sync

### Keywords (optional but recommended)

Use up to 7 keywords. Suggested set:

- vocabulary
- flashcards
- language learning
- memorization
- study
- spelling
- practice

### Copyright and trademark info (optional)

Copyright (c) 2026 ThienHN. All rights reserved.

### Developed by (recommended)

ThienHN

### Screenshots (required)

Minimum to pass: upload at least 1 PNG screenshot.

Recommended first 4 screenshots (Windows desktop):

1. Home/Landing screen
2. Learn mode (typed answer)
3. Practice mode (multiple choice)
4. Manage collections/cards screen

Technical constraints:

- PNG only
- 1366x768 or larger recommended
- Max 50 MB each

File naming suggestion:

- wn-01-home.png
- wn-02-learn.png
- wn-03-practice.png
- wn-04-manage.png

## 15. Submission options and certification text (copy/paste)

Use this section for Partner Center -> Submission options.

### Publishing hold options (which one to choose)

Recommended for first release:

- Select: `Don't publish this submission until I select Publish now`

Why:

- You can wait for `Certification passed` and then release manually when ready.
- Avoid accidental auto-release while you are still checking listing quality.

If you have an exact launch time, use:

- `Start publishing this submission on <date/time UTC>`

Only use:

- `Publish this submission as soon as it passes certification`

when you are fully ready for immediate release.

### Notes for certification (optional)

Paste this into certification notes (or Additional Testing Information when requested):

This is a packaged WPF desktop vocabulary learning app.

No special hardware is required.
No test account is required to validate core functionality.

Core validation path:
1. Launch app to Home screen.
2. Open Learn workspace and verify Flashcards, Learn, and Practice flows.
3. Open Manage workspace and create/edit collections and cards in local mode.
4. Optional: sign in with Google to validate cloud sync actions.

If Google sign-in is not used, local-first learning and management flows remain fully testable.

### Notes for certification (10.3.1 remediation template)

Use this template when Partner Center reports `10.3.1 App Is Testable - Test Account`:

Product ID: <YOUR_PRODUCT_ID>

Core app functionality does not require account creation or sign-in.

Primary no-account test path:
1. Launch app and open `Manage`.
2. Create/edit/delete collections and cards in local mode.
3. Open `Learn` and verify Flashcards, Learn (typed answer), and Practice modes.

Google login is optional and only required for cloud sync features.
Even when cloud login is unavailable, local mode remains fully usable and testable.

No test account credentials are required for certification of core functionality.

### Restricted capability approval: runFullTrust (required)

Paste this in:
`Why do you need the runFullTrust capability, and how will it be used in your product?`

Words note is a desktop WPF application packaged for Microsoft Store. It uses the `runFullTrust` capability to run as a full-trust Win32 desktop process (`Windows.FullTrustApplication`), which is required for the WPF runtime and desktop windowing model.

How it is used:
- Run the main WPF desktop UI and app process.
- Access user-scoped local app data under `%LocalAppData%` for settings and learning data.
- Perform normal HTTPS API calls for optional cloud sync.

How it is NOT used:
- No admin elevation or privileged system operations.
- No kernel drivers, services, or background system modification.
- No access outside normal user-scoped application behavior.

The capability is required only to support standard packaged desktop app execution and local user data functionality.
