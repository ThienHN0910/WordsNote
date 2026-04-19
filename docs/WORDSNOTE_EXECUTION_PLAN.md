# WordsNote Execution Plan (BE + FE + Extension + Desktop)

## Execution Status (2026-04-17)

- Completed: Phase 1 (documentation rewrite)
- Completed: Phase 2 core backend rollout (collections/cards aliases, study APIs, tests APIs, answer normalization)
- Completed: Phase 3 partial frontend rollout (new API contracts for study hub/session)
- Completed: Phase 4 core extension rollout (no-auth local storage workflow)
- Completed: Phase 5 hardening and validation (unit tests, multi-app builds, backend runtime smoke test)
- Completed: Phase 6 cleanup and UX redesign finalization
- Completed: Phase 7 desktop WPF rollout (landing, learn, manage, privacy + local/cloud sync)
- Completed: Phase 8 web parity hotfix (auth persistence, public learn resilience, desktop-web copy alignment)

Latest finalization scope:

- New route architecture:
  - `/` landing page
  - `/learn` public learning lab (flashcards, learn, practice without auth)
  - `/privacy-policy` public bilingual policy page (`?lang=vi|en`)
  - `/manage` local-first collection/card workspace (no login required)
    - Card UX is create-only form plus popup edit from card actions
  - `/manage/:deckId/session` authenticated focused session route
- Google login is optional and used for cloud-backed session/deep-study actions
- Register flow disabled
- Removal of unused frontend legacy pages/components/modules
- Documentation synced with final product behavior
- Extension popup aligned with local-first scope:
  - Learn Lab (flashcards, learn, practice)
  - Manage Lab (local collection/card CRUD + local text import)
- Extension supports Cloud read-only sync, collection filtering, and `Sync To Local` workflow
- New desktop app under `src/desktop/WordsNote.Desktop` with WPF UI and feature parity with active web frontend flows:
  - Landing workspace
  - Dedicated Login workspace
  - Learn workspace (Flashcards, Learn, Practice)
  - Privacy workspace (VI/EN) opened via landing link/button
  - Manage workspace (local-first CRUD/import/filter/sort + Google cloud mode)
  - Card UX mirrors web manage: create-only form and popup edit via Edit Selected
  - Browser-based Google login flow plus ID token fallback
  - Sync Local -> Cloud and Sync Cloud -> Local

## 1. Target Product Scope

Goal: rebuild this repository into a Quizlet-like product with:
- Backend API (ASP.NET Core + MongoDB)
- Frontend web app (Vue 3 + Pinia)
- Chrome extension (no auth required)

Core product capabilities:
- CRUD collections and cards
- Flashcard learning flow
- Quick study mode and deep study mode
- Quiz mode:
  - Multiple choice (single answer and multi-answer)
  - Written response
- Written answer grading defaults:
  - Ignore Vietnamese diacritics
  - Case-insensitive

## 2. Current-State Gaps (from codebase vs docs)

Main mismatch:
- Current docs describe an old portfolio/CMS architecture and API set.
- Current codebase is already a WordsNote learning app foundation.

Confirmed implementation status:
- Backend already has Desk and Card controllers with JWT auth.
- Frontend has Landing/Learn/Manage flows and public Privacy Policy page with language query mode.
- Extension runs local no-auth flow with popup Learn modes, highlighted-text capture, cloud read-only fetch, and sync-to-local.

Naming and contract inconsistencies:
- Backend route names use singular `desk` and `card`.
- Some models/DTO use mixed naming (`DeskId` in storage, `DeckId` in API DTO).
- Legacy naming aliases remain during migration; preferred naming is `collections` and `cards`.

Legacy or unused artifacts to be cleaned later:
- Frontend placeholders and old modules (for example FFP/Supabase/template pages).
- Backend placeholder folders and stale config blocks.
- Docs still represent old product.

## 3. Guiding Design Decisions

Architecture conventions to keep:
- Monorepo style: src/backend, src/frontend, src/extension, docs
- Domain-first naming by feature (WordsNote)
- One frontend store per backend domain resource
- Thin controllers, domain services for study/test logic
- API response envelope consistency

Naming decision (important):
- Standardize on Collection and Card in API and UI text.
- Keep backward compatibility aliases for one migration phase (desk/deck mapping).

