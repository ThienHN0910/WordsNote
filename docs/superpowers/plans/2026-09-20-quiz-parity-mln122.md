# Quiz Platform Parity with MLN122_FE Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement full feature and UI parity between `/quiz` in WordsNote and `E:\workspace\srcPrj\MLN122_FE`, including interactive study workspace, 16-character single-use unlock code access control for restricted subjects (JFE301 & JIT401), and full admin key/user management.

**Architecture:** ASP.NET Core 8 Web API + MongoDB Driver on the backend providing catalog, questions with 403 authorization guard, key generation/redemption, and admin endpoints. Vue 3 + TypeScript + Pinia + Bootstrap 5 on the frontend orchestrating a 2-column workspace (`HeaderNavbar`, `QuizControls`, `QuestionCard`, `SideQuestionMap`) with interactive drawers/modals (`SearchDrawer`, `UnlockModal`, `AdminModal`, `GoogleLoginModal`).

**Tech Stack:** C# .NET 8, MongoDB.Driver, Vue 3, Vite, TypeScript, Pinia, Bootstrap 5, FontAwesome / Lucide icons.

**Spec:** [docs/superpowers/specs/2026-09-20-quiz-parity-mln122-design.md](file:///E:/workspace/srcPrj/WordsNote/docs/superpowers/specs/2026-09-20-quiz-parity-mln122-design.md)

## Global Constraints
- Zero-leakage security: No hardcoded credentials, connection strings, or API secrets.
- Branch convention: `feat/quiz-parity-mln122`.
- Conventional commits: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`.
- **Backend Publish Checkpoint:** When backend code changes are completed and tested locally, pause and notify user to publish backend to `words-note.runasp.net` before proceeding to final live verification.

---

### Task 1: Backend Domain Models & Catalog Configuration

**Files:**
- Create: `src/backend/Domain/Entities/WordsNote/UnlockKeyDocument.cs`
- Modify: `src/backend/Domain/Entities/AS/User.cs`
- Modify: `src/backend/FeatureFusion/Data/QuizBanks/catalog.json`

**Interfaces:**
- Produces: `UnlockKeyDocument` entity with fields: `Id`, `Code`, `IsUsed`, `UsedAt`, `UsedByEmail`, `TargetSubjects`, `CreatedAt`.
- Modifies: `User` entity adding `UnlockedSubjects` (`List<string>`).
- Configures: `catalog.json` with `isRestricted: true` on `jfe301` and `jit401`, and `false` on `mln122` and `prm393`.

- [ ] **Step 1: Create `UnlockKeyDocument.cs`**
Create `src/backend/Domain/Entities/WordsNote/UnlockKeyDocument.cs` with BSON annotations and default target subjects `["jfe301", "jit401"]`.

- [ ] **Step 2: Update `User.cs` with `UnlockedSubjects`**
Add `public List<string> UnlockedSubjects { get; set; } = new();` to `src/backend/Domain/Entities/AS/User.cs`.

- [ ] **Step 3: Update `catalog.json`**
Add `"isRestricted": false` to `mln122` and `prm393`; add `"isRestricted": true` to `jfe301` and `jit401` in `src/backend/FeatureFusion/Data/QuizBanks/catalog.json`.

- [ ] **Step 4: Commit**
```bash
git add src/backend/Domain/Entities/WordsNote/UnlockKeyDocument.cs src/backend/Domain/Entities/AS/User.cs src/backend/FeatureFusion/Data/QuizBanks/catalog.json
git commit -m "feat(backend): add UnlockKeyDocument and subject restriction flags"
```

---

### Task 2: Backend Controller & Endpoints in `QuizSetsController.cs`

**Files:**
- Modify: `src/backend/Application/Dtos/WordsNote/QuizBankDtos.cs`
- Modify: `src/backend/FeatureFusion/Controllers/WordsNote/QuizSetsController.cs`

**Interfaces:**
- Produces DTOs: `UnlockKeyDTO`, `GenerateKeysRequestDTO`, `RedeemKeyRequestDTO`, `GrantUserAccessRequestDTO`, `UserAccessDTO`.
- Implements Endpoints:
  - `GET /api/quiz-sets/catalog`: Returns `isRestricted` and `unlocked` for each subject.
  - `GET /api/quiz-sets/{id}/questions`: Returns 403 `RESTRICTED_SUBJECT` if subject is restricted and user is neither Admin nor unlocked.
  - `POST /api/quiz-sets/unlock`: Single-use key redemption.
  - `GET /api/quiz-sets/admin/keys`: List all keys.
  - `POST /api/quiz-sets/admin/keys/generate`: Generate 1-50 16-character keys.
  - `DELETE /api/quiz-sets/admin/keys/{code}`: Delete/revoke key.
  - `GET /api/quiz-sets/admin/users`: List users and unlocked subjects.
  - `POST /api/quiz-sets/admin/users/grant`: Grant access directly.

- [ ] **Step 1: Add DTOs to `QuizBankDtos.cs`**
Add `IsRestricted` and `IsUnlocked` to `QuizSetDTO`. Add key generation, redemption, and admin management DTOs.

- [ ] **Step 2: Implement authorization check & endpoints in `QuizSetsController.cs`**
Inject `IMongoCollection<UnlockKeyDocument>` and `IMongoCollection<User>`.
Implement helper `IsAdminAsync()` checking user email or `X-Admin-Secret`.
Implement subject access guard on `GetQuestionsAsync`.
Implement `/unlock` key redemption with atomic update (`FindOneAndUpdateAsync` or state check).
Implement admin endpoints: `GetAdminKeysAsync`, `GenerateKeysAsync`, `DeleteKeyAsync`, `GetAdminUsersAsync`, `GrantUserAccessAsync`.

- [ ] **Step 3: Commit**
```bash
git add src/backend/Application/Dtos/WordsNote/QuizBankDtos.cs src/backend/FeatureFusion/Controllers/WordsNote/QuizSetsController.cs
git commit -m "feat(backend): implement access restriction and admin key management endpoints"
```

---

### Task 3: Backend Unit Tests & Local Verification

**Files:**
- Modify: `src/backend/FeatureFusion.Tests/QuizBankTests.cs`

- [ ] **Step 1: Add unit tests for key generation and redemption**
Write test cases in `QuizBankTests.cs`:
- Key generation produces 16-character alphanumeric code.
- Redemption marks key as used and returns success.
- Redeeming an already used key returns error.
- Redeeming non-existent key returns error.

- [ ] **Step 2: Run `dotnet test`**
Run: `dotnet test src/backend/FeatureFusion.Tests/FeatureFusion.Tests.csproj`
Verify all tests pass with exit code 0.

- [ ] **Step 3: Commit**
```bash
git add src/backend/FeatureFusion.Tests/QuizBankTests.cs
git commit -m "test(backend): add tests for key generation and unlock validation"
```

---

### Task 4: Backend Publish Notification Gate

- [ ] **Step 1: Build Release Package for Backend**
Run `dotnet publish src/backend/FeatureFusion/FeatureFusion.csproj -c Release` to verify publishable artifact is ready.
- [ ] **Step 2: Inform User for Backend Publish**
Notify user that backend code is ready and tested locally, and await their confirmation ("ok đã publish").

---

### Task 5: Frontend API, Types & Pinia Store

**Files:**
- Modify: `src/frontend/src/types/WordsNote.ts`
- Modify: `src/frontend/src/apis/WordsNote/QuizAPI.ts`
- Modify: `src/frontend/src/stores/WordsNote/QuizStore.ts`

**Interfaces:**
- Produces: `QuizAPI.unlockSubject()`, `QuizAPI.getAdminKeys()`, `QuizAPI.generateKeys()`, `QuizAPI.deleteKey()`, `QuizAPI.getAdminUsers()`, `QuizAPI.grantUserAccess()`.
- Updates `QuizStore`: Manages `activeSubjectId`, `progress` (persisted in `localStorage`), `mode` (`seq`, `wrong`, `unanswered`, `starred`, `shuffle`), `queue`, `selectedKeys`, `isChecked`, `isRevealed`.

- [ ] **Step 1: Update `types/WordsNote.ts`**
Add interfaces `UnlockKeyItem`, `AdminUserItem`, `SubjectCatalogItem`.

- [ ] **Step 2: Update `QuizAPI.ts`**
Add API methods for `/unlock`, `/admin/keys`, `/admin/keys/generate`, `/admin/keys/:code`, `/admin/users`, `/admin/users/grant`.

- [ ] **Step 3: Refactor `QuizStore.ts` to support MLN122_FE state model**
Implement per-subject progress loading/saving to `localStorage` (`fe_learn_progress_{subjectId}_v1`).
Implement queue generation based on modes (`seq`, `wrong`, `unanswered`, `starred`, `shuffle`).
Implement answer selection, immediate check for single-choice, manual check for multi-choice, and star toggle.

- [ ] **Step 4: Commit**
```bash
git add src/frontend/src/types/WordsNote.ts src/frontend/src/apis/WordsNote/QuizAPI.ts src/frontend/src/stores/WordsNote/QuizStore.ts
git commit -m "feat(frontend): add quiz unlock API and store state for MLN122 parity"
```

---

### Task 6: Frontend Workspace Components (Navbar, Controls, Map)

**Files:**
- Create: `src/frontend/src/views/Quiz/components/HeaderNavbar.vue`
- Create: `src/frontend/src/views/Quiz/components/QuizControls.vue`
- Create: `src/frontend/src/views/Quiz/components/SideQuestionMap.vue`

**Interfaces:**
- `HeaderNavbar`: Emits `select-subject`, `open-search`, `open-admin`, `toggle-theme`, `logout`.
- `QuizControls`: Props `pos`, `queueLength`, `totalQuestions`, `currentMode`, `stats`. Emits `jump`, `set-mode`, `set-source`.
- `SideQuestionMap`: Props `questions`, `currentQuestionId`, `progress`. Emits `select-question`.

- [ ] **Step 1: Create `HeaderNavbar.vue`**
Port navigation header with brand code, subject tabs with lock badges, action icons (Search, Admin, Theme, User Pill).

- [ ] **Step 2: Create `QuizControls.vue`**
Port controls toolbar with stats counters, progress bar, jump input, mode filter chips, and source filters.

- [ ] **Step 3: Create `SideQuestionMap.vue`**
Port sticky right-side question map with status indicators and star badge.

- [ ] **Step 4: Commit**
```bash
git add src/frontend/src/views/Quiz/components/HeaderNavbar.vue src/frontend/src/views/Quiz/components/QuizControls.vue src/frontend/src/views/Quiz/components/SideQuestionMap.vue
git commit -m "feat(frontend): port HeaderNavbar, QuizControls, and SideQuestionMap components"
```

---

### Task 7: Frontend Question Card & Interactive Modals

**Files:**
- Create: `src/frontend/src/views/Quiz/components/QuestionCard.vue`
- Create: `src/frontend/src/views/Quiz/components/SearchDrawer.vue`
- Create: `src/frontend/src/views/Quiz/components/UnlockModal.vue`
- Create: `src/frontend/src/views/Quiz/components/AdminModal.vue`
- Create: `src/frontend/src/views/Quiz/components/GoogleLoginModal.vue`

**Interfaces:**
- `QuestionCard`: Immediate answer check on single-choice, choose limit, navigation bar directly under options, "+ Save Word" SRS button, feedback callout, detailed explanation with wrong callout, note, and alt variants.
- `SearchDrawer`: Real-time search with input auto-focus, query by question id or text.
- `UnlockModal`: 16-character code redemption with loading and error handling.
- `AdminModal`: Key generator (16-char codes, batch 1-50), keys table with copy button, users table with grant/revoke.
- `GoogleLoginModal`: Google OAuth sign-in integration for identity verification.

- [ ] **Step 1: Create `QuestionCard.vue`**
Port question card with exact layout, option button styles, navigation buttons directly under options, explanation callouts, and WordsNote "+ Save Word" action.

- [ ] **Step 2: Create `SearchDrawer.vue`**
Port slide-in drawer for fuzzy searching questions.

- [ ] **Step 3: Create `UnlockModal.vue`**
Port single-use code redemption dialog.

- [ ] **Step 4: Create `AdminModal.vue`**
Port full admin dashboard with Tab 1 (Key Management) and Tab 2 (User Management).

- [ ] **Step 5: Create `GoogleLoginModal.vue`**
Port login modal for restricted subject access.

- [ ] **Step 6: Commit**
```bash
git add src/frontend/src/views/Quiz/components/QuestionCard.vue src/frontend/src/views/Quiz/components/SearchDrawer.vue src/frontend/src/views/Quiz/components/UnlockModal.vue src/frontend/src/views/Quiz/components/AdminModal.vue src/frontend/src/views/Quiz/components/GoogleLoginModal.vue
git commit -m "feat(frontend): port QuestionCard and quiz modals"
```

---

### Task 8: Main Workspace Page & Route Integration

**Files:**
- Create: `src/frontend/src/views/Quiz/QuizWorkspacePage.vue`
- Modify: `src/frontend/src/router/index.ts`
- Modify: `src/frontend/src/assets/styles/app-identity.css`

- [ ] **Step 1: Create `QuizWorkspacePage.vue`**
Assemble 2-column layout (Main Column + Side Question Map) connecting all components, keyboard event listeners (Left/Right arrow, A/D, 1-4, S), and state coordination.

- [ ] **Step 2: Update Vue Router in `src/frontend/src/router/index.ts`**
Route `/quiz` and `/quiz/:id` to `QuizWorkspacePage.vue`.

- [ ] **Step 3: Add CSS variables & style tokens in `app-identity.css`**
Add glass-panel, quiz option tokens, and map cell colors with dark mode support.

- [ ] **Step 4: Commit**
```bash
git add src/frontend/src/views/Quiz/QuizWorkspacePage.vue src/frontend/src/router/index.ts src/frontend/src/assets/styles/app-identity.css
git commit -m "feat(frontend): assemble QuizWorkspacePage and update routing"
```

---

### Task 9: Frontend Compilation & Theme Verification

**Files:**
- Verify: `src/frontend`

- [ ] **Step 1: Run typecheck and build**
Run: `npm run build` in `src/frontend`
Verify 0 errors, 0 warnings.

- [ ] **Step 2: Commit**
```bash
git add .
git commit -m "chore: ensure clean frontend build and styles"
```

---

### Task 10: End-to-End Verification & Merge

- [ ] **Step 1: Verify all 4 subjects in browser**
Verify MLN122, PRM393, JFE301, JIT401 loading, question map synchronization, answer evaluation, explanation display, search drawer, unlock modal, and admin key generation.
- [ ] **Step 2: Create PR and merge to `main`**
Push `feat/quiz-parity-mln122`, open PR using `gh pr create`, and merge via `gh pr merge --merge`.
