# WordsNote Release Notes

## WordsNote v1.1.2 - Dynamic Download Page and Extension Version Sync

Release date: 20/04/2026

Scope summary:
- Extension version synchronized to `1.1.2`
- Added dynamic web download page for desktop app distribution
- Added authenticated email-based edit mode for download metadata

### Highlights

New in this release:
- Extension version bump:
  - Updated `src/extension/package.json` to `1.1.2`
  - Updated `src/extension/public/manifest.json` to `1.1.2`
  - Synced `src/extension/package-lock.json` version metadata
- Added a new frontend download route:
  - Route: `/download`
  - Page: `src/frontend/src/pages/AppDownloadPage.vue`
  - Navigation links added in top bar and landing quick actions
- Download page now loads data dynamically from GitHub Releases API:
  - Pulls latest release metadata and assets
  - Selects preferred installer asset (priority: `.msixbundle` -> `.msix` -> `.exe` -> `.zip`)
  - Displays release assets list with file size and download count
- Added authenticated edit mode based on signed-in user email:
  - Loads profile from `/api/user/me`
  - Enables edit controls only when a signed-in profile has an email
  - Optional restriction via `VITE_DOWNLOAD_EDITOR_EMAIL`
  - Editable values are persisted in browser local storage for quick admin updates

### Build Validation

Verified in current cycle:
- Frontend: `cd src/frontend && npm run build`
- Extension: `cd src/extension && npm run build`

### Notes

- The `/download` page is public for viewing and uses dynamic release data from GitHub.
- Edit mode is hidden for anonymous users.
- For desktop installer distribution on GitHub Release, upload `.msixbundle` for end users and keep `.msixupload` for Partner Center submission only.

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
