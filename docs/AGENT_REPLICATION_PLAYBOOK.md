# Agent Replication Playbook

This playbook helps an engineering agent reproduce the WordsNote architecture with consistent conventions.

Use with:

- `docs/ENVIRONMENT.md`
- `docs/API_REFERENCE.md`
- `docs/ADMIN_CONTENT_GUIDE.md`

## 1) Target Outcome

Replicate a monorepo that contains:

1. ASP.NET Core backend (`net8.0`) with MongoDB and JWT
2. Vue 3 + Vite + Pinia frontend web app
3. Chrome extension popup flow with no-auth local mode and optional cloud read-only sync

Core product domain:

- collections
- cards
- study sessions (flash, quick, deep)
- tests (mcq and written)

## 2) Deterministic Reading Order

### Phase A - Repository Orientation

Read:

1. `docs/ENVIRONMENT.md`
2. `docs/API_REFERENCE.md`
3. `docs/ADMIN_CONTENT_GUIDE.md`
4. `docs/WORDSNOTE_EXECUTION_PLAN.md`

Deliverable:

- one-page architecture summary

### Phase B - Backend Runtime

Read in order:

1. `src/backend/FeatureFusion/Program.cs`
2. `src/backend/FeatureFusion/Extensions/*.cs`
3. `src/backend/Infrastructure/Configuration/*.cs`
4. `src/backend/FeatureFusion/Controllers/AS/*.cs`
5. `src/backend/FeatureFusion/Controllers/WordsNote/*.cs`

Extract:

- middleware order
- auth behavior
- route contracts
- Mongo collection usage

### Phase C - Backend DTOs and Domain Models

Read:

1. `src/backend/Application/Dtos/WordsNote/*.cs`
2. `src/backend/Domain/Entities/WordsNote/*.cs`
3. helper/normalization utilities used for grading

Extract:

- payload contracts
- compatibility fields
- text normalization and grading rules

### Phase D - Frontend Runtime and State

Read in order:

1. `src/frontend/src/main.ts`
2. `src/frontend/src/router/index.ts`
3. `src/frontend/src/apis/apiClient.ts`
4. `src/frontend/src/apis/WordsNote/*.ts`
5. `src/frontend/src/stores/WordsNote/*.ts`
6. `src/frontend/src/pages/WordsNote/*.vue`
7. `src/frontend/src/pages/PrivacyPolicyPage.vue`

Extract:

- auth guard behavior
- endpoint mappings
- study/test UI flow

### Phase E - Extension Runtime

Read:

1. `src/extension/public/manifest.json`
2. `src/extension/src/popup/App.vue`
3. `src/extension/src/popup/components/*.vue`
4. `src/extension/src/services/*.js`
5. `src/extension/public/content.js`
6. `src/extension/public/background.js`

Extract:

- no-auth local behavior
- highlight capture workflow
- local storage model
- cloud read-only sync and sync-to-local flow
- collection-level filtering in popup

## 3) Implementation Blueprint

1. Create backend skeleton and auth.
2. Implement collections and cards CRUD.
3. Implement study APIs.
4. Implement test APIs.
5. Add written-answer normalization.
6. Build frontend management + study + test pages.
7. Build extension local inbox flow with optional cloud read-only sync.
8. Add compatibility layer (`desk/card`) if migrating from old clients.
9. Add tests and smoke checks.
10. Remove obsolete modules.

## 4) Mandatory Conventions

- Keep API naming consistent: `collections`, `cards`, `study`, `tests`.
- Keep old names only as temporary compatibility aliases.
- Keep controller methods thin; move reusable logic to helpers/services.
- Keep answer normalization deterministic across deep-study and written tests.
- Keep extension independent from backend auth token.

## 5) Guardrails

- Do not couple extension flow to web login state.
- Do not compare written answers with raw string equality.
- Do not remove compatibility routes before frontend/extension migration is complete.
- Do not mix `deckId`, `deskId`, and `collectionId` in new contracts.

## 6) Verification Runbook

Backend:

1. `dotnet build src/backend/FeatureFusion.sln`
2. run backend and verify auth + collections/cards/study/tests endpoints

Frontend:

1. `cd src/frontend`
2. `npm run build`
3. smoke test management/study/test routes

Extension:

1. `cd src/extension`
2. `npm run build`
3. load unpacked extension and verify no-auth popup flow
4. verify Cloud mode can read `GET /api/collections` and `GET /api/cards`
5. verify `Sync To Local` writes cloud cards into local mode dataset

## 7) Definition Of Done

Replication is complete when:

1. Web app can CRUD collections and cards.
2. Flash/quick/deep study modes are functional.
3. MCQ and written tests run end-to-end.
4. Written grading is diacritics-insensitive and case-insensitive.
5. Extension works without login, stores local data, and supports cloud read-only sync to local storage.
6. Compatibility aliases are documented and scheduled for cleanup.
7. Backend, frontend, and extension builds pass.