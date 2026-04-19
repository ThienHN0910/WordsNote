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
