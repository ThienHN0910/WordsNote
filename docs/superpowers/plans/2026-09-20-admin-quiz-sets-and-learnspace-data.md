# Admin Quiz Sets Management & Learnspace Data Enrichment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expand the WordsNote Quiz system with Learnspace curriculum question banks (PRN232, ITE302c, HCM202) and full Admin Quiz Set CRUD & lock/unlock management.

**Architecture:** ASP.NET Core Web API controller endpoints protected by Google OAuth Admin JWT claims (`hnt.vn.vn@gmail.com`), MongoDB collections `wordsnote_quiz_sets` and `wordsnote_questions`, Vue 3 frontend tab in `AdminModal.vue` with interactive lock toggle and modal dialogs.

**Tech Stack:** ASP.NET Core 8, MongoDB C# Driver, xUnit, Vue 3, TypeScript, Pinia, Axios.

**Spec:** `docs/superpowers/specs/2026-09-20-admin-quiz-sets-and-learnspace-data-design.md`

## Global Constraints
- Zero credential leakage: No secrets or raw API keys in source, config, or commits.
- Branching: Commit to `feat/admin-quizsets-and-learnspace-data`.
- Admin email strictly `hnt.vn.vn@gmail.com`.
- Single backend publish to `runasp.net` at the very end after complete local verification.

---

### Task 1: Learnspace Question Bank Ingestion & Catalog Registration

**Files:**
- Create: `scripts/import_quiz_banks.mjs`
- Create: `src/backend/FeatureFusion/Data/QuizBanks/prn232.json`
- Create: `src/backend/FeatureFusion/Data/QuizBanks/ite302c.json`
- Create: `src/backend/FeatureFusion/Data/QuizBanks/hcm202.json`
- Modify: `src/backend/FeatureFusion/Data/QuizBanks/catalog.json`

- [ ] **Step 1: Write ingestion script**
Fetch datasets for PRN232, ITE302c, and HCM202, format into `QuestionDocument` structure, and write to `src/backend/FeatureFusion/Data/QuizBanks/`.

- [ ] **Step 2: Run ingestion script and verify generated JSON files**
Verify questions counts: PRN232 (150 questions), ITE302c (1060 questions), HCM202 (900 questions).

- [ ] **Step 3: Update catalog.json**
Add PRN232, ITE302c, and HCM202 to `catalog.json` subjects array.

- [ ] **Step 4: Commit Task 1**
```bash
git add scripts/import_quiz_banks.mjs src/backend/FeatureFusion/Data/QuizBanks/
git commit -m "feat(quiz): import question banks for PRN232, ITE302c, and HCM202"
```

---

### Task 2: Backend Admin Quiz Sets Endpoints & Catalog Sync Hygiene

**Files:**
- Modify: `src/backend/FeatureFusion/Controllers/WordsNote/QuizSetsController.cs`
- Modify: `src/backend/Application/Dtos/WordsNote/QuizBankDTOs.cs` (if needed for Admin DTOs)
- Test: `src/backend/FeatureFusion.Tests/QuizBankTests.cs`

- [ ] **Step 1: Add unit tests for Admin Quiz Set CRUD and Restriction Toggle**
Test that non-admin receives 403 Forbidden, and admin can create, update, toggle restriction, and delete quiz sets.

- [ ] **Step 2: Implement Admin endpoints in QuizSetsController.cs**
Implement `GET /api/admin/quiz-sets`, `POST /api/admin/quiz-sets`, `PUT /api/admin/quiz-sets/{id}`, `PATCH /api/admin/quiz-sets/{id}/restriction`, and `DELETE /api/admin/quiz-sets/{id}`.
Adjust `SyncCatalogMetadataAsync` so it only inserts missing quiz sets from `catalog.json` without overriding admin-modified properties in MongoDB.

- [ ] **Step 3: Run backend unit tests to verify all tests pass**
```bash
dotnet test src/backend/FeatureFusion.Tests/FeatureFusion.Tests.csproj
```

- [ ] **Step 4: Commit Task 2**
```bash
git add src/backend/
git commit -m "feat(backend): implement admin quiz sets CRUD and restriction toggle"
```

---

### Task 3: Frontend Admin Quiz Sets Management UI & API Client

**Files:**
- Modify: `src/frontend/src/apis/WordsNote/QuizAPI.ts`
- Modify: `src/frontend/src/views/Quiz/components/AdminModal.vue`

- [ ] **Step 1: Add Quiz Sets Admin API methods to QuizAPI.ts**
Add `getAdminQuizSets`, `createQuizSet`, `updateQuizSet`, `toggleQuizSetRestriction`, `deleteQuizSet`.

- [ ] **Step 2: Implement "Quản lý Bộ câu hỏi" Tab in AdminModal.vue**
Add the 3rd tab, list table/cards with color indicators, question counts, one-click Lock/Unlock toggle switch, and modal dialog for Create/Edit/Delete.

- [ ] **Step 3: Run frontend typecheck and build**
```bash
npm run build
```

- [ ] **Step 4: Commit Task 3**
```bash
git add src/frontend/
git commit -m "feat(frontend): add quiz sets CRUD and lock/unlock toggle to admin modal"
```

---

### Task 4: Backend Release Packaging & Single Publish Coordination

- [ ] **Step 1: Build .NET Release Package**
```bash
dotnet publish src/backend/FeatureFusion/FeatureFusion.csproj -c Release -o src/backend/FeatureFusion/bin/Release/net8.0/publish
```

- [ ] **Step 2: Ask user to publish backend to runasp.net and wait for confirmation**

- [ ] **Step 3: Live Verification using Chrome DevTools MCP**
Verify `/quiz` catalog displays new subjects (PRN232, ITE302c, HCM202), and admin portal allows toggling lock status and creating/editing quiz sets.

- [ ] **Step 4: Create Pull Request and Merge to Main**
```bash
gh pr create ...
gh pr merge ...
```
