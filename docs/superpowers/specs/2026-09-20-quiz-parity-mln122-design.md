# Design Spec: Quiz Platform Parity with MLN122_FE

- **Date:** 2026-09-20
- **Status:** Approved
- **Branch:** `feat/quiz-parity-mln122`
- **Scope:** Full-stack parity of `/quiz` in WordsNote with `E:\workspace\srcPrj\MLN122_FE` (UI components, client-side interaction logic, access restriction, 16-char single-use unlock keys, and admin management panel).

---

## 1. Overview & Objectives

Bring the full interactive study and exam review experience of `MLN122_FE` into `WordsNote` under `/quiz`:
1. **Interactive Workspace Experience:**
   - Real-time question navigation, instant answer feedback, option evaluation, wrong-answer explanation callouts, additional notes, and variant questions.
   - Question Map (`SideQuestionMap`): Sticky side panel showing color-coded statuses (current, correct, incorrect, unanswered) and star indicators for all questions.
   - Toolbar (`QuizControls`): Inline stats (done, correct, incorrect, remaining, progress bar), jump to question number, mode filters (All, Wrong, Unanswered, Starred, Shuffle), and question source filters (FE Exam, Extra review, PT quiz).
   - Subject Tabs: Instant 1-click switching between `MLN122`, `PRM393`, `JFE301`, and `JIT401`.
   - Dedicated Modals:
     - `SearchDrawer`: Fast keyword and question-number search.
     - `UnlockModal`: 16-character single-use unlock code redemption.
     - `AdminModal`: Key generation (custom or random 16-char codes, batch 1-50, target subjects) and user access management.
     - `GoogleLoginModal`: Google authentication for identity verification on restricted subjects.
   - Shortcut keys: Arrow keys / A / D for Prev/Next, 1-4 / A-D for options, S for Star, Space/Enter for reveal/check.
   - SRS Integration: Retain WordsNote's "+ Save Word" action directly on questions to capture vocabulary into flashcard desks.

2. **Access Control & Single-Use Unlock Code Architecture:**
   - Open Subjects: `MLN122` and `PRM393` are free and unrestricted (no login or code required).
   - Restricted Subjects: `JFE301` and `JIT401` require Google login + a 16-character single-use unlock code managed by Admin.
   - Single-use enforcement: Each key can only be redeemed once. Upon redemption, it records the user's email, timestamp, and permanently unlocks the target subjects for that user account.
   - Admin access: Users authenticated as Admin (`AdminEmail == hnt.vn.vn@gmail.com` or header `X-Admin-Secret`) have unrestricted access to all subjects and full key/user administration.

---

## 2. Backend Design (.NET 8 Web API & MongoDB)

### 2.1. MongoDB Entities