Extension decision (no auth):
- Extension runs in anonymous/local mode by default.
- Data persists in chrome.storage.local.
- Optional cloud read-only sync can fetch public collections/cards.
- Cloud cards can be synchronized into local storage for local study mode.

## 4. Execution Phases

### Phase 0 - Baseline and Safety

Tasks:
- Capture baseline build status for backend/frontend/extension.
- Create migration branch and rollback checkpoints.
- Add a migration checklist document for traceability.

Exit criteria:
- Baseline build log recorded.
- No unknown runtime blockers before refactor starts.

### Phase 1 - Rewrite Docs to Match New Product

Files to update first:
- docs/ENVIRONMENT.md
- docs/API_REFERENCE.md
- docs/ADMIN_CONTENT_GUIDE.md (rename concept to content/workflow for WordsNote)
- docs/AGENT_REPLICATION_PLAYBOOK.md

What to change:
- Replace old portfolio endpoints with WordsNote API contracts.
- Define module boundaries for BE/FE/extension.
- Document auth scope:
  - Web app: JWT auth
  - Extension: no auth
- Document learning and testing behaviors.

Exit criteria:
- Docs describe only WordsNote product.
- API and workflow docs are consistent with planned implementation.

### Phase 2 - Backend API Refactor (Contract-First)

2.1 Resource APIs
- Collections:
  - GET /api/collections
  - POST /api/collections
  - PUT /api/collections/{id}
  - DELETE /api/collections/{id}
- Cards:
  - GET /api/cards?collectionId=
  - POST /api/cards
  - PUT /api/cards/{id}
  - DELETE /api/cards/{id}
  - POST /api/cards/import

2.2 Study APIs
- GET /api/study/quick?collectionId=
- POST /api/study/review (difficulty grading)
- GET /api/study/deep/session?collectionId=
- POST /api/study/deep/answer

2.3 Test APIs
- POST /api/tests/mcq/start
- POST /api/tests/mcq/submit
- POST /api/tests/written/start
- POST /api/tests/written/submit

2.4 Written answer normalization
- Add a shared text normalizer utility:
  - Unicode normalize form D
  - Remove combining marks (diacritics)
  - Lowercase
  - Trim and collapse spaces
- Compare normalized expected vs actual answer.

2.5 Migration/compatibility
- Keep temporary adapters for existing desk/card payloads.
- Return deprecation headers for old routes.

Exit criteria:
- New API contracts implemented and tested.
- Old routes still work during migration window.

### Phase 3 - Frontend Refactor to New Contracts

Tasks:
- Replace StudyAPI endpoints with new collections/cards/study/tests routes.
- Rename store/domain concepts from desk/deck ambiguity to collection.
- Build separate UI flows:
  - Collection/Card management page
  - Flashcard mode
  - Quick study mode
  - Deep study mode
  - MCQ test mode
  - Written test mode
- Add UI feedback for written grading rule (accent-insensitive, case-insensitive).

Exit criteria:
- All core features operate on new API.
- No frontend call remains to deprecated routes.

### Phase 4 - Chrome Extension (No Auth Mode)

Tasks:
- Remove login form and token storage from extension.
- Replace with anonymous local workflow:
  - Save highlighted text to local Inbox collection
  - Review due cards in popup
- Decouple extension API service from authenticated backend calls.
- Keep extension independent from web app auth state.

Exit criteria:
- Extension works out-of-the-box after install, no sign-in required.

### Phase 5 - Testing and Hardening

Tasks:
- Backend unit tests:
  - normalizer behavior
  - MCQ scoring
  - written grading
  - study interval updates
- Integration tests:
  - collection/card CRUD
  - study flow end-to-end
- Frontend smoke tests for major routes.
- Manual QA checklist for extension behavior.

Exit criteria:
- All critical flows pass test checklist.

### Phase 6 - Cleanup and File Removal

Remove only after feature parity is confirmed:
- Frontend old placeholders and stale modules (FFP/template remnants, unused Supabase client if not needed).
- Backend unused folders/interfaces/services not referenced by WordsNote runtime.
- Obsolete docs and stale config examples.

Rules for cleanup:
- Search usages before deletion.
- Delete in small batches.
- Build and smoke test after each batch.

Exit criteria:
- Repository contains only active product modules.
- Build/test pipeline remains green.

### Phase 7 - Desktop WPF Parity App

