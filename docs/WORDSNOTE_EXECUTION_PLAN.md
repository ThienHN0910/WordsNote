# WordsNote Execution Plan (BE + FE + Chrome Extension)

## Execution Status (2026-04-17)

- Completed: Phase 1 (documentation rewrite)
- Completed: Phase 2 core backend rollout (collections/cards aliases, study APIs, tests APIs, answer normalization)
- Completed: Phase 3 partial frontend rollout (new API contracts for study hub/session)
- Completed: Phase 4 core extension rollout (no-auth local storage workflow)
- Completed: Phase 5 hardening and validation (unit tests, multi-app builds, backend runtime smoke test)
- Completed: Phase 6 cleanup and UX redesign finalization

Latest finalization scope:

- New route architecture:
  - `/` landing page
  - `/learn` public learning lab (flashcards, learn, practice without auth)
  - `/manage` authenticated collection/card workspace
- Google-only login for management workspace
- Register flow disabled
- Removal of unused frontend legacy pages/components/modules
- Documentation synced with final product behavior
- Extension popup aligned with Learn-only scope (flashcards, learn, practice); no homepage/manage flow in extension

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
- Frontend already has Study Hub and Study Session pages.
- Extension runs local no-auth flow with popup Learn modes and highlighted-text capture.

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
- Optional import/export with web app can be added later (not required in first delivery).

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

## 6. Immediate Next Action (recommended)

Start with Phase 1 now:
1. Rewrite docs/ENVIRONMENT.md for .NET backend + Vue frontend + extension.
2. Rewrite docs/API_REFERENCE.md with collections/cards/study/tests endpoints.
3. Rewrite docs/ADMIN_CONTENT_GUIDE.md into WordsNote workflow guide.
4. Rewrite docs/AGENT_REPLICATION_PLAYBOOK.md for this .NET architecture.

After docs are approved, execute Phase 2 API refactor in small PR-style batches.
