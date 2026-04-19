# WordsNote Release Notes

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
