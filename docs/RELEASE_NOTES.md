# WordsNote Release Notes

## WordsNote v1.1.2 - Multi-Version Download Center and Shared Config Sync

Release date: 20/04/2026

Baseline compared: `v1.1.0` -> `v1.1.2`

Scope summary:
- Added backend download-config API for shared editor persistence
- Added per-version manual custom links in download editor
- Download page now supports multiple versions and multiple links per version
- Added explicit download CTA fields (Microsoft Store + Edge Add-ons)
- Hardened save/reset and manage mutations against duplicate submit clicks
- Extension metadata synchronized to `1.1.2`

### Highlights

Added features include:
- Introduced dynamic desktop download page on web:
  - Route: `/download`
  - Page: `src/frontend/src/pages/AppDownloadPage.vue`
  - Pulls release metadata and assets from GitHub Releases API
  - Supports selecting among multiple release versions
  - Displays multiple download links per version
- Added authenticated email-based edit mode for download metadata and links:
  - Loads signed-in profile from `/api/user/me`
  - Keeps download page public for anonymous users
  - Enables edit controls for admin account only
  - Supports custom manual links per version (outside GitHub assets)
- Added explicit CTA link fields in shared config:
  - `appStoreUrl`
  - `edgeAddonsUrl`
- Download page UX split:
  - GitHub release assets shown in version channel
  - Manual/store links shown in dedicated quick-access/channel sections
- Added shared backend persistence for download page config:
  - Public read endpoint: `GET /api/download-config`
  - Authenticated write endpoint: `PUT /api/download-config`
  - Authenticated reset endpoint: `DELETE /api/download-config`
  - Uses Mongo collection `wordsnote_download_page_configs` so all devices see the same config
- Synced extension release version to `1.1.2`:
  - Updated `src/extension/package.json` to `1.1.2`
  - Updated `src/extension/public/manifest.json` to `1.1.2`
  - Synced `src/extension/package-lock.json` version metadata

Improved web behavior includes:
- Added direct navigation to download experience from top bar and landing quick actions.
- Kept download page public for readers while restricting edit/reset controls to admin.
- Fixed web auth persistence flow so refresh does not unexpectedly clear valid login session.
- Fixed `400 Bad Request` on download-config save by sending sanitized payload (exclude server-managed metadata fields).
- Added pending-state UI locks on manage/download mutation actions to prevent accidental duplicate creates/updates.

Documentation updates include:
- Updated release notes with a unified release-note structure aligned to previous release format.

### Breaking/Behavior Notes

- Download page metadata overrides are now backend-shared (Mongo), not local-browser scoped.
- GitHub Releases API availability affects dynamic metadata loading on `/download`.
- Download config write/reset APIs require admin privileges.
- Download config now stores explicit CTA URLs (`appStoreUrl`, `edgeAddonsUrl`) instead of deriving CTA links heuristically from manual assets.
- For distribution, `.msixbundle` is end-user installer artifact while `.msixupload` remains Partner Center submission artifact.

### Build Validation

Verified build commands in current cycle:
- Frontend: `cd src/frontend && npm run build`
- Backend: `cd src/backend && dotnet build FeatureFusion.sln`
- Extension: `cd src/extension && npm run build`

### Release Assets (Suggested Packaging)

Suggested artifacts:
- `release/v1.1.2/wordsnote-extension-v1.1.2.zip`
- `release/v1.1.2/WordsNote.Package_1.1.2.0_x64_arm64.msixbundle`
- `release/v1.1.2/SHA256SUMS.txt`
- `release/v1.1.2/RELEASE_NOTES.md`

### Full Commit List (since v1.1.0)

- (Fill this section after creating commits/tag for v1.1.2)

## WordsNote v1.1.0 - Desktop Parity, Local-First Manage, Web Auth/Learn Stabilization

Release date: 19/04/2026

Baseline compared: `v1.0.1` (`ba96f80`) -> `main` (`f741786`)

Scope summary:
- 5 commits on `main` since `v1.0.1`
- 60 files changed
- 6373 insertions, 104 deletions

### Highlights

Added features include:
- Introduced full WPF desktop app (`src/desktop/WordsNote.Desktop`) with parity workspaces:
  - Landing
  - Learn (Flashcards, Learn, Practice)
  - Manage (local-first CRUD/import/filter/sort)
  - Privacy (VI/EN)
  - Dedicated Login flow for optional cloud mode
- Added desktop cloud sync capabilities:
  - Sync Local -> Cloud
  - Sync Cloud -> Local
  - Browser-based Google login with ID token fallback
- Added MSIX packaging project (`src/desktop/WordsNote.Package`) with Store-prep documentation and trim-hardening configuration.
- Extended extension popup with local-first Manage workspace and local card management/import workflow.

Improved web behavior includes:
- Stabilized auth persistence after refresh by rehydrating token state from persistent storage.
- Preserved local-first access for `/manage` while keeping `/manage/:deckId/session` authenticated.
- Updated Learn route to remain usable without login by preferring public cloud reads and falling back to local data.
- Updated landing/learn copy to align desktop and web product language.

Documentation updates include:
- Expanded architecture and execution docs for desktop rollout and MSIX packaging.
- Updated API/replication/environment docs to match local-first and optional-auth behavior.

### Breaking/Behavior Notes

- Product access model is explicitly local-first for management on web/desktop when not authenticated.
- Focused session and deep-study APIs remain cloud-authenticated flows.
- Extension host permissions continue to target `words-note.runasp.net` by default.

### Build Validation

Verified build commands in current cycle:
- Frontend: `cd src/frontend && npm run build`
- Extension: `cd src/extension && npm run build`
- Desktop: `dotnet build src/desktop/WordsNote.Desktop.sln`

### Release Assets (Local Packaging)

Prepared artifacts:
- `release/v1.1.0/wordsnote-extension-v1.1.0.zip`
- `release/v1.1.0/SHA256SUMS.txt`
- `release/v1.1.0/RELEASE_NOTES.md`

### Full Commit List (since v1.0.1)

- `f741786` feat: enhance authentication flow and web parity stabilization; update UI for local-first experience
- `3d59af3` Add MSIX packaging for WordsNote application
- `d282cf5` feat: implement local management for study decks and cards
- `5e13a77` Add MainViewModel and project file for WordsNote Desktop application
- `43c25bd` Add: readme

### Previous Release

- `v1.0.1` (17/04/2026): Learn-only extension refresh, cloud read-only sync to local, bilingual privacy policy.