Tasks:
- Create standalone desktop solution at `src/desktop/WordsNote.Desktop.sln`.
- Implement shared API client against existing backend contracts.
- Implement desktop screens equivalent to active web frontend scope:
  - Landing
  - Learn Lab (flash, learn typing, practice MCQ)
  - Manage (auth + collection/card management + import + filters)
  - Session (flash review and quiz answer check)
  - Privacy policy (VI/EN)
- Validate desktop build and fix compile/runtime integration issues.
- Update docs (`README.md`, `docs/ENVIRONMENT.md`, `docs/API_REFERENCE.md`) for desktop setup and behavior.

Exit criteria:
- Desktop app builds successfully with `dotnet build src/desktop/WordsNote.Desktop.sln`.
- Core flows from web frontend are available in desktop client.
- Documentation includes desktop architecture and run instructions.

Status:
- Completed on 2026-04-18 in current execution cycle.

## 5. Suggested Delivery Sprints

Sprint 1:
- Phase 1 + API contract draft from Phase 2

Sprint 2:
- Phase 2 implementation (resources + study + tests)

Sprint 3:
- Phase 3 frontend alignment + core UX

Sprint 4:
- Phase 4 extension no-auth migration

Sprint 5:
- Phase 5 hardening + Phase 6 cleanup

Sprint 6:
- Phase 7 desktop WPF parity rollout

Sprint 7:
- Phase 8 web auth/learn parity stabilization

## 5.1 Local-First Alignment Plan (Desktop + Web + Extension)

Objective:
- Keep manage workflows available without login on every client.
- Keep cloud-dependent actions explicit and opt-in.

Execution steps:
1. Desktop UX split
  - Separate Login workspace from Manage workspace.
  - Hide Privacy tab from main tabs and open via Landing action.
2. Frontend local-first manage
  - Open `/manage` without auth guard.
  - Persist local collections/cards in browser local storage when not authenticated.
  - Keep `/manage/:deckId/session` authenticated for cloud-backed session actions.
3. Extension local-first manage
  - Add Manage popup workspace with local collections/cards CRUD and local text import.
  - Keep no-auth behavior and local storage persistence.
4. Docs and verification
  - Update product docs to match local-first/manage behavior.
  - Run frontend, extension, and desktop build validation.

Status:
- Completed in current execution cycle (2026-04-18).

## 5.2 Web Parity Stabilization Plan (2026-04-19)

Objective:
- Keep web behavior consistent with desktop local-first model while preserving optional cloud auth workflows.

Plan:
1. Auth persistence hardening
  - Persist auth token in browser local storage instead of session storage.
  - Rehydrate auth state at startup and in route guards.
  - Migrate legacy session-storage token payload when available.
2. Public learn resilience
  - Add Learn-specific load path that first attempts public cloud reads (`GET /api/collections`, `GET /api/cards`).
  - Fallback to local manage snapshot when cloud read is unavailable.
3. Safe unauthorized handling
  - Redirect to login on `401` only for protected API intents.
  - Avoid forcing login redirect for public-read learn requests.
4. Desktop-web UI/copy alignment
  - Update landing and learn copy to reflect local-first + optional-login product behavior.

Execution result:
- Completed in current execution cycle (2026-04-19).
- Implemented files:
  - `src/frontend/src/stores/AS/AuthStore.ts`
  - `src/frontend/src/main.ts`
  - `src/frontend/src/router/index.ts`
  - `src/frontend/src/apis/apiClient.ts`
  - `src/frontend/src/stores/WordsNote/StudyStore.ts`
  - `src/frontend/src/pages/WordsNote/LearnLabPage.vue`
  - `src/frontend/src/pages/WordsNote/StudyHubPage.vue`
  - `src/frontend/src/pages/AS/LoginPage.vue`
  - `src/frontend/src/pages/LandingPage.vue`

Validation:
1. Frontend build command:
  - `cd src/frontend && npm run build`
2. Smoke scenarios:
  - Login then refresh keeps authenticated state.
  - Anonymous Learn route can still load data via public cloud read; local fallback remains available.
  - Anonymous Manage remains local-first; focused session route still requires auth.

## 6. Immediate Next Action (recommended)

Current next action recommendation:
1. Add optional desktop UI coverage for backend Tests APIs (`/api/tests/mcq/*`, `/api/tests/written/*`).
2. Add desktop integration tests for API client and answer normalization behavior.
3. Package desktop release profile (self-contained + signing strategy).