#### `UnlockKeyDocument` (Collection: `wordsnote_unlock_keys`)
```csharp
namespace Domain.Entities.WordsNote;

public class UnlockKeyDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("code")]
    public string Code { get; set; } = null!;

    [BsonElement("isUsed")]
    public bool IsUsed { get; set; } = false;

    [BsonElement("usedAt")]
    public DateTime? UsedAt { get; set; }

    [BsonElement("usedByEmail")]
    public string? UsedByEmail { get; set; }

    [BsonElement("targetSubjects")]
    public List<string> TargetSubjects { get; set; } = ["jfe301", "jit401"];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### `User` Extension (Collection: `Users`)
Add property:
```csharp
[BsonElement("unlockedSubjects")]
public List<string> UnlockedSubjects { get; set; } = new();
```

### 2.2. API Endpoints in `QuizSetsController.cs`

1. `GET /api/quiz-sets/catalog`
   - Returns all subjects with `isRestricted` and `unlocked` status based on current user context (Anonymous, Authenticated User, or Admin).

2. `GET /api/quiz-sets/{id}/questions`
   - Authorization Check:
     - If subject is not restricted: Allow anonymous.
     - If subject is restricted: Require valid JWT token. Allow if user is Admin or if `id` is in user's `unlockedSubjects`. Otherwise, return HTTP 403 Forbidden with `{ error: "RESTRICTED_SUBJECT", message: "..." }`.

3. `POST /api/quiz-sets/unlock`
   - Requires authenticated user.
   - Body: `{ "code": "string" }`
   - Verifies key exists, is not used (`isUsed == false`).
   - Marks key `isUsed = true`, `usedAt = UtcNow`, `usedByEmail = User.Email`.
   - Appends key's `targetSubjects` to User's `unlockedSubjects`.
   - Returns `{ success: true, unlockedSubjects: [...] }`.

4. `GET /api/quiz-sets/admin/keys` (Admin Only)
   - Returns all keys sorted by `createdAt desc`.

5. `POST /api/quiz-sets/admin/keys/generate` (Admin Only)
   - Body: `{ "code": string?, "targetSubjects": string[]?, "batchCount": int }`
   - Generates 1-50 keys formatted as 16 alphanumeric characters (e.g. `WN-XXXX-XXXX-XXXX` or custom).

6. `DELETE /api/quiz-sets/admin/keys/{code}` (Admin Only)
   - Revokes / deletes specified unlock key.

7. `GET /api/quiz-sets/admin/users` (Admin Only)
   - Returns list of users with email, name, picture, and their `unlockedSubjects`.

8. `POST /api/quiz-sets/admin/users/grant` (Admin Only)
   - Body: `{ "email": string, "subjects": string[] }`
   - Directly grants unlocked subject access to a user.

---

## 3. Frontend Architecture (Vue 3, TypeScript & Pinia)

### 3.1. Route Structure
- `/quiz`: Renders the main `QuizWorkspacePage.vue` (defaulting to subject `mln122`).
- `/quiz/:id`: Directly opens `QuizWorkspacePage.vue` with target subject active.

### 3.2. Component Breakdown
- `src/frontend/src/views/Quiz/`:
  - `QuizWorkspacePage.vue`: Orchestrates workspace state, data fetching, question queue, and modals.
  - `components/HeaderNavbar.vue`: Brand title, Subject tabs with lock icons, action buttons (Search, Admin, Theme, User Pill).
  - `components/QuizControls.vue`: Progress & stats bar, Jump to question, mode filter chips, and source filters.
  - `components/QuestionCard.vue`: Question stem, choose limit indicator, interactive option buttons, nav bar with Prev/Next/Star/Check/Reveal/Save Word, feedback badge, explanation callout, and note/alt boxes.
  - `components/SideQuestionMap.vue`: Sticky question number grid (1..N) with status colors (current, correct, incorrect, unanswered) and star markers.
  - `components/SearchDrawer.vue`: Real-time question search by number, prompt, and explanation.
  - `components/UnlockModal.vue`: Code entry modal for unlocking restricted subjects.
  - `components/AdminModal.vue`: Two-tab Admin console for Key Management and User Access.
  - `components/GoogleLoginModal.vue`: Google One-Tap / GSI Sign-In dialog for restricted subjects.

### 3.3. State Management (`QuizStore.ts`)
- Pinia store managing:
  - `catalog`: List of subjects with restricted and unlock flags.
  - `activeSubjectId`: Current subject (`mln122`, `prm393`, `jfe301`, `jit401`).
  - `questions`: All questions loaded for active subject.
  - `queue`: Filtered / shuffled active question list based on selected mode (`seq`, `wrong`, `unanswered`, `starred`, `shuffle`).
  - `progress`: Per-subject question state in `localStorage` (`{ [qId]: { result: 'ok'|'bad', selected: string[], star: boolean } }`).
  - `user`: Current auth state, Google token, and unlocked subjects list.
  - `isDark`: Theme state synchronized with WordsNote identity.

---

## 4. Verification & Testing Plan

1. **Backend Unit / Integration Tests:**
   - Test key generation (single and batch).
   - Test key redemption: valid key, already used key, non-existent key.
   - Test restricted subject endpoint returns 403 for unauthorized users and 200 for authorized/admin users.
2. **Local Compilation & Build:**
   - `dotnet build src/backend/FeatureFusion.sln` -> 0 errors.
   - `dotnet test src/backend/FeatureFusion.Tests/FeatureFusion.Tests.csproj` -> All passing.
   - `npm run build` in `src/frontend` -> 0 errors.
3. **Backend Deployment Checkpoint:**
   - Wait for user confirmation of backend publish before final live testing.
4. **End-to-End Live Verification:**
   - Test switching subjects between MLN122, PRM393, JFE301, JIT401.
   - Test answer checking, feedback, explanation, star toggle, search drawer.
   - Test unlock flow with generated code.
   - Test admin panel key generation.
